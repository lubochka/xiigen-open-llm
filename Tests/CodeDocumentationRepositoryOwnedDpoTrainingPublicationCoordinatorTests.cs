// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using LoRA;
using Tokenizer;
using TrainableRouter;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;
using TrainingLoop.Optimizers;
using ModelServer.Http; // Exercise the explicit caller-lifetime transport without a live server.
using Xunit.Abstractions;

namespace LlmFromScratch.Tests;

public sealed class CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
{
    private const string QualitySubjectSlug =
        "csharp-documentation-quality-fixture";
    private const string QualityCorpusVersion =
        "v20260909-quality-gate";

    private readonly ITestOutputHelper _output;

    public CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ScientificRawCapture_PreservesLongExactUtf8AndBinding_AndCallerLifetimeKeepsLegacyBounds()
    {
        var root = CreateRepositoryRoot("scientific-raw-capture");
        try
        {
            Assert.False(ModelServer.CodeDocumentationScientificTrainingConfiguration.Resolve(_ => null).Open(root, false, default).IsSuccess); // Disabled scientific startup refuses before services/model/C0.
            Assert.False(ModelServer.CodeDocumentationScientificTrainingConfiguration.Resolve(name => name.EndsWith("MODE", StringComparison.Ordinal) ? "unknown-mode" : "malformed").Open(root, false, default).IsSuccess); // No malformed choice or guessed fallback grants retention/training/publication.
            var address = CreateModelTrainingAttemptAddress(root, "synthetic-raw-retention");
            var store = CodeDocumentationPrivateEvaluationStore.Create(address).Value!;
            var model = new CodeDocumentationSavedModelAssessmentModelPassport("csharp-code-documentation", "ilearning-unit", "synthetic", "v1", 1, "model/manifest.json", Hex('a'), "model/weights.bin", Hex('b'), 12, 1024);
            var binding = new CodeDocumentationRawAnswerBinding("synthetic-baseline-repeat-001", "synthetic-case", Hex('c'), Hex('d'), Hex('e'), Hex('f'), model);
            var text = "\0\t\r\n" + string.Concat(Enumerable.Repeat("汉字😀é\u0085\u2028", 300)) + "\r\n  ";
            Assert.True(text.Length > 1024);
            var retained = store.Capture(binding, text);
            Assert.True(retained.IsSuccess, retained.ErrorCode);
            var metadata = store.Read<CodeDocumentationRawAnswerArtifact>(retained.Value!);
            Assert.True(metadata.IsSuccess, metadata.ErrorCode);
            Assert.Equal(binding, metadata.Value!.Binding);
            Assert.Equal(text.Length, metadata.Value.CharCount);
            var actual = store.ReadBytes(metadata.Value.RawUtf8);
            Assert.True(actual.IsSuccess, actual.ErrorCode);
            Assert.Equal(new UTF8Encoding(false, true).GetBytes(text), actual.Value);
            Assert.Equal(text, new UTF8Encoding(false, true).GetString(actual.Value!));
            Assert.True(store.Capture(binding with { AssessmentRunId = "synthetic-baseline-repeat-002" }, text).IsSuccess); // Every repeat has a distinct metadata reference.
            var empty = store.Capture(binding with { CaseId = "empty" }, "");
            Assert.True(empty.IsSuccess, empty.ErrorCode);
            Assert.Empty(store.ReadBytes(store.Read<CodeDocumentationRawAnswerArtifact>(empty.Value!).Value!.RawUtf8).Value!);
            Assert.False(store.ReadBytes(metadata.Value.RawUtf8 with { Sha256 = Hex('a') }).IsSuccess);
            Assert.False(store.ReadBytes(metadata.Value.RawUtf8 with { RelativePath = "training-artifacts/private-evaluation/../escape" }).IsSuccess);
            File.AppendAllText(Path.Combine(root, metadata.Value.RawUtf8.RelativePath), "changed", new UTF8Encoding(false));
            Assert.False(store.ReadBytes(metadata.Value.RawUtf8).IsSuccess); // Actual byte drift, not merely a declared hash mismatch.
            Assert.False(store.Capture(binding, text).IsSuccess); // Never overwrite changed/partial retained evidence.
            Assert.True(store.Put("intent.json", new { synthetic = true }, requireNew: true).IsSuccess);
            Assert.False(store.Put("intent.json", new { synthetic = true }, requireNew: true).IsSuccess); // Identical bytes cannot reacquire fresh intent authority.
            var family = JsonSerializer.SerializeToNode(new CodeDocumentationScientificFamilyVerdict("case", Hex('a'), false, true, 0, 0, [Hex('b')], [Hex('c')]), CodeDocumentationScientificTrainingPolicy.Json)!;
            family.AsObject().Remove("baselineCorrect");
            Assert.False(store.Read<CodeDocumentationScientificFamilyVerdict>(store.Put("missing-boolean.json", family).Value!).IsSuccess); // Missing baselineCorrect must not default to false and manufacture scientific gain.
            family["baselineCorrect"] = false; family["BaselineCorrect"] = true;
            Assert.False(store.Read<CodeDocumentationScientificFamilyVerdict>(store.Put("case-alias.json", family).Value!).IsSuccess); // Differently cased aliases are unknown fields, not last-wins bindings.
            var observation = new CodeDocumentationSavedModelAssessmentCalibrationObservation("legacy", 1, 1, 1, Hex('a'), Hex('b'), Hex('b'), ["case"], [Hex('c')], ["request"]);
            Assert.DoesNotContain("privateRawAnswers", JsonSerializer.Serialize(observation, new JsonSerializerOptions(JsonSerializerDefaults.Web))); // Optional null retention preserves legacy wire omission.
            using var handler = new ScientificLifetimeHandler();
            using var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
            var transport = new HttpClientModelServerJsonTransport(client);
            var uri = new Uri("http://127.0.0.1:12345/");
            var legacyInvalid = await transport.PostJsonAsync<object, object>(uri, "/probe", new { }, Timeout.InfiniteTimeSpan);
            Assert.Equal(HttpClientModelServerJsonTransport.InvalidRequestErrorCode, legacyInvalid.ErrorCode);
            var tooLong = await transport.PostJsonAsync<object, object>(uri, "/probe", new { }, HttpClientModelServerJsonTransport.MaximumTimeout + TimeSpan.FromTicks(1));
            Assert.Equal(HttpClientModelServerJsonTransport.InvalidRequestErrorCode, tooLong.ErrorCode);
            using var finiteClient = new HttpClient(new ScientificLifetimeHandler()); // The caller-lifetime branch refuses a hidden HttpClient deadline.
            var hiddenDeadline = await new HttpClientModelServerJsonTransport(finiteClient).PostJsonForCallerLifetimeAsync<object, object>(uri, "/probe", new { });
            Assert.Equal(HttpClientModelServerJsonTransport.InvalidRequestErrorCode, hiddenDeadline.ErrorCode);
            using var cancellation = new CancellationTokenSource();
            var pending = transport.PostJsonForCallerLifetimeAsync<object, object>(uri, "/probe", new { }, cancellation.Token);
            await handler.Started.Task;
            Assert.False(pending.IsCompleted);
            cancellation.Cancel(); // Only explicit caller cancellation completes this waiting synthetic transport.
            var cancelled = await pending;
            Assert.Equal(HttpClientModelServerJsonTransport.CancelledErrorCode, cancelled.ErrorCode);
            Assert.Equal(1, handler.Calls); // Invalid legacy/unbounded envelopes never sent an HTTP request.
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    private sealed class ScientificLifetimeHandler : HttpMessageHandler
    {
        internal readonly TaskCompletionSource<bool> Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++; Started.TrySetResult(true);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Synthetic waiting handler requires caller cancellation.");
        }
    }

    [Fact]
    public async Task ScientificHold_PublishesSameSyntheticTrialOnlyAfterBoundVerdict_WithoutReplayOrAssessment()
    {
        // Engineering mechanism fixture only: tiny synthetic model/corpus and a scripted evaluator, never the real RP01 Student or closed exam.
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("scientific-hold");
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            var setup = CreateNoiseDecisionSetup(fixture, root, "scientific-hold", 12, [0, 0, 0, 0, 0], [12, 12, 12, 12, 12]); // Existing native noise floor is an additional condition; the separate independent-verdict check below still tests the exact 11-9=2 boundary.
            var lesson = SealRepositoryOwnedDpo(root, "scientific-short", "v1-short", "doc", "keep", "drop"); // Exact synthetic DPO material fits this tiny engineering model's context; real RP01 data is untouched.
            var receipt = lesson.Receipt with { Source = RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset };
            var raw = new CodeDocumentationTrainingRunResult(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, [], receipt, lesson.Pairs.ToArray(), []);
            var request = setup.Request with { DpoDatasetAddress = lesson.Address, RoutingRun = raw, RoutingDisposition = CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingNotPerformed };
            var store = CodeDocumentationPrivateEvaluationStore.Create(setup.AttemptAddress).Value!;
            var configDirectory = Path.Combine(root, "config");
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(configDirectory);
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.Combine(configDirectory, "code-documentation"));
            CodeDocumentationScientificSource Trusted(string name, string contents)
            {
                var relative = "config/code-documentation/" + name;
                using var file = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(Path.Combine(root, relative), FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough);
                var bytes = Encoding.UTF8.GetBytes(contents); file.Write(bytes); file.Flush(true);
                return new(relative, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), true);
            }
            var cases = request.QualityGate!.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).OrderBy(e => e.EntryId, StringComparer.Ordinal).Select(e => new CodeDocumentationScientificCase(e.EntryId, Sha256Hex("synthetic-family:" + e.EntryId), Sha256Hex(e.OwnerInput))).ToArray();
            var policyDocument = new CodeDocumentationScientificPolicyDocument("first-rp01-scientific-policy-v1", request.TrainingRunId, setup.ParentManifest.ManifestSha256, receipt.ManifestSha256, receipt.OrderedPairs.Single().PairId, "synthetic-independent-examiner", 12, 5, 2, 0, 0, Trusted("protocol.json", "synthetic protocol"), Trusted("commitment.json", "synthetic commitment"), Trusted("retention.json", "synthetic EVALUATION_ONLY retention"), Trusted("scorer.json", "synthetic frozen scorer"), cases);
            var policySource = Trusted("first-rp01-scientific-policy-v1.json", JsonSerializer.Serialize(policyDocument, CodeDocumentationScientificTrainingPolicy.Json));
            var openedPolicy = CodeDocumentationScientificTrainingPolicy.Open(root, policySource.Sha256);
            Assert.True(openedPolicy.IsSuccess, openedPolicy.ErrorCode);
            var policy = openedPolicy.Value!;
            var calls = 0;
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => { Assert.True(store.HasHoldIntent().Value); calls++; };
            var evaluator = new ScriptedQualityGateEvaluationClient(request.QualityGate.EvaluatorConfiguration, CreatePerRepeatQualityOutcomes(12, [0, 0, 0, 0, 0]).Concat(CreatePerRepeatQualityOutcomes(12, [12, 12, 12, 12, 12])).ToArray(), requestIdFactory: null, responseTransform: (_, result) => { Assert.True(store.HasHoldIntent().Value); Assert.False(File.Exists(setup.ChildAddress.ManifestPath)); return result; });
            request = request with { QualityGate = request.QualityGate with { EvaluationClient = evaluator } };
            var held = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndHoldForScienceAsync(request, policy);
            Assert.True(held.IsSuccess, held.ErrorCode + ":" + held.ErrorMessage);
            Assert.Equal("HELD_FOR_SCIENCE", held.Value!.Status);
            Assert.Null(held.Value.Publication);
            Assert.Equal("Improved", held.Value.NativeQualityStatus);
            Assert.False(File.Exists(setup.ChildAddress.ManifestPath));
            Assert.Equal(1, calls);
            Assert.Equal(120, evaluator.IssuedCallCount);
            var ordinary = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(request with { RoutingRun = CreateRoutingRun(receipt, lesson.Pairs), RoutingDisposition = null }); // A valid legacy envelope for the exact same held lesson tests the common intent guard, rather than an unrelated receipt mismatch.
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ScientificHoldRequired, ordinary.ErrorCode); // Ordinary completed-routing resume cannot bypass durable intent.
            var repeatedHold = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndHoldForScienceAsync(request, policy);
            Assert.False(repeatedHold.IsSuccess); // No second C0/T or optimizer from train-and-hold recovery.
            var absent = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishHeldForScienceAsync(request, policy, held.Value.HeldTrial!.Sha256, Hex('f'));
            Assert.False(absent.IsSuccess);
            using var bundle = JsonDocument.Parse(store.ReadBytes(held.Value.HeldTrial).Value!);
            var rootNode = bundle.RootElement;
            var baseline = JsonSerializer.Deserialize<CodeDocumentationSavedModelAssessmentCalibrationResult>(rootNode.GetProperty("detailed").GetProperty("baseline").GetProperty("calibration"), CodeDocumentationScientificTrainingPolicy.Json)!;
            var trial = JsonSerializer.Deserialize<CodeDocumentationSavedModelAssessmentCalibrationResult>(rootNode.GetProperty("detailed").GetProperty("trial"), CodeDocumentationScientificTrainingPolicy.Json)!;
            var hashes = cases.ToDictionary(c => c.CaseId, c => (Baseline: baseline.Calibration.Observations.Select(o => o.StudentAnswerSha256[o.CaseIds.ToList().IndexOf(c.CaseId)]).ToArray(), Trial: trial.Calibration.Observations.Select(o => o.StudentAnswerSha256[o.CaseIds.ToList().IndexOf(c.CaseId)]).ToArray()));
            var families = cases.Select((c, index) => new CodeDocumentationScientificFamilyVerdict(c.CaseId, c.FamilySha256, index < 9, index < 11, 0, 0, hashes[c.CaseId].Baseline, hashes[c.CaseId].Trial)).ToArray();
            var verdict = new CodeDocumentationScientificVerdict("first-rp01-independent-scientific-verdict-v1", "INDEPENDENT_BLIND_EXAMINER", policyDocument.ExaminerAuthority, policy.Sha256, policyDocument.Protocol.Sha256, request.TrainingRunId, held.Value.HeldTrial.Sha256, rootNode.GetProperty("sourceBindingSha256").GetString()!, setup.ParentManifest.ManifestSha256, rootNode.GetProperty("trialWeightsSha256").GetString()!, families);
            bool Accept(CodeDocumentationScientificVerdict value) => CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ValidScientificVerdict(policy, value, verdict.HeldBundleSha256, verdict.SourceBindingSha256, verdict.TrialWeightsSha256, hashes);
            Assert.True(Accept(verdict)); // Exact integer boundary 11-9=2, independent of floating fractions.
            Assert.False(Accept(verdict with { Families = families.Select((f, i) => i == 10 ? f with { TrialCorrect = false } : f).ToArray() }));
            Assert.False(Accept(verdict with { Families = families.Select((f, i) => i == 0 ? f with { TrialCriticalErrors = 1 } : f).ToArray() }));
            Assert.False(Accept(verdict with { Families = families.Select((f, i) => i == 0 ? f with { NewCriticalErrors = 1 } : f).ToArray() }));
            Assert.False(Accept(verdict with { SourceBindingSha256 = Hex('f') }));
            Assert.False(Accept(verdict with { Families = families.Select((f, i) => i == 0 ? f with { TrialRawAnswerSha256 = [Hex('f')] } : f).ToArray() }));
            var review = store.Put("scientific-verdict.json", verdict);
            Assert.True(review.IsSuccess, review.ErrorCode);
            var drifted = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishHeldForScienceAsync(request, policy, Hex('f'), review.Value!.Sha256);
            Assert.False(drifted.IsSuccess);
            var changedParent = request with { StartRequest = request.StartRequest with { RuntimeCompatibility = request.StartRequest.RuntimeCompatibility with { ConfigSha256 = Hex('f') } } };
            Assert.False((await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishHeldForScienceAsync(changedParent, policy, held.Value.HeldTrial.Sha256, review.Value.Sha256)).IsSuccess);
            Assert.Equal(1, calls); Assert.Equal(120, evaluator.IssuedCallCount); // All missing/negative/drift checks above ran zero new model/evaluator/executor work.
            var published = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishHeldForScienceAsync(request, policy, held.Value.HeldTrial.Sha256, review.Value.Sha256);
            Assert.True(published.IsSuccess, published.ErrorCode + ":" + published.ErrorMessage);
            Assert.Equal("PUBLISHED", published.Value!.Status);
            Assert.Equal(verdict.TrialWeightsSha256, published.Value.Publication!.Publication.WeightsSha256); // Same immutable Trial bytes, never a second update.
            Assert.Equal(1, calls); Assert.Equal(120, evaluator.IssuedCallCount);
            Assert.True(File.Exists(setup.ChildAddress.ManifestPath));
            _output.WriteLine("synthetic-scientific-mechanism: " + JsonSerializer.Serialize(new { held = held.Value.Status, held.Value.NativeQualityStatus, heldSha256 = held.Value.HeldTrial.Sha256, published = published.Value.Status, trialSha256 = verdict.TrialWeightsSha256, publishedWeightsSha256 = published.Value.Publication.Publication.WeightsSha256, logicalTrainerCalls = calls, scriptedEvaluatorCalls = evaluator.IssuedCallCount, resumeTrainerDelta = 0, resumeEvaluatorDelta = 0, independentCorrectFamilyGain = families.Count(f => f.TrialCorrect) - families.Count(f => f.BaselineCorrect), independentTrialCritical = families.Sum(f => f.TrialCriticalErrors), independentNewCritical = families.Sum(f => f.NewCriticalErrors) })); // Only synthetic hashes/counts; no real closed input/output or scientific result.
            var partialRequest = setup.Request with { TrainingRunId = request.TrainingRunId + "-partial" };
            var partialAddress = CreateModelTrainingAttemptAddress(root, partialRequest.TrainingRunId);
            Assert.True(CodeDocumentationPrivateEvaluationStore.Create(partialAddress).Value!.Put("intent.json", new { incomplete = true }).IsSuccess);
            var partial = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(partialRequest);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ScientificHoldRequired, partial.ErrorCode); // Partial/malformed early intent also blocks all ordinary resume.
            Assert.Equal(1, calls); Assert.Equal(120, evaluator.IssuedCallCount);
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScientificPartialBaselineRecovery_ReusesFiveRaw_CompletesOneExistingUpdateAndHolds(bool retainVerdict)
    {
        // One synthetic mechanism fixture: no real closed exam, model endpoint or API inference.
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("scientific-partial-c0");
        var previousTrainer = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        var previousGeneration = CodeDocumentationSavedModelAssessmentService.StudentGenerationObserverForTesting;
        try
        {
            var setup = CreateNoiseDecisionSetup(fixture, root, "partial-c0", 12, [0, 0, 0, 0, 0], [12, 12, 12, 12, 12]);
            var lesson = SealRepositoryOwnedDpo(root, "partial-c0-short", "v1-short", "doc", "keep", "drop");
            var receipt = lesson.Receipt with { Source = RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset };
            var raw = new CodeDocumentationTrainingRunResult(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, [], receipt, lesson.Pairs.ToArray(), []);
            var request = setup.Request with { DpoDatasetAddress = lesson.Address, RoutingRun = raw, RoutingDisposition = CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingNotPerformed };
            var store = CodeDocumentationPrivateEvaluationStore.Create(setup.AttemptAddress).Value!;
            var configDirectory = Path.Combine(root, "config");
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(configDirectory);
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.Combine(configDirectory, "code-documentation"));
            CodeDocumentationScientificSource Trusted(string name, string text)
            {
                var relative = "config/code-documentation/" + name;
                using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(Path.Combine(root, relative), FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough);
                var bytes = Encoding.UTF8.GetBytes(text); stream.Write(bytes); stream.Flush(true);
                return new(relative, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), true);
            }
            var cases = request.QualityGate!.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).OrderBy(e => e.EntryId, StringComparer.Ordinal).ToArray();
            var policyDocument = new CodeDocumentationScientificPolicyDocument("first-rp01-scientific-policy-v1", request.TrainingRunId, setup.ParentManifest.ManifestSha256, receipt.ManifestSha256, receipt.OrderedPairs.Single().PairId, "synthetic-independent-examiner", 12, 5, 2, 0, 0, Trusted("protocol.json", "synthetic protocol"), Trusted("commitment.json", "synthetic commitment"), Trusted("retention.json", "synthetic evaluation-only retention"), Trusted("scorer.json", "synthetic frozen scorer"), cases.Select(e => new CodeDocumentationScientificCase(e.EntryId, Sha256Hex("family:" + e.EntryId), Sha256Hex(e.OwnerInput))).ToArray());
            var policySource = Trusted("first-rp01-scientific-policy-v1.json", JsonSerializer.Serialize(policyDocument, CodeDocumentationScientificTrainingPolicy.Json));
            var policy = CodeDocumentationScientificTrainingPolicy.Open(root, policySource.Sha256).Value!;
            var trainerEntries = 0; var generatedBindings = new List<CodeDocumentationRawAnswerBinding>();
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => trainerEntries++;
            CodeDocumentationSavedModelAssessmentService.StudentGenerationObserverForTesting = b => generatedBindings.Add(b);
            var interruptedClient = new ScriptedQualityGateEvaluationClient(request.QualityGate.EvaluatorConfiguration, retainVerdict ? [false] : [], null); // The first permit issues, then the scripted checker has no response; raw and intent survive.
            var interrupted = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndHoldForScienceAsync(request with { QualityGate = request.QualityGate with { EvaluationClient = interruptedClient } }, policy);
            Assert.False(interrupted.IsSuccess); Assert.Equal(retainVerdict ? 2 : 1, interruptedClient.IssuedCallCount); Assert.Equal(retainVerdict ? 2 : 1, generatedBindings.Count); Assert.Equal(0, trainerEntries);
            Assert.False(Directory.Exists(setup.AttemptAddress.AttemptRoot)); // Structural pre-executor recovery is distinct from a historical optimizer counter claim.
            var first = generatedBindings.First();
            foreach (var entry in cases.Skip(generatedBindings.Count).Take(5 - generatedBindings.Count))
                Assert.True(store.Capture(first with { CaseId = entry.EntryId, OwnerInputSha256 = Sha256Hex(entry.OwnerInput) }, "synthetic retained baseline answer " + entry.EntryId).IsSuccess); // Seed four more valid fixture captures, not real Student answers or a private corpus.
            var privateRoot = Path.Combine(root, "training-artifacts", "private-evaluation", setup.AttemptAddress.DomainSlug, setup.AttemptAddress.DesignPatternSlug, setup.AttemptAddress.ModelIdSlug, setup.AttemptAddress.ModelVersion, setup.AttemptAddress.TrainingRunIdSha256);
            var beforeIntent = File.ReadAllBytes(Path.Combine(privateRoot, "intent.json"));
            var answers = Path.Combine(privateRoot, "answers");
            var retained = Directory.GetFiles(answers, "*.utf8").ToDictionary(p => p, p => File.ReadAllBytes(p));
            Assert.Equal(5, retained.Count); var cachedVerdicts = retainVerdict ? 1 : 0; Assert.Equal(cachedVerdicts, Directory.GetFiles(answers, "*.evaluation.json").Length);
            generatedBindings.Clear();
            var checker = new ScriptedQualityGateEvaluationClient(request.QualityGate.EvaluatorConfiguration, Enumerable.Repeat(false, 60 - cachedVerdicts).Concat(Enumerable.Repeat(true, 60)).ToArray(), i => "recovery-check-" + i.ToString("D4"));
            request = request with { QualityGate = request.QualityGate with { EvaluationClient = checker } };
            var leaseResult = store.AcquireScientificInvocationLease();
            Assert.True(leaseResult.IsSuccess, leaseResult.ErrorCode);
            using (leaseResult.Value!)
            {
                var concurrent = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ResumeBaselineAndHoldForScienceAsync(request, policy);
                Assert.False(concurrent.IsSuccess); Assert.Empty(generatedBindings); Assert.Equal(0, checker.IssuedCallCount); Assert.Equal(0, trainerEntries); // Same-run contention refuses before any measurement or optimizer.
            }
            var drifted = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ResumeBaselineAndHoldForScienceAsync(request with { QualityGate = request.QualityGate with { GenerationSettings = request.QualityGate.GenerationSettings with { Seed = request.QualityGate.GenerationSettings.Seed + 1 } } }, policy);
            Assert.False(drifted.IsSuccess); Assert.Empty(generatedBindings); Assert.Equal(0, checker.IssuedCallCount); // Recomputed full source binding must match the original intent.
            var held = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ResumeBaselineAndHoldForScienceAsync(request, policy);
            Assert.True(held.IsSuccess, held.ErrorCode + ": " + held.ErrorMessage);
            Assert.Equal("HELD_FOR_SCIENCE", held.Value!.Status); Assert.Null(held.Value.Publication); Assert.False(File.Exists(setup.ChildAddress.ManifestPath));
            Assert.Equal(1, trainerEntries); Assert.Equal(120 - cachedVerdicts, checker.IssuedCallCount);
            Assert.Equal(115, generatedBindings.Count);
            Assert.Equal(55, generatedBindings.Count(b => b.AssessmentRunId.Contains(":quality-baseline", StringComparison.Ordinal)));
            Assert.Equal(60, generatedBindings.Count(b => b.AssessmentRunId.Contains(":quality-trial", StringComparison.Ordinal)));
            Assert.DoesNotContain(generatedBindings, b => b.AssessmentRunId == first.AssessmentRunId && cases.Take(5).Any(e => e.EntryId == b.CaseId)); // All five retained raw slots issue zero Student generations.
            Assert.Equal(beforeIntent, File.ReadAllBytes(Path.Combine(privateRoot, "intent.json")));
            foreach (var saved in retained) Assert.Equal(saved.Value, File.ReadAllBytes(saved.Key)); // Original raw bytes are never overwritten.
            Assert.Equal(new CodeDocumentationScientificEvaluationAccounting("UNKNOWN", 120 - cachedVerdicts, 120 - cachedVerdicts, 5, cachedVerdicts, 120), held.Value.EvaluationAccounting);
            Assert.Equal(120, Directory.GetFiles(answers, "*.evaluation.json").Length); Assert.Equal(121, Directory.GetFiles(answers, "*.dispatch-*.json").Length); // The unresolved original dispatch remains visible and is not erased or counted as zero.
            var again = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ResumeBaselineAndHoldForScienceAsync(request, policy);
            Assert.False(again.IsSuccess); Assert.Equal(1, trainerEntries); Assert.Equal(120 - cachedVerdicts, checker.IssuedCallCount); Assert.Equal(115, generatedBindings.Count); // Existing canonical attempt can never enter this pre-executor route again.
            _output.WriteLine("partial-c0-recovery: " + JsonSerializer.Serialize(new { held.Value.Status, held.Value.NativeQualityStatus, held.Value.EvaluationAccounting, baselineGenerations = 55, trialGenerations = 60, cachedFiveGenerations = 0, existingExecutorEntries = trainerEntries, checker.IssuedCallCount, logicalResults = 120, cachedVerdicts, unresolvedHistoricalDispatches = 1, publication = "NONE", repeatedRecoveryEffect = "REFUSED" }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = previousTrainer; CodeDocumentationSavedModelAssessmentService.StudentGenerationObserverForTesting = previousGeneration; DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData("tampered-raw")]
    [InlineData("orphan")]
    [InlineData("extra-arm")]
    [InlineData("misbound")]
    [InlineData("canonical-attempt")]
    public void ScientificPartialBaselineRecovery_RefusesAmbiguousRetainedState(string fault)
    {
        var root = CreateRepositoryRoot("partial-c0-refusal-" + fault);
        try
        {
            var address = CreateModelTrainingAttemptAddress(root, "partial-refusal");
            var store = CodeDocumentationPrivateEvaluationStore.Create(address).Value!;
            Assert.True(store.Put("intent.json", new { synthetic = true }).IsSuccess); // Store-owner fixture only; lifecycle intent admission is tested by the integration Fact.
            var passport = new CodeDocumentationSavedModelAssessmentModelPassport("d", "p", "m", "v1", 1, "m.json", Hex('a'), "m.weights", Hex('b'), 1, 1024);
            var slots = Enumerable.Range(1, 5).SelectMany(repeat => Enumerable.Range(1, 12).Select(index => new CodeDocumentationRawAnswerBinding("run-baseline-repeat-" + repeat.ToString("D3"), "case-" + index.ToString("D3"), Hex('c'), Hex('d'), Hex('e'), Hex('f'), passport))).ToArray();
            var captured = store.Capture(slots[0], "exact raw"); Assert.True(captured.IsSuccess);
            var artifact = store.Read<CodeDocumentationRawAnswerArtifact>(captured.Value!).Value!;
            if (fault == "tampered-raw")
            {
                using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(Path.Combine(root, artifact.RawUtf8.RelativePath), FileMode.Open, FileAccess.Write, FileShare.None, FileOptions.WriteThrough);
                stream.WriteByte((byte)'X'); stream.Flush(true); // Alter bytes through the fixture's own storage boundary without changing its declared digest.
            }
            if (fault == "orphan")
            {
                var key = CodeDocumentationPrivateEvaluationStore.SlotKey(slots[1]);
                Assert.True(store.Put("answers/" + key + ".json", new CodeDocumentationRawAnswerArtifact(slots[1], artifact.RawUtf8, 9)).IsSuccess); // Metadata without its deterministic raw must refuse, never regenerate.
            }
            if (fault == "extra-arm") Assert.True(store.Capture(slots[0] with { AssessmentRunId = "unexpected-trial-repeat-001" }, "extra raw").IsSuccess);
            if (fault == "misbound") slots[0] = slots[0] with { OwnerInputSha256 = Hex('a') };
            if (fault == "canonical-attempt") SecureLocalFileBoundary.CreateOwnerOnlyDirectory(address.AttemptRoot); // Even an incomplete canonical attempt belongs to post-executor recovery.
            var eligibility = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.AdmitPartialBaselineRecovery(address, Hex('a'), Hex('b'));
            if (fault == "canonical-attempt") Assert.False(eligibility.IsSuccess);
            else { Assert.True(eligibility.IsSuccess, eligibility.ErrorCode); Assert.False(store.AdmitRetainedBaseline(eligibility.Value!, slots).IsSuccess); }
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Fact]
    public async Task ExactProfile_SourceBoundDiskContinuationAndReopen()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("exact-profile-continuation");
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var lora = CreateLoraConfig() with { Dropout = 0.2f };
            var seeds = new LoraPairRandomSeedAuthority(73, "policy.step", "reference.step");
            var compatibility = CreateCompatibility(fixture.ModelConfig, "exact-profile");
            var rootLesson = SealRepositoryOwnedDpo(root, "exact-root", "v0", "doc base", "keep base", "drop base");
            var lesson = SealRepositoryOwnedDpo(root, "exact-profile", "v1", "doc code", "keep code", "drop code");
            var parent = fixture.CreateFactory().Create("CODE").Value!.Model;
            LoraApplicator.ApplyFreshSeededPair(parent, CloneModel(parent), lora,
                seeds.RootInitializationSeed, seeds.PolicyDropoutStreamDomain, seeds.ReferenceDropoutStreamDomain);
            SealModelCheckpoint(root, parent, fixture.ModelConfig, compatibility, rootLesson.Identity, 1, null, "synthetic exact root");
            var start = CreateStartRequest(root, fixture.ModelConfig, compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied, lora) with { AdapterSeedAuthority = seeds };
            var corpus = SealQualityReferenceCorpus(root);
            var sourceBroker = new PreparedCoordinatorBroker(CreateAcceptingBroker());
            ScriptedQualityGateEvaluationClient sourceEvaluator = null!;
            var sourceRequest = new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                CreateRoutingRun(lesson.Receipt, lesson.Pairs), lesson.Address, start, tokenizer, lora,
                sourceBroker, "exact-profile:source", 1, DateTimeOffset.UnixEpoch,
                ["test:synthetic-exact-profile"], CreateQualityGateContext(corpus, tokenizer, CreateQualityOutcomes(false, true),
                    captureClient: client => sourceEvaluator = client),
                OptimizerStateColdStartAuthorization: ExplicitOptimizerColdStartAuthorization("exact profile root"),
                AdapterSeedAuthority: seeds,
                RandomStateColdStartAuthorization: new("test:exact-root", "synthetic RNG root", seeds.RootInitializationSeed));
            var sourceTrainerCalls = 0;
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => sourceTrainerCalls++;
            var staged = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(sourceRequest with
                { ProvenanceRefs = [new string('x', 257)] }); // Existing publisher refusal occurs after the actual trial and v5 continuation are durable.
            Assert.True(staged.ErrorCode == CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode,
                staged.ErrorCode + ":" + staged.ErrorMessage);
            Assert.Equal(1, sourceTrainerCalls);
            Assert.Equal(1, sourceBroker.Calls);
            var stagedEvaluationCalls = sourceEvaluator.IssuedCallCount;
            var sourceAttempt = CreateModelTrainingAttemptAddress(root, sourceRequest.TrainingRunId);
            Assert.True(CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(sourceAttempt).IsSuccess);
            var retained = SnapshotResumeFiles(root);
            var secondRunId = sourceRequest.TrainingRunId + ":second-run";
            var secondRun = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(sourceRequest with
                { TrainingRunId = secondRunId });
            Assert.False(secondRun.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode,
                secondRun.ErrorCode);
            Assert.Equal(1, sourceTrainerCalls); // Explicit zero second TrainerCalls for the same sealed pair under another run ID.
            Assert.Equal(1, sourceBroker.Calls); // Zero second legal lease acquisition for the new run.
            Assert.Equal(stagedEvaluationCalls, sourceEvaluator.IssuedCallCount); // Zero new evaluator calls before refusal.
            Assert.False(Directory.Exists(CreateModelTrainingAttemptAddress(root, secondRunId).AttemptRoot));
            AssertRetainedResumeFiles(root, retained);
            var source = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(sourceRequest with
                { QualityGate = CreateQualityGateContext(corpus, tokenizer, CreateQualityOutcomes(true, true),
                    requestIdFactory: i => "exact-profile-recovery-" + i) }); // Distinct real fixture response IDs preserve the existing cross-series exam guard during same-run evaluation/publication.
            Assert.True(source.IsSuccess, source.ErrorCode + ":" + source.ErrorMessage);
            Assert.Equal(1, sourceTrainerCalls); // Same-run recovery preserves the one actual optimizer update.
            Assert.Equal(1, sourceBroker.Calls); // Recovery does not acquire another training lease.
            _output.WriteLine("RP01_CROSS_RUN_RESERVED_PAIR_REFUSAL " + JsonSerializer.Serialize(new
                { firstFailure = staged.ErrorCode, guard = secondRun.ErrorCode, secondTrainerCalls = sourceTrainerCalls - 1,
                    secondLeaseCalls = sourceBroker.Calls - 1, secondEvaluatorCalls = sourceEvaluator.IssuedCallCount - stagedEvaluationCalls,
                    sameRunRecoveryTrainerCalls = sourceTrainerCalls - 1, sameRunPublished = source.IsSuccess,
                    pairId = lesson.Receipt.OrderedPairs.Single().PairId, sealedSha = lesson.Receipt.OrderedPairs.Single().PairFileSha256 }));
            var address = CreateModelTrainingAttemptAddress(root, sourceRequest.TrainingRunId);
            var disk = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(address);
            Assert.True(disk.IsSuccess, disk.ErrorMessage);
            var manifest = disk.Value!.Manifest;
            Assert.Equal(5, manifest.FormatVersion);
            Assert.Equal(DpoExactPairMaterial.Version, manifest.MaterialVersion);
            Assert.Equal(DpoExactPairMaterial.FramingVersion, manifest.FramingVersion);
            Assert.Equal(DpoExactPairMaterial.ObjectiveVersion, manifest.ObjectiveVersion);
            Assert.Equal(4, manifest.AllowedParameterMap!.Count);
            var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(address.AttemptRoot, manifest.OptimizerStateStep, manifest.OptimizerStateSha256).Value!.State;
            var random = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(Path.Combine(address.AttemptRoot,
                RepositoryOwnedDpoRandomStateFile.BuildFileName(manifest.RandomStateStep)), manifest.RandomStateSha256,
                manifest.RandomStateByteLength, manifest.RandomStateStep, manifest.RandomStateTopologySha256, manifest.LoraConfigurationSha256).Value!;
            Assert.NotEmpty(optimizer.Entries); Assert.Equal(optimizer.Step, random.RandomStateStep);
            var proof = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.IssueContinuationProof(address, optimizer, random);
            Assert.True(proof.IsSuccess, proof.ErrorMessage);
            proof.Value!.RequireMatches(optimizer, random);
            var changed = new AdamWPortableState(optimizer.FormatVersion, optimizer.Step + 1, optimizer.Entries);
            Assert.Equal(DpoExecutionProfile.MismatchErrorCode,
                Assert.Throws<DpoMutationAdmissionException>(() => proof.Value.RequireMatches(changed, random)).ErrorCode);

            var nextLesson = SealRepositoryOwnedDpo(root, "exact-profile-next", "v2", "doc next", "keep next", "drop next");
            ScriptedQualityGateEvaluationClient warmEvaluator = null!;
            var next = sourceRequest with
            {
                RoutingRun = CreateRoutingRun(nextLesson.Receipt, nextLesson.Pairs), DpoDatasetAddress = nextLesson.Address,
                TrainingRunId = "exact-profile:next", OptimizerStateSourceAttempt = address,
                OptimizerStateColdStartAuthorization = null, RandomStateColdStartAuthorization = null,
                QualityGate = CreateQualityGateContext(corpus, tokenizer, CreateQualityOutcomes(false, true),
                    captureClient: client => warmEvaluator = client)
            };
            var logicalCalls = 0;
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = input =>
            {
                logicalCalls++;
                Assert.Equal(optimizer.ToBytes(), input.OptimizerStateToRestore!.ToBytes());
                Assert.Equal(JsonSerializer.Serialize(random), JsonSerializer.Serialize(input.RandomStateToRestore));
                Assert.NotNull(input.ContinuationProof);
                input.ContinuationProof!.RequireMatches(input.OptimizerStateToRestore, input.RandomStateToRestore);
            };
            // Actual source manifests are changed only inside this temp fixture.
            var original = File.ReadAllBytes(disk.Value.ManifestPath);
            foreach (var bad in new[] { manifest with { FormatVersion = 4 }, manifest with { ObjectiveVersion = "wrong" }, manifest with { AllowedParameterMap = null } })
            {
                File.WriteAllBytes(disk.Value.ManifestPath, JsonSerializer.SerializeToUtf8Bytes(bad));
                var refused = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(next);
                Assert.False(refused.IsSuccess); Assert.Equal(DpoExecutionProfile.MismatchErrorCode, refused.ErrorCode);
                Assert.Equal(0, logicalCalls);
                Assert.False(Directory.Exists(CreateModelTrainingAttemptAddress(root, next.TrainingRunId).AttemptRoot));
            }
            File.WriteAllBytes(disk.Value.ManifestPath, original);
            var raw = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(next with
            { OptimizerStateSourceAttempt = null, OptimizerStateToRestore = optimizer, RandomStateToRestore = random });
            Assert.False(raw.IsSuccess); Assert.Equal(DpoExecutionProfile.MismatchErrorCode, raw.ErrorCode); Assert.Equal(0, logicalCalls);
            var warmStaged = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(next with
                { ProvenanceRefs = [new string('x', 257)] }); // Retain genuine source S1 -> applied unpublished Trial T2 using the existing publisher refusal.
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, warmStaged.ErrorCode);
            Assert.Equal(1, logicalCalls);
            Assert.Equal(2, sourceBroker.Calls);
            var nextAddress = CreateModelTrainingAttemptAddress(root, next.TrainingRunId);
            var nextManifest = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(nextAddress).Value!.Manifest;
            Assert.Equal(2, nextManifest.OptimizerStateStep); Assert.Equal(2, nextManifest.RandomStateStep);
            Assert.Equal(manifest.AllowedParameterMap, nextManifest.AllowedParameterMap);
            var trialOptimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(nextAddress.AttemptRoot,
                nextManifest.OptimizerStateStep, nextManifest.OptimizerStateSha256).Value!.State;
            var trialRandom = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(Path.Combine(nextAddress.AttemptRoot,
                RepositoryOwnedDpoRandomStateFile.BuildFileName(nextManifest.RandomStateStep)), nextManifest.RandomStateSha256,
                nextManifest.RandomStateByteLength, nextManifest.RandomStateStep, nextManifest.RandomStateTopologySha256,
                nextManifest.LoraConfigurationSha256).Value!;
            var warmRetained = SnapshotResumeFiles(root);
            var warmEvaluationCalls = warmEvaluator.IssuedCallCount;
            var missingSource = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(next with
                { OptimizerStateSourceAttempt = null }); // A trained parent and existing warm Trial cannot fall back to source-less recovery.
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode,
                missingSource.ErrorCode);
            Assert.Equal(1, logicalCalls); Assert.Equal(2, sourceBroker.Calls);
            Assert.Equal(warmEvaluationCalls, warmEvaluator.IssuedCallCount);
            AssertRetainedResumeFiles(root, warmRetained);
            var continued = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(next with
                { QualityGate = CreateQualityGateContext(corpus, tokenizer, CreateQualityOutcomes(true, true),
                    requestIdFactory: i => "exact-profile-warm-recovery-" + i) }); // Independent response IDs preserve the existing exam-series guard during ordinary warm recovery.
            Assert.True(continued.IsSuccess, continued.ErrorCode + ":" + continued.ErrorMessage);
            Assert.Equal(1, logicalCalls); Assert.Equal(2, sourceBroker.Calls); // Recovery evaluates and publishes retained T2 without another Step or training lease.
            AssertRetainedResumeFiles(root, warmRetained, allowAdditionalFiles: true); // Publication adds sealed accepted files while retaining every preexisting Trial T2 byte.
            var recoveredManifest = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(nextAddress).Value!.Manifest;
            Assert.Equal(JsonSerializer.Serialize(nextManifest), JsonSerializer.Serialize(recoveredManifest)); // Compare complete durable T2 metadata values including collection contents.
            var recoveredOptimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(nextAddress.AttemptRoot,
                recoveredManifest.OptimizerStateStep, recoveredManifest.OptimizerStateSha256).Value!.State;
            var recoveredRandom = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(Path.Combine(nextAddress.AttemptRoot,
                RepositoryOwnedDpoRandomStateFile.BuildFileName(recoveredManifest.RandomStateStep)), recoveredManifest.RandomStateSha256,
                recoveredManifest.RandomStateByteLength, recoveredManifest.RandomStateStep, recoveredManifest.RandomStateTopologySha256,
                recoveredManifest.LoraConfigurationSha256).Value!;
            Assert.Equal(trialOptimizer.ToBytes(), recoveredOptimizer.ToBytes());
            Assert.Equal(JsonSerializer.Serialize(trialRandom), JsonSerializer.Serialize(recoveredRandom));
            _output.WriteLine("RP01_ORDINARY_WARM_RECOVERY " + JsonSerializer.Serialize(new
                { stagedFailure = warmStaged.ErrorCode, missingSourceRefusal = missingSource.ErrorCode,
                    missingSourceTrainerCalls = logicalCalls - 1, missingSourceLeaseCalls = sourceBroker.Calls - 2,
                    missingSourceEvaluatorCalls = warmEvaluator.IssuedCallCount - warmEvaluationCalls,
                    resumeTrainerCalls = logicalCalls - 1, resumeLeaseCalls = sourceBroker.Calls - 2,
                    sameRunPublished = continued.IsSuccess, sourceStep = optimizer.Step, trialStep = recoveredOptimizer.Step,
                    trialRngStep = recoveredRandom.RandomStateStep, retainedT2Files = true }));
            var beforeReplay = SnapshotResumeFiles(root);
            var replay = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(next);
            Assert.False(replay.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PairAlreadyAppliedErrorCode, replay.ErrorCode);
            Assert.Equal(1, logicalCalls); AssertRetainedResumeFiles(root, beforeReplay);
            _output.WriteLine("RP01_BC exact-v5; source step1/RNG1 -> next step2/RNG2; actual adapter keys=" +
                string.Join(",", nextManifest.AllowedParameterMap!.Select(x => x.Key)) + "; raw/old/wrong/missing refused before logical; replay no second Step, files retained.");
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteFromPreparedBaselineAsync_UsesOriginalAndRealOneStepThenPublishes(bool seededOriginal)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: seededOriginal);
        var trainerEntries = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = logicalRequest =>
            {
                trainerEntries++;
                using var wire = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "baseline.json")));
                Assert.Equal(2, wire.RootElement.GetProperty("FormatVersion").GetInt32());
                Assert.False(wire.RootElement.TryGetProperty("BaselineQuality", out _));
            };
            var result = await scenario.Execute();
            Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage);
            Assert.Equal(1, trainerEntries);
            Assert.Equal(1, scenario.Broker.Calls);
            Assert.True(result.Value!.QualityDecision!.Accepted);
            Assert.Equal(0d, result.Value.QualityDecision.BaselineMeanCorrectFraction);
            Assert.Equal(1d, result.Value.QualityDecision.TrialMeanCorrectFraction);
            Assert.Equal(30, result.Value.QualityDecision.ObservedEvaluationCallCount);
            Assert.Equal(2, result.Value.PublishedCheckpointStep);
            Assert.Equal(30, scenario.Client.IssuedCallCount);
            Assert.Equal(scenario.OriginalBytes, File.ReadAllBytes(scenario.Original.Reference.BaselinePath));
            Assert.NotEqual(scenario.Parent.WeightsSha256, ComputeFileSha256(result.Value.Training.PolicyCheckpointPath));
            var marker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt);
            Assert.True(marker.IsSuccess, marker.ErrorMessage);
            var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(scenario.Attempt.AttemptRoot,
                marker.Value!.Manifest.OptimizerStateStep, marker.Value.Manifest.OptimizerStateSha256);
            Assert.True(optimizer.IsSuccess, optimizer.ErrorMessage);
            Assert.Equal(seededOriginal ? 2 : 1, optimizer.Value!.State.Step);
            if (seededOriginal)
            {
                Assert.NotNull(scenario.Inputs.OriginalLoraConfig);
                Assert.NotNull(scenario.Inputs.OriginalAdapterSeeds);
                Assert.True(marker.Value.Manifest.RandomStateRequired);
                Assert.True(File.Exists(result.Value.Training.RandomStatePath));
            }
            else Assert.Null(scenario.Inputs.OriginalLoraConfig);
            Assert.NotNull(scenario.Request.StartRequest.LoraConfig);
            var child = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(CreateModelCheckpointAddress(scenario.Root, 2));
            Assert.True(child.IsSuccess, child.ErrorMessage);
            Assert.Equal(result.Value.Publication.WeightsSha256, child.Value!.WeightsSha256);
            var baselineIds = CoordinatorRequestIds(scenario.Original.Baseline.Calibration.Calibration);
            var trialIds = CoordinatorRequestIds(result.Value.QualityDecision.TrialCalibration);
            Assert.Equal(15, baselineIds.Length);
            Assert.Equal(15, trialIds.Length);
            Assert.Empty(baselineIds.Intersect(trialIds, StringComparer.Ordinal));
            var reentry = await scenario.Execute();
            Assert.False(reentry.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PairAlreadyAppliedErrorCode, reentry.ErrorCode);
            Assert.Equal(1, scenario.Broker.Calls);
            Assert.Equal(1, trainerEntries);
            Assert.Equal(30, scenario.Client.IssuedCallCount);
            _output.WriteLine("S04_COORDINATOR_POSITIVE " + JsonSerializer.Serialize(new
            { seededOriginal, originalCalls = 15, baselineCallsAfterPrepare = 0, trainerEntries,
                broker = scenario.Broker.Calls, optimizerStep = optimizer.Value.State.Step,
                trialSha = ComputeFileSha256(result.Value.Training.PolicyCheckpointPath),
                originalSha = scenario.Original.Sha256, originalBytes = scenario.OriginalBytes.Length,
                compactBytes = result.Value.Training.BaselineAttemptStateByteLength,
                publishedStep = result.Value.PublishedCheckpointStep, reentry = reentry.ErrorCode,
                originalAndTrainingLoraRemainSeparate = true, parentBootstrapUpdates = seededOriginal ? 1 : 0,
                runtime = CoordinatorRuntimeIdentity() }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteFromPreparedBaselineAsync_CommittedAttemptResumesWithoutRetraining(bool seededOriginal)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: seededOriginal);
        scenario.BlockPublicationAfterCompleteTrial();
        var trainerEntries = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => trainerEntries++;
            var staged = await scenario.Execute();
            Assert.False(staged.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, staged.ErrorCode);
            Assert.Equal(1, trainerEntries);
            Assert.Equal(1, scenario.Broker.Calls);
            scenario.RemovePublicationBlocker();
            Assert.True(File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")));
            Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
            var saved = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenPreparedExistingAsync(scenario.Attempt, scenario.Inputs);
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            Assert.Equal(2, saved.Value!.State.FormatVersion);
            Assert.Equal(JsonSerializer.Serialize(scenario.Original.Baseline), JsonSerializer.Serialize(saved.Value.State.BaselineQuality));
            var preserved = SnapshotResumeFiles(scenario.Root);
            var stagingCalls = scenario.Client.IssuedCallCount;
            if (seededOriginal)
            {
                // The original parent RNG1 is stale for this committed RNG2
                // attempt. Supplying it must still fail before evaluation.
                var staleParentState = await scenario.Execute();
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode,
                    staleParentState.ErrorCode);
                Assert.Equal(1, trainerEntries);
                Assert.Equal(1, scenario.Broker.Calls);
                Assert.Equal(stagingCalls, scenario.Client.IssuedCallCount);
                AssertRetainedResumeFiles(scenario.Root, preserved, allowAdditionalFiles: false);
            }
            var resumed = scenario.ReconstructForEvaluation("recovery");
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecuteFromPreparedBaselineAsync(resumed.Request, resumed.Inputs);
            Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage);
            Assert.Equal(1, trainerEntries);
            Assert.Equal(1, scenario.Broker.Calls);
            Assert.Equal(0, resumed.Broker.Calls);
            Assert.Equal(stagingCalls, scenario.Client.IssuedCallCount);
            Assert.Equal(0, resumed.Client.IssuedCallCount);
            Assert.Equal(scenario.OriginalBytes, File.ReadAllBytes(scenario.Original.Reference.BaselinePath));
            AssertRetainedResumeFiles(scenario.Root, preserved, allowAdditionalFiles: true);
            Assert.Empty(CoordinatorRequestIds(result.Value!.QualityDecision!.BaselineCalibration)
                .Intersect(CoordinatorRequestIds(result.Value.QualityDecision.TrialCalibration), StringComparer.Ordinal));
            var child = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(CreateModelCheckpointAddress(scenario.Root, 2));
            Assert.True(child.IsSuccess, child.ErrorMessage);
            Assert.Equal(result.Value.Publication.WeightsSha256, child.Value!.WeightsSha256);
            _output.WriteLine("S04_COORDINATOR_RECOVERY " + JsonSerializer.Serialize(new
            { seededOriginal, stagedFailure = staged.ErrorCode, trainerEntries, firstBroker = scenario.Broker.Calls,
                recoveryBroker = resumed.Broker.Calls, originalCalls = 15, stagingCalls,
                trialCalls = resumed.Client.IssuedCallCount, publishedStep = result.Value.PublishedCheckpointStep,
                fullOriginalGraphExact = true, staleParentRngRejectedBeforeEvaluation = seededOriginal,
                originalSha = scenario.Original.Sha256,
                compactBytes = saved.Value.BaselineStateByteLength, filesRetained = preserved.Count }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Fact]
    public async Task ExecuteFromPreparedBaselineAsync_RejectsSecondRequestedStep()
    {
        using var scenario = await CreatePreparedCoordinatorScenario();
        var snapshot = SnapshotResumeFiles(scenario.Root);
        var result = await scenario.Execute(scenario.Request with { DpoStepCount = 2 });
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.InvalidErrorCode, result.ErrorCode);
        Assert.Equal(15, scenario.Client.IssuedCallCount);
        Assert.Equal(0, scenario.Broker.Calls);
        AssertRetainedResumeFiles(scenario.Root, snapshot, allowAdditionalFiles: false);
        Assert.False(Directory.Exists(scenario.Attempt.AttemptRoot));
        _output.WriteLine("S04_COORDINATOR_STEP2_ZERO_EFFECT");
    }

    [Theory]
    [InlineData(1, 5000)]
    [InlineData(2, 2500)]
    [InlineData(200, 25)]
    public async Task ExecuteFromPreparedBaselineAsync_SupportedMaximumOriginalGraphsReachCoordinator_PretrainingAuthorizationRefusal(
        int caseCount, int repeats)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(caseCount: caseCount, repeats: repeats);
        // Explicitly omit root optimizer authorization. This tests the complete
        // producer->storage->reader->coordinator graph, not successful training.
        var request = scenario.Request with { OptimizerStateColdStartAuthorization = null };
        var result = await scenario.Execute(request);
        Assert.False(result.IsSuccess);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OptimizerStateColdStartAuthorizationRequiredErrorCode,
            result.ErrorCode);
        Assert.Equal(5000, scenario.Client.IssuedCallCount);
        Assert.Equal(0, scenario.Broker.Calls);
        Assert.False(Directory.Exists(scenario.Attempt.AttemptRoot));
        var prepared = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
            .ValidatePreparedAsync(scenario.Attempt, scenario.Inputs, CancellationToken.None);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        Assert.Equal(JsonSerializer.Serialize(scenario.Original.Baseline), JsonSerializer.Serialize(prepared.Value!.State.BaselineQuality));
        var report = prepared.Value.State.BaselineQuality.Calibration.Calibration;
        Assert.Equal(repeats, report.Observations.Count);
        Assert.Equal(5000, CoordinatorRequestIds(report).Length);
        Assert.Equal(5000, CoordinatorRequestIds(report).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(scenario.OriginalBytes, File.ReadAllBytes(scenario.Original.Reference.BaselinePath));
        _output.WriteLine("S04_COORDINATOR_MAX_GRAPH " + JsonSerializer.Serialize(new
        { caseCount, repeats, fullCalls = 5000, broker = 0, trainer = 0, guard = result.ErrorCode,
            originalBytes = scenario.OriginalBytes.Length, compactCanonicalBytes = prepared.Value.Bytes.Length,
            graphExact = true, originalSha = scenario.Original.Sha256, successfulTraining = false }));
    }

    [Theory]
    [InlineData("missing-claim")]
    [InlineData("corrupt-claim")]
    [InlineData("missing-baseline")]
    [InlineData("corrupt-baseline")]
    [InlineData("missing-marker")]
    [InlineData("corrupt-marker")]
    [InlineData("missing-optimizer")]
    [InlineData("corrupt-optimizer")]
    [InlineData("missing-rng")]
    [InlineData("corrupt-rng")]
    [InlineData("missing-trial")]
    [InlineData("corrupt-trial")]
    [InlineData("runtime")]
    [InlineData("reference")]
    public async Task ExecuteFromPreparedBaselineAsync_IncompleteContinuationCannotRetrain(string mutation)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: true);
        scenario.BlockPublicationAfterCompleteTrial();
        var trainerEntries = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => trainerEntries++;
            var staged = await scenario.Execute();
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, staged.ErrorCode);
            scenario.RemovePublicationBlocker();
            Assert.Equal(1, trainerEntries);
            var marker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt);
            Assert.True(marker.IsSuccess, marker.ErrorMessage);
            var m = marker.Value!.Manifest;
            var target = mutation.Split('-').Last() switch
            {
                "claim" => scenario.Attempt.AttemptClaimPath,
                "baseline" => Path.Combine(scenario.Attempt.AttemptRoot, "baseline.json"),
                "marker" => marker.Value.ManifestPath,
                "optimizer" => Path.Combine(scenario.Attempt.AttemptRoot, RepositoryOwnedDpoOptimizerStateFile.BuildFileName(m.OptimizerStateStep)),
                "rng" => Path.Combine(scenario.Attempt.AttemptRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(m.RandomStateStep)),
                "trial" => Path.Combine(scenario.Attempt.AttemptRoot, DpoTrainer.BuildPolicyCheckpointFileName(1)),
                _ => ""
            };
            if (mutation.StartsWith("missing-", StringComparison.Ordinal)) File.Delete(target);
            else if (mutation.StartsWith("corrupt-", StringComparison.Ordinal)) File.AppendAllText(target, "corrupt");
            else if (mutation == "runtime") MutateOptimizerStateManifest(marker.Value.ManifestPath,
                node => node["ModelShape"]!["RopeTheta"] = scenario.Request.StartRequest.ModelConfig.RopeTheta + 1);
            var recovered = scenario.ReconstructForEvaluation("recovery-negative");
            if (mutation == "reference") recovered = recovered with { Inputs = recovered.Inputs with
                { Reference = ChangedReference(scenario.Original.Reference, sha: Hex('f')) } };
            var snapshot = SnapshotResumeFiles(scenario.Root);
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecuteFromPreparedBaselineAsync(recovered.Request, recovered.Inputs);
            Assert.False(result.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode,
                result.ErrorCode);
            Assert.Equal(1, trainerEntries);
            Assert.Equal(1, scenario.Broker.Calls);
            Assert.Equal(0, recovered.Broker.Calls);
            Assert.Equal(0, recovered.Client.IssuedCallCount);
            Assert.Equal(0, recovered.Client.IssuedCallCount);
            AssertRetainedResumeFiles(scenario.Root, snapshot, allowAdditionalFiles: false);
            Assert.Equal(scenario.Parent.ManifestSha256,
                CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(scenario.Request.StartRequest).Value!.ParentManifest!.ManifestSha256);
            _output.WriteLine("S04_COORDINATOR_RECOVERY_REFUSAL " + JsonSerializer.Serialize(new
                { mutation, guard = result.ErrorCode, retraining = 0, broker = recovered.Broker.Calls,
                    evaluator = recovered.Client.IssuedCallCount, filesRetained = snapshot.Count }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData("config", "before")]
    [InlineData("runtime", "before")]
    [InlineData("quality", "before")]
    [InlineData("profile", "before")]
    [InlineData("tokenizer", "before")]
    [InlineData("pair", "before")]
    [InlineData("receipt", "before")]
    [InlineData("reference", "before")]
    [InlineData("selector", "before")]
    [InlineData("parent", "before")]
    [InlineData("config", "broker")]
    [InlineData("quality", "broker")]
    [InlineData("tokenizer", "broker")]
    [InlineData("pair", "broker")]
    [InlineData("receipt", "broker")]
    [InlineData("provenance", "broker")]
    [InlineData("lora", "broker")]
    [InlineData("pair-file", "broker")]
    [InlineData("config", "trial")]
    [InlineData("quality", "trial")]
    [InlineData("tokenizer", "trial")]
    [InlineData("provenance", "trial")]
    public async Task ExecuteFromPreparedBaselineAsync_ChangedInputsCloseBeforeEffects(string mutation, string boundary)
    {
        using var scenario = await CreatePreparedCoordinatorScenario();
        var trainerEntries = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        void Change()
        {
            switch (mutation)
            {
                case "config": typeof(ModelConfig).GetProperty(nameof(ModelConfig.RopeTheta))!
                        .SetValue(scenario.Request.StartRequest.ModelConfig, 12000f); break;
                case "runtime": scenario.Inputs = scenario.Inputs with { RuntimeCompatibility =
                    scenario.Inputs.RuntimeCompatibility with { ConfigSha256 = Hex('e') } }; break;
                case "quality": ((string[])scenario.Request.QualityGate!.EvidenceRefs)[0] = "test:changed"; break;
                case "profile": scenario.Request = scenario.Request with { QualityGate = scenario.Request.QualityGate! with
                    { EvaluatorConfiguration = CreateTeacherConfiguration(30, modelId: "qwen-changed:9b") } }; break;
                case "tokenizer": scenario.Request.Tokenizer.Train("Changed tokenizer vocabulary and merges", 280); break;
                case "pair": scenario.Request.RoutingRun.RepositoryOwnedDpoPairs[0] = scenario.Dpo.Pairs[0] with { Prompt = "changed" }; break;
                case "receipt": scenario.Request.RoutingRun.RepositoryOwnedDpoReceipt!.OrderedPairs[0] =
                    scenario.Request.RoutingRun.RepositoryOwnedDpoReceipt.OrderedPairs[0] with { PairFileSha256 = Hex('e') }; break;
                case "reference": scenario.Inputs = scenario.Inputs with { Reference = ChangedReference(scenario.Original.Reference, sha: Hex('e')) }; break;
                case "selector": scenario.Inputs = scenario.Inputs with { Selector = new("different", "different") }; break;
                case "parent": File.AppendAllText(CreateModelCheckpointAddress(scenario.Root, 1).WeightsPath, "corrupt"); break;
                case "provenance": ((string[])scenario.Request.ProvenanceRefs)[0] = "test:changed"; break;
                case "lora": scenario.Request.LoraConfig.TargetModules[0] = "k_proj"; break;
                case "pair-file": File.AppendAllText(Directory.GetFiles(scenario.Dpo.Address.PairsRoot, "*.json").Single(), "corrupt"); break;
            }
        }
        if (boundary == "before") Change();
        if (boundary == "broker") scenario.Broker.BeforeAcquire = Change;
        if (boundary == "trial") scenario.TrialResponse = (number, response) => { if (number == 16) Change(); return response; };
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => trainerEntries++;
            var result = await scenario.Execute();
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.ErrorCode);
            Assert.Equal(boundary == "trial" ? 1 : 0, trainerEntries);
            Assert.Equal(boundary == "before" ? 0 : 1, scenario.Broker.Calls);
            if (boundary == "trial")
            {
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, result.ErrorCode);
                Assert.True(CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt).IsSuccess);
            }
            else Assert.False(Directory.Exists(scenario.Attempt.AttemptRoot));
            Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
            Assert.Equal(scenario.OriginalBytes, File.ReadAllBytes(scenario.Original.Reference.BaselinePath));
            _output.WriteLine("S04_COORDINATOR_CHANGED_INPUTS " + JsonSerializer.Serialize(new
                { mutation, boundary, guard = result.ErrorCode, trainerEntries, broker = scenario.Broker.Calls,
                    actualUpdatePreserved = boundary == "trial", publication = 0 }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Fact]
    public async Task ExecuteFromPreparedBaselineAsync_NotImprovedRetainsParentWithoutRetry()
    {
        using var scenario = await CreatePreparedCoordinatorScenario(trialPass: false);
        var trainerEntries = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => trainerEntries++;
            var result = await scenario.Execute();
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode, result.ErrorCode);
            Assert.Equal(1, trainerEntries);
            Assert.Equal(30, scenario.Client.IssuedCallCount);
            var recovered = scenario.ReconstructForEvaluation("negative-recovery", trialPass: false);
            var retry = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecuteFromPreparedBaselineAsync(recovered.Request, recovered.Inputs);
            Assert.Equal(result.ErrorCode, retry.ErrorCode);
            Assert.Equal(1, trainerEntries);
            Assert.Equal(0, recovered.Broker.Calls);
            Assert.Equal(scenario.Parent.ManifestSha256,
                CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(scenario.Request.StartRequest).Value!.ParentManifest!.ManifestSha256);
            Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
            Assert.Equal(scenario.OriginalBytes, File.ReadAllBytes(scenario.Original.Reference.BaselinePath));
            _output.WriteLine("S04_COORDINATOR_COMPLETE_NOT_IMPROVED " + JsonSerializer.Serialize(new
                { guard = result.ErrorCode, trainerEntries, retryUpdate = 0, parentRetained = true, typedNegative = false }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData("entry")]
    [InlineData("broker")]
    [InlineData("trial")]
    public async Task ExecuteFromPreparedBaselineAsync_CancellationPreservesActualEntryBoundary(string boundary)
    {
        using var scenario = await CreatePreparedCoordinatorScenario();
        using var cancellation = new CancellationTokenSource();
        var trainerEntries = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        if (boundary == "entry") cancellation.Cancel();
        if (boundary == "broker") scenario.Broker.BeforeAcquire = cancellation.Cancel;
        if (boundary == "trial") scenario.TrialResponse = (_, response) => { cancellation.Cancel(); return response; };
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => trainerEntries++;
            if (boundary == "trial")
            {
                var cancelled = await scenario.Execute(token: cancellation.Token);
                Assert.Equal(CodeDocumentationSavedModelAssessmentService.CancelledErrorCode, cancelled.ErrorCode);
            }
            else await Assert.ThrowsAnyAsync<OperationCanceledException>(() => scenario.Execute(token: cancellation.Token));
            Assert.Equal(boundary == "trial" ? 1 : 0, trainerEntries);
            Assert.Equal(boundary == "entry" ? 0 : 1, scenario.Broker.Calls);
            if (boundary == "trial")
            {
                Assert.True(CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt).IsSuccess);
                var recovered = scenario.ReconstructForEvaluation("cancel-recovery");
                var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                    .ExecuteFromPreparedBaselineAsync(recovered.Request, recovered.Inputs);
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, result.ErrorCode);
                Assert.Equal(1, trainerEntries);
                Assert.Equal(0, recovered.Broker.Calls);
                Assert.Equal(0, recovered.Client.IssuedCallCount);
            }
            else Assert.False(Directory.Exists(scenario.Attempt.AttemptRoot));
            _output.WriteLine("S04_COORDINATOR_CANCELLATION " + JsonSerializer.Serialize(new
                { boundary, trainerEntries, broker = scenario.Broker.Calls, committedUpdate = boundary == "trial" }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteFromPreparedBaselineAsync_RecoveryRejectsLegacyAndPublicRejectsPrepared(bool preparedIntoPublic)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(stageTrialFailure: true);
        if (!preparedIntoPublic)
        {
            var legacyContext = scenario.Request.QualityGate! with
            {
                EvaluationClient = new ScriptedQualityGateEvaluationClient(scenario.Request.QualityGate!.EvaluatorConfiguration,
                    Enumerable.Repeat(false, 15).ToArray(), i => "legacy-stage-" + i)
            };
            var legacy = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .TrainAndPublishAsync(scenario.Request with { QualityGate = legacyContext });
            Assert.False(legacy.IsSuccess);
        }
        else
        {
            var prepared = await scenario.Execute();
            Assert.Equal("SCRIPTED_QUALITY_GATE_DECISION_MISSING", prepared.ErrorCode);
        }
        var recovered = scenario.ReconstructForEvaluation("isolation");
        var before = SnapshotResumeFiles(scenario.Root);
        var result = preparedIntoPublic
            ? await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(recovered.Request)
            : await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecuteFromPreparedBaselineAsync(recovered.Request, recovered.Inputs);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Equal(0, recovered.Broker.Calls);
        Assert.Equal(0, recovered.Client.IssuedCallCount);
        AssertRetainedResumeFiles(scenario.Root, before, allowAdditionalFiles: false);
        _output.WriteLine("S04_COORDINATOR_FORMAT_ISOLATION " + JsonSerializer.Serialize(new { preparedIntoPublic, guard = result.ErrorCode }));
    }

    [Theory]
    [InlineData("Improved", 0, true, false)]
    [InlineData("Regressed", 4, false, false)]
    [InlineData("Indistinguishable", 0, false, false)]
    [InlineData("IdenticalWeights", 0, true, true)]
    public async Task ExecuteFromPreparedBaselineAsync_AllCompleteDispositionsAreDurableBeforeLegacyMapping(
        string disposition, int baselinePassCount, bool trialPass, bool zeroWeightParent)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(baselinePassCount: baselinePassCount,
            trialPass: trialPass, zeroWeightParent: zeroWeightParent);
        if (disposition == "Improved") scenario.BlockPublicationAfterCompleteTrial();
        var parentFiles = SnapshotResumeFiles(scenario.Root);
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting; var updates = 0;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => updates++;
            var result = await scenario.Execute();
            Assert.Equal(disposition == "Improved" ? CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode :
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode, result.ErrorCode);
            Assert.Equal(1, updates); Assert.Equal(1, scenario.Broker.Calls); Assert.Equal(30, scenario.Client.IssuedCallCount);
            AssertRetainedResumeFiles(scenario.Root, parentFiles, allowAdditionalFiles: true);
            var receipt = await ReadCurrentPreparedTrial(scenario);
            Assert.True(receipt.IsSuccess, receipt.ErrorCode + ":" + receipt.ErrorMessage);
            Assert.Equal(disposition, receipt.Value!.Detailed.Disposition.ToString());
            var path = Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json");
            using var wire = JsonDocument.Parse(File.ReadAllBytes(path));
            Assert.Equal(new[] { "Schema", "FormatVersion", "StartedSha256", "Disposition", "TrialSha256", "DerivedEvidenceSha256", "Trial" },
                wire.RootElement.EnumerateObject().Select(p => p.Name));
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(receipt.Value.Detailed.Trial),
                Encoding.UTF8.GetBytes(wire.RootElement.GetProperty("Trial").GetRawText()));
            Assert.Equal(JsonSerializer.Serialize(scenario.Original.Baseline), JsonSerializer.Serialize(receipt.Value.Detailed.Baseline));
            Assert.Equal(15, CoordinatorRequestIds(receipt.Value.Detailed.Trial.Calibration).Length);
            Assert.Equal(receipt.Value.CompletedSha256, ComputeFileSha256(path));
            var marker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt);
            Assert.True(marker.IsSuccess, marker.ErrorMessage); Assert.Equal(zeroWeightParent ? 2 : 1, marker.Value!.Manifest.OptimizerStateStep);
            if (zeroWeightParent) Assert.Equal(scenario.Parent.WeightsSha256, marker.Value.Manifest.TrialWeightsSha256);
            else Assert.NotEqual(scenario.Parent.WeightsSha256, marker.Value.Manifest.TrialWeightsSha256);
            var snapshot = SnapshotResumeFiles(scenario.Root);
            var reRead = await ReadCurrentPreparedTrial(scenario);
            Assert.True(reRead.IsSuccess, reRead.ErrorMessage);
            Assert.Equal(receipt.Value.CompletedSha256, reRead.Value!.CompletedSha256);
            Assert.Equal(JsonSerializer.Serialize(receipt.Value.Detailed), JsonSerializer.Serialize(reRead.Value.Detailed));
            if (disposition != "Improved")
            {
                var fresh = scenario.ReconstructForEvaluation("must-not-issue");
                var repeated = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteFromPreparedBaselineAsync(fresh.Request, fresh.Inputs);
                Assert.Equal(result.ErrorCode, repeated.ErrorCode); Assert.Equal(result.ErrorMessage, repeated.ErrorMessage);
                Assert.Equal(0, fresh.Client.IssuedCallCount); Assert.Equal(0, fresh.Broker.Calls);
            }
            AssertRetainedResumeFiles(scenario.Root, snapshot, allowAdditionalFiles: false);
            Assert.Equal(30, scenario.Client.IssuedCallCount); Assert.Equal(1, updates);
            Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
            _output.WriteLine("S04_DURABLE_COMPLETE_OUTCOME " + JsonSerializer.Serialize(new { disposition, updates,
                originalCalls = 15, trialCalls = 15, reopenedNewCalls = 0, receipt.Value.CompletedSha256,
                receipt.Value.StartedSha256, bytes = new FileInfo(path).Length, fullGraphExact = true, filesExact = snapshot.Count }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteFromPreparedBaselineAsync_IssuedButMissingResponseNeverRetriesTrial(bool seededOriginal)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: seededOriginal, stageTrialFailure: true);
        var staged = await scenario.Execute();
        Assert.Equal("SCRIPTED_QUALITY_GATE_DECISION_MISSING", staged.ErrorCode);
        Assert.Equal(16, scenario.Client.IssuedCallCount);
        Assert.True(File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json")));
        Assert.False(File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")));
        var before = SnapshotResumeFiles(scenario.Root); var fresh = scenario.ReconstructForEvaluation("lost-response");
        var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteFromPreparedBaselineAsync(fresh.Request, fresh.Inputs);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Equal(0, fresh.Client.IssuedCallCount); Assert.Equal(0, fresh.Broker.Calls);
        Assert.Equal(16, scenario.Client.IssuedCallCount); Assert.Equal(1, scenario.Broker.Calls);
        AssertRetainedResumeFiles(scenario.Root, before);
        _output.WriteLine("S04_DURABLE_LOST_RESPONSE " + JsonSerializer.Serialize(new { seededOriginal, historicalIssued = 16,
            newCalls = 0, newUpdate = 0, newPublish = 0, filesExact = before.Count, guard = result.ErrorCode }));
    }

    [Theory]
    [InlineData("missing-started")]
    [InlineData("missing-completed")]
    [InlineData("started-version")]
    [InlineData("started-inputs")]
    [InlineData("started-passport")]
    [InlineData("started-unknown")]
    [InlineData("started-missing-field")]
    [InlineData("started-duplicate")]
    [InlineData("started-alias")]
    [InlineData("started-number-type")]
    [InlineData("started-over-cap")]
    [InlineData("started-overflow")]
    [InlineData("started-reparse")]
    [InlineData("completed-version")]
    [InlineData("completed-unknown")]
    [InlineData("completed-missing-field")]
    [InlineData("completed-duplicate")]
    [InlineData("completed-trailing")]
    [InlineData("completed-space")]
    [InlineData("completed-partial")]
    [InlineData("completed-invalid-utf8")]
    [InlineData("completed-invalid-disposition")]
    [InlineData("completed-original-copy")]
    [InlineData("completed-response-id")]
    [InlineData("completed-duplicate-response")]
    [InlineData("completed-nested-duplicate")]
    [InlineData("completed-nested-missing")]
    [InlineData("completed-counter")]
    [InlineData("completed-report")]
    [InlineData("completed-null")]
    [InlineData("completed-over-cap")]
    [InlineData("completed-overflow")]
    [InlineData("started-unsafe-mode")]
    [InlineData("completed-unsafe-mode")]
    [InlineData("completed-directory")]
    public async Task ExecuteFromPreparedBaselineAsync_ClosedTrialRecoveryRejectsAtDurableOwnerWithoutEffects(string mutation)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: true);
        scenario.BlockPublicationAfterCompleteTrial();
        var staged = await scenario.Execute();
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, staged.ErrorCode);
        scenario.RemovePublicationBlocker();
        var control = await ReadCurrentPreparedTrial(scenario);
        Assert.True(control.IsSuccess, control.ErrorCode + ":" + control.ErrorMessage);
        var started = Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json");
        var completed = Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json");
        var path = mutation.StartsWith("started", StringComparison.Ordinal) || mutation == "missing-started" ? started : completed;
        var bytes = File.ReadAllBytes(path); var text = Encoding.UTF8.GetString(bytes); var node = JsonNode.Parse(text)!.AsObject();
        switch (mutation)
        {
            case "missing-started": case "missing-completed": File.Delete(path); break;
            case "completed-directory": File.Delete(path); Directory.CreateDirectory(path); break;
            case "started-reparse": File.Delete(path); File.CreateSymbolicLink(path, completed); break;
            case "started-unsafe-mode": case "completed-unsafe-mode":
                if (OperatingSystem.IsLinux()) File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupWrite);
                else { File.Delete(path); Directory.CreateDirectory(path); }
                break;
            case "started-duplicate": case "completed-duplicate": File.WriteAllText(path, text.Replace("{", "{\"FormatVersion\":1,", StringComparison.Ordinal)); break;
            case "completed-nested-duplicate": File.WriteAllText(path, text.Replace("\"Calibration\":{", "\"Calibration\":{\"RepeatCount\":3,", StringComparison.Ordinal)); break;
            case "completed-trailing": File.AppendAllText(path, "{}"); break;
            case "completed-space": File.AppendAllText(path, " "); break;
            case "completed-partial": File.WriteAllBytes(path, bytes[..^1]); break;
            case "completed-invalid-utf8": File.WriteAllBytes(path, bytes.Concat(new byte[] { 0xff }).ToArray()); break;
            case "started-over-cap": File.AppendAllText(path, new string(' ', CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.MaximumTrialStartedBytes)); break;
            case "completed-over-cap": File.AppendAllText(path, new string(' ', CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.MaximumTrialCompletedBytes)); break;
            case "started-overflow": File.WriteAllText(path, text.Replace("\"ExpectedTrialEvaluationCallCount\":15", "\"ExpectedTrialEvaluationCallCount\":999999999999999999999999", StringComparison.Ordinal)); break;
            case "completed-overflow": File.WriteAllText(path, text.Replace("\"ObservedEvaluationCallCount\":15", "\"ObservedEvaluationCallCount\":999999999999999999999999", StringComparison.Ordinal)); break;
            default:
                switch (mutation)
                {
                    case "started-version": case "completed-version": node["FormatVersion"] = 9; break;
                    case "started-inputs": node["PreparedInputsSha256"] = Hex('e'); break;
                    case "started-passport": node["TrialPassport"]!["WeightsSha256"] = scenario.Parent.WeightsSha256; break;
                    case "started-unknown": case "completed-unknown": node["Foreign"] = "value"; break;
                    case "started-missing-field": case "completed-missing-field": node.Remove("Schema"); break;
                    case "started-alias": node["schema"] = node["Schema"]!.DeepClone(); node.Remove("Schema"); break;
                    case "started-number-type": node["ExpectedTrialEvaluationCallCount"] = "15"; break;
                    case "completed-invalid-disposition": node["Disposition"] = "InvalidOrIncomplete"; break;
                    case "completed-original-copy": node["Baseline"] = JsonSerializer.SerializeToNode(scenario.Original.Baseline); break;
                    case "completed-response-id": node["Trial"]!["Calibration"]!["Observations"]![0]!["ObservedEvaluationRequestIds"]![0] = "foreign-response"; break;
                    case "completed-duplicate-response": node["Trial"]!["Calibration"]!["Observations"]![1]!["ObservedEvaluationRequestIds"]![0] =
                        node["Trial"]!["Calibration"]!["Observations"]![0]!["ObservedEvaluationRequestIds"]![0]!.DeepClone(); break;
                    case "completed-nested-missing": node["Trial"]!["Calibration"]!["Observations"]![0]!.AsObject().Remove("CaseIds"); break;
                    case "completed-counter": node["Trial"]!["Calibration"]!["ObservedEvaluationCallCount"] = 14; break;
                    case "completed-report": node["Trial"]!["RepresentativeAssessment"]!["ModelIdentitySha256"] = scenario.Parent.WeightsSha256; break;
                    case "completed-null": node["Trial"] = null; break;
                }
                File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(node)); break;
        }
        var snapshot = SnapshotResumeFiles(scenario.Root); var fresh = scenario.ReconstructForEvaluation("forbidden-recovery");
        var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteFromPreparedBaselineAsync(fresh.Request, fresh.Inputs);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Equal(0, fresh.Client.IssuedCallCount); Assert.Equal(0, fresh.Broker.Calls);
        Assert.Equal(30, scenario.Client.IssuedCallCount); Assert.Equal(1, scenario.Broker.Calls);
        AssertRetainedResumeFiles(scenario.Root, snapshot);
        Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
        _output.WriteLine("S04_DURABLE_WIRE_REFUSAL " + JsonSerializer.Serialize(new { mutation, validControl = control.Value!.CompletedSha256,
            intendedOwner = "BaselineAttemptJournal completed admission", guard = result.ErrorCode, newCalls = 0, retainedFiles = snapshot.Count }));
    }

    [Theory]
    [InlineData(200, 25)]
    [InlineData(1, 5000)]
    [InlineData(2, 2500)]
    public async Task ExecuteFromPreparedBaselineAsync_CompleteMaximumTrialRetainsAll5000ActualResponses(int cases, int repeats)
    {
        var runId = "trial-" + new string('x', 225);
        using var scenario = await CreatePreparedCoordinatorScenario(caseCount: cases, repeats: repeats, runIdOverride: runId);
        scenario.BlockPublicationAfterCompleteTrial();
        var result = await scenario.Execute();
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, result.ErrorCode);
        Assert.Equal(10000, scenario.Client.IssuedCallCount); Assert.Equal(1, scenario.Broker.Calls);
        var opened = await ReadCurrentPreparedTrial(scenario); Assert.True(opened.IsSuccess, opened.ErrorCode + ":" + opened.ErrorMessage);
        var graph = opened.Value!.Detailed.Trial; var ids = CoordinatorRequestIds(graph.Calibration);
        Assert.Equal(repeats, graph.Calibration.Observations.Count); Assert.Equal(5000, ids.Length);
        Assert.Equal(5000, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.Empty(ids.Intersect(CoordinatorRequestIds(scenario.Original.Baseline.Calibration.Calibration), StringComparer.Ordinal));
        Assert.All(graph.Calibration.Observations, row => { Assert.Equal(cases, row.CaseIds.Count);
            Assert.Equal(cases, row.StudentAnswerSha256.Count); Assert.Equal(cases, row.ObservedEvaluationRequestIds.Count); Assert.True(row.AssessmentRunId.Length <= 256); });
        Assert.Equal(repeats, graph.Calibration.MeasurementNoisePassport.RepeatedScores.Count);
        Assert.Equal(cases, graph.RepresentativeAssessment.Cases.Count);
        var snapshot = SnapshotResumeFiles(scenario.Root); var fresh = await ReadCurrentPreparedTrial(scenario);
        Assert.True(fresh.IsSuccess, fresh.ErrorCode); Assert.Equal(opened.Value.CompletedSha256, fresh.Value!.CompletedSha256);
        Assert.Equal(JsonSerializer.Serialize(graph), JsonSerializer.Serialize(fresh.Value.Detailed.Trial));
        AssertRetainedResumeFiles(scenario.Root, snapshot); Assert.Equal(10000, scenario.Client.IssuedCallCount);
        var bytes = new FileInfo(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")).Length;
        var bound = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes(scenario.Request.QualityGate!);
        Assert.True(bytes <= bound); Assert.True(bound <= CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.MaximumTrialCompletedBytes);
        _output.WriteLine("S04_DURABLE_MAX_TRIAL " + JsonSerializer.Serialize(new { cases, repeats, actualOriginalCalls = 5000,
            actualTrialCalls = 5000, optimizerEntries = 1, newReopenCalls = 0, fullTrialBytes = bytes, upperBoundBytes = bound,
            opened.Value.CompletedSha256, longAsciiRunUTF16 = runId.Length, wholeOptimizerRoute = true }));
        AssertMaximumStorageProjection(graph, cases, repeats);
    }

    [Fact]
    public async Task ExecuteFromPreparedBaselineAsync_ForeignPairScalarCannotMintOrReopenTrialEvidence()
    {
        using var scenario = await CreatePreparedCoordinatorScenario(trialPass: false);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode, (await scenario.Execute()).ErrorCode);
        var valid = await ReadCurrentPreparedTrial(scenario); Assert.True(valid.IsSuccess, valid.ErrorMessage);
        var parentAddress = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(scenario.Root,
            scenario.Attempt.DomainSlug, scenario.Attempt.DesignPatternSlug, "v0-parent");
        Assert.True(parentAddress.IsSuccess, parentAddress.ErrorMessage);
        var parentDataset = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedForManifestIntegrity(parentAddress.Value!);
        Assert.True(parentDataset.IsSuccess, parentDataset.ErrorMessage);
        var foreign = parentDataset.Value!.Manifest.OrderedPairs.Single().PairId;
        var snapshot = SnapshotResumeFiles(scenario.Root);
        var result = await ReadCurrentPreparedTrial(scenario, foreign);
        Assert.False(result.IsSuccess); Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.InvalidErrorCode, result.ErrorCode);
        Assert.Equal(30, scenario.Client.IssuedCallCount); AssertRetainedResumeFiles(scenario.Root, snapshot);
    }

    [Fact]
    public async Task ExecuteFromPreparedBaselineAsync_UnsupportedTrialRootFailsBeforeOptimizerEffects()
    {
        using var scenario = await CreatePreparedCoordinatorScenario(runIdOverride: new string('x', 240));
        var before = SnapshotResumeFiles(scenario.Root); var result = await scenario.Execute();
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.InvalidErrorCode, result.ErrorCode);
        Assert.Equal(15, scenario.Client.IssuedCallCount); Assert.Equal(0, scenario.Broker.Calls);
        Assert.False(Directory.Exists(scenario.Attempt.AttemptRoot)); AssertRetainedResumeFiles(scenario.Root, before);
    }

    private static async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>>
        ReadCurrentPreparedTrial(PreparedCoordinatorScenario scenario, string? pairOverride = null)
    {
        var (_, exam) = CurrentTrialExam(scenario);
        return await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.OpenCompletedTrialAsync(scenario.Attempt, scenario.Inputs,
            exam, pairOverride ?? scenario.Dpo.Receipt.OrderedPairs.Single().PairId);
    }

    private static (CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad Trial, CodeDocumentationSavedModelAssessmentRequest Exam)
        CurrentTrialExam(PreparedCoordinatorScenario scenario)
    {
        var loaded = CodeDocumentationRepositoryOwnedModelCheckpointStartService.LoadTrainingAttemptCheckpoint(scenario.Request.StartRequest, scenario.Attempt, 1);
        Assert.True(loaded.IsSuccess, loaded.ErrorCode + ":" + loaded.ErrorMessage); var trial = loaded.Value!; var p = scenario.Parent;
        var passport = new CodeDocumentationSavedModelAssessmentModelPassport(p.Domain, p.DesignPattern, p.ModelId, p.Version, p.Step + 1,
            "", "", trial.RepositoryRelativeWeightsPath, trial.WeightsSha256, trial.WeightsByteLength, scenario.Request.StartRequest.MaximumCheckpointBytes,
            CodeDocumentationSavedModelAssessmentModelPassport.TrialTrainingAttemptCheckpointSourceKind, trial.RepositoryRelativeWeightsPath, trial.WeightsSha256);
        var run = scenario.Request.TrainingRunId + ":quality-trial";
        if (run.Length > 256) run = "code-doc-quality-trial-sha256-" + Sha256Hex(scenario.Request.TrainingRunId)[..16];
        var exam = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BuildAssessmentRequest(scenario.Request.QualityGate!, trial.Model, passport, run);
        return (trial, exam);
    }

    private static async Task StageOnlyPreparedUpdate(PreparedCoordinatorScenario scenario)
    {
        var start = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(scenario.Request.StartRequest);
        Assert.True(start.IsSuccess, start.ErrorMessage);
        var request = scenario.Request;
        var reference = (OperationResult<LlmModel>)typeof(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator)
            .GetMethod("CloneModelBeforeMutation", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(null, [start.Value!.Model, request.LoraConfig, request.AdapterSeedAuthority])!;
        Assert.True(reference.IsSuccess, reference.ErrorMessage);
        var update = await new CodeDocumentationServerOwnedDpoTrainingExecutor().ExecuteFromPreparedBaselineAsync(
            new CodeDocumentationServerOwnedDpoTrainingRequest(scenario.Dpo.Pairs, start.Value!.Model, reference.Value!,
                request.Tokenizer, request.LoraConfig, scenario.Attempt, scenario.Broker, request.TrainingRunId, 1,
                OptimizerStateToRestore: request.OptimizerStateToRestore, AdapterSeedAuthority: request.AdapterSeedAuthority,
                SeededTopologyPreparedByRepositoryStart: start.Value.SeededTopologyPreparedByRepositoryStart,
                RandomStateToRestore: request.RandomStateToRestore,
                OptimizerStateColdStartAuthorization: request.OptimizerStateColdStartAuthorization), scenario.Inputs);
        Assert.True(update.IsSuccess, update.ErrorCode + ":" + update.ErrorMessage);
        Assert.False(File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json")));
        Assert.Equal(15, scenario.Client.IssuedCallCount); Assert.Equal(1, scenario.Broker.Calls);
    }

    [Fact]
    public async Task ExecuteFromPreparedBaselineAsync_ConcurrentCreateNewOwnersMintOneSingleUseDispatch()
    {
        using var scenario = await CreatePreparedCoordinatorScenario();
        var previous = StableDpoTrainingExecutor.LogicalRequestObserverForTesting; var updates = 0;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => updates++;
            await StageOnlyPreparedUpdate(scenario); var (_, exam) = CurrentTrialExam(scenario);
            using var barrier = new Barrier(2);
            var races = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Task.Run(async () =>
            {
                barrier.SignalAndWait();
                return await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch.ClaimAsync(
                    scenario.Attempt, scenario.Inputs, exam, Hex('a'), CancellationToken.None);
            })));
            var winner = Assert.Single(races, r => r.IsSuccess).Value!; Assert.Single(races, r => !r.IsSuccess);
            Assert.False((await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch.ClaimAsync(
                scenario.Attempt, scenario.Inputs, exam, Hex('a'), default)).IsSuccess);
            var startBytes = File.ReadAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json"));
            Assert.True((await winner.ConsumeDispatchAsync(scenario.Attempt, scenario.Inputs, exam, default)).IsSuccess);
            Assert.False((await winner.ConsumeDispatchAsync(scenario.Attempt, scenario.Inputs, exam, default)).IsSuccess);
            var measured = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(
                scenario.Request.QualityGate, scenario.Original.Baseline, exam.Model, exam.ModelPassport, exam.AssessmentRunId,
                scenario.Dpo.Receipt.OrderedPairs.Single().PairId);
            Assert.True(measured.IsSuccess, measured.ErrorMessage);
            var complete = await winner.CompleteAsync(scenario.Attempt, scenario.Inputs, exam, measured.Value!.Trial,
                scenario.Dpo.Receipt.OrderedPairs.Single().PairId, default);
            Assert.True(complete.IsSuccess, complete.ErrorMessage); Assert.Equal("Improved", complete.Value!.Detailed.Disposition.ToString());
            var files = SnapshotResumeFiles(scenario.Root);
            Assert.False((await winner.CompleteAsync(scenario.Attempt, scenario.Inputs, exam, measured.Value.Trial,
                scenario.Dpo.Receipt.OrderedPairs.Single().PairId, default)).IsSuccess);
            AssertRetainedResumeFiles(scenario.Root, files);
            Assert.Equal(startBytes, File.ReadAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json")));
            Assert.Equal(30, scenario.Client.IssuedCallCount); Assert.Equal(1, updates); Assert.Equal(1, scenario.Broker.Calls);
            Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
            _output.WriteLine("S04_DURABLE_CONCURRENCY actualCreators2 successfulCreateNew1 actualUpdates1 trialCalls15 completion1 secondDispatchAndCompletionRefused");
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = previous; }
    }

    [Theory]
    [InlineData("conditions", false)] [InlineData("conditions", true)]
    [InlineData("pair", false)] [InlineData("pair", true)]
    [InlineData("parent", false)] [InlineData("parent", true)]
    [InlineData("checkpoint", false)] [InlineData("checkpoint", true)]
    [InlineData("optimizer", false)] [InlineData("optimizer", true)]
    [InlineData("rng", false)] [InlineData("rng", true)]
    public async Task ExecuteFromPreparedBaselineAsync_CurrentOwnerBindingsRefuseBeforeAndAfterMint(string dimension, bool afterMint)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: true);
        await StageOnlyPreparedUpdate(scenario); var (_, exam) = CurrentTrialExam(scenario);
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch? cap = null;
        if (afterMint)
        {
            var claimed = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch.ClaimAsync(
                scenario.Attempt, scenario.Inputs, exam, Hex('a'), default);
            Assert.True(claimed.IsSuccess, claimed.ErrorMessage); cap = claimed.Value!;
        }
        var marker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt).Value!.Manifest;
        switch (dimension)
        {
            case "conditions": ((string[])scenario.Request.QualityGate!.EvidenceRefs)[0] = "changed:condition"; break;
            case "pair":
                var foreign = SealRepositoryOwnedDpo(scenario.Root, "genuine-foreign", "v2-foreign", "different prompt", "different chosen", "different rejected");
                Assert.True(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(foreign.Address, foreign.RequestIdentity).IsSuccess);
                scenario.Inputs = scenario.Inputs with { LessonDatasetAddress = foreign.Address, LessonExpectedRequestIdentity = foreign.RequestIdentity }; break;
            case "parent": File.WriteAllBytes(CreateModelCheckpointAddress(scenario.Root, 1).WeightsPath,
                File.ReadAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, DpoTrainer.BuildPolicyCheckpointFileName(1)))); break;
            case "checkpoint": File.WriteAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, DpoTrainer.BuildPolicyCheckpointFileName(1)),
                File.ReadAllBytes(CreateModelCheckpointAddress(scenario.Root, 1).WeightsPath)); break;
            case "optimizer": File.WriteAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, RepositoryOwnedDpoOptimizerStateFile.BuildFileName(marker.OptimizerStateStep)),
                File.ReadAllBytes(Path.Combine(scenario.Root, "seeded-parent", RepositoryOwnedDpoOptimizerStateFile.BuildFileName(1)))); break;
            case "rng": File.WriteAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(marker.RandomStateStep)),
                File.ReadAllBytes(Path.Combine(scenario.Root, "seeded-parent", RepositoryOwnedDpoRandomStateFile.BuildFileName(1)))); break;
        }
        var files = SnapshotResumeFiles(scenario.Root);
        if (afterMint) Assert.False((await cap!.ConsumeDispatchAsync(scenario.Attempt, scenario.Inputs, exam, default)).IsSuccess);
        else Assert.False((await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch.ClaimAsync(
            scenario.Attempt, scenario.Inputs, exam, Hex('a'), default)).IsSuccess);
        AssertRetainedResumeFiles(scenario.Root, files); Assert.Equal(15, scenario.Client.IssuedCallCount); Assert.Equal(1, scenario.Broker.Calls);
        Assert.Equal(afterMint, File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json")));
        Assert.False(File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")));
        _output.WriteLine($"S04_DURABLE_OWNER_MUTATION {dimension} afterMint={afterMint} ownerRefusal additionalEvaluation0 additionalUpdate0 filesExact");
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task ExecuteFromPreparedBaselineAsync_CancellationAtOwnerClaimNeverReclaimsDispatch(bool afterClaim)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(); await StageOnlyPreparedUpdate(scenario);
        var (_, exam) = CurrentTrialExam(scenario); using var cancel = new CancellationTokenSource();
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch? cap = null;
        if (afterClaim)
        {
            var claimed = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch.ClaimAsync(
                scenario.Attempt, scenario.Inputs, exam, Hex('a'), default);
            Assert.True(claimed.IsSuccess, claimed.ErrorMessage); cap = claimed.Value!;
        }
        var files = SnapshotResumeFiles(scenario.Root); cancel.Cancel();
        if (afterClaim) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cap!.ConsumeDispatchAsync(scenario.Attempt, scenario.Inputs, exam, cancel.Token));
        else await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch.ClaimAsync(
            scenario.Attempt, scenario.Inputs, exam, Hex('a'), cancel.Token));
        var recovered = scenario.ReconstructForEvaluation("cancelled-claim");
        var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteFromPreparedBaselineAsync(recovered.Request, recovered.Inputs);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Equal(0, recovered.Client.IssuedCallCount); Assert.Equal(0, recovered.Broker.Calls); Assert.Equal(15, scenario.Client.IssuedCallCount);
        AssertRetainedResumeFiles(scenario.Root, files);
    }

    [Fact]
    public async Task ExecuteFromPreparedBaselineAsync_DifferentValidCompletedReceiptFailsGuardBeforePublish()
    {
        using var scenario = await CreatePreparedCoordinatorScenario(); scenario.BlockPublicationAfterCompleteTrial();
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, (await scenario.Execute()).ErrorCode);
        scenario.RemovePublicationBlocker(); var consumed = (await ReadCurrentPreparedTrial(scenario)).Value!;
        var (trial, exam) = CurrentTrialExam(scenario);
        var separateControlClient = new ScriptedQualityGateEvaluationClient(scenario.Request.QualityGate!.EvaluatorConfiguration,
            Enumerable.Repeat(true, 15).ToArray(), i => "genuine-replacement-series-" + i);
        // A separately measured control graph is test mutation material only. The
        // production recovery/guard below issues no evaluation or completion.
        var context = scenario.Request.QualityGate with { EvaluationClient = separateControlClient };
        var separate = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(context,
            scenario.Original.Baseline, trial.Model, exam.ModelPassport, exam.AssessmentRunId, scenario.Dpo.Receipt.OrderedPairs.Single().PairId);
        Assert.True(separate.IsSuccess, separate.ErrorMessage); Assert.Equal(15, separateControlClient.IssuedCallCount);
        var ownerType = typeof(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal);
        var replacement = (byte[])ownerType.GetMethod("EncodeCompleted", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(null, [File.ReadAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json")), scenario.Inputs.Reference.Sha256,
                separate.Value!, CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(separate.Value!.Trial)])!;
        File.WriteAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json"), replacement);
        var current = await ReadCurrentPreparedTrial(scenario); Assert.True(current.IsSuccess, current.ErrorMessage);
        Assert.NotEqual(consumed.CompletedSha256, current.Value!.CompletedSha256);
        Assert.Equal("Improved", current.Value.Detailed.Disposition.ToString());
        var coordinator = typeof(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator);
        var captureType = coordinator.GetNestedType("PreparedInvocationCapture", System.Reflection.BindingFlags.NonPublic)!;
        var capture = Activator.CreateInstance(captureType, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public,
            null, [scenario.Request, scenario.Inputs], null)!;
        var dpo = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(scenario.Dpo.Address, scenario.Dpo.RequestIdentity).Value!;
        var files = SnapshotResumeFiles(scenario.Root);
        var guard = (Task<OperationResult<bool>>)coordinator.GetMethod("ValidatePreparedBeforePublicationAsync",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.Invoke(null,
            [scenario.Request, scenario.Inputs, capture, scenario.Attempt, scenario.Parent, dpo.Manifest, dpo.Manifest.OrderedPairs.Single(),
                scenario.Original.Baseline, trial, CancellationToken.None, true, consumed.CompletedSha256])!;
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, (await guard).ErrorCode);
        AssertRetainedResumeFiles(scenario.Root, files); Assert.Equal(30, scenario.Client.IssuedCallCount); Assert.Equal(1, scenario.Broker.Calls);
        Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
        _output.WriteLine("S04_EXACT_RECEIPT_MUTATION validImprovedA consumed then genuineMeasuredValidImprovedB admitted; oldReceiptGuardRefused; guardEvaluation0 publish0 filesExact separateControlCalls15");
    }

    private void AssertMaximumStorageProjection(CodeDocumentationSavedModelAssessmentCalibrationResult genuine, int cases, int repeats)
    {
        // Explicitly storage-only: this conservative projection is never supplied
        // to producer/dispatch/comparator admission as measured evidence.
        var node = JsonNode.Parse(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(genuine))!;
        var caseIds = Enumerable.Range(0, cases).Select(i => new string('\uffff', 250) + i.ToString("D6")).ToArray();
        var refs = Enumerable.Range(0, 128).Select(i => new string('\uffff', 506) + i.ToString("D6")).ToArray();
        var root = new string('\uffff', 243) + "\"\\"; Assert.Equal(245, root.Length);
        var repeatMethod = typeof(CodeDocumentationSavedModelAssessmentCalibrationService).GetMethod("BuildRepeatAssessmentRunId",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        var report = node["RepresentativeAssessment"]!; var calibration = node["Calibration"]!;
        calibration["CalibrationRunId"] = root;
        for (var repeat = 0; repeat < repeats; repeat++)
        {
            var observation = calibration["Observations"]![repeat]!;
            var repeatId = (string)repeatMethod.Invoke(null, [root, repeat + 1])!;
            Assert.InRange(repeatId.Length, 1, 256); observation["AssessmentRunId"] = repeatId;
            observation["CaseIds"] = JsonSerializer.SerializeToNode(caseIds);
            observation["ObservedEvaluationRequestIds"] = JsonSerializer.SerializeToNode(Enumerable.Range(0, cases)
                .Select(i => new string('\uffff', 250) + (repeat * cases + i).ToString("D6")));
        }
        report["AssessmentRunId"] = calibration["Observations"]![0]!["AssessmentRunId"]!.DeepClone();
        report["Evaluator"]!["ObservedEvaluationRequestIds"] = calibration["Observations"]![0]!["ObservedEvaluationRequestIds"]!.DeepClone();
        report["Evaluator"]!["ExpectedModelId"] = new string('\uffff', 256);
        report["Evaluator"]!["EvaluationProfileId"] = new string('\uffff', 256);
        report["RepositoryRelativeModelIdentityPath"] = new string('\uffff', 4096);
        foreach (var item in report["Cases"]!.AsArray().Select((value, i) => (value, i)))
        {
            item.value!["CaseId"] = caseIds[item.i]; item.value["StudentAnswerPreview"] = new string('\uffff', 1024);
            item.value["StudentAnswerCharCount"] = 65536; item.value["Reason"] = new string('\uffff', 64);
        }
        report["EvidenceRefs"] = JsonSerializer.SerializeToNode(refs);
        calibration["EvidenceRefs"] = JsonSerializer.SerializeToNode(refs.Concat(genuine.Calibration.EvidenceRefs.TakeLast(5)));
        var projection = JsonSerializer.Deserialize<CodeDocumentationSavedModelAssessmentCalibrationResult>(node.ToJsonString())!;
        var trialBytes = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(projection);
        var wire = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(new
        { Schema = "xiigen.code-documentation.prepared-trial.completed.v1", FormatVersion = 1, StartedSha256 = Hex('a'),
            Disposition = "Improved", TrialSha256 = ComputeBytesSha256(trialBytes), DerivedEvidenceSha256 = Hex('b'), Trial = projection });
        var upper = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes(caseIds, repeats);
        Assert.NotNull(upper); Assert.True(wire.LongLength <= upper); Assert.True(upper <= 32 * 1024 * 1024);
        var decoded = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.DecodeTrial<CodeDocumentationSavedModelAssessmentCalibrationResult>(trialBytes);
        Assert.Equal(trialBytes, CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(decoded));
        Assert.Equal(repeats, decoded.Calibration.Observations.Count);
        Assert.Equal(5000, CoordinatorRequestIds(decoded.Calibration).Distinct(StringComparer.Ordinal).Count());
        Assert.All(decoded.RepresentativeAssessment.Cases, c => Assert.Equal(1024, c.StudentAnswerPreview.Length));
        _output.WriteLine("S04_DURABLE_STORAGE_PROJECTION " + JsonSerializer.Serialize(new { cases, repeats, actualUtf8Bytes = wire.LongLength,
            upperBoundBytes = upper, storageOnly = true, escapedRootChars = root.Length, requestIdChars = 256, previewChars = 1024,
            answerChars = 65536, refs = 128, refChars = 512, typedRoundtripExact = true }));
    }

    private static string ComputeBytesSha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    [Fact]
    public void PreparedTrialWirePreflight_ActualSerializerEnvelopesAndUnsupportedShapes()
    {
        var envelope = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(
            new CodeDocumentationSavedModelAssessmentCalibrationObservation(new string('\uffff', 256), int.MaxValue, int.MaxValue,
                double.MinValue, Hex('a'), Hex('b'), Hex('c'), [], [], []));
        Assert.InRange(envelope.Length, 1, 2304);
        foreach (var value in new[] { double.MinValue, double.MaxValue, double.Epsilon, -double.Epsilon, -0d, 1d })
            Assert.InRange(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(value).Length, 1, 26);
        foreach (var counts in new[] { (200, 25), (1, 5000), (2, 2500) })
            Assert.InRange(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes(
                Enumerable.Repeat(new string('\uffff', 256), counts.Item1).ToArray(), counts.Item2)!.Value, 1, 32 * 1024 * 1024);
        Assert.Null(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes([], 2));
        Assert.Null(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes(["c"], 1));
        Assert.Null(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes(["c"], int.MaxValue));
        Assert.Null(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes(["c"], 5001));
        Assert.Null(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes([new string('x', 257)], 2));
        Assert.Null(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CompleteTrialUpperBoundBytes(Enumerable.Repeat("c", 201).ToArray(), 2));
        _output.WriteLine("S04_DURABLE_SERIALIZER_ENVELOPE actualNetUtf8Bytes=" + envelope.Length + " bound2304 finiteDoubleBound26 unsupportedShapesRefused");
    }

    [Fact]
    public void PreparedTrialWeightReader_GrowthTruncationAndCancellationStayWithinExactLongBound()
    {
        var reader = typeof(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal).GetMethod("ReadExactWeightHash",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        var bytes = Enumerable.Range(0, 1024).Select(i => (byte)i).ToArray();
        using var exact = new CountingWeightStream(bytes);
        var hash = (byte[])reader.Invoke(null, [exact, 1024L, CancellationToken.None])!;
        Assert.Equal(SHA256.HashData(bytes), hash); Assert.Equal(1024, exact.ReadBytes);
        using var growing = new CountingWeightStream(bytes.Concat(new byte[100000]).ToArray());
        Assert.Null(reader.Invoke(null, [growing, 1024L, CancellationToken.None]));
        Assert.Equal(1025, growing.ReadBytes);
        using var truncated = new CountingWeightStream(bytes[..1023]);
        Assert.Null(reader.Invoke(null, [truncated, 1024L, CancellationToken.None])); Assert.Equal(1023, truncated.ReadBytes);
        using var longBound = new CountingWeightStream(bytes);
        Assert.Null(reader.Invoke(null, [longBound, 16L * 1024 * 1024 * 1024, CancellationToken.None]));
        Assert.Equal(1024, longBound.ReadBytes);
        using var cancelled = new CountingWeightStream(bytes); using var token = new CancellationTokenSource(); token.Cancel();
        var error = Assert.Throws<System.Reflection.TargetInvocationException>(() => reader.Invoke(null, [cancelled, 1024L, token.Token]));
        Assert.IsAssignableFrom<OperationCanceledException>(error.InnerException); Assert.Equal(0, cancelled.ReadBytes);
        _output.WriteLine("S04_WEIGHT_READER exact1024/hashExact; growingRead1025maxIncludingEOFLookahead; truncated1023Rejected; long16GiBBoundNoIntOverflowNoLargeAllocation; cancelledReads0");
    }

    private sealed class CountingWeightStream(byte[] bytes) : MemoryStream(bytes, writable: false)
    {
        internal long ReadBytes;
        public override int Read(byte[] buffer, int offset, int count)
        { var read = base.Read(buffer, offset, count); ReadBytes += read; return read; }
        public override int ReadByte()
        { var value = base.ReadByte(); if (value >= 0) ReadBytes++; return value; }
    }

    [Fact]
    public void PreparedTrialWeightReader_MultiChunkHashAndMidReadCancellationUseOnly64KiBRequests()
    {
        var reader = typeof(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal).GetMethod("ReadExactWeightHash",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        var bytes = Enumerable.Range(0, 3 * 65536 + 17).Select(i => (byte)i).ToArray();
        using var exact = new ChunkRecordingWeightStream(bytes);
        var hash = (byte[])reader.Invoke(null, [exact, (long)bytes.Length, CancellationToken.None])!;
        Assert.Equal(SHA256.HashData(bytes), hash);
        Assert.Equal(new[] { 65536, 65536, 65536, 17 }, exact.RequestedCounts);
        Assert.Equal(bytes.Length, exact.ReadBytes);
        Assert.Equal(1, exact.EndLookaheads);
        using var cancellation = new CancellationTokenSource();
        using var interrupted = new ChunkRecordingWeightStream(bytes, cancellation);
        var error = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            reader.Invoke(null, [interrupted, 16L * 1024 * 1024 * 1024, cancellation.Token]));
        Assert.IsAssignableFrom<OperationCanceledException>(error.InnerException);
        Assert.Equal(new[] { 65536 }, interrupted.RequestedCounts);
        Assert.Equal(65536, interrupted.ReadBytes);
        Assert.Equal(0, interrupted.EndLookaheads);
        _output.WriteLine("S04_WEIGHT_READER_MULTICHUNK actualHashExact; requested65536/65536/65536/17; oneEOFLookahead; midReadCancellation16GiBBoundOnly65536ReadNoSecondRead");
    }

    private sealed class ChunkRecordingWeightStream(byte[] bytes, CancellationTokenSource? cancelAfterFirstRead = null)
        : MemoryStream(bytes, writable: false)
    {
        internal readonly List<int> RequestedCounts = [];
        internal long ReadBytes;
        internal int EndLookaheads;
        public override int Read(byte[] buffer, int offset, int count)
        {
            RequestedCounts.Add(count);
            var read = base.Read(buffer, offset, count);
            ReadBytes += read;
            if (RequestedCounts.Count == 1) cancelAfterFirstRead?.Cancel();
            return read;
        }
        public override int ReadByte() { EndLookaheads++; return base.ReadByte(); }
    }

    [Fact]
    public async Task ExecuteFromPreparedBaselineAsync_RejectedDispatchCannotBeReusedAfterInputsAreRestored()
    {
        using var scenario = await CreatePreparedCoordinatorScenario();
        await StageOnlyPreparedUpdate(scenario);
        var (_, exam) = CurrentTrialExam(scenario);
        var claimed = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch.ClaimAsync(
            scenario.Attempt, scenario.Inputs, exam, Hex('a'), default);
        Assert.True(claimed.IsSuccess, claimed.ErrorMessage);
        var refs = (string[])scenario.Request.QualityGate!.EvidenceRefs;
        var originalRef = refs[0];
        refs[0] = "test:changed-after-claim";
        Assert.False((await claimed.Value!.ConsumeDispatchAsync(scenario.Attempt, scenario.Inputs, exam, default)).IsSuccess);
        refs[0] = originalRef;
        var files = SnapshotResumeFiles(scenario.Root);
        Assert.False((await claimed.Value.ConsumeDispatchAsync(scenario.Attempt, scenario.Inputs, exam, default)).IsSuccess);
        Assert.False((await claimed.Value.CompleteAsync(scenario.Attempt, scenario.Inputs, exam,
            scenario.Original.Baseline.Calibration, scenario.Dpo.Receipt.OrderedPairs.Single().PairId, default)).IsSuccess);
        var fresh = scenario.ReconstructForEvaluation("rejected-dispatch");
        var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteFromPreparedBaselineAsync(fresh.Request, fresh.Inputs);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Equal(0, fresh.Client.IssuedCallCount);
        Assert.Equal(0, fresh.Broker.Calls);
        Assert.Equal(15, scenario.Client.IssuedCallCount);
        Assert.Equal(1, scenario.Broker.Calls);
        AssertRetainedResumeFiles(scenario.Root, files);
        Assert.False(File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")));
        _output.WriteLine("S04_REJECTED_DISPATCH inputsRestored; secondConsumeRefused; completionWithoutAuthorizedDispatchRefused; freshOwnerRecoveryRefused; additionalCalls0 updates0 filesExact");
    }

    [Fact]
    public async Task ExecuteFromPreparedBaselineAsync_CancellationAfterCompleteRetainsExactHistoryWithoutEffects()
    {
        using var scenario = await CreatePreparedCoordinatorScenario(); scenario.BlockPublicationAfterCompleteTrial();
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, (await scenario.Execute()).ErrorCode);
        scenario.RemovePublicationBlocker(); Assert.True((await ReadCurrentPreparedTrial(scenario)).IsSuccess);
        var files = SnapshotResumeFiles(scenario.Root); var timestamps = SnapshotWriteTimes(scenario.Root);
        using var token = new CancellationTokenSource(); token.Cancel(); var fresh = scenario.ReconstructForEvaluation("after-complete-cancel");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
            .ExecuteFromPreparedBaselineAsync(fresh.Request, fresh.Inputs, token.Token));
        AssertRetainedResumeFiles(scenario.Root, files); Assert.Equal(timestamps, SnapshotWriteTimes(scenario.Root));
        Assert.Equal(0, fresh.Client.IssuedCallCount); Assert.Equal(0, fresh.Broker.Calls); Assert.Equal(30, scenario.Client.IssuedCallCount);
        Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
    }

    private static string[] SnapshotWriteTimes(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        .Order(StringComparer.Ordinal).Select(p => p + ":" + new FileInfo(p).Length + ":" + File.GetLastWriteTimeUtc(p).Ticks).ToArray();

    [Theory]
    [InlineData("Improved", 0, true, false)]
    [InlineData("Regressed", 4, false, false)]
    [InlineData("Indistinguishable", 0, false, false)]
    [InlineData("IdenticalWeights", 0, true, true)]
    public async Task ExecutePreparedLessonWithOutcomeAsync_RealStepRetainsDetachedCompleteOutcome(
        string disposition, int baselinePassCount, bool trialPass, bool zeroWeightParent)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(baselinePassCount: baselinePassCount,
            trialPass: trialPass, zeroWeightParent: zeroWeightParent);
        var parentFiles = SnapshotResumeFiles(scenario.Root);
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        var updates = 0;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => updates++;
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecutePreparedLessonWithOutcomeAsync(scenario.Request, scenario.Inputs);
            Assert.True(result.IsSuccess, result.ErrorCode + ":" + result.ErrorMessage);
            var outcome = result.Value!;
            var owner = await ReadCurrentPreparedTrial(scenario);
            CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult expectedDetailed;
            if (disposition == "Improved")
            {
                // Publication changed the current accepted parent. The old Original
                // cannot regain current-owner authority through a postpublication open.
                Assert.False(owner.IsSuccess);
                var (_, exam) = CurrentTrialExam(scenario);
                using var completedWire = JsonDocument.Parse(File.ReadAllBytes(
                    Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")));
                var persistedTrial = completedWire.RootElement.GetProperty("Trial")
                    .Deserialize<CodeDocumentationSavedModelAssessmentCalibrationResult>()!;
                var compared = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.CompareCompletedTrialDetailed(
                    exam, scenario.Request.QualityGate!.NoisePolicyConfig, scenario.Original.Baseline,
                    persistedTrial, scenario.Dpo.Receipt.OrderedPairs.Single().PairId);
                Assert.True(compared.IsSuccess, compared.ErrorMessage);
                expectedDetailed = compared.Value!;
            }
            else
            {
                Assert.True(owner.IsSuccess, owner.ErrorMessage);
                expectedDetailed = owner.Value!.Detailed;
            }
            Assert.Equal(disposition, outcome.Disposition.ToString());
            Assert.Equal(ComputeFileSha256(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")), outcome.CompletedSha256);
            Assert.Equal(ComputeFileSha256(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json")), outcome.StartedSha256);
            Assert.Equal(scenario.Original.Sha256, outcome.OriginalSha256);
            Assert.Equal(scenario.Request.TrainingRunId, outcome.TrainingRunId);
            Assert.Equal(scenario.Attempt.AttemptRootRelativePath, outcome.AttemptRootRelativePath);
            Assert.Equal(scenario.Dpo.Receipt.OrderedPairs.Single().PairId, outcome.TrainingPairId);
            Assert.Equal(scenario.Dpo.Receipt.ManifestSha256, outcome.DpoManifestSha256);
            var expectedGraph = JsonSerializer.Serialize(expectedDetailed);
            Assert.Equal(expectedGraph, JsonSerializer.Serialize(outcome.Detailed));
            var canonicalCompleted = (byte[])typeof(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal)
                .GetMethod("EncodeCompleted", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(null, [File.ReadAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json")), scenario.Original.Sha256,
                    expectedDetailed, CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(expectedDetailed.Trial)])!;
            Assert.Equal(File.ReadAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")), canonicalCompleted);
            Assert.Equal(JsonSerializer.Serialize(scenario.Original.Baseline), JsonSerializer.Serialize(outcome.Detailed.Baseline));
            Assert.Equal(15, CoordinatorRequestIds(outcome.Detailed.Baseline.Calibration.Calibration).Length);
            Assert.Equal(15, CoordinatorRequestIds(outcome.Detailed.Trial.Calibration).Length);
            Assert.Empty(CoordinatorRequestIds(outcome.Detailed.Baseline.Calibration.Calibration)
                .Intersect(CoordinatorRequestIds(outcome.Detailed.Trial.Calibration), StringComparer.Ordinal));
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(outcome.Detailed.Baseline.Calibration.Calibration.MeasurementNoisePassport, out _));
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(outcome.Detailed.Trial.Calibration.MeasurementNoisePassport, out _));
            Assert.Equal(1, updates); Assert.Equal(1, scenario.Broker.Calls); Assert.Equal(30, scenario.Client.IssuedCallCount);
            var marker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt);
            Assert.True(marker.IsSuccess, marker.ErrorMessage);
            var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(scenario.Attempt.AttemptRoot,
                marker.Value!.Manifest.OptimizerStateStep, marker.Value.Manifest.OptimizerStateSha256);
            Assert.True(optimizer.IsSuccess, optimizer.ErrorMessage);
            Assert.Equal(zeroWeightParent ? 2 : 1, optimizer.Value!.State.Step);
            Assert.Equal(ComputeFileSha256(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")), outcome.CompletedSha256);
            if (zeroWeightParent) Assert.Equal(scenario.Parent.WeightsSha256, marker.Value.Manifest.TrialWeightsSha256);
            else Assert.NotEqual(scenario.Parent.WeightsSha256, marker.Value.Manifest.TrialWeightsSha256);
            AssertRetainedResumeFiles(scenario.Root, parentFiles, allowAdditionalFiles: true);

            // Mutate actual returned nested collections, then ask the envelope
            // for a new graph. None of these objects is its private snapshot.
            var exposed = outcome.Detailed;
            Assert.IsAssignableFrom<IList<string>>(exposed.Trial.Calibration.Observations[0].ObservedEvaluationRequestIds)[0] = "caller-mutated-trial";
            Assert.IsAssignableFrom<IList<string>>(exposed.Baseline.Calibration.Calibration.Observations[0].CaseIds)[0] = "caller-mutated-original";
            Assert.IsAssignableFrom<IList<string>>(exposed.Evidence.EvidenceRefs)[0] = "caller-mutated-provenance";
            Assert.Equal(expectedGraph, JsonSerializer.Serialize(outcome.Detailed));
            Assert.All(typeof(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.CompletedPreparedTrialOutcome)
                .GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic), c => Assert.True(c.IsPrivate));

            if (disposition == "Improved")
            {
                Assert.Equal(2, outcome.PublishedCheckpointStep);
                Assert.True(RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(CreateModelCheckpointAddress(scenario.Root, 2)).IsSuccess);
                var fresh = scenario.ReconstructForEvaluation("already-applied");
                var repeated = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(fresh.Request, fresh.Inputs);
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PairAlreadyAppliedErrorCode, repeated.ErrorCode);
                Assert.Equal(0, fresh.Client.IssuedCallCount); Assert.Equal(0, fresh.Broker.Calls);
            }
            else
            {
                Assert.Null(outcome.PublishedCheckpointStep);
                Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
                var files = SnapshotResumeFiles(scenario.Root); var times = SnapshotWriteTimes(scenario.Root);
                var fresh = scenario.ReconstructForEvaluation("negative-must-not-evaluate");
                var repeated = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(fresh.Request, fresh.Inputs);
                Assert.True(repeated.IsSuccess, repeated.ErrorCode + ":" + repeated.ErrorMessage);
                Assert.NotSame(outcome, repeated.Value);
                Assert.Equal(outcome.CompletedSha256, repeated.Value!.CompletedSha256);
                Assert.Equal(expectedGraph, JsonSerializer.Serialize(repeated.Value.Detailed));
                var legacy = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteFromPreparedBaselineAsync(fresh.Request, fresh.Inputs);
                var mapped = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.MapCompletedTrial(expectedDetailed);
                Assert.False(legacy.IsSuccess); Assert.Equal(mapped.ErrorCode, legacy.ErrorCode); Assert.Equal(mapped.ErrorMessage, legacy.ErrorMessage);
                Assert.Equal(0, fresh.Client.IssuedCallCount); Assert.Equal(0, fresh.Broker.Calls);
                AssertRetainedResumeFiles(scenario.Root, files); Assert.Equal(times, SnapshotWriteTimes(scenario.Root));
            }
            Assert.Equal(1, updates); Assert.Equal(30, scenario.Client.IssuedCallCount);
            _output.WriteLine("S04_TYPED_COMPLETED_OUTCOME " + JsonSerializer.Serialize(new
            { disposition, updates, optimizerStep = optimizer.Value.State.Step, baselineCalls = 15, trialCalls = 15,
                freshCalls = 0, freshLease = 0, newAuthoring = 0, newPair = 0, outcome.CompletedSha256,
                outcome.StartedSha256, outcome.OriginalSha256, outcome.PublishedCheckpointStep,
                nestedMutationIsolated = true, runtime = CoordinatorRuntimeIdentity() }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData("missing-complete")]
    [InlineData("partial-complete")]
    [InlineData("altered-hash")]
    [InlineData("invalid-disposition")]
    [InlineData("partial-graph")]
    [InlineData("foreign-provenance")]
    [InlineData("missing-optimizer")]
    [InlineData("foreign-original")]
    [InlineData("foreign-pair")]
    [InlineData("foreign-manifest")]
    [InlineData("source-mutation")]
    [InlineData("cancelled")]
    public async Task ExecutePreparedLessonWithOutcomeAsync_InvalidCurrentOwnerNeverBecomesMeasuredSuccess(string mutation)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(trialPass: false);
        var control = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(scenario.Request, scenario.Inputs);
        Assert.True(control.IsSuccess, control.ErrorMessage);
        Assert.Equal("Indistinguishable", control.Value!.Disposition.ToString());
        var fresh = scenario.ReconstructForEvaluation("forbidden-negative-effects");
        var request = fresh.Request; var inputs = fresh.Inputs;
        var completePath = Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json");
        var bytes = File.ReadAllBytes(completePath);
        using var foreign = mutation.StartsWith("foreign-", StringComparison.Ordinal) && mutation != "foreign-provenance"
            ? await CreatePreparedCoordinatorScenario(trialPass: false, lessonScope: "foreign-current") : null;
        switch (mutation)
        {
            case "missing-complete": File.Delete(completePath); break;
            case "partial-complete": File.WriteAllBytes(completePath, bytes[..^1]); break;
            case "altered-hash": File.AppendAllText(completePath, " "); break;
            case "missing-optimizer":
                var marker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt).Value!;
                File.Delete(Path.Combine(scenario.Root, marker.Manifest.OptimizerStateRelativePath)); break;
            case "foreign-original": inputs = inputs with { Reference = foreign!.Inputs.Reference }; break;
            case "foreign-pair": request = request with { RoutingRun = foreign!.Request.RoutingRun }; break;
            case "foreign-manifest": request = request with { DpoDatasetAddress = foreign!.Dpo.Address }; break;
            case "source-mutation":
                inputs = inputs with { LessonExpectedRequestIdentity = inputs.LessonExpectedRequestIdentity with { RequestIdentitySha256 = Hex('e') } }; break;
            case "cancelled": break;
            default:
                var node = JsonNode.Parse(bytes)!.AsObject();
                if (mutation == "invalid-disposition") node["Disposition"] = "InvalidOrIncomplete";
                if (mutation == "partial-graph") node["Trial"]!["Calibration"]!["Observations"]!.AsArray().RemoveAt(0);
                if (mutation == "foreign-provenance") node["Trial"]!["Calibration"]!["Observations"]![0]!["ObservedEvaluationRequestIds"]![0] = "foreign-response";
                File.WriteAllBytes(completePath, JsonSerializer.SerializeToUtf8Bytes(node)); break;
        }
        var files = SnapshotResumeFiles(scenario.Root); var times = SnapshotWriteTimes(scenario.Root);
        using var token = new CancellationTokenSource();
        if (mutation == "cancelled")
        {
            token.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecutePreparedLessonWithOutcomeAsync(request, inputs, token.Token));
        }
        else
        {
            var refused = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(request, inputs);
            Assert.False(refused.IsSuccess); Assert.Null(refused.Value);
            var legacy = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteFromPreparedBaselineAsync(request, inputs);
            Assert.Equal(legacy.ErrorCode, refused.ErrorCode); Assert.Equal(legacy.ErrorMessage, refused.ErrorMessage);
            Assert.NotEqual(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode, refused.ErrorCode);
            _output.WriteLine("S04_TYPED_OUTCOME_REFUSAL " + JsonSerializer.Serialize(new { mutation, guard = refused.ErrorCode, validControl = control.Value.CompletedSha256 }));
        }
        Assert.Equal(0, fresh.Client.IssuedCallCount); Assert.Equal(0, fresh.Broker.Calls);
        Assert.Equal(30, scenario.Client.IssuedCallCount); Assert.Equal(1, scenario.Broker.Calls);
        AssertRetainedResumeFiles(scenario.Root, files); Assert.Equal(times, SnapshotWriteTimes(scenario.Root));
        Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
    }

    [Fact]
    public async Task ExecutePreparedLessonWithOutcomeAsync_FailedPublicationCannotMintImprovedOutcome()
    {
        using var scenario = await CreatePreparedCoordinatorScenario();
        scenario.BlockPublicationAfterCompleteTrial();
        var refused = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(scenario.Request, scenario.Inputs);
        Assert.False(refused.IsSuccess); Assert.Null(refused.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, refused.ErrorCode);
        var completed = await ReadCurrentPreparedTrial(scenario);
        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        Assert.Equal("Improved", completed.Value!.Detailed.Disposition.ToString());
        Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
        scenario.RemovePublicationBlocker();
        var fresh = scenario.ReconstructForEvaluation("improved-recovery-no-new-effects");
        var accepted = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(fresh.Request, fresh.Inputs);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Equal(completed.Value.CompletedSha256, accepted.Value!.CompletedSha256);
        Assert.Equal(2, accepted.Value.PublishedCheckpointStep);
        Assert.Equal(0, fresh.Broker.Calls); Assert.Equal(0, fresh.Client.IssuedCallCount);
        Assert.Equal(1, scenario.Broker.Calls); Assert.Equal(30, scenario.Client.IssuedCallCount);
    }

    [Fact]
    public async Task CompletedNegativeOutcomeGuard_RejectsDifferentGenuineCompletedReceiptWithoutEffects()
    {
        using var scenario = await CreatePreparedCoordinatorScenario(trialPass: false);
        var accepted = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(scenario.Request, scenario.Inputs);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        var consumed = (await ReadCurrentPreparedTrial(scenario)).Value!;
        var (trial, exam) = CurrentTrialExam(scenario);
        var separateClient = new ScriptedQualityGateEvaluationClient(scenario.Request.QualityGate!.EvaluatorConfiguration,
            Enumerable.Repeat(false, 15).ToArray(), i => "genuine-negative-replacement-" + i);
        var separate = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(
            scenario.Request.QualityGate with { EvaluationClient = separateClient }, scenario.Original.Baseline,
            trial.Model, exam.ModelPassport, exam.AssessmentRunId, scenario.Dpo.Receipt.OrderedPairs.Single().PairId);
        Assert.True(separate.IsSuccess, separate.ErrorMessage); Assert.Equal(15, separateClient.IssuedCallCount);
        var replacement = (byte[])typeof(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal)
            .GetMethod("EncodeCompleted", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(null, [File.ReadAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "trial-started.json")), scenario.Inputs.Reference.Sha256,
                separate.Value!, CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.TrialBytes(separate.Value!.Trial)])!;
        File.WriteAllBytes(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json"), replacement);
        var current = await ReadCurrentPreparedTrial(scenario);
        Assert.True(current.IsSuccess, current.ErrorMessage);
        Assert.Equal(consumed.Detailed.Disposition, current.Value!.Detailed.Disposition);
        Assert.NotEqual(consumed.CompletedSha256, current.Value.CompletedSha256);
        var coordinator = typeof(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator);
        var captureType = coordinator.GetNestedType("PreparedInvocationCapture", System.Reflection.BindingFlags.NonPublic)!;
        var capture = Activator.CreateInstance(captureType, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public,
            null, [scenario.Request, scenario.Inputs], null)!;
        var dpo = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(scenario.Dpo.Address, scenario.Dpo.RequestIdentity).Value!;
        var files = SnapshotResumeFiles(scenario.Root); var times = SnapshotWriteTimes(scenario.Root);
        var guard = (Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>>)coordinator
            .GetMethod("ReopenCompletedNegativeAsync", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(null, [scenario.Request, scenario.Inputs, capture, scenario.Attempt, scenario.Parent, dpo.Manifest,
                dpo.Manifest.OrderedPairs.Single(), scenario.Original.Baseline, trial, exam.ModelPassport, consumed, CancellationToken.None])!;
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, (await guard).ErrorCode);
        AssertRetainedResumeFiles(scenario.Root, files); Assert.Equal(times, SnapshotWriteTimes(scenario.Root));
        Assert.Equal(30, scenario.Client.IssuedCallCount); Assert.Equal(1, scenario.Broker.Calls);
        Assert.Equal(consumed.CompletedSha256, accepted.Value!.CompletedSha256);
        Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 2).CheckpointRoot));
        _output.WriteLine("S04_TYPED_EXACT_NEGATIVE_SHA validOwnerA then genuineMeasuredOwnerB; oldSHA rejected; guardEffects0; separateControlCalls15");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecutePreparedLessonWithOutcomeAsync_GenuineWarmCompletedNegativeReopensWithoutEffects(bool dropout)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: dropout, genuineWarm: true, trialPass: false);
        var sourceAttempt = scenario.Request.OptimizerStateSourceAttempt!;
        var sourceMarker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(sourceAttempt);
        Assert.True(sourceMarker.IsSuccess, sourceMarker.ErrorMessage);
        var source = sourceMarker.Value!.Manifest;
        var sourceOptimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(sourceAttempt.AttemptRoot,
            source.OptimizerStateStep, source.OptimizerStateSha256);
        Assert.True(sourceOptimizer.IsSuccess, sourceOptimizer.ErrorMessage);
        Assert.True(sourceOptimizer.Value!.State.Step > 0);
        Assert.Contains(sourceOptimizer.Value.State.Entries, e => e.FirstMoment.Any(v => v != 0) && e.SecondMoment.Any(v => v != 0));
        var sourceRandom = dropout ? RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
            Path.Combine(scenario.Root, source.RandomStateRelativePath), source.RandomStateSha256,
            source.RandomStateByteLength, source.RandomStateStep, source.RandomStateTopologySha256, source.LoraConfigurationSha256).Value : null;
        if (dropout) Assert.NotNull(sourceRandom);
        var before = SnapshotResumeFiles(scenario.Root);
        var stable = Directory.GetFiles(scenario.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        var requests = 0; var mutationSeams = 0;
        var previousRequest = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        var previousMutation = LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = logical =>
            {
                requests++;
                Assert.NotSame(logical.PolicyModel, logical.ReferenceModel);
                Assert.Equal(scenario.Parent.WeightsSha256, ComputeCheckpointSha256(logical.PolicyModel));
                Assert.Equal(scenario.Parent.WeightsSha256, ComputeCheckpointSha256(logical.ReferenceModel));
                Assert.True(AnyLoraBMatrixContainsNonZeroValues(logical.PolicyModel));
                Assert.True(AnyLoraBMatrixContainsNonZeroValues(logical.ReferenceModel));
                Assert.Equal(sourceOptimizer.Value.State.ToBytes(), logical.OptimizerStateToRestore!.ToBytes());
                Assert.Equal(JsonSerializer.Serialize(sourceRandom), JsonSerializer.Serialize(logical.RandomStateToRestore));
            };
            // This seam is immediately before StepVector; the persisted actual
            // AdamW cumulative counter below is distinct from this observation.
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = (_, _) => mutationSeams++;
            var first = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecutePreparedLessonWithOutcomeAsync(scenario.Request, scenario.Inputs);
            Assert.True(first.IsSuccess, first.ErrorCode + ":" + first.ErrorMessage);
            Assert.Equal("Indistinguishable", first.Value!.Disposition.ToString());
            Assert.Null(first.Value.PublishedCheckpointStep);
            Assert.Equal(1, requests); Assert.Equal(1, mutationSeams); Assert.Equal(1, scenario.Broker.Calls);
            var trialMarker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt);
            Assert.True(trialMarker.IsSuccess, trialMarker.ErrorMessage);
            var trial = trialMarker.Value!.Manifest;
            var trialOptimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(scenario.Attempt.AttemptRoot,
                trial.OptimizerStateStep, trial.OptimizerStateSha256);
            Assert.True(trialOptimizer.IsSuccess, trialOptimizer.ErrorMessage);
            Assert.Equal(sourceOptimizer.Value.State.Step + 1, trialOptimizer.Value!.State.Step);
            Assert.Equal(1, trial.DpoStepCount);
            Assert.NotEqual(source.OptimizerStateSha256, trial.OptimizerStateSha256);
            LlmModelRandomStateSnapshot? trialRandom = null;
            if (dropout)
            {
                Assert.Equal(source.RandomStateStep + 1, trial.RandomStateStep);
                Assert.NotEqual(source.RandomStateSha256, trial.RandomStateSha256);
                var reopenedRandom = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
                    Path.Combine(scenario.Root, trial.RandomStateRelativePath), trial.RandomStateSha256,
                    trial.RandomStateByteLength, trial.RandomStateStep, trial.RandomStateTopologySha256, trial.LoraConfigurationSha256);
                Assert.True(reopenedRandom.IsSuccess, reopenedRandom.ErrorMessage);
                trialRandom = reopenedRandom.Value!;
                Assert.Equal(sourceRandom!.Entries.Select(e => (e.AdapterKey, e.StreamDomain, e.InitializationSeed, e.DropoutSeed)),
                    trialRandom.Entries.Select(e => (e.AdapterKey, e.StreamDomain, e.InitializationSeed, e.DropoutSeed)));
                Assert.All(sourceRandom.Entries.Zip(trialRandom.Entries), pair =>
                    Assert.True(pair.Second.ConsumedCursor > pair.First.ConsumedCursor));
            }
            AssertRetainedResumeFiles(scenario.Root, before, allowAdditionalFiles: true);
            foreach (var (path, value) in stable)
            { Assert.Equal(value.Bytes, File.ReadAllBytes(path)); Assert.Equal(value.Ticks, File.GetLastWriteTimeUtc(path).Ticks); }
            var after = SnapshotResumeFiles(scenario.Root);
            var timestamps = after.Keys.ToDictionary(p => p, p => File.GetLastWriteTimeUtc(p).Ticks);
            var resumed = scenario.ReconstructForEvaluation("warm-negative-fresh-owner", trialPass: true);
            Assert.Null(resumed.Request.OptimizerStateToRestore); Assert.Null(resumed.Request.RandomStateToRestore);
            var reopened = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecutePreparedLessonWithOutcomeAsync(resumed.Request, resumed.Inputs);
            _output.WriteLine("S04_GENUINE_WARM_RECOVERY " + JsonSerializer.Serialize(new
            { dropout, source, trial, sourceOptimizerStep = sourceOptimizer.Value.State.Step,
                trialOptimizerStep = trialOptimizer.Value.State.Step, localSuffix = trial.DpoStepCount,
                requests, mutationSeams, recoveryCode = reopened.ErrorCode, recoveryMessage = reopened.ErrorMessage,
                recovered = reopened.IsSuccess, sourceRandom, trialRandom,
                firstStartedSha = first.Value.StartedSha256, firstCompletedSha = first.Value.CompletedSha256,
                reopenedStartedSha = reopened.Value?.StartedSha256, reopenedCompletedSha = reopened.Value?.CompletedSha256,
                runtime = CoordinatorRuntimeIdentity() }));
            Assert.True(reopened.IsSuccess, reopened.ErrorCode + ":" + reopened.ErrorMessage);
            Assert.Equal(first.Value.StartedSha256, reopened.Value!.StartedSha256);
            Assert.Equal(first.Value.CompletedSha256, reopened.Value.CompletedSha256);
            Assert.Equal(JsonSerializer.Serialize(first.Value.Detailed), JsonSerializer.Serialize(reopened.Value.Detailed));
            Assert.Equal(1, requests); Assert.Equal(1, mutationSeams);
            Assert.Equal(0, resumed.Broker.Calls); Assert.Equal(0, resumed.Client.IssuedCallCount);
            Assert.Equal(30, scenario.Client.IssuedCallCount);
            AssertRetainedResumeFiles(scenario.Root, after);
            foreach (var (path, ticks) in timestamps) Assert.Equal(ticks, File.GetLastWriteTimeUtc(path).Ticks);
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = previousRequest;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = previousMutation;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecutePreparedLessonWithOutcomeAsync_GenuineWarmImprovedPublishesAndAlreadyApplied(bool interrupted)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: true, genuineWarm: true);
        var retained = SnapshotResumeFiles(scenario.Root);
        var requests = 0; var mutationSeams = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        var priorMutation = LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => requests++;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = (_, _) => mutationSeams++;
            if (interrupted)
            {
                scenario.BlockPublicationAfterCompleteTrial();
                var staged = await scenario.Execute();
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishFailedErrorCode, staged.ErrorCode);
                Assert.True(File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json")));
                scenario.RemovePublicationBlocker();
            }
            var receiving = scenario.ReconstructForEvaluation("warm-improved-owner");
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecutePreparedLessonWithOutcomeAsync(receiving.Request, receiving.Inputs);
            Assert.True(result.IsSuccess, result.ErrorCode + ":" + result.ErrorMessage);
            Assert.Equal("Improved", result.Value!.Disposition.ToString());
            Assert.Equal(scenario.Parent.Step + 1, result.Value.PublishedCheckpointStep);
            Assert.Equal(1, requests); Assert.Equal(1, mutationSeams);
            Assert.Equal(interrupted ? 0 : 1, receiving.Broker.Calls);
            Assert.Equal(interrupted ? 0 : 15, receiving.Client.IssuedCallCount);
            AssertRetainedResumeFiles(scenario.Root, retained, allowAdditionalFiles: true);
            var accepted = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(receiving.Request.StartRequest);
            Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
            Assert.Equal(result.Value.PublishedCheckpointStep, accepted.Value!.ParentManifest!.Step);
            var publication = SnapshotResumeFiles(scenario.Root);
            var again = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecutePreparedLessonWithOutcomeAsync(receiving.Request, receiving.Inputs);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PairAlreadyAppliedErrorCode, again.ErrorCode);
            Assert.Equal(1, requests); Assert.Equal(1, mutationSeams);
            AssertRetainedResumeFiles(scenario.Root, publication);
            _output.WriteLine("S04_GENUINE_WARM_IMPROVED " + JsonSerializer.Serialize(new
            { interrupted, requests, mutationSeams, receivingBroker = receiving.Broker.Calls,
                receivingEvaluator = receiving.Client.IssuedCallCount, published = result.Value.PublishedCheckpointStep,
                alreadyApplied = again.ErrorCode, runtime = CoordinatorRuntimeIdentity() }));
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = priorMutation;
        }
    }

    [Theory]
    [InlineData("missing-source")]
    [InlineData("source-manifest")]
    [InlineData("source-optimizer")]
    [InlineData("source-rng")]
    [InlineData("source-policy")]
    [InlineData("trial-optimizer")]
    [InlineData("trial-rng")]
    [InlineData("trial-policy")]
    [InlineData("trial-completed")]
    [InlineData("mixed-source-optimizer")]
    [InlineData("mixed-source-rng")]
    public async Task ExecutePreparedLessonWithOutcomeAsync_GenuineWarmCorruptRecoveryRefusesWithoutEffects(string fault)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: true, genuineWarm: true, trialPass: false);
        var first = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
            .ExecutePreparedLessonWithOutcomeAsync(scenario.Request, scenario.Inputs);
        Assert.True(first.IsSuccess, first.ErrorCode + ":" + first.ErrorMessage);
        var receiving = scenario.ReconstructForEvaluation("warm-corrupt-owner");
        var sourceAttempt = scenario.Request.OptimizerStateSourceAttempt!;
        var source = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(sourceAttempt).Value!.Manifest;
        var trial = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Attempt).Value!.Manifest;
        var target = fault switch
        {
            "source-manifest" => Path.Combine(sourceAttempt.AttemptRoot, CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.ManifestFileName),
            "source-optimizer" or "mixed-source-optimizer" => Path.Combine(scenario.Root, source.OptimizerStateRelativePath),
            "source-rng" or "mixed-source-rng" => Path.Combine(scenario.Root, source.RandomStateRelativePath),
            "source-policy" => Path.Combine(scenario.Root, source.TrialWeightsRelativePath),
            "trial-optimizer" => Path.Combine(scenario.Root, trial.OptimizerStateRelativePath),
            "trial-rng" => Path.Combine(scenario.Root, trial.RandomStateRelativePath),
            "trial-policy" => Path.Combine(scenario.Root, trial.TrialWeightsRelativePath),
            "trial-completed" => Path.Combine(scenario.Attempt.AttemptRoot, "trial-completed.json"),
            _ => ""
        };
        // Fault injection is an explicit fixture write, separate from effects
        // and durable writes of the invocation under test.
        if (fault == "missing-source") receiving = receiving with { Request = receiving.Request with { OptimizerStateSourceAttempt = null } };
        else if (fault == "mixed-source-optimizer") File.WriteAllBytes(target, File.ReadAllBytes(Path.Combine(scenario.Root, trial.OptimizerStateRelativePath)));
        else if (fault == "mixed-source-rng") File.WriteAllBytes(target, File.ReadAllBytes(Path.Combine(scenario.Root, trial.RandomStateRelativePath)));
        else File.Delete(target);
        var retained = SnapshotResumeFiles(scenario.Root);
        var timestamps = retained.Keys.ToDictionary(p => p, p => File.GetLastWriteTimeUtc(p).Ticks);
        var requests = 0; var mutationSeams = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        var priorMutation = LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => requests++;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = (_, _) => mutationSeams++;
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ExecutePreparedLessonWithOutcomeAsync(receiving.Request, receiving.Inputs);
            Assert.False(result.IsSuccess);
            var expected = fault is "source-manifest" or "source-optimizer" or "source-rng" or "mixed-source-optimizer" or "mixed-source-rng"
                ? CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.RandomStateRestoreRequiredErrorCode
                : CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode;
            Assert.Equal(expected, result.ErrorCode);
            if (fault == "missing-source") Assert.Contains("no canonical optimizer-state source", result.ErrorMessage);
            if (fault == "source-policy") Assert.Contains("not the accepted parent", result.ErrorMessage);
            Assert.Equal(0, requests); Assert.Equal(0, mutationSeams);
            Assert.Equal(0, receiving.Broker.Calls); Assert.Equal(0, receiving.Client.IssuedCallCount);
            AssertRetainedResumeFiles(scenario.Root, retained);
            foreach (var (path, ticks) in timestamps) Assert.Equal(ticks, File.GetLastWriteTimeUtc(path).Ticks);
            _output.WriteLine("S04_GENUINE_WARM_FAULT " + JsonSerializer.Serialize(new
            { fault, guard = result.ErrorCode, message = result.ErrorMessage, requests, mutationSeams,
                productWrites = 0, faultInjectionWrites = fault == "missing-source" ? 0 : 1 }));
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = priorMutation;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecutePreparedLessonWithOutcomeAsync_GenuineWarmValidForeignSourceRefuses(bool recovery)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: true, genuineWarm: true, trialPass: false);
        if (recovery)
        {
            var first = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(scenario.Request, scenario.Inputs);
            Assert.True(first.IsSuccess, first.ErrorCode + ":" + first.ErrorMessage);
        }
        var foreignDpo = SealRepositoryOwnedDpo(scenario.Root, "warm-valid-foreign", "v-foreign");
        var context = scenario.Request.QualityGate!;
        var foreignClient = new ScriptedQualityGateEvaluationClient(context.EvaluatorConfiguration, Enumerable.Repeat(false, 30).ToArray(), i => "foreign-" + i);
        var foreignRequest = scenario.Request with
        {
            RoutingRun = CreateRoutingRun(foreignDpo.Receipt, foreignDpo.Pairs), DpoDatasetAddress = foreignDpo.Address,
            TrainingRunId = "prepared-coordinator:foreign-source", LeaseBroker = CreateAcceptingBroker(),
            QualityGate = context with { EvaluationClient = foreignClient }
        };
        var foreign = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(foreignRequest);
        Assert.False(foreign.IsSuccess);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode, foreign.ErrorCode);
        var foreignAttempt = CreateModelTrainingAttemptAddress(scenario.Root, foreignRequest.TrainingRunId);
        var actual = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(foreignAttempt);
        Assert.True(actual.IsSuccess, actual.ErrorMessage);
        Assert.NotEqual(scenario.Parent.WeightsSha256, actual.Value!.Manifest.TrialWeightsSha256);
        var receiving = scenario.ReconstructForEvaluation("foreign-source-owner");
        var retained = SnapshotResumeFiles(scenario.Root);
        var requests = 0; var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => requests++;
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(
                receiving.Request with { OptimizerStateSourceAttempt = foreignAttempt }, receiving.Inputs);
            Assert.Equal(recovery ? CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode :
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OptimizerStateRestoreRequiredErrorCode, result.ErrorCode);
            Assert.Equal("The optimizer-state source policy checkpoint is not the accepted parent for the requested continuation.", result.ErrorMessage);
            Assert.Equal(0, requests); Assert.Equal(0, receiving.Broker.Calls); Assert.Equal(0, receiving.Client.IssuedCallCount);
            AssertRetainedResumeFiles(scenario.Root, retained);
            _output.WriteLine("S04_GENUINE_WARM_FOREIGN " + JsonSerializer.Serialize(new { recovery, guard = result.ErrorCode, message = result.ErrorMessage }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData("lease")]
    [InlineData("mutation")]
    [InlineData("evaluation")]
    public async Task ExecutePreparedLessonWithOutcomeAsync_GenuineWarmLateSourceMutationRefuses(string boundary)
    {
        using var scenario = await CreatePreparedCoordinatorScenario(seededOriginal: true, genuineWarm: true, trialPass: false);
        var source = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(scenario.Request.OptimizerStateSourceAttempt!).Value!.Manifest;
        var ownedRoot = Path.GetFullPath(scenario.Root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var sourcePath = Path.GetFullPath(Path.Combine(ownedRoot,
            source.TrialWeightsRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var pathComparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        Assert.True(sourcePath.StartsWith(ownedRoot + Path.DirectorySeparatorChar, pathComparison), sourcePath);
        var before = SnapshotResumeFiles(scenario.Root);
        var intendedTarget = Assert.Single(before.Keys.Where(path =>
            string.Equals(Path.GetFullPath(path), sourcePath, pathComparison)));
        var originalSourceBytes = File.ReadAllBytes(intendedTarget);
        Assert.Equal(before[intendedTarget], ComputeFileSha256(intendedTarget));
        var expectedFaultBytes = originalSourceBytes.ToArray(); expectedFaultBytes[^1] ^= 1;
        var requests = 0; var mutationSeams = 0; var injected = 0;
        void Mutate()
        {
            Assert.Equal(0, injected);
            Assert.Equal(originalSourceBytes, File.ReadAllBytes(intendedTarget));
            File.WriteAllBytes(intendedTarget, expectedFaultBytes); injected++;
        }
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        var priorMutation = LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting;
        var priorBeforeAcquire = scenario.Broker.BeforeAcquire;
        var priorTrialResponse = scenario.TrialResponse;
        try
        {
            if (boundary == "lease") scenario.Broker.BeforeAcquire = Mutate;
            if (boundary == "evaluation") scenario.TrialResponse = (_, response) => { if (injected == 0) Mutate(); return response; };
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => requests++;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = (_, _) => { mutationSeams++; if (boundary == "mutation") Mutate(); };
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecutePreparedLessonWithOutcomeAsync(scenario.Request, scenario.Inputs);
            Assert.False(result.IsSuccess); Assert.Equal(1, injected);
            Assert.Equal(expectedFaultBytes, File.ReadAllBytes(intendedTarget));
            Assert.NotEqual(before[intendedTarget], ComputeFileSha256(intendedTarget));
            Assert.Equal(boundary == "lease" ? 0 : 1, mutationSeams);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainingAttemptRecoveryRequiredErrorCode, result.ErrorCode);
            if (boundary == "mutation") Assert.Contains("No zero-effect claim", result.ErrorMessage);
            if (boundary == "evaluation") Assert.Equal("Prepared Trial inputs changed after evaluation.", result.ErrorMessage);
            Assert.False(Directory.Exists(CreateModelCheckpointAddress(scenario.Root, 3).CheckpointRoot));
            foreach (var (path, sha) in before.Where(kv => kv.Key != intendedTarget)) Assert.Equal(sha, ComputeFileSha256(path));
            _output.WriteLine("S04_GENUINE_WARM_LATE " + JsonSerializer.Serialize(new
            { boundary, guard = result.ErrorCode, message = result.ErrorMessage, requests, mutationSeams, faultInjectionWrites = injected,
                sourcePath = intendedTarget, sourceBeforeSha256 = before[intendedTarget], sourceAfterSha256 = ComputeFileSha256(intendedTarget),
                committedUpdate = File.Exists(Path.Combine(scenario.Attempt.AttemptRoot, DpoTrainer.BuildPolicyCheckpointFileName(1))) }));
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = priorMutation;
            scenario.Broker.BeforeAcquire = priorBeforeAcquire;
            scenario.TrialResponse = priorTrialResponse;
            File.WriteAllBytes(intendedTarget, originalSourceBytes);
            Assert.Equal(before[intendedTarget], ComputeFileSha256(intendedTarget));
        }
    }

    private async Task<PreparedCoordinatorScenario> CreatePreparedCoordinatorScenario(bool seededOriginal = false,
        int caseCount = 5, int repeats = 3, bool trialPass = true, bool stageTrialFailure = false,
        int baselinePassCount = 0, string? runIdOverride = null, bool zeroWeightParent = false,
        string lessonScope = "prepared-current", bool genuineWarm = false)
    {
        seededOriginal |= zeroWeightParent;
        var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("s04-prepared-coordinator");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(fixture.ModelConfig, "prepared-coordinator");
            var originalLora = seededOriginal ? CreateLoraConfig() with { Dropout = 0.2f } : genuineWarm || zeroWeightParent ? CreateLoraConfig() : null;
            var seeds = seededOriginal || genuineWarm ? new LoraPairRandomSeedAuthority(991, "policy.step", "reference.step") : null;
            var parentDpo = SealRepositoryOwnedDpo(root, "prepared-parent", "v0-parent", "doc code", "keep code", "drop code");
            var parentModel = fixture.CreateFactory().Create("CODE").Value!.Model;
            AdamWPortableState? parentOptimizer = null;
            LlmModelRandomStateSnapshot? parentRandom = null;
            if (seededOriginal)
            {
                // A seeded accepted parent is an actual continuation: bootstrap one
                // real update and persist its optimizer/RNG through their owners.
                var parentReference = CloneModel(parentModel);
                LoraApplicator.ApplyFreshSeededPair(parentModel, parentReference, originalLora,
                    seeds!.RootInitializationSeed, seeds.PolicyDropoutStreamDomain, seeds.ReferenceDropoutStreamDomain);
                if (zeroWeightParent)
                {
                    // Real zero tensors produce zero gradients and moments in both
                    // real optimizer steps; no altered assessment graph is supplied.
                    foreach (var model in new[] { parentModel, parentReference })
                        foreach (var entry in model.GetCheckpointParameterEntries()) Array.Clear(entry.Parameter.Value.Data);
                }
                var parentStateRoot = Path.Combine(root, "seeded-parent");
                Directory.CreateDirectory(parentStateRoot);
                var topology = LoraRandomStateIdentity.ComputeTopologySha256(fixture.ModelConfig, originalLora, seeds);
                var configuration = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(originalLora, seeds);
                var parentTrainer = global::DpoTrainerOptimizerStateHandoffTests.CreateSeededTrainer(
                    parentModel, parentReference, parentStateRoot, topology, configuration);
                var sealedParent = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(parentDpo.Address, parentDpo.RequestIdentity);
                Assert.True(sealedParent.IsSuccess, sealedParent.ErrorMessage);
                var parentPair = Assert.Single(sealedParent.Value!.Pairs);
                var preparedParent = LogicalDpoTrainingUnit.PrepareExecution(
                    [new LogicalDpoPreferenceExample(parentPair.Prompt, parentPair.Chosen, parentPair.Rejected)
                    {
                        ChosenAuthorKind = parentPair.ChosenAuthorship!.AuthorKind,
                        RejectedAuthorKind = parentPair.RejectedAuthorship!.AuthorKind
                    }], tokenizer);
                Assert.True(preparedParent.IsSuccess, preparedParent.ErrorMessage);
                parentTrainer.Step(Assert.Single(preparedParent.Value!.OptimizerInputs).Batch.Batch, 1);
                var optimizerFile = RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(parentStateRoot, 1,
                    parentTrainer.ExportOptimizerState());
                Assert.True(optimizerFile.IsSuccess, optimizerFile.ErrorMessage);
                var randomFile = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                    Path.Combine(parentStateRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(1)),
                    parentModel.CaptureRandomState(topology, configuration, 1));
                Assert.True(randomFile.IsSuccess, randomFile.ErrorMessage);
                var optimizerReadback = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(parentStateRoot, 1,
                    optimizerFile.Value!.OptimizerStateSha256);
                var randomReadback = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(randomFile.Value!.Path,
                    randomFile.Value.Sha256, randomFile.Value.ByteLength, 1, topology, configuration);
                Assert.True(optimizerReadback.IsSuccess, optimizerReadback.ErrorMessage);
                Assert.True(randomReadback.IsSuccess, randomReadback.ErrorMessage);
                parentOptimizer = optimizerReadback.Value!.State;
                parentRandom = randomReadback.Value!;
            }
            var parent = SealModelCheckpoint(root, parentModel, fixture.ModelConfig, compatibility,
                parentDpo.Identity, 1, null, "synthetic prepared coordinator parent");
            var entries = Enumerable.Range(1, caseCount).Select(i => CreateQualityReferenceEntry(
                "case " + i, "ok " + i, FrozenReferencePartition.HeldOut, "heldout-" + i))
                .Prepend(CreateQualityReferenceEntry("train 0", "ok train", FrozenReferencePartition.Train, "train"));
            var corpus = SealQualityReferenceCorpus(root, suppliedEntries: entries);
            RepositoryOwnedModelTrainingAttemptAddress? sourceAttempt = null;
            if (genuineWarm)
            {
                // The legacy publication owner creates the accepted trained Parent
                // and its canonical optimizer/RNG bundle. No DTO state is seeded.
                var sourceDpo = SealRepositoryOwnedDpo(root, "warm-source", "v0-trained");
                var sourceRun = "prepared-coordinator:warm-source";
                var sourceStart = CreateStartRequest(root, fixture.ModelConfig, compatibility,
                    RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied, originalLora ?? CreateLoraConfig())
                    with { AdapterSeedAuthority = seededOriginal ? seeds : null };
                var source = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(
                    new(CreateRoutingRun(sourceDpo.Receipt, sourceDpo.Pairs), sourceDpo.Address, sourceStart, tokenizer,
                        originalLora ?? CreateLoraConfig(), CreateAcceptingBroker(), sourceRun, 1,
                        DateTimeOffset.UnixEpoch, ["test:genuine-warm-source"],
                        CreateQualityGateContext(corpus, tokenizer, CreateQualityOutcomes(false, true)),
                        OptimizerStateToRestore: parentOptimizer,
                        OptimizerStateColdStartAuthorization: parentOptimizer is null ? ExplicitOptimizerColdStartAuthorization("warm source root") : null,
                        AdapterSeedAuthority: seededOriginal ? seeds : null,
                        RandomStateToRestore: parentRandom));
                Assert.True(source.IsSuccess, source.ErrorCode + ":" + source.ErrorMessage);
                sourceAttempt = CreateModelTrainingAttemptAddress(root, sourceRun);
                var trained = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(sourceStart);
                Assert.True(trained.IsSuccess, trained.ErrorMessage);
                parent = trained.Value!.ParentManifest!;
                Assert.Equal(2, parent.Step);
                Assert.NotNull(parent.ParentCheckpoint);
                Assert.True(AnyLoraBMatrixContainsNonZeroValues(trained.Value.Model));
                // The prepared objective must reopen this actual legacy source,
                // rather than receiving the root bootstrap's optional DTOs.
                parentOptimizer = null;
                parentRandom = null;
            }
            var calls = checked(caseCount * repeats);
            var teacher = CreateTeacherConfiguration(checked(2 * calls));
            PreparedCoordinatorScenario? scenario = null;
            var client = new ScriptedQualityGateEvaluationClient(teacher,
                Enumerable.Range(0, calls).Select(i => i % caseCount < baselinePassCount)
                    .Concat(stageTrialFailure ? [] : Enumerable.Repeat(trialPass, calls)).ToArray(),
                i => "original-and-first-trial-\"\\-" + i,
                (number, response) => number > calls && scenario?.TrialResponse is { } transform ? transform(number, response) : response);
            var context = CreateQualityGateContext(corpus, tokenizer, [], evaluatorConfiguration: teacher) with
            {
                EvaluationClient = client,
                GenerationSettings = new(1, 0, 1, 1, 17, caseCount, 256),
                NoisePolicyConfig = new MeasurementNoisePolicyConfig(repeats),
                MaximumDuration = TimeSpan.FromSeconds(caseCount * repeats == 5000 ? 120 : 30),
                EvidenceRefs = new string[] { "test:prepared-coordinator" }
            };
            var grant = new CodeDocumentationOneLessonGrantV1("prepared-grant", "v1", "prepared-selector",
                "prepared-objective", "prepared-objective-run", Sha256Hex("candidate"), parent.ManifestSha256,
                parent.WeightsSha256, compatibility.ConfigSha256, Sha256Hex("exam"))
            {
                FormatVersion = 2,
                DescriptorsV2 = new(2, Sha256Hex("source"), Sha256Hex("parent"), Sha256Hex("selection"),
                    Sha256Hex("corpus"), Sha256Hex("plan"), caseCount, repeats, Sha256Hex("evaluator"), Sha256Hex("profile"))
            };
            var catalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([grant], 2 * calls);
            var selector = new CodeDocumentationOneLessonSelectorV1(grant.ObjectiveId, grant.GrantSelector);
            var newPairAddress = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug, "v1-new").Value!;
            Assert.False(Directory.Exists(newPairAddress.DatasetRoot));
            var producerPoints = new List<string>();
            var original = await new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(durablePoint: producerPoints.Add).ProduceOrOpenAsync(
                catalog, selector, root, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug, "code-documentation-llm", "v1",
                fixture.ModelConfig, compatibility, context, originalLora, seeds);
            Assert.True(original.IsSuccess, original.ErrorCode + ": " + original.ErrorMessage +
                "; producer points=" + string.Join(",", producerPoints) + "; issued=" + client.IssuedCallCount +
                "; completed=" + client.CompletedResponseCount);
            Assert.Equal("NEEDS_LESSON", original.Value!.Classification);
            Assert.Equal(calls, client.IssuedCallCount);
            Assert.False(Directory.Exists(newPairAddress.DatasetRoot));
            // The current lesson is created only AFTER the real original producer.
            var dpo = SealRepositoryOwnedDpo(root, lessonScope, "v1-new");
            var trainingLora = seededOriginal ? CreateLoraConfig() with { Dropout = 0.2f } : CreateLoraConfig();
            var start = CreateStartRequest(root, fixture.ModelConfig, compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied, trainingLora) with { AdapterSeedAuthority = seededOriginal ? seeds : null };
            var broker = new PreparedCoordinatorBroker(CreateAcceptingBroker());
            var runId = runIdOverride ?? "prepared-coordinator:one-update";
            var request = new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(CreateRoutingRun(dpo.Receipt, dpo.Pairs),
                dpo.Address, start, tokenizer, trainingLora, broker, runId, 1, DateTimeOffset.UnixEpoch.AddDays(1),
                new string[] { "test:prepared-coordinator" }, context,
                OptimizerStateToRestore: parentOptimizer,
                OptimizerStateColdStartAuthorization: seededOriginal || genuineWarm ? null : ExplicitOptimizerColdStartAuthorization("prepared coordinator"),
                AdapterSeedAuthority: seededOriginal ? seeds : null,
                RandomStateToRestore: parentRandom,
                OptimizerStateSourceAttempt: sourceAttempt);
            var inputs = new CodeDocumentationPreparedBaselineAttemptInputs(original.Value.Reference, catalog, selector,
                fixture.ModelConfig with { }, compatibility with { }, context, originalLora, seeds, dpo.Address, dpo.RequestIdentity);
            scenario = new(fixture, root, parent, dpo, request, inputs, original.Value, client, broker);
            return scenario;
        }
        catch { DeleteDirectoryIfExists(root); fixture.Dispose(); throw; }
    }

    private sealed class PreparedCoordinatorScenario(TinyTrainingFixtureCL fixture, string root,
        RepositoryOwnedModelCheckpointManifest parent, RepositoryOwnedCodeDocumentationDpoSetup dpo,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request,
        CodeDocumentationPreparedBaselineAttemptInputs inputs, CodeDocumentationPreparedBaselineReadback original,
        ScriptedQualityGateEvaluationClient client, PreparedCoordinatorBroker broker) : IDisposable
    {
        internal string Root { get; } = root;
        internal RepositoryOwnedModelCheckpointManifest Parent { get; } = parent;
        internal RepositoryOwnedCodeDocumentationDpoSetup Dpo { get; } = dpo;
        internal CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest Request = request;
        internal CodeDocumentationPreparedBaselineAttemptInputs Inputs = inputs;
        internal CodeDocumentationPreparedBaselineReadback Original { get; } = original;
        internal byte[] OriginalBytes { get; } = File.ReadAllBytes(original.Reference.BaselinePath);
        internal ScriptedQualityGateEvaluationClient Client { get; } = client;
        internal PreparedCoordinatorBroker Broker { get; } = broker;
        internal RepositoryOwnedModelTrainingAttemptAddress Attempt { get; } = CreateModelTrainingAttemptAddress(root, request.TrainingRunId);
        internal Func<int, TeacherEvaluationInferenceResult, TeacherEvaluationInferenceResult>? TrialResponse;
        internal void BlockPublicationAfterCompleteTrial()
        {
            // The existing publisher's provenance grammar rejects this request
            // only after a genuine complete positive Trial has been persisted.
            Request = Request with { ProvenanceRefs = new[] { new string('x', 257) } };
        }
        internal void RemovePublicationBlocker() => Request = Request with { ProvenanceRefs = new[] { "test:prepared-coordinator" } };
        internal Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>> Execute(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest? changed = null, CancellationToken token = default)
            => CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteFromPreparedBaselineAsync(changed ?? Request, Inputs, token);

        internal PreparedCoordinatorRecovery ReconstructForEvaluation(string scope, bool trialPass = true)
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var context = Request.QualityGate!;
            var evaluator = new ScriptedQualityGateEvaluationClient(context.EvaluatorConfiguration,
                Enumerable.Repeat(trialPass, Original.Baseline.ExpectedEvaluationCallCount).ToArray(), i => scope + "-" + i);
            var quality = context with { Tokenizer = tokenizer, EvaluationClient = evaluator, EvidenceRefs = context.EvidenceRefs.ToArray() };
            var freshBroker = new PreparedCoordinatorBroker(CreateAcceptingBroker());
            var recovered = Request with
            {
                RoutingRun = CreateRoutingRun(Dpo.Receipt with { OrderedPairs = Dpo.Receipt.OrderedPairs.ToArray() }, Dpo.Pairs.ToArray()),
                StartRequest = Request.StartRequest with { ModelConfig = Request.StartRequest.ModelConfig with { },
                    RuntimeCompatibility = Request.StartRequest.RuntimeCompatibility with { } },
                Tokenizer = tokenizer, QualityGate = quality, LeaseBroker = freshBroker,
                ProvenanceRefs = Request.ProvenanceRefs.ToArray(),
                OptimizerStateToRestore = null, RandomStateToRestore = null
            };
            return new(recovered, Inputs with { ModelConfig = recovered.StartRequest.ModelConfig,
                RuntimeCompatibility = recovered.StartRequest.RuntimeCompatibility, QualityGateContext = quality }, evaluator, freshBroker);
        }
        public void Dispose() { DeleteDirectoryIfExists(Root); fixture.Dispose(); }
    }

    private sealed record PreparedCoordinatorRecovery(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest Request,
        CodeDocumentationPreparedBaselineAttemptInputs Inputs, ScriptedQualityGateEvaluationClient Client, PreparedCoordinatorBroker Broker);

    private sealed class PreparedCoordinatorBroker(IServerOwnedDpoTrainingLeaseBroker owner) : IServerOwnedDpoTrainingLeaseBroker
    {
        internal int Calls;
        internal Action? BeforeAcquire;
        public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease> AcquireTranslationTrainingLease(
            DpoTrainingContentBatch content, string runId) => owner.AcquireTranslationTrainingLease(content, runId);
        public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease> AcquireCodeDocumentationTrainingLease(
            DpoTrainingContentBatch content, string runId)
        { Calls++; BeforeAcquire?.Invoke(); return owner.AcquireCodeDocumentationTrainingLease(content, runId); }
    }

    private static CodeDocumentationPreparedBaselineReference ChangedReference(CodeDocumentationPreparedBaselineReference r, string sha)
        => new(r.FormatVersion, r.RepositoryRoot, r.ObjectiveSha256, r.BaselineRoot, r.BaselineRootRelativePath,
            r.BaselinePath, r.ClaimPath, r.LockPath, r.RequestSha256, sha, r.ByteLength, r.Classification);

    private static object CoordinatorRuntimeIdentity()
        => new[] { typeof(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator).Assembly,
            typeof(CodeDocumentationServerOwnedDpoTrainingExecutor).Assembly, typeof(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests).Assembly }
            .Select(a => new { name = a.GetName().Name, location = a.Location, mvid = a.ManifestModule.ModuleVersionId,
                informationalVersion = a.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                    .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().SingleOrDefault()?.InformationalVersion,
                bytes = new FileInfo(a.Location).Length, sha256 = ComputeFileSha256(a.Location),
                lastWriteUtcTicks = new FileInfo(a.Location).LastWriteTimeUtc.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture) }).ToArray();

    private static string[] CoordinatorRequestIds(CodeDocumentationSavedModelAssessmentCalibrationReport report)
        => report.Observations.SelectMany(o => o.ObservedEvaluationRequestIds).ToArray();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TrainAndPublishAsync_ReopensRepositoryDpoTrainsAndPublishesNextCheckpoint(bool initialBaseline)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-dpo-training-publish");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-lifecycle");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-parent",
                datasetVersion: "v0-parent", prompt: "doc code", chosen: "keep code", rejected: "drop code");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-new",
                datasetVersion: "v1-new", prompt: "doc code", chosen: "keep code", rejected: "drop code");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = initialBaseline
                ? CreateInitialBaseline()
                : SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent for CodeDocumentation DPO lifecycle");
            RepositoryOwnedModelCheckpointManifest CreateInitialBaseline()
            {
                // Один реальный owner создаёт свежую модель; caller не передаёт ему parentSession.Model.
                var baselineRequest = CreateStartRequest(repositoryRoot, fixture.ModelConfig, compatibility,
                    RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize("synthetic initial baseline without optimizer"));
                var published = CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.PublishInitialBaseline(
                    baselineRequest, dpoSetup.Identity, DateTimeOffset.UnixEpoch, ["test:initial-baseline-owner"]);
                Assert.True(published.IsSuccess, published.ErrorCode + ": " + published.ErrorMessage);
                var reopened = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(CreateModelCheckpointAddress(repositoryRoot, 1));
                Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
                Assert.Equal(RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate, reopened.Value!.Manifest.DpoApplication);
                Assert.Equal(dpoSetup.Identity, reopened.Value.Manifest.DpoDatasetManifest);
                return reopened.Value.Manifest;
            }
            var childAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 2);
            Assert.False(Directory.Exists(childAddress.CheckpointRoot));
            var trainingRunId = "code-doc-lifecycle:repository-owned-dpo";
            var attemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);
            Assert.False(Directory.Exists(attemptAddress.AttemptRoot));
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var routingRun = CreateRoutingRun(dpoSetup.Receipt, dpoSetup.Pairs);
            string? observedLogicalCheckpointDirectory = null;
            var observedPreTrainerRootExists = false;
            var observedPreTrainerRootOwnerOnly = false;
            BaselineAttemptStateSnapshot? observedBaselineBeforeTrainer = null;
            AdamWPortableState? observedOptimizerStateToRestore = null;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;

            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        observedLogicalCheckpointDirectory =
                            logicalRequest.CheckpointDirectory;
                        observedBaselineBeforeTrainer =
                            ReadBaselineAttemptStateSnapshotForTest(
                                attemptAddress);
                        observedOptimizerStateToRestore =
                            logicalRequest.OptimizerStateToRestore;
                        if (OperatingSystem.IsLinux())
                        {
                            observedPreTrainerRootExists =
                                Directory.Exists(attemptAddress.AttemptRoot);
                            observedPreTrainerRootOwnerOnly =
                                SecureLocalFileBoundary
                                    .HasOwnerOnlyDirectoryMode(
                                        attemptAddress.AttemptRoot);
                        }
                    };

                ScriptedQualityGateEvaluationClient? qualityClient = null;
                var qualityContext =
                    CreateQualityGateContext(
                        qualityCorpus,
                        tokenizer,
                        CreateQualityOutcomes(
                            baselinePass: false,
                            trialPass: true),
                        captureClient: client => qualityClient = client);

                if (!initialBaseline)
                {
                    // Старый unmarked Step 1 с ColdStartReason сохраняет строгую историю применения.
                    var changedAddress = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(repositoryRoot,
                        request.DomainSlug, request.DesignPatternSlug, "v2-same-id-changed-bytes").Value!;
                    var changedPair = parentDpoSetup.Pairs[0] with { CapturedAt = DateTimeOffset.UnixEpoch.AddSeconds(1) };
                    var changed = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(changedAddress, parentDpoSetup.RequestIdentity, [changedPair]);
                    Assert.True(changed.IsSuccess, changed.ErrorMessage);
                    Assert.Equal(parentDpoSetup.Receipt.OrderedPairs[0].PairId, changed.Value!.OrderedPairs[0].PairId);
                    Assert.NotEqual(parentDpoSetup.Receipt.OrderedPairs[0].PairFileSha256, changed.Value.OrderedPairs[0].PairFileSha256);
                    var changedReceipt = parentDpoSetup.Receipt with
                    {
                        ManifestSha256 = changed.Value.ManifestSha256, RepositoryRelativeManifestPath = changedAddress.ManifestRelativePath,
                        OrderedPairs = changed.Value.OrderedPairs.ToArray()
                    };
                    foreach (var (receipt, address, pairs, error) in new[]
                    {
                        (parentDpoSetup.Receipt, parentDpoSetup.Address, parentDpoSetup.Pairs,
                            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PairAlreadyAppliedErrorCode),
                        (changedReceipt, changedAddress, new[] { changedPair },
                            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.AppliedPairIdentityConflictErrorCode)
                    })
                    {
                        var legacy = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(
                            new(CreateRoutingRun(receipt, pairs), address, request, tokenizer, CreateLoraConfig(), CreateAcceptingBroker(),
                                "legacy-root-replay:" + address.DatasetVersion, 1, DateTimeOffset.UnixEpoch, ["test:legacy-root-replay"], qualityContext,
                                OptimizerStateColdStartAuthorization: ExplicitOptimizerColdStartAuthorization("root lifecycle")));
                        Assert.Equal(error, legacy.ErrorCode);
                        Assert.Null(observedLogicalCheckpointDirectory);
                        Assert.Equal(0, qualityClient!.IssuedCallCount);
                        Assert.False(Directory.Exists(childAddress.CheckpointRoot));
                    }
                }

                var result =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(
                            new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                                routingRun,
                                dpoSetup.Address,
                                request,
                            tokenizer,
                            CreateLoraConfig(),
                            CreateAcceptingBroker(),
                            trainingRunId,
                            DpoStepCount: 1,
                            DateTimeOffset.UnixEpoch.AddDays(1),
                            ["test:code-documentation-dpo-training-publication"],
                             qualityContext,
                             OptimizerStateColdStartAuthorization:
                                 ExplicitOptimizerColdStartAuthorization(
                                     "root lifecycle")))
                        .WaitAsync(TimeSpan.FromSeconds(30));

                Assert.True(result.IsSuccess,
                    $"{result.ErrorCode}: {result.ErrorMessage}");
                Assert.NotNull(result.Value);
                Assert.Equal(parentManifest.Step, result.Value!.ParentCheckpointStep);
                Assert.Equal(2, result.Value.PublishedCheckpointStep);
                Assert.Equal(dpoSetup.Receipt.ManifestSha256, result.Value.DpoManifestSha256);
                Assert.Equal(1, result.Value.ReopenedDpoPairCount);
                Assert.Equal(1, result.Value.Training.DpoStepCount);
                Assert.Null(observedOptimizerStateToRestore);
                Assert.Equal(
                    "Explicit initial-training optimizer cold start for root lifecycle.",
                    result.Value.Training.OptimizerStateColdStartReason);
                Assert.Equal(
                    result.Value.Training.OptimizerStateColdStartReason,
                    result.Value.OptimizerStateColdStartReason);
                Assert.Equal(
                    childAddress.WeightsPath,
                    result.Value.Publication.WeightsPath);
                Assert.Equal(
                    childAddress.ManifestPath,
                    result.Value.Publication.ManifestPath);
                Assert.EndsWith(
                    "training-artifacts/models/csharp-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000002/weights.bin",
                    result.Value.PublishedWeightsRelativePath,
                    StringComparison.Ordinal);
                Assert.True(File.Exists(result.Value.Publication.WeightsPath));
                Assert.True(File.Exists(result.Value.Publication.ManifestPath));
                Assert.Equal(
                    Path.Combine(
                        attemptAddress.AttemptRoot,
                        DpoTrainer.BuildPolicyCheckpointFileName(1)),
                    result.Value.Training.PolicyCheckpointPath);
                Assert.Equal(
                    Path.Combine(
                        attemptAddress.AttemptRoot,
                        DpoTrainer.BuildMetricsFileName(1)),
                    result.Value.Training.MetricsPath);
                Assert.Equal(
                    Path.Combine(
                        attemptAddress.AttemptRoot,
                        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                            .BaselineStateFileName),
                    result.Value.Training.BaselineAttemptStatePath);
                Assert.Equal(
                    ComputeFileSha256(result.Value.Training.BaselineAttemptStatePath),
                    result.Value.Training.BaselineAttemptStateSha256);
                Assert.True(result.Value.Training.BaselineAttemptStateByteLength > 0);
                Assert.True(File.Exists(result.Value.Training.PolicyCheckpointPath));
                Assert.Contains(
                    "code-documentation-dpo-dataset-reopened-from-repository",
                    result.Value.EvidenceRefs);
                Assert.NotNull(result.Value.QualityDecision);
                Assert.True(result.Value.QualityDecision!.Accepted);
                Assert.Equal(0d, result.Value.QualityDecision.BaselineMeanCorrectFraction);
                Assert.Equal(1d, result.Value.QualityDecision.TrialMeanCorrectFraction);
                Assert.Equal(1d, result.Value.QualityDecision.Difference);
                Assert.Equal(
                    result.Value.QualityDecision.ExpectedEvaluationCallCount,
                    result.Value.QualityDecision.ObservedEvaluationCallCount);
                Assert.NotNull(qualityClient);
                Assert.Equal(50, qualityClient!.IssuedCallCount);
                Assert.Equal(50, qualityClient.CompletedResponseCount);
                Assert.Equal(50, qualityClient.Observations.Count);
                Assert.Equal(
                    50,
                    qualityClient.Observations
                        .Select(static item => item.RequestId)
                        .Distinct(StringComparer.Ordinal)
                        .Count());
                Assert.Equal(
                    0,
                    qualityClient.Observations
                        .Take(25)
                        .Select(static item => item.RequestId)
                        .Intersect(
                            qualityClient.Observations
                                .Skip(25)
                                .Select(static item => item.RequestId),
                            StringComparer.Ordinal)
                        .Count());

                Assert.Equal(
                    attemptAddress.AttemptRoot,
                    observedLogicalCheckpointDirectory);
                Assert.NotNull(observedBaselineBeforeTrainer);
                var baselineSnapshot = observedBaselineBeforeTrainer!;
                Assert.Equal(
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                        .CurrentFormatVersion,
                    baselineSnapshot.FormatVersion);
                Assert.Equal(trainingRunId, baselineSnapshot.TrainingRunId);
                Assert.Equal(
                    attemptAddress.TrainingRunIdSha256,
                    baselineSnapshot.TrainingRunIdSha256);
                Assert.Equal(
                    attemptAddress.AttemptRootRelativePath,
                    baselineSnapshot.AttemptRootRelativePath);
                Assert.Equal(
                    attemptAddress.AttemptClaimRelativePath,
                    baselineSnapshot.AttemptClaimRelativePath);
                Assert.Equal(
                    parentManifest.ManifestSha256,
                    baselineSnapshot.AcceptedManifestSha256);
                Assert.Equal(
                    parentManifest.WeightsSha256,
                    baselineSnapshot.AcceptedWeightsSha256);
                Assert.Equal(
                    parentManifest.WeightsByteLength,
                    baselineSnapshot.AcceptedWeightsByteLength);
                Assert.Equal(
                    dpoSetup.Address.ManifestRelativePath,
                    baselineSnapshot.DpoManifestRelativePath);
                Assert.Equal(
                    dpoSetup.Receipt.ManifestSha256,
                    baselineSnapshot.DpoManifestSha256);
                Assert.Equal(
                    dpoSetup.Receipt.OrderedPairs[0].PairId,
                    baselineSnapshot.TrainingPairId);
                Assert.Equal(
                    dpoSetup.Receipt.OrderedPairs[0].RepositoryRelativePairFilePath,
                    baselineSnapshot.TrainingPairRelativePath);
                Assert.Equal(
                    dpoSetup.Receipt.OrderedPairs[0].PairFileSha256,
                    baselineSnapshot.TrainingPairSha256);
                Assert.Equal(25, baselineSnapshot.ExpectedEvaluationCallCount);
                Assert.Equal(25, baselineSnapshot.ObservedEvaluationCallCount);
                Assert.Equal(5, baselineSnapshot.CalibrationObservationCount);
                Assert.Equal(5, baselineSnapshot.HeldOutCaseCount);
                Assert.Equal(0d, baselineSnapshot.BaselineMeanCorrectFraction);
                if (OperatingSystem.IsLinux())
                {
                    Assert.True(observedPreTrainerRootExists);
                    Assert.True(observedPreTrainerRootOwnerOnly);
                    Assert.True(SecureLocalFileBoundary
                        .HasOwnerOnlyDirectoryMode(attemptAddress.AttemptRoot));
                }

                var nextStart =
                    CodeDocumentationRepositoryOwnedModelCheckpointStartService
                        .PrepareStart(request);
                Assert.True(nextStart.IsSuccess, nextStart.ErrorMessage);
                Assert.Equal(2, nextStart.Value!.ParentManifest!.Step);
                Assert.Null(nextStart.Value.ParentManifest.DpoApplication);
                var childBytes = File.ReadAllBytes(childAddress.ManifestPath);
                var childManifest = nextStart.Value.ParentManifest;
                var forgedInitial = Encoding.UTF8.GetString(childBytes)
                    .Replace(RepositoryOwnedModelCheckpointManifestSealer.CurrentSchema, RepositoryOwnedModelCheckpointManifestSealer.InitialBaselineSchema, StringComparison.Ordinal)
                    .Replace("\"runtimeCompatibility\":", "\"dpoApplication\":1,\"runtimeCompatibility\":", StringComparison.Ordinal);
                var childSignature = "\"manifestSha256\":\"" + childManifest.ManifestSha256 + "\"";
                var unsignedChild = forgedInitial.Replace(childSignature, "\"manifestSha256\":\"\"", StringComparison.Ordinal);
                forgedInitial = forgedInitial.Replace(childSignature, "\"manifestSha256\":\"" + ComputeSha256(Encoding.UTF8.GetBytes(unsignedChild)) + "\"", StringComparison.Ordinal);
                File.WriteAllText(childAddress.ManifestPath, forgedInitial, new UTF8Encoding(false));
                Assert.False(RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(childAddress).IsSuccess);
                Assert.False(CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(request).IsSuccess);
                File.WriteAllBytes(childAddress.ManifestPath, childBytes);
                var callsBeforeReplay = qualityClient.IssuedCallCount;
                var replay = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(
                    new(routingRun, dpoSetup.Address, request, tokenizer, CreateLoraConfig(), CreateAcceptingBroker(),
                        "code-doc-lifecycle:new-name-replay", 1, DateTimeOffset.UnixEpoch.AddDays(2), ["test:replay"], qualityContext,
                        OptimizerStateColdStartAuthorization: ExplicitOptimizerColdStartAuthorization("root lifecycle")));
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PairAlreadyAppliedErrorCode, replay.ErrorCode);
                Assert.Equal(callsBeforeReplay, qualityClient.IssuedCallCount);
                Assert.False(Directory.Exists(CreateModelCheckpointAddress(repositoryRoot, 3).CheckpointRoot));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_RejectsCrossSeriesDuplicateRequestIdsAndRetainsParent()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-cross-series-duplicate");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-cross-series-duplicate");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-cross-series-parent",
                datasetVersion: "v0-parent");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-cross-series-new",
                datasetVersion: "v1-new");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent for cross-series duplicate IDs");
            var parentAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 1);
            var childAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 2);
            var trainingRunId = "code-doc-cross-series:duplicate-request-ids";
            var attemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var routingRun = CreateRoutingRun(dpoSetup.Receipt, dpoSetup.Pairs);
            var parentWeightsSha256 = ComputeFileSha256(parentAddress.WeightsPath);
            var parentManifestSha256 = ComputeFileSha256(parentAddress.ManifestPath);

            ScriptedQualityGateEvaluationClient? qualityClient = null;
            var qualityContext = CreateQualityGateContext(
                qualityCorpus,
                tokenizer,
                CreateQualityOutcomes(
                    baselinePass: false,
                    trialPass: true),
                requestIdFactory: callNumber =>
                    "quality-gate-cross-series-" +
                    (((callNumber - 1) % 25) + 1).ToString("D4"),
                captureClient: client => qualityClient = client);

            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .TrainAndPublishAsync(
                    new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                        routingRun,
                        dpoSetup.Address,
                        request,
                        tokenizer,
                        CreateLoraConfig(),
                        CreateAcceptingBroker(),
                        trainingRunId,
                        DpoStepCount: 1,
                        DateTimeOffset.UnixEpoch.AddDays(1),
                        ["test:code-documentation-cross-series-duplicate-ids"],
                        qualityContext,
                        OptimizerStateColdStartAuthorization:
                            ExplicitOptimizerColdStartAuthorization(
                                "cross-series duplicate request IDs")))
                .WaitAsync(TimeSpan.FromSeconds(30));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                    .QualityGateConditionMismatchErrorCode,
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.NotNull(qualityClient);
            Assert.Equal(50, qualityClient!.IssuedCallCount);
            Assert.Equal(50, qualityClient.CompletedResponseCount);
            Assert.Equal(50, qualityClient.Observations.Count);

            var baselineRequestIds = qualityClient.Observations
                .Take(25)
                .Select(static observation => observation.RequestId)
                .ToArray();
            var trialRequestIds = qualityClient.Observations
                .Skip(25)
                .Select(static observation => observation.RequestId)
                .ToArray();
            Assert.Equal(
                25,
                baselineRequestIds.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(
                25,
                trialRequestIds.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(
                25,
                baselineRequestIds
                    .Intersect(trialRequestIds, StringComparer.Ordinal)
                    .Count());
            Assert.Equal(
                25,
                baselineRequestIds
                    .Concat(trialRequestIds)
                    .Distinct(StringComparer.Ordinal)
                    .Count());

            Assert.True(Directory.Exists(attemptAddress.AttemptRoot));
            Assert.True(File.Exists(Path.Combine(
                attemptAddress.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1))));
            Assert.False(Directory.Exists(childAddress.CheckpointRoot));
            Assert.Equal(
                parentWeightsSha256,
                ComputeFileSha256(parentAddress.WeightsPath));
            Assert.Equal(
                parentManifestSha256,
                ComputeFileSha256(parentAddress.ManifestPath));

            var retainedStart =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(retainedStart.IsSuccess, retainedStart.ErrorMessage);
            Assert.Equal(1, retainedStart.Value!.ParentManifest!.Step);
            Assert.Equal(
                parentManifest.ManifestSha256,
                retainedStart.Value.ParentManifest.ManifestSha256);
            Assert.Equal(
                parentManifest.WeightsSha256,
                retainedStart.Value.ParentManifest.WeightsSha256);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task ServerOwnedExecutor_ManifestPreparationFailureRollsBackPublishedArtifacts()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-manifest-rollback");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-manifest-rollback");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-manifest-rollback-parent",
                datasetVersion: "v0-parent");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-manifest-rollback",
                datasetVersion: "v1-new");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent before manifest rollback");
            var parentAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 1);
            var acceptedParentWeightsSha256 =
                ComputeFileSha256(parentAddress.WeightsPath);
            var acceptedParentManifestSha256 =
                ComputeFileSha256(parentAddress.ManifestPath);
            Assert.Equal(
                parentManifest.WeightsSha256,
                acceptedParentWeightsSha256);
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var trainingRunId = "code-doc-lifecycle:manifest-rollback";
            var attemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);
            var acceptedPassport = CreateAcceptedPassportForTest(
                parentManifest,
                request.MaximumCheckpointBytes);
            var baselineQuality =
                await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService()
                    .CalibrateBaselineAsync(
                        CreateQualityGateContext(
                            qualityCorpus,
                            tokenizer,
                            CreateQualityOutcomes(
                                baselinePass: false,
                                trialPass: true)),
                        parentSession.Model,
                        acceptedPassport,
                        trainingRunId + ":baseline",
                        CancellationToken.None)
                    .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(baselineQuality.IsSuccess, baselineQuality.ErrorMessage);
            Assert.NotNull(baselineQuality.Value);
            var baselineState = CreateBaselineAttemptStateForTest(
                attemptAddress,
                acceptedPassport,
                dpoSetup,
                baselineQuality.Value!);
            var manifestPath = Path.Combine(
                attemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .ManifestFileName);
            var preexistingManifestBytes = new byte[] { 0x7f };
            var policyModel = CloneModel(parentSession.Model);
            var referenceModel = CloneModel(parentSession.Model);
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            OperationResult<CodeDocumentationServerOwnedDpoTrainingEvidence> failed;

            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                            manifestPath,
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.None,
                            FileOptions.WriteThrough);
                        stream.Write(
                            preexistingManifestBytes,
                            0,
                            preexistingManifestBytes.Length);
                        stream.Flush(flushToDisk: true);
                    };

                failed = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                    .ExecuteAsync(
                        new CodeDocumentationServerOwnedDpoTrainingRequest(
                            dpoSetup.Pairs,
                            policyModel,
                            referenceModel,
                            tokenizer,
                            CreateLoraConfig(),
                            attemptAddress,
                            CreateAcceptingBroker(),
                            trainingRunId,
                            DpoStepCount: 1,
                            baselineState,
                            OptimizerStateToRestore: null,
                            OptimizerStateColdStartAuthorization:
                                ExplicitOptimizerColdStartAuthorization(
                                    "manifest rollback")))
                    .WaitAsync(TimeSpan.FromSeconds(20));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }

            Assert.False(failed.IsSuccess);
            Assert.Equal("DPO_EXECUTION_FAILED", failed.ErrorCode);
            Assert.Equal(
                acceptedParentWeightsSha256,
                ComputeFileSha256(parentAddress.WeightsPath));
            Assert.Equal(
                acceptedParentManifestSha256,
                ComputeFileSha256(parentAddress.ManifestPath));
            Assert.True(File.Exists(Path.Combine(
                attemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .BaselineStateFileName)));
            Assert.False(File.Exists(Path.Combine(
                attemptAddress.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1))));
            Assert.False(File.Exists(Path.Combine(
                attemptAddress.AttemptRoot,
                DpoTrainer.BuildMetricsFileName(1))));
            Assert.False(File.Exists(Path.Combine(
                attemptAddress.AttemptRoot,
                RepositoryOwnedDpoOptimizerStateFile.BuildFileName(1))));
            // This file predates the manifest-preparation transaction. Cleanup
            // must not delete it merely because it has the expected leaf name.
            Assert.True(File.Exists(manifestPath));
            Assert.Equal(
                ComputeSha256(preexistingManifestBytes),
                ComputeFileSha256(manifestPath));

            var reopenedAcceptedParent =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(
                reopenedAcceptedParent.IsSuccess,
                reopenedAcceptedParent.ErrorMessage);
            Assert.Equal(
                parentManifest.Step,
                reopenedAcceptedParent.Value!.ParentManifest!.Step);
            Assert.Equal(
                parentManifest.WeightsSha256,
                reopenedAcceptedParent.Value.ParentManifest.WeightsSha256);
            Assert.Equal(
                acceptedParentWeightsSha256,
                ComputeCheckpointSha256(reopenedAcceptedParent.Value.Model));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task ServerOwnedExecutor_MultiStepFailurePreservesPreExistingArtifactsAndCleansCreatedArtifacts()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-transaction-ownership");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-transaction-ownership");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-transaction-ownership-parent",
                datasetVersion: "v0-parent");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-transaction-ownership",
                datasetVersion: "v1-new");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent before transaction ownership");
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var trainingRunId = "code-doc-lifecycle:transaction-ownership";
            var attemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);
            var acceptedPassport = CreateAcceptedPassportForTest(
                parentManifest,
                request.MaximumCheckpointBytes);
            var baselineQuality =
                await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService()
                    .CalibrateBaselineAsync(
                        CreateQualityGateContext(
                            qualityCorpus,
                            tokenizer,
                            CreateQualityOutcomes(
                                baselinePass: false,
                                trialPass: true)),
                        parentSession.Model,
                        acceptedPassport,
                        trainingRunId + ":baseline",
                        CancellationToken.None)
                    .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(baselineQuality.IsSuccess, baselineQuality.ErrorMessage);
            Assert.NotNull(baselineQuality.Value);
            var baselineState = CreateBaselineAttemptStateForTest(
                attemptAddress,
                acceptedPassport,
                dpoSetup,
                baselineQuality.Value!);

            var preexistingPolicyBytes = new byte[] { 0x11, 0x22 };
            var preexistingMetricsBytes = new byte[] { 0x33, 0x44 };
            var preexistingOptimizerBytes = new byte[] { 0x55, 0x66 };
            var preexistingManifestBytes = new byte[] { 0x77, 0x88 };
            var preexistingPolicyPath = Path.Combine(
                attemptAddress.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(2));
            var preexistingMetricsPath = Path.Combine(
                attemptAddress.AttemptRoot,
                DpoTrainer.BuildMetricsFileName(2));
            var preexistingOptimizerPath = Path.Combine(
                attemptAddress.AttemptRoot,
                RepositoryOwnedDpoOptimizerStateFile.BuildFileName(2));
            var preexistingManifestPath = Path.Combine(
                attemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .ManifestFileName);

            static void WriteOwnerOnlyBytes(string path, byte[] bytes)
            {
                using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                    path,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    FileOptions.WriteThrough);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(flushToDisk: true);
            }

            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        WriteOwnerOnlyBytes(
                            preexistingPolicyPath,
                            preexistingPolicyBytes);
                        WriteOwnerOnlyBytes(
                            preexistingMetricsPath,
                            preexistingMetricsBytes);
                        WriteOwnerOnlyBytes(
                            preexistingOptimizerPath,
                            preexistingOptimizerBytes);
                        WriteOwnerOnlyBytes(
                            preexistingManifestPath,
                            preexistingManifestBytes);
                    };

                var failed = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                    .ExecuteAsync(
                        new CodeDocumentationServerOwnedDpoTrainingRequest(
                            dpoSetup.Pairs,
                            parentSession.Model,
                            CloneModel(parentSession.Model),
                            tokenizer,
                            CreateLoraConfig(),
                            attemptAddress,
                            CreateAcceptingBroker(),
                            trainingRunId,
                            DpoStepCount: 2,
                            baselineState,
                            OptimizerStateToRestore: null,
                            OptimizerStateColdStartAuthorization:
                                ExplicitOptimizerColdStartAuthorization(
                                    "transaction ownership")))
                    .WaitAsync(TimeSpan.FromSeconds(20));

                Assert.False(failed.IsSuccess);
                Assert.Equal("DPO_EXECUTION_FAILED", failed.ErrorCode);

                Assert.False(File.Exists(Path.Combine(
                    attemptAddress.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1))));
                Assert.False(File.Exists(Path.Combine(
                    attemptAddress.AttemptRoot,
                    DpoTrainer.BuildMetricsFileName(1))));
                Assert.False(File.Exists(Path.Combine(
                    attemptAddress.AttemptRoot,
                    RepositoryOwnedDpoOptimizerStateFile.BuildFileName(1))));

                Assert.Equal(
                    ComputeSha256(preexistingPolicyBytes),
                    ComputeFileSha256(preexistingPolicyPath));
                Assert.Equal(
                    ComputeSha256(preexistingMetricsBytes),
                    ComputeFileSha256(preexistingMetricsPath));
                Assert.Equal(
                    ComputeSha256(preexistingOptimizerBytes),
                    ComputeFileSha256(preexistingOptimizerPath));
                Assert.Equal(
                    ComputeSha256(preexistingManifestBytes),
                    ComputeFileSha256(preexistingManifestPath));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task ServerOwnedExecutor_ForeignBaselineAttemptStateStopsBeforeTraining()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-baseline-foreign-state");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-lifecycle-foreign-baseline-state");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-foreign-baseline-state-parent",
                datasetVersion: "v0-parent");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-foreign-baseline-state",
                datasetVersion: "v1-new");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                session.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent before foreign baseline state");
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var modelSha256Before = ComputeCheckpointSha256(session.Model);
            var trainingRunId = "code-doc-lifecycle:foreign-baseline-state";
            var attemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);
            var childAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 2);
            var acceptedPassport = CreateAcceptedPassportForTest(
                parentManifest,
                request.MaximumCheckpointBytes);
            var baselineQuality =
                await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService()
                    .CalibrateBaselineAsync(
                        CreateQualityGateContext(
                            qualityCorpus,
                            tokenizer,
                            CreateQualityOutcomes(
                                baselinePass: false,
                                trialPass: true)),
                        session.Model,
                        acceptedPassport,
                        trainingRunId + ":baseline",
                        CancellationToken.None)
                    .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(baselineQuality.IsSuccess, baselineQuality.ErrorMessage);
            Assert.NotNull(baselineQuality.Value);
            var correctBaselineState = CreateBaselineAttemptStateForTest(
                attemptAddress,
                acceptedPassport,
                dpoSetup,
                baselineQuality.Value!);
            var foreignBaselineState = correctBaselineState with
            {
                TrainingRunId = trainingRunId + ":foreign",
            };
            var trainerReached = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;

            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        trainerReached = true;
                    };

                var failed = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                    .ExecuteAsync(
                        new CodeDocumentationServerOwnedDpoTrainingRequest(
                            dpoSetup.Pairs,
                            session.Model,
                            CloneModel(session.Model),
                            tokenizer,
                            CreateLoraConfig(),
                            attemptAddress,
                            CreateAcceptingBroker(),
                            trainingRunId,
                            DpoStepCount: 1,
                            foreignBaselineState))
                    .WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(failed.IsSuccess);
                Assert.Equal(
                    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                        .InvalidErrorCode,
                    failed.ErrorCode);
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }

            Assert.False(trainerReached);
            Assert.Equal(modelSha256Before, ComputeCheckpointSha256(session.Model));
            Assert.True(Directory.Exists(attemptAddress.AttemptRoot));
            Assert.True(File.Exists(attemptAddress.AttemptClaimPath));
            Assert.False(File.Exists(Path.Combine(
                attemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .BaselineStateFileName)));
            Assert.False(File.Exists(Path.Combine(
                attemptAddress.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1))));
            Assert.False(File.Exists(childAddress.WeightsPath));
            Assert.False(File.Exists(childAddress.ManifestPath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_ClonesTrainedLoraParentAndPublishesSecondContinuation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-dpo-training-lora-parent");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-lifecycle-lora-parent");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-lora-parent-seed",
                datasetVersion: "v0-parent");
            var firstDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-lora-parent-first",
                datasetVersion: "v1");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var secondDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-lora-parent-second",
                datasetVersion: "v2",
                prompt:
                "Document how the accepted C# documentation model continues after a LoRA lesson.",
                chosen:
                "Open the accepted checkpoint, rebuild the LoRA slots, copy the trained weights, then run exactly the new pair.",
                rejected:
                "Create a fresh base model, ignore the saved LoRA weights, and train again from the first pair.");
            Assert.NotEqual(
                firstDpoSetup.Receipt.ManifestSha256,
                secondDpoSetup.Receipt.ManifestSha256);
            Assert.NotEqual(
                firstDpoSetup.Receipt.OrderedPairs[0].PairId,
                secondDpoSetup.Receipt.OrderedPairs[0].PairId);
            Assert.NotEqual(
                firstDpoSetup.Pairs[0].Prompt,
                secondDpoSetup.Pairs[0].Prompt);
            Assert.NotEqual(
                firstDpoSetup.Pairs[0].Chosen,
                secondDpoSetup.Pairs[0].Chosen);
            Assert.NotEqual(
                firstDpoSetup.Pairs[0].Rejected,
                secondDpoSetup.Pairs[0].Rejected);
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent before CodeDocumentation LoRA continuation");
            var parentCheckpointAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 1);
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var firstRoutingRun =
                CreateRoutingRun(firstDpoSetup.Receipt, firstDpoSetup.Pairs);
            var secondRoutingRun =
                CreateRoutingRun(secondDpoSetup.Receipt, secondDpoSetup.Pairs);
            var firstTrainingRunId = "code-doc-lifecycle:lora-parent-first";
            var firstAttemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                firstTrainingRunId);

            var first =
                await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                    .TrainAndPublishAsync(
                        new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                            firstRoutingRun,
                            firstDpoSetup.Address,
                            request,
                            tokenizer,
                            CreateLoraConfig(),
                            CreateAcceptingBroker(),
                            firstTrainingRunId,
                            DpoStepCount: 1,
                            DateTimeOffset.UnixEpoch.AddDays(4),
                            ["test:code-documentation-dpo-lora-parent-first"],
                             CreateQualityGateContext(
                                 qualityCorpus,
                                 tokenizer,
                                 CreateQualityOutcomes(
                                     baselinePass: false,
                                     trialPass: true)),
                             OptimizerStateColdStartAuthorization:
                                 ExplicitOptimizerColdStartAuthorization(
                                     "root lora lifecycle")))
                    .WaitAsync(TimeSpan.FromSeconds(30));

            Assert.True(first.IsSuccess,
                $"{first.ErrorCode}: {first.ErrorMessage}");
            Assert.Equal(1, first.Value!.ParentCheckpointStep);
            Assert.Equal(2, first.Value.PublishedCheckpointStep);
            Assert.NotEqual(
                parentManifest.WeightsSha256,
                first.Value.Publication.WeightsSha256);
            Assert.Equal(
                Path.Combine(
                    firstAttemptAddress.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1)),
                first.Value.Training.PolicyCheckpointPath);

            var reloadedTrainedParent =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(
                reloadedTrainedParent.IsSuccess,
                reloadedTrainedParent.ErrorMessage);
            Assert.Equal(2, reloadedTrainedParent.Value!.ParentManifest!.Step);
            Assert.Equal(
                first.Value.Publication.WeightsSha256,
                reloadedTrainedParent.Value.ParentManifest.WeightsSha256);
            Assert.True(
                AnyLoraBMatrixContainsNonZeroValues(
                    reloadedTrainedParent.Value.Model),
                "Первый урок должен оставить ненулевые значения в LoRA-B-матрицах.");
            var baseOnlyCloneFailure = Record.Exception(
                () => LoadFullCheckpointIntoBaseOnlyClone(
                    reloadedTrainedParent.Value.Model));
            Assert.NotNull(baseOnlyCloneFailure);
            Assert.True(
                LlmModel.IsCheckpointParameterCountMismatch(
                    baseOnlyCloneFailure),
                baseOnlyCloneFailure.Message);

            var sourceOptimizerStatePath = Path.Combine(
                firstAttemptAddress.AttemptRoot,
                RepositoryOwnedDpoOptimizerStateFile.BuildFileName(1));
            var sourceOptimizerManifestPath = Path.Combine(
                firstAttemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .ManifestFileName);
            var sourceOptimizerStateBytes =
                File.ReadAllBytes(sourceOptimizerStatePath);
            var sourceOptimizerManifestText =
                File.ReadAllText(sourceOptimizerManifestPath);
            var sourceOptimizerState = Assert.IsType<AdamWPortableState>(
                AdamWPortableState.FromBytes(sourceOptimizerStateBytes));
            var openedSourceManifest = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                .OpenExisting(firstAttemptAddress);
            Assert.True(openedSourceManifest.IsSuccess, openedSourceManifest.ErrorMessage);
            var sourceIdentity = openedSourceManifest.Value!.Manifest;
            var openedSourceOptimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                firstAttemptAddress.AttemptRoot, sourceIdentity.OptimizerStateStep, sourceIdentity.OptimizerStateSha256);
            Assert.True(openedSourceOptimizer.IsSuccess, openedSourceOptimizer.ErrorMessage);
            var sourceRuntimeMatch = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.MatchesRuntime(
                sourceIdentity, firstAttemptAddress, fixture.ModelConfig,
                reloadedTrainedParent.Value.Model.GetCheckpointParameterEntries()
                    .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter)).ToArray(),
                DpoTrainer.ProductionOptimizerSettings, openedSourceOptimizer.Value!.State);
            Assert.True(sourceRuntimeMatch.IsSuccess, sourceRuntimeMatch.ErrorMessage);
            Assert.Equal(first.Value.Publication.WeightsSha256, sourceIdentity.TrialWeightsSha256);
            Assert.Equal(new FileInfo(first.Value.Publication.WeightsPath).Length, sourceIdentity.TrialWeightsByteLength);
            Assert.Equal(sourceIdentity.TrialWeightsSha256, ComputeFileSha256(first.Value.Training.PolicyCheckpointPath));

            async Task AssertInvalidContinuationAsync(
                string scenario,
                Action mutateSource)
            {
                File.WriteAllBytes(
                    sourceOptimizerStatePath,
                    sourceOptimizerStateBytes);
                File.WriteAllText(
                    sourceOptimizerManifestPath,
                    sourceOptimizerManifestText);
                mutateSource();

                var invalidRunId =
                    "code-doc-lifecycle:lora-parent-invalid-" + scenario;
                var invalidAttempt = CreateModelTrainingAttemptAddress(
                    repositoryRoot,
                    invalidRunId);
                var invalidQuality = CreateQualityGateContext(qualityCorpus, tokenizer,
                    CreateQualityOutcomes(baselinePass: false, trialPass: true));
                var invalidEvaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(invalidQuality.EvaluationClient);
                var retainedSource = Directory.GetFiles(repositoryRoot, "*", SearchOption.AllDirectories)
                    .ToDictionary(path => path, ComputeFileSha256);
                var acceptedAddress = CreateModelCheckpointAddress(repositoryRoot, 2);
                var acceptedManifestHash = ComputeFileSha256(acceptedAddress.ManifestPath);
                var acceptedWeightsHash = ComputeFileSha256(acceptedAddress.WeightsPath);
                var trainerCalls = 0;
                var previousInvalidObserver = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
                OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence> invalid;
                try
                {
                    StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => trainerCalls++;
                    invalid =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(
                            new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                                secondRoutingRun,
                                secondDpoSetup.Address,
                                request,
                                tokenizer,
                                CreateLoraConfig(),
                                CreateAcceptingBroker(),
                                invalidRunId,
                                DpoStepCount: 1,
                                DateTimeOffset.UnixEpoch.AddDays(5),
                                ["test:invalid-optimizer-" + scenario],
                                invalidQuality,
                                OptimizerStateSourceAttempt:
                                    firstAttemptAddress))
                        .WaitAsync(TimeSpan.FromSeconds(30));
                }
                finally
                {
                    StableDpoTrainingExecutor.LogicalRequestObserverForTesting = previousInvalidObserver;
                }

                Assert.False(invalid.IsSuccess, scenario);
                if (scenario == "trial-sha-only")
                {
                    Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OptimizerStateRestoreRequiredErrorCode,
                        invalid.ErrorCode);
                    Assert.Equal("The optimizer-state source policy checkpoint is not the accepted parent for the requested continuation.",
                        invalid.ErrorMessage);
                    var heldOut = qualityCorpus.Corpus.Entries
                        .Where(entry => entry.Partition == FrozenReferencePartition.HeldOut)
                        .OrderBy(entry => entry.EntryId, StringComparer.Ordinal).Select(entry => entry.EntryId).ToArray();
                    Assert.Equal(5, heldOut.Length);
                    Assert.Equal(5, MeasurementNoisePolicyConfig.Canonical.RepeatCount);
                    Assert.Equal(25, invalidEvaluator.Observations.Count);
                    Assert.Equal(Enumerable.Range(0, 5).SelectMany(_ => heldOut),
                        invalidEvaluator.Observations.Select(observation => observation.CaseId));
                    Assert.Equal(0, trainerCalls);
                    // The 25 observations exhaust exactly baseline, never a trial series.
                    Assert.False(Directory.Exists(invalidAttempt.AttemptRoot));
                    Assert.False(Directory.Exists(CreateModelCheckpointAddress(repositoryRoot, 3).CheckpointRoot));
                    Assert.Equal(acceptedManifestHash, ComputeFileSha256(acceptedAddress.ManifestPath));
                    Assert.Equal(acceptedWeightsHash, ComputeFileSha256(acceptedAddress.WeightsPath));
                    Assert.Equal(retainedSource.Keys.Order(), Directory.GetFiles(repositoryRoot, "*", SearchOption.AllDirectories).Order());
                    foreach (var file in retainedSource) Assert.Equal(file.Value, ComputeFileSha256(file.Key));
                    var accepted = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(request);
                    Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
                    Assert.Equal(2, accepted.Value!.ParentManifest!.Step);
                    Assert.Equal(acceptedWeightsHash, ComputeCheckpointSha256(accepted.Value.Model));
                }
                Assert.False(File.Exists(Path.Combine(
                    invalidAttempt.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1))));
                Assert.False(File.Exists(Path.Combine(
                    invalidAttempt.AttemptRoot,
                    DpoTrainer.BuildMetricsFileName(1))));
                Assert.False(File.Exists(Path.Combine(
                    invalidAttempt.AttemptRoot,
                    RepositoryOwnedDpoOptimizerStateFile.BuildFileName(1))));
            }

            await AssertInvalidContinuationAsync(
                "trial-sha-only",
                () => MutateOptimizerStateManifest(sourceOptimizerManifestPath,
                    root => root["TrialWeightsSha256"] = sourceIdentity.TrialWeightsSha256 == new string('a', 64)
                        ? new string('b', 64) : new string('a', 64)));
            await AssertInvalidContinuationAsync(
                "wrong-hash",
                () => MutateOptimizerStateManifest(
                    sourceOptimizerManifestPath,
                    root => root["OptimizerStateSha256"] = new string('0', 64)));
            await AssertInvalidContinuationAsync(
                "wrong-length",
                () => MutateOptimizerStateManifest(
                    sourceOptimizerManifestPath,
                    root => root["OptimizerStateByteLength"] =
                        sourceOptimizerStateBytes.LongLength + 1));
            await AssertInvalidContinuationAsync(
                "wrong-step",
                () => MutateOptimizerStateManifest(
                    sourceOptimizerManifestPath,
                    root => root["OptimizerStateStep"] =
                        sourceOptimizerState!.Step + 1));
            await AssertInvalidContinuationAsync(
                "wrong-shape",
                () =>
                {
                    var entry = sourceOptimizerState!.Entries[0];
                    var wrongShape = new AdamWPortableState(
                        sourceOptimizerState.FormatVersion,
                        sourceOptimizerState.Step,
                        [
                            new AdamWPortableParameterState(
                                entry.Key,
                                [entry.FirstMoment.Count + 1],
                                new float[entry.FirstMoment.Count + 1],
                                new float[entry.SecondMoment.Count + 1])
                        ]);
                    var wrongShapeBytes = wrongShape.ToBytes();
                    File.WriteAllBytes(
                        sourceOptimizerStatePath,
                        wrongShapeBytes);
                    MutateOptimizerStateManifest(
                        sourceOptimizerManifestPath,
                        root =>
                        {
                            root["OptimizerStateSha256"] =
                                ComputeSha256(wrongShapeBytes);
                            root["OptimizerStateByteLength"] =
                                wrongShapeBytes.LongLength;
                        });
                });

            File.WriteAllBytes(sourceOptimizerStatePath, sourceOptimizerStateBytes);
            File.WriteAllText(sourceOptimizerManifestPath, sourceOptimizerManifestText);

            var expectedSecondChild = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 3);
            var secondTrainingRunId = "code-doc-lifecycle:lora-parent-second";
            var expectedSecondAttempt = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                secondTrainingRunId);
            string? observedSecondLogicalCheckpointDirectory = null;
            string? observedSecondPolicyBeforeOptimizerSha256 = null;
            string? observedSecondReferenceBeforeOptimizerSha256 = null;
            bool? observedSecondModelsAreSeparate = null;
            bool? observedSecondPolicyLoraBValuesAreNonZero = null;
            bool? observedSecondReferenceLoraBValuesAreNonZero = null;
            LlmModel? observedSecondReferenceModel = null;
            AdamWPortableState? observedSecondOptimizerStateToRestore = null;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;

            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        observedSecondLogicalCheckpointDirectory =
                            logicalRequest.CheckpointDirectory;
                        observedSecondModelsAreSeparate =
                            !ReferenceEquals(
                                logicalRequest.PolicyModel,
                                logicalRequest.ReferenceModel);
                        observedSecondPolicyBeforeOptimizerSha256 =
                            ComputeCheckpointSha256(logicalRequest.PolicyModel);
                        observedSecondReferenceBeforeOptimizerSha256 =
                            ComputeCheckpointSha256(logicalRequest.ReferenceModel);
                        observedSecondPolicyLoraBValuesAreNonZero =
                            AnyLoraBMatrixContainsNonZeroValues(
                                logicalRequest.PolicyModel);
                        observedSecondReferenceLoraBValuesAreNonZero =
                            AnyLoraBMatrixContainsNonZeroValues(
                                logicalRequest.ReferenceModel);
                        observedSecondReferenceModel =
                            logicalRequest.ReferenceModel;
                        observedSecondOptimizerStateToRestore =
                            logicalRequest.OptimizerStateToRestore;
                    };

                var second =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(
                            new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                                secondRoutingRun,
                                secondDpoSetup.Address,
                                request,
                                tokenizer,
                                CreateLoraConfig(),
                                CreateAcceptingBroker(),
                                secondTrainingRunId,
                            DpoStepCount: 1,
                            DateTimeOffset.UnixEpoch.AddDays(5),
                            ["test:code-documentation-dpo-lora-parent-second"],
                            CreateQualityGateContext(
                                qualityCorpus,
                                tokenizer,
                                CreateQualityOutcomes(
                                    baselinePass: false,
                                    trialPass: true)),
                            OptimizerStateSourceAttempt: firstAttemptAddress))
                        .WaitAsync(TimeSpan.FromSeconds(30));

                Assert.True(second.IsSuccess,
                    $"{second.ErrorCode}: {second.ErrorMessage}");
                Assert.Equal(2, second.Value!.ParentCheckpointStep);
                Assert.Equal(3, second.Value.PublishedCheckpointStep);
                Assert.Equal(1, second.Value.Training.ProcessedPairCount);
                var restoredSecondOptimizerState = Assert.IsType<
                    AdamWPortableState>(observedSecondOptimizerStateToRestore);
                Assert.Equal(
                    first.Value.Training.OptimizerStateStep,
                    restoredSecondOptimizerState.Step);
                Assert.Equal(
                    first.Value.Training.OptimizerStateSha256,
                    ComputeSha256(restoredSecondOptimizerState.ToBytes()));
                Assert.True(
                    observedSecondModelsAreSeparate is true,
                    "Второй DPO-шаг должен получать отдельные объекты обучаемой и неизменяемой моделей.");
                Assert.Equal(
                    first.Value.Publication.WeightsSha256,
                    observedSecondPolicyBeforeOptimizerSha256);
                Assert.Equal(
                    first.Value.Publication.WeightsSha256,
                    observedSecondReferenceBeforeOptimizerSha256);
                Assert.True(
                    observedSecondPolicyLoraBValuesAreNonZero is true,
                    "Перед вторым обновлением весов обучаемая модель должна сохранить обученные LoRA-B-значения первого урока.");
                Assert.True(
                    observedSecondReferenceLoraBValuesAreNonZero is true,
                    "Неизменяемая копия перед вторым обновлением весов должна сохранить обученные LoRA-B-значения первого урока.");
                Assert.Equal(
                    expectedSecondAttempt.AttemptRoot,
                    observedSecondLogicalCheckpointDirectory);
                Assert.Equal(
                    expectedSecondChild.WeightsPath,
                    second.Value.Publication.WeightsPath);
                Assert.Equal(
                    expectedSecondChild.ManifestPath,
                    second.Value.Publication.ManifestPath);
                Assert.Equal(
                    Path.Combine(
                        expectedSecondAttempt.AttemptRoot,
                        DpoTrainer.BuildPolicyCheckpointFileName(1)),
                    second.Value.Training.PolicyCheckpointPath);
                Assert.True(File.Exists(second.Value.Publication.WeightsPath));
                Assert.True(File.Exists(second.Value.Publication.ManifestPath));
                Assert.True(File.Exists(second.Value.Training.PolicyCheckpointPath));
                var secondOptimizerManifest =
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                        .OpenExisting(expectedSecondAttempt);
                Assert.True(
                    secondOptimizerManifest.IsSuccess &&
                    secondOptimizerManifest.Value is not null,
                    secondOptimizerManifest.ErrorMessage);
                Assert.Equal(
                    second.Value.Training.DpoStepCount,
                    secondOptimizerManifest.Value!.Manifest.DpoStepCount);
                Assert.Equal(
                    second.Value.Training.OptimizerStateStep,
                    secondOptimizerManifest.Value.Manifest.OptimizerStateStep);
                Assert.Equal(
                    second.Value.Training.OptimizerStateSha256,
                    secondOptimizerManifest.Value.Manifest.OptimizerStateSha256);
                Assert.Equal(
                    second.Value.Training.OptimizerStateByteLength,
                    secondOptimizerManifest.Value.Manifest.OptimizerStateByteLength);
                Assert.Equal(
                    second.Value.Training.OptimizerStateManifestPath,
                    secondOptimizerManifest.Value.ManifestPath);
                Assert.Equal(
                    second.Value.Training.OptimizerStateManifestSha256,
                    secondOptimizerManifest.Value.ManifestSha256);
                Assert.NotEqual(
                    first.Value.Publication.WeightsSha256,
                    second.Value.Publication.WeightsSha256);
                Assert.NotNull(observedSecondReferenceModel);
                Assert.Equal(
                    first.Value.Publication.WeightsSha256,
                    ComputeCheckpointSha256(observedSecondReferenceModel!));
                Assert.Equal(
                    first.Value.Publication.WeightsSha256,
                    ComputeFileSha256(first.Value.Publication.WeightsPath));
                Assert.Equal(
                    parentManifest.WeightsSha256,
                    ComputeFileSha256(parentCheckpointAddress.WeightsPath));

                var afterSecond =
                    CodeDocumentationRepositoryOwnedModelCheckpointStartService
                        .PrepareStart(request);
                Assert.True(afterSecond.IsSuccess, afterSecond.ErrorMessage);
                Assert.Equal(3, afterSecond.Value!.ParentManifest!.Step);
                Assert.Equal(
                    second.Value.Publication.WeightsSha256,
                    afterSecond.Value.ParentManifest.WeightsSha256);
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_MemoryPairTamperFailsBeforeCheckpointPublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-dpo-training-tamper");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-lifecycle-tamper");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-tamper");
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            _ = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                dpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent for CodeDocumentation tamper lifecycle");
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var tamperedRoutingRun = CreateRoutingRun(
                dpoSetup.Receipt,
                [CreateRepositoryOwnedDpoPair(dpoSetup.RequestIdentity, "tampered-memory-pair")]);

            var result =
                await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                    .TrainAndPublishAsync(
                        new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                            tamperedRoutingRun,
                            dpoSetup.Address,
                            request,
                            tokenizer,
                            CreateLoraConfig(),
                            CreateAcceptingBroker(),
                            "code-doc-lifecycle:tamper",
                            DpoStepCount: 1,
                            DateTimeOffset.UnixEpoch.AddDays(2),
                            ["test:code-documentation-dpo-training-tamper"]))
                    .WaitAsync(TimeSpan.FromSeconds(10));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                    .DpoDatasetMismatchErrorCode,
                result.ErrorCode);
            var childAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    repositoryRoot,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .CSharpDomainSlug,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .DesignPatternSlug,
                    "code-documentation-llm",
                    "v1",
                    step: 2);
            Assert.True(childAddress.IsSuccess, childAddress.ErrorMessage);
            Assert.False(File.Exists(childAddress.Value!.WeightsPath));
            Assert.False(File.Exists(childAddress.Value.ManifestPath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task ServerOwnedExecutor_ExistingGroupReadableAttemptRootFailsClosedBeforeTrainer()
    {
        if (!OperatingSystem.IsLinux())
            return;

        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-training-unsafe-attempt-root");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var trainingRunId = "code-doc-lifecycle:unsafe-attempt-root";
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-unsafe-attempt-root");
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var address = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);
            Directory.CreateDirectory(address.AttemptRoot);
            File.SetUnixFileMode(
                address.AttemptRoot,
                SecureLocalFileBoundary.OwnerDirectoryMode
                | UnixFileMode.GroupRead
                | UnixFileMode.GroupExecute);
            var reachedTrainer = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;

            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        reachedTrainer = true;
                    };

                var result =
                    await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                        .ExecuteAsync(
                            new CodeDocumentationServerOwnedDpoTrainingRequest(
                                dpoSetup.Pairs,
                                session.Model,
                                CloneModel(session.Model),
                                tokenizer,
                                CreateLoraConfig(),
                                address,
                                CreateAcceptingBroker(),
                                trainingRunId,
                                DpoStepCount: 1))
                        .WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(result.IsSuccess);
                Assert.Equal(
                    RepositoryOwnedModelCheckpointManifestSealer
                        .UnavailableErrorCode,
                    result.ErrorCode);
                Assert.Contains(
                    "Repository-owned model training attempt storage is unavailable",
                    result.ErrorMessage,
                    StringComparison.Ordinal);
                Assert.False(reachedTrainer);
                Assert.False(File.Exists(address.AttemptClaimPath));
                Assert.Empty(Directory.EnumerateFiles(address.AttemptRoot));
                Assert.False(SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                    address.AttemptRoot));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_MalformedReceiptFailsBeforeCheckpointPublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-training-malformed-receipt");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-lifecycle-malformed-receipt");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-malformed-receipt");
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            _ = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                dpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason:
                    "test parent for malformed CodeDocumentation DPO receipt");
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var malformedReceipt =
                dpoSetup.Receipt with { OrderedPairs = null! };
            var malformedRoutingRun = CreateRoutingRun(
                malformedReceipt,
                dpoSetup.Pairs);

            var result =
                await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                    .TrainAndPublishAsync(
                        new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                            malformedRoutingRun,
                            dpoSetup.Address,
                            request,
                            tokenizer,
                            CreateLoraConfig(),
                            CreateAcceptingBroker(),
                            "code-doc-lifecycle:malformed-receipt",
                            DpoStepCount: 1,
                            DateTimeOffset.UnixEpoch.AddDays(3),
                            ["test:code-documentation-dpo-training-malformed-receipt"]))
                    .WaitAsync(TimeSpan.FromSeconds(10));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                    .InvalidErrorCode,
                result.ErrorCode);
            var childAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    repositoryRoot,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .CSharpDomainSlug,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .DesignPatternSlug,
                    "code-documentation-llm",
                    "v1",
                    step: 2);
            Assert.True(childAddress.IsSuccess, childAddress.ErrorMessage);
            Assert.False(File.Exists(childAddress.Value!.WeightsPath));
            Assert.False(File.Exists(childAddress.Value.ManifestPath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_FailedAttemptKeepsPreviousAcceptedCheckpointLoadableAndNumericIncompleteStillFails()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-training-failed-attempt-keeps-parent");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-lifecycle-failed-attempt-keeps-parent");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-failed-attempt-keeps-parent-seed",
                datasetVersion: "v0-parent");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-failed-attempt-keeps-parent-new",
                datasetVersion: "v1-new");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent before failed attempt");
            var parentAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 1);
            var parentWeightsSha256Before =
                ComputeFileSha256(parentAddress.WeightsPath);
            var parentManifestSha256Before =
                ComputeFileSha256(parentAddress.ManifestPath);
            var childAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 2);
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var routingRun = CreateRoutingRun(dpoSetup.Receipt, dpoSetup.Pairs);
            var trainingRunId = "code-doc-lifecycle:failing-attempt";
            var attemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            // The forced-failure path is the whole point of this case. Without a
            // direct observation that the injected trainer failure actually ran,
            // any earlier coordinator failure that happens to surface the same
            // TrainingFailedErrorCode would turn this test green while the
            // intended attempt-claim-then-trainer-failure path stayed unexercised.
            var syntheticTrainerFailureInvocationCount = 0;

            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        Interlocked.Increment(
                            ref syntheticTrainerFailureInvocationCount);
                        priorObserver?.Invoke(logicalRequest);
                        throw new InvalidOperationException(
                            "Synthetic failure after attempt claim.");
                    };

                var failed =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(
                            new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                                routingRun,
                                dpoSetup.Address,
                                request,
                                tokenizer,
                                CreateLoraConfig(),
                                CreateAcceptingBroker(),
                                trainingRunId,
                            DpoStepCount: 1,
                            DateTimeOffset.UnixEpoch.AddDays(6),
                            ["test:code-documentation-dpo-failed-attempt"],
                            CreateQualityGateContext(
                                qualityCorpus,
                                tokenizer,
                                CreateQualityOutcomes(
                                    baselinePass: false,
                                    trialPass: true)),
                            OptimizerStateColdStartAuthorization:
                                ExplicitOptimizerColdStartAuthorization(
                                    "root failed-attempt lifecycle")))
                        .WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(failed.IsSuccess);
                Assert.Equal(
                    CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainingFailedErrorCode,
                    failed.ErrorCode);
                // Direct observation: the deliberately failing trainer observer ran,
                // so the reported failure is the injected trainer failure and not an
                // earlier failure that merely shares the same error code.
                Assert.Equal(1, syntheticTrainerFailureInvocationCount);
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }

            Assert.True(Directory.Exists(attemptAddress.AttemptRoot));
            Assert.True(File.Exists(attemptAddress.AttemptClaimPath));
            Assert.False(File.Exists(childAddress.WeightsPath));
            Assert.False(File.Exists(childAddress.ManifestPath));
            Assert.Equal(
                parentWeightsSha256Before,
                ComputeFileSha256(parentAddress.WeightsPath));
            Assert.Equal(
                parentManifestSha256Before,
                ComputeFileSha256(parentAddress.ManifestPath));

            var reopenedAfterAttemptFailure =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(
                reopenedAfterAttemptFailure.IsSuccess,
                reopenedAfterAttemptFailure.ErrorMessage);
            Assert.Equal(
                parentManifest.Step,
                reopenedAfterAttemptFailure.Value!.ParentManifest!.Step);
            Assert.Equal(
                parentManifest.WeightsSha256,
                reopenedAfterAttemptFailure.Value.ParentManifest.WeightsSha256);
            Assert.Equal(
                parentWeightsSha256Before,
                ComputeCheckpointSha256(reopenedAfterAttemptFailure.Value.Model));

            SecureLocalFileBoundary.CreateOwnerOnlyDirectory(
                childAddress.CheckpointRoot);
            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                       childAddress.WeightsPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough))
            {
                stream.WriteByte(42);
                stream.Flush(flushToDisk: true);
            }

            var blockedByIncompleteNumericCheckpoint =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.False(blockedByIncompleteNumericCheckpoint.IsSuccess);
            Assert.Contains(
                "corrupt, incomplete, tampered, or unsafe checkpoint candidate",
                blockedByIncompleteNumericCheckpoint.ErrorMessage,
                StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_TrialWeightsChangedButQualityNotImprovedKeepsAcceptedParent()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-training-quality-retains-parent");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-lifecycle-quality-retains-parent");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-quality-retains-parent-seed",
                datasetVersion: "v0-parent");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-lifecycle-quality-retains-parent-new",
                datasetVersion: "v1-new");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent before rejected quality decision");
            var parentAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 1);
            var parentWeightsSha256Before =
                ComputeFileSha256(parentAddress.WeightsPath);
            var parentManifestSha256Before =
                ComputeFileSha256(parentAddress.ManifestPath);
            var childAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 2);
            var request = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var routingRun = CreateRoutingRun(dpoSetup.Receipt, dpoSetup.Pairs);
            var trainingRunId = "code-doc-lifecycle:quality-retains-parent";
            var attemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);

            var result =
                await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                    .TrainAndPublishAsync(
                        new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                            routingRun,
                            dpoSetup.Address,
                            request,
                            tokenizer,
                            CreateLoraConfig(),
                            CreateAcceptingBroker(),
                            trainingRunId,
                            DpoStepCount: 1,
                            DateTimeOffset.UnixEpoch.AddDays(7),
                            ["test:code-documentation-dpo-quality-retains-parent"],
                            CreateQualityGateContext(
                                qualityCorpus,
                                tokenizer,
                                CreateQualityOutcomes(
                                    baselinePass: false,
                                    trialPass: false)),
                            OptimizerStateColdStartAuthorization:
                                ExplicitOptimizerColdStartAuthorization(
                                    "root quality-retain lifecycle")))
                    .WaitAsync(TimeSpan.FromSeconds(30));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                    .QualityGateNotImprovedErrorCode,
                result.ErrorCode);
            Assert.True(File.Exists(Path.Combine(
                attemptAddress.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1))));
            Assert.NotEqual(
                parentWeightsSha256Before,
                ComputeFileSha256(Path.Combine(
                    attemptAddress.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1))));
            Assert.False(File.Exists(childAddress.WeightsPath));
            Assert.False(File.Exists(childAddress.ManifestPath));
            Assert.Equal(
                parentWeightsSha256Before,
                ComputeFileSha256(parentAddress.WeightsPath));
            Assert.Equal(
                parentManifestSha256Before,
                ComputeFileSha256(parentAddress.ManifestPath));

            var reopened =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
            Assert.Equal(parentManifest.Step, reopened.Value!.ParentManifest!.Step);
            Assert.Equal(
                parentManifest.WeightsSha256,
                reopened.Value.ParentManifest.WeightsSha256);
            Assert.Equal(
                parentWeightsSha256Before,
                ComputeCheckpointSha256(reopened.Value.Model));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SameRunRecoveryRemainsEvaluationOnlyWithoutTrainerUpdate(bool seededDropout)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-dpo-training-resume-publishes");
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture,
                repositoryRoot,
                "resume-publishes", seededDropout);
            var savedBaseline = ReadVerifiedResumeBaseline(fixture, staged);
            var savedAssessment = savedBaseline.State.BaselineQuality.Calibration.RepresentativeAssessment;
            var savedCalibration = savedBaseline.State.BaselineQuality.Calibration.Calibration;
            // Pin the existing v1 byte recipe independently of the later C refactor.
            var expectedEvaluationSetId = "code-doc-assessment:" + Sha256Hex(string.Join("\n",
                "code-documentation-saved-model-assessment-evaluation-set-v1",
                savedAssessment.DomainSlug,
                savedAssessment.DesignPatternSlug,
                savedAssessment.CorpusVersion,
                savedAssessment.CorpusManifestSha256,
                savedAssessment.CorpusSha256,
                savedAssessment.GenerationSettingsSha256,
                savedAssessment.Evaluator.ExpectedModelId,
                savedAssessment.Evaluator.ExpectedOllamaManifestDigest,
                savedAssessment.Evaluator.ExpectedOllamaModelLayerDigest,
                savedAssessment.Evaluator.EvaluationProfileId,
                savedAssessment.Evaluator.EvaluationProfileConfigurationSha256,
                MeasurementNoisePolicyConfig.IdentitySchema,
                savedCalibration.NoisePolicyIdentitySha256,
                "code-documentation-case-pass-fraction-v1"));
            Assert.Equal(expectedEvaluationSetId, savedCalibration.EvaluationSetId);
            var originalCorpus = staged.QualityCorpus.Corpus;
            var reversedEntries = originalCorpus.Entries.Reverse().ToArray();
            Assert.NotEqual(originalCorpus.Entries[0].EntryId, reversedEntries[0].EntryId);
            var reorderedCorpus = SealQualityReferenceCorpus(
                repositoryRoot, originalCorpus.CorpusVersion, reversedEntries);
            Assert.Equal(originalCorpus.CanonicalRepresentation,
                reorderedCorpus.Corpus.CanonicalRepresentation);
            Assert.Equal(originalCorpus.CorpusSha256, reorderedCorpus.Corpus.CorpusSha256);
            Assert.Equal(staged.QualityCorpus.Manifest.ManifestSha256,
                reorderedCorpus.Manifest.ManifestSha256);
            Assert.Equal(originalCorpus.EntryNames, reorderedCorpus.Corpus.EntryNames);
            var retainedFiles = SnapshotResumeFiles(repositoryRoot);
            var policyCheckpointPath = Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1));
            var baselinePath = Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .BaselineStateFileName);
            var pairPath = Path.Combine(
                repositoryRoot,
                staged.DpoSetup.Receipt.OrderedPairs[0]
                    .RepositoryRelativePairFilePath);
            var policySha256Before = ComputeFileSha256(policyCheckpointPath);
            var baselineSha256Before = ComputeFileSha256(baselinePath);
            var pairSha256Before = ComputeFileSha256(pairPath);
            var parentWeightsSha256Before =
                ComputeFileSha256(staged.ParentAddress.WeightsPath);
            var parentManifestSha256Before =
                ComputeFileSha256(staged.ParentAddress.ManifestPath);

            // Recreate runtime-facing request, tokenizer, evaluator, and start
            // dependencies. This is an in-process reconstruction, not a claim
            // of a cross-process restart.
            var resumedTokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var resumedQuality = CreateQualityGateContext(
                reorderedCorpus,
                resumedTokenizer,
                CreateQualityOutcomes(baselinePass: true, trialPass: true),
                index => "resume-quality-gate-evaluation-" + index.ToString("D4"));
            var resumedEvaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(
                resumedQuality.EvaluationClient);
            var resumedRequest = CreateRecoveryRequest(
                fixture,
                staged,
                staged.DpoSetup,
                resumedTokenizer,
                resumedQuality);
            var resumedTrainerInvoked = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;

            OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>
                resumed;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        resumedTrainerInvoked = true;
                        priorObserver?.Invoke(logicalRequest);
                    };
                resumed =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(resumedRequest)
                        .WaitAsync(TimeSpan.FromSeconds(30));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }

            Assert.True(resumed.IsSuccess,
                $"{resumed.ErrorCode}: {resumed.ErrorMessage}");
            Assert.NotNull(resumed.Value);
            Assert.False(resumedTrainerInvoked);
            if (seededDropout)
            {
                var saved = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .OpenExisting(staged.AttemptAddress);
                Assert.True(saved.IsSuccess, saved.ErrorMessage);
                var identity = saved.Value!.Manifest;
                Assert.True(identity.RandomStateRequired);
                Assert.True(resumed.Value.Training.RandomStateRequired);
                Assert.Equal(identity.RandomStateSha256, resumed.Value.Training.RandomStateSha256);
                Assert.Equal(identity.RandomStateByteLength, resumed.Value.Training.RandomStateByteLength);
                Assert.Equal(identity.RandomStateStep, resumed.Value.Training.RandomStateStep);
                Assert.Equal(identity.RandomStateTopologySha256, resumed.Value.Training.RandomStateTopologySha256);
                Assert.Equal(identity.LoraConfigurationSha256, resumed.Value.Training.RandomStateLoraConfigurationSha256);
            }
            Assert.Equal(25, resumedEvaluator.Observations.Count);
            Assert.Equal(25, resumedEvaluator.IssuedCallCount);
            Assert.Equal(25, resumedEvaluator.CompletedResponseCount);
            Assert.NotEqual(savedBaseline.State.BaselineQuality.Calibration
                .RepresentativeAssessment.ModelSerializationBeforeSha256, policySha256Before);
            AssertRetainedResumeFiles(repositoryRoot, retainedFiles, allowAdditionalFiles: true);
            Assert.Equal(2, resumed.Value!.PublishedCheckpointStep);
            Assert.Equal(staged.ParentManifest.Step,
                resumed.Value.ParentCheckpointStep);
            Assert.Equal(policyCheckpointPath,
                resumed.Value.Training.PolicyCheckpointPath);
            Assert.Equal(policySha256Before,
                ComputeFileSha256(resumed.Value.Training.PolicyCheckpointPath));
            Assert.Equal(baselineSha256Before, ComputeFileSha256(baselinePath));
            Assert.Equal(pairSha256Before, ComputeFileSha256(pairPath));
            Assert.Equal(parentWeightsSha256Before,
                ComputeFileSha256(staged.ParentAddress.WeightsPath));
            Assert.Equal(parentManifestSha256Before,
                ComputeFileSha256(staged.ParentAddress.ManifestPath));
            Assert.Equal(50, resumed.Value.QualityDecision!.ExpectedEvaluationCallCount);
            Assert.Equal(50, resumed.Value.QualityDecision.ObservedEvaluationCallCount);
            Assert.All(
                resumed.Value.QualityDecision.TrialCalibration.Observations,
                observation => Assert.Equal(
                    policySha256Before,
                    observation.ModelSerializationBeforeSha256));
            Assert.All(
                resumed.Value.QualityDecision.TrialCalibration.Observations,
                observation => Assert.Equal(
                    observation.ModelSerializationBeforeSha256,
                    observation.ModelSerializationAfterSha256));
            Assert.True(File.Exists(staged.ChildAddress.WeightsPath));
            Assert.True(File.Exists(staged.ChildAddress.ManifestPath));

            var nextStart =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(staged.StartRequest);
            Assert.True(nextStart.IsSuccess, nextStart.ErrorMessage);
            Assert.Equal(2, nextStart.Value!.ParentManifest!.Step);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task CreateAppliedButUnpublishedAttemptAsync_IncompleteStagingPersistsDurableAttemptAfterScriptedTrialFailure()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-dpo-training-incomplete-staging");
        try
        {
            var prepared = PrepareAppliedButUnpublishedAttempt(
                fixture,
                repositoryRoot,
                "incomplete-staging",
                seededDropout: true);
            Assert.Equal(repositoryRoot, prepared.RepositoryRoot);
            Assert.Equal(1, prepared.ParentManifest.Step);
            var reopenedPreparedParent = RepositoryOwnedModelCheckpointManifestSealer
                .OpenSealed(prepared.ParentAddress);
            Assert.True(reopenedPreparedParent.IsSuccess, reopenedPreparedParent.ErrorMessage);
            Assert.Equal(prepared.ParentManifest.ManifestSha256,
                reopenedPreparedParent.Value!.Manifest.ManifestSha256);
            Assert.True(File.Exists(prepared.ParentAddress.ManifestPath));

            var staged = await StageIncomplete(prepared);
            Assert.Equal(prepared.RepositoryRoot, staged.RepositoryRoot);
            Assert.Equal(prepared.ParentManifest, staged.ParentManifest);
            Assert.Equal(prepared.ParentAddress, staged.ParentAddress);
            Assert.Equal(prepared.AttemptAddress, staged.AttemptAddress);
            Assert.Equal(prepared.TrainingRunId, staged.TrainingRunId);

            Assert.False(staged.Staging.Result.IsSuccess);
            Assert.Equal(
                "SCRIPTED_QUALITY_GATE_DECISION_MISSING",
                staged.Staging.Result.ErrorCode);
            Assert.Null(staged.Staging.Result.Value);
            Assert.Equal(51, staged.Staging.Evaluator.MaxTeacherCalls);
            Assert.Equal(26, staged.Staging.Evaluator.IssuedCallCount);
            Assert.Equal(25, staged.Staging.Evaluator.CompletedResponseCount);
            Assert.Equal(25, staged.Staging.Evaluator.Observations.Count);

            var expectedCaseIds = staged.QualityCorpus.Corpus.Entries
                .Where(entry => entry.Partition == FrozenReferencePartition.HeldOut)
                .Select(entry => entry.EntryId)
                .OrderBy(entryId => entryId, StringComparer.Ordinal)
                .ToArray();
            var observedCaseIds = staged.Staging.Evaluator.Observations
                .Select(observation => observation.CaseId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(caseId => caseId, StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(expectedCaseIds, observedCaseIds);
            Assert.All(
                staged.Staging.Evaluator.Observations
                    .GroupBy(observation => observation.CaseId, StringComparer.Ordinal),
                group => Assert.Equal(5, group.Count()));
            Assert.Equal(
                staged.Staging.Evaluator.Observations.Count,
                staged.Staging.Evaluator.Observations
                    .Select(observation => observation.RequestId)
                    .Distinct(StringComparer.Ordinal)
                    .Count());

            var savedBaseline = ReadVerifiedResumeBaseline(fixture, staged);
            Assert.Equal(25, savedBaseline.State.BaselineQuality.ObservedEvaluationCallCount);
            var savedOptimizer =
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .OpenExisting(staged.AttemptAddress);
            Assert.True(savedOptimizer.IsSuccess, savedOptimizer.ErrorMessage);
            var savedManifest = savedOptimizer.Value!.Manifest;
            Assert.True(File.Exists(Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                RepositoryOwnedDpoOptimizerStateFile.BuildFileName(
                    savedManifest.OptimizerStateStep))));
            Assert.True(savedManifest.RandomStateRequired);
            Assert.True(File.Exists(Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                RepositoryOwnedDpoRandomStateFile.BuildFileName(
                    savedManifest.RandomStateStep))));
            Assert.True(File.Exists(Path.Combine(
                repositoryRoot,
                staged.DpoSetup.Receipt.OrderedPairs[0]
                    .RepositoryRelativePairFilePath)));
            Assert.False(File.Exists(staged.ChildAddress.WeightsPath));
            Assert.False(File.Exists(staged.ChildAddress.ManifestPath));
            Assert.False(Directory.Exists(staged.ChildAddress.CheckpointRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("evaluator-model")]
    [InlineData("evaluator-manifest")]
    [InlineData("evaluator-layer")]
    [InlineData("corpus-version")]
    [InlineData("reference-content")]
    [InlineData("source-fingerprint")]
    [InlineData("held-out-membership")]
    [InlineData("generation-max-new-tokens")]
    [InlineData("generation-temperature")]
    [InlineData("generation-top-k")]
    [InlineData("generation-top-p")]
    [InlineData("generation-seed")]
    [InlineData("generation-max-held-out-cases")]
    [InlineData("generation-max-answer-chars")]
    [InlineData("noise-repeat")]
    [InlineData("noise-multiplier")]
    public async Task SameRunRecovery_RejectsChangedExamConditionBeforeEvaluation(string condition)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-exam-drift-" + condition);
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture, repositoryRoot, "exam-drift-" + condition);
            var baseline = ReadVerifiedResumeBaseline(fixture, staged);
            var originalFiles = SnapshotResumeFiles(repositoryRoot);
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var quality = CreateChangedExamQuality(staged, tokenizer, condition);
            AssertResumePlan(quality);
            AssertRetainedResumeFiles(repositoryRoot, originalFiles, allowAdditionalFiles: true);
            var retainedFiles = SnapshotResumeFiles(repositoryRoot);
            var evaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(quality.EvaluationClient);
            Assert.Equal(0, evaluator.IssuedCallCount);
            Assert.Equal(0, evaluator.CompletedResponseCount);
            var request = CreateRecoveryRequest(fixture, staged, staged.DpoSetup, tokenizer, quality);
            Assert.Equal(baseline.State.TrainingRunId, request.TrainingRunId);

            var observed = await ObserveResumeAsync(request, evaluator);

            AssertRejectedResumeRetainsAttempt(staged, retainedFiles);
            AssertResumeFailure(observed, evaluator,
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                    .QualityGateConditionMismatchErrorCode,
                expectedCalls: 0);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task SameRunRecovery_RejectsMissingSavedNoisePolicyIdentityBeforeEvaluation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-exam-legacy-noise");
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture, repositoryRoot, "exam-legacy-noise");
            var baseline = ReadVerifiedResumeBaseline(fixture, staged);
            var originalFiles = SnapshotResumeFiles(repositoryRoot);
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var quality = CreateQualityGateContext(staged.QualityCorpus, tokenizer,
                CreateQualityOutcomes(baselinePass: true, trialPass: true),
                index => "legacy-noise-resume-" + index.ToString("D4"));
            AssertResumePlan(quality);
            AssertSavedBaselineExamIdentity(staged, baseline.State, quality);
            var originalNoise = baseline.State.BaselineQuality.Calibration.Calibration
                .NoisePolicyIdentitySha256;
            Assert.Equal(MeasurementNoisePolicyConfig.Canonical.IdentitySha256, originalNoise);
            var json = JsonNode.Parse(File.ReadAllText(baseline.BaselineStatePath))!.AsObject();
            var calibration = json["BaselineQuality"]!["Calibration"]!["Calibration"]!.AsObject();
            Assert.True(calibration.Remove("NoisePolicyIdentitySha256"));
            File.WriteAllText(baseline.BaselineStatePath, json.ToJsonString());

            // Only this test-owned legacy field is removed. The journal must
            // accept the shape before a coordinator refusal can count as preflight evidence.
            var reopened = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(staged.AttemptAddress);
            Assert.True(reopened.IsSuccess, $"{reopened.ErrorCode}: {reopened.ErrorMessage}");
            Assert.Null(reopened.Value!.State.BaselineQuality.Calibration.Calibration
                .NoisePolicyIdentitySha256);
            Assert.NotEqual(baseline.BaselineStateSha256, reopened.Value.BaselineStateSha256);
            foreach (var file in originalFiles.Where(file =>
                         !StringComparer.Ordinal.Equals(file.Key, baseline.BaselineStatePath)))
                Assert.Equal(file.Value, ComputeFileSha256(file.Key));
            var retainedFiles = SnapshotResumeFiles(repositoryRoot);
            var evaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(quality.EvaluationClient);
            var request = CreateRecoveryRequest(fixture, staged, staged.DpoSetup, tokenizer, quality);
            Assert.Equal(baseline.State.TrainingRunId, request.TrainingRunId);

            var observed = await ObserveResumeAsync(request, evaluator);

            AssertRejectedResumeRetainsAttempt(staged, retainedFiles);
            AssertResumeFailure(observed, evaluator,
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                    .QualityGateConditionMismatchErrorCode,
                expectedCalls: 0);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("layer-provenance")]
    [InlineData("authoring-profile")]
    public async Task SameRunRecovery_RejectsMidSeriesEvaluationIdentityDrift(string drift)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-exam-response-" + drift);
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture, repositoryRoot, "exam-response-" + drift);
            ReadVerifiedResumeBaseline(fixture, staged);
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var transformedCalls = new List<int>();
            var quality = CreateQualityGateContext(staged.QualityCorpus, tokenizer,
                CreateQualityOutcomes(baselinePass: true, trialPass: true),
                index => "mid-series-" + drift + "-" + index.ToString("D4"),
                responseTransform: (call, response) =>
                {
                    if (call != 2)
                        return response;
                    transformedCalls.Add(call);
                    return drift switch
                    {
                        "layer-provenance" => response with
                        {
                            TrainingTeacherProvenance = response.TrainingTeacherProvenance with
                            {
                                ModelArtifactDigest = Hex('e')
                            }
                        },
                        "authoring-profile" => response with
                        {
                            InferenceProfile = LocalQwenOllamaInferenceProfileAuthority.Authoring
                        },
                        _ => throw new InvalidOperationException("Unknown response identity drift.")
                    };
                });
            AssertResumePlan(quality);
            var evaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(quality.EvaluationClient);
            var retainedFiles = SnapshotResumeFiles(repositoryRoot);
            var request = CreateRecoveryRequest(fixture, staged, staged.DpoSetup, tokenizer, quality);

            var observed = await ObserveResumeAsync(request, evaluator);

            Assert.Equal(new[] { 2 }, transformedCalls);
            AssertRejectedResumeRetainsAttempt(staged, retainedFiles);
            AssertResumeFailure(observed, evaluator,
                drift == "layer-provenance"
                    ? CodeDocumentationSavedModelAssessmentService.EvaluationProvenanceInvalidErrorCode
                    : CodeDocumentationSavedModelAssessmentService.EvaluationProfileInvalidErrorCode,
                expectedCalls: 2);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task SameRunRecovery_AlreadyPublishedReplayRejectsBeforeEvaluation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-exam-published-replay");
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture, repositoryRoot, "exam-published-replay");
            ReadVerifiedResumeBaseline(fixture, staged);
            var retainedBeforePublication = SnapshotResumeFiles(repositoryRoot);
            var publishTokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var publishQuality = CreateQualityGateContext(staged.QualityCorpus, publishTokenizer,
                CreateQualityOutcomes(baselinePass: true, trialPass: true),
                index => "publish-before-replay-" + index.ToString("D4"));
            var publishEvaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(publishQuality.EvaluationClient);
            var published = await ObserveResumeAsync(CreateRecoveryRequest(
                fixture, staged, staged.DpoSetup, publishTokenizer, publishQuality), publishEvaluator);
            Assert.True(published.Result.IsSuccess,
                $"{published.Result.ErrorCode}: {published.Result.ErrorMessage}");
            Assert.Equal(0, published.TrainerCalls);
            Assert.Equal(25, publishEvaluator.IssuedCallCount);
            Assert.Equal(25, publishEvaluator.CompletedResponseCount);
            Assert.Equal(25, publishEvaluator.Observations.Count);
            Assert.Equal(2, published.Result.Value!.PublishedCheckpointStep);
            Assert.True(File.Exists(staged.ChildAddress.WeightsPath));
            Assert.True(File.Exists(staged.ChildAddress.ManifestPath));
            AssertRetainedResumeFiles(repositoryRoot, retainedBeforePublication, allowAdditionalFiles: true);
            var accepted = CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(staged.StartRequest);
            Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
            Assert.Equal(2, accepted.Value!.ParentManifest!.Step);
            Assert.Equal(published.Result.Value.Publication.WeightsSha256,
                accepted.Value.ParentManifest.WeightsSha256);

            // This is a separate published replay, never the drift fixture.
            var retainedFiles = SnapshotResumeFiles(repositoryRoot);
            var replayTokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var replayQuality = CreateQualityGateContext(staged.QualityCorpus, replayTokenizer,
                CreateQualityOutcomes(baselinePass: true, trialPass: true),
                index => "already-published-replay-" + index.ToString("D4"));
            var replayEvaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(replayQuality.EvaluationClient);
            var replayRequest = CreateRecoveryRequest(
                fixture, staged, staged.DpoSetup, replayTokenizer, replayQuality);
            Assert.Equal(staged.TrainingRunId, replayRequest.TrainingRunId);

            var replay = await ObserveResumeAsync(replayRequest, replayEvaluator);

            AssertRetainedResumeFiles(repositoryRoot, retainedFiles);
            Assert.False(Directory.Exists(CreateModelCheckpointAddress(repositoryRoot, 3).CheckpointRoot));
            var afterReplay = CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(staged.StartRequest);
            Assert.True(afterReplay.IsSuccess, afterReplay.ErrorMessage);
            Assert.Equal(2, afterReplay.Value!.ParentManifest!.Step);
            Assert.Equal(accepted.Value.ParentManifest.ManifestSha256,
                afterReplay.Value.ParentManifest.ManifestSha256);
            AssertResumeFailure(replay, replayEvaluator,
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PairAlreadyAppliedErrorCode,
                expectedCalls: 0);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TrainAndPublishAsync_SameRunIdMissingOrCorruptPersistedTrialFailsClosedWithoutRetraining(
        bool corruptTrial)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            corruptTrial
                ? "code-doc-dpo-training-resume-corrupt-trial"
                : "code-doc-dpo-training-resume-missing-trial");
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture,
                repositoryRoot,
                corruptTrial ? "resume-corrupt-trial" : "resume-missing-trial");
            var policyCheckpointPath = Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1));
            var baselinePath = Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .BaselineStateFileName);
            var pairPath = Path.Combine(
                repositoryRoot,
                staged.DpoSetup.Receipt.OrderedPairs[0]
                    .RepositoryRelativePairFilePath);
            var baselineSha256Before = ComputeFileSha256(baselinePath);
            var pairSha256Before = ComputeFileSha256(pairPath);
            var parentWeightsSha256Before =
                ComputeFileSha256(staged.ParentAddress.WeightsPath);
            var parentManifestSha256Before =
                ComputeFileSha256(staged.ParentAddress.ManifestPath);
            if (corruptTrial)
                File.WriteAllBytes(policyCheckpointPath, [0x01, 0x02, 0x03]);
            else
                File.Delete(policyCheckpointPath);

            var resumedTokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var resumedQuality = CreateQualityGateContext(
                staged.QualityCorpus,
                resumedTokenizer,
                CreateQualityOutcomes(baselinePass: true, trialPass: true),
                index => "invalid-trial-resume-evaluation-" + index.ToString("D4"));
            var resumedEvaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(
                resumedQuality.EvaluationClient);
            var resumedRequest = CreateRecoveryRequest(
                fixture,
                staged,
                staged.DpoSetup,
                resumedTokenizer,
                resumedQuality);
            var resumedTrainerInvoked = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    _ => resumedTrainerInvoked = true;
                var resumed =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(resumedRequest)
                        .WaitAsync(TimeSpan.FromSeconds(30));

                Assert.False(resumed.IsSuccess);
                Assert.Equal(
                    CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrialCheckpointMismatchErrorCode,
                    resumed.ErrorCode);
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }

            Assert.False(resumedTrainerInvoked);
            Assert.Empty(resumedEvaluator.Observations);
            Assert.False(File.Exists(staged.ChildAddress.WeightsPath));
            Assert.False(File.Exists(staged.ChildAddress.ManifestPath));
            Assert.Equal(baselineSha256Before, ComputeFileSha256(baselinePath));
            Assert.Equal(pairSha256Before, ComputeFileSha256(pairPath));
            AssertAcceptedParentRemainsLoadable(
                staged,
                parentWeightsSha256Before,
                parentManifestSha256Before);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_SameRunIdDifferentSealedLessonRefusesWithoutMutatingAcceptedState()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-dpo-training-resume-lesson-mismatch");
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture,
                repositoryRoot,
                "resume-lesson-mismatch");
            var baselinePath = Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .BaselineStateFileName);
            var originalPairPath = Path.Combine(
                repositoryRoot,
                staged.DpoSetup.Receipt.OrderedPairs[0]
                    .RepositoryRelativePairFilePath);
            var baselineSha256Before = ComputeFileSha256(baselinePath);
            var originalPairSha256Before = ComputeFileSha256(originalPairPath);
            var parentWeightsSha256Before =
                ComputeFileSha256(staged.ParentAddress.WeightsPath);
            var parentManifestSha256Before =
                ComputeFileSha256(staged.ParentAddress.ManifestPath);
            var differentLesson = SealRepositoryOwnedDpo(
                repositoryRoot,
                "resume-lesson-mismatch-different-pair",
                datasetVersion: "v2-different-lesson");

            var resumedTokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var resumedQuality = CreateQualityGateContext(
                staged.QualityCorpus,
                resumedTokenizer,
                CreateQualityOutcomes(baselinePass: true, trialPass: true),
                index => "lesson-mismatch-resume-evaluation-" + index.ToString("D4"));
            var resumedEvaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(
                resumedQuality.EvaluationClient);
            var resumedRequest = CreateRecoveryRequest(
                fixture,
                staged,
                differentLesson,
                resumedTokenizer,
                resumedQuality);
            var resumedTrainerInvoked = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    _ => resumedTrainerInvoked = true;
                var resumed =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(resumedRequest)
                        .WaitAsync(TimeSpan.FromSeconds(30));

                Assert.False(resumed.IsSuccess);
                Assert.Equal(
                    CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainingAttemptRecoveryRequiredErrorCode,
                    resumed.ErrorCode);
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }

            Assert.False(resumedTrainerInvoked);
            Assert.Empty(resumedEvaluator.Observations);
            Assert.False(File.Exists(staged.ChildAddress.WeightsPath));
            Assert.False(File.Exists(staged.ChildAddress.ManifestPath));
            Assert.Equal(baselineSha256Before, ComputeFileSha256(baselinePath));
            Assert.Equal(originalPairSha256Before, ComputeFileSha256(originalPairPath));
            AssertAcceptedParentRemainsLoadable(
                staged,
                parentWeightsSha256Before,
                parentManifestSha256Before);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData(false, 50, 50, 50, 75)]
    [InlineData(true, 51, 26, 25, 51)]
    public async Task TrainAndPublishAsync_SameRunIdResumedNonImprovementKeepsAcceptedIdentity(
        bool incompleteStaging,
        int expectedMaxTeacherCalls,
        int expectedStagingIssuedCalls,
        int expectedStagingCompletedResponses,
        int expectedPhysicalCalls)
    {
        var stagingMode = incompleteStaging
            ? AppliedButUnpublishedAttemptStagingMode.IncompleteStaging
            : AppliedButUnpublishedAttemptStagingMode.CompletedNonImproving;
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-dpo-training-resume-non-improvement");
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture,
                repositoryRoot,
                "resume-non-improvement",
                stagingMode: stagingMode);
            Assert.False(staged.Staging.Result.IsSuccess);
            Assert.Equal(
                stagingMode == AppliedButUnpublishedAttemptStagingMode.CompletedNonImproving
                    ? CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                        .QualityGateNotImprovedErrorCode
                    : "SCRIPTED_QUALITY_GATE_DECISION_MISSING",
                staged.Staging.Result.ErrorCode);
            Assert.Null(staged.Staging.Result.Value);
            Assert.Equal(expectedMaxTeacherCalls, staged.Staging.Evaluator.MaxTeacherCalls);
            Assert.Equal(expectedStagingIssuedCalls, staged.Staging.Evaluator.IssuedCallCount);
            Assert.Equal(expectedStagingCompletedResponses, staged.Staging.Evaluator.CompletedResponseCount);
            Assert.Equal(expectedStagingCompletedResponses, staged.Staging.Evaluator.Observations.Count);
            Assert.False(File.Exists(staged.ChildAddress.WeightsPath));
            Assert.False(File.Exists(staged.ChildAddress.ManifestPath));

            var trialCheckpointPath = Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1));
            var baselinePath = Path.Combine(
                staged.AttemptAddress.AttemptRoot,
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .BaselineStateFileName);
            var pairPath = Path.Combine(
                repositoryRoot,
                staged.DpoSetup.Receipt.OrderedPairs[0]
                    .RepositoryRelativePairFilePath);
            var trialSha256Before = ComputeFileSha256(trialCheckpointPath);
            var baselineSha256Before = ComputeFileSha256(baselinePath);
            var pairSha256Before = ComputeFileSha256(pairPath);
            var parentWeightsSha256Before =
                ComputeFileSha256(staged.ParentAddress.WeightsPath);
            var parentManifestSha256Before =
                ComputeFileSha256(staged.ParentAddress.ManifestPath);

            var resumedTokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var resumedQuality = CreateQualityGateContext(
                staged.QualityCorpus,
                resumedTokenizer,
                CreateQualityOutcomes(baselinePass: false, trialPass: false),
                index => "non-improved-resume-evaluation-" + index.ToString("D4"));
            var resumedEvaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(
                resumedQuality.EvaluationClient);
            var resumedRequest = CreateRecoveryRequest(
                fixture,
                staged,
                staged.DpoSetup,
                resumedTokenizer,
                resumedQuality);
            var resumedTrainerInvoked = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    _ => resumedTrainerInvoked = true;
                var resumed =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(resumedRequest)
                        .WaitAsync(TimeSpan.FromSeconds(30));

                Assert.False(resumed.IsSuccess);
                Assert.Equal(
                    CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                        .QualityGateNotImprovedErrorCode,
                    resumed.ErrorCode);
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }

            Assert.False(resumedTrainerInvoked);
            Assert.Equal(25, resumedEvaluator.IssuedCallCount);
            // Recovery reuses the saved baseline; these 25 completed responses are trial-only.
            Assert.Equal(25, resumedEvaluator.CompletedResponseCount);
            Assert.Equal(25, resumedEvaluator.Observations.Count);
            Assert.Equal(
                expectedPhysicalCalls,
                staged.Staging.Evaluator.IssuedCallCount + resumedEvaluator.IssuedCallCount);
            Assert.False(File.Exists(staged.ChildAddress.WeightsPath));
            Assert.False(File.Exists(staged.ChildAddress.ManifestPath));
            Assert.Equal(trialSha256Before, ComputeFileSha256(trialCheckpointPath));
            Assert.Equal(baselineSha256Before, ComputeFileSha256(baselinePath));
            Assert.Equal(pairSha256Before, ComputeFileSha256(pairPath));
            AssertAcceptedParentRemainsLoadable(
                staged,
                parentWeightsSha256Before,
                parentManifestSha256Before);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("missing-rng", true)]
    [InlineData("corrupt-rng", true)]
    [InlineData("rng-length", true)]
    [InlineData("rng-step", true)]
    [InlineData("rng-topology", true)]
    [InlineData("rng-config", true)]
    [InlineData("authority", true)]
    [InlineData("no-authority", true)]
    [InlineData("caller-rng", true)]
    [InlineData("cancel", true)]
    [InlineData("same-shape-trial", true)]
    [InlineData("trial-path", true)]
    [InlineData("trial-length", true)]
    [InlineData("legacy-v2", true)]
    [InlineData("legacy-v3", true)]
    [InlineData("legacy-v2", false)]
    [InlineData("legacy-v3", false)]
    public async Task SameRunRejectsIncompleteOrMixedBundleBeforeEvaluationAndRetainsParent(
        string fault, bool seededDropout)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-bundle-" + fault);
        try
        {
            var staged = await CreateAppliedButUnpublishedAttemptAsync(
                fixture, repositoryRoot, "bundle-" + fault, seededDropout);
            var saved = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                .OpenExisting(staged.AttemptAddress);
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            var manifest = saved.Value!.Manifest;
            Assert.Equal(4, manifest.FormatVersion);
            Assert.Equal(seededDropout, manifest.RandomStateRequired);
            var trial = CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .LoadTrainingAttemptCheckpoint(staged.StartRequest, staged.AttemptAddress, 1);
            Assert.True(trial.IsSuccess, trial.ErrorMessage);
            Assert.Equal(manifest.TrialWeightsSha256, trial.Value!.WeightsSha256);
            var pristineTrial = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .ValidateTrialCheckpointAgainstManifest(manifest, staged.AttemptAddress, trial.Value);
            Assert.True(pristineTrial.IsSuccess, pristineTrial.ErrorMessage);
            var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(staged.AttemptAddress.AttemptRoot,
                manifest.OptimizerStateStep, manifest.OptimizerStateSha256);
            Assert.True(optimizer.IsSuccess, optimizer.ErrorMessage);
            var runtime = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.MatchesRuntime(
                manifest, staged.AttemptAddress, fixture.ModelConfig,
                trial.Value.Model.GetCheckpointParameterEntries().Select(entry =>
                    new AdamWParameterBinding(entry.Key, entry.Parameter)).ToArray(),
                DpoTrainer.ProductionOptimizerSettings, optimizer.Value!.State);
            Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
            var randomPath = Path.Combine(repositoryRoot, manifest.RandomStateRelativePath);
            LlmModelRandomStateSnapshot? random = null;
            if (seededDropout)
            {
                var opened = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
                    randomPath, manifest.RandomStateSha256, manifest.RandomStateByteLength,
                    manifest.RandomStateStep, manifest.RandomStateTopologySha256,
                    manifest.LoraConfigurationSha256);
                Assert.True(opened.IsSuccess, opened.ErrorMessage);
                random = opened.Value!;
            }
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var quality = CreateQualityGateContext(staged.QualityCorpus, tokenizer,
                CreateQualityOutcomes(baselinePass: true, trialPass: true));
            var evaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(quality.EvaluationClient);
            var request = CreateRecoveryRequest(fixture, staged, staged.DpoSetup, tokenizer, quality);
            switch (fault)
            {
                case "trial-path":
                case "trial-length":
                {
                    var altered = fault == "trial-path"
                        ? manifest with { TrialWeightsRelativePath = manifest.AttemptRootRelativePath + "/other-policy.bin" }
                        : manifest with { TrialWeightsByteLength = manifest.TrialWeightsByteLength + 1 };
                    Assert.True(altered.TrialWeightsByteLength > 0);
                    var comparator = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .ValidateTrialCheckpointAgainstManifest(altered, staged.AttemptAddress, trial.Value);
                    Assert.False(comparator.IsSuccess);
                    Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrialCheckpointMismatchErrorCode,
                        comparator.ErrorCode);
                    MutateOptimizerStateManifest(saved.Value.ManifestPath, root =>
                    {
                        if (fault == "trial-path") root["TrialWeightsRelativePath"] = altered.TrialWeightsRelativePath;
                        else root["TrialWeightsByteLength"] = altered.TrialWeightsByteLength;
                    });
                    var openedAltered = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                        .OpenExisting(staged.AttemptAddress);
                    // Path ownership refuses structurally; positive wrong length
                    // opens structurally and is rejected by the trial comparator.
                    Assert.Equal(fault == "trial-length", openedAltered.IsSuccess);
                    break;
                }
                case "missing-rng": File.Delete(randomPath); break;
                case "corrupt-rng":
                {
                    var bytes = File.ReadAllBytes(randomPath);
                    bytes[^1] ^= 1;
                    File.WriteAllBytes(randomPath, bytes);
                    break;
                }
                case "rng-length":
                    MutateOptimizerStateManifest(saved.Value.ManifestPath,
                        root => root["RandomStateByteLength"] = manifest.RandomStateByteLength + 1);
                    break;
                case "rng-step":
                    MutateOptimizerStateManifest(saved.Value.ManifestPath,
                        root => root["RandomStateStep"] = manifest.RandomStateStep + 1);
                    break;
                case "rng-topology":
                    MutateOptimizerStateManifest(saved.Value.ManifestPath,
                        root => root["RandomStateTopologySha256"] = new string('a', 64));
                    break;
                case "rng-config":
                    MutateOptimizerStateManifest(saved.Value.ManifestPath,
                        root => root["LoraConfigurationSha256"] = new string('b', 64));
                    break;
                case "authority":
                case "no-authority":
                    var authority = fault == "no-authority" ? null :
                        new LoraPairRandomSeedAuthority(992UL, "policy.step", "reference.step");
                    request = request with
                    {
                        AdapterSeedAuthority = authority,
                        StartRequest = request.StartRequest with { AdapterSeedAuthority = authority }
                    };
                    break;
                case "caller-rng":
                    request = request with { RandomStateToRestore = random! with { RandomStateStep = manifest.RandomStateStep + 1 } };
                    break;
                case "cancel": break;
                case "same-shape-trial":
                    var parameter = trial.Value.Model.GetCheckpointParameterEntries().First().Parameter;
                    parameter.Value.Data[0] += 0.125f;
                    Assert.True(float.IsFinite(parameter.Value.Data[0]));
                    using (var stream = File.Create(trial.Value.WeightsPath))
                        trial.Value.Model.SaveCheckpointToStream(stream);
                    var replaced = CodeDocumentationRepositoryOwnedModelCheckpointStartService
                        .LoadTrainingAttemptCheckpoint(staged.StartRequest, staged.AttemptAddress, 1);
                    Assert.True(replaced.IsSuccess, replaced.ErrorMessage);
                    Assert.NotEqual(manifest.TrialWeightsSha256, replaced.Value!.WeightsSha256);
                    Assert.Equal(manifest.TrialWeightsByteLength, replaced.Value.WeightsByteLength);
                    break;
                case "legacy-v2":
                case "legacy-v3":
                    MutateOptimizerStateManifest(saved.Value.ManifestPath, root =>
                    {
                        root["FormatVersion"] = fault == "legacy-v2" ? 2 : 3;
                        root.Remove("TrialWeightsRelativePath");
                        root.Remove("TrialWeightsSha256");
                        root.Remove("TrialWeightsByteLength");
                        if (fault == "legacy-v2")
                            foreach (var key in root.Select(p => p.Key).Where(k =>
                                k.StartsWith("RandomState", StringComparison.Ordinal) ||
                                k == "LoraConfigurationSha256").ToArray())
                                root.Remove(key);
                    });
                    break;
                default: throw new InvalidOperationException("Unknown fault.");
            }

            // Snapshot AFTER the intentional fault. Recovery must not repair,
            // delete or rewrite any of the retained evidence or accepted files.
            var retained = Directory.GetFiles(repositoryRoot, "*", SearchOption.AllDirectories)
                .ToDictionary(path => path, ComputeFileSha256);
            var parentWeights = ComputeFileSha256(staged.ParentAddress.WeightsPath);
            var parentManifest = ComputeFileSha256(staged.ParentAddress.ManifestPath);
            var priorObserver = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            var trainerCalls = 0;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => trainerCalls++;
                if (fault == "cancel")
                {
                    using var cancellation = new CancellationTokenSource();
                    cancellation.Cancel();
                    // Exercise the changed RNG guard itself; the outer START route
                    // normalizes an already-cancelled request into its failure result.
                    Assert.ThrowsAny<OperationCanceledException>(() =>
                        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OpenAndValidateExistingAttemptRandomState(
                            fixture.ModelConfig, request.LoraConfig, request.AdapterSeedAuthority,
                            null, staged.AttemptAddress, manifest, cancellation.Token));
                    var cancelled = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(request, cancellation.Token);
                    Assert.False(cancelled.IsSuccess);
                    Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.StartFailedErrorCode,
                        cancelled.ErrorCode);
                    Assert.Equal("CodeDocumentation repository-owned LlmModel START was cancelled.",
                        cancelled.ErrorMessage);
                }
                else
                {
                    var resumed = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(request).WaitAsync(TimeSpan.FromSeconds(30));
                    Assert.False(resumed.IsSuccess);
                    if (fault == "no-authority")
                    {
                        var guard = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                            .OpenAndValidateExistingAttemptRandomState(fixture.ModelConfig, request.LoraConfig,
                                null, null, staged.AttemptAddress, manifest, CancellationToken.None);
                        Assert.False(guard.IsSuccess);
                        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.RandomStateRestoreRequiredErrorCode,
                            guard.ErrorCode);
                        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.StartFailedErrorCode,
                            resumed.ErrorCode);
                        Assert.Equal("Dropout-enabled repository START requires deterministic adapter authority.",
                            resumed.ErrorMessage);
                    }
                    else
                        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                            .TrainingAttemptRecoveryRequiredErrorCode, resumed.ErrorCode);
                }
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting = priorObserver;
            }
            Assert.Equal(0, trainerCalls);
            Assert.Empty(evaluator.Observations);
            Assert.False(File.Exists(staged.ChildAddress.WeightsPath));
            Assert.False(File.Exists(staged.ChildAddress.ManifestPath));
            Assert.Equal(retained.Keys.Order(), Directory.GetFiles(repositoryRoot, "*",
                SearchOption.AllDirectories).Order());
            foreach (var file in retained)
                Assert.Equal(file.Value, ComputeFileSha256(file.Key));
            AssertAcceptedParentRemainsLoadable(staged, parentWeights, parentManifest);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task RejectsRandomStateIdentityMismatchWithoutChangingAcceptedParent()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-dpo-random-identity-mismatch");
        try
        {
            var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
            var compatibility = CreateCompatibility(
                fixture.ModelConfig,
                "code-doc-random-identity-mismatch");
            var parentDpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-random-identity-parent",
                datasetVersion: "v0-parent");
            var dpoSetup = SealRepositoryOwnedDpo(
                repositoryRoot,
                "code-doc-random-identity-new",
                datasetVersion: "v1-new");
            var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
            var parentSession = fixture.CreateFactory().Create("CODE").Value!;
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                parentSession.Model,
                fixture.ModelConfig,
                compatibility,
                parentDpoSetup.Identity,
                step: 1,
                parent: null,
                coldReason: "test parent before random identity mismatch");
            var parentAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 1);
            var parentWeightsBefore = ComputeFileSha256(parentAddress.WeightsPath);
            var parentManifestBefore = ComputeFileSha256(parentAddress.ManifestPath);
            var authority = new LoraPairRandomSeedAuthority(
                991UL,
                "policy.step",
                "reference.step");
            var dropoutConfig = new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0.25f,
                TargetModules = ["q_proj"]
            };
            var startRequest = CreateStartRequest(
                repositoryRoot,
                fixture.ModelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied) with
            {
                LoraConfig = dropoutConfig,
                AdapterSeedAuthority = authority
            };
            var trainingRunId = "code-doc-lifecycle:random-identity-mismatch";
            var attemptAddress = CreateModelTrainingAttemptAddress(
                repositoryRoot,
                trainingRunId);
            var invalidRandomState = new LlmModelRandomStateSnapshot(
                LlmModelRandomStateSnapshot.CurrentFormatVersion,
                LlmModelRandomStateSnapshot.CurrentAlgorithm,
                [],
                new string('0', 64),
                new string('1', 64),
                1);
            var request =
                new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                    CreateRoutingRun(dpoSetup.Receipt, dpoSetup.Pairs),
                    dpoSetup.Address,
                    startRequest,
                    tokenizer,
                    dropoutConfig,
                    CreateAcceptingBroker(),
                    trainingRunId,
                    DpoStepCount: 1,
                    DateTimeOffset.UnixEpoch.AddDays(10),
                    ["test:random-identity-mismatch"],
                    CreateQualityGateContext(
                        qualityCorpus,
                        tokenizer,
                        CreateQualityOutcomes(
                            baselinePass: false,
                            trialPass: true)),
                    OptimizerStateColdStartAuthorization:
                        ExplicitOptimizerColdStartAuthorization(
                            "random identity mismatch root"),
                    AdapterSeedAuthority: authority,
                    RandomStateToRestore: invalidRandomState);
            var trainerReached = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    _ => trainerReached = true;
                var result =
                    await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainAndPublishAsync(request)
                        .WaitAsync(TimeSpan.FromSeconds(30));

                Assert.False(result.IsSuccess);
                Assert.Equal(
                    CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                        .TrainingFailedErrorCode,
                    result.ErrorCode);
                Assert.False(trainerReached);
                Assert.Equal(
                    parentWeightsBefore,
                    ComputeFileSha256(parentAddress.WeightsPath));
                Assert.Equal(
                    parentManifestBefore,
                    ComputeFileSha256(parentAddress.ManifestPath));
                Assert.False(File.Exists(Path.Combine(
                    attemptAddress.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1))));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_ImprovementBelowCertificationTargetPublishesWithQualityEvidence()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-noise-below-target");
        try
        {
            var setup = CreateNoiseDecisionSetup(fixture, repositoryRoot, "below-target", 5,
                [0, 0, 0, 0, 0], [4, 4, 4, 4, 4]);
            var observed = await ObserveNoiseDecisionAsync(setup);
            var scores = AssertObservedQualitySeries(setup, [0, 0, 0, 0, 0, 4, 4, 4, 4, 4]);
            Assert.Equal(1, observed.TrainerCalls);
            Assert.True(observed.Result.IsSuccess,
                $"{observed.Result.ErrorCode}: {observed.Result.ErrorMessage}");
            var evidence = observed.Result.Value!;
            var quality = evidence.QualityDecision;
            Assert.NotNull(quality);
            Assert.True(quality!.Accepted);
            Assert.Equal(MeasurementNoisePolicy.ImprovedStatus, quality.Status);
            Assert.Equal(scores.Take(5).Average(), quality.BaselineMeanCorrectFraction, 12);
            Assert.Equal(scores.Skip(5).Average(), quality.TrialMeanCorrectFraction, 12);
            Assert.Equal(0.8d, quality.TrialMeanCorrectFraction, 12);
            Assert.True(quality.TrialMeanCorrectFraction < 0.95d);
            var baseline = quality.BaselineCalibration.MeasurementNoisePassport;
            var trial = quality.TrialCalibration.MeasurementNoisePassport;
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(baseline, out _));
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(trial, out _));
            Assert.True(quality.Difference > baseline.NoiseFloor);
            Assert.True(quality.Difference > trial.NoiseFloor);
            Assert.Equal(Math.Max(baseline.NoiseFloor, trial.NoiseFloor), quality.NoiseFloor);
            Assert.Equal(50, quality.ExpectedEvaluationCallCount);
            Assert.Equal(50, quality.ObservedEvaluationCallCount);
            Assert.Equal(1, evidence.Training.DpoStepCount);
            Assert.Equal(2, evidence.PublishedCheckpointStep);
            Assert.True(File.Exists(setup.ChildAddress.WeightsPath));
            Assert.True(File.Exists(setup.ChildAddress.ManifestPath));
            AssertNoiseTrialWeightsChanged(setup);
            AssertRetainedResumeFiles(repositoryRoot, setup.InputHashes, allowAdditionalFiles: true);
            var reopened = CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(setup.Request.StartRequest);
            Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
            Assert.Equal(2, reopened.Value!.ParentManifest!.Step);
            Assert.Equal(evidence.Publication.WeightsSha256, reopened.Value.ParentManifest.WeightsSha256);
            // These are pair-quality/publication fields, not an independent final-exam certificate.
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("noisy-trial")]
    [InlineData("noisy-baseline")]
    public async Task TrainAndPublishAsync_RejectsImprovementWithinEitherSeriesNoiseFloor(string noisySeries)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-noise-" + noisySeries);
        try
        {
            int[] baselineCounts = noisySeries == "noisy-trial" ? [0, 0, 0, 0, 0] : [0, 0, 0, 5, 5];
            int[] trialCounts = noisySeries == "noisy-trial" ? [0, 0, 0, 5, 5] : [5, 5, 5, 5, 5];
            var setup = CreateNoiseDecisionSetup(fixture, repositoryRoot, noisySeries, 5,
                baselineCounts, trialCounts);
            var observed = await ObserveNoiseDecisionAsync(setup);
            var scores = AssertObservedQualitySeries(setup, baselineCounts.Concat(trialCounts).ToArray());
            Assert.Equal(1, observed.TrainerCalls);
            AssertNoiseTrialWeightsChanged(setup);
            var saved = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(setup.AttemptAddress);
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            var baseline = saved.Value!.State.BaselineQuality.Calibration.Calibration.MeasurementNoisePassport;
            Assert.Equal(scores.Take(5).ToArray(), baseline.RepeatedScores.ToArray());
            var trial = MeasurementNoisePolicy.BuildPassport(
                baseline.ScorerIdentity, baseline.EvaluationSetId, 5, scores.Skip(5).ToArray(),
                MeasurementNoisePolicyConfig.Canonical, DateTimeOffset.UnixEpoch);
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(baseline, out _));
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(trial, out _));
            var difference = trial.MeanScore - baseline.MeanScore;
            Assert.True(difference > 0d);
            if (noisySeries == "noisy-trial")
            {
                Assert.True(difference > baseline.NoiseFloor);
                Assert.True(difference <= trial.NoiseFloor);
                Assert.True(trial.NoiseFloor > 1d);
            }
            else
            {
                Assert.True(difference > trial.NoiseFloor);
                Assert.True(difference <= baseline.NoiseFloor);
                Assert.True(baseline.NoiseFloor > 1d);
            }

            Assert.False(observed.Result.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode,
                observed.Result.ErrorCode);
            Assert.Null(observed.Result.Value);
            AssertRetainedResumeFiles(repositoryRoot, setup.InputHashes, allowAdditionalFiles: true);
            Assert.False(Directory.Exists(setup.ChildAddress.CheckpointRoot));
            AssertNoiseParentRemainsLoadable(setup);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task TrainAndPublishAsync_SufficientBaselineRefersBeforeTrainingOrPublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-noise-referral");
        try
        {
            var setup = CreateNoiseDecisionSetup(fixture, repositoryRoot, "referral", 5,
                [5, 5, 5, 5, 5]);
            var observed = await ObserveNoiseDecisionAsync(setup);
            AssertObservedQualitySeries(setup, [5, 5, 5, 5, 5]);
            Assert.Equal(0, observed.TrainerCalls);
            Assert.False(observed.Result.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BaselineAlreadySufficientErrorCode,
                observed.Result.ErrorCode);
            Assert.Equal("Исходная принятая модель уже выше целевой доли верных ответов с учётом действующей погрешности; новый урок не запускается, нужен отдельный итоговый экзамен.",
                observed.Result.ErrorMessage);
            Assert.Null(observed.Result.Value);
            AssertNoNoiseTrainingArtifacts(setup);
            AssertRetainedResumeFiles(repositoryRoot, setup.InputHashes);
            AssertNoiseParentRemainsLoadable(setup);
            // Exactly the diagnostic baseline was called; no trial/final-exam dispatch occurred here.
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("at-target")]
    [InlineData("above-target-within-noise")]
    public async Task BaselineRequiresNewTrainingPair_AtOrWithinTargetNoiseStillRequiresPair(string scenario)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-noise-" + scenario);
        try
        {
            int[] counts = scenario == "at-target" ? [19, 19, 19, 19, 19] : [19, 19, 19, 19, 20];
            var setup = CreateNoiseDecisionSetup(fixture, repositoryRoot, scenario, 20, counts);
            var baseline = await CalibrateNoiseBaselineAsync(setup, counts);
            var passport = baseline.Calibration.Calibration.MeasurementNoisePassport;
            Assert.Equal(scenario == "at-target" ? 0.95d : 0.96d, passport.MeanScore, 12);
            Assert.True(passport.MeanScore - 0.95d <= passport.NoiseFloor);
            var actual = ObserveBaselinePairDecision(setup, baseline, scenario);
            Assert.True(actual.IsSuccess, $"{actual.ErrorCode}: {actual.ErrorMessage}");
            Assert.True(actual.Value);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task BaselineRequiresNewTrainingPair_RejectsMissingBaseline()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-noise-missing");
        try
        {
            var setup = CreateNoiseDecisionSetup(fixture, repositoryRoot, "missing-baseline", 5,
                [0, 0, 0, 0, 0]);
            var baseline = await CalibrateNoiseBaselineAsync(setup, [0, 0, 0, 0, 0]);
            var control = ObserveBaselinePairDecision(setup, baseline, "valid-control");
            Assert.True(control.IsSuccess, control.ErrorMessage);
            Assert.True(control.Value);
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality?[] missing =
            [
                null,
                baseline with { Calibration = null! },
                baseline with { Calibration = baseline.Calibration with { Calibration = null! } },
                baseline with { Calibration = baseline.Calibration with
                    { Calibration = baseline.Calibration.Calibration with { MeasurementNoisePassport = null! } } }
            ];
            for (var index = 0; index < missing.Length; index++)
            {
                var actual = ObserveBaselinePairDecision(setup, missing[index], "missing-" + index);
                Assert.False(actual.IsSuccess);
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateInvalidErrorCode,
                    actual.ErrorCode);
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("nan")]
    [InlineData("finite-inconsistent")]
    public async Task BaselineRequiresNewTrainingPair_RejectsInvalidPassportFromPublicScoreMutation(string mutation)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryRoot("code-doc-noise-mutation-" + mutation);
        try
        {
            var setup = CreateNoiseDecisionSetup(fixture, repositoryRoot, "mutation-" + mutation, 5,
                [0, 0, 0, 0, 0]);
            var baseline = await CalibrateNoiseBaselineAsync(setup, [0, 0, 0, 0, 0]);
            var passport = baseline.Calibration.Calibration.MeasurementNoisePassport;
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(passport, out var validReason), validReason);
            var control = ObserveBaselinePairDecision(setup, baseline, "valid-control");
            Assert.True(control.IsSuccess, control.ErrorMessage);
            Assert.True(control.Value);
            var publicScores = Assert.IsType<double[]>(passport.RepeatedScores);
            Assert.Equal(new double[] { 0d, 0d, 0d, 0d, 0d }, publicScores);
            publicScores[0] = mutation == "nan" ? double.NaN : 0.2d;
            Assert.False(MeasurementNoisePolicy.TryValidatePassport(passport, out var invalidReason));
            Assert.Equal(mutation == "nan" ? MeasurementNoisePolicy.InvalidPassportStatus :
                "measurement_noise_passport_inconsistent", invalidReason);
            var comparison = MeasurementNoisePolicy.ClassifyDifference(passport.MeanScore - 0.95d, passport);
            Assert.Equal(MeasurementNoisePolicy.InvalidPassportStatus, comparison.Status);
            Assert.False(comparison.DifferenceProven);
            _output.WriteLine($"S03.06 public-score mutation={mutation}; validator={invalidReason}; classifier={comparison.Status}");

            var actual = ObserveBaselinePairDecision(setup, baseline, mutation);
            Assert.False(actual.IsSuccess,
                $"Invalid passport unexpectedly admitted: {actual.IsSuccess}/{actual.Value}/{actual.ErrorCode}");
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateInvalidErrorCode,
                actual.ErrorCode);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("improved", false)]
    [InlineData("improved", true)]
    [InlineData("regressed", false)]
    [InlineData("regressed", true)]
    [InlineData("equal", false)]
    [InlineData("equal", true)]
    [InlineData("trial-floor", false)]
    [InlineData("trial-floor", true)]
    [InlineData("baseline-floor", false)]
    [InlineData("baseline-floor", true)]
    [InlineData("positive-boundary", false)]
    [InlineData("positive-boundary", true)]
    [InlineData("negative-boundary", false)]
    [InlineData("negative-boundary", true)]
    [InlineData("identical", false)]
    [InlineData("identical", true)]
    public async Task DetailedQuality_RealSeriesRetainsGraphsAndWrapperMeasuresOnce(string scenario, bool wrapper)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("typed-quality-" + scenario + "-" + wrapper);
        try
        {
            var boundary = scenario.EndsWith("boundary", StringComparison.Ordinal);
            var heldOut = boundary ? 4 : 5;
            int[] baselineCounts = scenario switch
            {
                "regressed" => [4, 4, 4, 4, 4],
                "equal" => [2, 2, 2, 2, 2],
                "baseline-floor" => [0, 0, 0, 5, 5],
                "positive-boundary" => [2, 2, 2, 2, 2],
                "negative-boundary" => [4, 4, 4, 4, 4],
                _ => [0, 0, 0, 0, 0]
            };
            int[] trialCounts = scenario switch
            {
                "regressed" => [0, 0, 0, 0, 0],
                "equal" => [2, 2, 2, 2, 2],
                "trial-floor" => [0, 0, 0, 5, 5],
                "baseline-floor" => [5, 5, 5, 5, 5],
                "positive-boundary" => [4, 4, 4, 4, 4],
                "negative-boundary" => [2, 2, 2, 2, 2],
                _ => [4, 4, 4, 4, 4]
            };
            var setup = CreateNoiseDecisionSetup(fixture, root, "typed-" + scenario + "-" + (wrapper ? "wrapper" : "detailed"),
                heldOut, baselineCounts, trialCounts);
            var trial = await StageNumericDetailedTrialAsync(setup);
            var baseline = await CalibrateNoiseBaselineAsync(setup, baselineCounts, existingTrainingAttempt: true);
            var baselineBytes = JsonSerializer.SerializeToUtf8Bytes(baseline);
            var model = scenario == "identical"
                ? CloneModel(CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(setup.Request.StartRequest).Value!.Model)
                : trial.Model;
            var passport = CreateDetailedTrialPassport(setup, trial, model);
            var expected = scenario switch
            {
                "improved" => CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved,
                "regressed" => CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Regressed,
                "identical" => CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.IdenticalWeights,
                _ => CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Indistinguishable
            };
            var service = new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService();
            CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence? evidence;
            if (wrapper)
            {
                var result = await service.EvaluateTrialAsync(setup.Quality, baseline, model, passport,
                    "typed-wrapper-" + scenario, "typed-pair-" + scenario);
                Assert.Equal(expected == CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved, result.IsSuccess);
                if (!result.IsSuccess)
                {
                    Assert.Null(result.Value);
                    Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode, result.ErrorCode);
                    Assert.Equal(scenario == "identical"
                        ? "Пробные веса совпали с принятой моделью; такой результат не считается улучшением."
                        : "Пробная модель документации не показала убедительного улучшения по действующему правилу погрешности; принятая версия не изменена.", result.ErrorMessage);
                }
                evidence = result.Value;
            }
            else
            {
                var result = await service.EvaluateTrialDetailedAsync(setup.Quality, baseline, model, passport,
                    "typed-detailed-" + scenario, "typed-pair-" + scenario);
                Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage);
                var detailed = result.Value!;
                Assert.Equal(expected, detailed.Disposition);
                Assert.Same(baseline, detailed.Baseline);
                Assert.Same(baseline.Calibration.Calibration, detailed.Evidence.BaselineCalibration);
                Assert.Same(detailed.Trial.Calibration, detailed.Evidence.TrialCalibration);
                AssertDetailedGraphMatchesProducer(setup, baseline.Calibration, baselineCounts, offset: 0);
                AssertDetailedGraphMatchesProducer(setup, detailed.Trial, trialCounts, offset: heldOut * 5);
                Assert.Equal(ComputeCheckpointSha256(model), detailed.Trial.RepresentativeAssessment.ModelSerializationBeforeSha256);
                Assert.Equal(passport.WeightsByteLength, detailed.Trial.RepresentativeAssessment.ModelSerializationBeforeByteLength);
                Assert.Equal(scenario == "identical",
                    baseline.Calibration.RepresentativeAssessment.ModelSerializationBeforeSha256 ==
                    detailed.Trial.RepresentativeAssessment.ModelSerializationBeforeSha256);
                evidence = detailed.Evidence;
            }
            var actualScores = AssertObservedQualitySeries(setup, baselineCounts.Concat(trialCounts).ToArray());
            Assert.Equal(baselineBytes, JsonSerializer.SerializeToUtf8Bytes(baseline));
            _output.WriteLine("typed-quality-call-proof: " + JsonSerializer.Serialize(new
            {
                scenario, wrapper, ExpectedDisposition = expected.ToString(), ActualScores = actualScores,
                BaselineCalls = baseline.ObservedEvaluationCallCount,
                TrialCalls = ((ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient).IssuedCallCount - baseline.ObservedEvaluationCallCount,
                BaselineUnchanged = baselineBytes.SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(baseline)),
                Runtime = CoordinatorRuntimeIdentity()
            }));
            if (evidence is not null)
            {
                Assert.Equal(expected == CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved, evidence.Accepted);
                Assert.Equal(expected switch
                {
                    CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved => MeasurementNoisePolicy.ImprovedStatus,
                    CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Regressed => MeasurementNoisePolicy.RegressedStatus,
                    CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.IdenticalWeights => "identical_weights",
                    _ => MeasurementNoisePolicy.IndistinguishableStatus
                }, evidence.Status);
                Assert.Equal(actualScores.Skip(5).Average() - actualScores.Take(5).Average(), evidence.Difference);
                var before = evidence.BaselineCalibration.MeasurementNoisePassport;
                var after = evidence.TrialCalibration.MeasurementNoisePassport;
                var baselineComparison = MeasurementNoisePolicy.ClassifyDifference(evidence.Difference, before);
                var trialComparison = MeasurementNoisePolicy.ClassifyDifference(evidence.Difference, after);
                Assert.Contains("measurement-noise-status:baseline-floor:" + baselineComparison.Status, evidence.EvidenceRefs);
                Assert.Contains("measurement-noise-status:trial-floor:" + trialComparison.Status, evidence.EvidenceRefs);
                Assert.Equal(Math.Max(before.NoiseFloor, after.NoiseFloor), evidence.NoiseFloor);
                if (boundary)
                {
                    Assert.Equal(0.5d, evidence.NoiseFloor);
                    Assert.Equal(scenario == "positive-boundary" ? 0.5d : -0.5d, evidence.Difference);
                }
                if (scenario is "baseline-floor" or "trial-floor")
                {
                    Assert.Equal(MeasurementNoisePolicy.ImprovedStatus, scenario == "trial-floor" ? baselineComparison.Status : trialComparison.Status);
                    Assert.Equal(MeasurementNoisePolicy.IndistinguishableStatus, evidence.Status);
                }
                _output.WriteLine("typed-quality-complete: " + JsonSerializer.Serialize(new
                {
                    scenario, wrapper, Disposition = expected.ToString(), evidence.Status, evidence.Accepted,
                    evidence.Difference, evidence.NoiseFloor, evidence.ExpectedEvaluationCallCount, evidence.ObservedEvaluationCallCount,
                    Baseline = evidence.BaselineCalibration, Trial = evidence.TrialCalibration,
                    ActualProducerResponses = ((ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient).Observations,
                    ActualTrialWeightsSha256 = trial.WeightsSha256, ActualTrialWeightsByteLength = trial.WeightsByteLength
                }));
            }
            AssertNoiseParentRemainsLoadable(setup);
            Assert.False(File.Exists(setup.ChildAddress.WeightsPath));
            Assert.False(File.Exists(setup.ChildAddress.ManifestPath));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData("missing-id", false)]
    [InlineData("missing-id", true)]
    [InlineData("empty-id", false)]
    [InlineData("empty-id", true)]
    [InlineData("duplicate-id", false)]
    [InlineData("duplicate-id", true)]
    [InlineData("report-failed", false)]
    [InlineData("report-failed", true)]
    [InlineData("representative-failed", false)]
    [InlineData("representative-failed", true)]
    [InlineData("observed-count", false)]
    [InlineData("observed-count", true)]
    [InlineData("wrapper-count", false)]
    [InlineData("wrapper-count", true)]
    [InlineData("representative-ids", false)]
    [InlineData("representative-ids", true)]
    [InlineData("representative-passed", false)]
    [InlineData("representative-passed", true)]
    [InlineData("passed-count", false)]
    [InlineData("passed-count", true)]
    [InlineData("nonfinite-score", false)]
    [InlineData("nonfinite-score", true)]
    [InlineData("answer-count", false)]
    [InlineData("answer-count", true)]
    [InlineData("case-order", false)]
    [InlineData("case-order", true)]
    [InlineData("generation-hash", false)]
    [InlineData("generation-hash", true)]
    [InlineData("serialization-hash", false)]
    [InlineData("serialization-hash", true)]
    [InlineData("missing-passport", false)]
    [InlineData("missing-passport", true)]
    [InlineData("invalid-passport", false)]
    [InlineData("invalid-passport", true)]
    [InlineData("foreign-passport", false)]
    [InlineData("foreign-passport", true)]
    [InlineData("config-passport", false)]
    [InlineData("config-passport", true)]
    [InlineData("scores-passport", false)]
    [InlineData("scores-passport", true)]
    [InlineData("reordered-passport", false)]
    [InlineData("reordered-passport", true)]
    public async Task DetailedQuality_MalformedActualBaselineFailsBeforeIdenticalOrNegative(string mutation, bool identical)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("typed-invalid-" + mutation + "-" + identical);
        try
        {
            int[] counts = mutation == "reordered-passport" ? [0, 1, 2, 3, 4] : [0, 0, 0, 0, 0];
            var setup = CreateNoiseDecisionSetup(fixture, root, "invalid-" + mutation + "-" + (identical ? "identical" : "changed"), 5, counts, counts);
            var trial = await StageNumericDetailedTrialAsync(setup);
            var otherPassport = mutation is "foreign-passport" or "config-passport" or "scores-passport" or "reordered-passport"
                ? await ProduceOtherDetailedPassportAsync(setup, mutation, counts)
                : null;
            var original = await CalibrateNoiseBaselineAsync(setup, counts, existingTrainingAttempt: true);
            var baseline = MutateActualDetailedBaseline(original, mutation, otherPassport);
            var model = identical ? CloneModel(CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(setup.Request.StartRequest).Value!.Model) : trial.Model;
            var client = (ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient;
            var issuedBefore = client.IssuedCallCount;
            var result = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService()
                .EvaluateTrialDetailedAsync(setup.Quality, baseline, model, CreateDetailedTrialPassport(setup, trial, model),
                    "invalid-trial-" + mutation, "invalid-pair-" + mutation);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(mutation is "missing-passport" or "invalid-passport"
                ? CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateInvalidErrorCode
                : CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateConditionMismatchErrorCode, result.ErrorCode);
            Assert.Equal(issuedBefore, client.IssuedCallCount);
            Assert.Equal(issuedBefore, client.CompletedResponseCount);
            Assert.Equal(issuedBefore, client.Observations.Count);
            _output.WriteLine("typed-quality-invalid: " + JsonSerializer.Serialize(new
                { mutation, identical, result.ErrorCode, result.IsSuccess, ValueNull = result.Value is null, TrialCalls = client.IssuedCallCount - issuedBefore }));
            AssertNoiseParentRemainsLoadable(setup);
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData("incomplete", false)]
    [InlineData("incomplete", true)]
    [InlineData("evaluator", false)]
    [InlineData("evaluator", true)]
    [InlineData("cancelled", false)]
    [InlineData("cancelled", true)]
    [InlineData("cross-series-id", false)]
    [InlineData("cross-series-id", true)]
    [InlineData("baseline-mutates", false)]
    [InlineData("baseline-mutates", true)]
    public async Task DetailedQuality_ActualTrialFailurePreservesOriginalErrors(string failure, bool wrapper)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("typed-failure-" + failure + "-" + wrapper);
        try
        {
            int[] zero = [0, 0, 0, 0, 0];
            var setup = CreateNoiseDecisionSetup(fixture, root, "failure-" + failure + "-" + (wrapper ? "wrapper" : "detailed"), 5, zero, zero);
            var trial = await StageNumericDetailedTrialAsync(setup);
            var baseline = await CalibrateNoiseBaselineAsync(setup, zero, existingTrainingAttempt: true);
            var snapshot = JsonSerializer.SerializeToUtf8Bytes(baseline);
            var primary = (ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient;
            var outcomes = failure == "incomplete" ? new bool[5] : new bool[25];
            var trialClient = new ScriptedQualityGateEvaluationClient(setup.Quality.EvaluatorConfiguration, outcomes,
                failure == "cross-series-id" ? index => primary.Observations[index - 1].RequestId : index => "failure-trial-" + index,
                (index, response) =>
                {
                    if (failure == "baseline-mutates" && index == 25)
                        Assert.IsType<double[]>(baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = double.NaN;
                    return failure == "evaluator"
                        ? response with { InferenceProfile = LocalQwenOllamaInferenceProfileAuthority.Authoring }
                        : response;
                });
            var quality = setup.Quality with { EvaluationClient = trialClient };
            using var cancellation = new CancellationTokenSource();
            if (failure == "cancelled") cancellation.Cancel();
            var passport = CreateDetailedTrialPassport(setup, trial, trial.Model);
            string? error;
            if (wrapper)
            {
                var result = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialAsync(
                    quality, baseline, trial.Model, passport, "failure-wrapper", "failure-pair", cancellation.Token);
                Assert.False(result.IsSuccess);
                Assert.Null(result.Value);
                error = result.ErrorCode;
            }
            else
            {
                var result = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(
                    quality, baseline, trial.Model, passport, "failure-detailed", "failure-pair", cancellation.Token);
                Assert.False(result.IsSuccess);
                Assert.Null(result.Value);
                error = result.ErrorCode;
            }
            Assert.Equal(failure switch
            {
                "incomplete" => "SCRIPTED_QUALITY_GATE_DECISION_MISSING",
                "evaluator" => CodeDocumentationSavedModelAssessmentService.EvaluationProfileInvalidErrorCode,
                "cancelled" => CodeDocumentationSavedModelAssessmentService.CancelledErrorCode,
                "baseline-mutates" => CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateInvalidErrorCode,
                _ => CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateConditionMismatchErrorCode
            }, error);
            Assert.Equal(failure switch { "incomplete" => 6, "evaluator" => 1, "cancelled" => 0, _ => 25 }, trialClient.IssuedCallCount);
            Assert.Equal(25, primary.IssuedCallCount);
            if (failure != "baseline-mutates") Assert.Equal(snapshot, JsonSerializer.SerializeToUtf8Bytes(baseline));
            _output.WriteLine("typed-quality-trial-failure: " + JsonSerializer.Serialize(new
                { failure, wrapper, error, TrialCalls = trialClient.IssuedCallCount, BaselineCalls = primary.IssuedCallCount }));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData("improved", "Improved", 0.8d)]
    [InlineData("regressed", "Regressed", -0.8d)]
    [InlineData("equal", "Indistinguishable", 0d)]
    [InlineData("identical", "IdenticalWeights", 0.8d)]
    [InlineData("trial-floor", "Indistinguishable", 0.4d)]
    [InlineData("baseline-floor", "Indistinguishable", 0.6d)]
    [InlineData("positive-boundary", "Indistinguishable", 0.5d)]
    [InlineData("negative-boundary", "Indistinguishable", -0.5d)]
    public async Task PureQuality_CompletedActualGraphsKeepLiteralDispositionAndHaveNoEffects(string scenario, string expected, double difference)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("pure-valid-" + scenario);
        try
        {
            var boundary = scenario.EndsWith("boundary", StringComparison.Ordinal);
            int[] before = scenario switch
            {
                "regressed" => [4, 4, 4, 4, 4], "equal" => [2, 2, 2, 2, 2],
                "baseline-floor" => [0, 0, 0, 5, 5], "positive-boundary" => [2, 2, 2, 2, 2],
                "negative-boundary" => [4, 4, 4, 4, 4], _ => [0, 0, 0, 0, 0]
            };
            int[] after = scenario switch
            {
                "regressed" => [0, 0, 0, 0, 0], "equal" => [2, 2, 2, 2, 2],
                "trial-floor" => [0, 0, 0, 5, 5], "baseline-floor" => [5, 5, 5, 5, 5],
                "negative-boundary" => [2, 2, 2, 2, 2], _ => [4, 4, 4, 4, 4]
            };
            var setup = CreateNoiseDecisionSetup(fixture, root, "pure-" + scenario, boundary ? 4 : 5, before, after);
            var trial = await StageNumericDetailedTrialAsync(setup);
            var baseline = await CalibrateNoiseBaselineAsync(setup, before, existingTrainingAttempt: true);
            var model = scenario == "identical" ? CloneModel(CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(setup.Request.StartRequest).Value!.Model) : trial.Model;
            var passport = CreateDetailedTrialPassport(setup, trial, model);
            var measured = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(
                setup.Quality, baseline, model, passport, "pure-trial-" + scenario, "pure-pair-" + scenario);
            Assert.True(measured.IsSuccess, measured.ErrorCode + ": " + measured.ErrorMessage);
            var graph = measured.Value!.Trial;
            AssertDetailedGraphMatchesProducer(setup, baseline.Calibration, before, 0);
            AssertDetailedGraphMatchesProducer(setup, graph, after, setup.HeldOutCaseCount * 5);
            var sentinel = new CompletedOnlyEvaluationClient(setup.Quality.EvaluatorConfiguration);
            var request = CreatePureTrialRequest(setup.Quality, model, passport, "pure-trial-" + scenario, sentinel);
            var actual = ComparePureAndAssertUnchanged(setup, baseline, graph, request, "pure-pair-" + scenario, sentinel, scenario);
            Assert.True(actual.IsSuccess, actual.ErrorCode + ": " + actual.ErrorMessage);
            var result = actual.Value!;
            Assert.Equal(expected, result.Disposition.ToString());
            Assert.Same(baseline, result.Baseline);
            Assert.Same(graph, result.Trial);
            Assert.Same(baseline.Calibration.Calibration, result.Evidence.BaselineCalibration);
            Assert.Same(graph.Calibration, result.Evidence.TrialCalibration);
            Assert.Equal(JsonSerializer.Serialize(measured.Value.Evidence), JsonSerializer.Serialize(result.Evidence));
            Assert.Equal(difference, result.Evidence.Difference);
            Assert.Equal(expected == "Improved", result.Evidence.Accepted);
            var status = expected switch { "Improved" => "improved_beyond_measurement_noise", "Regressed" => "regressed_beyond_measurement_noise",
                "IdenticalWeights" => "identical_weights", _ => "indistinguishable_within_measurement_noise" };
            Assert.Equal(status, result.Evidence.Status);
            var floorStatus = scenario is "equal" or "positive-boundary" or "negative-boundary" ? "indistinguishable_within_measurement_noise"
                : scenario == "regressed" ? "regressed_beyond_measurement_noise" : "improved_beyond_measurement_noise";
            var baselineStatus = scenario == "baseline-floor" ? "indistinguishable_within_measurement_noise"
                : scenario == "negative-boundary" ? "regressed_beyond_measurement_noise" : floorStatus;
            var trialStatus = scenario == "trial-floor" ? "indistinguishable_within_measurement_noise"
                : scenario == "positive-boundary" ? "improved_beyond_measurement_noise" : floorStatus;
            Assert.Equal(new[] { "code-documentation-quality-gate:baseline-calibrated", "code-documentation-quality-gate:trial-calibrated",
                "code-documentation-quality-gate:single-new-pair:pure-pair-" + scenario,
                "measurement-noise-status:baseline-floor:" + baselineStatus, "measurement-noise-status:trial-floor:" + trialStatus,
                "baseline-calibration-run:" + baseline.Calibration.Calibration.CalibrationRunId,
                "trial-calibration-run:" + graph.Calibration.CalibrationRunId }.Concat(request.EvidenceRefs), result.Evidence.EvidenceRefs);
            Assert.Equal(Math.Max(baseline.Calibration.Calibration.MeasurementNoisePassport.NoiseFloor,
                graph.Calibration.MeasurementNoisePassport.NoiseFloor), result.Evidence.NoiseFloor);
            if (boundary)
            {
                Assert.Equal(0.5d, result.Evidence.NoiseFloor);
                Assert.Equal(scenario == "positive-boundary" ? 0.5d : 0d, result.Evidence.BaselineCalibration.MeasurementNoisePassport.NoiseFloor);
                Assert.Equal(scenario == "positive-boundary" ? 0d : 0.5d, result.Evidence.TrialCalibration.MeasurementNoisePassport.NoiseFloor);
            }
            _output.WriteLine("pure-quality-classification: " + JsonSerializer.Serialize(new { scenario, expected,
                Disposition = result.Disposition.ToString(), result.Evidence.Status, result.Evidence.Accepted,
                result.Evidence.Difference, result.Evidence.NoiseFloor,
                BaselineFloor = result.Evidence.BaselineCalibration.MeasurementNoisePassport.NoiseFloor,
                TrialFloor = result.Evidence.TrialCalibration.MeasurementNoisePassport.NoiseFloor,
                BaselineStatus = baselineStatus, TrialStatus = trialStatus }));
            Assert.Equal(setup.HeldOutCaseCount * 10, result.Evidence.ExpectedEvaluationCallCount);
            Assert.Equal(setup.HeldOutCaseCount * 10, result.Evidence.ObservedEvaluationCallCount);
            AssertNoiseParentRemainsLoadable(setup);
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData("missing-id", false)] [InlineData("missing-id", true)]
    [InlineData("empty-id", false)] [InlineData("empty-id", true)]
    [InlineData("duplicate-id", false)] [InlineData("duplicate-id", true)]
    [InlineData("report-failed", false)] [InlineData("report-failed", true)]
    [InlineData("representative-failed", false)] [InlineData("representative-failed", true)]
    [InlineData("observed-count", false)] [InlineData("observed-count", true)]
    [InlineData("answer-count", false)] [InlineData("answer-count", true)]
    [InlineData("case-order", false)] [InlineData("case-order", true)]
    [InlineData("generation-hash", false)] [InlineData("generation-hash", true)]
    [InlineData("serialization-hash", false)] [InlineData("serialization-hash", true)]
    [InlineData("missing-passport", false)] [InlineData("missing-passport", true)]
    [InlineData("invalid-passport", false)] [InlineData("invalid-passport", true)]
    [InlineData("foreign-passport", false)] [InlineData("foreign-passport", true)]
    [InlineData("config-passport", false)] [InlineData("config-passport", true)]
    [InlineData("scores-passport", false)] [InlineData("scores-passport", true)]
    [InlineData("reordered-passport", false)] [InlineData("reordered-passport", true)]
    [InlineData("source-kind", false)] [InlineData("source-kind", true)]
    [InlineData("root-run", false)] [InlineData("root-run", true)]
    [InlineData("repeat-run", false)] [InlineData("repeat-run", true)]
    [InlineData("missing-observations", false)] [InlineData("missing-observations", true)]
    public async Task PureQuality_MalformedActualGraphIsFailureWithoutNewEvaluation(string mutation, bool mutateTrial)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("pure-graph-" + mutation + "-" + mutateTrial);
        try
        {
            int[] counts = mutation == "reordered-passport" ? [0, 1, 2, 3, 4] : [0, 0, 0, 0, 0];
            var setup = CreateNoiseDecisionSetup(fixture, root, "pure-graph-" + mutation + "-" + (mutateTrial ? "trial" : "baseline"), 5, counts, counts);
            var trial = await StageNumericDetailedTrialAsync(setup);
            var other = mutation is "foreign-passport" or "config-passport" or "scores-passport" or "reordered-passport"
                ? await ProduceOtherDetailedPassportAsync(setup, mutation, counts) : null;
            var baseline = await CalibrateNoiseBaselineAsync(setup, counts, existingTrainingAttempt: true);
            var passport = CreateDetailedTrialPassport(setup, trial, trial.Model);
            var measured = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(
                setup.Quality, baseline, trial.Model, passport, "pure-graph-trial", "pure-graph-pair");
            Assert.True(measured.IsSuccess, measured.ErrorMessage);
            var graph = measured.Value!.Trial;
            var subject = mutateTrial ? new CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality(graph, 25, 25) : baseline;
            subject = JsonSerializer.Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>(JsonSerializer.Serialize(subject))!;
            var malformed = MutatePureActualGraph(subject, mutation, other);
            if (mutateTrial) graph = malformed.Calibration; else baseline = malformed;
            var sentinel = new CompletedOnlyEvaluationClient(setup.Quality.EvaluatorConfiguration);
            var request = CreatePureTrialRequest(setup.Quality, trial.Model, passport, "pure-graph-trial", sentinel);
            var actual = ComparePureAndAssertUnchanged(setup, baseline, graph, request, "pure-graph-pair", sentinel,
                "graph-" + mutation + "-" + mutateTrial);
            Assert.False(actual.IsSuccess);
            Assert.Null(actual.Value);
            Assert.Equal(mutation is "missing-passport" or "invalid-passport"
                ? CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateInvalidErrorCode
                : CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateConditionMismatchErrorCode, actual.ErrorCode);
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData("source-kind")] [InlineData("root-run")] [InlineData("repeat-run")]
    public async Task PureQuality_MalformedOriginalSourceRunIsRejectedBeforeAsyncTrialCalls(string mutation)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("pure-before-await-" + mutation);
        try
        {
            int[] counts = [0, 0, 0, 0, 0];
            var setup = CreateNoiseDecisionSetup(fixture, root, "pure-before-await-" + mutation, 5, counts, counts);
            var trial = await StageNumericDetailedTrialAsync(setup);
            var baseline = await CalibrateNoiseBaselineAsync(setup, counts, existingTrainingAttempt: true);
            var malformed = MutatePureActualGraph(baseline, mutation, null);
            var primary = (ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient;
            var calls = primary.IssuedCallCount;
            var sentinel = new CompletedOnlyEvaluationClient(setup.Quality.EvaluatorConfiguration);
            var result = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(
                setup.Quality with { EvaluationClient = sentinel }, malformed, trial.Model,
                CreateDetailedTrialPassport(setup, trial, trial.Model), "pure-before-await-trial", "pure-before-await-pair");
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateConditionMismatchErrorCode, result.ErrorCode);
            Assert.Equal(0, sentinel.Attempts);
            Assert.Equal(calls, primary.IssuedCallCount);
            _output.WriteLine("pure-quality-pretrial: " + JsonSerializer.Serialize(new { mutation, result.IsSuccess,
                result.ErrorCode, ValueNull = result.Value is null, TrialAttempts = sentinel.Attempts, BaselineCalls = calls }));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData("null-request")] [InlineData("null-model")] [InlineData("null-tokenizer")]
    [InlineData("null-client")] [InlineData("null-passport")] [InlineData("null-corpus")]
    [InlineData("null-manifest")] [InlineData("null-config")] [InlineData("null-generation")]
    [InlineData("null-policy")] [InlineData("utc")] [InlineData("evidence")]
    [InlineData("root-invalid")] [InlineData("source-invalid")] [InlineData("pair-invalid")]
    [InlineData("exam")] [InlineData("scorer")] [InlineData("source-path")]
    [InlineData("source-identity")] [InlineData("source-manifest")] [InlineData("source-weights")]
    [InlineData("source-length")] [InlineData("current-root")] [InlineData("baseline-null")]
    [InlineData("trial-null")] [InlineData("wrapper-count")] [InlineData("cross-series-id")]
    [InlineData("alternate-pair")]
    [InlineData("matched-empty-path")] [InlineData("matched-unsafe-path")] [InlineData("matched-nonhex-identity")]
    [InlineData("matched-manifest")] [InlineData("matched-nonhex-weights")] [InlineData("matched-zero-length")]
    [InlineData("generation-invalid")] [InlineData("manifest-invalid")] [InlineData("zero-time")]
    public async Task PureQuality_CurrentRequestAndSeriesBindingsFailClosed(string mutation)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("pure-current-" + mutation);
        try
        {
            int[] counts = [0, 0, 0, 0, 0];
            var setup = CreateNoiseDecisionSetup(fixture, root, "pure-current-" + mutation, 5, counts, counts);
            var trial = await StageNumericDetailedTrialAsync(setup);
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality? baseline = await CalibrateNoiseBaselineAsync(setup, counts, existingTrainingAttempt: true);
            var passport = CreateDetailedTrialPassport(setup, trial, trial.Model);
            var measured = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(
                setup.Quality, baseline, trial.Model, passport, "pure-current-trial", "pure-current-pair");
            Assert.True(measured.IsSuccess, measured.ErrorMessage);
            CodeDocumentationSavedModelAssessmentCalibrationResult? graph = measured.Value!.Trial;
            var sentinel = new CompletedOnlyEvaluationClient(setup.Quality.EvaluatorConfiguration);
            CodeDocumentationSavedModelAssessmentRequest? request = CreatePureTrialRequest(setup.Quality, trial.Model, passport, "pure-current-trial", sentinel);
            MeasurementNoisePolicyConfig? policy = setup.Quality.NoisePolicyConfig;
            var pair = "pure-current-pair";
            switch (mutation)
            {
                case "null-request": request = null; break;
                case "null-model": request = request with { Model = null! }; break;
                case "null-tokenizer": request = request with { Tokenizer = null! }; break;
                case "null-client": request = request with { EvaluationClient = null! }; break;
                case "null-passport": request = request with { ModelPassport = null! }; break;
                case "null-corpus": request = request with { Corpus = null! }; break;
                case "null-manifest": request = request with { CorpusManifest = null! }; break;
                case "null-config": request = request with { EvaluatorConfiguration = null! }; break;
                case "null-generation": request = request with { GenerationSettings = null! }; break;
                case "null-policy": policy = null; break;
                case "utc": request = request with { ObservedAtUtc = request.ObservedAtUtc.ToOffset(TimeSpan.FromHours(1)) }; break;
                case "zero-time": request = request with { ObservedAtUtc = default }; break;
                case "generation-invalid": request = request with { GenerationSettings = request.GenerationSettings with { Temperature = float.NaN } }; break;
                case "manifest-invalid": request = request with { CorpusManifest = request.CorpusManifest with { EntryCount = request.CorpusManifest.EntryCount + 1 } }; break;
                case "evidence": request = request with { EvidenceRefs = ["C:/private/evidence"] }; break;
                case "root-invalid": request = request with { AssessmentRunId = new string('x', 246) }; break;
                case "source-invalid": request = request with { ModelPassport = passport with { ModelSourceKind = "foreign" } }; break;
                case "pair-invalid": pair = " bad-pair "; break;
                case "exam": request = request with { GenerationSettings = request.GenerationSettings with { Seed = 18 } }; break;
                case "scorer": request = request with { EvaluatorConfiguration = CreateTeacherConfiguration(50, manifestDigest: Hex('e')) }; break;
                case "source-path": request = request with { ModelPassport = passport with { RepositoryRelativeModelIdentityPath = "other/model.bin" } }; break;
                case "source-identity": request = request with { ModelPassport = passport with { ModelIdentitySha256 = new string('a', 64) } }; break;
                case "source-manifest": request = request with { ModelPassport = passport with { ManifestSha256 = new string('a', 64) } }; break;
                case "source-weights": request = request with { ModelPassport = passport with { WeightsSha256 = new string('a', 64) } }; break;
                case "source-length": request = request with { ModelPassport = passport with { WeightsByteLength = passport.WeightsByteLength + 1 } }; break;
                case "current-root": request = request with { AssessmentRunId = "different-current-trial" }; break;
                case "baseline-null": baseline = null; break;
                case "trial-null": graph = null; break;
                case "wrapper-count": baseline = baseline with { ExpectedEvaluationCallCount = 24 }; break;
                case "cross-series-id":
                    var observations = graph.Calibration.Observations.ToArray();
                    observations[1] = observations[1] with { ObservedEvaluationRequestIds = baseline.Calibration.Calibration.Observations[1].ObservedEvaluationRequestIds };
                    graph = graph with { Calibration = graph.Calibration with { Observations = observations } }; break;
                case "alternate-pair": pair = "another-well-formed-label"; break;
                case "matched-empty-path":
                case "matched-unsafe-path":
                case "matched-nonhex-identity":
                case "matched-manifest":
                case "matched-nonhex-weights":
                case "matched-zero-length":
                    var bad = mutation switch
                    {
                        "matched-empty-path" => passport with { RepositoryRelativeModelIdentityPath = "" },
                        "matched-unsafe-path" => passport with { RepositoryRelativeModelIdentityPath = "../outside.bin" },
                        "matched-nonhex-identity" => passport with { ModelIdentitySha256 = new string('z', 64) },
                        "matched-manifest" => passport with { ManifestSha256 = new string('a', 64) },
                        "matched-nonhex-weights" => passport with { WeightsSha256 = new string('z', 64) },
                        _ => passport with { WeightsByteLength = 0 }
                    };
                    request = request with { ModelPassport = bad };
                    graph = graph with { RepresentativeAssessment = graph.RepresentativeAssessment with
                    {
                        RepositoryRelativeModelIdentityPath = bad.RepositoryRelativeModelIdentityPath,
                        ModelIdentitySha256 = bad.ModelIdentitySha256, ModelManifestSha256 = bad.ManifestSha256,
                        SavedWeightsSha256 = bad.WeightsSha256, ModelSerializationBeforeSha256 = bad.WeightsSha256,
                        ModelSerializationAfterSha256 = bad.WeightsSha256, ModelSerializationBeforeByteLength = bad.WeightsByteLength,
                        ModelSerializationAfterByteLength = bad.WeightsByteLength
                    } }; break;
                default: throw new ArgumentOutOfRangeException(nameof(mutation));
            }
            var beforePrimary = ((ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient).IssuedCallCount;
            var baselineBytes = JsonSerializer.SerializeToUtf8Bytes(baseline);
            var trialBytes = JsonSerializer.SerializeToUtf8Bytes(graph);
            var result = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.CompareCompletedTrialDetailed(request, policy, baseline, graph, pair);
            Assert.Equal(0, sentinel.Attempts);
            Assert.Equal(beforePrimary, ((ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient).IssuedCallCount);
            Assert.Equal(baselineBytes, JsonSerializer.SerializeToUtf8Bytes(baseline));
            Assert.Equal(trialBytes, JsonSerializer.SerializeToUtf8Bytes(graph));
            Assert.Equal(mutation == "alternate-pair", result.IsSuccess);
            if (mutation == "alternate-pair") Assert.Equal(pair, result.Value!.Evidence.TrainingPairId);
            else
            {
                Assert.Null(result.Value);
                var invalid = mutation.StartsWith("null-", StringComparison.Ordinal) || mutation.StartsWith("matched-", StringComparison.Ordinal)
                    || mutation is "utc" or "evidence" or "root-invalid" or "source-invalid" or "source-manifest" or "pair-invalid" or "generation-invalid" or "manifest-invalid" or "zero-time";
                Assert.Equal(invalid ? CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateInvalidErrorCode
                    : CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateConditionMismatchErrorCode, result.ErrorCode);
            }
            _output.WriteLine("pure-quality-current: " + JsonSerializer.Serialize(new { mutation, result.IsSuccess, result.ErrorCode,
                ValueNull = result.Value is null, SentinelAttempts = sentinel.Attempts, ProducerCalls = beforePrimary,
                ResultPair = result.Value?.Evidence.TrainingPairId, DescriptivePairOnly = mutation == "alternate-pair" }));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData(1, 5000, "Improved")]
    [InlineData(2, 2500, "Improved")]
    [InlineData(200, 25, "Improved")]
    public async Task PureQuality_ActualMaximumCompleteSeriesKeepEveryCaseRepeatAndFallbackRun(int n, int repeats, string expected)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var root = CreateRepositoryRoot("pure-max-" + n);
        try
        {
            // Staging is a separate small genuine numeric attempt: 25+25 calls, never the maximum target series.
            var staging = CreateNoiseDecisionSetup(fixture, root, "pure-max-stage-" + n, 5, [0, 0, 0, 0, 0], [0, 0, 0, 0, 0]);
            var trial = await StageNumericDetailedTrialAsync(staging);
            var targetCorpus = SealQualityReferenceCorpus(root, "v2-pure-maximum-" + n,
                Enumerable.Range(1, n).Select(i => CreateQualityReferenceEntry("max case " + i, "max ok " + i,
                    FrozenReferencePartition.HeldOut, "pure-max-heldout-" + i)).Prepend(
                    CreateQualityReferenceEntry("max train", "max train ok", FrozenReferencePartition.Train, "pure-max-train")));
            var config = CreateTeacherConfiguration(10000);
            var client = new ScriptedQualityGateEvaluationClient(config, new bool[5000].Concat(Enumerable.Repeat(true, 5000)).ToArray(),
                i => "pure-max-" + n + "-actual-" + i);
            var quality = staging.Quality with { Corpus = targetCorpus.Corpus, CorpusManifest = targetCorpus.Manifest,
                EvaluationClient = client, EvaluatorConfiguration = config, NoisePolicyConfig = new MeasurementNoisePolicyConfig(repeats, 2d),
                GenerationSettings = staging.Quality.GenerationSettings with { MaxHeldOutCases = n }, MaximumDuration = TimeSpan.FromSeconds(120) };
            var accepted = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(staging.Request.StartRequest);
            Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
            var baselineRoot = n == 1 ? new string('b', 245) : "pure-max-baseline-" + n;
            var trialRoot = n == 1 ? new string('t', 245) : "pure-max-trial-" + n;
            var baselineResult = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().CalibrateBaselineAsync(
                quality, accepted.Value!.Model, CreateAcceptedPassportForTest(accepted.Value.ParentManifest!, staging.Request.StartRequest.MaximumCheckpointBytes), baselineRoot);
            Assert.True(baselineResult.IsSuccess, baselineResult.ErrorCode + ": " + baselineResult.ErrorMessage);
            var baseline = baselineResult.Value!;
            var passport = CreateDetailedTrialPassport(staging, trial, trial.Model);
            var targetRequest = CreatePureTrialRequest(quality, trial.Model, passport, trialRoot, client);
            var completed = await new CodeDocumentationSavedModelAssessmentCalibrationService().CalibrateAsync(targetRequest,
                quality.NoisePolicyConfig, quality.MaximumDuration);
            Assert.True(completed.IsSuccess, completed.ErrorCode + ": " + completed.ErrorMessage);
            var graph = completed.Value!;
            var targetSetup = staging with { HeldOutCaseCount = n, Quality = quality };
            AssertDetailedGraphMatchesProducer(targetSetup, baseline.Calibration, Enumerable.Repeat(0, repeats).ToArray(), 0);
            AssertDetailedGraphMatchesProducer(targetSetup, graph, Enumerable.Repeat(n, repeats).ToArray(), 5000);
            Assert.Equal(10000, client.IssuedCallCount);
            Assert.Equal(10000, client.CompletedResponseCount);
            Assert.Equal(10000, client.Observations.Select(x => x.RequestId).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(5000, baseline.ExpectedEvaluationCallCount);
            Assert.Equal(5000, graph.Calibration.ObservedEvaluationCallCount);
            if (n == 1)
            {
                foreach (var (series, sourceRoot) in new[] { (baseline.Calibration, baselineRoot), (graph, trialRoot) })
                {
                    Assert.Equal("saved-model-calibration:" + sourceRoot, series.Calibration.CalibrationRunId);
                    Assert.True(series.Calibration.CalibrationRunId.Length > 256);
                    Assert.Equal(sourceRoot + "-repeat-999", series.Calibration.Observations[998].AssessmentRunId);
                    var hash16 = Sha256Hex(sourceRoot)[..16];
                    Assert.Equal(sourceRoot[..220] + "-sha256-" + hash16 + "-repeat-1000", series.Calibration.Observations[999].AssessmentRunId);
                    Assert.Equal(sourceRoot[..220] + "-sha256-" + hash16 + "-repeat-5000", series.Calibration.Observations[4999].AssessmentRunId);
                }
            }
            var sentinel = new CompletedOnlyEvaluationClient(config);
            var pure = ComparePureAndAssertUnchanged(targetSetup, baseline, graph, targetRequest with { EvaluationClient = sentinel },
                "pure-max-pair", sentinel, "maximum-" + n + "x" + repeats);
            Assert.True(pure.IsSuccess, pure.ErrorCode + ": " + pure.ErrorMessage);
            Assert.Equal(expected, pure.Value!.Disposition.ToString());
            Assert.Equal(1d, pure.Value.Evidence.Difference);
            Assert.Equal(0d, pure.Value.Evidence.NoiseFloor);
            Assert.Same(baseline, pure.Value.Baseline);
            Assert.Same(graph, pure.Value.Trial);
            _output.WriteLine("pure-quality-maximum: " + JsonSerializer.Serialize(new { n, repeats, TargetBaselineCalls = 5000,
                TargetTrialCalls = 5000, ConfiguredTargetBudget = 10000, SeparatelyAccountedNumericStagingCalls = 50,
                PureAdditionalCalls = sentinel.Attempts, ActualTargetCalls = client.IssuedCallCount,
                ActualBaselineRepeat999 = n == 1 ? baseline.Calibration.Calibration.Observations[998].AssessmentRunId : null,
                ActualTrialRepeat1000 = n == 1 ? graph.Calibration.Observations[999].AssessmentRunId : null,
                ActualTrialRepeat5000 = n == 1 ? graph.Calibration.Observations[4999].AssessmentRunId : null,
                Disposition = pure.Value.Disposition.ToString(), pure.Value.Evidence.NoiseFloor, pure.Value.Evidence.Difference,
                BaselineFloor = pure.Value.Evidence.BaselineCalibration.MeasurementNoisePassport.NoiseFloor,
                TrialFloor = pure.Value.Evidence.TrialCalibration.MeasurementNoisePassport.NoiseFloor }));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    private static CodeDocumentationSavedModelAssessmentRequest CreatePureTrialRequest(
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext quality, LlmModel model,
        CodeDocumentationSavedModelAssessmentModelPassport passport, string root, ILegalTeacherEvaluationInferenceClient client)
        => new(model, quality.Tokenizer, passport, quality.Corpus, quality.CorpusManifest, client, quality.EvaluatorConfiguration,
            quality.GenerationSettings, root, quality.ObservedAtUtc, quality.EvidenceRefs);

    private OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult> ComparePureAndAssertUnchanged(
        NoiseDecisionSetup setup, CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality baseline,
        CodeDocumentationSavedModelAssessmentCalibrationResult graph, CodeDocumentationSavedModelAssessmentRequest request,
        string pair, CompletedOnlyEvaluationClient sentinel, string scenario)
    {
        // Observe serialization and disk bytes outside the pure call, with real source models and previously produced graphs.
        var client = (ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient;
        var calls = client.IssuedCallCount;
        var responses = JsonSerializer.SerializeToUtf8Bytes(client.Observations);
        var graphOptions = new JsonSerializerOptions { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals };
        var before = JsonSerializer.SerializeToUtf8Bytes(baseline, graphOptions);
        var after = JsonSerializer.SerializeToUtf8Bytes(graph, graphOptions);
        var modelHash = ComputeCheckpointSha256(request.Model);
        var checkpoint = SnapshotResumeFiles(setup.RepositoryRoot);
        var result = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.CompareCompletedTrialDetailed(
            request, setup.Quality.NoisePolicyConfig, baseline, graph, pair);
        Assert.Equal(0, sentinel.Attempts);
        Assert.Equal(calls, client.IssuedCallCount);
        Assert.Equal(calls, client.CompletedResponseCount);
        Assert.Equal(responses, JsonSerializer.SerializeToUtf8Bytes(client.Observations));
        Assert.Equal(before, JsonSerializer.SerializeToUtf8Bytes(baseline, graphOptions));
        Assert.Equal(after, JsonSerializer.SerializeToUtf8Bytes(graph, graphOptions));
        Assert.Equal(modelHash, ComputeCheckpointSha256(request.Model));
        Assert.Equal(checkpoint, SnapshotResumeFiles(setup.RepositoryRoot));
        _output.WriteLine("pure-quality-proof: " + JsonSerializer.Serialize(new { scenario, result.IsSuccess, result.ErrorCode,
            ValueNull = result.Value is null, Disposition = result.Value?.Disposition.ToString(), SentinelAttempts = sentinel.Attempts,
            ProducerCallsBefore = calls, ProducerCallsAfter = client.IssuedCallCount,
            BaselineBytes = before.Length, BaselineSha256 = Convert.ToHexString(SHA256.HashData(before)).ToLowerInvariant(),
            TrialBytes = after.Length, TrialSha256 = Convert.ToHexString(SHA256.HashData(after)).ToLowerInvariant(),
            ProducerResponsesSha256 = Convert.ToHexString(SHA256.HashData(responses)).ToLowerInvariant(),
            BaselineRepeats = baseline.Calibration.Calibration.Observations?.Count, TrialRepeats = graph.Calibration.Observations?.Count,
            CasesPerRepeat = setup.HeldOutCaseCount, ModelSha256 = modelHash, GraphsAndCheckpointUnchanged = true,
            Runtime = CoordinatorRuntimeIdentity() }));
        return result;
    }

    private static CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality MutatePureActualGraph(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality original, string mutation, MeasurementNoisePassport? other)
    {
        var report = original.Calibration.Calibration;
        var representative = original.Calibration.RepresentativeAssessment;
        var observations = report.Observations.ToArray();
        switch (mutation)
        {
            case "source-kind": representative = representative with { ModelSourceKind = "foreign-source" }; break;
            case "root-run": report = report with { CalibrationRunId = "saved-model-calibration:foreign-root" }; break;
            case "repeat-run": observations[1] = observations[1] with { AssessmentRunId = "foreign-second-repeat" }; break;
            case "missing-observations": report = report with { Observations = null! }; break;
            default: return MutateActualDetailedBaseline(original, mutation, other);
        }
        return original with { Calibration = original.Calibration with { RepresentativeAssessment = representative,
            Calibration = mutation == "missing-observations" ? report : report with { Observations = observations } } };
    }

    private sealed class CompletedOnlyEvaluationClient(ContinuousLearningTp1DpoConfiguration configuration) : ILegalTeacherEvaluationInferenceClient
    {
        public string ModelId => configuration.ModelId;
        public string ExpectedOllamaManifestDigest => configuration.ExpectedOllamaManifestDigest;
        public string ExpectedOllamaModelLayerDigest => configuration.ExpectedOllamaModelLayerDigest;
        public int Attempts { get; private set; }
        public Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(string prompt,
            ILegalTeacherCallPermit callPermit, CancellationToken cancellationToken = default)
        {
            Attempts++;
            throw new InvalidOperationException("Completed-series comparison attempted a new evaluator effect.");
        }
        public void Dispose() { }
    }

    private static async Task<CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>
        StageNumericDetailedTrialAsync(NoiseDecisionSetup setup)
    {
        // The normal producer applies one real step into its unpublished attempt.
        // Its separate staging checker never consumes the assessment-under-test series.
        var total = setup.HeldOutCaseCount * setup.Quality.NoisePolicyConfig.RepeatCount * 2;
        var stagingClient = new ScriptedQualityGateEvaluationClient(setup.Quality.EvaluatorConfiguration,
            new bool[total], index => "typed-staging-" + index, null);
        var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(
            setup.Request with { QualityGate = setup.Quality with { EvaluationClient = stagingClient } });
        Assert.False(result.IsSuccess);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode, result.ErrorCode);
        Assert.Equal(total, stagingClient.IssuedCallCount);
        var loaded = CodeDocumentationRepositoryOwnedModelCheckpointStartService.LoadTrainingAttemptCheckpoint(
            setup.Request.StartRequest, setup.AttemptAddress, 1);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var trial = loaded.Value!;
        Assert.NotEqual(setup.ParentManifest.WeightsSha256, trial.WeightsSha256);
        Assert.Equal(ComputeFileSha256(trial.WeightsPath), trial.WeightsSha256);
        Assert.Equal(new FileInfo(trial.WeightsPath).Length, trial.WeightsByteLength);
        Assert.Equal(ComputeCheckpointSha256(trial.Model), trial.WeightsSha256);
        Assert.False(File.Exists(setup.ChildAddress.WeightsPath));
        return trial;
    }

    private static CodeDocumentationSavedModelAssessmentModelPassport CreateDetailedTrialPassport(
        NoiseDecisionSetup setup, CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad trial, LlmModel model)
    {
        using var bytes = new MemoryStream();
        model.SaveCheckpointToStream(bytes);
        var hash = Convert.ToHexString(SHA256.HashData(bytes.ToArray())).ToLowerInvariant();
        var weightsPath = trial.WeightsPath;
        if (!StringComparer.Ordinal.Equals(hash, trial.WeightsSha256))
        {
            weightsPath = Path.Combine(setup.AttemptAddress.AttemptRoot, "identical-model-test-checkpoint.bin");
            File.WriteAllBytes(weightsPath, bytes.ToArray());
        }
        Assert.Equal(hash, ComputeFileSha256(weightsPath));
        Assert.Equal(bytes.Length, new FileInfo(weightsPath).Length);
        return CreateAcceptedPassportForTest(setup.ParentManifest, setup.Request.StartRequest.MaximumCheckpointBytes) with
        {
            CheckpointStep = setup.ParentManifest.Step + 1,
            RepositoryRelativeManifestPath = string.Empty,
            ManifestSha256 = string.Empty,
            ModelSourceKind = CodeDocumentationSavedModelAssessmentModelPassport.TrialTrainingAttemptCheckpointSourceKind,
            RepositoryRelativeModelIdentityPath = Path.GetRelativePath(setup.RepositoryRoot, weightsPath).Replace('\\', '/'),
            ModelIdentitySha256 = hash,
            RepositoryRelativeWeightsPath = Path.GetRelativePath(setup.RepositoryRoot, weightsPath).Replace('\\', '/'),
            WeightsSha256 = hash,
            WeightsByteLength = bytes.Length
        };
    }

    private static void AssertDetailedGraphMatchesProducer(NoiseDecisionSetup setup,
        CodeDocumentationSavedModelAssessmentCalibrationResult graph, IReadOnlyList<int> counts, int offset)
    {
        var client = (ScriptedQualityGateEvaluationClient)setup.Quality.EvaluationClient;
        Assert.True(graph.Calibration.IsSuccess);
        Assert.True(graph.RepresentativeAssessment.IsSuccess);
        Assert.Equal(counts.Count, graph.Calibration.Observations.Count);
        Assert.Equal(counts.Select(count => count / (double)setup.HeldOutCaseCount), graph.Calibration.MeasurementNoisePassport.RepeatedScores);
        for (var repeat = 0; repeat < counts.Count; repeat++)
        {
            var observation = graph.Calibration.Observations[repeat];
            var actual = client.Observations.Skip(offset + repeat * setup.HeldOutCaseCount).Take(setup.HeldOutCaseCount).ToArray();
            Assert.Equal(setup.HeldOutCaseCount, observation.EvaluatedCaseCount);
            Assert.Equal(counts[repeat], observation.PassedCaseCount);
            Assert.Equal(counts[repeat] / (double)setup.HeldOutCaseCount, observation.CorrectFraction);
            Assert.Equal(actual.Select(item => item.CaseId), observation.CaseIds);
            Assert.Equal(actual.Select(item => item.RequestId), observation.ObservedEvaluationRequestIds);
            Assert.Equal(actual.Select(item => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(item.StudentAnswer))).ToLowerInvariant()), observation.StudentAnswerSha256);
            Assert.Equal(graph.RepresentativeAssessment.ModelSerializationBeforeSha256, observation.ModelSerializationBeforeSha256);
            Assert.Equal(observation.ModelSerializationBeforeSha256, observation.ModelSerializationAfterSha256);
            Assert.Equal(graph.RepresentativeAssessment.GenerationSettingsSha256, observation.GenerationSettingsSha256);
        }
        Assert.Equal(graph.Calibration.Observations[0].ObservedEvaluationRequestIds, graph.RepresentativeAssessment.Evaluator.ObservedEvaluationRequestIds);
        Assert.Equal(graph.Calibration.Observations[0].CaseIds, graph.RepresentativeAssessment.Cases.Select(item => item.CaseId));
        Assert.Equal(graph.Calibration.Observations[0].StudentAnswerSha256, graph.RepresentativeAssessment.Cases.Select(item => item.StudentAnswerSha256));
    }

    private static async Task<MeasurementNoisePassport> ProduceOtherDetailedPassportAsync(
        NoiseDecisionSetup setup, string mutation, IReadOnlyList<int> originalCounts)
    {
        var counts = mutation == "scores-passport" ? Enumerable.Repeat(5, 5).ToArray()
            : mutation == "reordered-passport" ? originalCounts.Reverse().ToArray()
            : originalCounts.ToArray();
        var client = new ScriptedQualityGateEvaluationClient(setup.Quality.EvaluatorConfiguration,
            CreatePerRepeatQualityOutcomes(5, counts), index => "other-" + mutation + "-" + index);
        var context = setup.Quality with
        {
            EvaluationClient = client,
            GenerationSettings = mutation == "foreign-passport"
                ? setup.Quality.GenerationSettings with { Seed = 18 } : setup.Quality.GenerationSettings,
            NoisePolicyConfig = mutation == "config-passport"
                ? new MeasurementNoisePolicyConfig(5, 3d) : setup.Quality.NoisePolicyConfig
        };
        var accepted = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(setup.Request.StartRequest);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        var result = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().CalibrateBaselineAsync(
            context, accepted.Value!.Model, CreateAcceptedPassportForTest(accepted.Value.ParentManifest!,
                setup.Request.StartRequest.MaximumCheckpointBytes), "actual-other-" + mutation);
        Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage);
        Assert.Equal(25, client.IssuedCallCount);
        Assert.Equal(25, client.CompletedResponseCount);
        var graph = result.Value!.Calibration.Calibration;
        Assert.Equal(counts.Select(count => count / 5d), graph.Observations.Select(item => item.CorrectFraction));
        Assert.Equal(client.Observations.Select(item => item.RequestId), graph.Observations.SelectMany(item => item.ObservedEvaluationRequestIds));
        Assert.True(MeasurementNoisePolicy.TryValidatePassport(graph.MeasurementNoisePassport, out var reason), reason);
        return graph.MeasurementNoisePassport;
    }

    private static CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality MutateActualDetailedBaseline(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality original, string mutation,
        MeasurementNoisePassport? actualOtherPassport)
    {
        var report = original.Calibration.Calibration;
        var representative = original.Calibration.RepresentativeAssessment;
        var observations = report.Observations.ToArray();
        var first = observations[0];
        var passport = report.MeasurementNoisePassport;
        switch (mutation)
        {
            case "missing-id": observations[0] = first with { ObservedEvaluationRequestIds = first.ObservedEvaluationRequestIds.Skip(1).ToArray() }; break;
            case "empty-id": observations[0] = first with { ObservedEvaluationRequestIds = first.ObservedEvaluationRequestIds.Select((id, index) => index == 0 ? "" : id).ToArray() }; break;
            case "duplicate-id": observations[1] = observations[1] with { ObservedEvaluationRequestIds = first.ObservedEvaluationRequestIds }; break;
            case "report-failed": report = report with { IsSuccess = false }; break;
            case "representative-failed": representative = representative with { IsSuccess = false }; break;
            case "observed-count": report = report with { ObservedEvaluationCallCount = report.ObservedEvaluationCallCount - 1 }; break;
            case "wrapper-count": return original with { ObservedEvaluationCallCount = original.ObservedEvaluationCallCount - 1 };
            case "representative-ids": representative = representative with { Evaluator = representative.Evaluator with { ObservedEvaluationRequestIds = first.ObservedEvaluationRequestIds.Reverse().ToArray() } }; break;
            case "representative-passed": representative = representative with { PassedCaseCount = representative.PassedCaseCount + 1 }; break;
            case "passed-count": observations[0] = first with { PassedCaseCount = 1 }; break;
            case "nonfinite-score": observations[0] = first with { CorrectFraction = double.NaN }; break;
            case "answer-count": observations[0] = first with { StudentAnswerSha256 = first.StudentAnswerSha256.Skip(1).ToArray() }; break;
            case "case-order": observations[0] = first with { CaseIds = first.CaseIds.Reverse().ToArray() }; break;
            case "generation-hash": observations[0] = first with { GenerationSettingsSha256 = new string('b', 64) }; break;
            case "serialization-hash": observations[0] = first with { ModelSerializationAfterSha256 = new string('b', 64) }; break;
            case "missing-passport": report = report with { MeasurementNoisePassport = null! }; break;
            case "invalid-passport": Assert.IsType<double[]>(passport.RepeatedScores)[0] = double.NaN; break;
            case "foreign-passport":
            case "config-passport":
            case "scores-passport":
            case "reordered-passport":
                Assert.NotNull(actualOtherPassport);
                report = report with { MeasurementNoisePassport = actualOtherPassport! }; break;
            default: throw new ArgumentOutOfRangeException(nameof(mutation));
        }
        if (mutation is "foreign-passport" or "config-passport" or "scores-passport" or "reordered-passport")
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(report.MeasurementNoisePassport, out var reason), reason);
        return original with { Calibration = original.Calibration with { Calibration = report with { Observations = observations }, RepresentativeAssessment = representative } };
    }

    private static NoiseDecisionSetup CreateNoiseDecisionSetup(
        TinyTrainingFixtureCL fixture, string repositoryRoot, string scope, int heldOutCases,
        IReadOnlyList<int> baselinePassCounts, IReadOnlyList<int>? trialPassCounts = null)
    {
        var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
        var compatibility = CreateCompatibility(fixture.ModelConfig, "noise-" + scope);
        var parentDpo = SealRepositoryOwnedDpo(repositoryRoot, "noise-" + scope + "-parent",
            datasetVersion: "v0-parent");
        var dpo = SealRepositoryOwnedDpo(repositoryRoot, "noise-" + scope + "-lesson",
            datasetVersion: "v1-new");
        var entries = Enumerable.Range(1, heldOutCases)
            .Select(index => CreateQualityReferenceEntry("case " + index.ToString("D3"),
                "ok " + index.ToString("D3"), FrozenReferencePartition.HeldOut,
                "heldout-" + index.ToString("D3")))
            .Prepend(CreateQualityReferenceEntry("train 000", "ok train",
                FrozenReferencePartition.Train, "train"));
        var corpus = SealQualityReferenceCorpus(repositoryRoot, suppliedEntries: entries);
        var heldOut = corpus.Corpus.Entries.Where(entry => entry.Partition == FrozenReferencePartition.HeldOut)
            .ToArray();
        Assert.Equal(heldOutCases, heldOut.Length);
        Assert.Equal(heldOutCases, heldOut.Select(entry => entry.EntryId).Distinct(StringComparer.Ordinal).Count());
        var outcomes = CreatePerRepeatQualityOutcomes(heldOutCases, baselinePassCounts);
        if (trialPassCounts is not null)
            outcomes = outcomes.Concat(CreatePerRepeatQualityOutcomes(heldOutCases, trialPassCounts)).ToArray();
        Assert.Equal(heldOutCases * 5 * (trialPassCounts is null ? 1 : 2), outcomes.Length);
        var configuration = CreateTeacherConfiguration(maxTeacherCalls: heldOutCases * 5 * 2);
        var quality = CreateQualityGateContext(corpus, tokenizer, outcomes,
            index => "noise-" + scope + "-evaluation-" + index.ToString("D4"),
            evaluatorConfiguration: configuration);
        quality = quality with
        {
            GenerationSettings = quality.GenerationSettings with { MaxHeldOutCases = heldOutCases }
        };
        var plan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(
            quality.Corpus, quality.GenerationSettings, quality.EvaluatorConfiguration, quality.NoisePolicyConfig);
        Assert.True(plan.IsSuccess, plan.ErrorMessage);
        Assert.Equal(5, plan.Value!.BaselinePlan.RepeatCount);
        Assert.Equal(2d, quality.NoisePolicyConfig.StandardDeviationMultiplier);
        Assert.Equal(heldOutCases, plan.Value.BaselinePlan.HeldOutCaseCount);
        Assert.Equal(heldOutCases * 5, plan.Value.BaselinePlan.ExpectedEvaluationCallCount);
        Assert.Equal(heldOutCases * 10, plan.Value.ExpectedTotalEvaluationCallCount);
        Assert.Equal(heldOutCases * 10, quality.EvaluatorConfiguration.MaxTeacherCalls);
        var parentModel = fixture.CreateFactory().Create("CODE").Value!.Model;
        var parent = SealModelCheckpoint(repositoryRoot, parentModel, fixture.ModelConfig,
            compatibility, parentDpo.Identity, step: 1, parent: null,
            coldReason: "synthetic parent for noise decision " + scope);
        var start = CreateStartRequest(repositoryRoot, fixture.ModelConfig, compatibility,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
        var trainingRunId = "code-doc-noise:" + scope;
        var request = new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
            CreateRoutingRun(dpo.Receipt, dpo.Pairs), dpo.Address, start, tokenizer,
            start.LoraConfig!, CreateAcceptingBroker(), trainingRunId, DpoStepCount: 1,
            DateTimeOffset.UnixEpoch.AddDays(10), ["test:code-documentation-noise-" + scope], quality,
            OptimizerStateColdStartAuthorization: ExplicitOptimizerColdStartAuthorization("noise fixture " + scope));
        return new NoiseDecisionSetup(repositoryRoot, heldOutCases, request, quality, parent,
            CreateModelCheckpointAddress(repositoryRoot, 1), CreateModelCheckpointAddress(repositoryRoot, 2),
            CreateModelTrainingAttemptAddress(repositoryRoot, trainingRunId), SnapshotResumeFiles(repositoryRoot));
    }

    private static bool[] CreatePerRepeatQualityOutcomes(int heldOutCases, IReadOnlyList<int> passCounts)
    {
        Assert.Equal(MeasurementNoisePolicyConfig.Canonical.RepeatCount, passCounts.Count);
        foreach (var count in passCounts)
            Assert.InRange(count, 0, heldOutCases);
        return passCounts.SelectMany(count => Enumerable.Range(0, heldOutCases).Select(index => index < count))
            .ToArray();
    }

    private async Task<(OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence> Result,
        int TrainerCalls)> ObserveNoiseDecisionAsync(NoiseDecisionSetup setup)
    {
        var trainerCalls = 0;
        var priorObserver = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = request =>
            {
                Interlocked.Increment(ref trainerCalls);
                priorObserver?.Invoke(request);
            };
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .TrainAndPublishAsync(setup.Request).WaitAsync(TimeSpan.FromSeconds(30));
            var evaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(setup.Quality.EvaluationClient);
            _output.WriteLine("S03.06 actual quality route: " + JsonSerializer.Serialize(new
            {
                result.IsSuccess, result.ErrorCode, result.ErrorMessage, trainerCalls,
                evaluator.IssuedCallCount, evaluator.CompletedResponseCount,
                ObservationCount = evaluator.Observations.Count
            }));
            return (result, trainerCalls);
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = priorObserver;
        }
    }

    private static double[] AssertObservedQualitySeries(NoiseDecisionSetup setup, IReadOnlyList<int> passCounts)
    {
        var evaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(setup.Quality.EvaluationClient);
        var expectedCalls = passCounts.Count * setup.HeldOutCaseCount;
        Assert.Equal(expectedCalls, evaluator.IssuedCallCount);
        Assert.Equal(expectedCalls, evaluator.CompletedResponseCount);
        Assert.Equal(expectedCalls, evaluator.Observations.Count);
        Assert.Equal(expectedCalls, evaluator.Observations.Select(observation => observation.RequestId)
            .Distinct(StringComparer.Ordinal).Count());
        var expectedCaseIds = setup.Quality.Corpus.Entries
            .Where(entry => entry.Partition == FrozenReferencePartition.HeldOut)
            .Select(entry => entry.EntryId).Order(StringComparer.Ordinal).ToArray();
        var repeats = evaluator.Observations.Chunk(setup.HeldOutCaseCount).ToArray();
        Assert.Equal(passCounts.Count, repeats.Length);
        var scores = new double[repeats.Length];
        for (var index = 0; index < repeats.Length; index++)
        {
            Assert.Equal(expectedCaseIds, repeats[index].Select(observation => observation.CaseId).ToArray());
            var passed = repeats[index].Count(observation => observation.Passed);
            Assert.Equal(passCounts[index], passed);
            scores[index] = (double)passed / setup.HeldOutCaseCount;
        }
        return scores;
    }

    private static async Task<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>
        CalibrateNoiseBaselineAsync(NoiseDecisionSetup setup, IReadOnlyList<int> passCounts,
            bool existingTrainingAttempt = false)
    {
        var trainerCalls = 0;
        var priorObserver = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = request =>
            {
                Interlocked.Increment(ref trainerCalls);
                priorObserver?.Invoke(request);
            };
            var accepted = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(setup.Request.StartRequest);
            Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
            Assert.NotNull(accepted.Value!.ParentManifest);
            var result = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService()
                .CalibrateBaselineAsync(setup.Quality, accepted.Value.Model,
                    CreateAcceptedPassportForTest(accepted.Value.ParentManifest!, setup.Request.StartRequest.MaximumCheckpointBytes),
                    setup.Request.TrainingRunId + ":baseline").WaitAsync(TimeSpan.FromSeconds(30));
            Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
            var scores = AssertObservedQualitySeries(setup, passCounts);
            Assert.Equal(0, trainerCalls);
            var baseline = result.Value!;
            Assert.Equal(setup.HeldOutCaseCount * 5, baseline.ExpectedEvaluationCallCount);
            Assert.Equal(setup.HeldOutCaseCount * 5, baseline.ObservedEvaluationCallCount);
            var calibration = baseline.Calibration.Calibration;
            Assert.Equal(5, calibration.RepeatCount);
            Assert.Equal(setup.HeldOutCaseCount, calibration.HeldOutCaseCount);
            Assert.Equal(scores, calibration.Observations.Select(observation => observation.CorrectFraction).ToArray());
            Assert.Equal(scores, calibration.MeasurementNoisePassport.RepeatedScores.ToArray());
            Assert.True(MeasurementNoisePolicy.TryValidatePassport(calibration.MeasurementNoisePassport, out var reason), reason);
            if (!existingTrainingAttempt) AssertNoNoiseTrainingArtifacts(setup);
            AssertRetainedResumeFiles(setup.RepositoryRoot, setup.InputHashes, allowAdditionalFiles: existingTrainingAttempt);
            return baseline;
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = priorObserver;
        }
    }

    private OperationResult<bool> ObserveBaselinePairDecision(NoiseDecisionSetup setup,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality? baseline, string phase)
    {
        var evaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(setup.Quality.EvaluationClient);
        var issuedBefore = evaluator.IssuedCallCount;
        var completedBefore = evaluator.CompletedResponseCount;
        var observationsBefore = evaluator.Observations.Count;
        var trainerCalls = 0;
        var priorObserver = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = request =>
            {
                Interlocked.Increment(ref trainerCalls);
                priorObserver?.Invoke(request);
            };
            var result = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BaselineRequiresNewTrainingPair(baseline);
            _output.WriteLine("S03.06 actual baseline decision: " + JsonSerializer.Serialize(new
            {
                phase, result.IsSuccess, result.Value, result.ErrorCode, result.ErrorMessage,
                trainerCalls, issuedBefore, completedBefore, observationsBefore,
                evaluator.IssuedCallCount, evaluator.CompletedResponseCount,
                ObservationCount = evaluator.Observations.Count
            }));
            Assert.Equal(0, trainerCalls);
            Assert.Equal(issuedBefore, evaluator.IssuedCallCount);
            Assert.Equal(completedBefore, evaluator.CompletedResponseCount);
            Assert.Equal(observationsBefore, evaluator.Observations.Count);
            AssertNoNoiseTrainingArtifacts(setup);
            AssertRetainedResumeFiles(setup.RepositoryRoot, setup.InputHashes);
            AssertNoiseParentRemainsLoadable(setup);
            return result;
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = priorObserver;
        }
    }

    private static void AssertNoNoiseTrainingArtifacts(NoiseDecisionSetup setup)
    {
        Assert.False(File.Exists(Path.Combine(setup.AttemptAddress.AttemptRoot,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.BaselineStateFileName)));
        Assert.False(File.Exists(Path.Combine(setup.AttemptAddress.AttemptRoot, DpoTrainer.BuildPolicyCheckpointFileName(1))));
        Assert.False(Directory.Exists(setup.ChildAddress.CheckpointRoot));
    }

    private static void AssertNoiseTrialWeightsChanged(NoiseDecisionSetup setup)
    {
        var trial = CodeDocumentationRepositoryOwnedModelCheckpointStartService.LoadTrainingAttemptCheckpoint(
            setup.Request.StartRequest, setup.AttemptAddress, 1);
        Assert.True(trial.IsSuccess, trial.ErrorMessage);
        Assert.NotEqual(setup.ParentManifest.WeightsSha256, trial.Value!.WeightsSha256);
        Assert.Equal(ComputeFileSha256(trial.Value.WeightsPath), trial.Value.WeightsSha256);
    }

    private static void AssertNoiseParentRemainsLoadable(NoiseDecisionSetup setup)
    {
        Assert.Equal(setup.InputHashes[setup.ParentAddress.WeightsPath], ComputeFileSha256(setup.ParentAddress.WeightsPath));
        Assert.Equal(setup.InputHashes[setup.ParentAddress.ManifestPath], ComputeFileSha256(setup.ParentAddress.ManifestPath));
        var reopened = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(setup.Request.StartRequest);
        Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
        Assert.Equal(1, reopened.Value!.ParentManifest!.Step);
        Assert.Equal(setup.ParentManifest.WeightsSha256, reopened.Value.ParentManifest.WeightsSha256);
        Assert.Equal(setup.ParentManifest.WeightsSha256, ComputeCheckpointSha256(reopened.Value.Model));
    }

    private sealed record NoiseDecisionSetup(
        string RepositoryRoot,
        int HeldOutCaseCount,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest Request,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext Quality,
        RepositoryOwnedModelCheckpointManifest ParentManifest,
        RepositoryOwnedModelCheckpointAddress ParentAddress,
        RepositoryOwnedModelCheckpointAddress ChildAddress,
        RepositoryOwnedModelTrainingAttemptAddress AttemptAddress,
        IReadOnlyDictionary<string, string> InputHashes);

    private static CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext
        CreateChangedExamQuality(StagedResumeAttempt staged, BpeTokenizer tokenizer, string condition)
    {
        var configuration = condition switch
        {
            "evaluator-model" => CreateTeacherConfiguration(50, modelId: "qwen-code-doc-changed:9b"),
            "evaluator-manifest" => CreateTeacherConfiguration(50, manifestDigest: Hex('e')),
            "evaluator-layer" => CreateTeacherConfiguration(50, layerDigest: Hex('e')),
            _ => CreateTeacherConfiguration(50)
        };
        var corpus = staged.QualityCorpus;
        if (condition is "corpus-version" or "reference-content" or
            "source-fingerprint" or "held-out-membership")
        {
            var original = corpus.Corpus;
            var entries = original.Entries.ToArray();
            var originalEntry = Assert.Single(entries.Where(entry => entry.OwnerInput == "case 001"));
            var replacement = condition switch
            {
                "reference-content" => CreateQualityReferenceEntry("case 001", "changed reference 001",
                    FrozenReferencePartition.HeldOut, "heldout-001"),
                "source-fingerprint" => CreateQualityReferenceEntry("case 001", "ok 001",
                    FrozenReferencePartition.HeldOut, "heldout-001",
                    "sha256:" + Sha256Hex("s03-05-changed-source-only")),
                "held-out-membership" => CreateQualityReferenceEntry("case 006", "ok 006",
                    FrozenReferencePartition.HeldOut, "heldout-006"),
                _ => originalEntry
            };
            entries = entries.Select(entry => entry.EntryId == originalEntry.EntryId ? replacement : entry)
                .ToArray();
            // A changed sealed snapshot needs its own versioned address. Keep
            // the original corpus/attempt bytes; never overwrite the old exam.
            corpus = SealQualityReferenceCorpus(staged.RepositoryRoot,
                QualityCorpusVersion + "-" + condition, entries);
            Assert.Equal(5, corpus.Corpus.Entries.Count(entry => entry.Partition == FrozenReferencePartition.HeldOut));
            Assert.NotEqual(original.CorpusSha256, corpus.Corpus.CorpusSha256);
            Assert.NotEqual(staged.QualityCorpus.Manifest.ManifestSha256, corpus.Manifest.ManifestSha256);
            if (condition == "corpus-version")
                Assert.Equal(original.EntryNames, corpus.Corpus.EntryNames);
            else
                Assert.NotEqual(originalEntry.EntryId, replacement.EntryId);
            if (condition == "source-fingerprint")
            {
                Assert.Equal(originalEntry.OwnerInput, replacement.OwnerInput);
                Assert.Equal(originalEntry.PaidModelReferenceAnswer, replacement.PaidModelReferenceAnswer);
                Assert.Equal(originalEntry.PaidReferenceProvenance, replacement.PaidReferenceProvenance);
                Assert.NotEqual(originalEntry.SourceEntityFingerprint, replacement.SourceEntityFingerprint);
            }
        }

        var quality = CreateQualityGateContext(corpus, tokenizer,
            CreateQualityOutcomes(baselinePass: true, trialPass: true),
            index => "changed-exam-" + condition + "-" + index.ToString("D4"),
            evaluatorConfiguration: configuration);
        var settings = quality.GenerationSettings;
        return condition switch
        {
            "generation-max-new-tokens" => quality with { GenerationSettings = settings with { MaxNewTokens = 2 } },
            "generation-temperature" => quality with { GenerationSettings = settings with { Temperature = 0.1f } },
            "generation-top-k" => quality with { GenerationSettings = settings with { TopK = 2 } },
            "generation-top-p" => quality with { GenerationSettings = settings with { TopP = 0.9f } },
            "generation-seed" => quality with { GenerationSettings = settings with { Seed = 18 } },
            "generation-max-held-out-cases" => quality with { GenerationSettings = settings with { MaxHeldOutCases = 6 } },
            "generation-max-answer-chars" => quality with { GenerationSettings = settings with { MaxAnswerChars = 257 } },
            "noise-repeat" => quality with { NoisePolicyConfig = new MeasurementNoisePolicyConfig(4, 2d) },
            "noise-multiplier" => quality with { NoisePolicyConfig = new MeasurementNoisePolicyConfig(5, 2.25d) },
            "evaluator-model" or "evaluator-manifest" or "evaluator-layer" or
                "corpus-version" or "reference-content" or "source-fingerprint" or "held-out-membership" => quality,
            _ => throw new InvalidOperationException("Unknown exam condition.")
        };
    }

    private static void AssertResumePlan(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext quality)
    {
        var plan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(
            quality.Corpus, quality.GenerationSettings, quality.EvaluatorConfiguration, quality.NoisePolicyConfig);
        Assert.True(plan.IsSuccess, $"{plan.ErrorCode}: {plan.ErrorMessage}");
        Assert.Equal(5, plan.Value!.BaselinePlan.HeldOutCaseCount);
        Assert.Equal(quality.NoisePolicyConfig.RepeatCount, plan.Value.BaselinePlan.RepeatCount);
        Assert.Equal(5 * quality.NoisePolicyConfig.RepeatCount, plan.Value.BaselinePlan.ExpectedEvaluationCallCount);
        Assert.Equal(10 * quality.NoisePolicyConfig.RepeatCount, plan.Value.ExpectedTotalEvaluationCallCount);
        Assert.True(plan.Value.ExpectedTotalEvaluationCallCount <= quality.EvaluatorConfiguration.MaxTeacherCalls);
        Assert.Equal(quality.Corpus.CorpusSha256, quality.CorpusManifest.CorpusSha256);
        Assert.Equal(quality.Corpus.CorpusVersion, quality.CorpusManifest.CorpusVersion);
    }

    internal static CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback
        ReadVerifiedResumeBaseline(TinyTrainingFixtureCL fixture, StagedResumeAttempt staged)
    {
        var baseline = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.OpenExisting(staged.AttemptAddress);
        Assert.True(baseline.IsSuccess, $"{baseline.ErrorCode}: {baseline.ErrorMessage}");
        var saved = baseline.Value!;
        Assert.Equal(staged.TrainingRunId, saved.State.TrainingRunId);
        Assert.Equal(25, saved.State.BaselineQuality.ExpectedEvaluationCallCount);
        Assert.Equal(25, saved.State.BaselineQuality.ObservedEvaluationCallCount);
        Assert.Equal(5, saved.State.BaselineQuality.Calibration.Calibration.HeldOutCaseCount);
        Assert.Equal(5, saved.State.BaselineQuality.Calibration.Calibration.RepeatCount);
        Assert.Equal(staged.DpoSetup.Receipt.ManifestSha256, saved.State.DpoManifestSha256);
        var pair = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
            .MatchesSingleTrainingPair(saved.State, staged.DpoSetup.Pairs);
        Assert.True(pair.IsSuccess, pair.ErrorMessage);
        var commit = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(staged.AttemptAddress);
        Assert.True(commit.IsSuccess, commit.ErrorMessage);
        var manifest = commit.Value!.Manifest;
        var matchesBaseline = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
            .MatchesBaseline(manifest, staged.AttemptAddress, saved.State);
        Assert.True(matchesBaseline.IsSuccess, matchesBaseline.ErrorMessage);
        var trial = CodeDocumentationRepositoryOwnedModelCheckpointStartService.LoadTrainingAttemptCheckpoint(
            staged.StartRequest, staged.AttemptAddress, 1);
        Assert.True(trial.IsSuccess, trial.ErrorMessage);
        var trialIdentity = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
            .ValidateTrialCheckpointAgainstManifest(manifest, staged.AttemptAddress, trial.Value!);
        Assert.True(trialIdentity.IsSuccess, trialIdentity.ErrorMessage);
        Assert.NotEqual(saved.State.AcceptedModelPassport.WeightsSha256, trial.Value!.WeightsSha256);
        var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
            staged.AttemptAddress.AttemptRoot, manifest.OptimizerStateStep, manifest.OptimizerStateSha256);
        Assert.True(optimizer.IsSuccess, optimizer.ErrorMessage);
        var runtime = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.MatchesRuntime(
            manifest, staged.AttemptAddress, fixture.ModelConfig,
            trial.Value.Model.GetCheckpointParameterEntries()
                .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter)).ToArray(),
            DpoTrainer.ProductionOptimizerSettings, optimizer.Value!.State);
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var random = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OpenAndValidateExistingAttemptRandomState(
            fixture.ModelConfig, staged.StartRequest.LoraConfig!, staged.StartRequest.AdapterSeedAuthority,
            null, staged.AttemptAddress, manifest, CancellationToken.None);
        Assert.True(random.IsSuccess, random.ErrorMessage);
        Assert.False(Directory.Exists(staged.ChildAddress.CheckpointRoot));
        AssertAcceptedParentRemainsLoadable(staged,
            staged.ParentManifest.WeightsSha256, ComputeFileSha256(staged.ParentAddress.ManifestPath));
        return saved;
    }

    private static void AssertSavedBaselineExamIdentity(
        StagedResumeAttempt staged,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState saved,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext quality)
    {
        var accepted = CodeDocumentationRepositoryOwnedModelCheckpointStartService
            .PrepareStart(staged.StartRequest);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        var request = new CodeDocumentationSavedModelAssessmentRequest(
            accepted.Value!.Model, quality.Tokenizer, saved.AcceptedModelPassport,
            quality.Corpus, quality.CorpusManifest, quality.EvaluationClient,
            quality.EvaluatorConfiguration, quality.GenerationSettings,
            "saved-exam-identity-projection", quality.ObservedAtUtc, quality.EvidenceRefs);
        var plan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(
            quality.Corpus, quality.GenerationSettings, quality.EvaluatorConfiguration,
            quality.NoisePolicyConfig);
        Assert.True(plan.IsSuccess, plan.ErrorMessage);
        var baseline = saved.BaselineQuality.Calibration;
        bool Matches(CodeDocumentationSavedModelAssessmentCalibrationResult? candidate)
            => CodeDocumentationSavedModelAssessmentCalibrationService.BaselineMatchesCurrentExam(
                candidate, request, quality.NoisePolicyConfig, plan.Value!.BaselinePlan);

        // Every negative below derives from this actually measured, reopened
        // positive. Only the noise field is later changed in the on-disk journal.
        Assert.True(Matches(baseline));
        Assert.False(Matches(null));
        Assert.False(Matches(baseline with { RepresentativeAssessment = null! }));
        Assert.False(Matches(baseline with { Calibration = null! }));
        var representative = baseline.RepresentativeAssessment;
        var calibration = baseline.Calibration;
        Assert.False(Matches(baseline with
        {
            RepresentativeAssessment = representative with { Evaluator = null! }
        }));
        Assert.False(Matches(baseline with
        {
            RepresentativeAssessment = representative with { GenerationSettings = null! }
        }));

        foreach (var missing in new string?[] { null, string.Empty })
        {
            var variantLabel = missing is null ? "null" : "empty";
            var variants = new (string Name, CodeDocumentationSavedModelAssessmentCalibrationResult Value)[]
            {
                ("evaluation-set", baseline with { Calibration = calibration with { EvaluationSetId = missing! } }),
                ("noise", baseline with { Calibration = calibration with { NoisePolicyIdentitySha256 = missing! } }),
                ("domain", baseline with { RepresentativeAssessment = representative with { DomainSlug = missing! } }),
                ("pattern", baseline with { RepresentativeAssessment = representative with { DesignPatternSlug = missing! } }),
                ("generation-hash", baseline with { RepresentativeAssessment = representative with { GenerationSettingsSha256 = missing! } }),
                ("corpus-version", baseline with { RepresentativeAssessment = representative with { CorpusVersion = missing! } }),
                ("corpus-manifest", baseline with { RepresentativeAssessment = representative with { CorpusManifestSha256 = missing! } }),
                ("corpus-hash", baseline with { RepresentativeAssessment = representative with { CorpusSha256 = missing! } }),
                ("evaluator-model", baseline with { RepresentativeAssessment = representative with
                    { Evaluator = representative.Evaluator with { ExpectedModelId = missing! } } }),
                ("evaluator-manifest", baseline with { RepresentativeAssessment = representative with
                    { Evaluator = representative.Evaluator with { ExpectedOllamaManifestDigest = missing! } } }),
                ("evaluator-layer", baseline with { RepresentativeAssessment = representative with
                    { Evaluator = representative.Evaluator with { ExpectedOllamaModelLayerDigest = missing! } } }),
                ("profile-id", baseline with { RepresentativeAssessment = representative with
                    { Evaluator = representative.Evaluator with { EvaluationProfileId = missing! } } }),
                ("profile-config", baseline with { RepresentativeAssessment = representative with
                    { Evaluator = representative.Evaluator with { EvaluationProfileConfigurationSha256 = missing! } } })
            };
            foreach (var variant in variants)
                Assert.False(Matches(variant.Value), variantLabel + " saved " + variant.Name);
        }

        Assert.False(Matches(baseline with
        {
            RepresentativeAssessment = representative with
            {
                Evaluator = representative.Evaluator with
                {
                    EvaluationProfileId = LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId
                }
            }
        }));
        Assert.False(Matches(baseline with
        {
            RepresentativeAssessment = representative with
            {
                Evaluator = representative.Evaluator with
                {
                    EvaluationProfileConfigurationSha256 =
                        LocalQwenOllamaInferenceProfileAuthority.Authoring.ConfigurationSha256
                }
            }
        }));
        Assert.False(Matches(baseline with
        {
            Calibration = calibration with { EvaluationSetId = "code-doc-assessment:" + Hex('e') }
        }));
        var evaluator = Assert.IsType<ScriptedQualityGateEvaluationClient>(quality.EvaluationClient);
        Assert.Equal(0, evaluator.IssuedCallCount);
        Assert.Equal(0, evaluator.CompletedResponseCount);
        Assert.Empty(evaluator.Observations);
    }

    internal static Dictionary<string, string> SnapshotResumeFiles(string repositoryRoot)
        => Directory.GetFiles(repositoryRoot, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, ComputeFileSha256, StringComparer.Ordinal);

    private static void AssertRetainedResumeFiles(string repositoryRoot,
        IReadOnlyDictionary<string, string> retainedFiles, bool allowAdditionalFiles = false)
    {
        if (!allowAdditionalFiles)
            Assert.Equal(retainedFiles.Keys.Order(StringComparer.Ordinal),
                Directory.GetFiles(repositoryRoot, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
        foreach (var file in retainedFiles)
            Assert.Equal(file.Value, ComputeFileSha256(file.Key));
    }

    private static void AssertRejectedResumeRetainsAttempt(
        StagedResumeAttempt staged, IReadOnlyDictionary<string, string> retainedFiles)
    {
        AssertRetainedResumeFiles(staged.RepositoryRoot, retainedFiles);
        Assert.False(Directory.Exists(staged.ChildAddress.CheckpointRoot));
        AssertAcceptedParentRemainsLoadable(staged,
            retainedFiles[staged.ParentAddress.WeightsPath], retainedFiles[staged.ParentAddress.ManifestPath]);
    }

    private async Task<ResumeObservation> ObserveResumeAsync(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request,
        ScriptedQualityGateEvaluationClient evaluator)
    {
        var trainerCalls = 0;
        var priorObserver = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = logicalRequest =>
            {
                Interlocked.Increment(ref trainerCalls);
                priorObserver?.Invoke(logicalRequest);
            };
            var result = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .TrainAndPublishAsync(request).WaitAsync(TimeSpan.FromSeconds(30));
            var observed = new ResumeObservation(result, trainerCalls);
            // Emit before any caller-side artifact assertion can fail and hide
            // the actual pre-fix route, evaluator, or trainer observations.
            _output.WriteLine(FormatResumeObservations(observed, evaluator));
            return observed;
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = priorObserver;
        }
    }

    private static void AssertResumeFailure(ResumeObservation observed,
        ScriptedQualityGateEvaluationClient evaluator, string expectedErrorCode, int expectedCalls)
    {
        var actual = FormatResumeObservations(observed, evaluator);
        Assert.False(observed.Result.IsSuccess, actual);
        Assert.True(StringComparer.Ordinal.Equals(expectedErrorCode, observed.Result.ErrorCode), actual);
        Assert.Null(observed.Result.Value);
        Assert.True(observed.TrainerCalls == 0, actual);
        Assert.True(evaluator.IssuedCallCount == expectedCalls, actual);
        Assert.True(evaluator.CompletedResponseCount == expectedCalls, actual);
        Assert.True(evaluator.Observations.Count == expectedCalls, actual);
    }

    private static string FormatResumeObservations(ResumeObservation observed,
        ScriptedQualityGateEvaluationClient evaluator)
        => "S03.05 actual recovery observations: " + JsonSerializer.Serialize(new
        {
            observed.Result.IsSuccess,
            observed.Result.ErrorCode,
            observed.Result.ErrorMessage,
            evaluator.IssuedCallCount,
            evaluator.CompletedResponseCount,
            ObservationCount = evaluator.Observations.Count,
            observed.TrainerCalls
        });

    private sealed record ResumeObservation(
        OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence> Result,
        int TrainerCalls);

    internal enum AppliedButUnpublishedAttemptStagingMode
    {
        CompletedNonImproving,
        IncompleteStaging
    }

    internal sealed record AppliedButUnpublishedAttemptStagingObservation(
        OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence> Result,
        ScriptedQualityGateEvaluationClient Evaluator);

    internal sealed record PreparedAppliedButUnpublishedAttempt(
        TinyTrainingFixtureCL RuntimeFixture,
        string RepositoryRoot,
        string Scope,
        RepositoryOwnedModelRuntimeCompatibility Compatibility,
        RepositoryOwnedCodeDocumentationDpoSetup ParentDpoSetup,
        RepositoryOwnedCodeDocumentationDpoSetup DpoSetup,
        RepositoryOwnedReferenceCorpusOpenResult QualityCorpus,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest StartRequest,
        RepositoryOwnedModelCheckpointManifest ParentManifest,
        RepositoryOwnedModelCheckpointAddress ParentAddress,
        RepositoryOwnedModelCheckpointAddress ChildAddress,
        RepositoryOwnedModelTrainingAttemptAddress AttemptAddress,
        string TrainingRunId,
        bool SeededDropout,
        LoraPairRandomSeedAuthority? SeedAuthority,
        BpeTokenizer Tokenizer,
        AdamWPortableState? ParentOptimizer,
        LlmModelRandomStateSnapshot? ParentRandom);

    private static Task<StagedResumeAttempt>
        CreateAppliedButUnpublishedAttemptAsync(
            TinyTrainingFixtureCL fixture,
            string repositoryRoot,
            string scope,
            bool seededDropout = false,
            AppliedButUnpublishedAttemptStagingMode stagingMode =
                AppliedButUnpublishedAttemptStagingMode.CompletedNonImproving,
            LoraConfig? loraConfig = null,
            LoraPairRandomSeedAuthority? seededAuthorityOverride = null)
    {
        var prepared = PrepareAppliedButUnpublishedAttempt(
            fixture,
            repositoryRoot,
            scope,
            seededDropout,
            loraConfig,
            seededAuthorityOverride);
        if (stagingMode == AppliedButUnpublishedAttemptStagingMode.IncompleteStaging)
            return StageIncomplete(prepared);
        return StagePreparedAttemptAsync(prepared, stagingMode);
    }

    internal static PreparedAppliedButUnpublishedAttempt
        PrepareAppliedButUnpublishedAttempt(
            TinyTrainingFixtureCL fixture,
            string repositoryRoot,
            string scope,
            bool seededDropout = false,
            LoraConfig? loraConfig = null,
            LoraPairRandomSeedAuthority? seededAuthorityOverride = null)
    {
        var tokenizer = BpeTokenizer.Load(fixture.TokenizerPath);
        var effectiveLoraConfig = loraConfig ?? CreateLoraConfig();
        var compatibility = CreateCompatibility(
            fixture.ModelConfig,
            "code-doc-lifecycle-" + scope);
        var parentDpoSetup = SealRepositoryOwnedDpo(
            repositoryRoot,
            "code-doc-lifecycle-" + scope + "-parent",
            datasetVersion: "v0-parent",
            prompt: "doc code",
            chosen: "keep code",
            rejected: "drop code");
        var dpoSetup = SealRepositoryOwnedDpo(
            repositoryRoot,
            "code-doc-lifecycle-" + scope + "-new",
            datasetVersion: "v1-new");
        var qualityCorpus = SealQualityReferenceCorpus(repositoryRoot);
        var parentSession = fixture.CreateFactory().Create("CODE").Value!;
        var seededConfig = loraConfig ?? new LoraConfig
        {
            Rank = 2, Alpha = 4f, Dropout = 0.25f, TargetModules = ["q_proj"]
        };
        var seededAuthority = seededAuthorityOverride ??
            new LoraPairRandomSeedAuthority(991UL, "policy.step", "reference.step");
        AdamWPortableState? parentOptimizer = null;
        LlmModelRandomStateSnapshot? parentRandom = null;
        if (seededDropout)
        {
            // A resumed dropout parent needs a real saved cursor and matching
            // optimizer, not a cold-start exception. Bootstrap a synthetic unit
            // fixture with one actual controlled update; never fabricate a step.
            var parentReference = CloneModel(parentSession.Model);
            LoraApplicator.ApplyFreshSeededPair(parentSession.Model, parentReference,
                seededConfig, seededAuthority.RootInitializationSeed,
                seededAuthority.PolicyDropoutStreamDomain, seededAuthority.ReferenceDropoutStreamDomain);
            var parentStateRoot = Path.Combine(repositoryRoot, "seeded-parent");
            Directory.CreateDirectory(parentStateRoot);
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(
                fixture.ModelConfig, seededConfig, seededAuthority);
            var configuration = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
                seededConfig, seededAuthority);
            var parentTrainer = global::DpoTrainerOptimizerStateHandoffTests.CreateSeededTrainer(
                parentSession.Model, parentReference, parentStateRoot, topology, configuration);
            var sealedParentDataset = RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealed(parentDpoSetup.Address, parentDpoSetup.RequestIdentity);
            Assert.True(sealedParentDataset.IsSuccess,
                sealedParentDataset.ErrorMessage);
            Assert.Equal(parentDpoSetup.Identity.ManifestSha256,
                sealedParentDataset.Value!.Manifest.ManifestSha256);
            var parentPair = Assert.Single(sealedParentDataset.Value.Pairs);
            Assert.Equal(Assert.Single(parentDpoSetup.Pairs).Prompt,
                parentPair.Prompt);
            Assert.Equal(parentDpoSetup.Pairs[0].Chosen, parentPair.Chosen);
            Assert.Equal(parentDpoSetup.Pairs[0].Rejected, parentPair.Rejected);
            var preparedParent = LogicalDpoTrainingUnit.PrepareExecution(
                [new LogicalDpoPreferenceExample(
                    parentPair.Prompt,
                    parentPair.Chosen,
                    parentPair.Rejected)
                {
                    ChosenAuthorKind = parentPair.ChosenAuthorship?.AuthorKind,
                    RejectedAuthorKind = parentPair.RejectedAuthorship?.AuthorKind,
                }],
                tokenizer);
            Assert.True(preparedParent.IsSuccess,
                preparedParent.ErrorMessage);
            var parentInput = Assert.Single(
                preparedParent.Value!.OptimizerInputs);
            Assert.Equal(parentPair.Prompt, parentInput.Prompt);
            Assert.Equal(parentPair.Chosen, parentInput.Chosen);
            Assert.Equal(parentPair.Rejected, parentInput.Rejected);
            var parentBatch = parentInput.Batch.Batch;
            Assert.NotNull(parentBatch.Admission);
            var expectedAdmission = DpoTrainerPairCustoms.CreateAdmitted(
                parentPair.Prompt,
                parentPair.Chosen,
                parentPair.Rejected,
                parentPair.ChosenAuthorship!.AuthorKind,
                parentPair.RejectedAuthorship!.AuthorKind,
                nameof(LogicalDpoTrainingUnit)).Admission;
            Assert.Equal(expectedAdmission, parentBatch.Admission);
            Assert.InRange(parentBatch.ChosenIds.Shape.Dimensions[1],
                2, fixture.ModelConfig.ContextLen);
            Assert.InRange(parentBatch.RejectedIds.Shape.Dimensions[1],
                2, fixture.ModelConfig.ContextLen);
            parentTrainer.Step(parentBatch, 1);
            var optimizerFile = RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                parentStateRoot, 1, parentTrainer.ExportOptimizerState());
            Assert.True(optimizerFile.IsSuccess, optimizerFile.ErrorMessage);
            var randomFile = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                Path.Combine(parentStateRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(1)),
                parentSession.Model.CaptureRandomState(topology, configuration, 1));
            Assert.True(randomFile.IsSuccess, randomFile.ErrorMessage);
            var reopenedOptimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                parentStateRoot, 1, optimizerFile.Value!.OptimizerStateSha256);
            var reopenedRandom = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
                randomFile.Value!.Path, randomFile.Value.Sha256, randomFile.Value.ByteLength,
                1, topology, configuration);
            Assert.True(reopenedOptimizer.IsSuccess, reopenedOptimizer.ErrorMessage);
            Assert.True(reopenedRandom.IsSuccess, reopenedRandom.ErrorMessage);
            parentOptimizer = reopenedOptimizer.Value!.State;
            parentRandom = reopenedRandom.Value!;
        }
        var parentManifest = SealModelCheckpoint(
            repositoryRoot,
            parentSession.Model,
            fixture.ModelConfig,
            compatibility,
            parentDpoSetup.Identity,
            step: 1,
            parent: null,
            coldReason: "test parent before same-run resume");
        var parentAddress = CreateModelCheckpointAddress(repositoryRoot, step: 1);
        var childAddress = CreateModelCheckpointAddress(repositoryRoot, step: 2);
        var request = CreateStartRequest(
            repositoryRoot,
            fixture.ModelConfig,
            compatibility,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
            effectiveLoraConfig);
        if (seededDropout)
        {
            request = request with
            {
                LoraConfig = seededConfig,
                AdapterSeedAuthority = seededAuthority
            };
        }
        var trainingRunId = "code-doc-lifecycle:" + scope;
        var attemptAddress = CreateModelTrainingAttemptAddress(
            repositoryRoot,
            trainingRunId);
        var openedParent = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(parentAddress);
        Assert.True(openedParent.IsSuccess, openedParent.ErrorMessage);
        Assert.Equal(parentManifest.ManifestSha256,
            openedParent.Value!.Manifest.ManifestSha256);
        Assert.Equal(1, openedParent.Value.Manifest.Step);
        Assert.Equal(parentManifest.WeightsSha256,
            openedParent.Value.WeightsSha256);
        if (seededDropout)
            Assert.Equal(seededAuthority, request.AdapterSeedAuthority);

        return new PreparedAppliedButUnpublishedAttempt(
            fixture,
            repositoryRoot,
            scope,
            compatibility,
            parentDpoSetup,
            dpoSetup,
            qualityCorpus,
            request,
            parentManifest,
            parentAddress,
            childAddress,
            attemptAddress,
            trainingRunId,
            seededDropout,
            seededDropout ? seededAuthority : null,
            tokenizer,
            parentOptimizer,
            parentRandom);
    }

    internal static Task<StagedResumeAttempt> StageIncomplete(
        PreparedAppliedButUnpublishedAttempt prepared)
        => StagePreparedAttemptAsync(
            prepared,
            AppliedButUnpublishedAttemptStagingMode.IncompleteStaging);

    private static async Task<StagedResumeAttempt> StagePreparedAttemptAsync(
        PreparedAppliedButUnpublishedAttempt prepared,
        AppliedButUnpublishedAttemptStagingMode stagingMode)
    {
        ScriptedQualityGateEvaluationClient? stagingEvaluator = null;
        var stagingOutcomes = stagingMode switch
        {
            AppliedButUnpublishedAttemptStagingMode.CompletedNonImproving =>
                CreateQualityOutcomes(baselinePass: false, trialPass: false),
            AppliedButUnpublishedAttemptStagingMode.IncompleteStaging =>
                Enumerable.Repeat(false, 5 * MeasurementNoisePolicyConfig.Canonical.RepeatCount)
                    .ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(stagingMode), stagingMode, null)
        };
        var stagingMaxTeacherCalls = stagingMode ==
            AppliedButUnpublishedAttemptStagingMode.IncompleteStaging ? 51 : 50;
        var initial =
            await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
                .TrainAndPublishAsync(
                    new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                        CreateRoutingRun(
                            prepared.DpoSetup.Receipt,
                            prepared.DpoSetup.Pairs),
                        prepared.DpoSetup.Address,
                        prepared.StartRequest,
                        prepared.Tokenizer,
                        prepared.StartRequest.LoraConfig!,
                        CreateAcceptingBroker(),
                        prepared.TrainingRunId,
                        DpoStepCount: 1,
                        DateTimeOffset.UnixEpoch.AddDays(8),
                        ["test:code-documentation-dpo-applied-unpublished-" +
                         prepared.Scope],
                        CreateQualityGateContext(
                            prepared.QualityCorpus,
                            prepared.Tokenizer,
                            stagingOutcomes,
                            captureClient: evaluator => stagingEvaluator = evaluator,
                            evaluatorConfiguration: CreateTeacherConfiguration(
                                stagingMaxTeacherCalls)),
                        OptimizerStateToRestore: prepared.ParentOptimizer,
                        OptimizerStateColdStartAuthorization: prepared.SeededDropout ? null :
                            ExplicitOptimizerColdStartAuthorization("root resume lifecycle"),
                        AdapterSeedAuthority: prepared.StartRequest.AdapterSeedAuthority,
                        RandomStateToRestore: prepared.ParentRandom))
                .WaitAsync(TimeSpan.FromSeconds(30));
        Assert.False(initial.IsSuccess);
        Assert.NotNull(stagingEvaluator);
        if (stagingMode == AppliedButUnpublishedAttemptStagingMode.CompletedNonImproving)
        {
            Assert.True(initial.ErrorCode ==
                CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateNotImprovedErrorCode,
                $"Staging failed before the expected quality decision: {initial.ErrorCode}: {initial.ErrorMessage}");
        }
        else
        {
            Assert.Equal("SCRIPTED_QUALITY_GATE_DECISION_MISSING", initial.ErrorCode);
        }
        Assert.True(File.Exists(Path.Combine(
            prepared.AttemptAddress.AttemptRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
        Assert.True(File.Exists(Path.Combine(
            prepared.AttemptAddress.AttemptRoot,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .BaselineStateFileName)));
        Assert.False(File.Exists(prepared.ChildAddress.WeightsPath));
        Assert.False(File.Exists(prepared.ChildAddress.ManifestPath));

        return new StagedResumeAttempt(
            prepared.RepositoryRoot,
            prepared.Compatibility,
            prepared.DpoSetup,
            prepared.QualityCorpus,
            prepared.StartRequest,
            prepared.ParentManifest,
            prepared.ParentAddress,
            prepared.ChildAddress,
            prepared.AttemptAddress,
            prepared.TrainingRunId,
            new AppliedButUnpublishedAttemptStagingObservation(
                initial,
                stagingEvaluator!));
    }

    internal static PreparedAppliedButUnpublishedAttempt
        PrepareIncompleteAppliedButUnpublishedAttemptForHttp(
            TinyTrainingFixtureCL fixture,
            string repositoryRoot,
            string scope,
            ulong rootInitializationSeed)
    {
        var authority = CodeDocumentationDpoSeedAuthorityResolver.Resolve(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            rootInitializationSeed);
        Assert.True(authority.IsSuccess, authority.ErrorMessage);
        return PrepareAppliedButUnpublishedAttempt(
            fixture,
            repositoryRoot,
            scope,
            seededDropout: true,
            loraConfig: IncrementalTrainer.DefaultConfig,
            seededAuthorityOverride: authority.Value!);
    }

    internal static Task<StagedResumeAttempt>
        CreateIncompleteAppliedButUnpublishedAttemptForHttpAsync(
            TinyTrainingFixtureCL fixture,
            string repositoryRoot,
            string scope,
            ulong rootInitializationSeed)
            => StageIncomplete(PrepareIncompleteAppliedButUnpublishedAttemptForHttp(
                fixture,
                repositoryRoot,
                scope,
                rootInitializationSeed));

    private static CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest
        CreateRecoveryRequest(
            TinyTrainingFixtureCL fixture,
            StagedResumeAttempt staged,
            RepositoryOwnedCodeDocumentationDpoSetup dpoSetup,
            BpeTokenizer tokenizer,
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext qualityGate)
        => new(
            CreateRoutingRun(dpoSetup.Receipt, dpoSetup.Pairs),
            dpoSetup.Address,
            CreateStartRequest(
                staged.RepositoryRoot,
                fixture.ModelConfig,
                staged.Compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied) with
            {
                LoraConfig = staged.StartRequest.LoraConfig,
                AdapterSeedAuthority = staged.StartRequest.AdapterSeedAuthority
            },
            tokenizer,
            staged.StartRequest.LoraConfig!,
            CreateAcceptingBroker(),
            staged.TrainingRunId,
            DpoStepCount: 1,
            DateTimeOffset.UnixEpoch.AddDays(9),
            ["test:code-documentation-dpo-resume-" + staged.TrainingRunId],
            qualityGate,
            AdapterSeedAuthority: staged.StartRequest.AdapterSeedAuthority);

    private static void AssertAcceptedParentRemainsLoadable(
        StagedResumeAttempt staged,
        string expectedWeightsSha256,
        string expectedManifestSha256)
    {
        Assert.Equal(expectedWeightsSha256,
            ComputeFileSha256(staged.ParentAddress.WeightsPath));
        Assert.Equal(expectedManifestSha256,
            ComputeFileSha256(staged.ParentAddress.ManifestPath));
        var reopened =
            CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(staged.StartRequest);
        Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
        Assert.Equal(staged.ParentManifest.Step,
            reopened.Value!.ParentManifest!.Step);
        Assert.Equal(staged.ParentManifest.WeightsSha256,
            reopened.Value.ParentManifest.WeightsSha256);
    }

    internal sealed record StagedResumeAttempt(
        string RepositoryRoot,
        RepositoryOwnedModelRuntimeCompatibility Compatibility,
        RepositoryOwnedCodeDocumentationDpoSetup DpoSetup,
        RepositoryOwnedReferenceCorpusOpenResult QualityCorpus,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest StartRequest,
        RepositoryOwnedModelCheckpointManifest ParentManifest,
        RepositoryOwnedModelCheckpointAddress ParentAddress,
        RepositoryOwnedModelCheckpointAddress ChildAddress,
        RepositoryOwnedModelTrainingAttemptAddress AttemptAddress,
        string TrainingRunId,
        AppliedButUnpublishedAttemptStagingObservation Staging);

    private static CodeDocumentationRepositoryOwnedModelCheckpointStartRequest
        CreateStartRequest(
            string repositoryRoot,
            ModelConfig modelConfig,
            RepositoryOwnedModelRuntimeCompatibility compatibility,
            RepositoryOwnedIncrementalTrainingColdStartPolicy coldStartPolicy,
            LoraConfig? loraConfig = null)
        => new(
            repositoryRoot,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
            ModelIdSlug: "code-documentation-llm",
            ModelVersion: "v1",
            modelConfig,
            compatibility,
            coldStartPolicy,
            LoraConfig: loraConfig ?? CreateLoraConfig());

    private static
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization
        ExplicitOptimizerColdStartAuthorization(string scope)
        => CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization
            .Authorize(
                "Explicit initial-training optimizer cold start for " + scope + ".");

    private static RepositoryOwnedModelCheckpointAddress CreateModelCheckpointAddress(
        string repositoryRoot,
        int step)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                "code-documentation-llm",
                "v1",
                step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedModelTrainingAttemptAddress
        CreateModelTrainingAttemptAddress(
            string repositoryRoot,
            string trainingRunId)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelTrainingAttemptAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                "code-documentation-llm",
                "v1",
                trainingRunId);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    internal static RepositoryOwnedModelCheckpointManifest SealModelCheckpoint(
        string repositoryRoot,
        LlmModel model,
        ModelConfig modelConfig,
        RepositoryOwnedModelRuntimeCompatibility compatibility,
        RepositoryOwnedDpoDatasetManifestIdentity dpoIdentity,
        int step,
        RepositoryOwnedModelCheckpointManifest? parent,
        string coldReason,
        string modelIdSlug = "code-documentation-llm",
        string modelVersion = "v1")
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                modelIdSlug,
                modelVersion,
                step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        Assert.Equal(
            TrainingSourceProvenance.Identify(modelConfig).Sha256,
            compatibility.ConfigSha256);

        var manifest = RepositoryOwnedModelCheckpointManifestSealer
            .PersistAndSeal(
                address.Value!,
                model.SaveCheckpointToStream,
                new RepositoryOwnedModelCheckpointManifestRequest(
                    parent is null
                        ? null
                        : new RepositoryOwnedModelCheckpointParentIdentity(
                            parent.RepositoryRelativeManifestPath,
                            parent.Step,
                            parent.WeightsSha256,
                            parent.WeightsByteLength),
                    parent is null ? coldReason : string.Empty,
                    dpoIdentity,
                    compatibility,
                    DateTimeOffset.UnixEpoch.AddMinutes(step),
                    ["test:seed-code-documentation-model-parent"]));
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        return manifest.Value!;
    }

    private static LlmModel CloneModel(LlmModel source)
    {
        using var stream = new MemoryStream();
        source.SaveCheckpointToStream(stream);
        stream.Position = 0;
        var clone = new LlmModel(source.Config);
        clone.LoadCheckpoint(stream);
        return clone;
    }

    private static string ComputeCheckpointSha256(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return Convert.ToHexString(SHA256.HashData(stream.ToArray()))
            .ToLowerInvariant();
    }

    internal static string ComputeFileSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void MutateOptimizerStateManifest(
        string path,
        Action<JsonObject> mutation)
    {
        var root = JsonNode.Parse(File.ReadAllText(path))?.AsObject();
        Assert.NotNull(root);
        mutation(root!);
        File.WriteAllText(
            path,
            root!.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            }));
    }

    private static BaselineAttemptStateSnapshot
        ReadBaselineAttemptStateSnapshotForTest(
            RepositoryOwnedModelTrainingAttemptAddress address)
    {
        var path = Path.Combine(
            address.AttemptRoot,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .BaselineStateFileName);
        Assert.True(File.Exists(path));
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var acceptedPassport =
            root.GetProperty("AcceptedModelPassport");
        var baseline = root.GetProperty("BaselineQuality");
        var calibrationResult = baseline.GetProperty("Calibration");
        var calibrationReport = calibrationResult.GetProperty("Calibration");
        var noisePassport =
            calibrationReport.GetProperty("MeasurementNoisePassport");
        return new BaselineAttemptStateSnapshot(
            root.GetProperty("FormatVersion").GetInt32(),
            root.GetProperty("TrainingRunId").GetString()!,
            root.GetProperty("TrainingRunIdSha256").GetString()!,
            root.GetProperty("AttemptRootRelativePath").GetString()!,
            root.GetProperty("AttemptClaimRelativePath").GetString()!,
            acceptedPassport.GetProperty("ManifestSha256").GetString()!,
            acceptedPassport.GetProperty("WeightsSha256").GetString()!,
            acceptedPassport.GetProperty("WeightsByteLength").GetInt64(),
            root.GetProperty("DpoManifestRelativePath").GetString()!,
            root.GetProperty("DpoManifestSha256").GetString()!,
            root.GetProperty("TrainingPairId").GetString()!,
            root.GetProperty("TrainingPairRelativePath").GetString()!,
            root.GetProperty("TrainingPairSha256").GetString()!,
            baseline.GetProperty("ExpectedEvaluationCallCount").GetInt32(),
            baseline.GetProperty("ObservedEvaluationCallCount").GetInt32(),
            calibrationReport.GetProperty("Observations").GetArrayLength(),
            calibrationReport.GetProperty("HeldOutCaseCount").GetInt32(),
            noisePassport.GetProperty("MeanScore").GetDouble());
    }

    private static CodeDocumentationSavedModelAssessmentModelPassport
        CreateAcceptedPassportForTest(
            RepositoryOwnedModelCheckpointManifest parent,
            long maximumModelSerializationBytes)
        => new(
            parent.Domain,
            parent.DesignPattern,
            parent.ModelId,
            parent.Version,
            parent.Step,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256,
            parent.RepositoryRelativeWeightsPath,
            parent.WeightsSha256,
            parent.WeightsByteLength,
            maximumModelSerializationBytes,
            CodeDocumentationSavedModelAssessmentModelPassport
                .AcceptedRepositoryCheckpointSourceKind,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256);

    private static CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState
        CreateBaselineAttemptStateForTest(
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            CodeDocumentationSavedModelAssessmentModelPassport acceptedPassport,
            RepositoryOwnedCodeDocumentationDpoSetup dpoSetup,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality
                baselineQuality)
    {
        var trainingPair = dpoSetup.Receipt.OrderedPairs[0];
        return new CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState(
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .CurrentFormatVersion,
            attemptAddress.TrainingRunId,
            attemptAddress.TrainingRunIdSha256,
            attemptAddress.AttemptRootRelativePath,
            attemptAddress.AttemptClaimRelativePath,
            attemptAddress.DomainSlug,
            attemptAddress.DesignPatternSlug,
            attemptAddress.ModelIdSlug,
            attemptAddress.ModelVersion,
            acceptedPassport,
            dpoSetup.Address.ManifestRelativePath,
            dpoSetup.Receipt.ManifestSha256,
            trainingPair.PairId,
            trainingPair.RepositoryRelativePairFilePath,
            trainingPair.PairFileSha256,
            baselineQuality);
    }

    private static void LoadFullCheckpointIntoBaseOnlyClone(LlmModel source)
    {
        using var stream = new MemoryStream();
        source.SaveCheckpointToStream(stream);
        stream.Position = 0;
        var clone = new LlmModel(source.Config);
        clone.LoadCheckpoint(stream);
    }

    private static bool AnyLoraBMatrixContainsNonZeroValues(LlmModel model)
    {
        var observedAttachedAdapter = false;
        for (var blockIndex = 0; blockIndex < model.Config.NumLayers; blockIndex++)
        {
            foreach (var projection in new[] { "q_proj", "v_proj" })
            {
                var adapter = model.GetAttachedProjectionAdapter(
                    blockIndex,
                    projection);
                if (adapter is null)
                    continue;

                observedAttachedAdapter = true;
                foreach (var (suffix, parameter) in adapter.AdapterParameters)
                {
                    if (!StringComparer.Ordinal.Equals(suffix, "lora_b"))
                        continue;

                    foreach (var value in parameter.Value.Data)
                    {
                        if (float.IsFinite(value) && MathF.Abs(value) > 0f)
                            return true;
                    }
                }
            }
        }

        Assert.True(
            observedAttachedAdapter,
            "Ожидались присоединённые LoRA-адаптеры для q_proj/v_proj.");
        return false;
    }

    private static CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext
        CreateQualityGateContext(
            RepositoryOwnedReferenceCorpusOpenResult corpus,
            BpeTokenizer tokenizer,
            IReadOnlyList<bool> scriptedOutcomes,
            Func<int, string>? requestIdFactory = null,
            Action<ScriptedQualityGateEvaluationClient>? captureClient = null,
            ContinuousLearningTp1DpoConfiguration? evaluatorConfiguration = null,
            Func<int, TeacherEvaluationInferenceResult, TeacherEvaluationInferenceResult>?
                responseTransform = null)
    {
        evaluatorConfiguration ??= CreateTeacherConfiguration(
            maxTeacherCalls: 50);
        var evaluatorClient = new ScriptedQualityGateEvaluationClient(
            evaluatorConfiguration,
            scriptedOutcomes,
            requestIdFactory,
            responseTransform);
        captureClient?.Invoke(evaluatorClient);
        return new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext(
            corpus.Corpus,
            corpus.Manifest,
            tokenizer,
            evaluatorClient,
            evaluatorConfiguration,
            new CodeDocumentationSavedModelAssessmentGenerationSettings(
                MaxNewTokens: 1,
                Temperature: 0f,
                TopK: 1,
                TopP: 1f,
                Seed: 17,
                MaxHeldOutCases: 5,
                MaxAnswerChars: 256),
            MeasurementNoisePolicyConfig.Canonical,
            TimeSpan.FromSeconds(10),
            DateTimeOffset.UnixEpoch.AddHours(1),
            ["test:code-documentation-quality-gate-context"]);
    }

    private static bool[] CreateQualityOutcomes(
        bool baselinePass,
        bool trialPass)
    {
        const int heldOutCases = 5;
        var callsPerSeries =
            heldOutCases * MeasurementNoisePolicyConfig.Canonical.RepeatCount;
        return Enumerable
            .Repeat(baselinePass, callsPerSeries)
            .Concat(Enumerable.Repeat(trialPass, callsPerSeries))
            .ToArray();
    }

    private static RepositoryOwnedReferenceCorpusOpenResult
        SealQualityReferenceCorpus(
            string repositoryRoot,
            string corpusVersion = QualityCorpusVersion,
            IEnumerable<FrozenReferenceEntry>? suppliedEntries = null)
    {
        var entries = suppliedEntries?.ToArray() ?? Enumerable
            .Range(1, 5)
            .Select(index => CreateQualityReferenceEntry(
                "case " + index.ToString("D3"),
                "ok " + index.ToString("D3"),
                FrozenReferencePartition.HeldOut,
                "heldout-" + index.ToString("D3")))
            .Prepend(CreateQualityReferenceEntry(
                "train 000",
                "ok train",
                FrozenReferencePartition.Train,
                "train"))
            .ToArray();
        var corpus = FrozenReferenceCorpus.Build(
            "csharp-documentation-quality-gate-fixture",
            corpusVersion,
            DateTimeOffset.UnixEpoch,
            entries);
        Assert.True(corpus.IsSuccess, corpus.ErrorMessage);

        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                QualitySubjectSlug,
                corpusVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);

        var sealedManifest =
            RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
                address.Value!,
                corpus.Value!);
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);

        var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(
            address.Value!);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        return opened.Value!;
    }

    private static FrozenReferenceEntry CreateQualityReferenceEntry(
        string ownerInput,
        string referenceAnswer,
        FrozenReferencePartition partition,
        string scope,
        string? sourceFingerprint = null)
    {
        var provenance = PaidReferenceProvenance.Create(
            "paid-reference-quality-fixture",
            "closed-doc-quality-reference-fixture",
            "2026-09-09",
            "quality-reference-request-" + scope,
            DateTimeOffset.UnixEpoch,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer));
        Assert.True(provenance.IsSuccess, provenance.ErrorMessage);
        var entry = FrozenReferenceEntry.Create(
            ownerInput,
            referenceAnswer,
            provenance.Value!,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            QualitySubjectSlug,
            partition,
            sourceFingerprint ?? "sha256:" + Sha256Hex("quality-source-" + scope));
        Assert.True(entry.IsSuccess, entry.ErrorMessage);
        return entry.Value!;
    }

    private static CodeDocumentationTrainingRunResult CreateRoutingRun(
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt receipt,
        IReadOnlyList<ContinuousLearning.DpoPair> repositoryOwnedPairs)
        => new(
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
            CpuMilliseconds: 12.5,
            PeakWorkingSetMb: 64,
            ElapsedMilliseconds: 25,
            DpoPairs:
            [
                new RoutingDpoPair(
                    "routing-pair-001",
                    "Document this C# method.",
                    "documentation unit",
                    "translation unit",
                    "code-doc-eval-001",
                    OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                    OrchestrationRoutingCurriculum.TranslationRuEnUnitId)
            ],
            receipt,
            repositoryOwnedPairs.ToArray(),
            Cases: []);

    internal static RepositoryOwnedCodeDocumentationDpoSetup SealRepositoryOwnedDpo(
        string repositoryRoot,
        string scope,
        string datasetVersion =
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DefaultDatasetVersion,
        string prompt = "Document the repository-owned code path.",
        string chosen = "Use the saved DPO dataset and publish the trained model.",
        string rejected = "Recreate the DPO dataset and drop the trained model.")
    {
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
            datasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);

        var identity = CreateRepositoryOwnedDpoRequestIdentity(scope);
        var pair = CreateRepositoryOwnedDpoPair(
            identity,
            scope,
            prompt,
            chosen,
            rejected);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            identity,
            [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        var dpoAddress = address.Value!;
        var dpoManifest = manifest.Value!;

        var receipt = new CodeDocumentationRepositoryOwnedDpoDatasetReceipt(
            RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            dpoManifest.ManifestSha256,
            dpoAddress.ManifestRelativePath,
            dpoManifest.PairCount,
            dpoManifest.RequestIdentity.RequestIdentitySha256,
            dpoManifest.OrderedPairs.ToArray());
        return new RepositoryOwnedCodeDocumentationDpoSetup(
            dpoAddress,
            identity,
            new RepositoryOwnedDpoDatasetManifestIdentity(
                dpoAddress.ManifestRelativePath,
                dpoManifest.ManifestSha256),
            receipt,
            [pair]);
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryOwnedDpoRequestIdentity(string scope)
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [Sha256Hex("paid CodeDocumentation reference " + scope)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen-code-doc-test:9b",
                    Hex('c'),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    Hex('d')),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "code-documentation-training-publication",
                    "v1",
                    Sha256Hex("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "code-documentation-training-publication-tests",
                    "v1",
                    Sha256Hex("creator policy " + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static ContinuousLearning.DpoPair CreateRepositoryOwnedDpoPair(
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        string scope,
        string prompt = "Document the repository-owned code path.",
        string chosen = "Use the saved DPO dataset and publish the trained model.",
        string rejected = "Recreate the DPO dataset and drop the trained model.")
    {
        var passport = new TrainingTeacherProvenance(
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
            "repository-owned-training-publication-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind = identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

        var bound = DpoPairTrainingContentAuthority.Bind(
            new ContinuousLearning.DpoPair(
                prompt,
                chosen,
                rejected,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = passport,
                GovernanceDecision = new TrainingDataGovernanceDecision(
                    licenseAccepted: true,
                    isQuarantined: false,
                    evidenceRefs:
                    [
                        "sha256:" + Sha256Hex("governance " + scope)
                    ]),
                ChosenAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.OpenTeacher,
                    passport),
                RejectedAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.LocalModel)
            });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private static IServerOwnedDpoTrainingLeaseBroker CreateAcceptingBroker()
        => (IServerOwnedDpoTrainingLeaseBroker)
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                _ => OperationResult<LegalTrainingTeacherAdmission>.Fail(
                    "CodeDocumentation coordinator must use content-bound leases.",
                    "UNBOUND_VALIDATOR_CALLED"),
                static () => true,
                concurrencyObserverForTesting: null,
                sealReplay: null,
                verifyReplay: null,
                sealExecutableManifest: null,
                verifyExecutableManifest: null,
                validateBoundBatch: (_, _) =>
                    OperationResult<LegalTrainingTeacherAdmission>.Ok(
                        new LegalTrainingTeacherAdmission(
                            [],
                            ExternalTeacherInvocationCount: 0,
                            PolicyIdentityRef:
                            "policy/code-documentation-training-publication-test",
                            EvidenceRefs: [])));

    private static ContinuousLearningTp1DpoConfiguration
        CreateTeacherConfiguration(
            int maxTeacherCalls,
            string modelId = "qwen-code-doc-quality-test:9b",
            string? manifestDigest = null,
            string? layerDigest = null)
    {
        var created = ContinuousLearningTp1DpoConfiguration.Create(
            modelId,
            manifestDigest ?? Hex('a'),
            layerDigest ?? Hex('b'),
            maxTeacherCalls,
            "sha256:" + Hex('c'),
            "sha256:" + Hex('d'),
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    private static LoraConfig CreateLoraConfig()
        => new()
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0,
            TargetModules = ["q_proj", "v_proj"]
        };

    private static RepositoryOwnedModelRuntimeCompatibility CreateCompatibility(
        ModelConfig config,
        string scope)
        => new(
            TrainingSourceProvenance.Identify(config).Sha256,
            Sha256Hex("topology " + scope),
            Sha256Hex("tokenizer " + scope),
            Sha256Hex("trainer " + scope),
            Sha256Hex("schema " + scope),
            "code-documentation-test-" + scope);

    private static string CreateRepositoryRoot(string scope)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"{scope}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        File.WriteAllText(Path.Combine(root, "LlmFromScratch.sln"), string.Empty);
        return root;
    }

    private static string Sha256Hex(string value)
        => Convert.ToHexString(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string Hex(char character)
        => new(character, 64);

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record BaselineAttemptStateSnapshot(
        int FormatVersion,
        string TrainingRunId,
        string TrainingRunIdSha256,
        string AttemptRootRelativePath,
        string AttemptClaimRelativePath,
        string AcceptedManifestSha256,
        string AcceptedWeightsSha256,
        long AcceptedWeightsByteLength,
        string DpoManifestRelativePath,
        string DpoManifestSha256,
        string TrainingPairId,
        string TrainingPairRelativePath,
        string TrainingPairSha256,
        int ExpectedEvaluationCallCount,
        int ObservedEvaluationCallCount,
        int CalibrationObservationCount,
        int HeldOutCaseCount,
        double BaselineMeanCorrectFraction);

    internal sealed record RepositoryOwnedCodeDocumentationDpoSetup(
        RepositoryOwnedDpoDatasetAddress Address,
        RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
        RepositoryOwnedDpoDatasetManifestIdentity Identity,
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt Receipt,
        ContinuousLearning.DpoPair[] Pairs);

    internal sealed record QualityGateCheckerObservation(
        string CaseId,
        string OwnerTask,
        string StudentAnswer,
        string ClosedReferenceAnswer,
        string RequestId)
    {
        public bool Passed { get; init; }
    }

    internal sealed class ScriptedQualityGateEvaluationClient(
        ContinuousLearningTp1DpoConfiguration configuration,
        IReadOnlyList<bool> scriptedOutcomes,
        Func<int, string>? requestIdFactory,
        Func<int, TeacherEvaluationInferenceResult, TeacherEvaluationInferenceResult>?
            responseTransform = null)
        : ILegalTeacherEvaluationInferenceClient
    {
        private readonly object _gate = new();
        private readonly Queue<bool> _scriptedOutcomes = new(
            scriptedOutcomes);
        private int _calls;
        private int _issuedCallCount;
        private int _completedResponseCount;

        internal List<QualityGateCheckerObservation> Observations { get; } = [];

        internal int MaxTeacherCalls => configuration.MaxTeacherCalls;

        public int IssuedCallCount =>
            Volatile.Read(ref _issuedCallCount);

        public int CompletedResponseCount =>
            Volatile.Read(ref _completedResponseCount);

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return Task.FromResult(
                    issued.MapError<TeacherEvaluationInferenceResult>());
            Interlocked.Increment(ref _issuedCallCount);

            bool pass;
            int callNumber;
            lock (_gate)
            {
                callNumber = ++_calls;
                if (!_scriptedOutcomes.TryDequeue(out pass))
                {
                    return Task.FromResult(
                        OperationResult<TeacherEvaluationInferenceResult>.Fail(
                            "Scripted quality gate checker ran out of decisions.",
                            "SCRIPTED_QUALITY_GATE_DECISION_MISSING"));
                }
            }

            var payloadStart = prompt.IndexOf('{');
            using var document = JsonDocument.Parse(prompt[payloadStart..]);
            var root = document.RootElement;
            var requestId = requestIdFactory?.Invoke(callNumber) ??
                            "quality-gate-evaluation-" +
                            callNumber.ToString("D4");
            var observation = new QualityGateCheckerObservation(
                root.GetProperty("caseId").GetString()!,
                root.GetProperty("ownerTask").GetString()!,
                root.GetProperty("studentAnswer").GetString()!,
                root.GetProperty("closedReferenceAnswer").GetString()!,
                requestId)
            {
                Passed = pass
            };
            Observations.Add(observation);
            Interlocked.Increment(ref _completedResponseCount);

            var output = JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId,
                pass,
                reason = pass
                    ? CodeDocumentationSavedModelAssessmentService
                        .CorrectReasonCode
                    : CodeDocumentationSavedModelAssessmentService
                        .MissingEssentialBehaviorReasonCode,
            });
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                configuration.ModelId,
                configuration.ExpectedOllamaManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: false,
                requestId,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest =
                    configuration.ExpectedOllamaModelLayerDigest
            };
            var response = new TeacherEvaluationInferenceResult(output, provenance)
            {
                InferenceProfile = LocalQwenOllamaInferenceProfileAuthority.Evaluation
            };
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Ok(
                    responseTransform is null ? response : responseTransform(callNumber, response)));
        }

        public void Dispose()
        {
        }
    }
}
