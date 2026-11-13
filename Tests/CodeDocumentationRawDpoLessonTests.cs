// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModelServer;
using ModelServer.Http;
using LoRA;
using Tokenizer;
using TransformerModel.Config;
using TrainableRouter.Training;

namespace LlmFromScratch.Tests;

public sealed class CodeDocumentationRawDpoLessonTests
{
    [Fact]
    public async Task RawLesson_PreservesExactPairAndLegacyIdentity_RefusesSourceDriftBeforeFactory()
    {
        // Hypothesis: task-language answers and meaningful C# punctuation survive exact disk reopen; source drift and invalid sides are refused.
        // These are synthetic wire fixtures and an in-process stub, not evidence of platform selection, model calls or lawful real training.
        var root = Path.Combine(Path.GetTempPath(), "xiigen-raw-lesson-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        // Reuse the existing layout owner's repository shape; do not bypass its production root check.
        Write(Path.Combine(root, "LlmFromScratch.sln"), "synthetic fixture solution sentinel");
        Write(Path.Combine(root, "llm_mvp_core.sln"), "synthetic fixture solution sentinel");
        Write(Path.Combine(root, "AGENTS.md"), "synthetic fixture governance sentinel");
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        try
        {
            var directory = Path.Combine(root, "private"); Directory.CreateDirectory(directory);
            var specimen = "#nullable enable\r\npublic static class Demo { public static int? Read(int[]? x) => x?.Length; }\r\n";
            var question = "C# 12: explain Read for null and empty arrays. Answer in English.\r\n";
            var input = question + "```csharp\r\n" + specimen + "```\r\n";
            var ownerPath = Path.Combine(directory, "owner-input.txt");
            var sourcePath = Path.Combine(directory, "specimen.cs");
            var questionPath = Path.Combine(directory, "question.txt");
            Write(ownerPath, input); Write(sourcePath, specimen); Write(questionPath, question);
            object Cell(string path, string value) => new { path, rawSha256 = Hash(value), byteLength = Encoding.UTF8.GetByteCount(value) };
            var handoffPath = Path.Combine(directory, "selected-lesson-handoff.json");
            var handoff = new
            {
                schema = "rp01-actual-astra-selected-raw-train-lesson-handoff-v1",
                identity = new { selectionOrigin = "ACTUAL_PLATFORM_SUBAGENT_SELECTION", platformAgentId = "/synthetic-wire-test/selector" },
                lesson = new { partition = "TRAIN", language = "C#", declaredLanguageVersion = "12.0", caseId = "synthetic-case", familyId = "synthetic-family",
                    frozenInput = Cell(ownerPath, input), source = Cell(sourcePath, specimen), question = Cell(questionPath, question) },
                sourceUse = new { sourceKind = "ORIGINAL_SYNTHETIC_AGENT_AUTHORED_TRAIN_SPECIMEN", paidReferenceClaimed = false, realRepositorySampleClaimed = false },
                prospectiveTeacher = new { modelId = "qwen3.5:9b", modelVersion = new string('a', 64), expectedModelLayerDigest = new string('1', 64) },
                finalDpoSidesAuthoredBySelector = false, judgedBySelector = false
            };
            WriteJson(handoffPath, handoff);
            var authorizationPath = Path.Combine(directory, "authorization.json");
            WriteJson(authorizationPath, new CodeDocumentationRawLessonAuthorization("raw-code-documentation-source-authorization-v1",
                "CURRENT_HUMAN_PROJECT_INSTRUCTION", "synthetic-test-operator",
                "Only this synthetic fixture and in-process stub; no real model, optimizer or external selection claim.",
                CodeDocumentationRawDpoLessonSource.AuthoringPurpose, FileHash(handoffPath), Hash(input), "LOCAL_OLLAMA_LOOPBACK",
                "REPOSITORY_LOCAL_CANDIDATE_PAIR_THEN_DISTINCT_JUDGE_BEFORE_TRAINING"));
            var licensePath = Path.Combine(directory, "license.txt"); Write(licensePath, "Apache License\nVersion 2.0\nSynthetic test text, not model license evidence.");
            var profile = LocalQwenOllamaInferenceProfileAuthority.ManualCpuAuthoring;
            var evaluation = LocalQwenOllamaInferenceProfileAuthority.ManualEvaluation;
            var configurationPath = Path.Combine(directory, "configuration.json");
            WriteJson(configurationPath, new CodeDocumentationRawLessonConfiguration("raw-code-documentation-local-authoring-configuration-v1",
                "qwen3.5:9b", new string('a', 64), new string('1', 64), licensePath, FileHash(licensePath),
                profile.ProfileId, profile.ConfigurationSha256, evaluation.ProfileId, evaluation.ConfigurationSha256, 1, 1));
            var intakePath = Path.Combine(directory, "intake.json");
            var intake = new CodeDocumentationRawLessonIntake("raw-code-documentation-lesson-intake-v1", handoffPath, FileHash(handoffPath),
                authorizationPath, FileHash(authorizationPath), configurationPath, FileHash(configurationPath), "v0001");
            WriteJson(intakePath, intake);
            var originalHandoff = File.ReadAllBytes(handoffPath);
            foreach (var forbidden in new[] { "SEALED", "UNKNOWN" })
            {
                var changed = JsonNode.Parse(originalHandoff)!;
                if (forbidden == "SEALED") changed["lesson"]!["partition"] = forbidden;
                else changed["sourceUse"]!["sourceKind"] = forbidden;
                File.WriteAllText(handoffPath, changed.ToJsonString(), new UTF8Encoding(false));
                WriteJson(intakePath, intake with { SelectionHandoffSha256 = FileHash(handoffPath) });
                Assert.False(CodeDocumentationRawDpoLessonSource.Open(root, intakePath, FileHash(intakePath)).IsSuccess);
                Assert.False(Directory.Exists(Path.Combine(root, "training-data")));
            }
            File.WriteAllBytes(handoffPath, originalHandoff); WriteJson(intakePath, intake);
            var admitted = Success(CodeDocumentationRawDpoLessonSource.Open(root, intakePath, FileHash(intakePath)));
            Assert.Equal(input, admitted.OwnerInput);
            var factoryCalls = 0; var inferenceCalls = 0;
            // Non-CJK answers differing only by the meaningful null-conditional operator must reach the separate judge unchanged.
            const string chosen = "  For a null receiver, x?.Length returns null. e\u0301\r\n";
            const string rejected = "  For a null receiver, x.Length returns null. e\u0301\r\n";
            var context = CodeDocumentationRawDpoLessonAuthoring.CreateProductionContext(admitted) with
            {
                CreateTeacherClient = () => { factoryCalls++; return OperationResult<ITrainingTeacherInferenceClient>.Ok(new Stub(() => inferenceCalls++, chosen, rejected)); }
            };
            var request = Success(CodeDocumentationRawDpoLessonAuthoring.PrepareRequest(admitted, context));
            Assert.Null(request.PaidReferenceAnswers);
            Assert.NotNull(request.RawSource);
            var created = Success(await CodeDocumentationRawDpoLessonAuthoring.ResolveOrAuthorAsync(admitted, context));
            Assert.Equal(1, factoryCalls); Assert.Equal(1, inferenceCalls);
            var pair = Assert.Single(created.Dataset.Pairs);
            Assert.Equal(input, pair.Prompt); Assert.Equal(chosen, pair.Chosen); Assert.Equal(rejected, pair.Rejected);
            var reopened = Success(CodeDocumentationRawDpoLessonAuthoring.Reopen(admitted, context));
            Assert.Equal(chosen, Assert.Single(reopened.Pairs).Chosen); Assert.Equal(rejected, reopened.Pairs[0].Rejected);
            Assert.Equal(created.Dataset.Manifest.ManifestSha256, reopened.Manifest.ManifestSha256);
            // The live adapter must reopen this true sealed fixture without another factory call, then preserve its exact wire pair.
            var trainingRequest = Success(CodeDocumentationRawDpoTrainingComposition.CreateRequest(admitted, context,
                new Uri("http://127.0.0.1:6060"), "synthetic-raw-wire", ["test:raw-composition"], TimeSpan.FromMinutes(1)));
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingNotPerformed, trainingRequest.RoutingDisposition);
            var receipt = trainingRequest.RoutingRun.RepositoryOwnedDpoReceipt!;
            Assert.Equal(reopened.Manifest.ManifestSha256, receipt.ManifestSha256);
            Assert.Equal(reopened.Manifest.RequestIdentity.RequestIdentitySha256, receipt.RequestIdentitySha256);
            Assert.Equal(reopened.Manifest.OrderedPairs, receipt.OrderedPairs);
            Assert.Equal(reopened.Pairs.Select(value => FileDpoPairStore.ComputeIdentity(value, 5)),
                trainingRequest.RoutingRun.RepositoryOwnedDpoPairs.Select(value => FileDpoPairStore.ComputeIdentity(value, 5)));
            var transport = new CaptureRawWireTransport();
            var trainingClient = new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(transport);
            var captured = await trainingClient.TrainAndPublishAsync(trainingRequest);
            Assert.False(captured.IsSuccess); Assert.Equal("TEST_WIRE_CAPTURED_NO_TRAINING", captured.ErrorCode);
            Assert.Equal(1, transport.Calls);
            var wireJson = transport.Json!;
            var wire = JsonSerializer.Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest>(wireJson, Json)!;
            Assert.Equal(trainingRequest.RoutingDisposition, wire.RoutingDisposition);
            Success(CodeDocumentationRepositoryOwnedDpoTrainingWireContract.ValidateRoutingDisposition(wire.RoutingRun, wire.RoutingDisposition, wire.DomainSlug));
            Assert.Equal(input, wire.RoutingRun.RepositoryOwnedDpoPairs[0].Prompt);
            Assert.Equal(chosen, wire.RoutingRun.RepositoryOwnedDpoPairs[0].Chosen);
            Assert.Equal(rejected, wire.RoutingRun.RepositoryOwnedDpoPairs[0].Rejected);
            // A null marker must serialize exactly as the old positional request did: no added JSON field.
            var legacyWire = new CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest(wire.DomainSlug, wire.DesignPatternSlug,
                wire.DatasetVersion, wire.TrainingRunId, wire.RoutingRun, wire.ProvenanceRefs);
            var legacyJson = JsonSerializer.Serialize(legacyWire, Json);
            Assert.DoesNotContain("routingDisposition", legacyJson);
            var rawNode = JsonNode.Parse(wireJson)!.AsObject(); rawNode.Remove("routingDisposition");
            Assert.True(JsonNode.DeepEquals(rawNode, JsonNode.Parse(legacyJson)));
            Assert.Null(JsonSerializer.Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest>(legacyJson, Json)!.RoutingDisposition);
            // Every refusal goes through the real client and server boundaries, before transport or startup/service access.
            var badRuns = new[]
            {
                wire.RoutingRun with { TrainingExampleCount = 1 }, wire.RoutingRun with { EvaluationExampleCount = 1 },
                wire.RoutingRun with { RoutedCorrect = 1 }, wire.RoutingRun with { SourceCorrect = 1 },
                wire.RoutingRun with { VersionCorrect = 1 }, wire.RoutingRun with { TransitCorrect = 1 },
                wire.RoutingRun with { RoutingAccuracy = 1 }, wire.RoutingRun with { SourceGateAccuracy = 1 },
                wire.RoutingRun with { VersionGateAccuracy = 1 }, wire.RoutingRun with { TransitGateAccuracy = 1 },
                wire.RoutingRun with { BottomUpOrderAccuracy = 1 }, wire.RoutingRun with { CalleeSummaryTransitAccuracy = 1 },
                wire.RoutingRun with { LocalVsDelegatedAccuracy = 1 }, wire.RoutingRun with { CpuMilliseconds = 1 },
                wire.RoutingRun with { PeakWorkingSetMb = double.NaN }, wire.RoutingRun with { ElapsedMilliseconds = 1 },
                wire.RoutingRun with { Cases = [null!] }, wire.RoutingRun with { DpoPairs = [null!] },
                wire.RoutingRun with { RepositoryOwnedDpoReceipt = receipt with { Source = RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset } },
                wire.RoutingRun with { RepositoryOwnedDpoPairs = [pair with { Chosen = chosen + "changed" }] },
                wire.RoutingRun with { RepositoryOwnedDpoPairs = [pair with { Domain = "orchestration" }] }
            };
            using var emptyServices = new ServiceCollection().BuildServiceProvider();
            foreach (var badWire in badRuns.Select(run => wire with { RoutingRun = run }).Append(wire with { RoutingDisposition = "UNKNOWN" }))
            {
                var refusedClient = await trainingClient.TrainAndPublishAsync(trainingRequest with
                { RoutingRun = badWire.RoutingRun, RoutingDisposition = badWire.RoutingDisposition });
                Assert.False(refusedClient.IsSuccess);
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingDispositionInvalidErrorCode, refusedClient.ErrorCode);
                var refusedServer = await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteCoreAsync(badWire, emptyServices,
                    DateTimeOffset.UnixEpoch, root);
                Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(refusedServer).StatusCode);
            }
            Assert.Equal(1, transport.Calls); Assert.Equal(1, factoryCalls); Assert.Equal(1, inferenceCalls);
            // Ordinary raw train-existing reaches the published scientific-hold guard before initial-choice admission.
            var missingChoice = await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteCoreAsync(wire, emptyServices, DateTimeOffset.UnixEpoch, root);
            Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(missingChoice).StatusCode);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ScientificHoldRequired,
                Assert.IsType<CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>(Assert.IsAssignableFrom<IValueHttpResult>(missingChoice).Value).ErrorCode);
            var tokenizer = new BpeTokenizer(); tokenizer.Train("synthetic startup boundary only", 256);
            var modelConfig = new ModelConfig { VocabSize = 256, ContextLen = 128, HiddenDim = 16, NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 32 };
            var zeroDropout = new LoraConfig { Rank = 8, Alpha = 16, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
            var seedAuthority = new LoraPairRandomSeedAuthority(20261014, "rp01-policy", "rp01-reference");
            var runtime = new RepositoryOwnedModelRuntimeCompatibility(new string('a', 64),
                LoraRandomStateIdentity.ComputeTopologySha256(modelConfig, zeroDropout, seedAuthority), tokenizer.ComputeStateSha256(),
                new string('c', 64), new string('d', 64), "synthetic-first-training");
            var start = new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(root, wire.DomainSlug, wire.DesignPatternSlug,
                "synthetic-root", "v1", modelConfig, runtime, RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                LoraConfig: zeroDropout, AdapterSeedAuthority: seedAuthority, BaseInitializationSeed: 20261014);
            var generation = new CodeDocumentationGenerationConfigurationResult(new(start, tokenizer, new(8, 0, 0, 1, 1, 1024)), null);
            var address = Success(RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(root, wire.DomainSlug, wire.DesignPatternSlug, "synthetic-root", "v1", 1));
            // This is arbitrary synthetic sealed metadata, never a real model/bootstrap or optimizer invocation.
            using var syntheticWeights = new MemoryStream([1, 2, 3]);
            var parent = Success(RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(address, syntheticWeights,
                new RepositoryOwnedModelCheckpointManifestRequest(null, "synthetic initial metadata only",
                    new(receipt.RepositoryRelativeManifestPath, receipt.ManifestSha256), runtime, DateTimeOffset.UnixEpoch, ["test:initial-choice"])
                { InitialBaselineNoUpdate = true }));
            const string optimizerReason = "Current human instruction authorizes this exact saved root's first one-pair optimizer update; synthetic test only.";
            var selectors = new Dictionary<string, string?>
            {
                [CodeDocumentationInitialTrainingConfiguration.ModeEnvironmentName] = CodeDocumentationInitialTrainingConfiguration.FirstRootUpdateMode,
                [CodeDocumentationInitialTrainingConfiguration.ReasonEnvironmentName] = optimizerReason,
                [CodeDocumentationInitialTrainingConfiguration.ParentEnvironmentName] = parent.ManifestSha256
            };
            var initial = CodeDocumentationInitialTrainingConfiguration.Resolve(name => selectors.GetValueOrDefault(name), generation);
            Success(initial.RequireChoice());
            var admittedContext = new ContinuousLearningRepositoryOwnedTrainingExecutionContext(
                new(root, wire.DomainSlug, wire.DesignPatternSlug, "synthetic-root", "v1", runtime, RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied),
                modelConfig, tokenizer, IncrementalTrainer.DefaultConfig, 20261014, 1, ["test:admitted-context"]);
            var initialInputs = Success(initial.Bind(admittedContext));
            Assert.Equal(optimizerReason, initialInputs.OptimizerAuthorization.Reason);
            Assert.True(initialInputs.OptimizerAuthorization.IsWellFormed);
            Assert.Equal(0f, initialInputs.StartRequest.LoraConfig!.Dropout);
            Assert.Equal(seedAuthority, initialInputs.StartRequest.AdapterSeedAuthority);
            Assert.False(initialInputs.StartRequest.ColdStartPolicy.IsAuthorized);
            Assert.Equal(0.05f, admittedContext.LoraConfig.Dropout); // Generic context/default is untouched by the explicit first-B0 binding.
            Assert.False(initial.Bind(admittedContext with { RootInitializationSeed = 1 }).IsSuccess);
            Assert.False(initial.Bind(admittedContext with { DpoStepCount = 2 }).IsSuccess);
            var absentInitial = CodeDocumentationInitialTrainingConfiguration.Resolve(_ => null, generation);
            var absentChoice = absentInitial.RequireChoice();
            Assert.False(absentChoice.IsSuccess); Assert.Equal(CodeDocumentationInitialTrainingConfiguration.Required, absentChoice.ErrorCode);
            var absentBinding = absentInitial.Bind(admittedContext);
            Assert.False(absentBinding.IsSuccess); Assert.Equal(CodeDocumentationInitialTrainingConfiguration.Required, absentBinding.ErrorCode);
            foreach (var malformedReason in new[] { "", " ", "line\nbreak", new string('x', 513) })
            {
                selectors[CodeDocumentationInitialTrainingConfiguration.ReasonEnvironmentName] = malformedReason;
                var invalidChoice = CodeDocumentationInitialTrainingConfiguration.Resolve(name => selectors.GetValueOrDefault(name), generation);
                var malformedChoice = invalidChoice.RequireChoice();
                Assert.False(malformedChoice.IsSuccess); Assert.Equal(CodeDocumentationInitialTrainingConfiguration.Invalid, malformedChoice.ErrorCode);
                var malformedBinding = invalidChoice.Bind(admittedContext);
                Assert.False(malformedBinding.IsSuccess); Assert.Equal(CodeDocumentationInitialTrainingConfiguration.Invalid, malformedBinding.ErrorCode);
                using var invalidServices = new ServiceCollection().AddSingleton(invalidChoice).BuildServiceProvider();
                var rejectedChoice = await CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.ExecuteCoreAsync(wire, invalidServices, DateTimeOffset.UnixEpoch, root);
                Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(rejectedChoice).StatusCode);
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ScientificHoldRequired,
                    Assert.IsType<CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>(Assert.IsAssignableFrom<IValueHttpResult>(rejectedChoice).Value).ErrorCode);
            }
            selectors[CodeDocumentationInitialTrainingConfiguration.ReasonEnvironmentName] = optimizerReason;
            selectors[CodeDocumentationInitialTrainingConfiguration.ModeEnvironmentName] = "AUTO_RESET";
            Assert.False(CodeDocumentationInitialTrainingConfiguration.Resolve(name => selectors.GetValueOrDefault(name), generation).RequireChoice().IsSuccess);
            var injectedGrant = JsonNode.Parse(wireJson)!.AsObject(); injectedGrant["optimizerStateColdStartAuthorization"] = new JsonObject { ["reason"] = optimizerReason };
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest>(injectedGrant.ToJsonString(), Json));
            // Once a trained descendant exists, the same root grant must fail rather than reset its optimizer or spend C0 calls.
            var childAddress = Success(RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(root, wire.DomainSlug, wire.DesignPatternSlug, "synthetic-root", "v1", 2));
            using var childWeights = new MemoryStream([4, 5, 6]);
            Success(RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(childAddress, childWeights,
                new(new(parent.RepositoryRelativeManifestPath, parent.Step, parent.WeightsSha256, parent.WeightsByteLength), "",
                    new(receipt.RepositoryRelativeManifestPath, receipt.ManifestSha256), runtime, DateTimeOffset.UnixEpoch, ["test:synthetic-continuation"])));
            Assert.False(initial.Bind(admittedContext).IsSuccess);
            Success(await CodeDocumentationRawDpoLessonAuthoring.ResolveOrAuthorAsync(admitted, context));
            Assert.Equal(1, factoryCalls); Assert.Equal(1, inferenceCalls);

            // Caller-side replacements cannot mutate the privately captured source preimage.
            var replaced = admitted.Identity with { OwnerInputSha256 = new string('f', 64) };
            Assert.NotEqual(replaced.OwnerInputSha256, admitted.Identity.OwnerInputSha256);
            var driftIdentity = Success(RepositoryOwnedDpoDatasetManifestSealer.CreateRawRequestIdentity(replaced,
                request.OpenTeacher, request.GenerationPolicy, request.Creator));
            Assert.False(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(admitted.Address, driftIdentity).IsSuccess);
            Assert.False(RepositoryOwnedDpoDatasetManifestSealer.IsValidRequestIdentity(request with { RawSource = replaced }));
            Assert.False(RepositoryOwnedDpoDatasetManifestSealer.CreateRawRequestIdentity(admitted.Identity with { Partition = "SEALED" },
                request.OpenTeacher, request.GenerationPolicy, request.Creator).IsSuccess);
            Assert.False(RepositoryOwnedDpoDatasetManifestSealer.CreateRawRequestIdentity(admitted.Identity with { SourceKind = "UNKNOWN" },
                request.OpenTeacher, request.GenerationPolicy, request.Creator).IsSuccess);
            var foreign = Success(RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root, "orchestration", "ilearning-unit", "v0001"));
            Assert.False(RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(foreign, request, [pair with { Domain = "orchestration" }]).IsSuccess);
            Assert.False(Directory.Exists(foreign.DatasetRoot));

            // Changing a selected file is refused before even constructing a client, including on a disk hit.
            Write(ownerPath, input + "changed");
            var drift = await CodeDocumentationRawDpoLessonAuthoring.ResolveOrAuthorAsync(admitted, context);
            Assert.False(drift.IsSuccess); Assert.Equal(CodeDocumentationRawDpoLessonSource.DriftErrorCode, drift.ErrorCode);
            Assert.Equal(1, factoryCalls); Assert.Equal(1, inferenceCalls);
            Assert.False(CodeDocumentationRawDpoLessonSource.Open(root, intakePath, FileHash(intakePath)).IsSuccess);
            Assert.False(CodeDocumentationRawDpoTrainingComposition.CreateRequest(admitted, context, trainingRequest.BaseUri,
                trainingRequest.TrainingRunId, trainingRequest.ProvenanceRefs, trainingRequest.Timeout).IsSuccess);

            var legacy = Success(RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity([new string('a', 64)],
                new("Qwen", "qwen3.5:9b", new string('a', 64), "ollama_model_layer_sha256", new string('1', 64)),
                new("test-policy", "v1", new string('b', 64)), new("test-creator", "v1", new string('c', 64))));
            // Independently calculated from the pre-change fixed-order v1 JSON preimage.
            Assert.Equal("1a1a123e2d2d1f8401449f13d83b59d2b58aebb9f824de4eb8d98ceb33495815", legacy.RequestIdentitySha256);
            Assert.DoesNotContain("rawSource", Encoding.UTF8.GetString(RepositoryOwnedDpoDatasetManifestSealer.CopyCanonicalRequestIdentityBytes(legacy)));

            // Exercise the actual parse/author boundary, not a test-only predicate, for exact identity and whitespace-only refusal.
            Write(ownerPath, input);
            var invalidSides = new[] { (Chosen: chosen, Rejected: chosen), (Chosen: " \r\n\t", Rejected: rejected) };
            for (var index = 0; index < invalidSides.Length; index++)
            {
                WriteJson(intakePath, intake with { DatasetVersion = "invalid-output-" + index });
                var invalidSource = Success(CodeDocumentationRawDpoLessonSource.Open(root, intakePath, FileHash(intakePath)));
                var sides = invalidSides[index];
                var invalidContext = CodeDocumentationRawDpoLessonAuthoring.CreateProductionContext(invalidSource) with
                {
                    CreateTeacherClient = () => { factoryCalls++; return OperationResult<ITrainingTeacherInferenceClient>.Ok(new Stub(() => inferenceCalls++, sides.Chosen, sides.Rejected)); }
                };
                var refused = await CodeDocumentationRawDpoLessonAuthoring.ResolveOrAuthorAsync(invalidSource, invalidContext);
                Assert.False(refused.IsSuccess);
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherOutputInvalidErrorCode, refused.ErrorCode);
                Assert.False(File.Exists(invalidSource.Address.ManifestPath));
            }
            Assert.Equal(3, factoryCalls); Assert.Equal(3, inferenceCalls);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private sealed class CaptureRawWireTransport : IModelServerJsonTransport
    {
        public int Calls { get; private set; }
        public string? Json { get; private set; }
        public Task<OperationResult<ModelServerJsonPostResponse<TResponse>>> PostJsonAsync<TRequest, TResponse>(Uri baseUri,
            string relativePath, TRequest payload, TimeSpan timeout, CancellationToken cancellationToken = default)
            where TRequest : class where TResponse : class
        {
            Calls++; Json = JsonSerializer.Serialize(payload, CodeDocumentationRawDpoLessonTests.Json);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingWireContract.EndpointPath, relativePath);
            return Task.FromResult(OperationResult<ModelServerJsonPostResponse<TResponse>>.Fail("Captured JSON only; no HTTP or training.", "TEST_WIRE_CAPTURED_NO_TRAINING"));
        }
    }

    private sealed class Stub(Action observed, string chosen, string rejected) : ITrainingTeacherInferenceClient
    {
        public TrainingTeacherInferenceProfileEvidence AuthoringProfile => LocalQwenOllamaInferenceProfileAuthority.ManualCpuAuthoring;
        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(string prompt, ITrainingTeacherCallPermit permit, CancellationToken cancellationToken)
        {
            Assert.True(permit.TryIssue().IsSuccess); observed();
            using var transport = JsonDocument.Parse(prompt);
            Assert.Contains("#nullable enable\r\n", transport.RootElement.GetProperty("raw_task").GetString());
            Assert.Contains("Answer in English.", transport.RootElement.GetProperty("raw_task").GetString());
            Assert.Contains("requested answer language", transport.RootElement.GetProperty("instruction").GetString());
            Assert.DoesNotContain("in Chinese", transport.RootElement.GetProperty("instruction").GetString());
            var passport = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput, OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight, TrainingTeacherHostingProvider.Ollama, "qwen3.5:9b", new string('a', 64),
                TrainingTeacherModelLicense.Apache20, false, false, false, true, new string('d', 64), 1)
            { ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind, ModelArtifactDigest = new string('1', 64) };
            return Task.FromResult(OperationResult<TrainingTeacherInferenceResult>.Ok(new(JsonSerializer.Serialize(new { chosen, rejected }), passport)
                { InferenceProfile = AuthoringProfile }));
        }
        public void Dispose() { }
    }
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static void WriteJson<T>(string path, T value) => File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(value, Json));
    private static void Write(string path, string value) => File.WriteAllText(path, value, new UTF8Encoding(false, true));
    private static string FileHash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static T Success<T>(OperationResult<T> result) { Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage); return result.Value!; }
}
