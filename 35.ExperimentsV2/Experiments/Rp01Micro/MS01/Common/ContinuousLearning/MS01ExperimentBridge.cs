// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// MS01 bridge. It lives inside the copied ContinuousLearning assembly because the real loader and the
// friend-only internals are reachable only here. This stage implements the one-time producer of the
// shared learned start h512-dt-joint/v1 per contract 33: original A semantics, exactly 1000 supervised
// updates, then the mandated terminal save order and the full readback before the bundle is published.

namespace ContinuousLearning;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using LoRA;
using ModelServer;
using TensorCore.Core;
using Tokenizer;
using TrainableRouter.Training;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;

public static partial class MS01ExperimentBridge
{
    public const string CompiledVariant =
#if MS01_VARIANT_B
        "B";
#else
        "A";
#endif

    private const string RepositoryRoot = "C:/Projects/llm_mvp_core";
    private const string HostRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MS01";
    private const string ConfigSha256 = "e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40";
    private const string TokenizerSha256 = "e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066";
    private const string B0ManifestSha256 = "c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d";
    private const string B0WeightsSha256 = "a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911";
    private const string FamilyId = "h512-dt-joint";
    private const string ProducerCard =
        "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-RESPONSE-MASK.md";
    private const int PreparationUpdates = 1000;
    private const int PreparationTotalSteps = 1004;
    private const float PreparationLearningRate = 0.001f;

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    // The two pinned lessons of this card, literal. Field order id,prompt,chosen,rejected.
    private static readonly Row[] Rows =
    [
        new("D-T1",
            "Explain in one short English sentence what the method returns. Describe argument order. Code: int Delta(int first,int second){return first-second;}",
            "Returns the first argument minus the second argument.",
            "Returns the second argument minus the first argument."),
        new("D-T2",
            "Explain in one short English sentence what the method returns. Describe argument order. Code: int Delta(int first,int second){return second-first;}",
            "Returns the second argument minus the first argument.",
            "Returns the first argument minus the second argument.")
    ];

    public static Task<int> PrepareLearnedStartAsync(string sharedRoot, string ownerNonce, CancellationToken ct)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var startedUtc = DateTimeOffset.UtcNow;
            var commonRoot = $"{HostRoot}/results/attempt-0001/common";

            // The owner script already reserved the shared root claim; the child only adds its study claim.
            if (!File.Exists($"{sharedRoot}/execution.claim"))
                throw new InvalidOperationException("MS01_SHARED_ROOT_CLAIM_MISSING: the owner must reserve it first");
            foreach (var guard in new[] { "bundle.json", "S0.weights.bin", "preparation.json" })
                if (File.Exists($"{sharedRoot}/{guard}"))
                    throw new IOException($"MS01_SHARED_START_EXISTS_OR_UNKNOWN: {guard} is preserved, no new preparation");

            Directory.CreateDirectory(commonRoot);
            WriteNew($"{commonRoot}/study.claim", new
            {
                familyId = FamilyId,
                scope = "producer-study",
                ownerNonce,
                pid = process.Id,
                processStartUtc = process.StartTime.ToUniversalTime(),
                issuedUtc = startedUtc,
                producerBranch = "original A semantics only; no A/B arm in producer"
            });

            // The assigned data bytes: UTF-8 without BOM, LF between rows and one final LF.
            Beat("producer: authoring the pinned data bytes");
            var dataSourceText = string.Concat(Rows.Select(row => JsonSerializer.Serialize(new
            {
                id = row.Id,
                prompt = row.Prompt,
                chosen = row.Chosen,
                rejected = row.Rejected
            }) + "\n"));
            var dataSourceBytes = new UTF8Encoding(false).GetBytes(dataSourceText);

            Beat("producer: loading the canonical saved root W0");
            var models = LoadModels(ct);
            foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;

            // The joint carrier of this card: tokenizer.Encode(Prompt + LF + Chosen), response spans by bytes.
            var vocabulary = TokenBytes(models.Tokenizer);
            var carriers = Rows.Select(row => BuildCarrier(row, models.Tokenizer, vocabulary, models.Policy.Config.ContextLen)).ToArray();

            Beat("producer: readout of the untrained start W0");
            var forwards = 0;
            var panelW0 = carriers.Select(carrier => Score(models.Policy, carrier, ref forwards)).ToArray();

            var trainer = new Trainer(new TrainingConfig
            {
                BatchSize = 1,
                SeqLen = 2048,
                WarmupSteps = 0,
                TotalSteps = PreparationTotalSteps,
                MaxLr = PreparationLearningRate,
                MinLr = PreparationLearningRate,
                GradAccumSteps = 1,
                GradClipNorm = 1f,
                WeightDecay = 0f
            });
            var optimizer = new AdamW(0.9f, 0.95f, 0f, 1e-8f);
            var losses = new List<float>(PreparationUpdates);
            var watch = Stopwatch.StartNew();
            var allocated0 = GC.GetTotalAllocatedBytes(precise: true);

            // Exactly 1000 real updates, alternating the two rows: 500 presentations each, no early stop.
            for (var i = 0; i < PreparationUpdates; i++)
            {
                ct.ThrowIfCancellationRequested();
                losses.Add(trainer.TrainSupervisedNextTokenStep(models.Policy, optimizer, carriers[i % 2].Ids, 1f));
                if ((i + 1) % 50 == 0) Beat($"producer: update {i + 1}/{PreparationUpdates} loss={losses[^1]:F6}");
            }

            watch.Stop();
            var allocated = GC.GetTotalAllocatedBytes(precise: true) - allocated0;

            Beat("producer: readout of the learned start S0");
            var panelS0 = carriers.Select(carrier => Score(models.Policy, carrier, ref forwards)).ToArray();

            // Qualification of this family: both whole responses exact at S0, at least one acquired.
            var acquired = new List<string>();
            for (var i = 0; i < Rows.Length; i++)
                if (!panelW0[i].ResponseExact && panelS0[i].ResponseExact) acquired.Add(Rows[i].Id);
            var allExactS0 = panelS0.All(score => score.ResponseExact);
            var qualified = allExactS0 && acquired.Count > 0;
            var failureReason = qualified ? null
                : !allExactS0 ? "PREPARATION_COMPLETED_LEARNING_UNQUALIFIED: not both whole responses are exact at S0"
                : "NO_ACQUISITION_CONTRAST: every pinned response was already exact at W0";

            // ── mandated terminal save order: no model calls from here on ─────────────────────────
            // Hold the facts, then write weights, optimizer, RNG and the receipt independently. No
            // validation runs between them, so one write failure cannot skip the remaining state.
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(models.LoraConfig, models.Seeds);
            var snapshot = models.Policy.CaptureRandomState(topology, loraIdentity, PreparationUpdates);
            var bindings = AdapterBindings(models);
            var exported = optimizer.ExportPortableState(bindings);
            var optimizerBytes = exported.ToBytes();

            var outcomes = new List<object>();
            var weightsPath = $"{sharedRoot}/S0.weights.bin";
            outcomes.Add(Attempt("S0.weights.bin", () =>
            {
                using var stream = new FileStream(weightsPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                models.Policy.SaveCheckpointToStream(stream);
                stream.Flush(flushToDisk: true);
            }));
            outcomes.Add(Attempt("S0.optimizer.bin", () =>
            {
                using var stream = new FileStream($"{sharedRoot}/S0.optimizer.bin", FileMode.CreateNew, FileAccess.Write, FileShare.None);
                stream.Write(optimizerBytes);
                stream.Flush(flushToDisk: true);
            }));
            outcomes.Add(Attempt("S0.rng.json", () =>
            {
                using var stream = new FileStream($"{sharedRoot}/S0.rng.json", FileMode.CreateNew, FileAccess.Write, FileShare.None);
                stream.Write(JsonSerializer.SerializeToUtf8Bytes(snapshot));
                stream.Flush(flushToDisk: true);
            }));

            var preparation = new
            {
                familyId = FamilyId,
                stage = "SHARED_LEARNED_START_PRODUCER",
                producerCard = ProducerCard,
                learning_qualified = qualified,
                completed_updates = PreparationUpdates,
                failure_reason = failureReason,
                acquired_ids = acquired,
                exact_by_id_W0 = Rows.Select((row, i) => new { id = row.Id, exact = panelW0[i].ResponseExact }).ToArray(),
                exact_by_id_S0 = Rows.Select((row, i) => new { id = row.Id, exact = panelS0[i].ResponseExact }).ToArray(),
                panel = new
                {
                    W0 = Rows.Select((row, i) => new { id = row.Id, score = panelW0[i] }).ToArray(),
                    S0 = Rows.Select((row, i) => new { id = row.Id, score = panelS0[i] }).ToArray()
                },
                carrier = new
                {
                    rule = "joint tokenizer.Encode(Prompt + LF + Chosen); inputs ids[0..T-2], targets ids[1..T-1]",
                    rows = carriers.Select((carrier, i) => new
                    {
                        id = Rows[i].Id,
                        tokens = carrier.Ids.Count,
                        promptBytes = carrier.PromptByteCount,
                        boundary = carrier.Boundary,
                        responseTargets = carrier.ResponseTargets.Count
                    }).ToArray()
                },
                supervisedLoss = new { first = losses[0], last = losses[^1] },
                costs = new
                {
                    wallMs = watch.Elapsed.TotalMilliseconds,
                    allocatedBytes = allocated,
                    commonReadoutForwards = forwards,
                    updates = PreparationUpdates,
                    peakWorkingSetBytes = Peak(process)
                },
                policyOrigin = "SFT-derived policy from the pinned W0 root; not a canonical DPO parent manifest",
                origin = new { manifestSha256 = B0ManifestSha256, weightsSha256 = B0WeightsSha256 },
                rng = new { topologySha256 = topology, loraConfigurationSha256 = loraIdentity, preparationStep = PreparationUpdates },
                optimizerStep = exported.Step,
                writeOutcomes = outcomes,
                limitation = "Narrow whole-response memorisation of two pinned rows; not a general rule, not a held-out result."
            };
            outcomes.Add(Attempt("preparation.json", () => WriteNew($"{sharedRoot}/preparation.json", preparation)));
            WriteNew($"{commonRoot}/preparation.json", preparation);

            if (!qualified)
            {
                Console.WriteLine($"PREPARATION_UNQUALIFIED acquired={acquired.Count} allExact={allExactS0}");
                return Task.FromResult(3);
            }

            // ── only now: typed validation and full readback, without any Train or Forward ────────
            Beat("producer: validating the saved weights and reloading them into a fresh copy");
            using (var verify = new FileStream(weightsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                verify.Position = 0;
                var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(verify, models.Policy.Config,
                    new FullCheckpointLoraTopology(8, new[] { "q_proj", "v_proj" }));
                if (!checkedArtifact.IsSuccess || checkedArtifact.Value != true)
                    throw new InvalidOperationException("MS01_WEIGHTS_VALIDATION_FAILED: " + checkedArtifact.ErrorCode);
            }

            var fresh = models.CreateReadbackModel(ct);
            fresh.LoadCheckpoint(weightsPath);
            var live = models.Policy.GetCheckpointParameterEntries();
            var restored = fresh.GetCheckpointParameterEntries();
            var weightsIdentical = live.Count == 55 && restored.Count == 55;
            for (var i = 0; i < live.Count && weightsIdentical; i++)
                weightsIdentical = live[i].Key == restored[i].Key &&
                                   live[i].Parameter.Value.Data.Length == restored[i].Parameter.Value.Data.Length &&
                                   live[i].Parameter.Value.Data.SequenceEqual(restored[i].Parameter.Value.Data);

            // The 39 frozen base tensors must still equal W0.
            var w0 = models.CreateReadbackModel(ct);
            var baseIdentical = w0.BaseParameters.Count == 39 && models.Policy.BaseParameters.Count == 39;
            for (var i = 0; i < w0.BaseParameters.Count && baseIdentical; i++)
                baseIdentical = w0.BaseParameters[i].Value.Data.SequenceEqual(models.Policy.BaseParameters[i].Value.Data);

            Beat("producer: reading the optimizer and random state back");
            var importedState = AdamWPortableState.FromBytes(optimizerBytes)
                ?? throw new InvalidOperationException("MS01_OPTIMIZER_STATE_UNREADABLE");
            var freshOptimizer = new AdamW(0.9f, 0.95f, 0f, 1e-8f);
            var imported = freshOptimizer.TryImportPortableState(bindings, importedState);
            var reExported = imported ? freshOptimizer.ExportPortableState(bindings) : null;
            var optimizerIdentical = imported && reExported is not null &&
                                     reExported.Step == exported.Step && reExported.Step == PreparationUpdates &&
                                     reExported.ToBytes().SequenceEqual(optimizerBytes);

            var rngBytes = File.ReadAllBytes($"{sharedRoot}/S0.rng.json");
            var rngRestored = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(rngBytes)
                ?? throw new InvalidOperationException("MS01_RNG_SNAPSHOT_UNREADABLE");
            models.Policy.RestoreRandomState(rngRestored, new LlmModelRandomStateIdentity(topology, loraIdentity));
            var rngReCaptured = models.Policy.CaptureRandomState(topology, loraIdentity, PreparationUpdates);
            var rngIdentical = JsonSerializer.SerializeToUtf8Bytes(rngReCaptured).SequenceEqual(
                JsonSerializer.SerializeToUtf8Bytes(rngRestored));

            // ── metadata of the bundle, derived from exact inputs without any model call ───────────
            Beat("producer: authoring the bundle metadata");
            File.WriteAllBytes($"{sharedRoot}/data.source.jsonl", dataSourceBytes);
            File.Copy($"{RepositoryRoot}/config/code-documentation/generation-v1.json", $"{sharedRoot}/generation.json");
            File.Copy($"{RepositoryRoot}/config/code-documentation/tokenizer.json", $"{sharedRoot}/tokenizer.json");

            WriteNew($"{sharedRoot}/data.material.json", new
            {
                familyId = FamilyId,
                origin = "producer_measured_material",
                recipe = "D-T1,D-T2 literal JSONL; alternate500 each; joint Prompt+LF+Chosen",
                presentationOrder = Enumerable.Range(0, PreparationUpdates).Select(i => Rows[i % 2].Id).Take(4).ToArray(),
                presentationOrderNote = "alternating D-T1 and D-T2, 500 presentations each, 1000 in total",
                weight = 1f,
                source = new { path = ProducerCard, sha256 = Sha(dataSourceBytes), bytes = dataSourceBytes.Length },
                rows = carriers.Select((carrier, i) => new
                {
                    id = Rows[i].Id,
                    rawUtf8Base64 = Convert.ToBase64String(new UTF8Encoding(false).GetBytes(carrier.Text)),
                    ids = carrier.Ids,
                    validLength = carrier.Ids.Count,
                    promptByteCount = carrier.PromptByteCount,
                    boundary = carrier.Boundary,
                    responseMask = carrier.ResponseMask,
                    causalMask = "full causal next-token mask over the joint sequence",
                    responseTargetIndices = carrier.ResponseTargets
                }).ToArray()
            });

            WriteNew($"{sharedRoot}/authority.json", new
            {
                familyId = FamilyId,
                origin = "producer_measured",
                b0 = new { manifestSha256 = B0ManifestSha256, weightsSha256 = B0WeightsSha256, path = models.SourceWeightsPath },
                reference = "independent frozen copy of the same B0 root; never trained",
                policyOrigin = "SFT-derived policy; private research start, not a canonical DPO parent manifest",
                config = new { sha256 = ConfigSha256, tokenizerSha256 = TokenizerSha256, tokenizerStateSha256 = models.Tokenizer.ComputeStateSha256() },
                lora = new { rank = models.LoraConfig.Rank, alpha = models.LoraConfig.Alpha, dropout = models.LoraConfig.Dropout, targetModules = models.LoraConfig.TargetModules },
                namedTensors = new { total = 55, baseFrozen = 39, adapterTrainable = 16 },
                bindings = bindings.Select(binding => binding.Key).ToArray(),
                rngIdentity = new { topologySha256 = topology, loraConfigurationSha256 = loraIdentity },
                transferable = "no admission, lease or capability is serialised by design"
            });

            var produced = new[] { "S0.weights.bin", "S0.optimizer.bin", "S0.rng.json", "preparation.json", "generation.json", "tokenizer.json", "data.source.jsonl", "data.material.json", "authority.json" };
            WriteNew($"{sharedRoot}/source-manifest.json", new
            {
                familyId = FamilyId,
                producedBy = "MS01 producer, original A semantics",
                producerHost = HostRoot,
                producerCard = ProducerCard,
                sourcePin = "ab583032562d6656de9dd5ca6fa0a325fd4b1f30",
                sourceClosure = new { trainingLoop = 6, dpo = 14, continuousLearning = 81, totalNative = 101, bridge = 1 },
                compiledVariant = CompiledVariant,
                budget = new { updates = PreparationUpdates, trainingForward = PreparationUpdates, backward = PreparationUpdates, commonReadoutForward = forwards, generate = 0 },
                files = produced.Select(name => new
                {
                    file = name,
                    bytes = new FileInfo($"{sharedRoot}/{name}").Length,
                    sha256 = Sha(File.ReadAllBytes($"{sharedRoot}/{name}"))
                }).ToArray(),
                ownerNonce,
                producedUtc = DateTimeOffset.UtcNow
            });

            // ── the bundle is published last, only when every required check passed ───────────────
            var allChecksPassed = weightsIdentical && baseIdentical && optimizerIdentical && rngIdentical;
            if (!allChecksPassed)
            {
                Console.Error.WriteLine($"PRODUCER_PARTIAL weights={weightsIdentical} base39={baseIdentical} optimizer={optimizerIdentical} rng={rngIdentical}");
                return Task.FromResult(7);
            }

            WriteNew($"{sharedRoot}/bundle.json", new
            {
                status = "COMPLETE",
                familyId = FamilyId,
                version = "v1",
                qualificationUnit = "both whole TRAIN responses exact at S0 and at least one acquired",
                learning_qualified = true,
                acquired_ids = acquired,
                completed_updates = PreparationUpdates,
                optimizerStep = exported.Step,
                consumerMode = "restore_sft_optimizer_step1000",
                sourceOrigin = new { producerHost = HostRoot, producerCard = ProducerCard, b0WeightsSha256 = B0WeightsSha256 },
                readback = new { weightsIdentical, baseFrozenIdenticalToW0 = baseIdentical, optimizerIdentical, rngIdentical },
                files = produced.Select(name => new
                {
                    file = name,
                    bytes = new FileInfo($"{sharedRoot}/{name}").Length,
                    sha256 = Sha(File.ReadAllBytes($"{sharedRoot}/{name}"))
                }).ToArray(),
                publishedUtc = DateTimeOffset.UtcNow,
                limitation = "Narrow whole-response memorisation of two pinned rows; consumers add no common updates."
            });

            Console.WriteLine($"PRODUCER_COMPLETE qualified=true acquired={acquired.Count} optimizerStep={exported.Step} readback=ok");
            return Task.FromResult(0);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"PRODUCER_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(4);
        }
    }

    // ── carrier: joint ids plus byte-exact response spans ─────────────────────────────────────

    private sealed record Row(string Id, string Prompt, string Chosen, string Rejected);

    private sealed record Carrier(
        string Text, List<int> Ids, int PromptByteCount, int Boundary,
        List<int> ResponseTargets, float[] ResponseMask);

    private static Carrier BuildCarrier(Row row, BpeTokenizer tokenizer, Dictionary<int, byte[]> vocabulary, int contextLength)
    {
        var text = row.Prompt + "\n" + row.Chosen;
        var utf8 = new UTF8Encoding(false, true);
        var textBytes = utf8.GetBytes(text);
        var ids = tokenizer.Encode(text);
        if (ids.Count < 2 || ids.Count > contextLength + 1)
            throw new InvalidOperationException("MS01_CARRIER_LENGTH_OUT_OF_RANGE");

        // Cumulative byte spans of the returned ids must concatenate byte for byte into the text.
        var spans = new List<(int Start, int End)>(ids.Count);
        var cursor = 0;
        foreach (var id in ids)
        {
            if (!vocabulary.TryGetValue(id, out var bytes))
                throw new InvalidOperationException($"MS01_TOKEN_NOT_IN_VOCABULARY: {id}");
            spans.Add((cursor, cursor + bytes.Length));
            cursor += bytes.Length;
        }

        if (cursor != textBytes.Length)
            throw new InvalidOperationException("MS01_CARRIER_BYTES_MISMATCH");
        var rebuilt = new byte[cursor];
        var offset = 0;
        foreach (var id in ids)
        {
            var bytes = vocabulary[id];
            Array.Copy(bytes, 0, rebuilt, offset, bytes.Length);
            offset += bytes.Length;
        }

        if (!rebuilt.SequenceEqual(textBytes))
            throw new InvalidOperationException("MS01_CARRIER_BYTES_MISMATCH");

        var boundary = utf8.GetByteCount(row.Prompt) + 1;
        var responseTargets = new List<int>();
        var mask = new float[ids.Count - 1];
        for (var i = 0; i < ids.Count - 1; i++)
        {
            var span = spans[i + 1];
            if (span.Start >= boundary) { responseTargets.Add(i); mask[i] = 1f; }
            else if (span.End <= boundary) mask[i] = 0f;
            else throw new InvalidOperationException("MASK_BOUNDARY_CROSSING");
        }

        if (responseTargets.Count == 0) throw new InvalidOperationException("MS01_NO_RESPONSE_TARGETS");
        return new Carrier(text, ids, utf8.GetByteCount(row.Prompt), boundary, responseTargets, mask);
    }

    // The tokenizer is the one the verified loader produced; its canonical JSON carries the token bytes.
    private static Dictionary<int, byte[]> TokenBytes(BpeTokenizer tokenizer)
    {
        using var stream = new MemoryStream();
        tokenizer.Save(stream);
        var document = JsonDocument.Parse(stream.ToArray());
        var vocabulary = new Dictionary<int, byte[]>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!property.NameEquals("Vocab") && !property.NameEquals("vocab")) continue;
            foreach (var entry in property.Value.EnumerateObject())
                vocabulary[int.Parse(entry.Name)] = Convert.FromBase64String(entry.Value.GetString()!);
        }

        if (vocabulary.Count == 0) throw new InvalidOperationException("MS01_TOKENIZER_VOCABULARY_EMPTY");
        return vocabulary;
    }

    // ── teacher-forced readout over the assigned response positions only ──────────────────────

    private sealed record ResponseScore(
        int ResponseTargets, int CorrectTargets, bool ResponseExact, float MeanNll, double MeanGoldRank);

    private static ResponseScore Score(LlmModel model, Carrier carrier, ref int forwards)
    {
        var inputs = new float[carrier.Ids.Count - 1];
        for (var i = 0; i < inputs.Length; i++) inputs[i] = carrier.Ids[i];
        var logits = model.Forward(new Tensor(inputs, new TensorShape(1, inputs.Length)), training: false);
        forwards++;

        var vocabulary = logits.Shape.Dimensions[2];
        var sum = 0f;
        var correct = 0;
        long rankSum = 0;
        foreach (var i in carrier.ResponseTargets)
        {
            var target = carrier.Ids[i + 1];
            var maxValue = float.NegativeInfinity;
            var best = 0;
            for (var j = 0; j < vocabulary; j++)
            {
                var value = logits.Data[i * vocabulary + j];
                if (value > maxValue) { maxValue = value; best = j; }
            }

            var logSumExp = 0f;
            for (var j = 0; j < vocabulary; j++) logSumExp += MathF.Exp(logits.Data[i * vocabulary + j] - maxValue);
            sum += -(logits.Data[i * vocabulary + target] - maxValue - MathF.Log(logSumExp));
            if (best == target) correct++;
            var goldLogit = logits.Data[i * vocabulary + target];
            var above = 0;
            for (var j = 0; j < vocabulary; j++) if (logits.Data[i * vocabulary + j] > goldLogit) above++;
            rankSum += above + 1;
        }

        var count = carrier.ResponseTargets.Count;
        return new ResponseScore(count, correct, correct == count, sum / count, (double)rankSum / count);
    }

    // ── shared helpers ────────────────────────────────────────────────────────────────────────

    private static IReadOnlyList<AdamWParameterBinding> AdapterBindings(SimulationDpoModelContext models)
    {
        var adapters = Enumerable.Range(0, models.Policy.Blocks.Count)
            .SelectMany(block => models.LoraConfig.TargetModules
                .Select(target => (LoraLinear)models.Policy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray();
        var allowed = adapters.SelectMany(adapter => adapter.AdapterParameters)
            .Select(parameter => parameter.Parameter)
            .ToHashSet(ReferenceEqualityComparer.Instance);
        return models.Policy.GetCheckpointParameterEntries()
            .Where(entry => allowed.Contains(entry.Parameter))
            .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
            .OrderBy(binding => binding.Key, StringComparer.Ordinal)
            .ToArray();
    }

    private static object Attempt(string file, Action action)
    {
        try
        {
            action();
            return new { file, outcome = "WRITTEN" };
        }
        catch (Exception error)
        {
            return new { file, outcome = "WRITE_FAILED", error = error.GetType().Name, message = error.Message };
        }
    }

    private static SimulationDpoModelContext LoadModels(CancellationToken cancellationToken)
    {
        var verified = CodeDocumentationGenerationConfiguration.ReadVerified(name => name switch
        {
            "XIIGEN_CODE_DOCUMENTATION_GENERATION_ENABLED" => "true",
            "XIIGEN_CODE_DOCUMENTATION_GENERATION_CONFIG_SHA256" => ConfigSha256,
            _ => null
        }, RepositoryRoot, cancellationToken);
        if (!verified.IsSuccess || verified.Value is null) throw new InvalidOperationException(verified.ErrorCode);
        var options = verified.Value;

        CodeDocumentationRepositoryOwnedModelCheckpointStart Load()
        {
            var loaded = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(
                options.StartRequest, cancellationToken);
            if (!loaded.IsSuccess || loaded.Value is null) throw new InvalidOperationException(loaded.ErrorCode);
            var parent = loaded.Value.ParentManifest;
            if (parent is null || parent.Step != 1 || parent.ParentCheckpoint is not null ||
                parent.ManifestSha256 != B0ManifestSha256 || parent.WeightsSha256 != B0WeightsSha256 ||
                parent.DpoApplication != RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate ||
                loaded.Value.Model.GetCheckpointParameterEntries().Count != 55 ||
                loaded.Value.Model.BaseParameters.Count != 39)
                throw new InvalidOperationException("MS01_SAVED_ROOT_IDENTITY_MISMATCH");
            return loaded.Value;
        }

        var policy = Load();
        var reference = Load();
        var weights = Path.GetFullPath(Path.Combine(RepositoryRoot, policy.ParentManifest!.RepositoryRelativeWeightsPath));
        return new(policy.Model, reference.Model, options.Tokenizer, options.StartRequest.LoraConfig!,
            options.StartRequest.AdapterSeedAuthority!, weights, B0WeightsSha256,
            token => { token.ThrowIfCancellationRequested(); return Load().Model; });
    }

    private static long Peak(Process process)
    {
        process.Refresh();
        return process.PeakWorkingSet64;
    }

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void WriteNew(string path, object payload)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, payload, Json);
    }

    private static void Beat(string message) =>
        Console.WriteLine($"[{DateTimeOffset.UtcNow:HH:mm:ss}] {message}");
}
