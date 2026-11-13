// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;
using System.Globalization;
using CodeDocBatchRunner;
using AgentInfrastructure;
using ContinuousLearning;
using DPO;
using DPO.Models;
using LoRA;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModelServer;
using ModelServer.DesignConversion;
using System.Net.Http.Json;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

[Collection(ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed class CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void EndpointFixture_ConstructorFailureReleasesAcquiredLaneAndBootstrap()
        => EndpointTrainingFixture.AssertIsolatedConstructorFailureCleanup(failLaneCleanup: false);

    [Fact]
    public void EndpointFixture_ConstructorFailureRetainsPrimaryWhenLaneCleanupFails()
        => EndpointTrainingFixture.AssertIsolatedConstructorFailureCleanup(failLaneCleanup: true);

    [Fact]
    public async Task EndpointFixture_OrdinaryBootstrapOwnsFreshRootBoundHistoryAcrossPlatforms()
    {
        var checkoutRoot = EndpointTrainingFixture.ResolveActualRepositoryRoot();
        string ownedRoot;
        using (var fixture = new EndpointTrainingFixture(10000))
        {
            ownedRoot = Path.GetFullPath(fixture.RepositoryRoot);
            Assert.NotEqual(Path.GetFullPath(checkoutRoot), ownedRoot);
            var tempRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())) + Path.DirectorySeparatorChar;
            Assert.True(ownedRoot.StartsWith(tempRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(Path.Combine(ownedRoot, "training-artifacts", "models")));
            var historyRoot = Path.Combine(ownedRoot, "training-artifacts", "models", ".code-documentation-certification-v1");
            var marker = Path.Combine(ownedRoot, "training-artifacts", "models", ".code-documentation-certification-v1-state", "initialized.json");
            Assert.False(Directory.Exists(historyRoot)); Assert.False(File.Exists(marker));

            await using (var host = Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments, fixture.ReadEnvironment, fixture.RepositoryRoot))
            {
                Assert.NotNull(host.Services.GetRequiredService<ContinuousLearningModelServerStartup>());
            }
            using var initialized = JsonDocument.Parse(File.ReadAllBytes(marker));
            using var store = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(historyRoot, "store.json")));
            Assert.Equal(ownedRoot, initialized.RootElement.GetProperty("repository").GetString());
            var payload = store.RootElement.GetProperty("payload");
            Assert.Equal(ownedRoot, payload.GetProperty("canonicalRepositoryRoot").GetString());
            Assert.Equal(initialized.RootElement.GetProperty("storeId").GetString(), payload.GetProperty("storeId").GetString());
            var expectedId = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("code-documentation-certification-session-store-v1\n" + ownedRoot))).ToLowerInvariant();
            Assert.Equal(expectedId, payload.GetProperty("storeId").GetString());
        }
        Assert.False(Directory.Exists(ownedRoot)); // Only the acquired synthetic temp owner is released; checkout history is never imported.
    }

    [Fact]
    public async Task OneLessonEndpoint_ProtectedProcessStartupCapturesFullMaterialWithoutProviderOrRawEgress()
        => await AssertProtectedStartupHttpNativePreparation(false);

    [Fact]
    public async Task OneLessonEndpoint_ProtectedNativePreparedRequestKeepsHostBoundary503()
        => await AssertProtectedStartupHttpNativePreparation(true);

    private async Task AssertProtectedStartupHttpNativePreparation(bool threeCandidates)
    {
        foreach (var branch in threeCandidates ? new[] { "protected-process-env", "callback", "replaced-di" } : new[] { "protected-process-env", "callback" })
        {
            var callback = branch == "callback"; var replaced = branch == "replaced-di"; var denied = branch != "protected-process-env"; // Distinguish refusal before baseline from the callback disclosure refusal after baseline.
            // The public Host owns a fixed C: root even when CI loads this test from a D: checkout.
            using var fixture = new EndpointTrainingFixture(10000,2,usePublicHostRepositoryRoot: true);
            // Seal genuinely configured weights before baseline evaluation; shared initializer order must not choose this positive fixture's saved answer.
            fixture.ProvisionOrdinaryV2(5,genuineTeachingCurriculum:threeCandidates ? CodeDocumentationPreparedBaselineTests.NativePreparationCurriculum(fixture) : fixture.SealGenuineProtectedNativeCapture(),
                configureGenuineParent: ConfigureSelectedTaskParent);
            // Provisioning already owns a synthetic accepted step1; HTTP must preserve it rather than assert it never existed.
            var acceptedParentBefore = fixture.SnapshotAcceptedParentLane(expectOriginalAbsent: true);
            var json = CodeDocumentationOneLessonStartupSelectionTests.TeachingDisclosureFixtureLiteral(fixture);
            using var environment = new CodeDocumentationOneLessonStartupSelectionTests.TeachingDisclosureProcessEnvironment(json,fixture);
            var diagnosticClients = new List<CodeDocumentationPreparedBaselineTests.DiagnosticClient>(); // Observe actual generated baseline answers, never replace Generate.
            var factory = new EndpointEvaluationClientFactory(null,null,c=> {
                var client = new CodeDocumentationPreparedBaselineTests.DiagnosticClient(c,(_,_) => (false,"missing_essential_behavior"));
                diagnosticClients.Add(client); return client;
            });
            WebApplication app;
            Assert.Equal(Path.GetFullPath(Program.RepositoryOwnedTrainingTrustedRepositoryRoot),Path.GetFullPath(fixture.RepositoryRoot));
            // Full public Host prepares its existing retained/training route owners at the unchanged fixed root.
            app = callback ? Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments,Environment.GetEnvironmentVariable,fixture.RepositoryRoot)
                : Program.CreateLocalLoopbackHost(fixture.ReadyFileArguments);
            await using var owned = app; // Keep the same public Host responsible for disposing its retained startup owners.
            var retainedStartup = app.Services.GetRequiredService<ContinuousLearningModelServerStartup>(); // Resolve the actual startup used by this Host, without substituting fixture-owned authority.
            var fixtureApproval = retainedStartup.ResolveProtectedTeachingDisclosureApproval(fixture.OrdinaryGrant); // Diagnose the original oracle's foreign grant object under the unchanged membership guard.
            Assert.False(fixtureApproval.IsSuccess); Assert.Equal(callback ? CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable : CodeDocumentationOneLessonTeachingDisclosureApproval.OwnerMismatch, fixtureApproval.ErrorCode); // Equivalent fixture values must never mint retained protected approval membership.
            output.WriteLine("Protected startup fixture grant: branch={0}; refusal={1}", branch, fixtureApproval.ErrorCode); // Preserve the concrete bounded lookup refusal for exact Windows evidence.
            var retainedGrant = retainedStartup.OneLessonGrantCatalog.Resolve(fixture.OrdinarySelector); // Rebind lookup keys through the current protected catalog to its own immutable receiving grant.
            Assert.True(retainedGrant.IsSuccess, retainedGrant.ErrorCode); Assert.NotNull(retainedGrant.Value); Assert.NotSame(fixture.OrdinaryGrant, retainedGrant.Value); // Require a successful catalog lookup and actual distinct owner identity before resolving approval.
            var retainedApproval = retainedStartup.ResolveProtectedTeachingDisclosureApproval(retainedGrant.Value!); // Obtain the precise approval owned by the Host's retained grant.
            if (callback) { Assert.False(retainedApproval.IsSuccess); Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable, retainedApproval.ErrorCode); } // Callback provenance remains unavailable even after genuine catalog membership lookup.
            else { Assert.True(retainedApproval.IsSuccess, retainedApproval.ErrorCode); Assert.NotNull(retainedApproval.Value); } // Protected process startup must provide a successful current approval before any Value access.
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext? nativeContext = null;
            var captures=0; var rawReads=0; var providerLookups=0; var collectionLookups=0; var singleLookups=0; var students=0; var teachers=0;
            var sealedBefore=fixture.SnapshotSealedInputs();
            var sealedFilesBefore = new[] { fixture.DpoAddress.DatasetRoot, fixture.CorpusAddress.CorpusRoot }.SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)) // Snapshot every sealed dataset/corpus member independently of later baseline creation.
                .ToDictionary(path => path, path => (Bytes: File.ReadAllBytes(path), LastWriteTicks: File.GetLastWriteTimeUtc(path).Ticks), StringComparer.Ordinal); // Preserve actual bytes and timestamps for all three ownership stages.
            void ConfigureObservation(WebApplication host) => host.Use(async(context,next)=> {
                var services=context.RequestServices;
                var old=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
                var oldRead=CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
                var oldStudent=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting;
                var oldTeacher=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting;
                context.RequestServices=new FreshMaterialReplacingServiceProvider(services,factory,
                    ()=>{providerLookups++;throw new InvalidOperationException("V3 inspection must precede provider lookup.");},
                    ()=>{collectionLookups++;throw new InvalidOperationException("V3 inspection must precede admission lookup.");},()=>singleLookups++,
                    branch == "replaced-di" ? ContinuousLearningModelServerStartup.Create(Environment.GetEnvironmentVariable) : null);
                CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting=_=>rawReads++;
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=c=> {
                    nativeContext = c; captures++; Assert.Equal("ProtectedStartupOperatorApproval",c.Origin); Assert.False(c.FullTeacherContextReady); if (threeCandidates) Assert.Equal(3, c.Candidates.Count); else Assert.True(c.Candidates.Count > 1);
                    output.WriteLine("Protected startup native capacity: threeCandidates={0}; inputBytes={1}; packetBytes={2}; packetLimit=16384",
                        threeCandidates, c.MetadataInput.CopyBytes().Length, c.CopyCanonicalBytes().Length);
                    foreach(var item in c.Candidates) { Assert.Contains(item.OwnerInput,Encoding.UTF8.GetString(c.CopyCanonicalBytes())); Assert.Contains(item.PaidModelReferenceAnswer,Encoding.UTF8.GetString(c.CopyCanonicalBytes())); }
                };
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=_=>students++;
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting=_=>teachers++;
                try {await next(context);} finally {context.RequestServices=services;CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=old;CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting=oldRead;CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=oldStudent;CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting=oldTeacher;}
            });
            ConfigureObservation(app);
            var stopped = false;
            try
            {
                await app.StartAsync(); using var http=new HttpClient{BaseAddress=Program.ResolveStartedLoopbackAddress(app)};
                using var response=await http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,new StringContent(JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(fixture.OrdinarySelector)),Encoding.UTF8,"application/json"));
                var body=await response.Content.ReadAsStringAsync();using var parsed=JsonDocument.Parse(body);
                // Retain the refusal branch before the strict assertion without logging any response body or message.
                var diagnosticErrorCode = "<missing-public-error-code>"; // Missing protocol evidence stays distinct from a public refusal code.
                if (parsed.RootElement.ValueKind == JsonValueKind.Object && parsed.RootElement.TryGetProperty("errorCode", out var publicCode)) // Read only the existing public scalar.
                {
                    var publicErrorCode = publicCode.ValueKind == JsonValueKind.String ? publicCode.GetString() : null; // Never serialize arbitrary JSON into test output.
                    diagnosticErrorCode = publicErrorCode is { Length: > 0 and <= 160 } && // Bound the complete code rather than emitting a partial response value.
                        publicErrorCode.All(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_') // Only the public error-code alphabet may leave this diagnostic.
                        ? publicErrorCode : "<invalid-or-unbounded-public-error-code>"; // Suppress untrusted text, line breaks and oversized values.
                }
                output.WriteLine("Protected startup HTTP: branch={0}; status={1}; publicErrorCode={2}", // Existing xUnit output is retained in TRX even when the next assertion fails.
                    branch, (int)response.StatusCode, diagnosticErrorCode); // Fixed branch names, integer status and the bounded public code only.
                Assert.Equal(503,(int)response.StatusCode);
                Assert.False(parsed.RootElement.GetProperty("isSuccess").GetBoolean()); // Every stage returns a strict public failure envelope.
                Assert.Equal(replaced ? "CONTINUOUS_LEARNING_REPOSITORY_OWNED_TRAINING_CONTEXT_UNAVAILABLE" : callback ? CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable : "CODE_DOCUMENTATION_ONE_LESSON_NATIVE_HOST_READ_BOUNDARY_UNAVAILABLE", parsed.RootElement.GetProperty("errorCode").GetString()); // Assert each actual owner boundary's exact refusal rather than a universal denied oracle.
                if (replaced) // The substituted callback startup lacks the retained Host execution plan and returns before evaluator construction.
                { // Absence is the intended stage outcome, not a skipped baseline assertion.
                    Assert.False(File.Exists(fixture.OrdinaryBaseline)); Assert.False(File.Exists(fixture.OrdinaryClaim)); // This request must create neither saved ORIGINAL nor its claim.
                    Assert.False(Directory.Exists(Path.GetDirectoryName(fixture.OrdinaryBaseline)!)); Assert.False(Directory.Exists(Path.GetDirectoryName(Path.GetDirectoryName(fixture.OrdinaryBaseline)!)!)); // No baseline custody directories may appear on the early refusal.
                    Assert.Empty(diagnosticClients); Assert.Equal(0, diagnosticClients.Sum(client => client.Calls)); Assert.Null(nativeContext); // Neither evaluator creation/generation nor raw recipient capture was reached.
                } // The callback and protected stages still require the full actual saved baseline below.
                else // Both later stages own a genuine evaluator-produced ORIGINAL before their distinct disclosure outcomes.
                { // Inspect only the baseline this HTTP request actually persisted; no second evaluator operation.
                    using var savedOriginal = JsonDocument.Parse(File.ReadAllBytes(fixture.OrdinaryBaseline)); // A missing baseline on either later stage remains a hard failure.
                    var baseline = savedOriginal.RootElement.GetProperty("Baseline").Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>(); Assert.NotNull(baseline); // Require the complete saved quality record.
                    AssertProtectedCleanOriginal(fixture, baseline, diagnosticClients, retainedApproval.IsSuccess ? retainedApproval.Value!.ExposedMetadata() : null, // Compare ORIGINAL against the exact retained approval when the Host has protected provenance.
                        (field, digest, length) => output.WriteLine("Protected startup ORIGINAL witness: field={0}; sha256={1}; utf8Bytes={2}", field, digest, length)); // Preserve the actual ten-answer/full-exclusion oracle for both later stages.
                } // Finish the strict stage-specific ORIGINAL evidence before shared no-egress checks.
                Assert.Equal(denied?0:1,captures);if(denied)Assert.Equal(0,rawReads);else Assert.True(rawReads>0);
                if (denied) Assert.Null(nativeContext); // Neither unavailable branch may retain a prepared native payload.
                Assert.Equal(0,providerLookups);Assert.Equal(0,collectionLookups);Assert.Equal(0,singleLookups);Assert.Equal(0,students);Assert.Equal(0,teachers);
                foreach(var name in new[]{"ownerInput","paidModelReferenceAnswer","receipt","candidates","approval"})Assert.False(parsed.RootElement.TryGetProperty(name,out _));
                Assert.DoesNotContain("task material suffix",body,StringComparison.Ordinal);Assert.DoesNotContain("synthetic-legal-operator-fixture",body,StringComparison.Ordinal);
                var requestPath = Path.Combine(Path.GetDirectoryName(fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.RequestFileName);
                var schemaPath = Path.Combine(Path.GetDirectoryName(fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.SchemaFileName);
                if (denied) { Assert.False(File.Exists(requestPath)); Assert.False(File.Exists(schemaPath)); }
                else
                {
                    // Endpoint has released the Session; ordinary reads here inspect the actual retained whole family.
                    var requestBytes = File.ReadAllBytes(requestPath); var schemaBytes = File.ReadAllBytes(schemaPath);
                    var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(requestBytes); Assert.True(decoded.IsSuccess, decoded.ErrorCode);
                    Assert.Equal(schemaBytes, decoded.Value!.CopyOutputSchemaBytes()); Assert.NotNull(nativeContext);
                    var expectedPrompt = CodeDocumentationPreparedBaselineTests.IndependentlyExpectedNativePrompt(nativeContext!, decoded.Value.Nonce);
                    Assert.True(decoded.Value.MatchesPromptBytes(expectedPrompt));
                    Assert.True(retainedApproval.IsSuccess, retainedApproval.ErrorCode); Assert.NotNull(retainedApproval.Value); // Whole-request equality requires a real successful protected approval, never a null value or a synthetic substitute.
                    var approvalBytes = retainedApproval.Value!.CopyBytes(); // Use the exact immutable approval resolved from the current Host's receiving catalog.
                    Assert.Equal(CodeDocumentationPreparedBaselineTests.IndependentlyExpectedNativeRequest(decoded.Value.SessionId,nativeContext!,approvalBytes,decoded.Value.Nonce), requestBytes);
                    if (threeCandidates)
                    {
                        async Task AssertReplay(HttpClient replayClient)
                        {
                            using var replayResponse = await replayClient.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath, new StringContent(JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(fixture.OrdinarySelector)), Encoding.UTF8, "application/json"));
                            using var replayJson = JsonDocument.Parse(await replayResponse.Content.ReadAsStringAsync());
                            Assert.Equal(503, (int)replayResponse.StatusCode);
                            Assert.Equal("CODE_DOCUMENTATION_ONE_LESSON_NATIVE_HOST_READ_BOUNDARY_UNAVAILABLE", replayJson.RootElement.GetProperty("errorCode").GetString());
                            Assert.Equal(requestBytes, File.ReadAllBytes(requestPath)); Assert.Equal(schemaBytes, File.ReadAllBytes(schemaPath));
                            Assert.Equal(decoded.Value.Nonce, CodeDocumentationNativeCodexSelectionWire.DecodePrepared(File.ReadAllBytes(requestPath)).Value!.Nonce);
                        }
                        await AssertReplay(http);
                        await app.StopAsync(); stopped = true; await app.DisposeAsync();
                        await using var freshHost = Program.CreateLocalLoopbackHost(fixture.ReadyFileArguments); ConfigureObservation(freshHost);
                        await freshHost.StartAsync();
                        try { using var freshHttp = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(freshHost) }; await AssertReplay(freshHttp); }
                        finally { await freshHost.StopAsync(); }
                        Assert.Equal(3, captures); Assert.Equal(0, providerLookups); Assert.Equal(0, collectionLookups); Assert.Equal(0, singleLookups); Assert.Equal(0, students); Assert.Equal(0, teachers);
                    }
                }
                Assert.False(File.Exists(fixture.OrdinaryHistory)); if (!stopped) Assert.Equal(0,app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                fixture.AssertSnapshotUnchanged(sealedBefore,fixture.SnapshotSealedInputs(),"V3 full disclosure sealed inputs");fixture.AssertAcceptedParentLaneUnchanged(acceptedParentBefore, expectOriginalCreated: !replaced);environment.AssertStillConfigured(); // Allow exact ORIGINAL additions only on stages that actually evaluate the baseline.
                foreach (var file in sealedFilesBefore) { Assert.Equal(file.Value.Bytes, File.ReadAllBytes(file.Key)); Assert.Equal(file.Value.LastWriteTicks, File.GetLastWriteTimeUtc(file.Key).Ticks); } // Refusal/replay must not rewrite or retimestamp any sealed source member.
            }
            finally {if (!stopped) await app.StopAsync();}
        }
    }

    internal static void AssertProtectedCleanOriginal(EndpointTrainingFixture fixture,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality baseline,
        IReadOnlyList<CodeDocumentationPreparedBaselineTests.DiagnosticClient> clients,
        IEnumerable<string>? approvalMetadata = null, Action<string, string, int>? witness = null)
    {
        // The existing genuine parent makes every ordinary closed prompt generate the same unique printable token.
        // Verify the actual evaluator inputs and saved representative/recheck cells before claiming a clean positive.
        static string Digest(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
        var expectedDigest = Digest("~"); var answers = clients.SelectMany(client => client.StudentAnswers).ToArray();
        Assert.Equal(10, clients.Sum(client => client.Calls)); Assert.Equal(10, answers.Length);
        for (var index = 0; index < answers.Length; index++)
        {
            var bytes = Encoding.UTF8.GetBytes(answers[index]); var digest = Digest(answers[index]);
            if (digest != expectedDigest || bytes.Length != 1) witness?.Invoke("saved-answer[" + index + "]", digest, bytes.Length);
            Assert.True(answers[index] == "~", "Positive baseline saved-answer mismatch; sha256=" + digest + "; utf8Bytes=" + bytes.Length); // No raw answer enters assertion output.
        }
        var calibration = baseline.Calibration; Assert.Equal(2, calibration.RepresentativeAssessment.Cases.Count);
        Assert.All(calibration.RepresentativeAssessment.Cases, cell => Assert.Equal(expectedDigest, cell.StudentAnswerSha256));
        var repeated = calibration.Calibration.Observations.SelectMany(row => row.StudentAnswerSha256).ToArray();
        Assert.Equal(10, repeated.Length); Assert.All(repeated, digest => Assert.Equal(expectedDigest, digest));
        var source = fixture.OrdinaryGrant.SourceSelection!;
        var additional = (approvalMetadata ?? []).Append(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialPolicy).ToArray(); // Match the protected recipient's complete additional scalar set.
        var guarded = source.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(fixture.RepositoryRoot, fixture.Corpus.Corpus,
            calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, additionalExposedMetadata: additional);
        if (!guarded.IsSuccess && witness is not null)
        {
            // Diagnose only actual owner strings using the unchanged comparison, with bounded field/hash/length output.
            var comparison = CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback.OriginalComparison.Capture(
                fixture.Corpus.Corpus, calibration.RepresentativeAssessment.Cases, calibration.Calibration.Observations, CancellationToken.None);
            var graph = source.OpenDeclaredTeachingCorpusIdentities(fixture.RepositoryRoot); var emitted = 0;
            void Inspect(string field, string text)
            {
                if (comparison is not null && comparison.OverlapsText(text, CancellationToken.None) && emitted++ < 8)
                    witness(field, Digest(text), Encoding.UTF8.GetByteCount(text));
            }
            if (graph.IsSuccess)
            {
                for (var sourceIndex = 0; sourceIndex < graph.Value!.Sources.Count; sourceIndex++)
                {
                    var descriptor = graph.Value.Sources[sourceIndex].Descriptor;
                    var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(fixture.RepositoryRoot, descriptor.DomainSlug, descriptor.SubjectSlug, descriptor.CorpusVersion);
                    Assert.True(address.IsSuccess, address.ErrorCode);
                    var corpus = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address.Value!); Assert.True(corpus.IsSuccess, corpus.ErrorCode);
                    for (var entryIndex = 0; entryIndex < corpus.Value!.Corpus.Entries.Count; entryIndex++)
                    {
                        var entry = corpus.Value.Corpus.Entries[entryIndex]; var field = "source[" + sourceIndex + "].entry[" + entryIndex + "]";
                        Inspect(field + ".OwnerInput", entry.OwnerInput); Inspect(field + ".PaidModelReferenceAnswer", entry.PaidModelReferenceAnswer);
                    }
                }
                // Reuse the full production metadata enumeration, including unused SOURCE rows; do not maintain a partial copied field list.
                var metadataMethod = typeof(CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback).GetMethod("ExposedMetadata", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.NotNull(metadataMethod);
                var metadata = (IEnumerable<string>)metadataMethod.Invoke(null, [graph.Value])!; var metadataIndex = 0;
                foreach (var text in metadata) Inspect("source-metadata[" + metadataIndex++ + "]", text);
            }
            var approvalIndex = 0; foreach (var text in additional ?? []) Inspect("approval-metadata[" + approvalIndex++ + "]", text);
        }
        Assert.True(guarded.IsSuccess, "Positive fixture full ORIGINAL exclusion: " + guarded.ErrorCode); // Exact full graph/content/metadata guard remains the falsifier.
        witness?.Invoke("saved-answer.expected", expectedDigest, 1); // Fixed expected commitment only, never closed ORIGINAL or answer text.
    }
    [Fact]
    public async Task OneLessonEndpoint_FreshCompleteSyntheticSetReachesFullRecipientGuardWithoutEffects()
        => await AssertFreshAdmissionHttpRefusal("complete");

    [Fact]
    public async Task OneLessonEndpoint_FreshAdmissionSetMismatchRefusesBeforeEffects()
    {
        foreach (var scenario in new[] { "missing", "duplicate", "extra", "foreign", "stale" })
            await AssertFreshAdmissionHttpRefusal(scenario);
    }

    private async Task AssertFreshAdmissionHttpRefusal(string scenario)
    {
        // Each scenario owns a genuinely fresh ordinary Session and a separate repository fixture.
        using var h = CodeDocumentationPreparedBaselineTests.FreshFullMaterialHarness();
        var b = (await h.Consume()).Value!; var a = CodeDocumentationPreparedBaselineTests.SelectionMaterialAdmissions(h, b);
        using var foreign = scenario is "extra" or "foreign" ? CodeDocumentationPreparedBaselineTests.FreshFullMaterialHarness() : null;
        var foreignAdmission = foreign is null ? null : CodeDocumentationPreparedBaselineTests.SelectionMaterialAdmissions(foreign, (await foreign.Consume()).Value!)[0];
        var supplied = scenario switch
        {
            "missing" => a[..^1],
            "duplicate" => a.Select((item, i) => i == 0 ? a[1] : item).ToArray(),
            "extra" => a.Append(foreignAdmission!).ToArray(),
            "foreign" => a.Select((item, i) => i == 0 ? foreignAdmission! : item).ToArray(),
            "stale" => a.Select((item, i) => i == 0 ? CodeDocumentationPreparedBaselineTests.SelectedMaterialAdmission(h, b, new string('e', 64), item.Item.ItemId) : item).ToArray(),
            _ => a
        };
        // Complete set is demonstrated to match the actual frozen input before HTTP reconstructs it.
        var witness = h.Inspect(b).Value!; var diagnosis = h.Classify(b).Value!;
        var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.OriginalReceiptId, diagnosis.OriginalReceiptSha256, diagnosis.Categories, witness.TeachingIdentities).Value!;
        Assert.All(a, item => Assert.True(item.MatchesSelectionInput(witness.BindingSha256, input, item.Item).IsSuccess));
        if (scenario is "foreign" or "stale")
            Assert.False(supplied[0].MatchesSelectionInput(witness.BindingSha256, input, a[0].Item).IsSuccess,
                "The negative admission must actually differ from this frozen owner input before the ordinary recipient guard.");
        Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); Assert.Equal(10, h.Client.Calls); h.ReleaseSession();
        var fixture = h.Fixture; using var environment = fixture.ApplyEnvironment();
        var factory = new EndpointEvaluationClientFactory(null, null);
        var collections = 0; var singles = 0; var lookups = 0; var calls = 0; var raw = 0; var captures = 0; var students = 0; var teachers = 0;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((_, _) =>
        { calls++; throw new InvalidOperationException("Ordinary recipient refusal must precede dispatch."); });
        WebApplication CreateHost()
        {
            var app = Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments, fixture.ReadEnvironment, fixture.RepositoryRoot);
            app.Use(async (context, next) =>
            {
                var previous = context.RequestServices;
                context.RequestServices = new FreshMaterialReplacingServiceProvider(previous, factory,
                    () => { lookups++; return provider; }, () => { collections++; return supplied; }, () => singles++);
                try { await next(context); } finally { context.RequestServices = previous; }
            });
            return app;
        }
        await using (var warm = CreateHost()) { await warm.StartAsync(); await warm.StopAsync(); }
        var files = fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        var previousRead = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var previousCapture = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        var previousStudent = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting;
        var previousTeacher = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting;
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = point => { if (point == "raw-before-open") raw++; };
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting = _ => students++;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = _ => teachers++;
            await using var app = CreateHost(); await app.StartAsync();
            try
            {
                using var http = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(app) };
                using var response = await http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
                    new StringContent(JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(fixture.OrdinarySelector)), Encoding.UTF8, "application/json"));
                var bytes = await response.Content.ReadAsByteArrayAsync(); using var json = JsonDocument.Parse(bytes);
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); Assert.False(json.RootElement.GetProperty("isSuccess").GetBoolean());
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, json.RootElement.GetProperty("errorCode").GetString());
                foreach (var field in new[] { "ownerInput", "paidModelReferenceAnswer", "receipt", "decision", "origin" }) Assert.False(json.RootElement.TryGetProperty(field, out _));
                Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            }
            finally { await app.StopAsync(); }
        }
        finally
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = previousRead;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previousCapture;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting = previousStudent;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = previousTeacher;
        }
        Assert.Equal(1, collections); Assert.Equal(0, singles); Assert.Equal(1, lookups); Assert.Equal(0, calls);
        Assert.Equal(1, factory.CreateCalls); Assert.Equal(0, factory.Client!.EvaluationCalls); Assert.Equal(10, h.Client.Calls);
        Assert.Equal(0, raw); Assert.Equal(0, captures); Assert.Equal(0, students); Assert.Equal(0, teachers); Assert.False(File.Exists(fixture.OrdinaryHistory));
        var after = fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        Assert.Equal(files.Keys.Order(), after.Keys.Order());
        foreach (var path in files.Keys) { Assert.Equal(files[path].Bytes, after[path].Bytes); Assert.Equal(files[path].Ticks, after[path].Ticks); }
        output.WriteLine("FRESH_HTTP scenario=" + scenario + " collection=" + collections + " single=" + singles + " evaluation=" + factory.Client!.EvaluationCalls);
    }

    private sealed class FreshMaterialReplacingServiceProvider(IServiceProvider inner,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory factory,
        Func<CodeDocumentationOneLessonSelectionDecisionProvider?> provider,
        Func<IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>> collection,
        Action single, ContinuousLearningModelServerStartup? replacedStartup = null) : IServiceProvider
    {
        public object? GetService(Type type)
        {
            if (type == typeof(IServiceProvider)) return this;
            if (type == typeof(ContinuousLearningModelServerStartup) && replacedStartup is not null) return replacedStartup;
            if (type == typeof(ICodeDocumentationSavedModelAssessmentEvaluationClientFactory)) return factory;
            if (type == typeof(CodeDocumentationOneLessonSelectionDecisionProvider)) return provider();
            if (type == typeof(IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>)) return collection();
            if (type == typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission)) { single(); return null; }
            return inner.GetService(type);
        }
    }

    [Fact]
    public async Task OneLessonEndpoint_MatchingSyntheticSavedSelectionRefusesBeforeRecipientWithoutEffects()
    {
        using var harness = CodeDocumentationPreparedBaselineTests.SelectedMaterialHarness();
        var fixture = harness.Fixture;
        var grant = fixture.OrdinaryGrant;
        var graph = grant.SourceSelection!.OpenDeclaredTeachingCorpusIdentities(fixture.RepositoryRoot).Value!;
        var selected = graph.Items.Single(i => i.DeclaredItem.ItemId == "item-teach-7-1"); Assert.NotEqual(graph.Items[0], selected);
        var binding = new CodeDocumentationOneLessonSessionBinding(grant.ObjectiveId, grant.GrantId, grant.GrantVersion, grant.TrainingRunId,
            grant.CandidateSha256, grant.ParentManifestSha256, grant.ParentWeightsSha256, grant.ConfigurationSha256, grant.ExamConditionSha256)
            { FormatVersion = 2, DescriptorsV2 = grant.DescriptorsV2 };
        var bindingSha = CodeDocumentationOneLessonSession.ComputeBindingSha256(binding);
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission? admission = null;
        CodeDocumentationTeachingSelectionInputV1? savedInput = null;
        var operatorCalls = 0; var operatorLookups = 0; var admissionLookups = 0;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input, _) =>
        {
            operatorCalls++; savedInput = input;
            var issued = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(graph, input,
                bindingSha, selected.DeclaredItem.ItemId, CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference,
                new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + new string('b', 64) }));
            Assert.True(issued.IsSuccess, issued.ErrorCode); admission = issued.Value!;
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(new(
                selected.DeclaredItem.ItemId, selected.DeclaredItem.SourceId, selected.Entry.EntryId, selected.Entry.SourceEntityFingerprint,
                "Synthetic explicit non-first selection for the ordinary-recipient refusal control.", admission.EvidenceSha256)));
        });
        using var environment = fixture.ApplyEnvironment();
        var factory = new EndpointEvaluationClientFactory(null, null,
            configuration => new CodeDocumentationPreparedBaselineTests.DiagnosticClient(configuration, (_, _) => (false, "missing_essential_behavior")));
        var materialCaptures = 0; var studentCaptures = 0; var teacherCaptures = 0;
        WebApplication CreateHost()
        {
            var host = Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments, fixture.ReadEnvironment, fixture.RepositoryRoot);
            host.Use(async (context, next) =>
            {
                var previous = context.RequestServices;
                var materialObserver = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting;
                var studentObserver = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting;
                var teacherObserver = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting;
                context.RequestServices = new SelectedMaterialReplacingServiceProvider(previous, factory,
                    () => { operatorLookups++; throw new InvalidOperationException("Saved selection must not resolve another operator."); },
                    () => { admissionLookups++; return admission; });
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = _ => materialCaptures++;
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting = _ => studentCaptures++;
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = _ => teacherCaptures++;
                try { await next(context); }
                finally
                {
                    context.RequestServices = previous;
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = materialObserver;
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting = studentObserver;
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = teacherObserver;
                }
            });
            return host;
        }

        var baseline = await harness.Consume(); Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var witness = harness.Inspect(baseline.Value!); Assert.True(witness.IsSuccess, witness.ErrorMessage);
        var seeded = await harness.Open.Session.BindOrdinarySelectionDecisionSyntheticForTestingAsync(baseline.Value!, harness.Catalog,
            fixture.OrdinarySelector, witness.Value!, provider);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage); Assert.Equal(1, operatorCalls); Assert.Equal(10, harness.Client.Calls);
        CodeDocumentationPreparedBaselineTests.AssertSyntheticSelectionHistory(harness.HistoryBytes(), selected.DeclaredItem.ItemId); Assert.Equal(selected.DeclaredItem.ItemId, seeded.Value!.Payload.ItemId);
        Assert.Equal(witness.Value!.OriginalReceiptId, seeded.Value.Payload.OriginalReceiptId);
        Assert.Equal(0, factory.CreateCalls); Assert.Equal(0, operatorLookups); Assert.Equal(0, admissionLookups);
        harness.ReleaseSession();
        // Bootstrap the actual adapter owner/readiness files without issuing an HTTP request, then release all host custody.
        await using (var app = CreateHost()) { await app.StartAsync(); await app.StopAsync(); }
        Assert.NotNull(admission); Assert.NotNull(savedInput);
        Assert.True(admission.MatchesSelectionInput(bindingSha, savedInput, selected.DeclaredItem).IsSuccess, "The supplied synthetic admission matches the actual completed baseline and saved metadata choice.");
        // Disposing the real host releases its exclusive adapter owner lock before the complete byte snapshot.
        var files = fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        var history = File.ReadAllBytes(fixture.OrdinaryHistory); Assert.Equal(2, history.Count(value => value == (byte)'\n'));
        Assert.Contains(fixture.OrdinaryBaseline, files.Keys); Assert.Contains(fixture.OrdinaryClaim, files.Keys);
        Assert.Contains(Path.Combine(fixture.AdapterRoot, ".continuous-learning.adapter-owner.lock"), files.Keys);
        for (var replay = 0; replay < 2; replay++)
        {
            // Each real HTTP replay reopens the same saved selection after the previous host released custody.
            await using (var app = CreateHost())
            {
                await app.StartAsync();
                try
                {
                    using var http = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(app) };
                    using var result = await Request(http);
                    var bytes = await result.Content.ReadAsByteArrayAsync(); using var json = JsonDocument.Parse(bytes);
                    Assert.Equal(409, (int)result.StatusCode); Assert.False(json.RootElement.GetProperty("isSuccess").GetBoolean());
                    Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, json.RootElement.GetProperty("errorCode").GetString());
                    foreach (var field in new[] { "ownerInput", "paidModelReferenceAnswer", "materialAdmissionEvidenceSha256", "receipt", "decision" }) Assert.False(json.RootElement.TryGetProperty(field, out _));
                    Assert.DoesNotContain("original-exclusion", Encoding.UTF8.GetString(bytes), StringComparison.Ordinal);
                    Assert.Equal(0, materialCaptures); Assert.Equal(0, studentCaptures); Assert.Equal(0, teacherCaptures);
                    Assert.Equal(1, operatorCalls); Assert.Equal(0, operatorLookups); Assert.Equal(replay + 1, admissionLookups);
                    Assert.Equal(replay + 1, factory.CreateCalls); Assert.Equal(0, factory.Client!.EvaluationCalls); Assert.Equal(10, harness.Client.Calls);
                    Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                }
                finally { await app.StopAsync(); }
            }
            Assert.Equal(history, File.ReadAllBytes(fixture.OrdinaryHistory));
            var after = fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
            Assert.Equal(files.Keys.Order(), after.Keys.Order());
            foreach (var path in files.Keys) { Assert.Equal(files[path].Bytes, after[path].Bytes); Assert.Equal(files[path].Ticks, after[path].Ticks); }
        }
        Task<HttpResponseMessage> Request(HttpClient http) => http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
            new StringContent(JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(fixture.OrdinarySelector)), Encoding.UTF8, "application/json"));
    }

    [Fact]
    public async Task OriginalTeachingExclusion_GenuineHeldSessionWholeGraphIsImmutablePrivateAndHasZeroPostOriginalEffects()
    {
        using var h = OriginalExclusionHarness();
        var consumed = await h.Consume(); Assert.True(consumed.IsSuccess, consumed.ErrorCode + ":" + consumed.ErrorMessage);
        var b = consumed.Value!; Assert.Equal(10, h.Client.Calls);
        var owner = h.Grant.SourceSelection!.OpenDeclaredTeachingCorpusIdentities(h.Fixture.RepositoryRoot);
        Assert.True(owner.IsSuccess, owner.ErrorCode); var files = h.Files(); var effects = h.Open.Session.Snapshot.Effects;
        Assert.Contains(h.Fixture.BaseCheckpoint, files.Keys); Assert.Contains(h.Fixture.TokenizerPath, files.Keys);
        Assert.Contains(h.Fixture.OrdinaryBaseline, files.Keys); Assert.Contains(h.Fixture.OrdinaryClaim, files.Keys);
        Assert.Contains(Path.Combine(Path.GetDirectoryName(h.Fixture.OrdinaryHistory)!, ".owner.lock"), files.Keys);
        foreach (var source in h.Grant.SourceSelection!.TeachingCurriculum!.Sources)
        {
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,
                source.DomainSlug, source.SubjectSlug, source.CorpusVersion);
            Assert.True(address.IsSuccess, address.ErrorCode);
            Assert.Contains(address.Value!.ManifestPath, files.Keys);
            Assert.Contains(Path.Combine(address.Value.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName), files.Keys);
        }
        var baselineBytes = JsonSerializer.SerializeToUtf8Bytes(b.Readback.Baseline);
        var first = h.Inspect(b); var second = h.Inspect(b);
        Assert.True(first.IsSuccess, first.ErrorCode); Assert.True(second.IsSuccess, second.ErrorCode);
        var witness = first.Value!; var graph = witness.TeachingIdentities;
        Assert.True(witness.IsIssuedBy(h.Open.Session));
        Assert.Equal(b.ExternalReference.ReceiptId, witness.OriginalReceiptId);
        Assert.Equal(b.ExternalReference.ReceiptSha256, witness.OriginalReceiptSha256);
        Assert.Equal(witness.BindingSha256, second.Value!.BindingSha256);
        Assert.Equal(h.Grant.SourceSelection.SourceSelectionSha256, graph.Selection.SourceSelectionSha256);
        Assert.Equal(new[] { "teach-7", "teach-2" }, graph.Sources.Select(s => s.Descriptor.SourceId));
        Assert.All(graph.Sources, s => Assert.Equal(4, s.Entries.Count));
        Assert.Equal(h.Grant.SourceSelection.TeachingCurriculum!.Items.Select(i => i.ItemId), graph.Items.Select(i => i.DeclaredItem.ItemId));
        foreach (var category in graph.CategoryBindings)
            Assert.Equal(category.OrderedItemIds, category.Items.Select(i => i.DeclaredItem.ItemId));
        Assert.Throws<NotSupportedException>(() => ((IList<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingSourceReadback>)graph.Sources)[0] = graph.Sources[1]);
        var exposed = CodeDocumentationOneLessonSourceSelectionTests.ExposedGraphStrings(graph).ToArray();
        foreach (var closed in b.Context.Corpus.Entries.SelectMany(e => new[] { e.EntryId, e.SourceEntityFingerprint, e.OwnerInput, e.PaidModelReferenceAnswer })
            .Concat(b.Readback.Baseline.Calibration.Calibration.Observations.SelectMany(o => o.StudentAnswerSha256)))
            Assert.DoesNotContain(exposed, text => text.Contains(closed, StringComparison.Ordinal));
        var properties = witness.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal(new[] { "BindingSha256", "OriginalReceiptId", "OriginalReceiptSha256", "TeachingIdentities" }, properties.Select(p => p.Name).Order());
        Assert.All(witness.GetType().GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), c => Assert.True(c.IsPrivate));
        Assert.DoesNotContain(witness.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), m => m.Name == "Create");
        Assert.Equal(baselineBytes, JsonSerializer.SerializeToUtf8Bytes(b.Readback.Baseline));
        Assert.Equal(10, h.Client.Calls); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); h.AssertFiles(files);
        LogOriginalExclusionRuntime();
    }

    [Theory]
    [InlineData("owner")]
    [InlineData("paid")]
    [InlineData("inclusion")]
    [InlineData("fingerprint")]
    [InlineData("reference")]
    [InlineData("source-label")]
    [InlineData("source-label-bare")]
    [InlineData("item-label")]
    [InlineData("category-label")]
    [InlineData("manifest")]
    [InlineData("provenance")]
    public async Task OriginalTeachingExclusion_HonestlySealedSemanticOrExposedMetadataAliasPassesOwnerThenNewGuardRejects(string fault)
    {
        using var h = OriginalExclusionHarness(fault);
        var original = await h.Consume(); Assert.True(original.IsSuccess, original.ErrorCode + ":" + original.ErrorMessage);
        Assert.True(h.Classify(original.Value!).IsSuccess);
        var owner = h.Grant.SourceSelection!.OpenDeclaredTeachingCorpusIdentities(h.Fixture.RepositoryRoot);
        Assert.True(owner.IsSuccess, owner.ErrorCode + ":" + owner.ErrorMessage); // This proves the new guard, not an outer stale hash, rejects this genuine fixture.
        var files = h.Files(); var effects = h.Open.Session.Snapshot.Effects;
        var baseline = JsonSerializer.SerializeToUtf8Bytes(original.Value!.Readback.Baseline);
        var result = h.Inspect(original.Value);
        Assert.False(result.IsSuccess); Assert.Null(result.Value);
        Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", result.ErrorCode);
        Assert.Equal("Declared teaching material overlaps the ORIGINAL.", result.ErrorMessage);
        Assert.Equal(10, h.Client.Calls); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); h.AssertFiles(files);
        Assert.Equal(baseline, JsonSerializer.SerializeToUtf8Bytes(original.Value.Readback.Baseline));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OriginalTeachingExclusion_GenuineSavedRepeatedAnswerBytesAndDigestMetadataHaveZeroNewCalls(bool digestMetadata)
    {
        // Fix the real pre-seal model, not the captured answer: byte-token argmax from random weights can be a forbidden control.
        using var control = new CodeDocumentationPreparedBaselineTests.OrdinaryDiagnosticHarness((_, _) => (false, "missing_essential_behavior"),
            f => f.SealGenuineTeaching(), configureGenuineParent: ConfigureSavedAnswerParent);
        var original = await control.Consume(); Assert.True(original.IsSuccess, original.ErrorCode);
        Assert.Equal(10, control.Client.Calls); Assert.Equal(10, control.Client.StudentAnswers.Count);
        var captured = control.Client.StudentAnswers[0]; Assert.Equal("~", captured);
        Assert.All(control.Client.StudentAnswers, answer => Assert.Equal(captured, answer));
        var capturedBytes = new UTF8Encoding(false, true).GetBytes(captured); Assert.Equal(new byte[] { (byte)'~' }, capturedBytes);
        Assert.DoesNotContain(captured, character => char.IsControl(character) && character is not ('\n' or '\t'));
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(captured))).ToLowerInvariant();
        var originalDigests = original.Value!.Readback.Baseline.Calibration.Calibration.Observations.SelectMany(o => o.StudentAnswerSha256).ToArray();
        Assert.Equal(10, originalDigests.Length); Assert.All(originalDigests, d => Assert.Equal(digest, d));
        var cleanFiles = control.Files(); var cleanEffects = control.Open.Session.Snapshot.Effects;
        var clean = control.Inspect(original.Value); Assert.True(clean.IsSuccess, "clean whole teaching owner/exclusion: " + clean.ErrorCode + ":" + clean.ErrorMessage);
        Assert.Equal(10, control.Client.Calls); Assert.Equal(cleanEffects, control.Open.Session.Snapshot.Effects); control.AssertFiles(cleanFiles);
        var load = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(control.Start.RepositoryRoot, control.Start.DomainSlug,
            control.Start.DesignPatternSlug, control.Start.ModelIdSlug, control.Start.ModelVersion, control.Start.RuntimeCompatibility);
        Assert.True(load.IsSuccess, load.ErrorCode); using var parent = load.Value!;
        using var h = new CodeDocumentationPreparedBaselineTests.OrdinaryDiagnosticHarness((_, _) => (false, "missing_essential_behavior"),
            f => f.SealGenuineTeaching(
                digestMetadata ? "digest-metadata" : "saved-answer", savedAnswer: captured), parent.Weights);
        Assert.NotEqual(control.Grant.SourceSelection!.TeachingCurriculum!.Sources[0].CorpusVersion,
            h.Grant.SourceSelection!.TeachingCurriculum!.Sources[0].CorpusVersion);
        Assert.Equal(control.Grant.ParentWeightsSha256, h.Grant.ParentWeightsSha256); // Reseal the actual accepted model through the genuine fixture owner; no fabricated baseline or report cells.
        var baseline = await h.Consume(); Assert.True(baseline.IsSuccess, baseline.ErrorCode);
        Assert.Equal(10, h.Client.Calls); Assert.Equal(10, h.Client.StudentAnswers.Count);
        Assert.All(h.Client.StudentAnswers, answer => Assert.Equal(capturedBytes, new UTF8Encoding(false, true).GetBytes(answer)));
        var replayedDigests = baseline.Value!.Readback.Baseline.Calibration.Calibration.Observations.SelectMany(o => o.StudentAnswerSha256).ToArray();
        Assert.Equal(10, replayedDigests.Length); Assert.All(replayedDigests, d => Assert.Equal(digest, d));
        var wholeOwner = h.Grant.SourceSelection!.OpenDeclaredTeachingCorpusIdentities(h.Fixture.RepositoryRoot);
        Assert.True(wholeOwner.IsSuccess, "fault must pass the genuine full teaching owner: " + wholeOwner.ErrorCode + ":" + wholeOwner.ErrorMessage);
        var source = h.Grant.SourceSelection.TeachingCurriculum!.Sources[0];
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,
            source.DomainSlug, source.SubjectSlug, source.CorpusVersion); Assert.True(address.IsSuccess, address.ErrorCode);
        var sealedCorpus = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address.Value!);
        Assert.True(sealedCorpus.IsSuccess, sealedCorpus.ErrorCode);
        var savedEntry = Assert.Single(sealedCorpus.Value!.Corpus.Entries.Where(entry => digestMetadata
            ? entry.PaidReferenceProvenance.RequestId == digest : entry.OwnerInput == captured));
        if (digestMetadata) Assert.Equal(digest, savedEntry.PaidReferenceProvenance.RequestId);
        else Assert.Equal(capturedBytes, new UTF8Encoding(false, true).GetBytes(savedEntry.OwnerInput)); // The owner receives the unchanged full generated UTF8, including its exact hash.
        var files = h.Files(); var effects = h.Open.Session.Snapshot.Effects;
        var refused = h.Inspect(baseline.Value); Assert.False(refused.IsSuccess); Assert.Null(refused.Value);
        Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", refused.ErrorCode);
        Assert.Equal("Declared teaching material overlaps the ORIGINAL.", refused.ErrorMessage);
        Assert.Equal(10, h.Client.Calls); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); h.AssertFiles(files);
    }

    internal static void ConfigureSelectedTaskParent(LlmModel model)
    {
        // Genuine sealed weights map the last task token X/Y to different greedy outputs; every ordinary closed prompt ends elsewhere.
        foreach (var entry in model.GetCheckpointParameterEntries()) Array.Clear(entry.Parameter.Value.Data);
        var embedding = model.BaseParameters[0].Value; var gamma = model.BaseParameters[^2].Value;
        var head = model.BaseParameters[^1].Value; var width = model.Config.HiddenDim;
        Assert.True(width >= 3); Assert.Equal(256, model.Config.VocabSize);
        Array.Fill(gamma.Data, 1f);
        for (var token = 0; token < model.Config.VocabSize; token++) embedding.Data[token * width + 2] = 1f;
        embedding.Data['X' * width + 2] = 0; embedding.Data['X' * width] = 1;
        embedding.Data['Y' * width + 2] = 0; embedding.Data['Y' * width + 1] = 1;
        head.Data['a' * width] = 1; head.Data['b' * width + 1] = 1; head.Data['~' * width + 2] = 1;
        // Zero attention/FFN/adapters preserve the residual; the existing actual generator and tokenizer choose a unique printable token.
    }

    private static void ConfigureSavedAnswerParent(LlmModel model)
    {
        // Null callbacks retain every other fixture's initialization; this one fixes all base/adapter values before the existing sealer.
        var entries = model.GetCheckpointParameterEntries();
        foreach (var entry in entries) Array.Clear(entry.Parameter.Value.Data);
        // LlmModel freezes the existing registration order: token embedding first, final RMS gamma and language head last.
        var embedding = model.BaseParameters[0].Value; var finalGamma = model.BaseParameters[^2].Value;
        Assert.Equal(new[] { model.Config.VocabSize, model.Config.HiddenDim }, embedding.Shape.Dimensions);
        Assert.Equal(new[] { model.Config.HiddenDim }, finalGamma.Shape.Dimensions);
        Array.Fill(embedding.Data, 1f); Array.Fill(finalGamma.Data, 1f);
        var head = model.BaseParameters[^1].Value;
        Assert.Equal(new[] { model.Config.VocabSize, model.Config.HiddenDim }, head.Shape.Dimensions);
        const int savedAnswerToken = '~'; // The actual 256-byte tokenizer maps this printable, non-EOS token to one unchanged UTF8 byte.
        Assert.True(savedAnswerToken < model.Config.VocabSize);
        Array.Fill(head.Data, 1f, savedAnswerToken * model.Config.HiddenDim, model.Config.HiddenDim);
        // Zero attention/FFN and LoRA preserve the all-one residual; final RMS and this sole positive head row make greedy argmax unique.
    }

    [Theory]
    [InlineData("pre-cancel")]
    [InlineData("disposed")]
    [InlineData("foreign-catalog")]
    [InlineData("foreign-baseline")]
    [InlineData("nested-case")]
    [InlineData("nested-observation")]
    [InlineData("nested-context")]
    public async Task OriginalTeachingExclusion_ReceivingGuardsRejectWithoutOpeningMissingTeaching(string fault)
    {
        using var h = OriginalExclusionHarness(); var consumed = await h.Consume(); Assert.True(consumed.IsSuccess, consumed.ErrorCode);
        var b = consumed.Value!;
        // A missing genuine payload would produce an owner error if any forbidden teaching open occurred.
        var descriptor = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[0];
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, descriptor.DomainSlug, descriptor.SubjectSlug, descriptor.CorpusVersion).Value!;
        var path = Path.Combine(address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName); var bytes = File.ReadAllBytes(path);
        CodeDocumentationPreparedBaselineTests.OrdinaryDiagnosticHarness? other = null;
        object? changed = null; string? field = null; object? beforeValue = null;
        try
        {
            File.Delete(path);
            if (fault == "disposed") h.Open.Dispose();
            if (fault == "foreign-baseline")
            {
                other = OriginalExclusionHarness();
                Assert.NotEqual(h.Grant.SourceSelection!.TeachingCurriculum!.Sources[0].CorpusVersion,
                    other.Grant.SourceSelection!.TeachingCurriculum!.Sources[0].CorpusVersion);
                var o = await other.Consume(); Assert.True(o.IsSuccess, o.ErrorCode); b = o.Value!;
            }
            if (fault.StartsWith("nested-", StringComparison.Ordinal))
            {
                var change = fault switch {
                    "nested-case" => ((object)b.Readback.Baseline.Calibration.RepresentativeAssessment.Cases[1], "Reason", (object)"unknown"),
                    "nested-observation" => ((object)b.Readback.Baseline.Calibration.Calibration.Observations[^1], "PassedCaseCount", (object)1),
                    _ => ((object)b.Context, "EvidenceRefs", (object)new[] { "mutated-evidence" }) };
                changed = change.Item1; field = change.Item2; var replacement = change.Item3;
                var backing = changed.GetType().GetField("<" + field + ">k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
                beforeValue = backing.GetValue(changed); backing.SetValue(changed, replacement);
            }
            var files = h.Files(); var effects = h.Open.Session.Snapshot.Effects;
            if (fault == "pre-cancel") Assert.Throws<OperationCanceledException>(() => h.Inspect(b, new CancellationToken(true)));
            else
            {
                var result = fault == "foreign-catalog" ? h.Open.Session.InspectOrdinaryOriginalTeachingExclusion(b,
                    ContinuousLearningModelServerStartup.Create(h.Fixture.ReadEnvironment).OneLessonGrantCatalog, h.Fixture.OrdinarySelector) : h.Inspect(b);
                Assert.Equal(fault == "disposed" ? CodeDocumentationOneLessonSession.CapacityInvalidErrorCode : CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, result.ErrorCode); Assert.Null(result.Value);
            }
            Assert.Equal(10, h.Client.Calls); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); h.AssertFiles(files);
        }
        finally
        {
            if (changed is not null) changed.GetType().GetField("<" + field + ">k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(changed, beforeValue);
            File.WriteAllBytes(path, bytes); other?.Dispose();
        }
    }

    [Theory]
    [InlineData("absent")]
    [InlineData("sufficient")]
    [InlineData("diagnosis")]
    public async Task OriginalTeachingExclusion_AbsentSufficientAndMissingDiagnosisNeverEnterTeaching(string branch)
    {
        using var h = new CodeDocumentationPreparedBaselineTests.OrdinaryDiagnosticHarness((n, _) => branch == "sufficient" || branch == "diagnosis" && n <= 2
            ? (true, "correct") : (false, "missing_essential_behavior"), branch == "absent" ? null : f => CodeDocumentationOneLessonSourceSelectionTests.Curriculum());
        var original = await h.Consume(); Assert.True(original.IsSuccess, original.ErrorCode);
        var files = h.Files(); var effects = h.Open.Session.Snapshot.Effects;
        var result = h.Inspect(original.Value!); Assert.False(result.IsSuccess); Assert.Null(result.Value);
        Assert.Equal(branch switch { "absent" => "ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE", "diagnosis" => CodeDocumentationOneLessonSession.UsableDiagnosisMissingErrorCode,
            _ => h.Classify(original.Value!).ErrorCode }, result.ErrorCode);
        Assert.Equal(10, h.Client.Calls); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); h.AssertFiles(files);
    }

    private static CodeDocumentationPreparedBaselineTests.OrdinaryDiagnosticHarness OriginalExclusionHarness(string fault = "clean", string? savedAnswer = null)
        => new((_, _) => (false, "missing_essential_behavior"), f => f.SealGenuineTeaching(fault, savedAnswer: savedAnswer));

    private void LogOriginalExclusionRuntime()
    {
        var test = GetType().Assembly;
        foreach (var path in new[] { typeof(CodeDocumentationOneLessonSourceSelection).Assembly.Location, typeof(CodeDocumentationOneLessonSession).Assembly.Location,
            test.Location, Path.ChangeExtension(test.Location, ".runtimeconfig.json"), Path.ChangeExtension(test.Location, ".deps.json") })
            output.WriteLine("ORIGINAL_EXCLUSION_RUNTIME " + JsonSerializer.Serialize(new { path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant() }));
    }

    [Theory]
    [InlineData("clean", 205)]
    [InlineData("owner", 4)]
    [InlineData("paid", 4)]
    [InlineData("reference", 4)]
    [InlineData("source-label", 4)]
    [InlineData("unused-last", 205)]
    [InlineData("last-source", 4)]
    public async Task OriginalTeachingExclusion_OrdinaryProgramHttpAndClientInspectWholeGenuineMaterialBeforeUnsupported(string fault, int count)
    {
        var fixture = new EndpointTrainingFixture(10000, 2);
        fixture.ProvisionOrdinaryV2(5, genuineTeachingCurriculum: fixture.SealGenuineTeaching(fault, count));
        await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            var owner = f.OrdinaryGrant.SourceSelection!.OpenDeclaredTeachingCorpusIdentities(f.RepositoryRoot);
            Assert.True(owner.IsSuccess, owner.ErrorCode); // Honest pins/joins must pass before the intended semantic rejection.
            Assert.Equal(2, owner.Value!.Sources.Count);
            Assert.All(owner.Value.Sources, source => Assert.Equal(count, source.Entries.Count));
            var expectedCode = fault == "clean" ? "CODE_DOCUMENTATION_ONE_LESSON_SELECTION_OPERATOR_UNAVAILABLE" : "ONE_LESSON_TEACHING_ORIGINAL_OVERLAP";
            var expectedStatus = fault == "clean" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Conflict;
            var freshFiles = CaptureOriginalExclusionFiles(f);
            await AssertOriginalExclusionHttpFailure(app, f, expectedStatus, expectedCode);
            Assert.Equal(10, factory.Client!.EvaluationCalls); Assert.Equal(1, factory.Client.DisposeCalls);
            AssertOriginalExclusionNoDownstream(app, f); AssertOriginalExclusionFreshFiles(f, freshFiles);
            var files = CaptureOriginalExclusionFiles(f); // Initial ORIGINAL creation is complete; subsequent requests must preserve every owned artifact.
            var client = await new CodeDocumentationOneLessonPreflightClient().PreflightAsync(
                Program.ResolveStartedLoopbackAddress(app), f.OrdinarySelector, TimeSpan.FromMinutes(2));
            Assert.False(client.IsSuccess); Assert.Null(client.Value); Assert.Equal(expectedCode, client.ErrorCode);
            Assert.Equal(0, factory.Client!.EvaluationCalls); Assert.Equal(1, factory.Client.DisposeCalls);
            AssertOriginalExclusionNoDownstream(app, f); AssertOriginalExclusionFiles(f, files);
            LogOriginalExclusionRuntime();
            output.WriteLine("ORDINARY_ORIGINAL_EXCLUSION_RUNTIME " + JsonSerializer.Serialize(new {
                path = typeof(Program).Assembly.Location,
                sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(Program).Assembly.Location))).ToLowerInvariant() }));
            return true;
        }, false, c => new OrdinaryEvaluationClient(c, false));
    }

    [Fact]
    public async Task OriginalTeachingExclusion_OrdinaryAbsentDeclarationHasExact503AndNoTeachingOrPostOriginalEffects()
    {
        var fixture = new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture, null, () => throw new InvalidOperationException("synthetic disposal failure"), async (f, factory, app) =>
        {
            var freshFiles = CaptureOriginalExclusionFiles(f);
            await AssertOriginalExclusionHttpFailure(app, f, HttpStatusCode.ServiceUnavailable, "ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE");
            Assert.Equal(10, factory.Client!.EvaluationCalls); Assert.Equal(1, factory.Client.DisposeCalls);
            f.AssertTeachingDirectoryAbsent(); AssertOriginalExclusionNoDownstream(app, f); AssertOriginalExclusionFreshFiles(f, freshFiles);
            var files = CaptureOriginalExclusionFiles(f);
            var refused = await new CodeDocumentationOneLessonPreflightClient().PreflightAsync(
                Program.ResolveStartedLoopbackAddress(app), f.OrdinarySelector, TimeSpan.FromMinutes(2));
            Assert.Equal("ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE", refused.ErrorCode); Assert.False(refused.IsSuccess);
            Assert.Equal(0, factory.Client!.EvaluationCalls); Assert.Equal(1, factory.Client.DisposeCalls);
            f.AssertTeachingDirectoryAbsent(); AssertOriginalExclusionNoDownstream(app, f); AssertOriginalExclusionFiles(f, files);
            return true;
        }, false, c => new OrdinaryEvaluationClient(c, false));
    }

    [Fact]
    public async Task OriginalTeachingExclusion_OrdinaryFreshHostReusesOriginalAndRejectsChangedTeachingWithoutRemeasurement()
    {
        using var fixture = new EndpointTrainingFixture(10000, 2);
        fixture.ProvisionOrdinaryV2(5, genuineTeachingCurriculum: fixture.SealGenuineTeaching());
        await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            var freshFiles = CaptureOriginalExclusionFiles(f);
            await AssertOriginalExclusionHttpFailure(app, f, HttpStatusCode.ServiceUnavailable, "CODE_DOCUMENTATION_ONE_LESSON_SELECTION_OPERATOR_UNAVAILABLE");
            Assert.Equal(10, factory.Client!.EvaluationCalls); AssertOriginalExclusionNoDownstream(app, f);
            AssertOriginalExclusionFreshFiles(f, freshFiles); return true;
        }, false, c => new OrdinaryEvaluationClient(c, false), disposeFixture: false);
        var baseline = File.ReadAllBytes(fixture.OrdinaryBaseline); var baselineTicks = File.GetLastWriteTimeUtc(fixture.OrdinaryBaseline).Ticks;
        var descriptor = fixture.OrdinaryGrant.SourceSelection!.TeachingCurriculum!.Sources[^1];
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(fixture.RepositoryRoot, descriptor.DomainSlug, descriptor.SubjectSlug, descriptor.CorpusVersion).Value!;
        var payload = Path.Combine(address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        var bytes = File.ReadAllBytes(payload); var ticks = File.GetLastWriteTimeUtc(payload);
        try
        {
            File.WriteAllBytes(payload, bytes.Concat(new byte[] { (byte)' ' }).ToArray()); // One genuine sealed payload changes while all bound startup pins stay frozen.
            var control = fixture.OrdinaryGrant.SourceSelection.OpenDeclaredTeachingCorpusIdentities(fixture.RepositoryRoot);
            Assert.False(control.IsSuccess); Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, control.ErrorCode);
            await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
            {
                var files = CaptureOriginalExclusionFiles(f);
                await AssertOriginalExclusionHttpFailure(app, f, HttpStatusCode.Conflict, RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode);
                Assert.Equal(0, factory.Client!.EvaluationCalls); Assert.Equal(1, factory.Client.DisposeCalls);
                AssertOriginalExclusionNoDownstream(app, f); AssertOriginalExclusionFiles(f, files);
                Assert.Equal(baseline, File.ReadAllBytes(f.OrdinaryBaseline)); Assert.Equal(baselineTicks, File.GetLastWriteTimeUtc(f.OrdinaryBaseline).Ticks);
                return true;
            }, false, c => new OrdinaryEvaluationClient(c, false), disposeFixture: false);
        }
        finally { File.WriteAllBytes(payload, bytes); File.SetLastWriteTimeUtc(payload, ticks); }
    }

    [Fact]
    public async Task OriginalTeachingExclusion_OrdinarySufficientAndFreshHostTerminalReplayAvoidUnavailableGenuineTeaching()
    {
        using var fixture = new EndpointTrainingFixture(10000, 2);
        fixture.ProvisionOrdinaryV2(5, genuineTeachingCurriculum: fixture.SealGenuineTeaching());
        var source = fixture.OrdinaryGrant.SourceSelection!.TeachingCurriculum!.Sources[0];
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(fixture.RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion).Value!;
        File.Move(address.ManifestPath, address.ManifestPath + ".unavailable");
        try
        {
            var first = await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
            {
                var completed = await OrdinaryPost(app, f); Assert.Equal("Completed", completed.Disposition);
                Assert.Equal(10, factory.Client!.EvaluationCalls); Assert.Equal(1, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                Assert.False(File.Exists(address.ManifestPath)); return completed;
            }, false, c => new OrdinaryEvaluationClient(c, true), disposeFixture: false);
            var history = File.ReadAllBytes(fixture.OrdinaryHistory); var ticks = File.GetLastWriteTimeUtc(fixture.OrdinaryHistory);
            MakeOrdinaryExternalInputsUnavailable(fixture);
            await RunStartedFixtureAsync(fixture, "MUST_NOT_CONSTRUCT_EVALUATOR", null, async (f, factory, app) =>
            {
                var replay = await OrdinaryPost(app, f); Assert.Equal("TerminalReplay", replay.Disposition);
                Assert.Equal(first.TerminalOutcomeSha256, replay.TerminalOutcomeSha256);
                Assert.Equal(first.TerminalOutcome!.Value.GetRawText(), replay.TerminalOutcome!.Value.GetRawText());
                Assert.Equal(0, factory.CreateCalls); Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                Assert.Equal(history, File.ReadAllBytes(f.OrdinaryHistory)); Assert.Equal(ticks, File.GetLastWriteTimeUtc(f.OrdinaryHistory));
                Assert.False(File.Exists(address.ManifestPath)); return true;
            }, false, null, disposeFixture: false);
        }
        finally { File.Move(address.ManifestPath + ".unavailable", address.ManifestPath); }
    }

    private static async Task AssertOriginalExclusionHttpFailure(WebApplication app, EndpointTrainingFixture f, HttpStatusCode status, string code)
    {
        using var client = new HttpClient(); using var response = await client.PostAsJsonAsync(
            new Uri(Program.ResolveStartedLoopbackAddress(app), CodeDocumentationOneLessonPreflightWireContract.EndpointPath), new CodeDocumentationOneLessonPreflightRequestV1(f.OrdinarySelector));
        var raw = await response.Content.ReadAsStringAsync(); using var document = JsonDocument.Parse(raw); var body = document.RootElement;
        Assert.Equal(status, response.StatusCode); Assert.False(body.GetProperty("isSuccess").GetBoolean()); Assert.Equal(code, body.GetProperty("errorCode").GetString());
        Assert.Equal(new[] { "bindingSha256", "disposition", "errorCode", "errorMessage", "isSuccess", "objectiveId", "sessionId", "sessionRevision", "terminalOutcome", "terminalOutcomeSha256", "trainingRunId" },
            body.EnumerateObject().Select(p => p.Name).Order());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("terminalOutcome").ValueKind);
        foreach (var secret in f.Corpus.Corpus.Entries.SelectMany(e => new[] { e.EntryId, e.SourceEntityFingerprint, e.OwnerInput, e.PaidModelReferenceAnswer }))
            Assert.DoesNotContain(secret, raw, StringComparison.Ordinal);
        using var saved = JsonDocument.Parse(File.ReadAllBytes(f.OrdinaryBaseline));
        var baseline = saved.RootElement.GetProperty("Baseline").Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>()!;
        foreach (var item in baseline.Calibration.RepresentativeAssessment.Cases)
            foreach (var secret in new[] { item.CaseId, item.Reason, item.StudentAnswerPreview, item.StudentAnswerSha256 }.Where(s => s.Length > 1))
                Assert.DoesNotContain(secret, raw, StringComparison.Ordinal);
        foreach (var forbidden in new[] { "TeachingIdentities", "Categories", "OriginalReceipt", "OrdinaryOriginalTeachingExclusion" })
            Assert.DoesNotContain(forbidden, raw, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertOriginalExclusionNoDownstream(WebApplication app, EndpointTrainingFixture f)
    {
        Assert.False(File.Exists(f.OrdinaryHistory)); // No session frame, permit, authoring, training or lesson completion is admitted.
        Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
    }

    private static Dictionary<string, (byte[] Bytes, long Ticks)> CaptureOriginalExclusionFiles(EndpointTrainingFixture f)
    {
        var adapterOwnerLock = Path.GetFullPath(Path.Combine(f.Root, "adapter", ".continuous-learning.adapter-owner.lock"));
        var registryOwnerLock = Path.GetFullPath(Path.Combine(f.Root, "registry", ".continuous-learning.registry-owner.lock"));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return f.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p =>
        {
            if (p.Equals(adapterOwnerLock, comparison) || p.Equals(registryOwnerLock, comparison))
            {
                // These two exact started-host locks are held with FileShare.None: prove read refusal,
                // retain its path/presence/mtime, and never label the unread bytes as verified.
                Assert.True(File.Exists(p)); Assert.Throws<IOException>(() => File.ReadAllBytes(p));
                return (Array.Empty<byte>(), File.GetLastWriteTimeUtc(p).Ticks);
            }
            return (File.ReadAllBytes(p), File.GetLastWriteTimeUtc(p).Ticks);
        });
    }

    private static void AssertOriginalExclusionFiles(EndpointTrainingFixture f, Dictionary<string, (byte[] Bytes, long Ticks)> before)
    {
        var after = CaptureOriginalExclusionFiles(f); Assert.Equal(before.Keys.Order(), after.Keys.Order());
        foreach (var p in before.Keys) { Assert.Equal(before[p].Bytes, after[p].Bytes); Assert.Equal(before[p].Ticks, after[p].Ticks); }
    }

    private static void AssertOriginalExclusionFreshFiles(EndpointTrainingFixture f, Dictionary<string, (byte[] Bytes, long Ticks)> before)
    {
        var after = CaptureOriginalExclusionFiles(f);
        foreach (var p in before.Keys)
        {
            Assert.True(after.ContainsKey(p), p);
            Assert.Equal(before[p].Bytes, after[p].Bytes); Assert.Equal(before[p].Ticks, after[p].Ticks);
        }
        // Only the genuine ORIGINAL producer and held session owner may add these exact paths.
        // Every model, DPO and complete teaching/training corpus file remains in the immutable snapshot.
        var original = RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(f.RepositoryRoot,
            EndpointTrainingFixture.DomainSlug, EndpointTrainingFixture.DesignPatternSlug,
            f.ModelIdSlug, f.ModelVersion,
            Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(f.OrdinarySelector.ObjectiveId))).ToLowerInvariant());
        Assert.True(original.IsSuccess, original.ErrorCode);
        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            f.OrdinaryBaseline, f.OrdinaryClaim, original.Value!.LockPath,
            Path.Combine(Path.GetDirectoryName(f.OrdinaryHistory)!, ".owner.lock")
        };
        foreach (var p in after.Keys.Except(before.Keys)) Assert.True(allowed.Contains(p), p);
        Assert.True(after.ContainsKey(f.OrdinaryBaseline)); Assert.True(after.ContainsKey(f.OrdinaryClaim));
    }

    [Fact]
    public async Task OrdinaryV2_PassiveCurriculum_ProtectedStartupSufficientAndFreshHostReplayKeepCommitmentWithoutTeachingEffects()
    {
        using var fixture = new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5, teachingCurriculum: true);
        var source = fixture.OrdinaryGrant.SourceSelection!; var hash = source.SourceSelectionSha256;
        Assert.Equal(hash, fixture.OrdinaryGrant.DescriptorsV2!.SourceSelectionSha256);
        var reopened = source.OpenFresh(fixture.RepositoryRoot);
        Assert.True(reopened.IsSuccess, reopened.ErrorCode);
        Assert.Equal(hash, reopened.Value!.Selection.SourceSelectionSha256);
        Assert.Equal(CodeDocumentationOneLessonSourceSelectionTests.CurriculumLiteral,
            CodeDocumentationOneLessonSourceSelectionTests.TeachingJson(reopened.Value.Selection.TeachingCurriculum!));
        fixture.AssertTeachingDirectoryAbsent();
        var first = await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            var response = await OrdinaryPost(app, f);
            Assert.Equal("Completed", response.Disposition); Assert.Equal(6, response.SessionRevision);
            Assert.Equal(10, factory.Client!.EvaluationCalls); Assert.Equal(1, factory.Client.DisposeCalls);
            var runner = app.Services.GetRequiredService<CodeDocumentationTrainingRunner>();
            Assert.Equal(1, runner.SelectedProfileTrainCalls); Assert.Equal(50, runner.SelectedProfileTrainExampleCount);
            var frames = File.ReadAllLines(f.OrdinaryHistory); Assert.Equal(7, frames.Length);
            foreach (var frame in frames)
            {
                using var doc = JsonDocument.Parse(frame);
                Assert.Equal(hash, doc.RootElement.GetProperty("BindingV2").GetProperty("DescriptorsV2").GetProperty("SourceSelectionSha256").GetString());
                Assert.Equal("0", doc.RootElement.GetProperty("Effects").GetProperty("TeachingDispatched").GetString());
                Assert.Equal("0", doc.RootElement.GetProperty("Effects").GetProperty("TeachingCompleted").GetString());
                Assert.Equal("0", doc.RootElement.GetProperty("Effects").GetProperty("OptimizerEntries").GetString());
            }
            AssertIndependentEarlyReceipts(f, response, frames, JsonNode.Parse(File.ReadAllText(f.OrdinaryBaseline))!);
            f.AssertTeachingDirectoryAbsent();
            return response;
        }, false, c => new OrdinaryEvaluationClient(c, true), disposeFixture: false);
        var bytes = File.ReadAllBytes(fixture.OrdinaryHistory); var ticks = File.GetLastWriteTimeUtc(fixture.OrdinaryHistory);
        MakeOrdinaryExternalInputsUnavailable(fixture);
        var replay = await RunStartedFixtureAsync(fixture, "MUST_NOT_CONSTRUCT_EVALUATOR", null, async (f, factory, app) =>
        {
            File.Move(f.CorpusAddress.ManifestPath, f.CorpusAddress.ManifestPath + ".unavailable");
            try
            {
                var response = await OrdinaryPost(app, f);
                Assert.Equal(0, factory.CreateCalls);
                Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                f.AssertTeachingDirectoryAbsent(); return response;
            }
            finally { File.Move(f.CorpusAddress.ManifestPath + ".unavailable", f.CorpusAddress.ManifestPath); }
        }, false, null, disposeFixture: false);
        Assert.Equal("TerminalReplay", replay.Disposition); Assert.Equal(6, replay.SessionRevision);
        Assert.Equal(first.TerminalOutcomeSha256, replay.TerminalOutcomeSha256);
        Assert.Equal(first.TerminalOutcome!.Value.GetRawText(), replay.TerminalOutcome!.Value.GetRawText());
        Assert.Equal(bytes, File.ReadAllBytes(fixture.OrdinaryHistory)); Assert.Equal(ticks, File.GetLastWriteTimeUtc(fixture.OrdinaryHistory));
        fixture.AssertTeachingDirectoryAbsent();
        output.WriteLine("PASSIVE_CURRICULUM_RUNTIME source=" + hash + " Completed/revision6 -> fresh-host TerminalReplay/revision6 evaluation10/0 profile1/0 teaching0/0");
    }

    [Fact]
    public async Task OrdinaryV2_PassiveCurriculum_ActualHttpNeedsRemains503WithoutTeachingOrProfile()
    {
        var fixture = new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5, teachingCurriculum: true);
        await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            var failure = await OrdinaryFailure(app, f);
            Assert.Equal(HttpStatusCode.Conflict, failure.Status); // Declared passive literals lack genuine material; this is not the absent-declaration contract.
            Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, failure.Code);
            Assert.Equal(10, factory.Client!.EvaluationCalls);
            Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            Assert.False(File.Exists(f.OrdinaryHistory)); f.AssertTeachingDirectoryAbsent();
            return true;
        }, false, c => new OrdinaryEvaluationClient(c, false));
    }

    [Theory]
    [InlineData(2, 5)]
    [InlineData(2, 2500)]
    [InlineData(1, 5000)]
    public async Task OrdinaryV2_BaselineSufficient_CompletesSevenDurableFrames_ThroughProgramAndClient(int cases, int repeats)
    {
        var fixture = new EndpointTrainingFixture(10000, cases);
        fixture.ProvisionOrdinaryV2(repeats);
        await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            var response = await OrdinaryPost(app, f);
            Assert.Equal("Completed", response.Disposition); Assert.Equal(6, response.SessionRevision);
            Assert.Equal(cases * repeats, factory.Client!.EvaluationCalls); Assert.Equal(1, factory.Client.DisposeCalls);
            var runner = app.Services.GetRequiredService<CodeDocumentationTrainingRunner>();
            Assert.Equal(1, runner.SelectedProfileTrainCalls); Assert.Equal(50, runner.SelectedProfileTrainExampleCount);
            Assert.NotEmpty(runner.SelectedProfileTrainResult!);
            Assert.Same(app.Services.GetRequiredService<RoutingProfileTrainer>(),
                typeof(CodeDocumentationTrainingRunner).GetField("_profileTrainer",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(runner));
            Assert.Same(runner,typeof(CodeDocumentationBatchFileRunner).GetField("_runner",BindingFlags.Instance|BindingFlags.NonPublic)!
                .GetValue(app.Services.GetRequiredService<CodeDocumentationBatchFileRunner>()));
            Assert.All(Enumerable.Range(0,50),i=>Assert.Contains("ordinary"+(char)('a'+i/26)+(char)('a'+i%26),
                runner.SelectedProfileTrainResult![OrchestrationRoutingCurriculum.DocumentationDotNetUnitId].Keywords));
            var frames = File.ReadAllLines(f.OrdinaryHistory);
            Assert.Equal(7, frames.Length);
            var previous = "";
            for (var i = 0; i < frames.Length; i++)
            {
                using var doc = JsonDocument.Parse(frames[i]); var root = doc.RootElement;
                Assert.Equal(i.ToString(CultureInfo.InvariantCulture), root.GetProperty("Revision").GetString());
                Assert.Equal(previous, root.GetProperty("PreviousFrameSha256").GetString());
                Assert.Equal(IndependentJcs(JsonNode.Parse(frames[i])!), frames[i]);
                Assert.All(root.GetProperty("Effects").EnumerateObject(), p =>
                { if (p.Name == "EffectOutcomeUnknown") Assert.False(p.Value.GetBoolean()); else Assert.Equal("0", p.Value.GetString()); });
                previous = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(frames[i]))).ToLowerInvariant();
            }
            Assert.Equal("ProfileTrainingStarted", JsonNode.Parse(frames[2])!["TransitionEvent"]!.GetValue<string>());
            var original = JsonNode.Parse(File.ReadAllText(f.OrdinaryBaseline))!;
            var baseline = original["Baseline"]!;
            Assert.Equal(cases * repeats, baseline["ExpectedEvaluationCallCount"]!.GetValue<int>());
            Assert.Equal(cases * repeats, baseline["ObservedEvaluationCallCount"]!.GetValue<int>());
            var ids = baseline["Calibration"]!["Calibration"]!["Observations"]!.AsArray()
                .SelectMany(x => x!["ObservedEvaluationRequestIds"]!.AsArray()).Select(x => x!.GetValue<string>()).ToArray();
            Assert.Equal(cases * repeats, ids.Length); Assert.Equal(ids.Length, ids.Distinct(StringComparer.Ordinal).Count());
            AssertIndependentEarlyReceipts(f,response,frames,original);
            var historyBytes=File.ReadAllBytes(f.OrdinaryHistory); var historyTicks=File.GetLastWriteTimeUtc(f.OrdinaryHistory);
            var replay=await OrdinaryPost(app,f);
            Assert.Equal("TerminalReplay",replay.Disposition); Assert.Equal(response.TerminalOutcomeSha256,replay.TerminalOutcomeSha256);
            Assert.Equal(historyBytes,File.ReadAllBytes(f.OrdinaryHistory)); Assert.Equal(historyTicks,File.GetLastWriteTimeUtc(f.OrdinaryHistory));
            Assert.Equal(1,factory.CreateCalls); Assert.Equal(cases*repeats,factory.Client.EvaluationCalls);
            Assert.Equal(1,runner.SelectedProfileTrainCalls);
            foreach(var assembly in new[] { typeof(CodeDocumentationOneLessonSession).Assembly,
                typeof(CodeDocumentationRepositoryOwnedPreparedBaselineJournal).Assembly, typeof(Program).Assembly,
                typeof(CodeDocumentationBatchRunnerCli).Assembly, GetType().Assembly })
                output.WriteLine("ORDINARY_MODULE "+JsonSerializer.Serialize(new { path=assembly.Location,
                    sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))).ToLowerInvariant(),
                    byteLength=new FileInfo(assembly.Location).Length,utcTicks=File.GetLastWriteTimeUtc(assembly.Location).Ticks.ToString(CultureInfo.InvariantCulture),
                    mvid=assembly.ManifestModule.ModuleVersionId.ToString(), informationalVersion=assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion }));
            return response;
        }, false, c => new OrdinaryEvaluationClient(c, true));
    }

    [Fact]
    public async Task OrdinaryV2_IdenticalSelector_TerminalReplay_FreshHost_NoExternalIO()
    {
        using var fixture = new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        var first = await RunStartedFixtureAsync(fixture, null, null,
            async (f, factory, app) => await OrdinaryPost(app, f), false,
            c => new OrdinaryEvaluationClient(c, true), disposeFixture: false);
        var bytes = File.ReadAllBytes(fixture.OrdinaryHistory); var ticks = File.GetLastWriteTimeUtc(fixture.OrdinaryHistory);
        // Make every selected external owner unavailable after the first host has really closed.
        File.Move(fixture.OrdinaryBaseline, fixture.OrdinaryBaseline + ".unavailable");
        File.Move(fixture.OrdinaryTraining, fixture.OrdinaryTraining + ".unavailable");
        var parentAddress = RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(fixture.RepositoryRoot,
            EndpointTrainingFixture.DomainSlug, EndpointTrainingFixture.DesignPatternSlug, fixture.ModelIdSlug, fixture.ModelVersion, 1).Value!;
        File.Move(parentAddress.ManifestPath, parentAddress.ManifestPath + ".unavailable");
        var replay = await RunStartedFixtureAsync(fixture, "MUST_NOT_CONSTRUCT_EVALUATOR", null,
            async (f, factory, app) =>
            {
                File.Move(f.CorpusAddress.ManifestPath,f.CorpusAddress.ManifestPath+".unavailable");
                try
                {
                    var response = await OrdinaryPost(app, f); Assert.Equal(0, factory.CreateCalls);
                    Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                    return response;
                }
                finally { File.Move(f.CorpusAddress.ManifestPath+".unavailable",f.CorpusAddress.ManifestPath); }
            }, false, null, disposeFixture: false);
        Assert.Equal("TerminalReplay", replay.Disposition);
        Assert.Equal(first.TerminalOutcome!.Value.GetRawText(), replay.TerminalOutcome!.Value.GetRawText());
        Assert.Equal(first.TerminalOutcomeSha256, replay.TerminalOutcomeSha256);
        Assert.Equal(bytes, File.ReadAllBytes(fixture.OrdinaryHistory)); Assert.Equal(ticks, File.GetLastWriteTimeUtc(fixture.OrdinaryHistory));
        fixture.ChangeOrdinaryTrainingRunBinding();
        await RunStartedFixtureAsync(fixture,"MUST_NOT_CONSTRUCT_EVALUATOR",null,async(f,factory,app)=>
        {
            var refused=await OrdinaryFailure(app,f);
            Assert.Equal(HttpStatusCode.Conflict,refused.Status);
            Assert.Equal("OBJECTIVE_BINDING_CONFLICT",refused.Code);
            Assert.Equal(0,factory.CreateCalls); Assert.Equal(bytes,File.ReadAllBytes(f.OrdinaryHistory));
            return true;
        },false,null,disposeFixture:false);
    }

    [Fact]
    public async Task OrdinaryV2_UnicodeObjective_CompletedAndFreshHostReplay_PreserveCanonicalHttpBytes()
    {
        using var fixture = new EndpointTrainingFixture(10000, 2);
        fixture.ProvisionOrdinaryV2(5, "lesson　one");
        var completedBodies = new List<byte[]>();
        var first = await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            var response = await OrdinaryPost(app, f);
            Assert.Equal("Completed", response.Disposition);
            Assert.Equal("lesson　one", response.ObjectiveId);
            Assert.Equal(1, factory.CreateCalls);
            Assert.Equal(10, factory.Client!.EvaluationCalls);
            Assert.Equal(1, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            return response;
        }, false, c => new OrdinaryEvaluationClient(c, true), disposeFixture: false,
            responseObserver: completedBodies.Add);
        var history = File.ReadAllBytes(fixture.OrdinaryHistory);
        var ticks = File.GetLastWriteTimeUtc(fixture.OrdinaryHistory);
        var frames = File.ReadAllLines(fixture.OrdinaryHistory);
        Assert.Equal(7, frames.Length);
        foreach (var frame in frames)
        {
            using var document = JsonDocument.Parse(frame);
            Assert.All(document.RootElement.GetProperty("Effects").EnumerateObject(), p =>
            {
                if (p.Name == "EffectOutcomeUnknown") Assert.False(p.Value.GetBoolean());
                else Assert.Equal("0", p.Value.GetString());
            });
        }
        using var terminalFrame = JsonDocument.Parse(frames[^1]);
        var sealedOutcome = terminalFrame.RootElement.GetProperty("TerminalOutcome").GetRawText();
        Assert.Contains("lesson　one", sealedOutcome, StringComparison.Ordinal);
        Assert.Equal(sealedOutcome, first.TerminalOutcome!.Value.GetRawText());
        Assert.Equal(IndependentHash(sealedOutcome), first.TerminalOutcomeSha256);
        using var completed = JsonDocument.Parse(Assert.Single(completedBodies));
        Assert.Equal(11, completed.RootElement.EnumerateObject().Count());
        Assert.Equal(sealedOutcome, completed.RootElement.GetProperty("terminalOutcome").GetRawText());
        MakeOrdinaryExternalInputsUnavailable(fixture);
        var replayBodies = new List<byte[]>();
        var replay = await RunStartedFixtureAsync(fixture, "MUST_NOT_CONSTRUCT_EVALUATOR", null, async (f, factory, app) =>
        {
            File.Move(f.CorpusAddress.ManifestPath, f.CorpusAddress.ManifestPath + ".unavailable");
            try
            {
                var response = await OrdinaryPost(app, f);
                Assert.Equal(0, factory.CreateCalls);
                Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                return response;
            }
            finally { File.Move(f.CorpusAddress.ManifestPath + ".unavailable", f.CorpusAddress.ManifestPath); }
        }, false, null, disposeFixture: false, responseObserver: replayBodies.Add);
        Assert.Equal("TerminalReplay", replay.Disposition);
        Assert.Equal(first.ObjectiveId, replay.ObjectiveId);
        Assert.Equal(first.SessionId, replay.SessionId);
        Assert.Equal(first.BindingSha256, replay.BindingSha256);
        Assert.Equal(6, replay.SessionRevision);
        Assert.Equal(first.TerminalOutcomeSha256, replay.TerminalOutcomeSha256);
        Assert.Equal(sealedOutcome, replay.TerminalOutcome!.Value.GetRawText());
        using var replayWire = JsonDocument.Parse(Assert.Single(replayBodies));
        Assert.Equal(11, replayWire.RootElement.EnumerateObject().Count());
        Assert.Equal(sealedOutcome, replayWire.RootElement.GetProperty("terminalOutcome").GetRawText());
        Assert.Equal(history, File.ReadAllBytes(fixture.OrdinaryHistory));
        Assert.Equal(ticks, File.GetLastWriteTimeUtc(fixture.OrdinaryHistory));
        output.WriteLine("CLIENT_UNICODE_CANONICAL Completed+fresh-host-TerminalReplay objective=lesson　one revision=6 evaluator=10/0 profile=1/0 history=unchanged");
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("TerminalReplay")]
    public async Task OrdinaryV2_ActualClient_RejectsUnchangedGenuineOtherObjectiveResponse(string disposition)
    {
        using var requestedFixture = new EndpointTrainingFixture(10000, 2);
        requestedFixture.ProvisionOrdinaryV2(5);
        using var fixture = new EndpointTrainingFixture(10000, 2);
        fixture.ProvisionOrdinaryV2(5);
        var bodies = new List<byte[]>();
        await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) => await OrdinaryPost(app, f),
            false, c => new OrdinaryEvaluationClient(c, true), disposeFixture: false, responseObserver: bodies.Add);
        if (disposition == "TerminalReplay")
        {
            MakeOrdinaryExternalInputsUnavailable(fixture);
            bodies.Clear();
            await RunStartedFixtureAsync(fixture, "MUST_NOT_CONSTRUCT_EVALUATOR", null, async (f, factory, app) =>
            {
                File.Move(f.CorpusAddress.ManifestPath, f.CorpusAddress.ManifestPath + ".unavailable");
                try
                {
                    var replay = await OrdinaryPost(app, f);
                    Assert.Equal(0, factory.CreateCalls);
                    Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                    return replay;
                }
                finally { File.Move(f.CorpusAddress.ManifestPath + ".unavailable", f.CorpusAddress.ManifestPath); }
            }, false, null, disposeFixture: false, responseObserver: bodies.Add);
        }
        var genuine = Assert.Single(bodies);
        using var genuineDocument = JsonDocument.Parse(genuine);
        Assert.Equal(disposition, genuineDocument.RootElement.GetProperty("disposition").GetString());
        var history = File.ReadAllBytes(fixture.OrdinaryHistory);
        var ticks = File.GetLastWriteTimeUtc(fixture.OrdinaryHistory);
        var requests = new List<CodeDocumentationOneLessonSelectorV1>();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var transport = builder.Build();
        // This transport returns the exact untouched bytes obtained from a real Program terminal response B.
        transport.MapPost(CodeDocumentationOneLessonPreflightWireContract.EndpointPath, async (HttpContext context) =>
        {
            var request = await context.Request.ReadFromJsonAsync<CodeDocumentationOneLessonPreflightRequestV1>();
            requests.Add(request!.Selector!);
            return Results.Bytes(genuine, "application/json");
        });
        await transport.StartAsync();
        try
        {
            var client = new CodeDocumentationOneLessonPreflightClient();
            var uri = Program.ResolveStartedLoopbackAddress(transport);
            var control = await client.PreflightAsync(uri, fixture.OrdinarySelector, TimeSpan.FromSeconds(10));
            Assert.True(control.IsSuccess, control.ErrorCode);
            Assert.Equal(disposition, control.Value!.Disposition);
            Assert.NotEqual(fixture.OrdinarySelector.ObjectiveId, requestedFixture.OrdinarySelector.ObjectiveId);
            var rejected = await client.PreflightAsync(uri, requestedFixture.OrdinarySelector, TimeSpan.FromSeconds(10));
            Assert.Equal(new[] { fixture.OrdinarySelector, requestedFixture.OrdinarySelector }, requests);
            Assert.False(rejected.IsSuccess);
            Assert.Equal("CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_RESPONSE_INVALID", rejected.ErrorCode);
            Assert.Equal(history, File.ReadAllBytes(fixture.OrdinaryHistory));
            Assert.Equal(ticks, File.GetLastWriteTimeUtc(fixture.OrdinaryHistory));
            Assert.False(File.Exists(requestedFixture.OrdinaryHistory));
            Assert.False(File.Exists(requestedFixture.OrdinaryBaseline));
            output.WriteLine("CLIENT_OBJECTIVE_BINDING " + disposition + " unchanged-genuine-B accepted-for-B rejected-for-actual-A-request history=unchanged");
        }
        finally { await transport.StopAsync(); }
    }

    private static void MakeOrdinaryExternalInputsUnavailable(EndpointTrainingFixture fixture)
    {
        File.Move(fixture.OrdinaryBaseline, fixture.OrdinaryBaseline + ".unavailable");
        File.Move(fixture.OrdinaryTraining, fixture.OrdinaryTraining + ".unavailable");
        var parent = RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(fixture.RepositoryRoot,
            EndpointTrainingFixture.DomainSlug, EndpointTrainingFixture.DesignPatternSlug, fixture.ModelIdSlug, fixture.ModelVersion, 1).Value!;
        File.Move(parent.ManifestPath, parent.ManifestPath + ".unavailable");
    }

    [Fact]
    public void OrdinaryV2_IndependentLiteralDecisionInputsAndReceiptIds()
    {
        const string session = "5a04a13c212832e6d835b80975559d368eb36e01824213c6e5f2d35a0c475e24";
        var binding = new string('1', 64);
        Assert.Equal("8d288ded90f899ededc8e6777143d63d", CodeDocumentationOneLessonSession.EarlyReceiptId(session, "QualityDecision", 2, binding));
        Assert.Equal("05aa34c733add733e4293eed4f05bc0e", CodeDocumentationOneLessonSession.EarlyReceiptId(session, "Disposition", 3, binding));
        Assert.Equal("08fef9a02fd36c5f2750255647cde8da", CodeDocumentationOneLessonSession.EarlyReceiptId(session, "FinalReopen", 6, binding));
        Assert.Equal("5cc6e78857842cd74ec91f2da0c70f48923553c745d6e3d00cec1dd6357dd2f7",
            CodeDocumentationOneLessonSession.DecisionInputsSha256(binding,
                new(new string('a',32),new string('b',64)),1,2,new string('c',64)));
        Assert.Equal("eb75ed003770be44667e9df88b68ce41",IndependentHash("xiigen-one-lesson-outcome-id-v2\n"+session+"\n"+binding+
            "\nTERMINAL_NEGATIVE_PARENT_RETAINED\n08fef9a02fd36c5f2750255647cde8da")[..32]);
    }

    [Fact]
    public async Task OrdinaryV2_InsufficientOrInvalidBaseline_PreProfileUnsupported()
    {
        var fixture = new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            var invalidIndex=0;
            var expectedErrors=new[] { CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateInvalidErrorCode,
                CodeDocumentationOneLessonEvaluatorSelection.InvalidErrorCode,CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode };
            foreach(var invalid in new Func<CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext,CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext>[] {
                c=>c with { NoisePolicyConfig=null! },
                c=>c with { NoisePolicyConfig=new(5,3) },
                c=>c with { MaximumDuration=CodeDocumentationSavedModelAssessmentCalibrationService.DefaultMaximumDuration+TimeSpan.FromSeconds(1) } })
            {
                var refused=await TryPrepareOrdinaryPrerequisite(app,f,factory,invalid);
                Assert.False(refused.IsSuccess); Assert.Equal(expectedErrors[invalidIndex++],refused.ErrorCode);
                Assert.Equal(0,factory.Client!.EvaluationCalls); Assert.Equal(1,factory.Client.DisposeCalls);
                Assert.False(File.Exists(f.OrdinaryHistory)); Assert.False(File.Exists(f.OrdinaryBaseline));
                Assert.Equal(0,app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            }
            using var client = new HttpClient();
            using var response = await client.PostAsJsonAsync(new Uri(Program.ResolveStartedLoopbackAddress(app),
                CodeDocumentationOneLessonPreflightWireContract.EndpointPath), new CodeDocumentationOneLessonPreflightRequestV1(f.OrdinarySelector));
            var raw = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Contains("ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE", raw); // This fixture deliberately has no bound teaching declaration.
            Assert.False(File.Exists(f.OrdinaryHistory));
            Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            Assert.Equal(10, factory.Client!.EvaluationCalls);
            return true;
        }, false, c => new OrdinaryEvaluationClient(c, false));
        var throwingFixture=new EndpointTrainingFixture(10000,2); throwingFixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(throwingFixture,null,()=>throw new InvalidOperationException("ordinary pre-profile dispose failure"),
            async(f,factory,app)=>
            {
                var refused=await OrdinaryFailure(app,f);
                Assert.Equal(HttpStatusCode.ServiceUnavailable,refused.Status);
                Assert.Equal("ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE",refused.Code);
                Assert.Equal(1,factory.Client!.DisposeCalls); Assert.Equal(10,factory.Client.EvaluationCalls);
                Assert.False(File.Exists(f.OrdinaryHistory));
                Assert.Equal(0,app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
                return true;
            },false,c=>new OrdinaryEvaluationClient(c,false));
    }

    private static async Task<CodeDocumentationOneLessonPreflightResponseV1> OrdinaryPost(WebApplication app, EndpointTrainingFixture f)
    {
        var result = await new CodeDocumentationOneLessonPreflightClient().PreflightAsync(
            Program.ResolveStartedLoopbackAddress(app), f.OrdinarySelector, TimeSpan.FromMinutes(2));
        Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage); Assert.NotNull(result.Value);
        return result.Value!;
    }

    [Fact]
    public async Task OrdinaryV2_CompletedAndReplay_Cli_ReturnBeforeRunRoot()
    {
        var fixture = new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            var forbidden = 0; var output = new List<string>(); var responses = new List<string>();
            int Forbidden() { forbidden++; throw new InvalidOperationException("CLI effect forbidden"); }
            var client = new CodeDocumentationOneLessonPreflightClient();
            var dependencies = new CodeDocumentationBatchRunnerDependencies
            {
                Now = () => DateTimeOffset.UtcNow, ReadEnvironment = _ => null,
                FileExists = _ => Forbidden() == 0, CreateDirectory = _ => { Forbidden(); },
                WriteTextAsync = (_,_,_,_) => { Forbidden(); return Task.CompletedTask; },
                WriteLinesAsync = (_,_,_,_) => { Forbidden(); return Task.CompletedTask; },
                WriteStdOut = output.Add, WriteStdErr = text => throw new InvalidOperationException(text),
                ResolveAuthoringConfiguration = _ => { Forbidden(); return OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail("forbidden"); },
                CreateTeacherClient = _ => { Forbidden(); return OperationResult<ITrainingTeacherInferenceClient>.Fail("forbidden"); },
                RunBatchAsync = (_,_,_,_) => { Forbidden(); return Task.FromResult(OperationResult<CodeDocumentationTrainingRunResult>.Fail("forbidden")); },
                PublishRepositoryOwnedDpoAsync = (_,_) => { Forbidden(); return Task.FromResult(OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>.Fail("forbidden")); },
                PreflightOneLessonAsync = async (uri,selector,timeout,ct) =>
                { var actual = await client.PreflightAsync(uri,selector,timeout,ct); Assert.True(actual.IsSuccess,actual.ErrorCode); responses.Add(actual.Value!.Disposition!); return actual; },
            };
            var args = new[] { "--one-lesson-objective", f.OrdinarySelector.ObjectiveId, "--one-lesson-grant",
                f.OrdinarySelector.GrantSelector, "--model-server-base-uri", Program.ResolveStartedLoopbackAddress(app).ToString() };
            Assert.Equal(0, await CodeDocumentationBatchRunnerCli.RunAsync(args, dependencies));
            Assert.Equal(0, await CodeDocumentationBatchRunnerCli.RunAsync(args, dependencies));
            Assert.Equal(new[] { "Completed", "TerminalReplay" }, responses);
            Assert.Equal(0,forbidden); Assert.Equal(2,output.Count); Assert.All(output,t => Assert.Contains("terminal_outcome=",t));
            Assert.Equal(10,factory.Client!.EvaluationCalls);
            return true;
        },false,c => new OrdinaryEvaluationClient(c,true));
    }

    [Fact]
    public async Task OrdinaryV2_EarlyPrefix_MutationsFailClosed_NoRetrain()
    {
        var fixture = new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture, null, null, async (f,factory,app) =>
        {
            await OrdinaryPost(app,f); var genuine = File.ReadAllBytes(f.OrdinaryHistory);
            var mutations = new Dictionary<string,Action<JsonNode[]>>(StringComparer.Ordinal)
            {
                ["qd-decision"] = a => a[3]["ProgressReceipt"]!["Payload"]!["Decision"] = "NeedsLesson",
                ["decision-inputs"] = a => a[3]["ProgressReceipt"]!["Payload"]!["DecisionInputsSha256"] = new string('e',64),
                ["qd-slot"] = a => a[3]["ProgressReceipt"]!["ReceiptId"] = new string('1',32),
                ["qd-foreign-binding"] = a => a[3]["ProgressReceipt"]!["BindingSha256"] = new string('2',64),
                ["qd-type"] = a => a[3]["ProgressReceipt"]!["ReceiptType"] = "Disposition",
                ["qd-local-baseline"] = a => a[3]["ProgressReceipt"]!["Payload"]!["BaselineRef"] = new JsonObject(),
                ["disposition-role"] = a => a[4]["ProgressReceipt"]!["Payload"]!["Disposition"] = "PromoteCandidate",
                ["selected-parent-id"] = a => a[4]["ProgressReceipt"]!["Payload"]!["SelectedCheckpointSha256"] = f.OrdinaryGrant.DescriptorsV2!.ParentCheckpointSha256,
                ["final-parent-id"] = a => a[5]["ProgressReceipt"]!["Payload"]!["CheckpointSha256"] = f.OrdinaryGrant.DescriptorsV2!.ParentCheckpointSha256,
                ["final-manifest"] = a => a[5]["ProgressReceipt"]!["Payload"]!["ManifestSha256"] = new string('3',64),
                ["final-weights"] = a => a[5]["ProgressReceipt"]!["Payload"]!["WeightsSha256"] = new string('4',64),
                ["final-length-alias"] = a => a[5]["ProgressReceipt"]!["Payload"]!["WeightsByteLengthDecimal"] = "01",
                ["final-role"] = a => a[5]["ProgressReceipt"]!["Payload"]!["SelectedRole"] = "Candidate",
                ["terminal-state"] = a => a[6]["TerminalOutcome"]!["TerminalState"] = "TERMINAL_PROMOTED",
                ["outcome-id"] = a => a[6]["TerminalOutcome"]!["OutcomeId"] = new string('5',32),
                ["post-trial-ref"] = a => a[6]["TerminalOutcome"]!["TrialEvaluationReceipt"] = new JsonObject(),
                ["publication-ref"] = a => a[6]["TerminalOutcome"]!["PublicationReceipt"] = new JsonObject(),
                ["mixed-version"] = a => a[4]["FormatVersion"] = 1,
                ["unknown-effects"] = a => a[2]["Effects"]!["EffectOutcomeUnknown"] = true,
                ["nonzero-effects"] = a => a[2]["Effects"]!["EvaluatorDispatched"] = "1",
                ["decimal-revision"] = a => a[4]["Revision"] = "04",
                ["runner-order"] = a => a[1]["TransitionEvent"] = "ProfileTrainingStarted",
                ["profile-order"] = a => a[2]["TransitionEvent"] = "RunnerStarted",
                ["frame-extra"] = a => a[4]["Unknown"] = 1,
                ["ref-extra"] = a => a[6]["TerminalOutcome"]!["QualityDecisionReceipt"]!["Path"] = "foreign",
                ["ref-quality-forward"] = a => a[4]["ProgressReceipt"]!["Payload"]!["QualityDecisionRef"] =
                    new JsonObject { ["ReceiptId"]=a[5]["ProgressReceipt"]!["ReceiptId"]!.GetValue<string>(),
                        ["ReceiptSha256"]=IndependentHash(IndependentJcs(a[5]["ProgressReceipt"]!)) },
                ["ref-final-wrongtype"] = a => a[5]["ProgressReceipt"]!["Payload"]!["PredecessorReceiptRef"] =
                    new JsonObject { ["ReceiptId"]=a[3]["ProgressReceipt"]!["ReceiptId"]!.GetValue<string>(),
                        ["ReceiptSha256"]=IndependentHash(IndependentJcs(a[3]["ProgressReceipt"]!)) },
                ["ref-final-null"] = a => a[5]["ProgressReceipt"]!["Payload"]!["PredecessorReceiptRef"] = null,
                ["ref-terminal-null"] = a => a[6]["TerminalOutcome"]!["DispositionReceipt"] = null,
                ["ref-terminal-alias"] = a => a[6]["TerminalOutcome"]!["QualityDecisionReceipt"]!["receiptId"] = "alias",
            };
            foreach (var mutation in mutations)
            {
                var nodes = File.ReadAllLines(f.OrdinaryHistory).Select(x => JsonNode.Parse(x)!).ToArray();
                mutation.Value(nodes); var raw = RehashOrdinaryChain(nodes);
                if(mutation.Key.StartsWith("ref-",StringComparison.Ordinal))
                { mutation.Value(nodes); raw=RehashOrdinaryChain(nodes,refreshReferences:false); }
                // Updating receipt refs and outer links keeps these tests focused on inner typed semantics.
                File.WriteAllBytes(f.OrdinaryHistory, raw);
                var rejected = await OrdinaryFailure(app,f);
                Assert.Equal(HttpStatusCode.Conflict,rejected.Status);
                Assert.Contains(rejected.Code,new[] { CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,
                    CodeDocumentationOneLessonSession.InvalidErrorCode,
                    CodeDocumentationOneLessonSession.CapacityExceededErrorCode });
                File.WriteAllBytes(f.OrdinaryHistory,genuine);
                Assert.Equal("TerminalReplay",(await OrdinaryPost(app,f)).Disposition);
            }
            foreach (var raw in new[] { genuine[..^1], Encoding.UTF8.GetBytes(" " + Encoding.UTF8.GetString(genuine)),
                         genuine.Concat(Encoding.UTF8.GetBytes(File.ReadAllLines(f.OrdinaryHistory)[6]+"\n")).ToArray(),
                         Encoding.UTF8.GetBytes(string.Join("\n",File.ReadAllLines(f.OrdinaryHistory).Where((_,i)=>i!=4))+"\n") })
            {
                File.WriteAllBytes(f.OrdinaryHistory,raw); Assert.Equal(HttpStatusCode.Conflict,(await OrdinaryFailure(app,f)).Status);
                File.WriteAllBytes(f.OrdinaryHistory,genuine); Assert.Equal("TerminalReplay",(await OrdinaryPost(app,f)).Disposition);
            }
            var ordered=File.ReadAllLines(f.OrdinaryHistory).Select(x=>JsonNode.Parse(x)!).ToArray();
            foreach(var changed in new[] {
                new[] { ordered[0].DeepClone(),ordered[2].DeepClone(),ordered[1].DeepClone(),ordered[3].DeepClone(),ordered[4].DeepClone(),ordered[5].DeepClone(),ordered[6].DeepClone() },
                new[] { ordered[0].DeepClone(),ordered[1].DeepClone(),ordered[1].DeepClone(),ordered[3].DeepClone(),ordered[4].DeepClone(),ordered[5].DeepClone(),ordered[6].DeepClone() } })
            {
                for(var i=0;i<changed.Length;i++) changed[i]["Revision"]=i.ToString(CultureInfo.InvariantCulture);
                File.WriteAllBytes(f.OrdinaryHistory,RehashOrdinaryChain(changed,refreshReferences:false));
                Assert.Equal(HttpStatusCode.Conflict,(await OrdinaryFailure(app,f)).Status);
                File.WriteAllBytes(f.OrdinaryHistory,genuine); Assert.Equal("TerminalReplay",(await OrdinaryPost(app,f)).Disposition);
            }
            var duplicate=Encoding.UTF8.GetString(genuine).Replace("\"Revision\":\"3\"","\"Revision\":\"3\",\"Revision\":\"3\"",StringComparison.Ordinal);
            Assert.NotEqual(Encoding.UTF8.GetString(genuine),duplicate);
            File.WriteAllBytes(f.OrdinaryHistory,RehashRawFrameLinks(duplicate));
            Assert.Equal(HttpStatusCode.Conflict,(await OrdinaryFailure(app,f)).Status);
            File.WriteAllBytes(f.OrdinaryHistory,genuine); Assert.Equal("TerminalReplay",(await OrdinaryPost(app,f)).Disposition);
            Assert.Equal(10,factory.Client!.EvaluationCalls);
            Assert.Equal(1,app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true));
    }

    private static byte[] RehashOrdinaryChain(JsonNode[] a,bool refreshReferences=true)
    {
        JsonNode Ref(int i) => new JsonObject { ["ReceiptId"]=a[i]["ProgressReceipt"]!["ReceiptId"]!.GetValue<string>(),
            ["ReceiptSha256"]=IndependentHash(IndependentJcs(a[i]["ProgressReceipt"]!)) };
        if(refreshReferences)
        {
            a[4]["ProgressReceipt"]!["Payload"]!["QualityDecisionRef"] = Ref(3);
            a[5]["ProgressReceipt"]!["Payload"]!["PredecessorReceiptRef"] = Ref(4);
            foreach(var (field,index) in new[] {("QualityDecisionReceipt",3),("DispositionReceipt",4),("FinalReopenReceipt",5)})
                a[6]["TerminalOutcome"]![field] = Ref(index);
        }
        var previous=""; var lines=new List<string>();
        foreach(var node in a) { node["PreviousFrameSha256"]=previous; var line=IndependentJcs(node); lines.Add(line); previous=IndependentHash(line); }
        return Encoding.UTF8.GetBytes(string.Join("\n",lines)+"\n");
    }

    private static byte[] RehashRawFrameLinks(string journal)
    {
        var previous=""; var lines=new List<string>();
        foreach(var raw in journal.Split('\n',StringSplitOptions.RemoveEmptyEntries))
        {
            var line=System.Text.RegularExpressions.Regex.Replace(raw,"\"PreviousFrameSha256\":\"[a-f0-9]*\"",
                "\"PreviousFrameSha256\":\""+previous+"\"");
            lines.Add(line); previous=IndependentHash(line);
        }
        return Encoding.UTF8.GetBytes(string.Join("\n",lines)+"\n");
    }

    private static string IndependentHash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private static void AssertIndependentEarlyReceipts(EndpointTrainingFixture f,CodeDocumentationOneLessonPreflightResponseV1 response,
        string[] frames,JsonNode original)
    {
        var grant=f.OrdinaryGrant; var binding=response.BindingSha256!; var session=IndependentHash(grant.ObjectiveId);
        Assert.Equal(session,response.SessionId);
        var expectedBinding=JsonSerializer.SerializeToNode(new { grant.ObjectiveId,grant.GrantId,grant.GrantVersion,grant.TrainingRunId,
            grant.CandidateSha256,grant.ParentManifestSha256,grant.ParentWeightsSha256,grant.ConfigurationSha256,
            grant.ExamConditionSha256,grant.DescriptorsV2 })!;
        Assert.Equal(IndependentHash("code-documentation-one-lesson-binding-v2\n"+IndependentJcs(expectedBinding)),binding);
        string Id(string type,int slot)=>IndependentHash("xiigen-one-lesson-receipt-id-v2\n"+session+"\n"+type+"\n"+slot+"\n"+binding)[..32];
        var external=JsonNode.Parse(frames[3])!["ProgressReceipt"]!["Payload"]!["ExternalBaselineRef"]!.DeepClone();
        var originalBytes=File.ReadAllBytes(f.OrdinaryBaseline);
        Assert.Equal(IndependentHash(Encoding.UTF8.GetString(originalBytes)),external["ReceiptSha256"]!.GetValue<string>());
        var externalId=IndependentHash("xiigen-s04-original-baseline-reference-v2\n"+session+"\n"+
            original["RequestSha256"]!.GetValue<string>()+"\n"+external["ReceiptSha256"]!.GetValue<string>()+"\n"+originalBytes.Length+"\n")[..32];
        Assert.Equal(externalId,external["ReceiptId"]!.GetValue<string>());
        var count=checked(grant.DescriptorsV2!.CaseCount*grant.DescriptorsV2.RepeatCount);
        var inputs=new JsonObject { ["SchemaVersion"]=2,["BindingSha256"]=binding,["ExternalBaselineRef"]=external.DeepClone(),
            ["CaseCount"]=grant.DescriptorsV2.CaseCount,["RepeatCount"]=grant.DescriptorsV2.RepeatCount,
            ["ExpectedEvaluationCallCountDecimal"]=count.ToString(CultureInfo.InvariantCulture),["ObservedEvaluationCallCountDecimal"]=count.ToString(CultureInfo.InvariantCulture),
            ["NoisePolicyIdentitySha256"]=grant.EvaluatorSelection!.Evaluator.NoisePolicyIdentitySha256,["FinalCertificationTargetCorrectFractionR"]="0.95",
            ["QualityPolicy"]="BaselineRequiresNewTrainingPair",["Decision"]="BaselineSufficient" };
        JsonNode Receipt(string type,int slot,JsonNode payload)=>new JsonObject { ["SchemaVersion"]=2,["ReceiptType"]=type,["ReceiptId"]=Id(type,slot),
            ["ObjectiveId"]=grant.ObjectiveId,["SessionId"]=session,["TrainingRunId"]=grant.TrainingRunId,["BindingSha256"]=binding,["Payload"]=payload };
        JsonNode Ref(JsonNode receipt)=>new JsonObject { ["ReceiptId"]=receipt["ReceiptId"]!.GetValue<string>(),["ReceiptSha256"]=IndependentHash(IndependentJcs(receipt)) };
        var qd=Receipt("QualityDecision",2,new JsonObject { ["Decision"]="BaselineSufficient",["ExternalBaselineRef"]=external.DeepClone(),
            ["DecisionInputsSha256"]=IndependentHash("xiigen-s04-decisioninputs-v2\n"+IndependentJcs(inputs)) });
        var p=grant.CheckpointSelection!.Parent;
        var checkpoint=IndependentHash("xiigen-s04-checkpoint-id-v2\n"+IndependentJcs(JsonSerializer.SerializeToNode(new { p.Step,p.DomainSlug,p.DesignPatternSlug,
            p.ModelIdSlug,p.ModelVersion,p.ManifestSha256,p.WeightsSha256,p.WeightsByteLength })!));
        Assert.NotEqual(grant.DescriptorsV2.ParentCheckpointSha256,checkpoint);
        var disposition=Receipt("Disposition",3,new JsonObject { ["Disposition"]="RetainParent",["QualityDecisionRef"]=Ref(qd),["SelectedCheckpointSha256"]=checkpoint });
        var final=Receipt("FinalReopen",6,new JsonObject { ["SelectedRole"]="Parent",["CheckpointSha256"]=checkpoint,["ManifestSha256"]=p.ManifestSha256,
            ["WeightsSha256"]=p.WeightsSha256,["WeightsByteLengthDecimal"]=p.WeightsByteLength.ToString(CultureInfo.InvariantCulture),["PredecessorReceiptRef"]=Ref(disposition) });
        var outcome=new JsonObject { ["OutcomeId"]=IndependentHash("xiigen-one-lesson-outcome-id-v2\n"+session+"\n"+binding+
                "\nTERMINAL_NEGATIVE_PARENT_RETAINED\n"+Id("FinalReopen",6))[..32],
            ["SchemaVersion"]=2,["ObjectiveId"]=grant.ObjectiveId,["SessionId"]=session,["TrainingRunId"]=grant.TrainingRunId,["BindingSha256"]=binding,
            ["TerminalState"]="TERMINAL_NEGATIVE_PARENT_RETAINED",["QualityDecisionReceipt"]=Ref(qd),["DispositionReceipt"]=Ref(disposition),["FinalReopenReceipt"]=Ref(final) };
        foreach(var (expected,index) in new[] {(qd,3),(disposition,4),(final,5)})
            Assert.Equal(IndependentJcs(expected),IndependentJcs(JsonNode.Parse(frames[index])!["ProgressReceipt"]!));
        Assert.Equal(IndependentJcs(outcome),response.TerminalOutcome!.Value.GetRawText());
        Assert.Equal(IndependentHash(IndependentJcs(outcome)),response.TerminalOutcomeSha256);
        var effects=new JsonObject { ["BaselineDispatched"]="0",["BaselineCompleted"]="0",["TeachingDispatched"]="0",
            ["TeachingCompleted"]="0",["EvaluatorDispatched"]="0",["EvaluatorCompleted"]="0",["OptimizerEntries"]="0",["EffectOutcomeUnknown"]=false };
        var previous=""; var complete=new List<string>();
        for(var i=0;i<7;i++)
        {
            var expected=new JsonObject { ["Kind"]=i==0?"session":i<3?"transition":i<6?"receipt":"terminal",["FormatVersion"]=2,
                ["SessionId"]=session,["BindingV2"]=expectedBinding.DeepClone(),["BindingSha256"]=binding,
                ["Revision"]=i.ToString(CultureInfo.InvariantCulture),["PreviousFrameSha256"]=previous,["Effects"]=effects.DeepClone() };
            if(i is 1 or 2) expected["TransitionEvent"]=i==1?"RunnerStarted":"ProfileTrainingStarted";
            if(i is >=3 and <=5) expected["ProgressReceipt"]=(i==3?qd:i==4?disposition:final).DeepClone();
            if(i==6) expected["TerminalOutcome"]=outcome.DeepClone();
            var literal=IndependentJcs(expected); Assert.Equal(literal,frames[i]); complete.Add(literal); previous=IndependentHash(literal);
        }
        Assert.Equal(Encoding.UTF8.GetBytes(string.Join("\n",complete)+"\n"),File.ReadAllBytes(f.OrdinaryHistory));
        using var fresh=new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(f.RepositoryRoot,EndpointTrainingFixture.DomainSlug,
            EndpointTrainingFixture.DesignPatternSlug,f.ModelIdSlug,f.ModelVersion,f.RuntimeCompatibility).Value!;
        Assert.Equal(p.ManifestSha256,fresh.Manifest.ManifestSha256); Assert.Equal(p.WeightsSha256,fresh.WeightsSha256);
        Assert.Equal(p.WeightsByteLength,fresh.WeightsByteLength);
    }
    private static async Task<(HttpStatusCode Status,string Code)> OrdinaryFailure(WebApplication app,EndpointTrainingFixture f)
    {
        using var client=new HttpClient(); using var response=await client.PostAsJsonAsync(
            new Uri(Program.ResolveStartedLoopbackAddress(app),CodeDocumentationOneLessonPreflightWireContract.EndpointPath),new CodeDocumentationOneLessonPreflightRequestV1(f.OrdinarySelector));
        var raw=await response.Content.ReadAsStringAsync(); using var doc=JsonDocument.Parse(raw);
        return (response.StatusCode,doc.RootElement.GetProperty("errorCode").GetString()!);
    }

    [Fact]
    public async Task OrdinaryV2_CommittedOriginal_ReconstructsExactRequestContext_AfterFreshHost()
    {
        using var fixture=new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        var anchors=await RunStartedFixtureAsync(fixture,null,null,async(f,factory,app)=>
        {
            using var owned=await PrepareOrdinaryPrerequisite(app,f,factory);
            Assert.False(File.Exists(f.OrdinaryHistory)); Assert.Equal(10,factory.Client!.EvaluationCalls);
            return (owned.Baseline.Readback.RequestSha256,owned.Baseline.ExternalReference.ReceiptId,
                owned.Baseline.Context.ObservedAtUtc);
        },false,c=>new OrdinaryEvaluationClient(c,true),disposeFixture:false);
        var raw=File.ReadAllBytes(fixture.OrdinaryBaseline); var ticks=File.GetLastWriteTimeUtc(fixture.OrdinaryBaseline);
        await Task.Delay(30); Assert.True(DateTimeOffset.UtcNow>anchors.ObservedAtUtc);
        await RunStartedFixtureAsync(fixture,null,null,async(f,factory,app)=>
        {
            Assert.Equal("Completed",(await OrdinaryPost(app,f)).Disposition);
            Assert.Equal(0,factory.Client!.EvaluationCalls); Assert.Equal(1,factory.Client.DisposeCalls);
            Assert.Equal(raw,File.ReadAllBytes(f.OrdinaryBaseline)); Assert.Equal(ticks,File.GetLastWriteTimeUtc(f.OrdinaryBaseline));
            var qd=JsonNode.Parse(File.ReadAllLines(f.OrdinaryHistory)[3])!["ProgressReceipt"]!["Payload"]!;
            Assert.Equal(anchors.ReceiptId,qd["ExternalBaselineRef"]!["ReceiptId"]!.GetValue<string>());
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true),disposeFixture:false);
    }

    [Fact]
    public async Task OrdinaryV2_OriginalRequestContext_MismatchOrUncertainClaim_NeverRemeasures()
    {
        var fixture=new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture,null,null,async(f,factory,app)=>
        {
            using(var first=await PrepareOrdinaryPrerequisite(app,f,factory)) { }
            var original=File.ReadAllBytes(f.OrdinaryBaseline);
            var originalClaim=File.ReadAllBytes(f.OrdinaryClaim);
            foreach(var mutate in new Action<JsonNode>[] {
                n=>n["Baseline"]!["Calibration"]!["Calibration"]!["MeasurementNoisePassport"]!["CalibratedAtUtc"]="2020-01-01T00:00:00+01:00",
                n=>n["Baseline"]!["Calibration"]!["RepresentativeAssessment"]!["EvidenceRefs"] = new JsonArray("different-evidence"),
                n=>n["Baseline"]!["ObservedEvaluationCallCount"]=9,
                n=>n["Baseline"]!["Calibration"]!["Calibration"]!["Observations"]![4]!["ObservedEvaluationRequestIds"]![1]=
                    n["Baseline"]!["Calibration"]!["Calibration"]!["Observations"]![0]!["ObservedEvaluationRequestIds"]![0]!.DeepClone() })
            {
                var node=JsonNode.Parse(original)!; mutate(node); var changed=Encoding.UTF8.GetBytes(node.ToJsonString());
                File.WriteAllBytes(f.OrdinaryBaseline,changed);
                var claim=Encoding.UTF8.GetString(originalClaim).Split('\n',StringSplitOptions.RemoveEmptyEntries).Select(x=>JsonNode.Parse(x)!).ToArray();
                claim[1]["BaselineSha256"]=IndependentHash(Encoding.UTF8.GetString(changed)); claim[1]["BaselineByteLength"]=changed.LongLength;
                File.WriteAllText(f.OrdinaryClaim,string.Join("\n",claim.Select(x=>x.ToJsonString()))+"\n",new UTF8Encoding(false));
                var failed=await OrdinaryFailure(app,f); Assert.Equal(HttpStatusCode.Conflict,failed.Status);
                Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode,failed.Code);
                Assert.Equal(0,factory.Client!.EvaluationCalls); Assert.False(File.Exists(f.OrdinaryHistory));
                File.WriteAllBytes(f.OrdinaryBaseline,original); File.WriteAllBytes(f.OrdinaryClaim,originalClaim);
                using var reopened=await PrepareOrdinaryPrerequisite(app,f,factory);
                Assert.Equal(0,factory.Client!.EvaluationCalls);
            }
            foreach(var claim in new[] { originalClaim[..^1],Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(originalClaim).Split('\n')[0]+"\n") })
            {
                File.WriteAllBytes(f.OrdinaryClaim,claim); var refused=await OrdinaryFailure(app,f);
                Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode,refused.Code);
                Assert.Equal(0,factory.Client!.EvaluationCalls); Assert.False(File.Exists(f.OrdinaryHistory));
                File.WriteAllBytes(f.OrdinaryClaim,originalClaim); using var restored=await PrepareOrdinaryPrerequisite(app,f,factory);
                Assert.Equal(0,factory.Client!.EvaluationCalls);
            }
            var differentTokenizer=new BpeTokenizer(); differentTokenizer.Train("different different different tokenizer tokenizer tokenizer material",f.ModelConfiguration.VocabSize+8);
            using(var serialized=new MemoryStream())
            { differentTokenizer.Save(serialized); Assert.NotEqual(File.ReadAllBytes(f.TokenizerPath),serialized.ToArray()); }
            var tokenizerRefusal=await TryPrepareOrdinaryPrerequisite(app,f,factory,c=>c with {Tokenizer=differentTokenizer});
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode,tokenizerRefusal.ErrorCode);
            Assert.Equal(0,factory.Client!.EvaluationCalls); Assert.False(File.Exists(f.OrdinaryHistory));
            using(var restored=await PrepareOrdinaryPrerequisite(app,f,factory)) { Assert.Equal(0,factory.Client!.EvaluationCalls); }
            File.Move(f.OrdinaryBaseline,f.OrdinaryBaseline+".held");
            var uncertain=await OrdinaryFailure(app,f);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode,uncertain.Code);
            Assert.Equal(0,factory.Client!.EvaluationCalls); Assert.False(File.Exists(f.OrdinaryHistory));
            File.Move(f.OrdinaryBaseline+".held",f.OrdinaryBaseline);
            Assert.Equal("Completed",(await OrdinaryPost(app,f)).Disposition); Assert.Equal(0,factory.Client!.EvaluationCalls);
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true));
    }

    private sealed record OrdinaryOwned(CodeDocumentationOneLessonSessionOpen Open,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline Baseline,
        CodeDocumentationBatchFileRunner.CodeDocumentationProfileInputsOpenResult Material,
        RepositoryOwnedModelArtifactRoot Root,CodeDocumentationOneLessonGrantCatalog Catalog) : IDisposable
    { public void Dispose()=>Open.Dispose(); }

    private static async Task<OrdinaryOwned> PrepareOrdinaryPrerequisite(WebApplication app,EndpointTrainingFixture f,EndpointEvaluationClientFactory factory)
    {
        var actual=await TryPrepareOrdinaryPrerequisite(app,f,factory);
        Assert.True(actual.IsSuccess,actual.ErrorCode+":"+actual.ErrorMessage);
        return actual.Value!;
    }

    private static async Task<OperationResult<OrdinaryOwned>> TryPrepareOrdinaryPrerequisite(WebApplication app,EndpointTrainingFixture f,
        EndpointEvaluationClientFactory factory,
        Func<CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext,CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext>? alterContext=null)
    {
        static T Require<T>(OperationResult<T> r) { Assert.True(r.IsSuccess,r.ErrorCode+":"+r.ErrorMessage); return r.Value!; }
        var startup=app.Services.GetRequiredService<ContinuousLearningModelServerStartup>();
        var prepared=Require(startup.PrepareRepositoryOwnedTrainingExecutionContext(EndpointTrainingFixture.DomainSlug,
            app.Services.GetRequiredService<ServerOwnedContinuousLearningFileAccess>()));
        var start=prepared.RepositoryStartRequest; var catalog=startup.OneLessonGrantCatalog; var grant=Require(catalog.Resolve(f.OrdinarySelector));
        var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(f.RepositoryRoot));
        var binding=new CodeDocumentationOneLessonSessionBinding(grant.ObjectiveId,grant.GrantId,grant.GrantVersion,grant.TrainingRunId,
            grant.CandidateSha256,grant.ParentManifestSha256,grant.ParentWeightsSha256,grant.ConfigurationSha256,grant.ExamConditionSha256)
            {FormatVersion=2,DescriptorsV2=grant.DescriptorsV2};
        var open=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,binding,catalog.MaxTeacherCalls,grant));
        try
        {
            var corpus=Require(CodeDocumentationSavedModelAssessmentEndpoint.OpenConfiguredCorpus(f.RepositoryRoot,f.Profile));
            var seeds=Require(CodeDocumentationDpoSeedAuthorityResolver.Resolve(start.DomainSlug,prepared.RootInitializationSeed));
            var request=new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(start.RepositoryRoot,start.DomainSlug,start.DesignPatternSlug,
                start.ModelIdSlug,start.ModelVersion,prepared.ModelConfig,start.RuntimeCompatibility,start.ColdStartPolicy,LoraConfig:prepared.LoraConfig,AdapterSeedAuthority:seeds);
            using var client=Require(factory.Create(startup.TeacherConfiguration));
            var context=new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext(corpus.Corpus,corpus.Manifest,prepared.Tokenizer,client,
                startup.TeacherConfiguration,f.Profile.GenerationSettings,new(grant.DescriptorsV2!.RepeatCount,2),
                CodeDocumentationSavedModelAssessmentCalibrationService.DefaultMaximumDuration,DateTimeOffset.UtcNow,prepared.EvidenceRefs.ToArray());
            if(alterContext is not null) context=alterContext(context);
            var consumed=await open.Session.ConsumeOrdinaryBaselineAsync(catalog,f.OrdinarySelector,request,context,CancellationToken.None);
            if(!consumed.IsSuccess || consumed.Value is null)
            { open.Dispose(); return consumed.MapError<OrdinaryOwned>(); }
            var baseline=consumed.Value;
            var material=Require(app.Services.GetRequiredService<CodeDocumentationBatchFileRunner>().OpenProfileInputsFresh(catalog,f.OrdinarySelector,root));
            return OperationResult<OrdinaryOwned>.Ok(new(open,baseline,material,root,catalog));
        }
        catch { open.Dispose(); throw; }
    }

    [Fact]
    public async Task OrdinaryV2_FullProfileTrain_RejectsForeignMintStaleInputsAndInjectedMetadata()
    {
        var fixture=new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture,null,null,async(f,factory,app)=>
        {
            using var owned=await PrepareOrdinaryPrerequisite(app,f,factory);
            var batch=app.Services.GetRequiredService<CodeDocumentationBatchFileRunner>();
            var runner=app.Services.GetRequiredService<CodeDocumentationTrainingRunner>();
            var foreign=new CodeDocumentationBatchFileRunner();
            var foreignMaterial=foreign.OpenProfileInputsFresh(owned.Catalog,f.OrdinarySelector,owned.Root);
            Assert.True(foreignMaterial.IsSuccess,foreignMaterial.ErrorCode);
            Assert.False(batch.TrainSelectedProfileOnly(foreignMaterial.Value!,owned.Catalog,f.OrdinarySelector,owned.Root,owned.Open.Session).IsSuccess);
            foreach(var path in new[] { f.OrdinaryTraining,Path.Combine(f.Root,"ordinary-eval.jsonl") })
            {
                var before=File.ReadAllBytes(path); var changed=Encoding.UTF8.GetString(before).Replace("documentation 49","documentation 48",StringComparison.Ordinal);
                Assert.NotEqual(before,Encoding.UTF8.GetBytes(changed)); File.WriteAllText(path,changed,new UTF8Encoding(false));
                Assert.False(batch.OpenProfileInputsFresh(owned.Catalog,f.OrdinarySelector,owned.Root).IsSuccess);
                File.WriteAllBytes(path,before); Assert.True(batch.OpenProfileInputsFresh(owned.Catalog,f.OrdinarySelector,owned.Root).IsSuccess);
            }
            var defaultRegistry=OrchestrationRoutingCurriculum.CreateDefaultUnitMetadataRegistry();
            var candidates=CodeDocumentationRoutingFlow.DescribeCandidateUnits();
            var missing=new CodeDocumentationBatchFileRunner(new(new RoutingProfileTrainer(new RoutingUnitMetadataRegistry(
                candidates.Take(2).Select(id=>defaultRegistry.Require(id).Value!)))));
            Assert.False(missing.OpenProfileInputsFresh(owned.Catalog,f.OrdinarySelector,owned.Root).IsSuccess);
            var distinct=new CodeDocumentationBatchFileRunner(new(new RoutingProfileTrainer(new RoutingUnitMetadataRegistry(
                candidates.Select(id=>defaultRegistry.Require(id).Value! with { AllowedInputs=defaultRegistry.Require(id).Value!.AllowedInputs.ToArray() })))));
            Assert.True(distinct.OpenProfileInputsFresh(owned.Catalog,f.OrdinarySelector,owned.Root).IsSuccess);
            Assert.Equal(0,runner.SelectedProfileTrainCalls); Assert.False(File.Exists(f.OrdinaryHistory));
            var completed=owned.Open.Session.CompleteEarlyBaselineSufficient(owned.Baseline,batch,owned.Material,owned.Catalog,f.OrdinarySelector,
                owned.Root,f.RuntimeCompatibility,CancellationToken.None);
            Assert.True(completed.IsSuccess,completed.ErrorCode); Assert.Equal(1,runner.SelectedProfileTrainCalls);
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true));
    }

    private static string[] ReadHeldOrdinaryHistoryForFaultObserver(string path)
    {
        // The session retains RW custody; this observer requests no write access and permits that existing writer.
        using var stream=SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path,FileMode.Open,
            FileAccess.Read,FileShare.ReadWrite,FileOptions.SequentialScan);
        using var reader=new StreamReader(stream,Encoding.UTF8,detectEncodingFromByteOrderMarks:true);
        var lines=new List<string>();
        while(reader.ReadLine() is { } line) lines.Add(line);
        return lines.ToArray();
    }

    private void WriteFaultObserverRuntimeModules()
    {
        foreach(var assembly in new[] { typeof(CodeDocumentationOneLessonSession).Assembly,
            typeof(SecureLocalFileBoundary).Assembly, GetType().Assembly })
            output.WriteLine("ORDINARY_FAULT_MODULE "+JsonSerializer.Serialize(new { path=assembly.Location,
                sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))).ToLowerInvariant(),
                mvid=assembly.ManifestModule.ModuleVersionId.ToString() }));
    }

    [Theory]
    [InlineData("cancel")]
    [InlineData("throw")]
    [InlineData("history-swap")]
    [InlineData("lock-swap")]
    [InlineData("actual-train-refusal")]
    [InlineData("input-content")]
    [InlineData("input-swap")]
    [InlineData("history-swap-during-train")]
    [InlineData("input-swap-during-train")]
    public async Task OrdinaryV2_ProfileStart_Interrupted_AlwaysRecovery(string fault)
    {
        WriteFaultObserverRuntimeModules();
        var fixture=new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        var originalTraining=File.ReadAllBytes(fixture.OrdinaryTraining);
        var registry=OrchestrationRoutingCurriculum.CreateDefaultUnitMetadataRegistry();
        var field=typeof(RoutingUnitMetadataRegistry).GetField("_entries",BindingFlags.Instance|BindingFlags.NonPublic)!;
        var entries=(IReadOnlyDictionary<string,UnitRoutingMetadata>)field.GetValue(registry)!;
        var phaseReads=0; var faulted=false; var startedReadback=false;
        var observedAction=string.Empty; var storageOperation=string.Empty;
        var actualSwap=false; var movedTarget=false; IOException? storageFailure=null;
        var probe=new MetadataReadProbe(entries,() =>
        {
            if(!File.Exists(fixture.OrdinaryHistory)) return;
            var lines=ReadHeldOrdinaryHistoryForFaultObserver(fixture.OrdinaryHistory);
            if(lines.Length!=3 || !lines[2].Contains("ProfileTrainingStarted",StringComparison.Ordinal)) return;
            startedReadback=true; phaseReads++;
            var duringTrain=fault is "actual-train-refusal" or "history-swap-during-train" or "input-swap-during-train";
            var duringRunnerPreparation=fault is "input-content" or "input-swap";
            if(faulted || (duringTrain && phaseReads<7) || (duringRunnerPreparation && phaseReads<4)) return;
            faulted=true;
            output.WriteLine($"ORDINARY_FAULT_OBSERVER fault={fault} phaseReads={phaseReads} startedReadback={startedReadback}");
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(fixture.OrdinaryHistory));
            if(fault=="cancel")
            { observedAction="explicit-cancellation"; throw new OperationCanceledException("actual post-start metadata boundary cancellation"); }
            if(fault=="throw")
            { observedAction="explicit-io-fault"; throw new IOException("actual post-start metadata boundary failure"); }
            if(fault=="actual-train-refusal")
            {
                Assert.True(((Dictionary<string,UnitRoutingMetadata>)entries).Remove(OrchestrationRoutingCurriculum.DocumentationDotNetUnitId));
                observedAction="registry-entry-removed"; return;
            }
            if(fault=="input-content" && OperatingSystem.IsLinux())
            {
                var bytes=File.ReadAllBytes(fixture.OrdinaryTraining);
                var changed=Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(bytes).Replace("documentation 49","documentation 48",StringComparison.Ordinal));
                Assert.Equal(bytes.Length,changed.Length); Assert.NotEqual(bytes,changed);
                // Change bytes on the SAME inode while both reader handles remain held.
                var start=new System.Diagnostics.ProcessStartInfo("/usr/bin/python3") { UseShellExecute=false,RedirectStandardError=true };
                start.ArgumentList.Add("-c");
                start.ArgumentList.Add("import os,sys,base64; f=os.open(sys.argv[1],os.O_WRONLY); b=base64.b64decode(sys.argv[2]); assert os.pwrite(f,b,0)==len(b); os.close(f)");
                start.ArgumentList.Add(fixture.OrdinaryTraining); start.ArgumentList.Add(Convert.ToBase64String(changed));
                using var child=System.Diagnostics.Process.Start(start)!;
                var error=child.StandardError.ReadToEnd(); child.WaitForExit();
                output.WriteLine($"ORDINARY_INPUT_MUTATOR pid={child.Id} exit={child.ExitCode} path={fixture.OrdinaryTraining}");
                Assert.True(child.ExitCode==0,error); Assert.Equal(changed,File.ReadAllBytes(fixture.OrdinaryTraining));
                observedAction="same-file-content-changed";
                return;
            }
            var target=fault is "history-swap" or "history-swap-during-train" ? fixture.OrdinaryHistory :
                fault is "input-content" or "input-swap" or "input-swap-during-train" ? fixture.OrdinaryTraining :
                Path.Combine(Path.GetDirectoryName(fixture.OrdinaryHistory)!,".owner.lock");
            try
            {
                storageOperation="read";
                var same=File.ReadAllBytes(target);
                storageOperation="move"; File.Move(target,target+".old"); movedTarget=true;
                storageOperation="write"; File.WriteAllBytes(target,same);
                if(OperatingSystem.IsLinux()) File.SetUnixFileMode(target,SecureLocalFileBoundary.OwnerFileMode);
                Assert.Equal(same,File.ReadAllBytes(target));
                actualSwap=true; observedAction="path-replaced"; storageOperation="completed";
                output.WriteLine($"ORDINARY_FAULT_STORAGE fault={fault} target={target} operation={storageOperation} actualSwap=true");
            }
            catch(IOException exception)
            {
                // Capture the failing operation and preserve the original error.
                // Final assertions distinguish lawful sharing denial from an
                // unrelated IOException; a started attempt is not a swap.
                storageFailure=exception;
                output.WriteLine($"ORDINARY_FAULT_STORAGE fault={fault} target={target} operation={storageOperation} actualSwap={actualSwap} nativeError={exception.HResult & 0xffff} error={exception}");
                throw;
            }
        });
        // Only the passive metadata dictionary is instrumented. Every value comes from
        // the real injected registry; the real sealed trainer still executes Train.
        field.SetValue(registry,probe);
        await RunStartedFixtureAsync(fixture,null,fault=="throw" ? ()=>throw new InvalidOperationException("ordinary post-start dispose failure") : null,async(f,factory,app)=>
        {
            var failed=await OrdinaryFailure(app,f);
            output.WriteLine($"ORDINARY_FAULT_HTTP fault={fault} status={failed.Status} code={failed.Code} startedReadback={startedReadback} faulted={faulted}");
            Assert.True(startedReadback); Assert.True(faulted);
            if(storageFailure is not null)
            {
                var nativeError=storageFailure.HResult & 0xffff;
                var windowsSharingDenial=OperatingSystem.IsWindows() && nativeError is 32 or 33 &&
                    !movedTarget && storageOperation is "read" or "move";
                var linuxHeldLockDenial=OperatingSystem.IsLinux() && fault=="lock-swap" &&
                    storageOperation=="read" && nativeError==11;
                Assert.True(windowsSharingDenial || linuxHeldLockDenial,storageFailure.ToString());
                Assert.False(actualSwap);
                Assert.False(movedTarget);
                observedAction="held-custody-denial";
            }
            else
                Assert.Equal(fault switch
                {
                    "cancel" => "explicit-cancellation",
                    "throw" => "explicit-io-fault",
                    "actual-train-refusal" => "registry-entry-removed",
                    "input-content" when OperatingSystem.IsLinux() => "same-file-content-changed",
                    _ => "path-replaced"
                },observedAction);
            output.WriteLine($"ORDINARY_FAULT_ACTION fault={fault} action={observedAction} operation={storageOperation} actualSwap={actualSwap}");
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,failed.Code);
            Assert.Equal(3,File.ReadAllLines(f.OrdinaryHistory).Length);
            var runner=app.Services.GetRequiredService<CodeDocumentationTrainingRunner>();
            var expectedTrains=fault is "actual-train-refusal" or "history-swap-during-train" or "input-swap-during-train" ? 1 : 0;
            Assert.Equal(expectedTrains,runner.SelectedProfileTrainCalls);
            Assert.Equal(10,factory.Client!.EvaluationCalls); var creates=factory.CreateCalls;
            Assert.Equal(1,factory.Client.DisposeCalls);
            var retried=await OrdinaryFailure(app,f);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,retried.Code);
            Assert.Equal(creates,factory.CreateCalls); Assert.Equal(10,factory.Client.EvaluationCalls);
            Assert.Equal(expectedTrains,runner.SelectedProfileTrainCalls);
            if(fault is "input-content" or "input-swap" or "input-swap-during-train")
            {
                if(File.Exists(f.OrdinaryTraining+".old"))
                { File.Delete(f.OrdinaryTraining); File.Move(f.OrdinaryTraining+".old",f.OrdinaryTraining); }
                else File.WriteAllBytes(f.OrdinaryTraining,originalTraining);
                Assert.Equal(originalTraining,File.ReadAllBytes(f.OrdinaryTraining));
            }
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true),profileTrainer:new(registry));
    }

    [Fact]
    public async Task OrdinaryV2_HeldCustodyBusy_AndActualAppendFailure_PreserveStartedPrefix()
    {
        WriteFaultObserverRuntimeModules();
        var fixture=new EndpointTrainingFixture(10000,2); fixture.ProvisionOrdinaryV2(5);
        var registry=OrchestrationRoutingCurriculum.CreateDefaultUnitMetadataRegistry();
        var entriesField=typeof(RoutingUnitMetadataRegistry).GetField("_entries",BindingFlags.Instance|BindingFlags.NonPublic)!;
        var entries=(IReadOnlyDictionary<string,UnitRoutingMetadata>)entriesField.GetValue(registry)!;
        var historyField=typeof(CodeDocumentationOneLessonSession).GetField("_v2History",BindingFlags.Instance|BindingFlags.NonPublic)!;
        CodeDocumentationOneLessonSession? held=null; var phaseReads=0; var actualReadOnly=false;
        FileStream? installedReadOnly=null;
        entriesField.SetValue(registry,new MetadataReadProbe(entries,()=>
        {
            if(held is null || !File.Exists(fixture.OrdinaryHistory) || ReadHeldOrdinaryHistoryForFaultObserver(fixture.OrdinaryHistory).Length!=3) return;
            if(++phaseReads!=7) return;
            var prior=(FileStream)historyField.GetValue(held)!;
            Assert.True(prior.CanWrite); prior.Dispose();
            var readOnly=SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(fixture.OrdinaryHistory,FileMode.Open,
                FileAccess.Read,FileShare.Read,FileOptions.SequentialScan);
            Assert.False(readOnly.CanWrite); Assert.True(readOnly.CanRead);
            installedReadOnly=readOnly;
            historyField.SetValue(held,readOnly); actualReadOnly=true;
            Assert.True(held.ReadCapacityPrefixV2().IsSuccess);
        }));
        await RunStartedFixtureAsync(fixture,null,null,async(f,factory,app)=>
        {
            var runner=app.Services.GetRequiredService<CodeDocumentationTrainingRunner>();
            using(var owned=await PrepareOrdinaryPrerequisite(app,f,factory))
            {
                held=owned.Open.Session;
                var creates=factory.CreateCalls;
                var busy=await OrdinaryFailure(app,f);
                Assert.Equal(HttpStatusCode.Conflict,busy.Status); Assert.Equal(CodeDocumentationOneLessonSession.BusyErrorCode,busy.Code);
                Assert.Equal(creates,factory.CreateCalls); Assert.False(File.Exists(f.OrdinaryHistory)); Assert.Equal(0,runner.SelectedProfileTrainCalls);
                var refused=held.CompleteEarlyBaselineSufficient(owned.Baseline,app.Services.GetRequiredService<CodeDocumentationBatchFileRunner>(),
                    owned.Material,owned.Catalog,f.OrdinarySelector,owned.Root,f.RuntimeCompatibility,CancellationToken.None);
                output.WriteLine($"ORDINARY_APPEND_GUARD code={refused.ErrorCode} actualReadOnly={actualReadOnly} phaseReads={phaseReads}");
                Assert.True(actualReadOnly); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,refused.ErrorCode);
                Assert.Same(installedReadOnly,historyField.GetValue(held));
                Assert.False(installedReadOnly!.CanWrite);
                Assert.Equal(1,runner.SelectedProfileTrainCalls); Assert.Equal(50,runner.SelectedProfileTrainExampleCount);
                Assert.Equal(3,File.ReadAllLines(f.OrdinaryHistory).Length);
                Assert.Equal("ProfileTrainingStarted",JsonNode.Parse(File.ReadAllLines(f.OrdinaryHistory)[2])!["TransitionEvent"]!.GetValue<string>());
            }
            held=null; var prefix=File.ReadAllBytes(f.OrdinaryHistory); var factoryCalls=factory.CreateCalls;
            var retry=await OrdinaryFailure(app,f);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,retry.Code);
            Assert.Equal(prefix,File.ReadAllBytes(f.OrdinaryHistory)); Assert.Equal(factoryCalls,factory.CreateCalls);
            Assert.Equal(10,factory.Client!.EvaluationCalls); Assert.Equal(1,runner.SelectedProfileTrainCalls);
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true),profileTrainer:new(registry));
    }

    [Fact]
    public async Task OrdinaryV2_ActualClient_RejectsMutatedGenuineCompletedWire_WithRestoredReplay()
    {
        var fixture=new EndpointTrainingFixture(10000,2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture,null,null,async(f,factory,app)=>
        {
            using var http=new HttpClient(); using var actual=await http.PostAsJsonAsync(
                new Uri(Program.ResolveStartedLoopbackAddress(app),CodeDocumentationOneLessonPreflightWireContract.EndpointPath),
                new CodeDocumentationOneLessonPreflightRequestV1(f.OrdinarySelector));
            Assert.Equal(HttpStatusCode.OK,actual.StatusCode);
            var genuine=await actual.Content.ReadAsStringAsync(); var source=JsonNode.Parse(genuine)!;
            Assert.Equal("Completed",source["disposition"]!.GetValue<string>());
            Assert.Equal("TerminalReplay",(await OrdinaryPost(app,f)).Disposition);
            var history=File.ReadAllBytes(f.OrdinaryHistory); var ticks=File.GetLastWriteTimeUtc(f.OrdinaryHistory);
            var mutations=new Dictionary<string,Action<JsonNode>>
            {
                ["envelope-extra"]=n=>n["unexpected"]=1,
                ["envelope-alias"]=n=>n["SessionId"]=n["sessionId"]!.DeepClone(),
                ["envelope-null"]=n=>n["errorCode"]=null,
                ["revision"]=n=>n["sessionRevision"]=5,
                ["outcome-extra"]=n=>n["terminalOutcome"]!["TrialEvaluationReceipt"]=n["terminalOutcome"]!["QualityDecisionReceipt"]!.DeepClone(),
                ["outcome-alias"]=n=>n["terminalOutcome"]!["schemaVersion"]=2,
                ["outcome-null"]=n=>n["terminalOutcome"]!["ObjectiveId"]=null,
                ["outcome-schema"]=n=>n["terminalOutcome"]!["SchemaVersion"]=1,
                ["outcome-id"]=n=>n["terminalOutcome"]!["OutcomeId"]=new string('0',32),
                ["outcome-state"]=n=>n["terminalOutcome"]!["TerminalState"]="TERMINAL_PUBLICATION_CONFIRMED",
                ["ref-null"]=n=>n["terminalOutcome"]!["FinalReopenReceipt"]=null,
                ["ref-alias"]=n=>n["terminalOutcome"]!["FinalReopenReceipt"]!["receiptId"]=new string('a',32),
                ["ref-extra"]=n=>n["terminalOutcome"]!["FinalReopenReceipt"]!["Revision"]=6,
                ["ref-slot"]=n=>n["terminalOutcome"]!["FinalReopenReceipt"]!["ReceiptId"]=n["terminalOutcome"]!["QualityDecisionReceipt"]!["ReceiptId"]!.DeepClone(),
                ["ref-upper"]=n=>n["terminalOutcome"]!["FinalReopenReceipt"]!["ReceiptSha256"]=new string('A',64),
                ["outcome-hash"]=n=>n["terminalOutcomeSha256"]=new string('0',64),
            };
            var packets=new List<(string Name,string Raw)>();
            foreach(var (name,mutate) in mutations)
            {
                var changed=source.DeepClone(); mutate(changed);
                if(name!="outcome-hash") changed["terminalOutcomeSha256"]=IndependentHash(IndependentJcs(changed["terminalOutcome"]!));
                packets.Add((name,IndependentJcs(changed)));
            }
            var canonical=IndependentJcs(source);
            packets.Add(("envelope-duplicate",canonical.Replace("\"disposition\":\"Completed\"","\"disposition\":\"Completed\",\"disposition\":\"Completed\"",StringComparison.Ordinal)));
            packets.Add(("outcome-duplicate",canonical.Replace("\"SchemaVersion\":2","\"SchemaVersion\":2,\"SchemaVersion\":2",StringComparison.Ordinal)));
            packets.Add(("ref-duplicate",canonical.Replace("\"ReceiptId\":","\"ReceiptId\":\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\",\"ReceiptId\":",StringComparison.Ordinal)));
            packets.Add(("noncanonical-number",canonical.Replace("\"SchemaVersion\":2","\"SchemaVersion\":2.0",StringComparison.Ordinal)));
            packets.Add(("noncanonical-outcome",genuine.Replace("\"OutcomeId\":","\"OutcomeId\" : ",StringComparison.Ordinal)));
            var currentPacket="";
            var builder=WebApplication.CreateBuilder(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            await using var negativeTransport=builder.Build();
            // Transport only altered bytes captured from the actual completed Program response.
            negativeTransport.MapPost(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
                ()=>Results.Text(currentPacket,"application/json"));
            await negativeTransport.StartAsync();
            try
            {
                var client=new CodeDocumentationOneLessonPreflightClient();
                foreach(var (name,raw) in packets)
                {
                    Assert.NotEqual(genuine,raw); currentPacket=raw;
                    var rejected=await client.PreflightAsync(Program.ResolveStartedLoopbackAddress(negativeTransport),f.OrdinarySelector,TimeSpan.FromSeconds(10));
                    Assert.False(rejected.IsSuccess,name); output.WriteLine("ORDINARY_WIRE_REJECTED "+name+" "+rejected.ErrorCode);
                    Assert.Equal("TerminalReplay",(await OrdinaryPost(app,f)).Disposition);
                }
            }
            finally { await negativeTransport.StopAsync(); }
            Assert.Equal(history,File.ReadAllBytes(f.OrdinaryHistory)); Assert.Equal(ticks,File.GetLastWriteTimeUtc(f.OrdinaryHistory));
            Assert.Equal(1,factory.CreateCalls); Assert.Equal(10,factory.Client!.EvaluationCalls);
            Assert.Equal(1,app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true));
    }

    private sealed class MetadataReadProbe(IReadOnlyDictionary<string,UnitRoutingMetadata> actual,Action observed)
        : IReadOnlyDictionary<string,UnitRoutingMetadata>
    {
        public UnitRoutingMetadata this[string key]=>actual[key]; public IEnumerable<string> Keys=>actual.Keys;
        public IEnumerable<UnitRoutingMetadata> Values=>actual.Values; public int Count=>actual.Count;
        public bool ContainsKey(string key)=>actual.ContainsKey(key);
        public bool TryGetValue(string key,out UnitRoutingMetadata value) { observed(); return actual.TryGetValue(key,out value!); }
        public IEnumerator<KeyValuePair<string,UnitRoutingMetadata>> GetEnumerator()=>actual.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
    }

    [Fact]
    public async Task OrdinaryV2_ForeignGenuineOriginal_CannotAuthorizeAnotherHeldSession()
    {
        var fixture=new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        using var foreignFixture=new EndpointTrainingFixture(10000, 2); foreignFixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture,null,null,async(f,factory,app)=>
        {
            using var owned=await PrepareOrdinaryPrerequisite(app,f,factory);
            var foreign=await RunStartedFixtureAsync(foreignFixture,null,null,async(ff,otherFactory,otherApp)=>
            {
                using var other=await PrepareOrdinaryPrerequisite(otherApp,ff,otherFactory);
                Assert.Equal("NO_TRAINING_NEEDED",other.Baseline.Readback.Classification);
                return other.Baseline;
            },false,c=>new OrdinaryEvaluationClient(c,true),disposeFixture:false);
            var batch=app.Services.GetRequiredService<CodeDocumentationBatchFileRunner>();
            var runner=app.Services.GetRequiredService<CodeDocumentationTrainingRunner>();
            var rejected=owned.Open.Session.CompleteEarlyBaselineSufficient(foreign,batch,owned.Material,owned.Catalog,f.OrdinarySelector,
                owned.Root,f.RuntimeCompatibility,CancellationToken.None);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,rejected.ErrorCode);
            Assert.False(File.Exists(f.OrdinaryHistory)); Assert.Equal(0,runner.SelectedProfileTrainCalls);
            var ids=(IList<string>)owned.Baseline.Readback.Baseline.Calibration.Calibration.Observations[^1].ObservedEvaluationRequestIds;
            var genuineId=ids[^1]; ids[^1]="foreign-genuine-request";
            var changed=owned.Open.Session.CompleteEarlyBaselineSufficient(owned.Baseline,batch,owned.Material,owned.Catalog,f.OrdinarySelector,
                owned.Root,f.RuntimeCompatibility,CancellationToken.None);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,changed.ErrorCode);
            Assert.False(File.Exists(f.OrdinaryHistory)); Assert.Equal(0,runner.SelectedProfileTrainCalls);
            ids[^1]=genuineId;
            var completed=owned.Open.Session.CompleteEarlyBaselineSufficient(owned.Baseline,batch,owned.Material,owned.Catalog,f.OrdinarySelector,
                owned.Root,f.RuntimeCompatibility,CancellationToken.None);
            Assert.True(completed.IsSuccess,completed.ErrorCode); Assert.Equal(1,runner.SelectedProfileTrainCalls);
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true));
    }

    [Fact]
    public async Task OrdinaryV2_Completed_RejectsV1Terminal_AndPreservesV1Replay()
    {
        using var services=new ServiceCollection().BuildServiceProvider();
        var catalog=ContinuousLearningModelServerStartup.Create(_=>null).OneLessonGrantCatalog;
        var execute=typeof(CodeDocumentationOneLessonEndpoint).GetMethod("ExecuteAsync",BindingFlags.Static|BindingFlags.NonPublic)!;
        using var cancelled=new CancellationTokenSource(); cancelled.Cancel();
        var response=(Task<IResult>)execute.Invoke(null,[null,catalog,Path.GetTempPath(),services,cancelled.Token])!;
        var invalid=await CaptureAsync(await response);
        Assert.Equal(StatusCodes.Status400BadRequest,invalid.StatusCode);
        Assert.Equal("CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_REQUEST_INVALID",invalid.Json.RootElement.GetProperty("errorCode").GetString());
        var fixture=new EndpointTrainingFixture(10000,2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture,null,null,async(f,factory,app)=>
        {
            var selectedCatalog=app.Services.GetRequiredService<ContinuousLearningModelServerStartup>().OneLessonGrantCatalog;
            var ordinary=(Task<IResult>)execute.Invoke(null,[new CodeDocumentationOneLessonPreflightRequestV1(f.OrdinarySelector),
                selectedCatalog,f.RepositoryRoot,new FactoryReplacingServiceProvider(app.Services,factory),cancelled.Token])!;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async()=> { await ordinary; });
            Assert.Equal(0,factory.CreateCalls); Assert.False(File.Exists(f.OrdinaryHistory)); Assert.False(File.Exists(f.OrdinaryBaseline));
            Assert.Equal(0,app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            return true;
        },false,c=>new OrdinaryEvaluationClient(c,true));
        // Wire-validator regression only; this literal is not a product completion fixture.
        var terminal=JsonSerializer.SerializeToElement(new { OutcomeId=new string('a',32),SchemaVersion=1,ObjectiveId="legacy-objective",
            SessionId="legacy-session",TrainingRunId="legacy-run",BindingSha256=new string('b',64),TerminalState="TERMINAL_NEGATIVE_PARENT_RETAINED",
            TrialEvaluationReceipt=new {ReceiptId="trial",ReceiptSha256=new string('c',64)},QualityDecisionReceipt=new {ReceiptId="quality",ReceiptSha256=new string('d',64)},
            DispositionReceipt=new {ReceiptId="disposition",ReceiptSha256=new string('e',64)},FinalReopenReceipt=new {ReceiptId="final",ReceiptSha256=new string('f',64)} });
        var method=typeof(CodeDocumentationOneLessonPreflightClient).GetMethod("ValidSuccessResponse",BindingFlags.Static|BindingFlags.NonPublic)!;
        var replay=new CodeDocumentationOneLessonPreflightResponseV1(true,"","","TerminalReplay","legacy-objective","legacy-run",new string('b',64),
            "legacy-session",6,terminal,new string('1',64));
        Assert.True((bool)method.Invoke(null,[replay])!);
        Assert.False((bool)method.Invoke(null,[replay with { Disposition="Completed" }])!);
    }

    private static readonly JsonSerializerOptions IndependentJsonOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static string IndependentJcs(JsonNode node) => node switch
    {
        JsonObject o => "{" + string.Join(",", o.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p =>
            JsonSerializer.Serialize(p.Key,IndependentJsonOptions) + ":" + (p.Value is null ? "null" : IndependentJcs(p.Value)))) + "}",
        JsonArray a => "[" + string.Join(",", a.Select(x => x is null ? "null" : IndependentJcs(x))) + "]",
        _ => node.ToJsonString(IndependentJsonOptions),
    };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OrdinaryV2_DiagnosticClassification_ActualHttpNeedsRefusesWithoutExportOrProfile(bool representativeAllPass)
    {
        var fixture = new EndpointTrainingFixture(10000, 2); fixture.ProvisionOrdinaryV2(5);
        await RunStartedFixtureAsync(fixture, null, null, async (f, factory, app) =>
        {
            using var client = new HttpClient();
            using var response = await client.PostAsJsonAsync(new Uri(Program.ResolveStartedLoopbackAddress(app),
                CodeDocumentationOneLessonPreflightWireContract.EndpointPath), new CodeDocumentationOneLessonPreflightRequestV1(f.OrdinarySelector));
            var bytes = await response.Content.ReadAsByteArrayAsync();
            using var body = JsonDocument.Parse(bytes);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            var expected = representativeAllPass ? CodeDocumentationOneLessonSession.UsableDiagnosisMissingErrorCode
                : "ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE";
            Assert.Equal(expected, body.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(new[] { "bindingSha256", "disposition", "errorCode", "errorMessage", "isSuccess", "objectiveId",
                "sessionId", "sessionRevision", "terminalOutcome", "terminalOutcomeSha256", "trainingRunId" },
                body.RootElement.EnumerateObject().Select(p => p.Name).Order());
            var text = Encoding.UTF8.GetString(bytes);
            using var saved = JsonDocument.Parse(File.ReadAllBytes(f.OrdinaryBaseline));
            var baseline = saved.RootElement.GetProperty("Baseline").Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>()!;
            foreach (var item in baseline.Calibration.RepresentativeAssessment.Cases)
                foreach (var secret in new[] { item.CaseId, item.Reason, item.StudentAnswerPreview, item.StudentAnswerSha256 }.Where(s => s.Length > 1))
                    Assert.DoesNotContain(secret, text, StringComparison.Ordinal);
            Assert.DoesNotContain("Categories", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("OriginalReceipt", text, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(f.OrdinaryHistory));
            Assert.Equal(10, factory.Client!.EvaluationCalls);
            Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            return true;
        }, false, c => new OrdinaryEvaluationClient(c, false, n => representativeAllPass && n <= 2));
    }

    private sealed class OrdinaryEvaluationClient(ContinuousLearningTp1DpoConfiguration configuration, bool pass,
        Func<int, bool>? passForCall = null)
        : ILegalTeacherEvaluationInferenceClient
    {
        private int calls;
        public Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(string prompt,
            ILegalTeacherCallPermit permit, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested(); var issued = permit.TryIssue();
            if (!issued.IsSuccess) return Task.FromResult(issued.MapError<TeacherEvaluationInferenceResult>());
            var number = Interlocked.Increment(ref calls);
            var currentPass = passForCall?.Invoke(number) ?? pass;
            using var input = JsonDocument.Parse(prompt[prompt.IndexOf('{')..]);
            var output = JsonSerializer.Serialize(new { caseId = input.RootElement.GetProperty("caseId").GetString(),
                pass = currentPass, reason = currentPass ? "correct" : "missing_essential_behavior" });
            var provenance = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput, OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight, TrainingTeacherHostingProvider.Ollama, configuration.ModelId,
                configuration.ExpectedOllamaManifestDigest, TrainingTeacherModelLicense.Apache20, false, false, false, false,
                "ordinary-evaluation-" + number.ToString("D5", CultureInfo.InvariantCulture), 1)
                { ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                  ModelArtifactDigest = configuration.ExpectedOllamaModelLayerDigest };
            return Task.FromResult(OperationResult<TeacherEvaluationInferenceResult>.Ok(new(output, provenance)
                { InferenceProfile = LocalQwenOllamaInferenceProfileAuthority.Evaluation }));
        }
        public void Dispose() { }
    }
    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_RouteIsMappedAndRejectsInvalidBodyInsteadOf404()
    {
        using var environment = new EnvironmentScope(new Dictionary<string, string?>
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.FalseString,
            [Program.RepositoryOwnedTrainingEnabledEnvironmentName]
                = bool.FalseString,
        });
        var builder = Program.CreateLocalLoopbackBuilder([]);
        await using var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);

        try
        {
            await app.StartAsync();
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath,
                new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public void CodeDocumentationRepositoryOwnedDpoTraining_ReadyFileListsPostRoute()
    {
        var payload = Program.CreateLocalLoopbackReadyFilePayload(
            new Uri("http://127.0.0.1:12345"),
            ContinuousLearningModelServerStartup.Create(_ => null));

        var paths = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            payload["routePaths"]);
        var routes = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            payload["routes"]);

        Assert.Contains(
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath,
            paths);
        Assert.Contains(
            "POST " +
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath,
            routes);
    }

    [Fact]
    public void CodeDocumentationRepositoryOwnedDpoTraining_RequestDtoExcludesCallerControlledTrainingAuthority()
    {
        var propertyNames = typeof(
                CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(static property => property.Name)
            .ToArray();

        Assert.Equal(
            [
                "DomainSlug",
                "DesignPatternSlug",
                "DatasetVersion",
                "TrainingRunId",
                "RoutingRun",
                "ProvenanceRefs"
            ],
            propertyNames);
        foreach (var forbidden in new[]
                 {
                     "RepositoryRoot",
                     "ModelConfig",
                     "Tokenizer",
                     "LoraConfig",
                     "RuntimeCompatibility",
                     "ColdStartPolicy",
                     "DpoStepCount",
                     "LeaseBroker",
                 })
        {
            Assert.DoesNotContain(forbidden, propertyNames);
        }
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_AssessmentProfilesConfigurationRequiredFailsClosedBeforeServerContext()
    {
        var request = CreateValidShapeRequest();
        var services = new ServiceCollection().BuildServiceProvider();

        var result =
            await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteAsync(
                request,
                services,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.StatusCode);
        Assert.False(response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentProfileResolver
                .ConfigurationRequiredErrorCode,
            response.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.DoesNotContain(@"C:\", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/mnt/", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "не получила новую принятую версию",
            response.Json.RootElement.GetProperty("humanMessage").GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_MissingAssessmentProfileFailsClosedBeforeServerContext()
    {
        using var environment = new EnvironmentScope(
            new Dictionary<string, string?>
            {
                [CodeDocumentationSavedModelAssessmentProfileResolver
                    .EnvironmentName] = CreateAssessmentProfileJson(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .AngularDomainSlug),
            });
        var request = CreateValidShapeRequest();
        var services = new ServiceCollection().BuildServiceProvider();

        var result =
            await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteAsync(
                request,
                services,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.StatusCode);
        Assert.False(response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentEndpoint.ProfileMissingErrorCode,
            response.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.DoesNotContain(@"C:\", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/mnt/", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "не получила новую принятую версию",
            response.Json.RootElement.GetProperty("humanMessage").GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_NoServerContextFailsClosedAfterAssessmentProfile()
    {
        using var environment = new EnvironmentScope(
            new Dictionary<string, string?>
            {
                [CodeDocumentationSavedModelAssessmentProfileResolver
                    .EnvironmentName] = CreateAssessmentProfileJson(),
            });
        var request = CreateValidShapeRequest();
        var services = new ServiceCollection().BuildServiceProvider();

        var result =
            await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteAsync(
                request,
                services,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.StatusCode);
        Assert.False(response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_CONTEXT_UNAVAILABLE",
            response.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.DoesNotContain(@"C:\", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/mnt/", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "не получила новую принятую версию",
            response.Json.RootElement.GetProperty("humanMessage").GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_DirectCallRejectsUnsafeProvenanceBeforeServerContext()
    {
        var request = CreateValidShapeRequest() with
        {
            ProvenanceRefs = ["C:\\outside\\teacher-secret.json"],
        };
        var services = new ServiceCollection().BuildServiceProvider();

        var result =
            await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteAsync(
                request,
                services,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .InvalidRequestErrorCode,
            response.Json.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_DirectCallRejectsUnsafeDpoReceiptBeforeServerContext()
    {
        var unsafeReceipt = CreateValidReceipt() with
        {
            RepositoryRelativeManifestPath =
                "/training-data/dpo/csharp-code-documentation/manifest.json",
            OrderedPairs =
            [
                new RepositoryOwnedDpoDatasetManifestPair(
                    "pair-001",
                    "C:\\outside\\pair.json",
                    Hex('d')),
            ],
        };
        var request = CreateValidShapeRequest() with
        {
            RoutingRun = CreateValidShapeRequest().RoutingRun with
            {
                RepositoryOwnedDpoReceipt = unsafeReceipt,
            },
        };
        var services = new ServiceCollection().BuildServiceProvider();

        var result =
            await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteAsync(
                request,
                services,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .InvalidRequestErrorCode,
            response.Json.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_DirectCallRejectsMissingDpoReceiptBeforeServerContext()
    {
        var request = CreateValidShapeRequest() with
        {
            RoutingRun = CreateValidShapeRequest().RoutingRun with
            {
                RepositoryOwnedDpoReceipt = null,
            },
        };
        var services = new ServiceCollection().BuildServiceProvider();

        var result =
            await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteAsync(
                request,
                services,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .InvalidRequestErrorCode,
            response.Json.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_DirectCallRejectsOverlongCoordinatesBeforeServerContext()
    {
        var request = CreateValidShapeRequest() with
        {
            TrainingRunId = new string(
                'r',
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumTrainingRunIdLength + 1),
        };
        var services = new ServiceCollection().BuildServiceProvider();

        var result =
            await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteAsync(
                request,
                services,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .InvalidRequestErrorCode,
            response.Json.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public void CodeDocumentationRepositoryOwnedDpoTraining_DatasetGateRequiresServerCurrentManifest()
    {
        using var repository = new TempRepositoryRoot(
            "code-doc-endpoint-dataset-gate");
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repository.Path,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DefaultDatasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var method = typeof(CodeDocumentationRepositoryOwnedDpoTrainingEndpoint)
            .GetMethod(
                "DatasetMatchesServerCurrentManifest",
                BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var matchingReceipt = new CodeDocumentationRepositoryOwnedDpoDatasetReceipt(
            RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset,
            Hex('a'),
            address.Value!.ManifestRelativePath,
            PairCount: 1,
            Hex('b'),
            []);
        var matchingManifest = new RepositoryOwnedDpoDatasetManifestIdentity(
            address.Value.ManifestRelativePath,
            Hex('a'));

        Assert.True(InvokeDatasetGate(
            method!,
            address.Value,
            matchingReceipt,
            matchingManifest));
        Assert.False(InvokeDatasetGate(
            method!,
            address.Value,
            matchingReceipt with { ManifestSha256 = Hex('c') },
            matchingManifest));
        Assert.False(InvokeDatasetGate(
            method!,
            address.Value,
            matchingReceipt,
            null));
    }

    [Fact]
    public void CodeDocumentationRepositoryOwnedDpoTraining_SuccessWireResponseRequiresResumedParent()
    {
        var nullParent =
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .CreateSuccessResponse(
                    CreatePublicationEvidence(parentStep: null));

        Assert.False(nullParent.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .ParentCheckpointRequiredErrorCode,
            nullParent.ErrorCode);

        var missingQualityDecision =
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .CreateSuccessResponse(
                    CreatePublicationEvidence(
                        parentStep: 1,
                        includeQualityDecision: false));

        Assert.False(missingQualityDecision.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .ParentCheckpointRequiredErrorCode,
            missingQualityDecision.ErrorCode);

        var rejectedQualityDecision =
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .CreateSuccessResponse(
                    CreatePublicationEvidence(
                        parentStep: 1,
                        qualityDecisionAccepted: false));

        Assert.False(rejectedQualityDecision.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .ParentCheckpointRequiredErrorCode,
            rejectedQualityDecision.ErrorCode);

        var resumedParent =
            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                .CreateSuccessResponse(
                    CreatePublicationEvidence(parentStep: 1));

        Assert.True(resumedParent.IsSuccess, resumedParent.ErrorMessage);
        Assert.NotNull(resumedParent.Value);
        Assert.Equal(1, resumedParent.Value!.ParentCheckpointStep);
        Assert.Equal(2, resumedParent.Value.PublishedCheckpointStep);
    }

    [Fact]
    public void CodeDocumentationRepositoryOwnedDpoTraining_PublicSuccessFactoryRequiresAcceptedConsistentQualityDecision()
    {
        var missingQualityDecision =
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse.Success(
                CreatePublicationEvidence(
                    parentStep: 1,
                    includeQualityDecision: false));

        Assert.False(missingQualityDecision.IsSuccess);
        Assert.DoesNotContain(
            "подтвердила улучшение",
            missingQualityDecision.HumanMessage,
            StringComparison.Ordinal);

        var rejectedQualityDecision =
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse.Success(
                CreatePublicationEvidence(
                    parentStep: 1,
                    qualityDecisionAccepted: false));

        Assert.False(rejectedQualityDecision.IsSuccess);
        Assert.DoesNotContain(
            "подтвердила улучшение",
            rejectedQualityDecision.HumanMessage,
            StringComparison.Ordinal);

        var contradictoryQualityDecision =
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse.Success(
                CreatePublicationEvidence(
                    parentStep: 1,
                    qualityDecisionDifference: 0.25));

        Assert.False(contradictoryQualityDecision.IsSuccess);
        Assert.DoesNotContain(
            "подтвердила улучшение",
            contradictoryQualityDecision.HumanMessage,
            StringComparison.Ordinal);

        var acceptedQualityDecision =
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse.Success(
                CreatePublicationEvidence(parentStep: 1));

        Assert.True(acceptedQualityDecision.IsSuccess);
        Assert.Contains(
            "подтвердила улучшение",
            acceptedQualityDecision.HumanMessage,
            StringComparison.Ordinal);
    }

    [Fact]
    public void CodeDocumentationRepositoryOwnedDpoTraining_SourceUsesInjectedBrokerAndDoesNotCreateAuthority()
    {
        var source = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "25.ModelServer",
                "ContinuousLearning",
                "CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.cs"));

        Assert.Contains(
            "services.GetService<IServerOwnedDpoTrainingLeaseBroker>()",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "new ServerOwnedLegalTrainingTeacherAdmissionAuthority",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_CombinedBudgetRejectsBeforeEvaluationFactory()
    {
        var run = await RunHttpAsync(maxTeacherCalls: 19);

        Assert.Equal(StatusCodes.Status409Conflict, run.StatusCode);
        Assert.False(run.Json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .QualityGateBudgetExceededErrorCode,
            run.Json.GetProperty("errorCode").GetString());
        Assert.Equal(0, run.CreateCalls);
        Assert.Equal(0, run.DisposeCalls);
        Assert.Equal(0, run.EvaluationCalls);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_ExactCombinedBudgetReachesEvaluationFactory()
    {
        var run = await RunHttpAsync(
            maxTeacherCalls: 20,
            createErrorCode: "S0304_HTTP_FACTORY_SENTINEL");

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, run.StatusCode);
        Assert.False(run.Json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            "S0304_HTTP_FACTORY_SENTINEL",
            run.Json.GetProperty("errorCode").GetString());
        Assert.Equal(1, run.CreateCalls);
        Assert.Equal(0, run.DisposeCalls);
        Assert.Equal(0, run.EvaluationCalls);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_TrustedRootMismatchRejectsBeforeFactory()
    {
        var run = await RunStartedFixtureAsync(
            maxTeacherCalls: 20,
            createErrorCode: null,
            disposeAction: null,
            async (fixture, factory, app) =>
            {
                var result =
                    await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                        .ExecuteCoreAsync(
                            fixture.Request,
                            new FactoryReplacingServiceProvider(
                                app.Services,
                                factory),
                            DateTimeOffset.UnixEpoch,
                            Path.Combine(fixture.Root, "wrong-expected-root"));
                return new DirectEndpointRun(
                    await CaptureAsync(result),
                    factory.CreateCalls);
            });

        Assert.Equal(StatusCodes.Status409Conflict, run.Captured.StatusCode);
        Assert.False(run.Captured.Json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_START_MISMATCH",
            run.Captured.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(0, run.CreateCalls);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_ProductionWrapperKeepsCanonicalRoot()
    {
        var run = await RunStartedFixtureAsync(
            maxTeacherCalls: 19,
            createErrorCode: null,
            disposeAction: null,
            async (fixture, factory, app) =>
            {
                var result =
                    await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
                        .ExecuteAsync(
                            fixture.Request,
                            new FactoryReplacingServiceProvider(
                                app.Services,
                                factory),
                            DateTimeOffset.UnixEpoch);
                return new DirectEndpointRun(
                    await CaptureAsync(result),
                    factory.CreateCalls,
                    fixture.RepositoryRoot);
            });

        Assert.Equal(StatusCodes.Status409Conflict, run.Captured.StatusCode);
        Assert.False(run.Captured.Json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(0, run.CreateCalls);
        var expectedErrorCode = SameRepositoryRoot(
                run.RepositoryRoot!,
                Program.RepositoryOwnedTrainingTrustedRepositoryRoot)
            ? CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .QualityGateBudgetExceededErrorCode
            : "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_START_MISMATCH";
        Assert.Equal(
            expectedErrorCode,
            run.Captured.Json.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_PrimaryFailurePreservedWhenEvaluationClientDisposeThrows()
    {
        var normal = await RunHttpAsync(maxTeacherCalls: 20);
        Assert.Equal(StatusCodes.Status409Conflict, normal.StatusCode);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .InvalidErrorCode,
            normal.Json.GetProperty("errorCode").GetString());
        Assert.Equal(
            "CodeDocumentation repository-owned DPO training publication needs one sealed DPO dataset, one valid START request, one server-owned training lease broker, UTC publication time, and nonempty provenance.",
            normal.Json.GetProperty("errorMessage").GetString());
        Assert.Equal(
            "Модель документации кода не получила новую принятую версию: ModelServer закрыл операцию до обучения или после пробной попытки без публикации.",
            normal.Json.GetProperty("humanMessage").GetString());
        Assert.Equal(1, normal.CreateCalls);
        Assert.Equal(1, normal.DisposeCalls);
        Assert.Equal(0, normal.EvaluationCalls);

        var throwing = await RunHttpAsync(
            maxTeacherCalls: 20,
            disposeAction: () => throw new InvalidOperationException(
                "S03.04 primary-disposal sentinel"));

        Assert.Equal(normal.StatusCode, throwing.StatusCode);
        Assert.Equal(
            normal.Json.GetProperty("errorCode").GetString(),
            throwing.Json.GetProperty("errorCode").GetString());
        Assert.Equal(
            normal.Json.GetProperty("errorMessage").GetString(),
            throwing.Json.GetProperty("errorMessage").GetString());
        Assert.Equal(
            normal.Json.GetProperty("humanMessage").GetString(),
            throwing.Json.GetProperty("humanMessage").GetString());
        Assert.Equal(1, throwing.CreateCalls);
        Assert.Equal(1, throwing.DisposeCalls);
        Assert.Equal(0, throwing.EvaluationCalls);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_TrainingRouteResumesIncompleteAppliedAttempt()
    {
        var run = await RunPositiveHttpAsync();

        Assert.Equal(StatusCodes.Status200OK, run.StatusCode);
        Assert.True(run.Json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(1, run.CreateCalls);
        Assert.Equal(25, run.EvaluationCalls);
        Assert.False(run.TrainerInvoked);
        Assert.Equal(25, run.ResumedIssuedCallCount);
        Assert.Equal(25, run.ResumedCompletedResponseCount);
        Assert.Equal(25, run.ResumedObservationCount);
        Assert.Equal(50, run.TotalObservationCount);
        Assert.Equal(51, run.TotalAttemptedCallCount);
        Assert.Equal(1, run.ParentCheckpointStep);
        Assert.Equal(2, run.PublishedCheckpointStep);
        var qualityDecision = run.Json.GetProperty("qualityDecision");
        Assert.Equal(50, qualityDecision.GetProperty("expectedEvaluationCallCount").GetInt32());
        Assert.Equal(50, qualityDecision.GetProperty("observedEvaluationCallCount").GetInt32());
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_DiagnosticExposureThenChangedConditionRejectsAndHttpResumesSameAttempt()
    {
        using var runtimeFixture = new TinyTrainingFixtureCL();
        using var repository = new TempRepositoryRoot(
            "code-doc-http-linked-diagnostic-recovery");
        var seedEnvironment = new RecoverySeedEnvironment();
        var scope = "http-linked-diagnostic-" + Guid.NewGuid().ToString("N");
        var prepared = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .PrepareIncompleteAppliedButUnpublishedAttemptForHttp(
                runtimeFixture,
                repository.Path,
                scope,
                seedEnvironment.RootInitializationSeed);

        Assert.Equal(repository.Path, prepared.RepositoryRoot);
        Assert.Equal(1, prepared.ParentManifest.Step);
        Assert.True(File.Exists(prepared.ParentAddress.ManifestPath));
        Assert.False(File.Exists(prepared.ChildAddress.ManifestPath));
        var preparedInputPaths = new[]
        {
            prepared.DpoSetup.Address.ManifestPath,
            Path.Combine(
                prepared.RepositoryRoot,
                prepared.QualityCorpus.Manifest.RepositoryRelativeManifestPath),
            prepared.ParentAddress.ManifestPath,
            prepared.ParentAddress.WeightsPath,
        };
        var preparedInputHashes = preparedInputPaths.ToDictionary(
            path => path,
            path =>
            {
                Assert.True(File.Exists(path),
                    $"Prepared input is absent: {Path.GetFileName(path)}.");
                return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
            },
            StringComparer.Ordinal);

        var diagnosticRunId = "s03-12-linked-diagnostic-" + scope;
        Assert.Empty(SnapshotPersistedAssessmentExposureFiles(repository.Path));
        await RunDiagnosticAssessmentHttpAsync(
            runtimeFixture,
            prepared,
            seedEnvironment,
            diagnosticRunId);
        var exposureSnapshot = SnapshotPersistedAssessmentExposureFiles(
            repository.Path);
        Assert.NotEmpty(exposureSnapshot);
        foreach (var (path, expectedHash) in preparedInputHashes)
        {
            Assert.True(File.Exists(path),
                $"Diagnostic assessment removed prepared input: {Path.GetFileName(path)}.");
            Assert.Equal(
                expectedHash,
                Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));
        }

        var staged = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .StageIncomplete(prepared);
        Assert.Equal(prepared.RepositoryRoot, staged.RepositoryRoot);
        Assert.Equal(prepared.TrainingRunId, staged.TrainingRunId);
        Assert.Equal(prepared.AttemptAddress, staged.AttemptAddress);
        Assert.Equal(prepared.ParentManifest, staged.ParentManifest);
        Assert.False(staged.Staging.Result.IsSuccess);
        Assert.Equal(
            "SCRIPTED_QUALITY_GATE_DECISION_MISSING",
            staged.Staging.Result.ErrorCode);
        Assert.Equal(26, staged.Staging.Evaluator.IssuedCallCount);
        Assert.Equal(25, staged.Staging.Evaluator.CompletedResponseCount);
        Assert.Equal(25, staged.Staging.Evaluator.Observations.Count);
        Assert.Equal(
            1,
            staged.Staging.Evaluator.IssuedCallCount -
                staged.Staging.Evaluator.CompletedResponseCount);
        Assert.False(File.Exists(staged.ChildAddress.WeightsPath));
        Assert.False(File.Exists(staged.ChildAddress.ManifestPath));
        Assert.False(Directory.Exists(staged.ChildAddress.CheckpointRoot));

        var savedBaseline = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .ReadVerifiedResumeBaseline(runtimeFixture, staged);
        var expectedCaseIds = staged.QualityCorpus.Corpus.Entries
            .Where(entry => entry.Partition == FrozenReferencePartition.HeldOut)
            .Select(entry => entry.EntryId)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var baselineObservations = staged.Staging.Evaluator.Observations;
        Assert.Equal(
            expectedCaseIds,
            baselineObservations
                .Select(observation => observation.CaseId)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
        Assert.All(
            baselineObservations.GroupBy(
                observation => observation.CaseId,
                StringComparer.Ordinal),
            group =>
            {
                Assert.Equal(5, group.Count());
                Assert.All(group, observation => Assert.False(observation.Passed));
            });
        Assert.Equal(
            25,
            baselineObservations
                .Select(observation => observation.RequestId)
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.Equal(25, savedBaseline.State.BaselineQuality
            .ObservedEvaluationCallCount);
        AssertAssessmentExposureSnapshotUnchanged(
            exposureSnapshot,
            SnapshotPersistedAssessmentExposureFiles(repository.Path));

        var changedCondition = await RunChangedExamConditionRejectionHttpAsync(
            runtimeFixture,
            staged,
            seedEnvironment,
            scope,
            evaluatorManifestDigestOverride: Hex('e'));
        Assert.Equal(StatusCodes.Status409Conflict, changedCondition.StatusCode);
        Assert.False(changedCondition.Json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .QualityGateConditionMismatchErrorCode,
            changedCondition.Json.GetProperty("errorCode").GetString());
        Assert.Equal(0, changedCondition.EvaluationCalls);
        Assert.Equal(0, changedCondition.ResumedIssuedCallCount);
        Assert.Equal(0, changedCondition.ResumedCompletedResponseCount);
        Assert.Equal(0, changedCondition.ResumedObservationCount);
        Assert.False(changedCondition.TrainerInvoked);
        var stillUnpublished = CodeDocumentationRepositoryOwnedModelCheckpointStartService
            .PrepareStart(staged.StartRequest);
        Assert.True(stillUnpublished.IsSuccess, stillUnpublished.ErrorMessage);
        Assert.Equal(1, stillUnpublished.Value!.ParentManifest!.Step);
        Assert.False(File.Exists(staged.ChildAddress.ManifestPath));
        AssertAssessmentExposureSnapshotUnchanged(
            exposureSnapshot,
            SnapshotPersistedAssessmentExposureFiles(repository.Path));

        var recovered = await RunPositiveHttpAsync(
            runtimeFixture,
            staged,
            seedEnvironment);
        Assert.Equal(StatusCodes.Status200OK, recovered.StatusCode);
        Assert.True(recovered.Json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(25, recovered.ResumedIssuedCallCount);
        Assert.Equal(25, recovered.ResumedCompletedResponseCount);
        Assert.Equal(25, recovered.ResumedObservationCount);
        Assert.Equal(50, recovered.TotalObservationCount);
        Assert.Equal(51, recovered.TotalAttemptedCallCount);
        Assert.Equal(1, recovered.ParentCheckpointStep);
        Assert.Equal(2, recovered.PublishedCheckpointStep);
        Assert.False(recovered.TrainerInvoked);
        Assert.Equal(expectedCaseIds, recovered.ResumedCaseIds
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray());
        Assert.Equal(
            50,
            baselineObservations
                .Select(observation => observation.RequestId)
                .Concat(recovered.ResumedRequestIds)
                .Distinct(StringComparer.Ordinal)
                .Count());
        var reopenedBaselineResult = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
            .OpenExisting(staged.AttemptAddress);
        Assert.True(reopenedBaselineResult.IsSuccess, reopenedBaselineResult.ErrorCode);
        var reopenedBaseline = reopenedBaselineResult.Value!;
        Assert.Equal(savedBaseline.BaselineStateSha256, reopenedBaseline.BaselineStateSha256);
        Assert.Equal(25, reopenedBaseline.State.BaselineQuality
            .ObservedEvaluationCallCount);
        var publishedChild = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(staged.ChildAddress);
        Assert.True(publishedChild.IsSuccess, publishedChild.ErrorCode);
        Assert.Equal(2, publishedChild.Value!.Manifest.Step);
        Assert.Equal(
            staged.ParentManifest.RepositoryRelativeManifestPath,
            publishedChild.Value.Manifest.ParentCheckpoint?.RepositoryRelativeManifestPath);

        var certificationCheckpointPaths = new[]
        {
            staged.ParentAddress.ManifestPath,
            staged.ParentAddress.WeightsPath,
            staged.ChildAddress.ManifestPath,
            staged.ChildAddress.WeightsPath,
        };
        var checkpointHashesBeforeCertification = certificationCheckpointPaths
            .ToDictionary(
                path => path,
                path => Convert.ToHexString(
                    SHA256.HashData(File.ReadAllBytes(path))),
                StringComparer.Ordinal);
        using var certificationFixture = new EndpointTrainingFixture(
            runtimeFixture,
            staged,
            seedEnvironment);
        var certificationProfileEntry = new
        {
            domainSlug = certificationFixture.Profile.DomainSlug,
            designPatternSlug = certificationFixture.Profile.DesignPatternSlug,
            subjectSlug = certificationFixture.SubjectSlug,
            corpusVersion = staged.QualityCorpus.Corpus.CorpusVersion,
            expectedCorpusManifestSha256 =
                staged.QualityCorpus.Manifest.ManifestSha256,
            scopeId = "s03-linked-recovery-boundary-scope",
            scopeVersion = "s03-linked-recovery-boundary-v1",
            coverageRevision = "s03-linked-recovery-coverage-v1",
            coverageCutUtc = DateTimeOffset.UtcNow.ToString("O"),
            ownerSnapshotId = "s03-linked-recovery-owner-snapshot",
            ownerSnapshotVersion = "s03-linked-recovery-owner-v1",
            generation = new
            {
                maxNewTokens = 1,
                temperature = 0f,
                topK = 1,
                topP = 1f,
                seed = 17,
            },
            maxHeldOutCases = 5,
            maxAnswerChars = 256,
        };
        var certificationProfileJson = JsonSerializer.Serialize(
            new[] { certificationProfileEntry });
        {
            using var fixtureEnvironment = certificationFixture.ApplyEnvironment();
            using var certificationProfileEnvironment = new EnvironmentScope(
                new Dictionary<string, string?>
                {
                    [CodeDocumentationCertificationProfileResolver.EnvironmentName]
                        = certificationProfileJson,
                });
            Func<string, string?> readFailClosedCertificationEnvironment = name =>
                name == CodeDocumentationCertificationProfileResolver.EnvironmentName
                    ? certificationProfileJson
                    : certificationFixture.ReadEnvironment(name);
            var authorityProvider =
                new RecordingFailClosedCertificationAuthorityProvider();
            var evaluationFactory = new EndpointEvaluationClientFactory(
                "S03_LINKED_RECOVERY_EVALUATOR_FACTORY_MUST_NOT_RUN",
                disposeAction: null);
            await using var certificationApp = Program.CreateLocalLoopbackHostCore(
                certificationFixture.ReadyFileArguments,
                readFailClosedCertificationEnvironment,
                certificationFixture.RepositoryRoot,
                certificationAuthorityProvider: authorityProvider,
                evaluationClientFactory: evaluationFactory);
            await certificationApp.StartAsync();
            using var certificationClient = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(certificationApp),
            };
            var failClosedCertificationRequestJson = JsonSerializer.Serialize(
                CodeDocumentationCertificationEndpointTests.CreateRequest(
                    "s03-linked-recovery-without-client-seed"));
            Assert.False(failClosedCertificationRequestJson.Contains(
                "\"seed\"",
                StringComparison.OrdinalIgnoreCase));
            using var certificationResponse = await certificationClient.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationEndpointPath,
                new StringContent(
                    failClosedCertificationRequestJson,
                    Encoding.UTF8,
                    "application/json"));
            var certificationResponseBody =
                await certificationResponse.Content.ReadAsStringAsync();

            using var certificationJson = JsonDocument.Parse(
                certificationResponseBody);
            var certificationErrorCode = certificationJson.RootElement
                .TryGetProperty("errorCode", out var errorCodeElement) &&
                errorCodeElement.ValueKind == JsonValueKind.String
                    ? errorCodeElement.GetString()
                    : null;
            Assert.True(
                certificationResponse.StatusCode == HttpStatusCode.Conflict,
                $"Expected Conflict, actual {certificationResponse.StatusCode}; " +
                $"errorCode={certificationErrorCode ?? "<missing>"}");
            Assert.Equal(
                RecordingFailClosedCertificationAuthorityProvider.ErrorCode,
                certificationErrorCode);
            Assert.DoesNotContain(
                RecordingFailClosedCertificationAuthorityProvider.ErrorMessage,
                certificationResponseBody,
                StringComparison.Ordinal);
            Assert.Equal(1, authorityProvider.ResolveCalls);
            Assert.Equal(repository.Path, authorityProvider.TrustedRepositoryRoot);
            Assert.NotNull(authorityProvider.StartRequest);
            Assert.NotNull(authorityProvider.ParentManifest);
            Assert.Equal(2, authorityProvider.ParentManifest!.Step);
            Assert.Equal(
                publishedChild.Value.Manifest.ManifestSha256,
                authorityProvider.ParentManifest.ManifestSha256);
            var expectedSeedAuthority = CodeDocumentationDpoSeedAuthorityResolver
                .Resolve(
                    certificationFixture.Profile.DomainSlug,
                    seedEnvironment.RootInitializationSeed);
            Assert.True(expectedSeedAuthority.IsSuccess, expectedSeedAuthority.ErrorCode);
            Assert.Equal(
                expectedSeedAuthority.Value,
                authorityProvider.StartRequest!.AdapterSeedAuthority);
            Assert.True(
                authorityProvider.StartRequest.LoraConfig is { Dropout: > 0f });
            Assert.Equal(0, evaluationFactory.CreateCalls);
            Assert.Null(evaluationFactory.Client);

            var certificationSessionsRoot = Path.Combine(
                repository.Path,
                "training-artifacts",
                "models",
                CodeDocumentationCertificationSessionStore.DirectoryName,
                "sessions");
            Assert.Empty(Directory.Exists(certificationSessionsRoot)
                ? Directory.EnumerateFiles(
                    certificationSessionsRoot,
                    "completed.json",
                    SearchOption.AllDirectories)
                : Array.Empty<string>());
            foreach (var checkpointPath in certificationCheckpointPaths)
            {
                Assert.Equal(
                    checkpointHashesBeforeCertification[checkpointPath],
                    Convert.ToHexString(
                        SHA256.HashData(File.ReadAllBytes(checkpointPath))));
            }
            AssertAssessmentExposureSnapshotUnchanged(
                exposureSnapshot,
                SnapshotPersistedAssessmentExposureFiles(repository.Path));
        }

        using var cleanFixtureEnvironment = certificationFixture.ApplyEnvironment();
        var cleanCertificationCorpusVersion =
            "s03-linked-cert-" + Guid.NewGuid().ToString("N");
        var cleanCertificationCorpus = SealLinkedRecoveryCleanCertificationCorpus(
            repository.Path,
            certificationFixture.Profile.DomainSlug,
            certificationFixture.SubjectSlug,
            cleanCertificationCorpusVersion);
        var cleanCertificationCoverageCutUtc = DateTimeOffset.UtcNow;
        var cleanCertificationProfileEntry = new
        {
            domainSlug = certificationFixture.Profile.DomainSlug,
            designPatternSlug = certificationFixture.Profile.DesignPatternSlug,
            subjectSlug = certificationFixture.SubjectSlug,
            corpusVersion = cleanCertificationCorpusVersion,
            expectedCorpusManifestSha256 =
                cleanCertificationCorpus.Manifest.ManifestSha256,
            scopeId = "s03-linked-recovery-cert-scope-" +
                Guid.NewGuid().ToString("N"),
            scopeVersion = "s03-linked-recovery-cert-v1",
            coverageRevision = "s03-linked-recovery-cert-coverage-v1",
            coverageCutUtc = cleanCertificationCoverageCutUtc.ToString("O"),
            ownerSnapshotId = "s03-linked-recovery-cert-owner",
            ownerSnapshotVersion = "s03-linked-recovery-cert-owner-v1",
            generation = new
            {
                maxNewTokens = 1,
                temperature = 0f,
                topK = 1,
                topP = 1f,
                seed = 17,
            },
            maxHeldOutCases = 5,
            maxAnswerChars = 256,
        };
        var cleanCertificationProfileJson = JsonSerializer.Serialize(
            new[] { cleanCertificationProfileEntry });
        Func<string, string?> readCleanCertificationProfile = name =>
            name == CodeDocumentationCertificationProfileResolver.EnvironmentName
                ? cleanCertificationProfileJson
                : null;
        var resolvedCertificationProfiles = CodeDocumentationCertificationProfileResolver
            .Resolve(readCleanCertificationProfile);
        Assert.True(
            resolvedCertificationProfiles.IsSuccess,
            resolvedCertificationProfiles.ErrorMessage);
        var cleanCertificationProfile = Assert.Single(
            resolvedCertificationProfiles.Value!.Values);
        var diagnosticEntries = staged.QualityCorpus.Corpus.Entries
            .Where(entry => entry.Partition == FrozenReferencePartition.HeldOut)
            .OrderBy(entry => entry.EntryId, StringComparer.Ordinal)
            .ToArray();
        var persistedDiagnosticExposurePath = Assert.Single(exposureSnapshot.Keys);
        using var persistedDiagnosticExposure = JsonDocument.Parse(
            File.ReadAllText(persistedDiagnosticExposurePath));
        Assert.Equal(
            "code-documentation-material-exposure-envelope-v1",
            persistedDiagnosticExposure.RootElement.GetProperty("schema").GetString());
        var persistedDiagnosticPayload = persistedDiagnosticExposure.RootElement
            .GetProperty("payload");
        Assert.Equal(
            "teacher-visible-diagnostic",
            persistedDiagnosticPayload.GetProperty("role").GetString());
        Assert.Equal(
            staged.QualityCorpus.Manifest.ManifestSha256,
            persistedDiagnosticPayload.GetProperty(
                "diagnosticCorpusManifestSha256").GetString());
        var persistedExposureKeys = persistedDiagnosticPayload
            .GetProperty("materialExposureKeys")
            .EnumerateArray()
            .Select(key => (
                Kind: key.GetProperty("kind").GetString()!,
                Digest: key.GetProperty("digest").GetString()!))
            .ToArray();
        var diagnosticFingerprints = diagnosticEntries
            .Select(entry => entry.SourceEntityFingerprint)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var diagnosticNormalizedInputs = diagnosticEntries
            .Select(entry => FrozenReferenceEntry.ComputeNormalizedTextSha256(
                entry.OwnerInput))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            diagnosticFingerprints,
            persistedExposureKeys
                .Where(key => key.Kind == "source")
                .Select(key => key.Digest)
                .Order(StringComparer.Ordinal)
                .ToArray());
        Assert.Equal(
            diagnosticNormalizedInputs,
            persistedExposureKeys
                .Where(key => key.Kind == "content")
                .Select(key => key.Digest)
                .Order(StringComparer.Ordinal)
                .ToArray());

        var cleanCertificationEntries = cleanCertificationCorpus.Corpus.Entries;
        var certificationFingerprints = cleanCertificationEntries
            .Select(entry => entry.SourceEntityFingerprint)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var certificationNormalizedInputs = cleanCertificationEntries
            .Select(entry => FrozenReferenceEntry.ComputeNormalizedTextSha256(
                entry.OwnerInput))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(2, cleanCertificationEntries.Count);
        var certificationTokenizer = BpeTokenizer.Load(
            certificationFixture.TokenizerPath);
        var selectedMaxNewTokens = cleanCertificationProfile.GenerationSettings
            .MaxNewTokens;
        var selectedContextLength = certificationFixture.ModelConfiguration
            .ContextLen;
        Assert.Equal(1, selectedMaxNewTokens);
        Assert.All(cleanCertificationEntries, entry =>
        {
            var promptTokenCount = certificationTokenizer.Encode(
                entry.OwnerInput).Count;
            var requiredContext =
                (long)promptTokenCount + selectedMaxNewTokens - 1;
            Assert.True(promptTokenCount > 0 &&
                promptTokenCount <= selectedContextLength &&
                requiredContext <= selectedContextLength,
                $"Clean certification case {entry.EntryId} needs {requiredContext} context tokens (prompt={promptTokenCount}, maxNewTokens={selectedMaxNewTokens}); fixture model allows {selectedContextLength}.");
        });
        Assert.All(
            cleanCertificationEntries,
            entry => Assert.Equal(
                FrozenReferencePartition.HeldOut,
                entry.Partition));
        Assert.Empty(certificationFingerprints.Intersect(
            diagnosticFingerprints,
            StringComparer.Ordinal));
        Assert.Empty(certificationNormalizedInputs.Intersect(
            diagnosticNormalizedInputs,
            StringComparer.Ordinal));
        Assert.NotEqual(
            staged.QualityCorpus.Manifest.ManifestSha256,
            cleanCertificationCorpus.Manifest.ManifestSha256);
        Assert.NotEqual(
            staged.QualityCorpus.Corpus.CorpusSha256,
            cleanCertificationCorpus.Corpus.CorpusSha256);
        var provisioningDescriptor = CodeDocumentationCertificationEndpointTests
            .CreateApprovedProvisioningDescriptorForLinkedRecovery(
                repository.Path,
                cleanCertificationProfile,
                cleanCertificationCorpus,
                staged.ParentManifest,
                publishedChild.Value.Manifest,
                prepared.ParentDpoSetup,
                staged.DpoSetup,
                diagnosticEntries);
        using var cleanCertificationEnvironment = new EnvironmentScope(
            new Dictionary<string, string?>
            {
                [CodeDocumentationCertificationProfileResolver.EnvironmentName]
                    = cleanCertificationProfileJson,
                [CodeDocumentationCertificationProvisioning.EnvironmentName]
                    = provisioningDescriptor,
            });
        Func<string, string?> readCertificationEnvironment = name =>
            name == CodeDocumentationCertificationProfileResolver.EnvironmentName
                ? cleanCertificationProfileJson
                : name == CodeDocumentationCertificationProvisioning.EnvironmentName
                    ? provisioningDescriptor
                    : certificationFixture.ReadEnvironment(name);

        var certificationRunId = "s03-linked-recovery-clean-certification";
        var expectedCalibrationRunId =
            "saved-model-calibration:" + certificationRunId;
        var certificationRequestJson = JsonSerializer.Serialize(
            CodeDocumentationCertificationEndpointTests.CreateRequest(
                certificationRunId));
        Assert.False(certificationRequestJson.Contains(
            "\"seed\"",
            StringComparison.OrdinalIgnoreCase));
        var expectedCertificationCaseIds = cleanCertificationEntries
            .Select(entry => entry.EntryId)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expectedRepeatCount = MeasurementNoisePolicyConfig.Canonical.RepeatCount;
        Assert.Equal(5, expectedRepeatCount);
        var expectedEvaluationCalls =
            expectedCertificationCaseIds.Length * expectedRepeatCount;
        Assert.Equal(10, expectedEvaluationCalls);
        var certificationFactory = new CodeDocumentationCertificationEndpointTests
            .RecordingCertificationClientFactory(pass: true);
        ICodeDocumentationCertificationAuthorityProvider firstCertificationProvider;
        string certificationReceiptIdentity;
        DateTimeOffset certificationMeasurementCalibratedAtUtc;
        string completedReceiptPath;
        string completedReceiptHash;
        var firstCertificationApp = Program.CreateLocalLoopbackHostCore(
            certificationFixture.ReadyFileArguments,
            readCertificationEnvironment,
            repository.Path,
            evaluationClientFactory: certificationFactory);
        try
        {
            firstCertificationProvider = firstCertificationApp.Services
                .GetRequiredService<ICodeDocumentationCertificationAuthorityProvider>();
            Assert.IsType<
                ServerOwnedCodeDocumentationCertificationAuthorityProvider>(
                    firstCertificationProvider);
            await firstCertificationApp.StartAsync();
            using var certificationClient = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(
                    firstCertificationApp),
            };
            using var response = await certificationClient.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationEndpointPath,
                new StringContent(
                    certificationRequestJson,
                    Encoding.UTF8,
                    "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();
            using var responseJson = JsonDocument.Parse(responseBody);
            var reportedErrorCode = responseJson.RootElement.TryGetProperty(
                "errorCode", out var errorCodeElement) &&
                errorCodeElement.ValueKind == JsonValueKind.String
                    ? errorCodeElement.GetString()
                    : null;
            var safeErrorCode = reportedErrorCode is { Length: > 0 and <= 128 } &&
                reportedErrorCode.All(character =>
                    (character >= 'A' && character <= 'Z') ||
                    (character >= '0' && character <= '9') ||
                    character == '_')
                    ? reportedErrorCode
                    : "<missing-or-invalid>";
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Certification HTTP {(int)response.StatusCode}; errorCode={safeErrorCode}.");
            Assert.True(responseJson.RootElement.GetProperty("isSuccess")
                .GetBoolean());
            var certification = responseJson.RootElement
                .GetProperty("certification");
            Assert.True(certification.GetProperty("certified").GetBoolean());
            Assert.False(certification.GetProperty("replayed").GetBoolean());
            certificationReceiptIdentity = certification
                .GetProperty("receiptIdentitySha256").GetString()!;
            certificationMeasurementCalibratedAtUtc = certification
                .GetProperty("measurementCalibratedAtUtc").GetDateTimeOffset();
            Assert.False(string.IsNullOrWhiteSpace(certificationReceiptIdentity));
            Assert.Equal(expectedCalibrationRunId, certification
                .GetProperty("certificationRunId").GetString());
            Assert.Equal(cleanCertificationCorpus.Manifest.ManifestSha256,
                certification.GetProperty("corpusManifestSha256").GetString());
            Assert.Equal(expectedCertificationCaseIds.Length,
                certification.GetProperty("heldOutCaseCount").GetInt32());
            Assert.Equal(expectedRepeatCount,
                certification.GetProperty("repeatCount").GetInt32());
            Assert.Equal(expectedEvaluationCalls,
                certification.GetProperty("expectedEvaluationCallCount").GetInt32());
            Assert.Equal(expectedEvaluationCalls,
                certification.GetProperty("observedEvaluationCallCount").GetInt32());
            Assert.Equal(
                CodeDocumentationCertificationAdmission.TargetCorrectFraction,
                certification.GetProperty("targetCorrectFraction").GetDouble());
            Assert.Equal(
                TimeSpan.Zero,
                certificationMeasurementCalibratedAtUtc.Offset);
            Assert.True(certificationMeasurementCalibratedAtUtc <=
                DateTimeOffset.UtcNow);
            Assert.Equal(1, certificationFactory.CreateCalls);
            var firstCertificationClient = Assert.IsType<
                CodeDocumentationCertificationEndpointTests.RecordingCertificationClient>(
                    certificationFactory.Client);
            Assert.Equal(expectedEvaluationCalls,
                firstCertificationClient.IssuedCallCount);
            Assert.Equal(expectedEvaluationCalls,
                firstCertificationClient.CompletedResponseCount);
            Assert.Equal(1, firstCertificationClient.DisposeCalls);
            Assert.Equal(expectedEvaluationCalls,
                firstCertificationClient.Observations.Count);
            Assert.Equal(
                expectedCertificationCaseIds,
                firstCertificationClient.Observations
                    .Select(observation => observation.CaseId)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());
            Assert.All(
                firstCertificationClient.Observations.GroupBy(
                    observation => observation.CaseId,
                    StringComparer.Ordinal),
                group => Assert.Equal(expectedRepeatCount, group.Count()));

            Assert.False(responseBody.Contains(
                repository.Path,
                StringComparison.OrdinalIgnoreCase));
            Assert.False(responseBody.Contains(
                "closedReferenceAnswer",
                StringComparison.Ordinal));
            foreach (var secret in cleanCertificationEntries
                         .SelectMany(entry => new[]
                         {
                             entry.OwnerInput,
                             entry.PaidModelReferenceAnswer,
                         })
                         .Concat(diagnosticEntries.SelectMany(entry => new[]
                         {
                             entry.OwnerInput,
                             entry.PaidModelReferenceAnswer,
                         })))
            {
                Assert.False(responseBody.Contains(secret, StringComparison.Ordinal));
                Assert.False(responseBody.Contains(
                    JsonSerializer.Serialize(secret)[1..^1],
                    StringComparison.Ordinal));
            }

            completedReceiptPath = Assert.Single(
                CompletedCertificationSessionFiles(repository.Path));
            completedReceiptHash = Convert.ToHexString(SHA256.HashData(
                File.ReadAllBytes(completedReceiptPath)));
            Assert.Equal("sha256:" + completedReceiptHash.ToLowerInvariant(),
                certificationReceiptIdentity);
            using var completedJson = JsonDocument.Parse(
                File.ReadAllText(completedReceiptPath));
            Assert.Equal(
                "code-documentation-certification-session-envelope-v1",
                completedJson.RootElement.GetProperty("schema").GetString());
            var completedPayload = completedJson.RootElement.GetProperty("payload");
            Assert.Equal("completed", completedPayload.GetProperty("kind").GetString());
            var durableReceipt = completedPayload.GetProperty("receipt");
            Assert.Equal(expectedCertificationCaseIds.Length,
                durableReceipt.GetProperty("heldOutCaseCount").GetInt32());
            Assert.Equal(expectedRepeatCount,
                durableReceipt.GetProperty("repeatCount").GetInt32());
            Assert.Equal(expectedEvaluationCalls,
                durableReceipt.GetProperty("expectedEvaluationCallCount").GetInt32());
            Assert.Equal(expectedEvaluationCalls,
                durableReceipt.GetProperty("observedEvaluationCallCount").GetInt32());
            Assert.Equal(
                MeasurementNoisePolicyConfig.Canonical.IdentitySha256,
                durableReceipt.GetProperty("noisePolicyIdentitySha256").GetString());
            Assert.Equal(
                MeasurementNoisePolicyConfig.Canonical.StandardDeviationMultiplier,
                durableReceipt.GetProperty("measurementNoisePassport")
                    .GetProperty("standardDeviationMultiplier").GetDouble());
            Assert.Equal(
                certificationMeasurementCalibratedAtUtc,
                durableReceipt.GetProperty("measurementNoisePassport")
                    .GetProperty("calibratedAtUtc").GetDateTimeOffset());
            Assert.Equal(
                expectedCalibrationRunId,
                durableReceipt.GetProperty("calibrationRunId").GetString());
            var durableCaseIds = durableReceipt.GetProperty("representative")
                .GetProperty("cases")
                .EnumerateArray()
                .Select(item => item.GetProperty("caseId").GetString()!)
                .Order(StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(expectedCertificationCaseIds, durableCaseIds);
            var representativeRequestIds = durableReceipt
                .GetProperty("representative")
                .GetProperty("evaluator")
                .GetProperty("observedEvaluationRequestIds")
                .EnumerateArray()
                .Select(id => id.GetString()!)
                .ToArray();
            Assert.Equal(expectedCertificationCaseIds.Length,
                representativeRequestIds.Length);
            Assert.Equal(expectedCertificationCaseIds.Length,
                representativeRequestIds.Distinct(StringComparer.Ordinal).Count());
            var persistedObservations = durableReceipt.GetProperty("observations")
                .EnumerateArray()
                .ToArray();
            Assert.Equal(expectedRepeatCount, persistedObservations.Length);
            Assert.Equal(representativeRequestIds,
                persistedObservations[0]
                    .GetProperty("observedEvaluationRequestIds")
                    .EnumerateArray()
                    .Select(id => id.GetString()!)
                    .ToArray());
            var observationRequestIds = new List<string>(expectedEvaluationCalls);
            foreach (var observation in persistedObservations)
            {
                Assert.Equal(expectedCertificationCaseIds,
                    observation.GetProperty("caseIds")
                        .EnumerateArray()
                        .Select(id => id.GetString()!)
                        .Order(StringComparer.Ordinal)
                        .ToArray());
                var observedIds = observation
                    .GetProperty("observedEvaluationRequestIds")
                    .EnumerateArray()
                    .Select(id => id.GetString()!)
                    .ToArray();
                observationRequestIds.AddRange(observedIds);
                Assert.Equal(expectedCertificationCaseIds.Length, observedIds.Length);
                Assert.Equal(expectedCertificationCaseIds.Length,
                    observedIds.Distinct(StringComparer.Ordinal).Count());
            }
            Assert.Equal(expectedEvaluationCalls, observationRequestIds.Count);
            Assert.Equal(expectedEvaluationCalls,
                observationRequestIds.Distinct(StringComparer.Ordinal).Count());
            foreach (var checkpointPath in certificationCheckpointPaths)
            {
                Assert.Equal(
                    checkpointHashesBeforeCertification[checkpointPath],
                    Convert.ToHexString(SHA256.HashData(
                        File.ReadAllBytes(checkpointPath))));
            }
            AssertAssessmentExposureSnapshotUnchanged(
                exposureSnapshot,
                SnapshotPersistedAssessmentExposureFiles(repository.Path));
        }
        finally
        {
            await firstCertificationApp.DisposeAsync();
        }

        var replayCertificationFactory = new CodeDocumentationCertificationEndpointTests
            .RecordingCertificationClientFactory(pass: true);
        var replayCertificationApp = Program.CreateLocalLoopbackHostCore(
            certificationFixture.ReadyFileArguments,
            readCertificationEnvironment,
            repository.Path,
            evaluationClientFactory: replayCertificationFactory);
        try
        {
            var replayCertificationProvider = replayCertificationApp.Services
                .GetRequiredService<ICodeDocumentationCertificationAuthorityProvider>();
            Assert.IsType<
                ServerOwnedCodeDocumentationCertificationAuthorityProvider>(
                    replayCertificationProvider);
            Assert.NotSame(firstCertificationProvider, replayCertificationProvider);
            await replayCertificationApp.StartAsync();
            using var replayCertificationClient = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(
                    replayCertificationApp),
            };
            using var replayResponse = await replayCertificationClient.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationEndpointPath,
                new StringContent(
                    certificationRequestJson,
                    Encoding.UTF8,
                    "application/json"));
            var replayBody = await replayResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, replayResponse.StatusCode);
            using var replayJson = JsonDocument.Parse(replayBody);
            var replayCertification = replayJson.RootElement
                .GetProperty("certification");
            Assert.True(replayCertification.GetProperty("certified").GetBoolean());
            Assert.True(replayCertification.GetProperty("replayed").GetBoolean());
            Assert.Equal(expectedCalibrationRunId,
                replayCertification.GetProperty("certificationRunId").GetString());
            Assert.Equal(
                certificationReceiptIdentity,
                replayCertification.GetProperty("receiptIdentitySha256").GetString());
            Assert.Equal(
                certificationMeasurementCalibratedAtUtc,
                replayCertification.GetProperty("measurementCalibratedAtUtc")
                    .GetDateTimeOffset());
            Assert.Equal(0, replayCertificationFactory.CreateCalls);
            Assert.Null(replayCertificationFactory.Client);
            Assert.False(replayBody.Contains(
                repository.Path,
                StringComparison.OrdinalIgnoreCase));
            Assert.False(replayBody.Contains(
                "closedReferenceAnswer",
                StringComparison.Ordinal));
            foreach (var secret in cleanCertificationEntries
                         .SelectMany(entry => new[]
                         {
                             entry.OwnerInput,
                             entry.PaidModelReferenceAnswer,
                         })
                         .Concat(diagnosticEntries.SelectMany(entry => new[]
                         {
                             entry.OwnerInput,
                             entry.PaidModelReferenceAnswer,
                         })))
            {
                Assert.False(replayBody.Contains(secret, StringComparison.Ordinal));
                Assert.False(replayBody.Contains(
                    JsonSerializer.Serialize(secret)[1..^1],
                    StringComparison.Ordinal));
            }
            Assert.True(certificationMeasurementCalibratedAtUtc <
                DateTimeOffset.UtcNow);
            Assert.Equal(completedReceiptHash,
                Convert.ToHexString(SHA256.HashData(
                    File.ReadAllBytes(completedReceiptPath))));
            Assert.Single(CompletedCertificationSessionFiles(repository.Path));
            foreach (var checkpointPath in certificationCheckpointPaths)
            {
                Assert.Equal(
                    checkpointHashesBeforeCertification[checkpointPath],
                    Convert.ToHexString(SHA256.HashData(
                        File.ReadAllBytes(checkpointPath))));
            }
            AssertAssessmentExposureSnapshotUnchanged(
                exposureSnapshot,
                SnapshotPersistedAssessmentExposureFiles(repository.Path));
        }
        finally
        {
            await replayCertificationApp.DisposeAsync();
        }
    }

    private static async Task RunDiagnosticAssessmentHttpAsync(
        TinyTrainingFixtureCL runtimeFixture,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .PreparedAppliedButUnpublishedAttempt prepared,
        RecoverySeedEnvironment seedEnvironment,
        string assessmentRunId)
    {
        using var fixture = new EndpointTrainingFixture(
            runtimeFixture,
            prepared,
            seedEnvironment);
        using var environment = fixture.ApplyEnvironment();
        var profileEnvironmentName =
            CodeDocumentationSavedModelAssessmentProfileResolver.EnvironmentName;
        Assert.Equal(fixture.ProfileJson, fixture.ReadEnvironment(profileEnvironmentName));
        Assert.Equal(
            fixture.ProfileJson,
            Environment.GetEnvironmentVariable(profileEnvironmentName));

        var factory = new CodeDocumentationCertificationEndpointTests
            .RecordingCertificationClientFactory(pass: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            evaluationClientFactory: factory);
        await app.StartAsync();

        using var client = new HttpClient
        {
            BaseAddress = Program.ResolveStartedLoopbackAddress(app),
        };
        using var response = await client.PostAsync(
            CodeDocumentationSavedModelAssessmentEndpoint.EndpointPath,
            JsonContent.Create(new
            {
                domainSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                designPatternSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                assessmentRunId,
                provenanceRefs = new[] { "test:s03.12-linked-diagnostic" },
            }));
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var errorCode = json.RootElement.TryGetProperty("errorCode", out var errorProperty) &&
            errorProperty.ValueKind == JsonValueKind.String
            ? errorProperty.GetString()
            : null;
        var safeErrorCode = errorCode is { Length: > 0 and <= 128 } &&
            errorCode.All(static value => value is
                (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_')
            ? errorCode
            : "MISSING_OR_INVALID";
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Diagnostic HTTP status {(int)response.StatusCode}; errorCode={safeErrorCode}.");
        Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.False(
            body.Contains(fixture.RepositoryRoot, StringComparison.OrdinalIgnoreCase),
            "Diagnostic HTTP response disclosed a repository path.");
        Assert.False(
            body.Contains("closedReferenceAnswer", StringComparison.Ordinal),
            "Diagnostic HTTP response disclosed a closed reference field.");
        var heldOutAnswers = prepared.QualityCorpus.Corpus.Entries
            .Where(entry => entry.Partition == FrozenReferencePartition.HeldOut)
            .Select(entry => entry.PaidModelReferenceAnswer)
            .ToArray();
        Assert.NotEmpty(heldOutAnswers);
        foreach (var answer in heldOutAnswers)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(answer),
                "The fixture has an empty held-out reference answer.");
            Assert.False(
                body.Contains(answer, StringComparison.Ordinal),
                "Diagnostic HTTP response disclosed a held-out reference answer.");
            var escapedAnswer = JsonSerializer.Serialize(answer)[1..^1];
            Assert.False(
                body.Contains(escapedAnswer, StringComparison.Ordinal),
                "Diagnostic HTTP response disclosed an escaped held-out reference answer.");
        }
        Assert.Equal(1, factory.CreateCalls);
        var evaluator = Assert.IsType<
            CodeDocumentationCertificationEndpointTests.RecordingCertificationClient>(
                factory.Client);
        Assert.True(evaluator.IssuedCallCount > 0);
        Assert.Equal(evaluator.IssuedCallCount, evaluator.Observations.Count);
    }

    private static Dictionary<string, string>
        SnapshotPersistedAssessmentExposureFiles(string repositoryRoot)
    {
        var exposureRoot = Path.Combine(
            repositoryRoot,
            "training-artifacts",
            "models",
            CodeDocumentationCertificationSessionStore.DirectoryName,
            "exposures");
        return Directory.Exists(exposureRoot)
            ? Directory
                .EnumerateFiles(
                    exposureRoot,
                    "exposure.json",
                    SearchOption.AllDirectories)
                .ToDictionary(
                    path => path,
                    path => Convert.ToHexString(
                        SHA256.HashData(File.ReadAllBytes(path))),
                    StringComparer.Ordinal)
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static void AssertAssessmentExposureSnapshotUnchanged(
        IReadOnlyDictionary<string, string> expected,
        IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(
            expected.Keys.Order(StringComparer.Ordinal),
            actual.Keys.Order(StringComparer.Ordinal));
        foreach (var exposure in expected)
            Assert.Equal(exposure.Value, actual[exposure.Key]);
    }

    private static RepositoryOwnedReferenceCorpusOpenResult
        SealLinkedRecoveryCleanCertificationCorpus(
            string repositoryRoot,
            string domainSlug,
            string subjectSlug,
            string corpusVersion)
    {
        var createdAtUtc = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            CreateEntry("catalog", "Catalog"),
            CreateEntry("normalize", "Normalize"),
        };
        var corpus = FrozenReferenceCorpus.Build(
            "s03-linked-recovery-clean-certification-corpus",
            corpusVersion,
            createdAtUtc,
            entries);
        Assert.True(corpus.IsSuccess, corpus.ErrorMessage);
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                repositoryRoot,
                domainSlug,
                subjectSlug,
                corpusVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var sealedCorpus = RepositoryOwnedReferenceCorpusManifestSealer
            .PersistAndSeal(address.Value!, corpus.Value!);
        Assert.True(sealedCorpus.IsSuccess, sealedCorpus.ErrorMessage);
        var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(
            address.Value!);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        return opened.Value!;

        FrozenReferenceEntry CreateEntry(string caseSlug, string operation)
        {
            var ownerInput = caseSlug switch
            {
                "catalog" => "Catalog",
                "normalize" => "Norm",
                _ => throw new ArgumentOutOfRangeException(nameof(caseSlug)),
            };
            var referenceAnswer =
                $"S03_LINKED_RECOVERY_CLEAN_CERT_REFERENCE_{corpusVersion}_{caseSlug}: {operation} returns the synthetic catalog result without mutating its input.";
            var provenance = PaidReferenceProvenance.Create(
                "s03-linked-recovery-synthetic-reference-provider",
                "s03-linked-recovery-synthetic-closed-documentation-reference",
                createdAtUtc.ToString("yyyy-MM-dd"),
                $"s03-linked-recovery-reference-request-{corpusVersion}-{caseSlug}",
                createdAtUtc,
                FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer));
            Assert.True(provenance.IsSuccess, provenance.ErrorMessage);
            var sourceFingerprint = "sha256:" + Convert.ToHexString(
                    SHA256.HashData(Encoding.UTF8.GetBytes(
                        $"s03-linked-recovery-clean-source-{corpusVersion}-{caseSlug}")))
                .ToLowerInvariant();
            var entry = FrozenReferenceEntry.Create(
                ownerInput,
                referenceAnswer,
                provenance.Value!,
                domainSlug,
                subjectSlug,
                FrozenReferencePartition.HeldOut,
                sourceFingerprint);
            Assert.True(entry.IsSuccess, entry.ErrorMessage);
            return entry.Value!;
        }
    }

    private static IReadOnlyList<string> CompletedCertificationSessionFiles(
        string repositoryRoot)
    {
        var sessionsRoot = Path.Combine(
            repositoryRoot,
            "training-artifacts",
            "models",
            CodeDocumentationCertificationSessionStore.DirectoryName,
            "sessions");
        return Directory.Exists(sessionsRoot)
            ? Directory.EnumerateFiles(
                sessionsRoot,
                "completed.json",
                SearchOption.AllDirectories).ToArray()
            : [];
    }

    private static void AssertNonemptyRecoveryRequestWebJsonRoundTrip(
        CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest request,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .StagedResumeAttempt staged)
    {
        var expectedPair = Assert.Single(request.RoutingRun.RepositoryOwnedDpoPairs);
        var expectedReceipt = Assert.IsType<CodeDocumentationRepositoryOwnedDpoDatasetReceipt>(
            request.RoutingRun.RepositoryOwnedDpoReceipt);
        Assert.Equal(staged.DpoSetup.Pairs[0], expectedPair);

        var webJson = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var serialized = JsonSerializer.Serialize(request, webJson);
        var rebound = Assert.IsType<CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest>(
            JsonSerializer.Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest>(
                serialized, webJson));
        Assert.Equal(request.TrainingRunId, rebound.TrainingRunId);
        var actualPair = Assert.Single(rebound.RoutingRun.RepositoryOwnedDpoPairs);
        Assert.Equal(expectedPair.Prompt, actualPair.Prompt);
        Assert.Equal(expectedPair.Chosen, actualPair.Chosen);
        Assert.Equal(expectedPair.Rejected, actualPair.Rejected);
        Assert.Equal(expectedPair.Domain, actualPair.Domain);
        Assert.Equal(expectedPair.CapturedAt, actualPair.CapturedAt);

        var expectedGovernance = Assert.IsType<TrainingDataGovernanceDecision>(
            expectedPair.GovernanceDecision);
        var actualGovernance = Assert.IsType<TrainingDataGovernanceDecision>(
            actualPair.GovernanceDecision);
        Assert.True(actualGovernance.LicenseAccepted);
        Assert.False(actualGovernance.IsQuarantined);
        Assert.Equal(expectedGovernance.LicenseAccepted, actualGovernance.LicenseAccepted);
        Assert.Equal(expectedGovernance.IsQuarantined, actualGovernance.IsQuarantined);
        Assert.NotEmpty(actualGovernance.EvidenceRefs);
        Assert.Equal(expectedGovernance.EvidenceRefs, actualGovernance.EvidenceRefs);

        var expectedTeacher = Assert.IsType<TrainingTeacherProvenance>(
            expectedPair.TeacherProvenance);
        var actualTeacher = Assert.IsType<TrainingTeacherProvenance>(
            actualPair.TeacherProvenance);
        Assert.Equal(expectedTeacher, actualTeacher);
        Assert.NotEmpty(actualTeacher.TrainingContentSha256);
        var reboundBinding = DpoPairTrainingContentAuthority.Bind(actualPair);
        Assert.True(reboundBinding.IsSuccess, reboundBinding.ErrorMessage);
        Assert.Equal(actualTeacher.TrainingContentSha256,
            reboundBinding.Value!.TeacherProvenance!.TrainingContentSha256);
        Assert.Equal(expectedPair.ChosenAuthorship, actualPair.ChosenAuthorship);
        Assert.Equal(expectedPair.RejectedAuthorship, actualPair.RejectedAuthorship);
        Assert.Equal(DpoPairSideAuthorKind.OpenTeacher,
            actualPair.ChosenAuthorship!.AuthorKind);
        Assert.Equal(actualTeacher, actualPair.ChosenAuthorship.OpenTeacherPassport);
        Assert.Equal(DpoPairSideAuthorKind.LocalModel,
            actualPair.RejectedAuthorship!.AuthorKind);
        Assert.Null(actualPair.RejectedAuthorship.OpenTeacherPassport);

        var actualReceipt = Assert.IsType<CodeDocumentationRepositoryOwnedDpoDatasetReceipt>(
            rebound.RoutingRun.RepositoryOwnedDpoReceipt);
        Assert.Equal(expectedReceipt.Source, actualReceipt.Source);
        Assert.Equal(expectedReceipt.ManifestSha256, actualReceipt.ManifestSha256);
        Assert.Equal(expectedReceipt.RepositoryRelativeManifestPath,
            actualReceipt.RepositoryRelativeManifestPath);
        Assert.Equal(expectedReceipt.PairCount, actualReceipt.PairCount);
        Assert.Equal(expectedReceipt.RequestIdentitySha256,
            actualReceipt.RequestIdentitySha256);
        Assert.Equal(Assert.Single(expectedReceipt.OrderedPairs),
            Assert.Single(actualReceipt.OrderedPairs));
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_ChangedEvaluatorManifestRejectsRecoveryBeforeEvaluation()
    {
        var run = await RunChangedEvaluatorManifestRejectionHttpAsync();

        Assert.Equal(StatusCodes.Status409Conflict, run.StatusCode);
        Assert.False(run.Json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .QualityGateConditionMismatchErrorCode,
            run.Json.GetProperty("errorCode").GetString());
        Assert.Equal(1, run.CreateCalls);
        Assert.Equal(0, run.EvaluationCalls);
        Assert.Equal(0, run.ResumedIssuedCallCount);
        Assert.Equal(0, run.ResumedCompletedResponseCount);
        Assert.Equal(0, run.ResumedObservationCount);
        Assert.False(run.TrainerInvoked);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoTraining_ChangedGenerationMaxNewTokensRejectsRecoveryBeforeEvaluation()
    {
        var run = await RunChangedGenerationMaxNewTokensRejectionHttpAsync();

        Assert.Equal(StatusCodes.Status409Conflict, run.StatusCode);
        Assert.False(run.Json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .QualityGateConditionMismatchErrorCode,
            run.Json.GetProperty("errorCode").GetString());
        Assert.Equal(1, run.CreateCalls);
        Assert.Equal(0, run.EvaluationCalls);
        Assert.Equal(0, run.ResumedIssuedCallCount);
        Assert.Equal(0, run.ResumedCompletedResponseCount);
        Assert.Equal(0, run.ResumedObservationCount);
        Assert.False(run.TrainerInvoked);
    }

    private static Task<HttpEndpointRun> RunHttpAsync(
        int maxTeacherCalls,
        string? createErrorCode = null,
        Action? disposeAction = null)
        => RunStartedFixtureAsync(
            maxTeacherCalls,
            createErrorCode,
            disposeAction,
            async (fixture, factory, app) =>
            {
                Assert.Empty(fixture.Request.RoutingRun.RepositoryOwnedDpoPairs);
                var receipt = fixture.Request.RoutingRun.RepositoryOwnedDpoReceipt;
                Assert.NotNull(receipt);
                var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    fixture.DpoAddress,
                    fixture.DpoRequestIdentity);
                Assert.True(opened.IsSuccess, opened.ErrorMessage);
                Assert.Single(opened.Value!.Pairs);
                var manifest = opened.Value.Manifest;
                Assert.Equal(1, receipt!.PairCount);
                Assert.Equal(manifest.PairCount, receipt.PairCount);
                Assert.Equal(
                    fixture.DpoAddress.ManifestRelativePath,
                    receipt.RepositoryRelativeManifestPath);
                Assert.Equal(manifest.ManifestSha256, receipt.ManifestSha256);
                Assert.Equal(
                    manifest.RequestIdentity.RequestIdentitySha256,
                    receipt.RequestIdentitySha256);
                Assert.Equal(
                    Assert.Single(manifest.OrderedPairs),
                    Assert.Single(receipt.OrderedPairs));

                using var client = new HttpClient
                {
                    BaseAddress = Program.ResolveStartedLoopbackAddress(app),
                };
                using var response = await client.PostAsJsonAsync(
                    CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath,
                    fixture.Request);
                var statusCode = (int)response.StatusCode;
                var body = await response.Content.ReadAsStringAsync();
                var rawBodyLengthChars = body.Length;
                var createCalls = factory.CreateCalls;
                var evaluationClient = factory.Client;
                var disposeCalls = evaluationClient?.DisposeCalls;
                var evaluationCalls = evaluationClient?.EvaluationCalls;
                try
                {
                    using var document = JsonDocument.Parse(body);
                    return new HttpEndpointRun(
                        statusCode,
                        body,
                        document.RootElement.Clone(),
                        createCalls,
                        disposeCalls ?? 0,
                        evaluationCalls ?? 0);
                }
                catch (JsonException exception)
                {
                    var observations = JsonSerializer.Serialize(new
                    {
                        StatusCode = statusCode,
                        RawBodyLengthChars = rawBodyLengthChars,
                        RawBody = body,
                        CreateCalls = createCalls,
                        ClientCreated = evaluationClient is not null,
                        DisposeCalls = disposeCalls,
                        EvaluationCalls = evaluationCalls,
                    });
                    throw new Xunit.Sdk.XunitException(
                        "S03.04 endpoint response JSON parse failed. " +
                        "Actual HTTP/client observations: " + observations +
                        Environment.NewLine + exception);
                }
            });

    private static async Task<HttpRecoveryRun> RunPositiveHttpAsync()
    {
        using var runtimeFixture = new TinyTrainingFixtureCL();
        using var repository = new TempRepositoryRoot("code-doc-http-recovery");
        var seedEnvironment = new RecoverySeedEnvironment();
        var staged = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .CreateIncompleteAppliedButUnpublishedAttemptForHttpAsync(
                runtimeFixture,
                repository.Path,
                "http-recovery-" + Guid.NewGuid().ToString("N"),
                seedEnvironment.RootInitializationSeed);
        return await RunPositiveHttpAsync(
            runtimeFixture,
            staged,
            seedEnvironment);
    }

    private static async Task<HttpRecoveryRun> RunPositiveHttpAsync(
        TinyTrainingFixtureCL runtimeFixture,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .StagedResumeAttempt staged,
        RecoverySeedEnvironment seedEnvironment)
    {
        AssertHttpRecoverySeedState(runtimeFixture, staged, seedEnvironment);
        var savedBaseline = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .ReadVerifiedResumeBaseline(runtimeFixture, staged);
        using var fixture = new EndpointTrainingFixture(runtimeFixture, staged, seedEnvironment);
        Assert.Equal(seedEnvironment.RootInitializationSeedText,
            fixture.ReadEnvironment(ContinuousLearningModelServerConfigurationLoader
                .TrainingRootInitializationSeedEnvironmentName));
        Assert.Equal(staged.RepositoryRoot, fixture.RepositoryRoot);
        Assert.Equal(staged.TrainingRunId, fixture.Request.TrainingRunId);
        Assert.Equal(staged.StartRequest.ModelIdSlug, fixture.ModelIdSlug);
        Assert.Equal(staged.StartRequest.ModelVersion, fixture.ModelVersion);
        Assert.Equal(staged.DpoSetup.Receipt.ManifestSha256,
            fixture.Request.RoutingRun.RepositoryOwnedDpoReceipt!.ManifestSha256);
        Assert.Equal(staged.DpoSetup.Receipt.RepositoryRelativeManifestPath,
            fixture.Request.RoutingRun.RepositoryOwnedDpoReceipt
                .RepositoryRelativeManifestPath);
        Assert.Single(fixture.Request.RoutingRun.RepositoryOwnedDpoPairs);
        Assert.Equal(staged.DpoSetup.Pairs,
            fixture.Request.RoutingRun.RepositoryOwnedDpoPairs);

        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .ScriptedQualityGateEvaluationClient? resumedEvaluator = null;
        var resumedTrainerInvoked = false;
        var priorObserver = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = logicalRequest =>
            {
                resumedTrainerInvoked = true;
                priorObserver?.Invoke(logicalRequest);
            };
            return await RunStartedFixtureAsync(
                fixture,
                createErrorCode: null,
                disposeAction: null,
                async (startedFixture, factory, app) =>
                {
                    var request = startedFixture.CreateRouteRecoveryRequest();
                    Assert.Equal(staged.TrainingRunId, request.TrainingRunId);
                    Assert.Equal(staged.DpoSetup.Receipt.ManifestSha256,
                        request.RoutingRun.RepositoryOwnedDpoReceipt!.ManifestSha256);
                    Assert.Equal(staged.DpoSetup.Pairs,
                        request.RoutingRun.RepositoryOwnedDpoPairs);
                    Assert.NotEmpty(request.RoutingRun.RepositoryOwnedDpoPairs);
                    AssertNonemptyRecoveryRequestWebJsonRoundTrip(request, staged);

                    var before = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                        .SnapshotResumeFiles(staged.RepositoryRoot);
                    using var client = new HttpClient
                    {
                        BaseAddress = Program.ResolveStartedLoopbackAddress(app),
                    };
                    var httpAssertionsPassed = false;
                    try
                    {
                        using var response = await client.PostAsJsonAsync(
                            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath,
                            request);
                        var body = await response.Content.ReadAsStringAsync();
                        Assert.True((int)response.StatusCode == StatusCodes.Status200OK,
                            $"Expected HTTP 200; {DescribeHttpResponse(response, body)}");
                        Assert.True(body.Length > 0,
                            $"Expected a nonempty JSON body; {DescribeHttpResponse(response, body)}");
                        using var document = JsonDocument.Parse(body);
                        var root = document.RootElement;
                        Assert.True(root.GetProperty("isSuccess").GetBoolean(),
                            $"Expected successful recovery; {DescribeHttpResponse(response, body)}");
                        var expectedRouteEvidenceRef =
                            "modelserver-route:" +
                            CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath
                                .TrimStart('/');
                        Assert.Contains(
                            expectedRouteEvidenceRef,
                            root.GetProperty("evidenceRefs")
                                .EnumerateArray()
                                .Select(reference => reference.GetString()));
                        Assert.Equal(staged.DpoSetup.Receipt.ManifestSha256,
                            root.GetProperty("dpoManifestSha256").GetString());
                        Assert.Equal(staged.ParentManifest.Step,
                            root.GetProperty("parentCheckpointStep").GetInt32());
                        Assert.Equal(staged.ParentManifest.Step + 1,
                            root.GetProperty("publishedCheckpointStep").GetInt32());
                        var qualityDecision = root.GetProperty("qualityDecision");
                        Assert.Equal(50,
                            qualityDecision.GetProperty("expectedEvaluationCallCount")
                                .GetInt32());
                        Assert.Equal(50,
                            qualityDecision.GetProperty("observedEvaluationCallCount")
                                .GetInt32());
                        var evaluator = Assert.IsType<
                            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                                .ScriptedQualityGateEvaluationClient>(resumedEvaluator);
                        Assert.Equal(25, evaluator.IssuedCallCount);
                        Assert.Equal(25, evaluator.CompletedResponseCount);
                        Assert.Equal(25, evaluator.Observations.Count);
                        var expectedCaseIds = staged.QualityCorpus.Corpus.Entries
                            .Where(entry => entry.Partition == FrozenReferencePartition.HeldOut)
                            .Select(entry => entry.EntryId)
                            .Order(StringComparer.Ordinal)
                            .ToArray();
                        Assert.Equal(expectedCaseIds,
                            evaluator.Observations
                                .Select(observation => observation.CaseId)
                                .Distinct(StringComparer.Ordinal)
                                .Order(StringComparer.Ordinal)
                                .ToArray());
                        Assert.All(
                            evaluator.Observations
                                .GroupBy(observation => observation.CaseId,
                                    StringComparer.Ordinal),
                            group =>
                            {
                                Assert.Equal(5, group.Count());
                                Assert.All(group, observation => Assert.True(observation.Passed));
                            });
                        Assert.Equal(
                            50,
                            staged.Staging.Evaluator.Observations.Count +
                            evaluator.Observations.Count);
                        Assert.Equal(51,
                            staged.Staging.Evaluator.IssuedCallCount +
                            evaluator.IssuedCallCount);
                        Assert.Equal(25, savedBaseline.State.BaselineQuality
                            .ObservedEvaluationCallCount);
                        Assert.False(resumedTrainerInvoked);
                        var nextStart = CodeDocumentationRepositoryOwnedModelCheckpointStartService
                            .PrepareStart(staged.StartRequest);
                        Assert.True(nextStart.IsSuccess, nextStart.ErrorMessage);
                        Assert.Equal(2, nextStart.Value!.ParentManifest!.Step);

                        var run = new HttpRecoveryRun(
                            (int)response.StatusCode,
                            document.RootElement.Clone(),
                            factory.CreateCalls,
                            factory.Client?.EvaluationCalls ?? 0,
                            resumedTrainerInvoked,
                            evaluator.IssuedCallCount,
                            evaluator.CompletedResponseCount,
                            evaluator.Observations.Count,
                            staged.Staging.Evaluator.Observations.Count +
                                evaluator.Observations.Count,
                            staged.Staging.Evaluator.IssuedCallCount +
                                evaluator.IssuedCallCount,
                            root.GetProperty("parentCheckpointStep").GetInt32(),
                            root.GetProperty("publishedCheckpointStep").GetInt32(),
                            evaluator.Observations
                                .Select(observation => observation.CaseId)
                                .ToArray(),
                            evaluator.Observations
                                .Select(observation => observation.RequestId)
                                .ToArray());
                        httpAssertionsPassed = true;
                        return run;
                    }
                    finally
                    {
                        if (httpAssertionsPassed)
                            AssertPositiveRecoveryOutputSnapshot(staged, before);
                    }
                },
                assertNoNegativeTrainingOutputs: false,
                innerClientFactory: configuration =>
                {
                    resumedEvaluator = new CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                        .ScriptedQualityGateEvaluationClient(
                        configuration,
                        Enumerable.Repeat(true, 5 * MeasurementNoisePolicyConfig.Canonical.RepeatCount)
                            .ToArray(),
                        index => "http-resume-quality-gate-" + index.ToString("D4"));
                    return resumedEvaluator;
                });
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = priorObserver;
        }
    }

    private static Task<HttpRecoveryRejectionRun>
        RunChangedEvaluatorManifestRejectionHttpAsync()
        => RunChangedExamConditionRejectionHttpAsync(
            "code-doc-http-exam-drift",
            evaluatorManifestDigestOverride: Hex('e'));

    private static Task<HttpRecoveryRejectionRun>
        RunChangedGenerationMaxNewTokensRejectionHttpAsync()
        => RunChangedExamConditionRejectionHttpAsync(
            "code-doc-http-generation-drift",
            generationMaxNewTokensOverride: 2);

    private static async Task<HttpRecoveryRejectionRun>
        RunChangedExamConditionRejectionHttpAsync(
            string attemptName,
            string? evaluatorManifestDigestOverride = null,
            int? generationMaxNewTokensOverride = null)
    {
        Assert.True((evaluatorManifestDigestOverride is null) !=
                    (generationMaxNewTokensOverride is null));
        using var runtimeFixture = new TinyTrainingFixtureCL();
        using var repository = new TempRepositoryRoot(attemptName);
        var seedEnvironment = new RecoverySeedEnvironment();
        var staged = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .CreateIncompleteAppliedButUnpublishedAttemptForHttpAsync(
                runtimeFixture,
                repository.Path,
                attemptName + "-" + Guid.NewGuid().ToString("N"),
                seedEnvironment.RootInitializationSeed);
        return await RunChangedExamConditionRejectionHttpAsync(
            runtimeFixture,
            staged,
            seedEnvironment,
            attemptName,
            evaluatorManifestDigestOverride,
            generationMaxNewTokensOverride);
    }

    private static async Task<HttpRecoveryRejectionRun>
        RunChangedExamConditionRejectionHttpAsync(
            TinyTrainingFixtureCL runtimeFixture,
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                .StagedResumeAttempt staged,
            RecoverySeedEnvironment seedEnvironment,
            string attemptName,
            string? evaluatorManifestDigestOverride = null,
            int? generationMaxNewTokensOverride = null)
    {
        AssertHttpRecoverySeedState(runtimeFixture, staged, seedEnvironment);
        var savedBaseline = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .ReadVerifiedResumeBaseline(runtimeFixture, staged);
        var savedExam = savedBaseline.State.BaselineQuality.Calibration.RepresentativeAssessment;
        Assert.Equal(
            Hex('a'),
            savedExam.Evaluator.ExpectedOllamaManifestDigest);
        Assert.Equal(1, savedExam.GenerationSettings.MaxNewTokens);
        // Keep this as the pre-host path -> SHA256 baseline. Host startup may add
        // its own files, so the request-scope baseline is captured after startup.
        var retainedFiles = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .SnapshotResumeFiles(staged.RepositoryRoot);
        using var fixture = new EndpointTrainingFixture(
            runtimeFixture,
            staged,
            seedEnvironment,
            evaluatorManifestDigestOverride,
            generationMaxNewTokensOverride);
        Assert.Equal(seedEnvironment.RootInitializationSeedText,
            fixture.ReadEnvironment(ContinuousLearningModelServerConfigurationLoader
                .TrainingRootInitializationSeedEnvironmentName));
        Assert.Equal(51, fixture.MaxTeacherCalls);
        Assert.Equal(5, fixture.ExpectedHeldOutCaseCount);
        Assert.Equal(50, fixture.ExpectedTotalEvaluationCallCount);
        Assert.Equal(savedExam.CorpusSha256, staged.QualityCorpus.Corpus.CorpusSha256);
        Assert.Equal(staged.QualityCorpus.Corpus.CorpusVersion, fixture.Profile.CorpusVersion);
        Assert.Equal(savedExam.CorpusManifestSha256,
            fixture.Profile.ExpectedCorpusManifestSha256);
        Assert.Equal(
            savedExam.GenerationSettings with
            {
                MaxNewTokens = generationMaxNewTokensOverride ?? savedExam.GenerationSettings.MaxNewTokens,
            },
            fixture.Profile.GenerationSettings);
        Assert.Equal(
            savedExam.Evaluator.ExpectedModelId,
            fixture.ReadEnvironment(
                ContinuousLearningModelServerConfigurationLoader
                    .ModelIdEnvironmentName));
        Assert.Equal(
            evaluatorManifestDigestOverride ?? savedExam.Evaluator.ExpectedOllamaManifestDigest,
            fixture.ReadEnvironment(
                ContinuousLearningModelServerConfigurationLoader
                    .OllamaManifestDigestEnvironmentName));
        Assert.Equal(
            savedExam.Evaluator.ExpectedOllamaModelLayerDigest,
            fixture.ReadEnvironment(
                ContinuousLearningModelServerConfigurationLoader
                    .OllamaModelLayerDigestEnvironmentName));

        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .ScriptedQualityGateEvaluationClient? resumedEvaluator = null;
        var trainerInvoked = false;
        var priorObserver = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = logicalRequest =>
            {
                trainerInvoked = true;
                priorObserver?.Invoke(logicalRequest);
            };
            return await RunStartedFixtureAsync(
                fixture,
                createErrorCode: null,
                disposeAction: null,
                async (startedFixture, factory, app) =>
                {
                    var request = startedFixture.CreateRouteRecoveryRequest();
                    Assert.Equal(staged.TrainingRunId, request.TrainingRunId);
                    Assert.Equal(staged.DpoSetup.Receipt.ManifestSha256,
                        request.RoutingRun.RepositoryOwnedDpoReceipt!.ManifestSha256);
                    Assert.Equal(staged.DpoSetup.Pairs,
                        request.RoutingRun.RepositoryOwnedDpoPairs);
                    Assert.NotEmpty(request.RoutingRun.RepositoryOwnedDpoPairs!);
                    AssertPreHostRetainedFilesPreservedAfterStartup(staged, retainedFiles);

                    using var client = new HttpClient
                    {
                        BaseAddress = Program.ResolveStartedLoopbackAddress(app),
                    };
                    var beforeRequest = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                        .SnapshotResumeFiles(staged.RepositoryRoot);
                    using var response = await client.PostAsJsonAsync(
                        CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.EndpointPath,
                        request);
                    var body = await response.Content.ReadAsStringAsync();
                    Assert.True((int)response.StatusCode == StatusCodes.Status409Conflict,
                        $"Expected HTTP 409; {DescribeHttpResponse(response, body)}");
                    Assert.True(body.Length > 0,
                        $"Expected a nonempty JSON body; {DescribeHttpResponse(response, body)}");
                    using var document = JsonDocument.Parse(body);
                    var root = document.RootElement;
                    Assert.Equal(StatusCodes.Status409Conflict, (int)response.StatusCode);
                    Assert.False(root.GetProperty("isSuccess").GetBoolean());
                    Assert.Equal(
                        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                            .QualityGateConditionMismatchErrorCode,
                        root.GetProperty("errorCode").GetString());
                    var evaluator = Assert.IsType<
                        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                            .ScriptedQualityGateEvaluationClient>(resumedEvaluator);
                    var endpointClient = Assert.IsType<EndpointEvaluationClient>(factory.Client);
                    Assert.Equal(0, endpointClient.EvaluationCalls);
                    Assert.Equal(0, evaluator.IssuedCallCount);
                    Assert.Equal(0, evaluator.CompletedResponseCount);
                    Assert.Empty(evaluator.Observations);
                    Assert.False(trainerInvoked);
                    AssertRejectedRecoveryOutputSnapshot(staged, beforeRequest);

                    return new HttpRecoveryRejectionRun(
                        (int)response.StatusCode,
                        root.Clone(),
                        factory.CreateCalls,
                        endpointClient.EvaluationCalls,
                        evaluator.IssuedCallCount,
                        evaluator.CompletedResponseCount,
                        evaluator.Observations.Count,
                        trainerInvoked);
                },
                assertNoNegativeTrainingOutputs: false,
                innerClientFactory: configuration =>
                {
                    resumedEvaluator = new CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                        .ScriptedQualityGateEvaluationClient(
                        configuration,
                        Enumerable.Repeat(true, 5 * MeasurementNoisePolicyConfig.Canonical.RepeatCount)
                            .ToArray(),
                        index => attemptName + "-quality-gate-" + index.ToString("D4"));
                    return resumedEvaluator;
                });
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = priorObserver;
        }
    }

    private static string DescribeHttpResponse(HttpResponseMessage response, string body)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType switch
        {
            "application/json" => "application/json",
            "application/problem+json" => "application/problem+json",
            "text/plain" => "text/plain",
            null => "<missing>",
            _ => "<redacted-other>",
        };
        return $"status={(int)response.StatusCode}; bodyLengthChars={body.Length}; contentType={contentType}";
    }

    private static void AssertHttpRecoverySeedState(
        TinyTrainingFixtureCL runtimeFixture,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .StagedResumeAttempt staged,
        RecoverySeedEnvironment seedEnvironment)
    {
        var resolved = CodeDocumentationDpoSeedAuthorityResolver.Resolve(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            seedEnvironment.RootInitializationSeed);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var authority = Assert.IsType<LoraPairRandomSeedAuthority>(
            staged.StartRequest.AdapterSeedAuthority);
        Assert.Equal(seedEnvironment.RootInitializationSeed, authority.RootInitializationSeed);
        Assert.Equal(resolved.Value, authority);
        Assert.Equal(IncrementalTrainer.DefaultConfig.Dropout,
            staged.StartRequest.LoraConfig!.Dropout);
        Assert.True(staged.StartRequest.LoraConfig.Dropout > 0);

        var commit = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
            .OpenExisting(staged.AttemptAddress);
        Assert.True(commit.IsSuccess, commit.ErrorMessage);
        var manifest = commit.Value!.Manifest;
        Assert.True(manifest.RandomStateRequired);
        Assert.Equal(staged.ParentManifest.Step + 1, manifest.RandomStateStep);
        Assert.Equal(
            LoraRandomStateIdentity.ComputeTopologySha256(
                runtimeFixture.ModelConfig, staged.StartRequest.LoraConfig, authority),
            manifest.RandomStateTopologySha256);
        Assert.Equal(
            LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
                staged.StartRequest.LoraConfig, authority),
            manifest.LoraConfigurationSha256);
        Assert.True(File.Exists(Path.Combine(
            staged.RepositoryRoot, manifest.RandomStateRelativePath)));
    }

    private static void AssertPositiveRecoveryOutputSnapshot(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .StagedResumeAttempt staged,
        IReadOnlyDictionary<string, string> before)
    {
        var after = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .SnapshotResumeFiles(staged.RepositoryRoot);
        foreach (var (path, hash) in before)
        {
            Assert.True(after.TryGetValue(path, out var afterHash),
                $"Positive HTTP recovery removed retained file {path}.");
            Assert.Equal(hash, afterHash);
        }

        var added = after.Keys.Except(before.Keys, StringComparer.Ordinal).ToArray();
        Assert.Contains(staged.ChildAddress.WeightsPath, added);
        Assert.Contains(staged.ChildAddress.ManifestPath, added);
        var modelLaneRoot = Path.Combine(
            staged.RepositoryRoot,
            "training-artifacts",
            "models",
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
            staged.StartRequest.ModelIdSlug,
            staged.StartRequest.ModelVersion);
        Assert.True(IsPathUnder(staged.AttemptAddress.AttemptRoot, modelLaneRoot));
        Assert.True(IsPathUnder(staged.ChildAddress.CheckpointRoot, modelLaneRoot));
        Assert.All(added, path =>
        {
            Assert.True(IsPathUnder(path, staged.ChildAddress.CheckpointRoot),
                $"Positive HTTP recovery created an unexpected output: {path}");
        });

        var baselinePath = Path.Combine(
            staged.AttemptAddress.AttemptRoot,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .BaselineStateFileName);
        var pairPath = Path.Combine(
            staged.RepositoryRoot,
            staged.DpoSetup.Receipt.OrderedPairs[0]
                .RepositoryRelativePairFilePath);
        var trialPath = Path.Combine(
            staged.AttemptAddress.AttemptRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var optimizerManifest = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
            .OpenExisting(staged.AttemptAddress);
        Assert.True(optimizerManifest.IsSuccess, optimizerManifest.ErrorMessage);
        var optimizerPath = Path.Combine(
            staged.RepositoryRoot,
            optimizerManifest.Value!.Manifest.OptimizerStateRelativePath);
        var optimizerManifestPath = optimizerManifest.Value.ManifestPath;
        var retainedPaths = new[]
        {
            baselinePath,
            pairPath,
            trialPath,
            staged.ParentAddress.WeightsPath,
            staged.ParentAddress.ManifestPath,
            optimizerPath,
            optimizerManifestPath,
        }.Select(Path.GetFullPath).ToArray();
        foreach (var path in retainedPaths)
        {
            Assert.True(before.ContainsKey(path),
                $"Positive HTTP recovery did not snapshot required retained file {path}.");
            Assert.Equal(before[path], after[path]);
        }

        if (optimizerManifest.Value.Manifest.RandomStateRequired)
        {
            var randomPath = Path.GetFullPath(Path.Combine(
                staged.RepositoryRoot,
                optimizerManifest.Value.Manifest.RandomStateRelativePath));
            Assert.True(before.ContainsKey(randomPath));
            Assert.Equal(before[randomPath], after[randomPath]);
        }
    }

    private static void AssertPreHostRetainedFilesPreservedAfterStartup(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .StagedResumeAttempt staged,
        IReadOnlyDictionary<string, string> preHostRetainedFiles)
    {
        var afterStartup = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .SnapshotResumeFiles(staged.RepositoryRoot);
        foreach (var (path, hash) in preHostRetainedFiles)
        {
            Assert.True(File.Exists(path),
                $"Host startup removed pre-host retained file {path}.");
            Assert.True(afterStartup.TryGetValue(path, out var afterHash),
                $"Host startup removed pre-host retained file {path}.");
            Assert.Equal(hash, afterHash);
        }
    }

    private static void AssertRejectedRecoveryOutputSnapshot(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .StagedResumeAttempt staged,
        IReadOnlyDictionary<string, string> beforeRequest)
    {
        var after = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .SnapshotResumeFiles(staged.RepositoryRoot);
        var added = after.Keys
            .Except(beforeRequest.Keys, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(path => Path.GetRelativePath(staged.RepositoryRoot, path))
            .ToArray();
        var removed = beforeRequest.Keys
            .Except(after.Keys, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(path => Path.GetRelativePath(staged.RepositoryRoot, path))
            .ToArray();
        Assert.True(added.Length == 0 && removed.Length == 0,
            $"Changed exam rejection changed resume file keys. " +
            $"Added: [{string.Join(", ", added)}]. " +
            $"Removed: [{string.Join(", ", removed)}].");
        foreach (var (path, hash) in beforeRequest)
        {
            Assert.True(after.TryGetValue(path, out var afterHash),
                $"Changed exam rejection removed retained file {path}.");
            Assert.Equal(hash, afterHash);
        }

        Assert.False(File.Exists(staged.ChildAddress.WeightsPath));
        Assert.False(File.Exists(staged.ChildAddress.ManifestPath));
        Assert.False(Directory.Exists(staged.ChildAddress.CheckpointRoot));
        var accepted = CodeDocumentationRepositoryOwnedModelCheckpointStartService
            .PrepareStart(staged.StartRequest);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Equal(staged.ParentManifest.Step, accepted.Value!.ParentManifest!.Step);
        Assert.Equal(
            staged.ParentManifest.WeightsSha256,
            accepted.Value.ParentManifest.WeightsSha256);
    }

    private static bool IsPathUnder(string path, string root)
    {
        var fullPath = Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(fullPath, fullRoot, StringComparison.OrdinalIgnoreCase) ||
            fullPath.StartsWith(
                fullRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
    }

    private static Task<T> RunStartedFixtureAsync<T>(
        int maxTeacherCalls,
        string? createErrorCode,
        Action? disposeAction,
        Func<EndpointTrainingFixture, EndpointEvaluationClientFactory,
            WebApplication, Task<T>> action)
        => RunStartedFixtureAsync(
            new EndpointTrainingFixture(maxTeacherCalls),
            createErrorCode,
            disposeAction,
            action,
            assertNoNegativeTrainingOutputs: true,
            innerClientFactory: null);

    private static async Task<T> RunStartedFixtureAsync<T>(
        EndpointTrainingFixture fixture,
        string? createErrorCode,
        Action? disposeAction,
        Func<EndpointTrainingFixture, EndpointEvaluationClientFactory,
            WebApplication, Task<T>> action,
        bool assertNoNegativeTrainingOutputs,
        Func<ContinuousLearningTp1DpoConfiguration,
            ILegalTeacherEvaluationInferenceClient>? innerClientFactory,
        bool disposeFixture = true,
        RoutingProfileTrainer? profileTrainer = null,
        Action<byte[]>? responseObserver = null)
    {
        IDisposable? environment = null;
        var factory = new EndpointEvaluationClientFactory(
            createErrorCode,
            disposeAction,
            innerClientFactory);
        WebApplication? app = null;
        var started = false;
        IReadOnlyDictionary<string, string>? startupSnapshot = null;
        IReadOnlyDictionary<string, string>? sealedInputSnapshot = null;
        Exception? primaryFailure = null;
        try
        {
            environment = fixture.ApplyEnvironment();
            app = Program.CreateLocalLoopbackHostCore(
                fixture.ReadyFileArguments,
                fixture.ReadEnvironment,
                fixture.RepositoryRoot,
                profileTrainer: profileTrainer);
            app.Use(async (context, next) =>
            {
                var originalServices = context.RequestServices;
                var originalBody = context.Response.Body;
                using var capturedBody = responseObserver is null ? null : new MemoryStream();
                if (capturedBody is not null) context.Response.Body = capturedBody;
                context.RequestServices = new FactoryReplacingServiceProvider(
                    originalServices,
                    factory);
                try
                {
                    await next(context);
                }
                finally
                {
                    context.RequestServices = originalServices;
                    context.Response.Body = originalBody;
                    if (capturedBody is not null)
                    {
                        responseObserver!(capturedBody.ToArray());
                        capturedBody.Position = 0;
                        await capturedBody.CopyToAsync(originalBody);
                    }
                }
            });
            await app.StartAsync();
            started = true;
            var serviceProbe = new FactoryReplacingServiceProvider(
                app.Services,
                factory);
            Assert.Same(
                serviceProbe,
                serviceProbe.GetService(typeof(IServiceProvider)));
            Assert.Same(
                factory,
                serviceProbe.GetService(
                    typeof(ICodeDocumentationSavedModelAssessmentEvaluationClientFactory)));
            AssertActiveFixtureReady(app, fixture);
            fixture.AssertSnapshotSelectorContract();
            startupSnapshot = fixture.SnapshotOwnedFiles();
            fixture.AssertActiveOwnerLockPresence(
                startupSnapshot,
                "active startup snapshot");
            sealedInputSnapshot = fixture.SnapshotSealedInputs();
            return await action(fixture, factory, app!);
        }
        catch (Exception exception)
        {
            primaryFailure = exception;
            throw;
        }
        finally
        {
            Exception? cleanupFailure = null;
            try
            {
                if (startupSnapshot is not null && sealedInputSnapshot is not null)
                {
                    var finalOwnedSnapshot = fixture.SnapshotOwnedFiles();
                    fixture.AssertActiveOwnerLockPresence(
                        finalOwnedSnapshot,
                        "active HTTP snapshot");
                    fixture.AssertSnapshotUnchanged(
                        startupSnapshot,
                        finalOwnedSnapshot,
                        "host startup to HTTP");
                    fixture.AssertSnapshotUnchanged(
                        sealedInputSnapshot,
                        fixture.SnapshotSealedInputs(),
                        "sealed inputs through HTTP");
                    if (assertNoNegativeTrainingOutputs)
                        fixture.AssertNoNegativeTrainingOutputs();
                }
            }
            catch (Exception exception)
            {
                cleanupFailure = exception;
            }
            try
            {
                if (started && app is not null)
                    await app.StopAsync();
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
            try
            {
                if (app is not null)
                    await app.DisposeAsync();
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
            try
            {
                environment?.Dispose();
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
            try
            {
                if (disposeFixture) fixture.Dispose();
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
            if (primaryFailure is null && cleanupFailure is not null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo
                    .Capture(cleanupFailure)
                    .Throw();
        }
    }

    private static bool SameRepositoryRoot(string left, string right)
    {
        try
        {
            static string Normalize(string value)
                => Path.GetFullPath(value).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            return string.Equals(
                Normalize(left),
                Normalize(right),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception)
            when (exception is ArgumentException or IOException or
                NotSupportedException or System.Security.SecurityException)
        {
            return false;
        }
    }

    private static void AssertActiveFixtureReady(
        WebApplication app,
        EndpointTrainingFixture fixture)
    {
        var startup = app.Services.GetRequiredService<
            ContinuousLearningModelServerStartup>();
        var readiness = startup.Readiness;
        Assert.True(readiness.ConfigurationAdmitted);
        Assert.True(readiness.LegalEvidenceVerified);
        Assert.True(readiness.CompositionMaterialized);
        Assert.True(readiness.PathIdentityVerified);
        Assert.True(readiness.RetainedObjectIdentityBound);
        Assert.True(readiness.StableDpoExecutorConfigured);
        Assert.True(readiness.TrainingEnabled);
        Assert.True(readiness.CycleExecutionAvailable);
        Assert.True(readiness.PerformsTraining);
        Assert.True(startup.IsLegalTrainingTeacherAdmissionActive());
        Assert.True(app.Services.GetRequiredService<
            DesignConversionRetainedDpoPolicyBaseProviderGateway>().IsBound);
        Assert.True(File.Exists(fixture.ReadyFilePath));
        Assert.StartsWith(
            fixture.Root,
            fixture.ReadyFilePath,
            StringComparison.OrdinalIgnoreCase);

        var fileAccess = app.Services.GetRequiredService<
            ServerOwnedContinuousLearningFileAccess>();
        var context = startup.PrepareRepositoryOwnedTrainingExecutionContext(
            EndpointTrainingFixture.DomainSlug,
            fileAccess);
        Assert.True(context.IsSuccess, context.ErrorMessage);
        Assert.NotNull(context.Value);
        Assert.Equal(
            fixture.RepositoryRoot,
            context.Value!.RepositoryStartRequest.RepositoryRoot);
        Assert.Equal(
            fixture.ModelIdSlug,
            context.Value.RepositoryStartRequest.ModelIdSlug);
        Assert.Equal(
            fixture.ModelVersion,
            context.Value.RepositoryStartRequest.ModelVersion);
        Assert.False(
            context.Value.RepositoryStartRequest.ColdStartPolicy.IsAuthorized);
        Assert.Null(
            context.Value.RepositoryStartRequest.ColdStartPolicy.HumanReason);
        Assert.Equal(
            fixture.DpoIdentity.RepositoryRelativeManifestPath,
            context.Value.RepositoryStartRequest.CurrentDpoDatasetManifest!
                .RepositoryRelativeManifestPath);
        Assert.Equal(
            fixture.DpoIdentity.ManifestSha256,
            context.Value.RepositoryStartRequest.CurrentDpoDatasetManifest!
                .ManifestSha256);

        var corpus = CodeDocumentationSavedModelAssessmentEndpoint
            .OpenConfiguredCorpus(fixture.RepositoryRoot, fixture.Profile);
        Assert.True(corpus.IsSuccess, corpus.ErrorMessage);
        Assert.Equal(fixture.ExpectedHeldOutCaseCount, corpus.Value!.Corpus.Entries.Count(
            static entry => entry.Partition == FrozenReferencePartition.HeldOut));
        var qualityPlan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
            .PlanQualityGate(
                corpus.Value.Corpus,
                fixture.Profile.GenerationSettings,
                startup.TeacherConfiguration,
                MeasurementNoisePolicyConfig.Canonical);
        var baselinePlan = qualityPlan.IsSuccess
            ? qualityPlan.Value!.BaselinePlan
            : CodeDocumentationSavedModelAssessmentCalibrationService
                .PlanCalibration(
                    corpus.Value.Corpus,
                    fixture.Profile.GenerationSettings,
                    startup.TeacherConfiguration,
                    MeasurementNoisePolicyConfig.Canonical)
                .Value;
        Assert.NotNull(baselinePlan);
        Assert.Equal(fixture.ExpectedHeldOutCaseCount, baselinePlan!.HeldOutCaseCount);
        Assert.Equal(5, baselinePlan.RepeatCount);
        Assert.Equal(
            fixture.ExpectedHeldOutCaseCount * baselinePlan.RepeatCount,
            baselinePlan.ExpectedEvaluationCallCount);
        if (fixture.MaxTeacherCalls == 19)
        {
            Assert.False(qualityPlan.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                    .QualityGateBudgetExceededErrorCode,
                qualityPlan.ErrorCode);
            Assert.Null(qualityPlan.Value);
        }
        else
        {
            Assert.True(qualityPlan.IsSuccess, qualityPlan.ErrorMessage);
            Assert.Equal(
                fixture.ExpectedTotalEvaluationCallCount,
                qualityPlan.Value!.ExpectedTotalEvaluationCallCount);
        }
    }

    private static CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest
        CreateValidShapeRequest()
    {
        var receipt = CreateValidReceipt();
        return new CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DefaultDatasetVersion,
            "code-doc-endpoint-test-run",
            new CodeDocumentationTrainingRunResult(
                TrainingExampleCount: 50,
                EvaluationExampleCount: 50,
                RoutedCorrect: 50,
                SourceCorrect: 50,
                VersionCorrect: 50,
                TransitCorrect: 50,
                RoutingAccuracy: 1.0,
                SourceGateAccuracy: 1.0,
                VersionGateAccuracy: 1.0,
                TransitGateAccuracy: 1.0,
                BottomUpOrderAccuracy: 1.0,
                CalleeSummaryTransitAccuracy: 1.0,
                LocalVsDelegatedAccuracy: 1.0,
                CpuMilliseconds: 1,
                PeakWorkingSetMb: 1,
                ElapsedMilliseconds: 1,
                DpoPairs: [],
                RepositoryOwnedDpoReceipt: receipt,
                RepositoryOwnedDpoPairs: [],
                Cases: []),
            ["test:code-documentation-repository-owned-dpo-endpoint"]);
    }

    private static CodeDocumentationRepositoryOwnedDpoDatasetReceipt
        CreateValidReceipt()
        => new(
            RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset,
            Hex('a'),
            "training-data/dpo/csharp-code-documentation/ilearning-unit/v1/manifest.json",
            PairCount: 1,
            Hex('b'),
            [
                new RepositoryOwnedDpoDatasetManifestPair(
                    "pair-001",
                    "training-data/dpo/csharp-code-documentation/ilearning-unit/v1/pairs/pair-001.json",
                    Hex('c')),
            ]);

    private static CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence
        CreatePublicationEvidence(
            int? parentStep,
            bool includeQualityDecision = true,
            bool qualityDecisionAccepted = true,
            double? qualityDecisionDifference = null)
    {
        const string checkpointDirectory =
            "training-artifacts/models/csharp-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000002";
        var dpoIdentity = new RepositoryOwnedDpoDatasetManifestIdentity(
            "training-data/dpo/csharp-code-documentation/ilearning-unit/v1/manifest.json",
            Hex('a'));
        var compatibility = new RepositoryOwnedModelRuntimeCompatibility(
            Hex('1'),
            Hex('2'),
            Hex('3'),
            Hex('4'),
            Hex('5'),
            "compatibility-test");
        var parent = parentStep is null
            ? null
            : new RepositoryOwnedModelCheckpointParentIdentity(
                "training-artifacts/models/csharp-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000001/manifest.json",
                parentStep.Value,
                Hex('6'),
                2);
        var publication =
            new CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence(
                Step: 2,
                CheckpointRoot: "not-read-by-contract-test",
                WeightsPath: "not-read-by-contract-test",
                ManifestPath: "not-read-by-contract-test",
                RepositoryRelativeCheckpointDirectory: checkpointDirectory,
                RepositoryRelativeWeightsPath:
                checkpointDirectory + "/weights.bin",
                RepositoryRelativeManifestPath:
                checkpointDirectory + "/manifest.json",
                ManifestSha256: Hex('7'),
                WeightsSha256: Hex('8'),
                WeightsByteLength: 2,
                ParentCheckpoint: parent,
                ColdStartReason: null,
                DpoDatasetManifest: dpoIdentity,
                RuntimeCompatibility: compatibility,
                ProvenanceRefs: ["test:provenance"],
                EvidenceRefs: ["test:publication"]);

        return new CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence(
            dpoIdentity.RepositoryRelativeManifestPath,
            dpoIdentity.ManifestSha256,
            ReopenedDpoPairCount: 1,
            ParentCheckpointStep: parentStep,
            PublishedCheckpointStep: 2,
            PublishedWeightsRelativePath:
            publication.RepositoryRelativeWeightsPath,
            PublishedManifestRelativePath:
            publication.RepositoryRelativeManifestPath,
            Training: new CodeDocumentationServerOwnedDpoTrainingEvidence(
                ProcessedPairCount: 1,
                DpoStepCount: 1,
                PolicyCheckpointPath:
                checkpointDirectory + "/dpo_policy_step_000001.bin",
                MetricsPath:
                checkpointDirectory + "/dpo_metrics.json",
                LegalTeacherPolicyIdentityRef: "test:legal-teacher"),
            Publication: publication,
            EvidenceRefs: ["test:endpoint-success-response"],
            QualityDecision: includeQualityDecision
                ? CreateQualityDecision(
                    qualityDecisionAccepted,
                    qualityDecisionDifference)
                : null);
    }

    private static CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence
        CreateQualityDecision(
            bool accepted,
            double? difference = null)
        => new(
            accepted,
            "pair-001",
            accepted
                ? MeasurementNoisePolicy.ImprovedStatus
            : MeasurementNoisePolicy.IndistinguishableStatus,
            BaselineMeanCorrectFraction: 0,
            TrialMeanCorrectFraction: accepted ? 1 : 0,
            Difference: difference ?? (accepted ? 1 : 0),
            NoiseFloor: 0.01,
            ExpectedEvaluationCallCount: 50,
            ObservedEvaluationCallCount: 50,
            BaselineCalibration: null!,
            TrialCalibration: null!,
            EvidenceRefs: ["test:quality-decision"]);

    private static string CreateAssessmentProfileJson(
        string domainSlug =
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug)
        => JsonSerializer.Serialize(
            new[]
            {
                new
                {
                    domainSlug,
                    designPatternSlug =
                        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                            .DesignPatternSlug,
                    subjectSlug = "csharp-documentation-endpoint-fixture",
                    corpusVersion = "v20260909-endpoint",
                    expectedCorpusManifestSha256 = Hex('d'),
                    generation = new
                    {
                        maxNewTokens = 1,
                        temperature = 0f,
                        topK = 1,
                        topP = 1f,
                        seed = 17,
                    },
                    maxHeldOutCases = 5,
                    maxAnswerChars = 256,
                },
            });

    private static bool InvokeDatasetGate(
        MethodInfo method,
        RepositoryOwnedDpoDatasetAddress address,
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt? receipt,
        RepositoryOwnedDpoDatasetManifestIdentity? manifest)
        => Assert.IsType<bool>(method.Invoke(
            null,
            [address, receipt, manifest]));

    private static async Task<CapturedResult> CaptureAsync(IResult result)
    {
        var context = new DefaultHttpContext();
        await using var services = new ServiceCollection()
            .AddLogging()
            .AddRouting()
            .BuildServiceProvider();
        await using var body = new MemoryStream();
        context.RequestServices = services;
        context.Response.Body = body;
        await result.ExecuteAsync(context);
        body.Position = 0;
        using var reader = new StreamReader(body, Encoding.UTF8);
        var text = await reader.ReadToEndAsync();
        return new CapturedResult(
            context.Response.StatusCode,
            text,
            JsonDocument.Parse(text));
    }

    private static string Hex(char value)
        => new(value, 64);

    private sealed record CapturedResult(
        int StatusCode,
        string Body,
        JsonDocument Json);

    private sealed record HttpEndpointRun(
        int StatusCode,
        string Body,
        JsonElement Json,
        int CreateCalls,
        int DisposeCalls,
        int EvaluationCalls);

    private sealed record HttpRecoveryRun(
        int StatusCode,
        JsonElement Json,
        int CreateCalls,
        int EvaluationCalls,
        bool TrainerInvoked,
        int ResumedIssuedCallCount,
        int ResumedCompletedResponseCount,
        int ResumedObservationCount,
        int TotalObservationCount,
        int TotalAttemptedCallCount,
        int ParentCheckpointStep,
        int PublishedCheckpointStep,
        string[] ResumedCaseIds,
        string[] ResumedRequestIds);

    private sealed record HttpRecoveryRejectionRun(
        int StatusCode,
        JsonElement Json,
        int CreateCalls,
        int EvaluationCalls,
        int ResumedIssuedCallCount,
        int ResumedCompletedResponseCount,
        int ResumedObservationCount,
        bool TrainerInvoked);

    private sealed record DirectEndpointRun(
        CapturedResult Captured,
        int CreateCalls,
        string? RepositoryRoot = null);

    private sealed class SelectedMaterialReplacingServiceProvider(IServiceProvider inner,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory factory,
        Func<CodeDocumentationOneLessonSelectionDecisionProvider?> provider,
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission?> admission) : IServiceProvider
    {
        public object? GetService(Type type) => type == typeof(IServiceProvider) ? this :
            type == typeof(ICodeDocumentationSavedModelAssessmentEvaluationClientFactory) ? factory :
            type == typeof(CodeDocumentationOneLessonSelectionDecisionProvider) ? provider() :
            type == typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission) ? admission() : inner.GetService(type);
    }

    private sealed class FactoryReplacingServiceProvider(
        IServiceProvider inner,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory factory)
        : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => serviceType == typeof(IServiceProvider)
                ? this
                : serviceType == typeof(
                    ICodeDocumentationSavedModelAssessmentEvaluationClientFactory)
                    ? factory
                    : inner.GetService(serviceType);
    }

    private sealed class EndpointEvaluationClientFactory(
        string? createErrorCode,
        Action? disposeAction,
        Func<ContinuousLearningTp1DpoConfiguration,
            ILegalTeacherEvaluationInferenceClient>? innerClientFactory = null)
        : ICodeDocumentationSavedModelAssessmentEvaluationClientFactory
    {
        public int CreateCalls { get; private set; }
        public EndpointEvaluationClient? Client { get; private set; }

        public OperationResult<ILegalTeacherEvaluationInferenceClient> Create(
            ContinuousLearningTp1DpoConfiguration configuration)
        {
            CreateCalls++;
            if (createErrorCode is not null)
            {
                return OperationResult<ILegalTeacherEvaluationInferenceClient>.Fail(
                    "S03.04 evaluation factory sentinel.",
                    createErrorCode);
            }

            Client = new EndpointEvaluationClient(
                disposeAction,
                innerClientFactory?.Invoke(configuration));
            return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                Client);
        }
    }

    private sealed class EndpointEvaluationClient(
        Action? disposeAction,
        ILegalTeacherEvaluationInferenceClient? innerClient = null)
        : ILegalTeacherEvaluationInferenceClient
    {
        private int _disposeCalls;
        private int _evaluationCalls;

        public int DisposeCalls => Volatile.Read(ref _disposeCalls);
        public int EvaluationCalls => Volatile.Read(ref _evaluationCalls);

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _evaluationCalls);
            if (innerClient is not null)
            {
                return innerClient.EvaluateAsync(
                    prompt,
                    callPermit,
                    cancellationToken);
            }

            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Fail(
                    "S03.04 evaluation client must not be reached by the start-failure fixture.",
                    "S0304_EVALUATION_SENTINEL"));
        }

        public void Dispose()
        {
            Interlocked.Increment(ref _disposeCalls);
            try
            {
                innerClient?.Dispose();
            }
            finally
            {
                disposeAction?.Invoke();
            }
        }
    }

    internal sealed class RecoverySeedEnvironment
    {
        internal RecoverySeedEnvironment(string rootInitializationSeedText = "0")
        {
            RootInitializationSeedText = rootInitializationSeedText;
            RootInitializationSeed =
                ContinuousLearningModelServerConfigurationLoader
                    .ReadRootInitializationSeed(ReadEnvironment, required: true)
                ?? throw new InvalidOperationException(
                    "The recovery fixture requires a canonical startup seed.");
        }

        internal string RootInitializationSeedText { get; }
        internal ulong RootInitializationSeed { get; }

        private string? ReadEnvironment(string name)
            => StringComparer.Ordinal.Equals(
                name,
                ContinuousLearningModelServerConfigurationLoader
                    .TrainingRootInitializationSeedEnvironmentName)
                ? RootInitializationSeedText
                : null;
    }

    internal sealed class EndpointTrainingFixture : IDisposable
    {
        private static readonly string FixtureRoot = Path.GetFullPath(
            Path.Combine(
                ResolveActualRepositoryRoot(),
                "artifacts",
                "test-results",
                "code-documentation-repository-owned-dpo-training-fixtures"));
        private readonly Dictionary<string, string> _environment;
        private readonly bool _ownsRepositoryArtifacts;
        private readonly TempRepositoryRoot? _nativeStorage;
        private readonly ConstructorFailureProbe? _constructorFailureProbe;
        private bool _owningFixtureLaneStarted;
        private readonly string _ownedFixtureRoot = FixtureRoot;
        private readonly List<RepositoryOwnedReferenceCorpusAddress> _ownedTeachingCorpora = [];
        private readonly List<RepositoryOwnedDpoDatasetAddress> _ownedContextHistory = [];
        internal IReadOnlyList<ContinuousLearning.DpoPair> ContextHistoryCreationInputs { get; private set; } = []; // Retain independent pre-seal inputs for manifest-order expectations, never derive them from reopened actual pairs.
        private bool _disposed;

        public EndpointTrainingFixture(int maxTeacherCalls, int? ordinaryCaseCount = null, bool usePublicHostRepositoryRoot = false)
            : this(maxTeacherCalls, ordinaryCaseCount, usePublicHostRepositoryRoot, failureProbe: null)
        { }

        private EndpointTrainingFixture(int maxTeacherCalls, int? ordinaryCaseCount, bool usePublicHostRepositoryRoot, ConstructorFailureProbe? failureProbe)
        {
            Assert.False(usePublicHostRepositoryRoot && failureProbe is not null); // An isolated failure probe can never select the public Host authority.
            _constructorFailureProbe = failureProbe;
            _ownsRepositoryArtifacts = true;
            MaxTeacherCalls = maxTeacherCalls;
            ExpectedHeldOutCaseCount = ordinaryCaseCount ?? 2;
            ExpectedTotalEvaluationCallCount = checked(10 * ExpectedHeldOutCaseCount);
            // Ordinary synthetic fixtures own fresh runtime material on every platform;
            // machine-bound checkout history cannot become their bootstrap input.
            // Source and loaded assemblies still come from the actual checkout.
            // Only the public-Host fixture opts into its unchanged fixed-root authority.
            _nativeStorage = failureProbe is not null ? new TempRepositoryRoot("s04-constructor-failure-owned")
                : usePublicHostRepositoryRoot ? TempRepositoryRoot.PreparePublicHostRoot()
                : new TempRepositoryRoot("s04-ordinary-native-owned");
            try // Ownership of the acquired bootstrap survives every later constructor failure.
            {
                RepositoryRoot = _nativeStorage?.Path ?? ResolveActualRepositoryRoot();
                if (failureProbe is not null) failureProbe.BootstrapRoot = RepositoryRoot;
                var lane = Guid.NewGuid().ToString("N");
                // Actual runtime files and sealed artifact addresses share RepositoryRoot, including the unique lane used by exact cleanup.
                _ownedFixtureRoot = usePublicHostRepositoryRoot
                    ? Path.Combine(RepositoryRoot, "artifacts", "test-results", "code-documentation-repository-owned-dpo-training-fixtures")
                    : _nativeStorage is null ? FixtureRoot : Path.Combine(RepositoryRoot, "artifacts", "test-results", "endpoint-fixtures");
                Root = Path.Combine(_ownedFixtureRoot, lane);
                Assert.False(Directory.Exists(Root) || File.Exists(Root)); // Never acquire a pre-existing GUID lane for constructor-failure cleanup.
                _owningFixtureLaneStarted = true;
                Directory.CreateDirectory(Root);
                if (failureProbe is not null)
                {
                    failureProbe.FixtureLane = Root; // Fail only after the real isolated GUID directory is acquired, before either address is assigned.
                    throw failureProbe.PrimaryError;
                }
                Directory.CreateDirectory(Path.Combine(Root, "ready"));

                DatasetVersion = "s0304-" + lane;
                SubjectSlug = "s0304-" + lane;
                CorpusVersion = "s0304-" + lane;
                ModelIdSlug = "s0304-" + lane;
                ModelVersion = "s0304-" + lane;
                ReadyFilePath = Path.Combine(
                    Root,
                    "ready",
                    "modelserver-ready.json");
                BaseCheckpoint = Path.Combine(Root, "base.bin");
                TokenizerPath = Path.Combine(Root, "tokenizer.bin");
                AdapterRoot = Path.Combine(Root, "adapter");
                CacheRoot = Path.Combine(Root, "cache");
                RegistryRoot = Path.Combine(Root, "registry");
                ModelConfiguration = CreateModelConfig(ordinaryCaseCount.HasValue ? 64 : 8);
                RuntimeCompatibility = new RepositoryOwnedModelRuntimeCompatibility(
                    TrainingSourceProvenance.Identify(ModelConfiguration).Sha256,
                    new string('2', 64),
                    new string('3', 64),
                    new string('4', 64),
                    new string('5', 64),
                    "tp1-dpo-runtime-v0001");
                new LlmModel(ModelConfiguration).SaveCheckpoint(BaseCheckpoint);
                var tokenizer = new BpeTokenizer();
                tokenizer.Train(
                    "S03.04 repository owned DPO training endpoint fixture Add Subtract",
                    ModelConfiguration.VocabSize);
                tokenizer.Save(TokenizerPath);
                Directory.CreateDirectory(AdapterRoot);
                Directory.CreateDirectory(CacheRoot);
                Directory.CreateDirectory(RegistryRoot);

                var dpoAddress = RepositoryOwnedTrainingArtifactLayout
                    .CreateDpoDatasetAddress(
                        RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        DatasetVersion);
                Assert.True(dpoAddress.IsSuccess, dpoAddress.ErrorMessage);
                DpoAddress = dpoAddress.Value!;
                DpoIdentity = SealDpoDataset(DpoAddress);

                var corpusAddress = RepositoryOwnedTrainingArtifactLayout
                    .CreateReferenceCorpusAddress(
                        RepositoryRoot,
                        DomainSlug,
                        SubjectSlug,
                        CorpusVersion);
                Assert.True(corpusAddress.IsSuccess, corpusAddress.ErrorMessage);
                CorpusAddress = corpusAddress.Value!;
                Corpus = SealReferenceCorpus(CorpusAddress);
                // Freeze the explicit ordinary corpus size in its real generation profile before startup decoding.
                ProfileJson = CreateProfileJson(Corpus.Manifest.ManifestSha256,ordinaryCaseCount ?? 2);
                Profile = ResolveProfile(ProfileJson);
                Request = CreateRequest();

                _environment = BuildEnvironment(
                    lane,
                    maxTeacherCalls,
                    useCoordinatorTeacherIdentity: false,
                    seedEnvironment: new RecoverySeedEnvironment());
            }
            catch (Exception constructionError)
            {
                try { Dispose(); } // Partial-safe cleanup tries every acquired lane and always attempts the bootstrap release.
                catch (Exception cleanupError) { constructionError.Data["EndpointFixtureCleanupErrors"] = cleanupError; } // Retain cleanup evidence without replacing the exact primary exception.
                throw;
            }
        }

        private sealed class ConstructorFailureProbe(bool failLaneCleanup)
        {
            internal bool FailLaneCleanup { get; } = failLaneCleanup;
            internal InvalidOperationException PrimaryError { get; } = new("Fixed isolated constructor failure after GUID lane acquisition.");
            internal IOException CleanupError { get; } = new("Fixed isolated exact-lane cleanup failure.");
            internal string BootstrapRoot { get; set; } = "";
            internal string FixtureLane { get; set; } = "";
            internal int BootstrapReleaseAttempts { get; set; }
        }

        internal static void AssertIsolatedConstructorFailureCleanup(bool failLaneCleanup)
        {
            // Both roots are independently owned synthetic temp siblings; neither represents a canonical or shared repository.
            using var sibling = new TempRepositoryRoot("s04-constructor-failure-sibling-owned");
            var files = Directory.EnumerateFiles(sibling.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
            var directories = Directory.EnumerateDirectories(sibling.Path, "*", SearchOption.AllDirectories).Prepend(sibling.Path).Order(StringComparer.Ordinal).ToArray();
            var bytes = files.ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);
            var times = files.ToDictionary(path => path, path => File.GetLastWriteTimeUtc(path).Ticks, StringComparer.Ordinal);
            static byte[] Modes(string path)
            {
                if (OperatingSystem.IsWindows())
                    return Directory.Exists(path)
                        ? System.IO.FileSystemAclExtensions.GetAccessControl(new DirectoryInfo(path)).GetSecurityDescriptorBinaryForm()
                        : System.IO.FileSystemAclExtensions.GetAccessControl(new FileInfo(path)).GetSecurityDescriptorBinaryForm();
                return BitConverter.GetBytes((int)File.GetUnixFileMode(path));
            }
            var modes = files.Concat(directories).ToDictionary(path => path, Modes, StringComparer.Ordinal);
            var probe = new ConstructorFailureProbe(failLaneCleanup);
            var primary = Assert.Throws<InvalidOperationException>(() => new EndpointTrainingFixture(10000, null, false, probe));
            Assert.Same(probe.PrimaryError, primary); // Constructor cleanup preserves the original object and throw identity.
            Assert.NotEmpty(probe.BootstrapRoot);
            Assert.NotEmpty(probe.FixtureLane);
            Assert.Equal(1, probe.BootstrapReleaseAttempts);
            Assert.False(Directory.Exists(probe.FixtureLane));
            Assert.False(Directory.Exists(probe.BootstrapRoot)); // The real acquired TempRepositoryRoot release runs even after a lane-cleanup error.
            if (failLaneCleanup)
            {
                var cleanup = Assert.IsType<AggregateException>(primary.Data["EndpointFixtureCleanupErrors"]);
                Assert.Same(probe.CleanupError, Assert.Single(cleanup.InnerExceptions));
            }
            else Assert.False(primary.Data.Contains("EndpointFixtureCleanupErrors"));
            Assert.Equal(files, Directory.EnumerateFiles(sibling.Path, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
            Assert.Equal(directories, Directory.EnumerateDirectories(sibling.Path, "*", SearchOption.AllDirectories).Prepend(sibling.Path).Order(StringComparer.Ordinal));
            foreach (var path in files)
            {
                Assert.Equal(bytes[path], File.ReadAllBytes(path));
                Assert.Equal(times[path], File.GetLastWriteTimeUtc(path).Ticks);
            }
            foreach (var path in modes.Keys) Assert.Equal(modes[path], Modes(path)); // Exact Windows security descriptors or Unix modes of the unrelated sibling stay unchanged.
        }

        public EndpointTrainingFixture(
            TinyTrainingFixtureCL runtimeFixture,
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                .PreparedAppliedButUnpublishedAttempt prepared,
            RecoverySeedEnvironment seedEnvironment)
        {
            _ownsRepositoryArtifacts = false;
            MaxTeacherCalls = 51;
            ExpectedHeldOutCaseCount = 5;
            ExpectedTotalEvaluationCallCount = 50;
            RepositoryRoot = prepared.RepositoryRoot;
            var lane = Guid.NewGuid().ToString("N");
            _nativeStorage = OperatingSystem.IsLinux() ? new TempRepositoryRoot("s04-endpoint-recovery-native-owned") : null;
            _ownedFixtureRoot = _nativeStorage is not null ? Path.Combine(_nativeStorage.Path,"artifacts","test-results","endpoint-fixtures") : FixtureRoot;
            Root = Path.Combine(_ownedFixtureRoot, lane);

            DatasetVersion = prepared.DpoSetup.Address.DatasetVersion;
            SubjectSlug = "csharp-documentation-quality-fixture";
            CorpusVersion = prepared.QualityCorpus.Corpus.CorpusVersion;
            ModelIdSlug = prepared.StartRequest.ModelIdSlug;
            ModelVersion = prepared.StartRequest.ModelVersion;
            ReadyFilePath = Path.Combine(
                Root,
                "ready",
                "modelserver-ready.json");
            BaseCheckpoint = runtimeFixture.BaseCheckpointPath;
            TokenizerPath = runtimeFixture.TokenizerPath;
            AdapterRoot = Path.Combine(Root, "adapter");
            CacheRoot = Path.Combine(Root, "cache");
            RegistryRoot = Path.Combine(Root, "registry");
            ModelConfiguration = prepared.StartRequest.ModelConfig;
            RuntimeCompatibility = prepared.Compatibility;
            DpoAddress = prepared.DpoSetup.Address;
            DpoIdentity = prepared.DpoSetup.Identity;
            DpoRequestIdentity = prepared.DpoSetup.RequestIdentity;
            DpoPairs = prepared.DpoSetup.Pairs;
            Corpus = prepared.QualityCorpus;
            var corpusAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateReferenceCorpusAddress(
                    RepositoryRoot,
                    DomainSlug,
                    SubjectSlug,
                    CorpusVersion);
            Assert.True(corpusAddress.IsSuccess, corpusAddress.ErrorMessage);
            CorpusAddress = corpusAddress.Value!;
            ProfileJson = CreateProfileJson(
                Corpus.Manifest.ManifestSha256,
                ExpectedHeldOutCaseCount);
            Profile = ResolveProfile(ProfileJson);
            Request = CreateRequest(
                prepared.TrainingRunId,
                prepared.DpoSetup.Receipt,
                prepared.DpoSetup.Pairs);

            _environment = BuildEnvironment(
                lane,
                MaxTeacherCalls,
                useCoordinatorTeacherIdentity: true,
                seedEnvironment: seedEnvironment);

            if (Directory.Exists(Root) || File.Exists(Root))
                throw new InvalidOperationException(
                    "S03.04 endpoint recovery fixture lane already exists.");
            try
            {
                Directory.CreateDirectory(Root);
                Directory.CreateDirectory(Path.Combine(Root, "ready"));
                Directory.CreateDirectory(AdapterRoot);
                Directory.CreateDirectory(CacheRoot);
                Directory.CreateDirectory(RegistryRoot);
            }
            catch (Exception constructionError)
            {
                try
                {
                    DeleteExactDirectory(Root);
                }
                catch (Exception cleanupError)
                {
                    throw new AggregateException(
                        "S03.04 endpoint recovery fixture creation and exact-lane cleanup failed.",
                        constructionError,
                        cleanupError);
                }

                throw;
            }
        }

        public EndpointTrainingFixture(
            TinyTrainingFixtureCL runtimeFixture,
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                .StagedResumeAttempt staged,
            RecoverySeedEnvironment seedEnvironment,
            string? evaluatorManifestDigestOverride = null,
            int? generationMaxNewTokensOverride = null)
        {
            _ownsRepositoryArtifacts = false;
            MaxTeacherCalls = staged.Staging.Evaluator.MaxTeacherCalls;
            ExpectedHeldOutCaseCount = 5;
            ExpectedTotalEvaluationCallCount = 50;
            RepositoryRoot = staged.RepositoryRoot;
            var lane = Guid.NewGuid().ToString("N");
            _nativeStorage = OperatingSystem.IsLinux() ? new TempRepositoryRoot("s04-endpoint-recovery-native-owned") : null;
            _ownedFixtureRoot = _nativeStorage is not null ? Path.Combine(_nativeStorage.Path,"artifacts","test-results","endpoint-fixtures") : FixtureRoot;
            Root = Path.Combine(_ownedFixtureRoot, lane);

            DatasetVersion = staged.DpoSetup.Address.DatasetVersion;
            SubjectSlug = "csharp-documentation-quality-fixture";
            CorpusVersion = staged.QualityCorpus.Corpus.CorpusVersion;
            ModelIdSlug = staged.StartRequest.ModelIdSlug;
            ModelVersion = staged.StartRequest.ModelVersion;
            ReadyFilePath = Path.Combine(
                Root,
                "ready",
                "modelserver-ready.json");
            BaseCheckpoint = runtimeFixture.BaseCheckpointPath;
            TokenizerPath = runtimeFixture.TokenizerPath;
            AdapterRoot = Path.Combine(Root, "adapter");
            CacheRoot = Path.Combine(Root, "cache");
            RegistryRoot = Path.Combine(Root, "registry");
            ModelConfiguration = staged.StartRequest.ModelConfig;
            RuntimeCompatibility = staged.Compatibility;
            DpoAddress = staged.DpoSetup.Address;
            DpoIdentity = staged.DpoSetup.Identity;
            DpoRequestIdentity = staged.DpoSetup.RequestIdentity;
            DpoPairs = staged.DpoSetup.Pairs;
            Corpus = staged.QualityCorpus;
            var corpusAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateReferenceCorpusAddress(
                    RepositoryRoot,
                    DomainSlug,
                    SubjectSlug,
                    CorpusVersion);
            Assert.True(corpusAddress.IsSuccess, corpusAddress.ErrorMessage);
            CorpusAddress = corpusAddress.Value!;
            ProfileJson = CreateProfileJson(
                Corpus.Manifest.ManifestSha256,
                ExpectedHeldOutCaseCount,
                generationMaxNewTokensOverride ?? 1);
            Profile = ResolveProfile(ProfileJson);
            Request = CreateRequest(
                staged.TrainingRunId,
                staged.DpoSetup.Receipt,
                staged.DpoSetup.Pairs);

            _environment = BuildEnvironment(
                lane,
                MaxTeacherCalls,
                useCoordinatorTeacherIdentity: true,
                seedEnvironment: seedEnvironment,
                evaluatorManifestDigestOverride: evaluatorManifestDigestOverride);

            if (Directory.Exists(Root) || File.Exists(Root))
                throw new InvalidOperationException(
                    "S03.04 endpoint recovery fixture lane already exists.");
            try
            {
                Directory.CreateDirectory(Root);
                Directory.CreateDirectory(Path.Combine(Root, "ready"));
                Directory.CreateDirectory(AdapterRoot);
                Directory.CreateDirectory(CacheRoot);
                Directory.CreateDirectory(RegistryRoot);
            }
            catch (Exception constructionError)
            {
                try
                {
                    DeleteExactDirectory(Root);
                }
                catch (Exception cleanupError)
                {
                    throw new AggregateException(
                        "S03.04 endpoint recovery fixture creation and exact-lane cleanup failed.",
                        constructionError,
                        cleanupError);
                }

                throw;
            }
        }

        public const string DomainSlug =
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug;
        public const string DesignPatternSlug =
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug;
        public int MaxTeacherCalls { get; }
        public int ExpectedHeldOutCaseCount { get; }
        public int ExpectedTotalEvaluationCallCount { get; }
        public string Root { get; }
        public string RepositoryRoot { get; }
        public string DatasetVersion { get; }
        public string SubjectSlug { get; }
        public string CorpusVersion { get; }
        public string ModelIdSlug { get; }
        public string ModelVersion { get; }
        public string ReadyFilePath { get; }
        public string BaseCheckpoint { get; }
        public string TokenizerPath { get; }
        public string AdapterRoot { get; }
        public string CacheRoot { get; }
        public string RegistryRoot { get; }
        public ModelConfig ModelConfiguration { get; }
        public RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility { get; }
        public RepositoryOwnedDpoDatasetAddress DpoAddress { get; }
        public RepositoryOwnedReferenceCorpusAddress CorpusAddress { get; }
        public RepositoryOwnedDpoDatasetManifestIdentity DpoIdentity { get; }
        public RepositoryOwnedDpoDatasetRequestIdentity DpoRequestIdentity { get; private set; } = null!;
        public IReadOnlyList<ContinuousLearning.DpoPair> DpoPairs { get; private set; } = [];
        public RepositoryOwnedReferenceCorpusOpenResult Corpus { get; }
        public string ProfileJson { get; }
        public CodeDocumentationSavedModelAssessmentProfile Profile { get; }
        public CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest Request { get; }
        public string ModelLaneRoot => Path.Combine(
            RepositoryRoot,
            "training-artifacts",
            "models",
            DomainSlug,
            DesignPatternSlug,
            ModelIdSlug,
            ModelVersion);

        public string? ReadEnvironment(string name)
            => _environment.TryGetValue(name, out var value) ? value : null;

        internal CodeDocumentationOneLessonSelectorV1 OrdinarySelector { get; private set; } = null!;
        internal string OrdinaryHistory { get; private set; } = "";
        internal string OrdinaryBaseline { get; private set; } = "";
        internal string OrdinaryClaim { get; private set; } = "";
        internal string OrdinaryTraining => Path.Combine(Root, "ordinary-train.jsonl");
        internal CodeDocumentationOneLessonGrantV1 OrdinaryGrant { get; private set; } = null!;

        internal CodeDocumentationOneLessonTeachingCurriculumV2 SealGenuineTeaching(string fault = "clean", int count = 4, string? savedAnswer = null)
            => CodeDocumentationOneLessonSourceSelectionTests.GenuineTeaching(RepositoryRoot, Corpus.Corpus, fault, count, savedAnswer,
                fixtureNamespace: ModelIdSlug, registerOwnedAddress: RegisterOwnedTeachingCorpus);

        internal CodeDocumentationOneLessonTeachingCurriculumV2 SealGenuineNativePreparation(string? expandedTask = null)
            => CodeDocumentationOneLessonSourceSelectionTests.GenuineNativePreparation(RepositoryRoot,
                Path.GetFileName(Root), RegisterOwnedNativePreparationCorpus, expandedTask);

        internal CodeDocumentationOneLessonTeachingCurriculumV2 SealGenuineProtectedNativeCapture()
            => CodeDocumentationOneLessonSourceSelectionTests.GenuineNativePreparation(RepositoryRoot,
                Path.GetFileName(Root), RegisterOwnedNativePreparationCorpus, protectedProcessStrings: true);

        private void RegisterOwnedNativePreparationCorpus(RepositoryOwnedReferenceCorpusAddress address)
        {
            var lane = Path.GetFileName(Root);
            Assert.True(_ownsRepositoryArtifacts); Assert.Equal(32, lane.Length);
            Assert.True(lane.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'));
            Assert.Contains(address.CorpusVersion, new[] { "np7-" + lane, "np2-" + lane });
            var expected = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(RepositoryRoot, "bug", "np", address.CorpusVersion);
            Assert.True(expected.IsSuccess, expected.ErrorCode); Assert.NotNull(expected.Value);
            Assert.Equal(NormalizePath(expected.Value!.CorpusRoot), NormalizePath(address.CorpusRoot));
            Assert.Equal(NormalizePath(RepositoryRoot), NormalizePath(address.RepositoryRoot));
            Assert.False(_ownedTeachingCorpora.Any(a => NormalizePath(a.CorpusRoot) == NormalizePath(address.CorpusRoot)));
            Assert.False(Directory.Exists(address.CorpusRoot)); Assert.False(File.Exists(address.CorpusRoot));
            _ownedTeachingCorpora.Add(address);
        }

        private void RegisterOwnedTeachingCorpus(RepositoryOwnedReferenceCorpusAddress address)
        {
            // A shared Windows checkout may contain real corpora: ownership is only these two fresh addresses in this fixture's lane.
            Assert.True(_ownsRepositoryArtifacts);
            var rawSelectedContext = address.CorpusVersion == "raw-selected-context-v7-" + ModelIdSlug ||
                address.CorpusVersion == "raw-selected-context-v2-" + ModelIdSlug;
            if (!rawSelectedContext) Assert.Contains(address.CorpusVersion, new[] { "exclusion-reference-v7-" + ModelIdSlug, "exclusion-reference-v2-" + ModelIdSlug });
            var expected = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(RepositoryRoot,
                rawSelectedContext ? CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug : "bug", "learning-unit", address.CorpusVersion);
            Assert.True(expected.IsSuccess, expected.ErrorCode); Assert.NotNull(expected.Value);
            Assert.Equal(NormalizePath(expected.Value!.CorpusRoot), NormalizePath(address.CorpusRoot));
            Assert.Equal(NormalizePath(RepositoryRoot), NormalizePath(address.RepositoryRoot));
            if (!_ownedTeachingCorpora.Any(a => NormalizePath(a.CorpusRoot) == NormalizePath(address.CorpusRoot)))
            {
                // Never adopt or clean a pre-existing address on seal conflict, even inside a coincidentally named synthetic lane.
                Assert.False(Directory.Exists(address.CorpusRoot)); Assert.False(File.Exists(address.CorpusRoot));
                _ownedTeachingCorpora.Add(address);
            }
        }

        internal IEnumerable<string> EnumerateOwnedOrdinaryFiles()
        {
            // Inventory all created material for this synthetic owner; never recurse through the checkout itself or another fixture's lane.
            var roots = new List<string> { Root, DpoAddress.DatasetRoot, CorpusAddress.CorpusRoot, ModelLaneRoot };
            roots.AddRange(_ownedContextHistory.Select(a => a.DatasetRoot));
            if (ParentDpoAddress is not null) roots.Add(ParentDpoAddress.DatasetRoot);
            if (OrdinaryHistory.Length != 0) roots.Add(Path.GetDirectoryName(OrdinaryHistory)!);
            if (OrdinaryBaseline.Length != 0) roots.Add(Path.GetDirectoryName(OrdinaryBaseline)!);
            roots.AddRange(_ownedTeachingCorpora.Select(a => a.CorpusRoot));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var repository = NormalizePath(RepositoryRoot);
            var files = new HashSet<string>(comparer);
            foreach (var root in roots.Select(NormalizePath).Distinct(comparer))
            {
                Assert.False(string.Equals(root, repository, comparison));
                Assert.True(root.StartsWith(repository + Path.DirectorySeparatorChar, comparison));
                if (Directory.Exists(root)) // Absent owned roots contribute no files; later creation/deletion is still caught by key comparison.
                    foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)) files.Add(file);
            }
            return files.Order(comparer);
        }

        internal void ChangeOrdinaryTrainingRunBinding()
        {
            var wire=JsonNode.Parse(_environment[CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable])!;
            wire["Grants"]![0]!["TrainingRunId"]=OrdinaryGrant.TrainingRunId+"-changed";
            _environment[CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable]=IndependentJcs(wire);
            var changed=ContinuousLearningModelServerStartup.Create(ReadEnvironment).OneLessonGrantCatalog.Resolve(OrdinarySelector);
            Assert.True(changed.IsSuccess,changed.ErrorCode); Assert.NotEqual(OrdinaryGrant.TrainingRunId,changed.Value!.TrainingRunId);
        }

        internal void ProvisionOrdinaryV2(int repeats, string? objectiveId = null, bool teachingCurriculum = false,
            CodeDocumentationOneLessonTeachingCurriculumV2? genuineTeachingCurriculum = null, Stream? genuineParentWeights = null,
            Action<LlmModel>? configureGenuineParent = null, int? syntheticContextHistoryPairs = null, int contextHistoryPadding = 0,
            string? contextHistoryFault = null, string? contextHistoryChosenSuffix = null) // Forward optional overflow content before the genuine owner seals the historical pair.
        {
            Assert.False(teachingCurriculum && genuineTeachingCurriculum is not null);
            SeedAcceptedParentCheckpoint(includeServerLora: true, genuineParentWeights: genuineParentWeights,
                configureGenuineParent: configureGenuineParent);
            static T Require<T>(OperationResult<T> result)
            { Assert.True(result.IsSuccess, result.ErrorCode + ":" + result.ErrorMessage); return result.Value!; }
            var modelRoot = Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(RepositoryRoot));
            var train = OrdinaryTraining; var eval = Path.Combine(Root, "ordinary-eval.jsonl");
            foreach (var (path, role) in new[] { (train, "train"), (eval, "eval") })
            {
                var rows = Enumerable.Range(0, 50).Select(i => JsonSerializer.Serialize(new {
                    example_id = role + i.ToString(CultureInfo.InvariantCulture), code_text = "C# selected code " + i.ToString("D3",CultureInfo.InvariantCulture),
                    symbol_id = "ordinary" + (char)('a'+i/26) + (char)('a'+i%26),
                    chosen = "selected documentation " + i, rejected = "[WRONG: missing selected documentation " + i + "]",
                    expected_unit_id = OrchestrationRoutingCurriculum.DocumentationDotNetUnitId }));
                File.WriteAllText(path, string.Join("\n", rows) + "\n", new UTF8Encoding(false));
                if (OperatingSystem.IsLinux()) File.SetUnixFileMode(path, SecureLocalFileBoundary.OwnerFileMode);
            }
            var batch = new CodeDocumentationBatchFileRunner();
            var profile = Require(batch.DescribeProfileSelection(modelRoot,
                Path.GetRelativePath(RepositoryRoot, train).Replace('\\','/'), Path.GetRelativePath(RepositoryRoot, eval).Replace('\\','/')));
            using var loaded = Require(new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(RepositoryRoot,
                DomainSlug, DesignPatternSlug, ModelIdSlug, ModelVersion, RuntimeCompatibility));
            var parent = loaded.Manifest;
            var checkpoint = Require(CodeDocumentationOneLessonCheckpointSelection.Create(
                new(parent.Step, parent.Domain, parent.DesignPattern, parent.ModelId, parent.Version,
                    parent.ManifestSha256, loaded.WeightsSha256, loaded.WeightsByteLength), new("ordinary-candidate", Sha256Hex("ordinary-candidate"))));
            var dpo = Require(RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(DpoAddress, DpoRequestIdentity));
            var sourceRows = new List<CodeDocumentationOneLessonSourceV2>();
            if (syntheticContextHistoryPairs is > 0)
            {
                var history = SealSyntheticContextHistorySource(syntheticContextHistoryPairs.Value, contextHistoryPadding, contextHistoryFault, contextHistoryChosenSuffix);
                var historyRead = Require(RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(history, DpoRequestIdentity));
                sourceRows.Add(new("history-source", "TrainingPairs", DomainSlug, DesignPatternSlug, history.DatasetVersion,
                    historyRead.ManifestFileSha256, historyRead.ManifestFileByteLength, historyRead.Manifest.ManifestSha256));
            }
            sourceRows.Add(new("ordinary-source", "TrainingPairs", DomainSlug, DesignPatternSlug, DatasetVersion,
                dpo.ManifestFileSha256, dpo.ManifestFileByteLength, dpo.Manifest.ManifestSha256));
            var source = Require(CodeDocumentationOneLessonSourceSelection.Create(
                sourceRows,
                [new(dpo.Manifest.OrderedPairs.Single().PairId, "ordinary-source")],
                genuineTeachingCurriculum ?? (teachingCurriculum ? CodeDocumentationOneLessonSourceSelectionTests.Curriculum() : null)));
            var actualCorpus = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(CorpusAddress));
            var cases = actualCorpus.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut)
                .OrderBy(e => e.EntryId,StringComparer.Ordinal).Select(e => e.EntryId).ToArray();
            var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(new("ordinary-corpus", "ClosedEvaluation",
                DomainSlug, SubjectSlug, CorpusVersion, actualCorpus.ManifestFileSha256, actualCorpus.Corpus.CorpusSha256[7..],
                actualCorpus.ManifestFileByteLength, actualCorpus.Manifest.PayloadSha256, actualCorpus.Manifest.PayloadByteLength), cases));
            var plan = Require(CodeDocumentationOneLessonEvaluationPlanSelection.Create(corpus, "ordinary-plan", "ordinary-evaluator", repeats, checked(cases.Length * repeats)));
            var configuration = ContinuousLearningModelServerStartup.Create(ReadEnvironment).TeacherConfiguration;
            var g = Profile.GenerationSettings; var p = LocalQwenOllamaInferenceProfileAuthority.EvaluationDescriptor;
            var tokenizer = BpeTokenizer.Load(TokenizerPath);
            Assert.All(actualCorpus.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut), e =>
            {
                var tokens = tokenizer.Encode(e.OwnerInput).Count;
                Assert.True(tokens > 0 && (long)tokens + g.MaxNewTokens - 1 <= ModelConfiguration.ContextLen,
                    $"Full ordinary case {e.EntryId} needs {tokens}+{g.MaxNewTokens}-1 tokens; selected context={ModelConfiguration.ContextLen}.");
            });
            var noise = new MeasurementNoisePolicyConfig(repeats, 2);
            var evaluator = Require(CodeDocumentationOneLessonEvaluatorSelection.Create(corpus, plan,
                new("ordinary-evaluator", "v2", Sha256Hex("ordinary-config"), Sha256Hex("ordinary-condition"),
                    CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(g,
                        actualCorpus.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).OrderBy(e => e.EntryId,StringComparer.Ordinal).ToArray()),
                    noise.IdentitySha256, CodeDocumentationSavedModelAssessmentCalibrationService.BuildCurrentScorerIdentity(DomainSlug,configuration.ModelId,configuration.ExpectedOllamaManifestDigest),
                    new(g.MaxNewTokens,g.Temperature.ToString("R",CultureInfo.InvariantCulture),g.TopK,g.TopP.ToString("R",CultureInfo.InvariantCulture),g.Seed,g.MaxHeldOutCases,g.MaxAnswerChars),
                    new(repeats,"2"), new(p.ProfileId,p.Purpose,p.FormatIdentity,p.RequestFormatMode.ToString(),p.NumContext,
                        p.Temperature.ToString("R",CultureInfo.InvariantCulture),p.Seed,p.Think!.Value,p.NumPredict!.Value,
                        p.RequestTimeout.TotalSeconds.ToString("R",CultureInfo.InvariantCulture),p.Evidence.ConfigurationSha256),
                    new(configuration.ModelId,configuration.ExpectedOllamaManifestDigest,configuration.ExpectedOllamaModelLayerDigest,
                        configuration.MaxTeacherCalls,configuration.LocalRetentionPolicyRef,configuration.OutputLicenseRef,configuration.DpoMaxStepCount))));
            var lane = Path.GetFileName(Root); OrdinarySelector = new(objectiveId ?? "ordinary-" + lane, "ordinary-selector-" + lane);
            OrdinaryGrant = new("ordinary-grant", "v2", OrdinarySelector.GrantSelector, OrdinarySelector.ObjectiveId,
                "ordinary-run-" + lane, checkpoint.Candidate.CandidateSha256, parent.ManifestSha256, loaded.WeightsSha256,
                evaluator.Evaluator.ConfigurationSha256, evaluator.Evaluator.ExamConditionSha256)
                { FormatVersion = 2, SourceSelection = source, CheckpointSelection = checkpoint, CorpusSelection = corpus,
                  EvaluationPlanSelection = plan, EvaluatorSelection = evaluator, ProfileSelection = profile,
                  DescriptorsV2 = new(2, source.SourceSelectionSha256, checkpoint.ParentCheckpointSha256, checkpoint.CandidateSelectionSha256,
                    corpus.CorpusDescriptorSha256, plan.PlanSha256, cases.Length, repeats, evaluator.EvaluatorProfileSha256, profile.ProfileSelectionSha256) };
            var descriptor = JsonSerializer.SerializeToNode(new { SchemaVersion=2, source.Sources, source.PairSelections,
                ParentCheckpoint=checkpoint.Parent, checkpoint.Candidate, corpus.Corpus,
                EvaluationPlan=new {plan.PlanId,plan.PlanSha256,plan.CorpusId,plan.EvaluatorId,plan.CaseIds,plan.RepeatCount,plan.ExpectedEvaluationCallCount},
                Evaluator=evaluator.Evaluator, ProfileSelection=profile.Snapshot })!;
            if (teachingCurriculum || genuineTeachingCurriculum is not null)
            {
                // Independent literal data commits to this protected startup value through the real decoder/catalog.
                descriptor["TeachingCurriculum"] = genuineTeachingCurriculum is null
                    ? JsonNode.Parse(CodeDocumentationOneLessonSourceSelectionTests.CurriculumLiteral)
                    : JsonSerializer.SerializeToNode(genuineTeachingCurriculum);
                var preimage = new JsonObject { ["Sources"] = descriptor["Sources"]!.DeepClone(),
                    ["PairSelections"] = descriptor["PairSelections"]!.DeepClone(),
                    ["TeachingCurriculum"] = descriptor["TeachingCurriculum"]!.DeepClone() };
                Assert.Equal(IndependentHash("xiigen-s04-source-selection-v2\n" + IndependentJcs(preimage)), source.SourceSelectionSha256);
                if (teachingCurriculum) AssertTeachingDirectoryAbsent();
            }
            var grantNode = JsonSerializer.SerializeToNode(OrdinaryGrant)!; grantNode["DescriptorsV2"] = descriptor;
            var wire = IndependentJcs(new JsonObject { ["SchemaVersion"]=2,["Grants"]=new JsonArray(grantNode) });
            _environment[CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable]=wire;
            var catalog=ContinuousLearningModelServerStartup.Create(ReadEnvironment).OneLessonGrantCatalog;
            OrdinaryGrant=Require(catalog.Resolve(OrdinarySelector));
            Assert.NotNull(OrdinaryGrant.ProfileSelection);
            if (teachingCurriculum)
            {
                Assert.Equal(source.SourceSelectionSha256, OrdinaryGrant.SourceSelection!.SourceSelectionSha256);
                Assert.Equal(CodeDocumentationOneLessonSourceSelectionTests.CurriculumLiteral,
                    CodeDocumentationOneLessonSourceSelectionTests.TeachingJson(OrdinaryGrant.SourceSelection.TeachingCurriculum!));
                AssertTeachingDirectoryAbsent();
            }
            OrdinaryHistory = Path.Combine(modelRoot.ModelArtifactRoot,"code-documentation-one-lesson-sessions",Sha256Hex(OrdinarySelector.ObjectiveId),"session.jsonl");
            var originalAddress=Require(RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(RepositoryRoot,
                DomainSlug,DesignPatternSlug,ModelIdSlug,ModelVersion,Sha256Hex(OrdinarySelector.ObjectiveId)));
            OrdinaryBaseline=originalAddress.BaselinePath; OrdinaryClaim=originalAddress.ClaimPath;
        }

        internal void AssertTeachingDirectoryAbsent()
        {
            foreach (var source in CodeDocumentationOneLessonSourceSelectionTests.Curriculum().Sources)
            {
                var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                    RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion);
                Assert.True(address.IsSuccess, address.ErrorCode);
                Assert.False(Directory.Exists(Path.GetDirectoryName(address.Value!.ManifestPath)));
            }
        }

        public string[] ReadyFileArguments =>
        [
            Program.ReadyFileArgumentName,
            ReadyFilePath,
        ];

        public IDisposable ApplyEnvironment()
        {
            var values = _environment.ToDictionary(
                static pair => pair.Key,
                static pair => (string?)pair.Value,
                StringComparer.Ordinal);
            values[CodeDocumentationSavedModelAssessmentProfileResolver
                .EnvironmentName] = ProfileJson;
            return new EnvironmentScope(values);
        }

        public CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest
            CreateRouteRecoveryRequest()
            => Request with
            {
                RoutingRun = Request.RoutingRun with
                {
                    RepositoryOwnedDpoPairs = DpoPairs.ToArray(),
                },
            };

        public RepositoryOwnedModelTrainingAttemptAddress
            CreateTrainingAttemptAddress()
        {
            var address = RepositoryOwnedTrainingArtifactLayout
                .CreateModelTrainingAttemptAddress(
                    RepositoryRoot,
                    DomainSlug,
                    DesignPatternSlug,
                    ModelIdSlug,
                    ModelVersion,
                    Request.TrainingRunId);
            Assert.True(address.IsSuccess, address.ErrorMessage);
            return address.Value!;
        }

        public void SeedAcceptedParentCheckpoint(bool includeServerLora = false, Stream? genuineParentWeights = null,
            Action<LlmModel>? configureGenuineParent = null)
        {
            if (!_ownsRepositoryArtifacts)
                throw new InvalidOperationException(
                    "Cannot seed a parent checkpoint into a borrowed repository lane.");
            Assert.Null(ParentDpoAddress);
            var setup =
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                    .SealRepositoryOwnedDpo(
                        RepositoryRoot,
                        "s03-12-parent-" + DatasetVersion,
                        "s03-12-parent-" + DatasetVersion);
            ParentDpoAddress = setup.Address;
            ParentDpoIdentity = setup.Identity;
            var parentModel = new LlmModel(ModelConfiguration);
            if (includeServerLora)
            {
                var rootSeed = ContinuousLearningModelServerConfigurationLoader.ReadRootInitializationSeed(ReadEnvironment, required: true);
                Assert.NotNull(rootSeed);
                var seed = CodeDocumentationDpoSeedAuthorityResolver.Resolve(DomainSlug, rootSeed!.Value);
                Assert.True(seed.IsSuccess,seed.ErrorCode);
                LoraApplicator.ApplySeeded(parentModel,IncrementalTrainer.DefaultConfig,seed.Value!.RootInitializationSeed,
                    seed.Value.PolicyDropoutStreamDomain);
            }
            if (genuineParentWeights is not null) parentModel.LoadCheckpoint(genuineParentWeights);
            Assert.False(genuineParentWeights is not null && configureGenuineParent is not null); // Copied exact parent bytes and a new configured parent are distinct fixture inputs.
            configureGenuineParent?.Invoke(parentModel); // Configure only the genuine test model, after LoRA setup and before the existing checkpoint owner seals it.
            var parent =
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
                    .SealModelCheckpoint(
                        RepositoryRoot,
                        parentModel,
                        ModelConfiguration,
                        RuntimeCompatibility,
                        ParentDpoIdentity,
                        step: 1,
                        parent: null,
                        coldReason: "test parent before route recovery",
                        modelIdSlug: ModelIdSlug,
                        modelVersion: ModelVersion);
            var parentAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    RepositoryRoot,
                    DomainSlug,
                    DesignPatternSlug,
                    ModelIdSlug,
                    ModelVersion,
                    step: 1);
            Assert.True(parentAddress.IsSuccess, parentAddress.ErrorMessage);
            Assert.Equal(
                parentAddress.Value!.CheckpointRootRelativePath,
                parent.RepositoryRelativeCheckpointDirectory);
        }

        private RepositoryOwnedDpoDatasetAddress? ParentDpoAddress { get; set; }
        private RepositoryOwnedDpoDatasetManifestIdentity ParentDpoIdentity { get; set; } = null!;

        public void Dispose()
        {
            if (_disposed)
                return;
            var cleanupErrors = new List<Exception>();
            void Release(Action release)
            {
                try { release(); }
                catch (Exception error) { cleanupErrors.Add(error); } // One failed release cannot strand the remaining acquired owners.
            }
            try
            {
                if (Root is not null && (!_ownsRepositoryArtifacts || _owningFixtureLaneStarted)) Release(() => DeleteExactDirectory(Root));
                if (_ownsRepositoryArtifacts)
                {
                    if (DpoAddress is not null) Release(() => DeleteExactDirectory(DpoAddress.DatasetRoot)); // An address assigned before seal failure remains an acquired exact lane.
                    if (CorpusAddress is not null) Release(() => DeleteExactDirectory(CorpusAddress.CorpusRoot));
                    foreach (var teaching in _ownedTeachingCorpora) Release(() => DeleteExactDirectory(teaching.CorpusRoot));
                    foreach (var history in _ownedContextHistory) Release(() => DeleteExactDirectory(history.DatasetRoot));
                    if (ParentDpoAddress is not null) Release(() => DeleteExactDirectory(ParentDpoAddress.DatasetRoot));
                    if (RepositoryRoot is not null && ModelIdSlug is not null && ModelVersion is not null) Release(() => DeleteExactDirectory(ModelLaneRoot));
                    if (OrdinaryHistory.Length != 0) Release(() =>
                    {
                        var directory=Path.GetDirectoryName(OrdinaryHistory)!;
                        var ownedRoot=Path.GetFullPath(Path.Combine(RepositoryRoot,"training-artifacts","models","code-documentation-one-lesson-sessions"));
                        Assert.Equal(Path.Combine(ownedRoot,Sha256Hex(OrdinarySelector.ObjectiveId)),directory);
                        if(Directory.Exists(directory)) Directory.Delete(directory,true);
                    });
                }
            }
            finally
            {
                Release(() =>
                {
                    if (_constructorFailureProbe is not null) _constructorFailureProbe.BootstrapReleaseAttempts++;
                    _nativeStorage?.Dispose(); // Always try the acquired bootstrap, even if an exact lane release failed.
                });
            }
            if (cleanupErrors.Count != 0) throw new AggregateException("Endpoint fixture owned cleanup failed.", cleanupErrors);
            _disposed = true;
        }

        public IReadOnlyDictionary<string, string> SnapshotOwnedFiles()
            => SnapshotFiles(("fixture", Root));

        public IReadOnlyDictionary<string, string> SnapshotSealedInputs()
            => SnapshotFiles(
                ("dpo", DpoAddress.DatasetRoot),
                ("corpus", CorpusAddress.CorpusRoot));

        public void AssertSnapshotUnchanged(
            IReadOnlyDictionary<string, string> before,
            IReadOnlyDictionary<string, string> after,
            string scope)
        {
            Assert.Equal(before.Keys, after.Keys);
            foreach (var (path, hash) in before)
            {
                Assert.True(after.TryGetValue(path, out var afterHash),
                    $"{scope}: missing {path}");
                Assert.Equal(hash, afterHash);
            }
        }

        public void AssertActiveOwnerLockPresence(
            IReadOnlyDictionary<string, string> snapshot,
            string scope)
        {
            foreach (var key in new[]
                     {
                         "fixture/adapter/.continuous-learning.adapter-owner.lock",
                         "fixture/registry/.continuous-learning.registry-owner.lock",
                     })
            {
                Assert.True(
                    snapshot.TryGetValue(key, out var marker),
                    $"{scope}: missing active owner lock {key}");
                Assert.Equal(
                    "operational-owner-lock:present",
                    marker);
            }

            Assert.DoesNotContain(
                "fixture/cache/.semantic-compiler.owner.lock",
                snapshot.Keys);
        }

        public void AssertSnapshotSelectorContract()
        {
            var actual = SnapshotOwnedFiles();
            Assert.Contains(
                "fixture/adapter/.continuous-learning.adapter-owner.lock",
                actual.Keys);
            Assert.Contains(
                "fixture/registry/.continuous-learning.registry-owner.lock",
                actual.Keys);
            Assert.DoesNotContain(
                "fixture/cache/.semantic-compiler.owner.lock",
                actual.Keys);
            Assert.True(
                IsOwnedOperationalLock(
                    "fixture",
                    "adapter/.continuous-learning.adapter-owner.lock"));
            Assert.True(
                IsOwnedOperationalLock(
                    "fixture",
                    "registry/.continuous-learning.registry-owner.lock"));
            Assert.False(
                IsOwnedOperationalLock(
                    "fixture",
                    "cache/.semantic-compiler.owner.lock"));
            Assert.False(
                IsOwnedOperationalLock(
                    "fixture",
                    "adapter/nested/.continuous-learning.adapter-owner.lock"));
            Assert.False(
                IsOwnedOperationalLock(
                    "fixture-other",
                    "adapter/.continuous-learning.adapter-owner.lock"));
            Assert.False(
                IsOwnedOperationalLock(
                    "dpo",
                    "adapter/.continuous-learning.adapter-owner.lock"));
            Assert.False(
                IsOwnedOperationalLock(
                    "corpus",
                    "registry/.continuous-learning.registry-owner.lock"));

            var probeRoot = Path.Combine(Root, "snapshot-selector-contract");
            var ordinaryRoot = Path.Combine(probeRoot, "ordinary");
            var cacheProbeRoot = Path.Combine(probeRoot, "cache-probe");
            var presentLockRoot = Path.Combine(probeRoot, "present-lock");
            var absentLockRoot = Path.Combine(probeRoot, "absent-lock");
            var elsewhereRoot = Path.Combine(probeRoot, "elsewhere");
            try
            {
                Directory.CreateDirectory(ordinaryRoot);
                var ordinaryFile = Path.Combine(ordinaryRoot, "ordinary.bin");
                File.WriteAllText(ordinaryFile, "before");
                var ordinaryBefore = SnapshotFiles(("fixture", ordinaryRoot));
                File.WriteAllText(ordinaryFile, "after");
                var ordinaryAfter = SnapshotFiles(("fixture", ordinaryRoot));
                Assert.NotEqual(
                    ordinaryBefore["fixture/ordinary.bin"],
                    ordinaryAfter["fixture/ordinary.bin"]);

                var cacheProbeFile = Path.Combine(
                    cacheProbeRoot,
                    "cache",
                    ".semantic-compiler.owner.lock");
                Directory.CreateDirectory(
                    Path.GetDirectoryName(cacheProbeFile)!);
                File.WriteAllText(cacheProbeFile, "before");
                var cacheBefore = SnapshotFiles(("fixture", cacheProbeRoot));
                File.WriteAllText(cacheProbeFile, "after");
                var cacheAfter = SnapshotFiles(("fixture", cacheProbeRoot));
                const string cacheProbeKey =
                    "fixture/cache/.semantic-compiler.owner.lock";
                Assert.NotEqual(
                    SnapshotOwnedOperationalLock(),
                    cacheBefore[cacheProbeKey]);
                Assert.NotEqual(
                    cacheBefore[cacheProbeKey],
                    cacheAfter[cacheProbeKey]);

                var presentLock = Path.Combine(
                    presentLockRoot,
                    "adapter",
                    ".continuous-learning.adapter-owner.lock");
                Directory.CreateDirectory(Path.GetDirectoryName(presentLock)!);
                File.WriteAllText(presentLock, "owned");
                var present = SnapshotFiles(("fixture", presentLockRoot));
                Assert.Contains(
                    "fixture/adapter/.continuous-learning.adapter-owner.lock",
                    present.Keys);
                Directory.CreateDirectory(absentLockRoot);
                var absent = SnapshotFiles(("fixture", absentLockRoot));
                Assert.DoesNotContain(
                    "fixture/adapter/.continuous-learning.adapter-owner.lock",
                    absent.Keys);

                var sameNamedElsewhere = Path.Combine(
                    elsewhereRoot,
                    "adapter",
                    ".continuous-learning.adapter-owner.lock");
                Directory.CreateDirectory(Path.GetDirectoryName(sameNamedElsewhere)!);
                File.WriteAllText(sameNamedElsewhere, "before");
                var elsewhereBefore = SnapshotFiles(("dpo", elsewhereRoot));
                var corpusBefore = SnapshotFiles(("corpus", elsewhereRoot));
                File.WriteAllText(sameNamedElsewhere, "after");
                var elsewhereAfter = SnapshotFiles(("dpo", elsewhereRoot));
                var corpusAfter = SnapshotFiles(("corpus", elsewhereRoot));
                Assert.NotEqual(
                    elsewhereBefore["dpo/adapter/.continuous-learning.adapter-owner.lock"],
                    elsewhereAfter["dpo/adapter/.continuous-learning.adapter-owner.lock"]);
                Assert.NotEqual(
                    corpusBefore["corpus/adapter/.continuous-learning.adapter-owner.lock"],
                    corpusAfter["corpus/adapter/.continuous-learning.adapter-owner.lock"]);
            }
            finally
            {
                if (Directory.Exists(probeRoot))
                    Directory.Delete(probeRoot, recursive: true);
            }
        }

        internal (string[] Directories, Dictionary<string, (byte[] Bytes, long LastWriteTicks)> Files) SnapshotAcceptedParentLane(bool expectOriginalAbsent = false)
        {
            Assert.True(Directory.Exists(ModelLaneRoot));
            if (expectOriginalAbsent)
            {
                // Provisioning releases its checkpoint leases before this snapshot; ORIGINAL is created only by the later HTTP request.
                Assert.False(Directory.Exists(Path.GetDirectoryName(OrdinaryBaseline)!));
                Assert.False(Directory.Exists(Path.GetDirectoryName(Path.GetDirectoryName(OrdinaryBaseline)!)!));
                Assert.False(File.Exists(OrdinaryBaseline));
                Assert.False(File.Exists(OrdinaryClaim));
            }
            var directories = Directory.EnumerateDirectories(ModelLaneRoot, "*", SearchOption.AllDirectories)
                .Prepend(ModelLaneRoot).Order(StringComparer.Ordinal).ToArray();
            var files = Directory.EnumerateFiles(ModelLaneRoot, "*", SearchOption.AllDirectories)
                .ToDictionary(path => path, path => (File.ReadAllBytes(path), File.GetLastWriteTimeUtc(path).Ticks), StringComparer.Ordinal);
            return (directories, files);
        }

        internal void AssertAcceptedParentLaneUnchanged((string[] Directories, Dictionary<string, (byte[] Bytes, long LastWriteTicks)> Files) before, bool expectOriginalCreated = true) // Early execution-context refusal permits no ORIGINAL additions; later stages retain the existing exact additions.
        {
            var after = SnapshotAcceptedParentLane();
            var original = RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(
                RepositoryRoot, DomainSlug, DesignPatternSlug, ModelIdSlug, ModelVersion, Sha256Hex(OrdinarySelector.ObjectiveId));
            Assert.True(original.IsSuccess, original.ErrorCode);
            var address = original.Value!;
            Assert.Equal(OrdinaryBaseline, address.BaselinePath);
            Assert.Equal(OrdinaryClaim, address.ClaimPath);
            // The later protected/callback paths produce ORIGINAL; replaced DI refuses before this family exists.
            var originalDirectories = expectOriginalCreated ? new[] { Path.GetDirectoryName(address.BaselineRoot)!, address.BaselineRoot } : []; // Require exactly two custody additions only for evaluated branches.
            var originalFiles = expectOriginalCreated ? new[] { address.BaselinePath, address.ClaimPath, address.LockPath } : []; // The early branch must preserve the exact original parent file set with zero additions.
            Assert.All(originalDirectories, path => Assert.DoesNotContain(path, before.Directories));
            Assert.All(originalFiles, path => Assert.False(before.Files.ContainsKey(path)));
            Assert.Equal(before.Directories.Concat(originalDirectories).Order(StringComparer.Ordinal), after.Directories);
            Assert.Equal(before.Files.Keys.Concat(originalFiles).Order(StringComparer.Ordinal), after.Files.Keys.Order(StringComparer.Ordinal));
            foreach (var path in before.Files.Keys)
            {
                Assert.Equal(before.Files[path].Bytes, after.Files[path].Bytes);
                Assert.Equal(before.Files[path].LastWriteTicks, after.Files[path].LastWriteTicks);
            }
            // Every pre-existing file and directory remains identical; only the exact ORIGINAL additions are admitted above.
            // Both run identities remain free of a new training attempt; directory equality covers every extra checkpoint.
            foreach (var runId in new[] { Request.TrainingRunId, OrdinaryGrant.TrainingRunId })
            {
                var attempt = RepositoryOwnedTrainingArtifactLayout.CreateModelTrainingAttemptAddress(
                    RepositoryRoot, DomainSlug, DesignPatternSlug, ModelIdSlug, ModelVersion, runId);
                Assert.True(attempt.IsSuccess, attempt.ErrorCode);
                Assert.False(Directory.Exists(attempt.Value!.AttemptRoot));
            }
            var child = RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                RepositoryRoot, DomainSlug, DesignPatternSlug, ModelIdSlug, ModelVersion, step: 2);
            Assert.True(child.IsSuccess, child.ErrorCode);
            Assert.False(Directory.Exists(child.Value!.CheckpointRoot));
        }

        public void AssertNoNegativeTrainingOutputs()
        {
            var modelLaneRoot = Path.Combine(
                RepositoryRoot,
                "training-artifacts",
                "models",
                DomainSlug,
                DesignPatternSlug,
                ModelIdSlug,
                ModelVersion);
            Assert.False(
                Directory.Exists(modelLaneRoot),
                $"Negative endpoint fixture created model lane outputs: {modelLaneRoot}");

            var attempt = RepositoryOwnedTrainingArtifactLayout
                .CreateModelTrainingAttemptAddress(
                    RepositoryRoot,
                    DomainSlug,
                    DesignPatternSlug,
                    ModelIdSlug,
                    ModelVersion,
                    Request.TrainingRunId);
            Assert.True(attempt.IsSuccess, attempt.ErrorMessage);
            Assert.False(
                Directory.Exists(attempt.Value!.AttemptRoot),
                $"Negative endpoint fixture created a training attempt: {attempt.Value.AttemptRoot}");

            var checkpoint = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    RepositoryRoot,
                    DomainSlug,
                    DesignPatternSlug,
                    ModelIdSlug,
                    ModelVersion,
                    1);
            Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
            Assert.False(
                Directory.Exists(checkpoint.Value!.CheckpointRoot),
                $"Negative endpoint fixture created a checkpoint: {checkpoint.Value.CheckpointRoot}");
        }

        private RepositoryOwnedReferenceCorpusOpenResult SealReferenceCorpus(
            RepositoryOwnedReferenceCorpusAddress address)
        {
            var entries = new[]
            {
                CreateReferenceEntry(
                    "Document the S03.04 train-only helper.",
                    "Reference answer for the train-only helper.",
                    FrozenReferencePartition.Train,
                    "train"),
                CreateReferenceEntry(
                    "Document Add method.",
                    "Reference answer: Add returns the sum and has no side effects.",
                    FrozenReferencePartition.HeldOut,
                    "heldout-add"),
                CreateReferenceEntry(
                    "Document Subtract method.",
                    "Reference answer: Subtract returns left minus right and does not mutate inputs.",
                    FrozenReferencePartition.HeldOut,
                    "heldout-subtract"),
            };
            // Preserve the original one/two-case controls, and build actual distinct held-out cases for larger genuine grants.
            var selectedEntries = ExpectedHeldOutCaseCount == 1 ? entries[..2] : entries;
            if (ExpectedHeldOutCaseCount > 2)
                selectedEntries = entries.Concat(Enumerable.Range(3, ExpectedHeldOutCaseCount - 2).Select(ordinal =>
                    CreateReferenceEntry(
                        $"Document synthetic held-out method {ordinal}.",
                        $"Reference answer: synthetic method {ordinal} returns its input and has no side effects.",
                        FrozenReferencePartition.HeldOut,
                        $"heldout-synthetic-{ordinal}"))).ToArray();
            var corpus = FrozenReferenceCorpus.Build(
                "s0304-code-documentation-corpus",
                CorpusVersion,
                DateTimeOffset.UnixEpoch,
                selectedEntries);
            Assert.True(corpus.IsSuccess, corpus.ErrorMessage);
            var sealedManifest = RepositoryOwnedReferenceCorpusManifestSealer
                .PersistAndSeal(address, corpus.Value!);
            Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
            var opened = RepositoryOwnedReferenceCorpusManifestSealer
                .OpenSealed(address);
            Assert.True(opened.IsSuccess, opened.ErrorMessage);
            return opened.Value!;
        }

        private FrozenReferenceEntry CreateReferenceEntry(
            string ownerInput,
            string referenceAnswer,
            FrozenReferencePartition partition,
            string scope)
        {
            var provenance = PaidReferenceProvenance.Create(
                "s0304-paid-reference-fixture",
                "s0304-closed-doc-reference-fixture",
                "2026-09-15",
                "s0304-reference-request-" + scope + "-" + CorpusVersion,
                DateTimeOffset.UnixEpoch,
                FrozenReferenceEntry.ComputeNormalizedTextSha256(
                    referenceAnswer));
            Assert.True(provenance.IsSuccess, provenance.ErrorMessage);
            var entry = FrozenReferenceEntry.Create(
                ownerInput,
                referenceAnswer,
                provenance.Value!,
                DomainSlug,
                SubjectSlug,
                partition,
                "sha256:" + Sha256Hex("s0304-source-" + scope + CorpusVersion));
            Assert.True(entry.IsSuccess, entry.ErrorMessage);
            return entry.Value!;
        }

        private RepositoryOwnedDpoDatasetManifestIdentity SealDpoDataset(
            RepositoryOwnedDpoDatasetAddress address)
        {
            var identity = RepositoryOwnedDpoDatasetManifestSealer
                .CreateRequestIdentity(
                    [Sha256Hex("s0304-paid-reference-dpo-" + DatasetVersion)],
                    new RepositoryOwnedDpoOpenTeacherIdentity(
                        OpenWeightTeacherFamily.Qwen.ToString(),
                        "qwen-s0304-fixture:9b",
                        Hex('d'),
                        LegalTrainingTeacherAdmissionAuthority
                            .OllamaModelLayerSha256DigestKind,
                        Hex('c')),
                    new RepositoryOwnedDpoGenerationPolicyIdentity(
                        "s0304-generation-policy",
                        DatasetVersion,
                        Sha256Hex("s0304-generation-" + DatasetVersion)),
                    new RepositoryOwnedDpoCreatorIdentity(
                        "s0304-endpoint-tests",
                        DatasetVersion,
                        Sha256Hex("s0304-creator-" + DatasetVersion)));
            Assert.True(identity.IsSuccess, identity.ErrorMessage);
            DpoRequestIdentity = identity.Value!;
            var pair = CreateDpoPair(DpoRequestIdentity);
            DpoPairs = [pair];
            var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                address,
                identity.Value!,
                [pair]);
            Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
            return new RepositoryOwnedDpoDatasetManifestIdentity(
                address.ManifestRelativePath,
                manifest.Value!.ManifestSha256);
        }

        internal RepositoryOwnedDpoDatasetAddress SealSyntheticContextHistorySource(int count = 1, int padding = 0, string? fault = null, string? chosenSuffix = null) // Preserve complete owner content and manifest order while constructing a genuine token-overflow case.
        {
            Assert.InRange(count, 1, 2); Assert.InRange(padding, 0, 65536);
            var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(RepositoryRoot, DomainSlug, DesignPatternSlug,
                "context-history-" + Path.GetFileName(Root));
            Assert.True(address.IsSuccess, address.ErrorCode); _ownedContextHistory.Add(address.Value!);
            // Each synthetic historical authoring intent owns its request id; reusing the
            // ordinary one-pair fixture id would correctly trip the durable LEGAL guard.
            var requestIds = Enumerable.Range(0, count).Select(i =>
                "s04-context-history-" + Path.GetFileName(Root) + "-" + i.ToString(CultureInfo.InvariantCulture)).ToArray();
            var pairs = Enumerable.Range(0, count).Select(i => CreateDpoPair(DpoRequestIdentity,
                fault == "original-task" ? Corpus.Corpus.Entries.First(e => e.Partition == FrozenReferencePartition.HeldOut).OwnerInput : "Document a previously accepted context function " + i,
                "Explain the complete earlier behavior " + i + new string('p', padding) + (chosenSuffix ?? string.Empty),
                fault == "original-digest" ? Corpus.Corpus.Entries.First(e => e.Partition == FrozenReferencePartition.HeldOut).EntryId : "Incorrect earlier explanation " + i,
                teacherRequestId: requestIds[i])).ToArray();
            Assert.Equal(count, pairs.Select(p => p.TeacherProvenance!.RequestId).Distinct(StringComparer.Ordinal).Count());
            ContextHistoryCreationInputs = Array.AsReadOnly(pairs.ToArray());
            // Keep independent creation inputs indexed by the genuine schema5 identity;
            // the sealer declares canonical PairId order rather than authoring ordinal order.
            var expectedById = pairs.ToDictionary(p => FileDpoPairStore.ComputeIdentity(p, schemaVersion: 5), StringComparer.Ordinal);
            Assert.Equal(count, expectedById.Count);
            var sealedHistory = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(address.Value!, DpoRequestIdentity, pairs);
            Assert.True(sealedHistory.IsSuccess, sealedHistory.ErrorCode + ":" + sealedHistory.ErrorMessage);
            Assert.Equal(expectedById.Keys.OrderBy(id => id, StringComparer.Ordinal), sealedHistory.Value!.OrderedPairs.Select(row => row.PairId));
            var expectedInManifestOrder = sealedHistory.Value.OrderedPairs.Select(row => expectedById[row.PairId]).ToArray();
            var reopened = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(address.Value!, DpoRequestIdentity);
            Assert.True(reopened.IsSuccess, reopened.ErrorCode + ":" + reopened.ErrorMessage);
            Assert.Equal(count, reopened.Value!.Manifest.PairCount); Assert.Equal(count, reopened.Value.Pairs.Count);
            Assert.Equal(sealedHistory.Value.OrderedPairs, reopened.Value.Manifest.OrderedPairs);
            Assert.Equal(sealedHistory.Value.OrderedPairs.Select(row => row.PairId),
                reopened.Value.Pairs.Select(p => FileDpoPairStore.ComputeIdentity(p, schemaVersion: 5)));
            Assert.Equal(expectedInManifestOrder.Select(p => p.TeacherProvenance!.RequestId), reopened.Value.Pairs.Select(p => p.TeacherProvenance!.RequestId));
            Assert.Equal(expectedInManifestOrder.Select(p => p.Prompt), reopened.Value.Pairs.Select(p => p.Prompt));
            Assert.Equal(expectedInManifestOrder.Select(p => p.Chosen), reopened.Value.Pairs.Select(p => p.Chosen));
            Assert.Equal(expectedInManifestOrder.Select(p => p.Rejected), reopened.Value.Pairs.Select(p => p.Rejected));
            return address.Value!;
        }

        private ContinuousLearning.DpoPair CreateDpoPair(
            RepositoryOwnedDpoDatasetRequestIdentity identity, string? prompt = null, string? chosen = null, string? rejected = null,
            string? teacherRequestId = null)
        {
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                identity.OpenTeacher.ModelId,
                identity.OpenTeacher.ModelVersion,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                teacherRequestId ?? "s0304-repository-owned-fixture",
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    identity.OpenTeacher.ModelArtifactDigestKind,
                ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256,
            };
            var pair = new ContinuousLearning.DpoPair(
                prompt ?? "Document a S03.04 repository-owned endpoint method.",
                chosen ?? "Explain the method's accepted endpoint behavior.",
                rejected ?? "Give an unhelpful endpoint summary.",
                DomainSlug,
                DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = provenance,
                GovernanceDecision = new TrainingDataGovernanceDecision(
                    licenseAccepted: true,
                    isQuarantined: false,
                    evidenceRefs: ["sha256:" + Sha256Hex("s0304-governance")]),
                ChosenAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.OpenTeacher,
                    provenance),
                RejectedAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.LocalModel),
            };
            var bound = DpoPairTrainingContentAuthority.Bind(pair);
            Assert.True(bound.IsSuccess, bound.ErrorMessage);
            return bound.Value!;
        }

        private string CreateProfileJson(
            string manifestSha256,
            int maxHeldOutCases = 2,
            int maxNewTokens = 1)
            => JsonSerializer.Serialize(new[]
            {
                new
                {
                    domainSlug = DomainSlug,
                    designPatternSlug = DesignPatternSlug,
                    subjectSlug = SubjectSlug,
                    corpusVersion = CorpusVersion,
                    expectedCorpusManifestSha256 = manifestSha256,
                    generation = new
                    {
                        maxNewTokens,
                        temperature = 0f,
                        topK = 1,
                        topP = 1f,
                        seed = 17,
                    },
                    maxHeldOutCases,
                    maxAnswerChars = 256,
                },
            });

        private static CodeDocumentationSavedModelAssessmentProfile ResolveProfile(
            string json)
        {
            var result = CodeDocumentationSavedModelAssessmentProfileResolver
                .Resolve(name => name == CodeDocumentationSavedModelAssessmentProfileResolver
                    .EnvironmentName
                    ? json
                    : null);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            return Assert.Single(result.Value!).Value;
        }

        private CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest CreateRequest(
            string? trainingRunId = null,
            CodeDocumentationRepositoryOwnedDpoDatasetReceipt? receipt = null,
            IReadOnlyList<ContinuousLearning.DpoPair>? repositoryOwnedPairs = null)
        {
            receipt ??= new CodeDocumentationRepositoryOwnedDpoDatasetReceipt(
                RepositoryOwnedDpoDatasetResolutionSource
                    .NewlyPersistedRepositoryDataset,
                DpoIdentity.ManifestSha256,
                DpoAddress.ManifestRelativePath,
                PairCount: 1,
                DpoManifestRequestIdentitySha256,
                DpoManifestOrderedPairs);
            return new(
                DomainSlug,
                DesignPatternSlug,
                DatasetVersion,
                trainingRunId ?? "s0304-training-run-" + DatasetVersion,
                new CodeDocumentationTrainingRunResult(
                    TrainingExampleCount: 1,
                    EvaluationExampleCount: 1,
                    RoutedCorrect: 1,
                    SourceCorrect: 1,
                    VersionCorrect: 1,
                    TransitCorrect: 1,
                    RoutingAccuracy: 1,
                    SourceGateAccuracy: 1,
                    VersionGateAccuracy: 1,
                    TransitGateAccuracy: 1,
                    BottomUpOrderAccuracy: 1,
                    CalleeSummaryTransitAccuracy: 1,
                    LocalVsDelegatedAccuracy: 1,
                    CpuMilliseconds: 1,
                    PeakWorkingSetMb: 1,
                    ElapsedMilliseconds: 1,
                    DpoPairs: [],
                    RepositoryOwnedDpoReceipt: receipt,
                    RepositoryOwnedDpoPairs: repositoryOwnedPairs?.ToArray() ?? [],
                    Cases: []),
                ["test:s03.04-endpoint"]);
        }

        private string DpoManifestRequestIdentitySha256
        {
            get
            {
                var opened = RepositoryOwnedDpoDatasetManifestSealer
                    .OpenSealed(DpoAddress, DpoRequestIdentity);
                Assert.True(opened.IsSuccess, opened.ErrorMessage);
                return opened.Value!.Manifest.RequestIdentity.RequestIdentitySha256;
            }
        }

        private RepositoryOwnedDpoDatasetManifestPair[]
            DpoManifestOrderedPairs
        {
            get
            {
                var opened = RepositoryOwnedDpoDatasetManifestSealer
                    .OpenSealed(DpoAddress, DpoRequestIdentity);
                Assert.True(opened.IsSuccess, opened.ErrorMessage);
                return opened.Value!.Manifest.OrderedPairs.ToArray();
            }
        }

        private static ModelConfig CreateModelConfig(int contextLength = 8)
            => new()
            {
                VocabSize = 256,
                ContextLen = contextLength,
                HiddenDim = 8,
                NumLayers = 1,
                NumHeads = 1,
                NumKvHeads = 1,
                FfnHiddenDim = 16,
                RopeTheta = 10000f,
                RmsNormEps = 1e-5f,
            };

        private static void AddExplicitModelAndTrainingConfiguration(
            IDictionary<string, string> environment,
            ModelConfig model)
        {
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelVocabSizeEnvironmentName] = model.VocabSize.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelContextLenEnvironmentName] = model.ContextLen.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelHiddenDimEnvironmentName] = model.HiddenDim.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelNumLayersEnvironmentName] = model.NumLayers.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelNumHeadsEnvironmentName] = model.NumHeads.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelNumKvHeadsEnvironmentName] = model.NumKvHeads.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelFfnHiddenDimEnvironmentName] = model.FfnHiddenDim.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelRopeThetaEnvironmentName] = "10000";
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelRmsNormEpsEnvironmentName] = "1E-05";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingBatchSizeEnvironmentName] = "1";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingSeqLenEnvironmentName] = "8";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingMaxLrEnvironmentName] = "0.01";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingMinLrEnvironmentName] = "0.001";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingWarmupStepsEnvironmentName] = "0";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingTotalStepsEnvironmentName] = "10";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingGradAccumStepsEnvironmentName] = "1";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingGradClipNormEnvironmentName] = "1";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingWeightDecayEnvironmentName] = "0";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingCheckpointEveryEnvironmentName] = "1";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingLogEveryEnvironmentName] = "1";
        }

        private Dictionary<string, string> BuildEnvironment(
            string lane,
            int maxTeacherCalls,
            bool useCoordinatorTeacherIdentity,
            RecoverySeedEnvironment seedEnvironment,
            string? evaluatorManifestDigestOverride = null)
        {
            var teacherModelId = useCoordinatorTeacherIdentity
                ? "qwen-code-doc-quality-test:9b"
                : "qwen-s0304-fixture:9b";
            var manifestDigest = evaluatorManifestDigestOverride ??
                (useCoordinatorTeacherIdentity ? Hex('a') : Hex('d'));
            var layerDigest = useCoordinatorTeacherIdentity
                ? Hex('b')
                : Hex('c');
            var auditSaltStart = useCoordinatorTeacherIdentity ? 1 : 40;
            var retentionPolicyRef = useCoordinatorTeacherIdentity
                ? Hex('c')
                : Hex('a');
            var outputLicenseRef = useCoordinatorTeacherIdentity
                ? Hex('d')
                : Hex('b');
            var environment = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [ContinuousLearningModelServerConfigurationLoader
                    .EnabledEnvironmentName] = bool.TrueString,
                [ContinuousLearningModelServerConfigurationLoader
                    .ModelIdEnvironmentName] = teacherModelId,
                [ContinuousLearningModelServerConfigurationLoader
                    .OllamaManifestDigestEnvironmentName] = manifestDigest,
                [ContinuousLearningModelServerConfigurationLoader
                    .OllamaModelLayerDigestEnvironmentName] = layerDigest,
                [ContinuousLearningModelServerConfigurationLoader
                    .BaseModelIdEnvironmentName] = "registry/s0304-base-" + lane,
                [ContinuousLearningModelServerConfigurationLoader
                    .BaseModelRevisionEnvironmentName] = "s0304-base-revision-" + lane,
                [ContinuousLearningModelServerConfigurationLoader
                    .MaxTeacherCallsEnvironmentName] = maxTeacherCalls.ToString(),
                [ContinuousLearningModelServerConfigurationLoader
                    .AuditSaltEnvironmentName] = Convert.ToBase64String(
                    Enumerable.Range(auditSaltStart, 32)
                        .Select(static value => checked((byte)value))
                        .ToArray()),
                [ContinuousLearningModelServerConfigurationLoader
                    .LocalRetentionPolicyRefEnvironmentName] =
                    "sha256:" + retentionPolicyRef,
                [ContinuousLearningModelServerConfigurationLoader
                    .OutputLicenseRefEnvironmentName] =
                    "sha256:" + outputLicenseRef,
                [ContinuousLearningModelServerConfigurationLoader
                    .BaseCheckpointEnvironmentName] = BaseCheckpoint,
                [ContinuousLearningModelServerConfigurationLoader
                    .TokenizerEnvironmentName] = TokenizerPath,
                [ContinuousLearningModelServerConfigurationLoader
                    .AdapterRootEnvironmentName] = AdapterRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .CacheRootEnvironmentName] = CacheRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .RegistryRootEnvironmentName] = RegistryRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .RepositoryRootEnvironmentName] = RepositoryRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .RepositoryModelIdEnvironmentName] = ModelIdSlug,
                [ContinuousLearningModelServerConfigurationLoader
                    .RepositoryModelVersionEnvironmentName] = ModelVersion,
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeConfigSha256EnvironmentName] = RuntimeCompatibility.ConfigSha256,
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTopologySha256EnvironmentName] = RuntimeCompatibility.TopologySha256,
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTokenizerSha256EnvironmentName] = RuntimeCompatibility.TokenizerSha256,
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTrainerSha256EnvironmentName] = RuntimeCompatibility.TrainerSha256,
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeSchemaSha256EnvironmentName] = RuntimeCompatibility.SchemaSha256,
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeCompatibilityKeyEnvironmentName] =
                    RuntimeCompatibility.CompatibilityKey,
                [ContinuousLearningModelServerConfigurationLoader
                    .DpoPairRootEnvironmentName] = DpoAddress.PairsRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .TrainingActivationEnabledEnvironmentName] = bool.TrueString,
                [ContinuousLearningModelServerConfigurationLoader
                    .TrainingRootInitializationSeedEnvironmentName] =
                    seedEnvironment.RootInitializationSeedText,
                [Program.RepositoryOwnedTrainingEnabledEnvironmentName] =
                    bool.FalseString,
                [Program.ReadyFileEnvironmentName] = ReadyFilePath,
                [CodeDocumentationSavedModelAssessmentProfileResolver
                    .EnvironmentName] = ProfileJson,
            };
            AddExplicitModelAndTrainingConfiguration(
                environment,
                ModelConfiguration);
            return environment;
        }

        private void DeleteExactDirectory(string path)
        {
            var target = NormalizePath(path);
            var root = NormalizePath(Root);
            var fixtureRoot = NormalizePath(_ownedFixtureRoot);
            var repositoryRoot = NormalizePath(RepositoryRoot);
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (string.Equals(target, root, comparison))
            {
                if (string.Equals(root, fixtureRoot, comparison) ||
                    !string.Equals(
                        Path.GetDirectoryName(root), fixtureRoot, comparison) ||
                    !Guid.TryParseExact(Path.GetFileName(root), "N", out _))
                {
                    throw new InvalidOperationException(
                        "S03.04 endpoint fixture cleanup root is not an exact fixture lane under the canonical fixture root.");
                }
            }
            else
            {
                var allowed = new List<string>(); // A partial constructor can own one address while the next is still unassigned.
                if (DpoAddress is not null) allowed.Add(NormalizePath(DpoAddress.DatasetRoot));
                if (CorpusAddress is not null) allowed.Add(NormalizePath(CorpusAddress.CorpusRoot));
                if (ModelIdSlug is not null && ModelVersion is not null) allowed.Add(NormalizePath(ModelLaneRoot));
                if (ParentDpoAddress is not null) allowed.Add(NormalizePath(ParentDpoAddress.DatasetRoot));
                allowed.AddRange(_ownedTeachingCorpora.Select(a => NormalizePath(a.CorpusRoot)));
                allowed.AddRange(_ownedContextHistory.Select(a => NormalizePath(a.DatasetRoot)));
                if (string.Equals(target, repositoryRoot, comparison) ||
                    !target.StartsWith(
                        repositoryRoot + Path.DirectorySeparatorChar,
                        comparison) ||
                    !allowed.Any(allowedPath =>
                        string.Equals(allowedPath, target, comparison)))
                {
                    throw new InvalidOperationException(
                        "S03.04 endpoint fixture cleanup target is not an exact repository-owned fixture path.");
                }
            }

            if (_constructorFailureProbe is { FailLaneCleanup: true } probe && string.Equals(target, root, comparison))
                throw probe.CleanupError; // The fixed isolated probe faults the genuine guarded lane release, never a Host or arbitrary factory.
            if (Directory.Exists(target))
                Directory.Delete(target, recursive: true);
        }

        internal static string ResolveActualRepositoryRoot()
        {
            var actual = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                ".."));
            if (!Directory.Exists(actual) ||
                !File.Exists(Path.Combine(actual, "LlmFromScratch.sln")))
            {
                throw new InvalidOperationException(
                    $"S03.04 endpoint fixture could not verify the actual checkout root derived from AppContext.BaseDirectory: {actual}");
            }

            if (StringComparer.OrdinalIgnoreCase.Equals(
                    Environment.GetEnvironmentVariable("GITHUB_ACTIONS"),
                    bool.TrueString))
            {
                var workspace = Environment.GetEnvironmentVariable(
                    "GITHUB_WORKSPACE");
                if (string.IsNullOrWhiteSpace(workspace) ||
                    !Path.IsPathFullyQualified(workspace) ||
                    !Directory.Exists(workspace) ||
                    !SameRepositoryRoot(actual, workspace))
                {
                    throw new InvalidOperationException(
                        "S03.04 endpoint fixture requires the derived actual checkout root to equal normalized absolute GITHUB_WORKSPACE in CI.");
                }
            }

            return actual;
        }

        private static string NormalizePath(string path)
            => Path.GetFullPath(path).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

        private static IReadOnlyDictionary<string, string> SnapshotFiles(
            params (string Prefix, string Root)[] roots)
        {
            var snapshot = new Dictionary<string, string>(
                StringComparer.Ordinal);
            foreach (var (prefix, root) in roots)
            {
                if (!Directory.Exists(root))
                    continue;
                foreach (var file in Directory.EnumerateFiles(
                             root,
                             "*",
                             SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(root, file)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    snapshot[$"{prefix}/{relative}"] =
                        IsOwnedOperationalLock(prefix, relative)
                            ? SnapshotOwnedOperationalLock()
                            : Convert.ToHexString(
                                SHA256.HashData(File.ReadAllBytes(file)));
                }
            }

            return snapshot;
        }

        private static bool IsOwnedOperationalLock(
            string prefix,
            string relative)
            => prefix.Equals("fixture", StringComparison.Ordinal) &&
               (relative.Equals(
                   "adapter/.continuous-learning.adapter-owner.lock",
                   StringComparison.OrdinalIgnoreCase) ||
                relative.Equals(
                   "registry/.continuous-learning.registry-owner.lock",
                   StringComparison.OrdinalIgnoreCase));

        private static string SnapshotOwnedOperationalLock()
            => "operational-owner-lock:present";

        private static string Hex(char value) => new(value, 64);

        private static string Sha256Hex(string value)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
                .ToLowerInvariant();
    }

    private sealed class TempRepositoryRoot : IDisposable
    {
        private readonly bool _publicHostBootstrap;
        private readonly List<string> _ownedPublicDirectories = [];
        private string? _ownedPublicSentinel;
        private readonly byte[] _publicSentinelBytes = Encoding.UTF8.GetBytes("fixture solution sentinel " + Guid.NewGuid().ToString("N"));

        public TempRepositoryRoot(string scope)
        {
            Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"{scope}-{Guid.NewGuid():N}"));
            Directory.CreateDirectory(Path);
            File.WriteAllText(
                System.IO.Path.Combine(Path, "LlmFromScratch.sln"),
                "fixture solution sentinel");
            Directory.CreateDirectory(System.IO.Path.Combine(
                Path,
                "14.ContinuousLearning"));
            Directory.CreateDirectory(System.IO.Path.Combine(
                Path,
                "11.DPO"));
            // Bootstrap opens the existing model root before it creates its
            // diagnostic store. An isolated fixture must provision that real root.
            var artifacts=System.IO.Path.Combine(Path,"training-artifacts");
            Directory.CreateDirectory(artifacts); // Native custody creation is nonrecursive; this fixture owns the missing artifact parent.
            var models=System.IO.Path.Combine(artifacts,"models");
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(models);
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(models));
        }

        public static TempRepositoryRoot PreparePublicHostRoot() => new(publicHostBootstrap: true);

        private TempRepositoryRoot(bool publicHostBootstrap)
        {
            Assert.True(publicHostBootstrap);
            _publicHostBootstrap = true;
            Path = System.IO.Path.GetFullPath(Program.RepositoryOwnedTrainingTrustedRepositoryRoot);
            Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(Path));
            Assert.False(File.Exists(Path));
            try
            {
                CreateMissingPublicDirectory(Path);
                var solution = System.IO.Path.Combine(Path, "LlmFromScratch.sln");
                Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(solution));
                Assert.False(Directory.Exists(solution));
                if (!File.Exists(solution))
                {
                    // CreateNew owns only this synthetic sentinel; a real checkout's solution is never rewritten.
                    using var file = SecureLocalFileBoundary.OpenOwnerOnlyFile(solution, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.None);
                    _ownedPublicSentinel = solution;
                    file.Write(_publicSentinelBytes);
                }
                CreateMissingPublicDirectory(System.IO.Path.Combine(Path, "14.ContinuousLearning"));
                CreateMissingPublicDirectory(System.IO.Path.Combine(Path, "11.DPO"));
                CreateMissingPublicDirectory(System.IO.Path.Combine(Path, "training-artifacts"));
                CreateMissingPublicDirectory(System.IO.Path.Combine(Path, "training-artifacts", "models"), custody: true);
            }
            catch (Exception constructionError)
            {
                try { Dispose(); } // Construction failure may clean only this bootstrap's exact newly owned structural paths.
                catch (Exception cleanupError) { throw new AggregateException("Public Host fixture bootstrap and exact structural cleanup failed.", constructionError, cleanupError); }
                throw;
            }
        }

        private void CreateMissingPublicDirectory(string path, bool custody = false)
        {
            Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path));
            Assert.False(File.Exists(path));
            if (!Directory.Exists(path))
            {
                if (custody) SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(path);
                else SecureLocalFileBoundary.CreateOwnerOnlyDirectory(path);
                _ownedPublicDirectories.Add(path);
            }
            // Existing permissions are inspected, never changed to satisfy a fixture.
            Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path));
            if (custody) Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(path));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (_publicHostBootstrap)
            {
                Assert.Equal(System.IO.Path.GetFullPath(Program.RepositoryOwnedTrainingTrustedRepositoryRoot), Path);
                Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(Path));
                if (_ownedPublicSentinel is not null)
                {
                    Assert.Equal(System.IO.Path.Combine(Path, "LlmFromScratch.sln"), _ownedPublicSentinel);
                    Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(_ownedPublicSentinel));
                    Assert.Equal(_publicSentinelBytes, File.ReadAllBytes(_ownedPublicSentinel));
                    File.Delete(_ownedPublicSentinel);
                    _ownedPublicSentinel = null;
                }
                foreach (var directory in _ownedPublicDirectories.AsEnumerable().Reverse())
                {
                    Assert.True(directory == Path || directory.StartsWith(Path + System.IO.Path.DirectorySeparatorChar,
                        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                    Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory));
                    // Nonrecursive removal only: pre-existing paths and any surviving Host material stay untouched.
                    if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory, recursive: false);
                }
                _ownedPublicDirectories.Clear();
                return; // Never enter the native temp-root recursive cleanup with the fixed public Host root.
            }
            var temp=System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(System.IO.Path.GetTempPath()))+System.IO.Path.DirectorySeparatorChar;
            var comparison=OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if(!Path.StartsWith(temp,comparison) || System.IO.Path.GetFullPath(Path)!=Path ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(Path))
                throw new IOException("The exact owned native fixture cleanup path changed.");
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }

    private sealed class EnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _prior = new(
            StringComparer.Ordinal);

        public EnvironmentScope(IReadOnlyDictionary<string, string?> values)
        {
            foreach (var (name, value) in values)
            {
                _prior[name] = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        public void Dispose()
        {
            foreach (var (name, value) in _prior)
                Environment.SetEnvironmentVariable(name, value);
        }
    }

    private sealed class RecordingFailClosedCertificationAuthorityProvider
        : ICodeDocumentationCertificationAuthorityProvider
    {
        internal const string ErrorCode =
            "S03_LINKED_RECOVERY_AUTHORITY_REJECTED";
        internal const string ErrorMessage =
            "Test-only certification authority refuses to issue a scope.";

        public int ResolveCalls { get; private set; }
        public string? TrustedRepositoryRoot { get; private set; }
        public CodeDocumentationRepositoryOwnedModelCheckpointStartRequest?
            StartRequest { get; private set; }
        public RepositoryOwnedModelCheckpointManifest? ParentManifest
            { get; private set; }

        public OperationResult<ServerOwnedCodeDocumentationCertificationScope>
            Resolve(
                string trustedRepositoryRoot,
                CodeDocumentationRepositoryOwnedModelCheckpointStartRequest
                    startRequest,
                RepositoryOwnedModelCheckpointManifest parent,
                RepositoryOwnedReferenceCorpusOpenResult corpus,
                CodeDocumentationCertificationProfile profile,
                ContinuousLearningTp1DpoConfiguration evaluator)
        {
            ResolveCalls++;
            TrustedRepositoryRoot = trustedRepositoryRoot;
            StartRequest = startRequest;
            ParentManifest = parent;
            return OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                .Fail(ErrorMessage, ErrorCode);
        }
    }
}
