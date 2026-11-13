// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// MS01 consumer of card E-SFT-RESPONSE-MASK. It performs zero common updates and zero extra common
// readouts: the learned start comes from the shared bundle of family h512-dt-joint. The single factor
// is which supervised step the executor calls — the original one in arm A, the masked overload in arm B.

namespace ContinuousLearning;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DPO;
using LoRA;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;

public static partial class MS01ExperimentBridge
{
    private const string SharedBundleRoot =
        "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-joint/v1";
    private const string StudyRoot =
        "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MS01/results/attempt-0001";

    // The four pinned EXAM cases of this card, literal and in order.
    private static readonly Row[] Exam =
    [
        new("D1", "Explain in one short English sentence what the method returns. Describe argument order. Code: int Gap(int a,int b){return a-b;}",
            "Returns the first argument minus the second argument.", "Returns the second argument minus the first argument."),
        new("D2", "Explain in one short English sentence what the method returns. Describe argument order. Code: int Gap(int a,int b){return b-a;}",
            "Returns the second argument minus the first argument.", "Returns the first argument minus the second argument."),
        new("D3", "Explain in one short English sentence what the method returns. Describe argument order. Code: int Diff(int left,int right)=>left-right;",
            "Returns the first argument minus the second argument.", "Returns the second argument minus the first argument."),
        new("D4", "Explain in one short English sentence what the method returns. Describe argument order. Code: int Diff(int left,int right)=>right-left;",
            "Returns the second argument minus the first argument.", "Returns the first argument minus the second argument.")
    ];

    private static readonly CodeDocumentationStudentGenerationSettings GenerationSettings =
        new(MaxNewTokens: 64, Temperature: 0f, TopK: 0, TopP: 1f, Seed: 17, MaxAnswerChars: 160);

    // ── --prepare: freeze the inputs, verify the shared bundle, no model forward or update ───────

    public static Task<int> PrepareStudyAsync(string studyRoot, CancellationToken ct)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            if (!string.Equals(Normalize(studyRoot), StudyRoot, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"MS01_STUDY_ROOT_NOT_ASSIGNED: expected {StudyRoot}");
            if (CompiledVariant != "A")
                throw new InvalidOperationException("MS01_PREPARE_REQUIRES_VARIANT_A");
            if (File.Exists($"{studyRoot}/study.json"))
                throw new IOException("MS01_STUDY_EXISTS_OR_UNKNOWN: the existing study is preserved");

            var bundle = ReadCompleteBundle();
            Directory.CreateDirectory($"{studyRoot}/inputs");

            // The frozen panel file of this study: the four pinned EXAM cases, UTF-8 without BOM.
            var examText = string.Concat(Exam.Select(row => JsonSerializer.Serialize(new
            {
                id = row.Id, prompt = row.Prompt, chosen = row.Chosen, rejected = row.Rejected
            }) + "\n"));
            var examBytes = new UTF8Encoding(false).GetBytes(examText);
            var examPath = $"{studyRoot}/inputs/exam.jsonl";
            using (var stream = new FileStream(examPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                stream.Write(examBytes);

            // A canonical load without any forward: it proves the tokenizer and the carrier material.
            var models = LoadModels(ct);
            var vocabulary = TokenBytes(models.Tokenizer);
            var carriers = Rows.Select(row => BuildCarrier(row, models.Tokenizer, vocabulary, models.Policy.Config.ContextLen)).ToArray();

            WriteNew($"{studyRoot}/inputs.json", new
            {
                train = Rows.Select((row, i) => new
                {
                    row.Id, row.Prompt, row.Chosen, row.Rejected,
                    tokens = carriers[i].Ids.Count,
                    promptBytes = carriers[i].PromptByteCount,
                    boundary = carriers[i].Boundary,
                    responseTargets = carriers[i].ResponseTargets.Count,
                    ids = carriers[i].Ids,
                    responseMask = carriers[i].ResponseMask
                }).ToArray(),
                exam = Exam.Select(row => new { row.Id, row.Prompt, row.Chosen, row.Rejected }).ToArray(),
                panel = new { path = examPath, sha256 = Sha(examBytes), bytes = examBytes.Length },
                generationSettings = GenerationSettings,
                tokenizerStateSha256 = models.Tokenizer.ComputeStateSha256()
            });

            WriteNew($"{studyRoot}/study.json", new
            {
                experimentId = "E-SFT-RESPONSE-MASK",
                attemptRoot = StudyRoot,
                factor = "weights on shifted targets: arm A all ones, arm B zero on prompt and line feed, one on the response",
                armOrder = new[] { "A", "B" },
                sharedStart = new
                {
                    familyId = FamilyId,
                    bundle = $"{SharedBundleRoot}/bundle.json",
                    bundleSha256 = Sha(File.ReadAllBytes($"{SharedBundleRoot}/bundle.json")),
                    status = bundle.GetProperty("status").GetString(),
                    learningQualified = bundle.GetProperty("learning_qualified").GetBoolean(),
                    optimizerStep = bundle.GetProperty("optimizerStep").GetInt32(),
                    consumerMode = bundle.GetProperty("consumerMode").GetString()
                },
                budget = new
                {
                    commonUpdates = 0,
                    commonReadouts = 0,
                    armUpdates = 2,
                    armTrainingForwards = 2,
                    armBackwards = 2,
                    diagTrainForwardsPerArm = 4,
                    evaluationStatesPerArm = 2,
                    pairScoresPerArm = 16,
                    generationsPerArm = 8
                },
                noRetry = true,
                owner = new { pid = process.Id, startedUtc = DateTimeOffset.UtcNow }
            });

            WriteNew($"{studyRoot}/dependency-manifest.json", new
            {
                sourcePin = "ab583032562d6656de9dd5ca6fa0a325fd4b1f30",
                sourceClosure = new { trainingLoop = 6, dpo = 14, continuousLearning = 81, totalNative = 101 },
                replacedFile = "04.TrainingLoop/Trainer.cs copied into A/ and B/; both carry the masked overload, the factor is the call site",
                binaryBoundary = "C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/bin/Release/net9.0 with exactly 27 assigned assemblies",
                addedExperimentFiles = new[] { "MS01ExperimentBridge.cs", "MS01ConsumerBridge.cs", "MS01EvaluateResearch.cs", "ExecuteMasked in SupervisedTrainingExecution.cs" },
                productChanged = false
            });

            Console.WriteLine("PREPARE_DONE modelForwards=0 updates=0 generations=0");
            return Task.FromResult(0);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"PREPARE_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(5);
        }
    }

    // ── --arm A|B: restore the shared start, two supervised updates, panels before and after ────

    public static Task<int> RunArmAsync(string arm, string armRoot, CancellationToken ct)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var startedUtc = DateTimeOffset.UtcNow;
            if (arm != CompiledVariant)
                throw new InvalidOperationException($"MS01_VARIANT_MISMATCH: binary is {CompiledVariant}, arm {arm} requested");
            if (!string.Equals(Normalize(armRoot), $"{StudyRoot}/{arm}", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"MS01_ARM_ROOT_NOT_ASSIGNED: expected {StudyRoot}/{arm}");
            if (Directory.Exists(armRoot))
                throw new IOException("MS01_ARM_EXISTS_OR_UNKNOWN: the existing arm result is preserved");
            if (!File.Exists($"{StudyRoot}/study.json"))
                throw new InvalidOperationException("MS01_STUDY_NOT_PREPARED");

            var bundle = ReadCompleteBundle();
            var panelPath = $"{StudyRoot}/inputs/exam.jsonl";
            var panelSha = Sha(File.ReadAllBytes(panelPath));

            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(armRoot);
            var checkpoints = $"{armRoot}/checkpoints";
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(checkpoints);
            WriteNew($"{armRoot}/issued.json", new
            {
                experimentId = "E-SFT-RESPONSE-MASK",
                arm,
                compiledVariant = CompiledVariant,
                factor = arm == "A" ? "original supervised step: all shifted target weights 1"
                                    : "masked supervised step: prompt and line feed weighted 0, response weighted 1",
                sharedBundleSha256 = Sha(File.ReadAllBytes($"{SharedBundleRoot}/bundle.json")),
                pid = process.Id,
                issuedUtc = startedUtc
            });

            // Canonical construction and validation first, then the shared weights are loaded.
            Beat($"arm {arm}: canonical load, then restore of the shared learned start");
            var models = LoadModels(ct);
            foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;
            models.Policy.LoadCheckpoint($"{SharedBundleRoot}/S0.weights.bin");

            var topology = LoraRandomStateIdentity.ComputeTopologySha256(models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(models.LoraConfig, models.Seeds);
            var rngSnapshot = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
                File.ReadAllBytes($"{SharedBundleRoot}/S0.rng.json")) ?? throw new InvalidOperationException("MS01_RNG_UNREADABLE");
            models.Policy.RestoreRandomState(rngSnapshot, new LlmModelRandomStateIdentity(topology, loraIdentity));

            // The supervised optimizer continues from the shared step 1000 through keyed bindings.
            var bindings = AdapterBindings(models);
            var optimizer = new AdamW(0.9f, 0.95f, 0f, 1e-8f);
            var portable = AdamWPortableState.FromBytes(File.ReadAllBytes($"{SharedBundleRoot}/S0.optimizer.bin"))
                ?? throw new InvalidOperationException("MS01_OPTIMIZER_STATE_UNREADABLE");
            if (!optimizer.TryImportPortableState(bindings, portable))
                throw new InvalidOperationException("MS01_OPTIMIZER_IMPORT_REFUSED");
            var importedStep = optimizer.ExportPortableState(bindings).Step;
            if (importedStep != 1000) throw new InvalidOperationException($"MS01_OPTIMIZER_STEP_UNEXPECTED: {importedStep}");

            // The recipient check confirms the real attached q/v adapters; it grants no mutation permit.
            var adapters = Enumerable.Range(0, models.Policy.Blocks.Count)
                .SelectMany(block => models.LoraConfig.TargetModules
                    .Select(target => (LoraLinear)models.Policy.GetAttachedProjectionAdapter(block, target)!))
                .ToArray();
            var recipient = DpoProjectLoraRecipient.Capture(models.Policy, models.Reference, adapters, models.LoraConfig);

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
            var session = new IncrementalTrainingSession(models.Policy, models.Tokenizer, trainer, optimizer,
                B0WeightsSha256, models.Tokenizer.ComputeStateSha256(), TrainingSourceProvenance.Identify(models.Policy.Config));
            var executor = new ProductionSupervisedTrainingStepExecutor();

            var vocabulary = TokenBytes(models.Tokenizer);
            var carriers = Rows.Select(row => BuildCarrier(row, models.Tokenizer, vocabulary, models.Policy.Config.ContextLen)).ToArray();

            // The initial candidate is an exact copy of the qualified shared start inside this arm.
            var initialPath = $"{checkpoints}/initial.weights.bin";
            File.Copy($"{SharedBundleRoot}/S0.weights.bin", initialPath);
            var initialSha = Sha(File.ReadAllBytes(initialPath));

            var forwards = 0;
            Beat($"arm {arm}: training readout before the lessons");
            var diagBefore = carriers.Select(carrier => Score(models.Policy, carrier, ref forwards)).ToArray();

            Beat($"arm {arm}: evaluation of the restored state");
            var stateBefore = CodeDocumentationSimulationDpoExperiment.EvaluateResearch(EvaluationOptions(models, panelPath, panelSha, initialPath, initialSha),
                StudyRoot, arm, arm + "0", ct);

            // The two assigned lessons, D-T1 then D-T2, through the arm's own supervised step.
            var receipts = new List<object>();
            var watch = Stopwatch.StartNew();
            var allocated0 = GC.GetTotalAllocatedBytes(precise: true);
            for (var lesson = 0; lesson < 2; lesson++)
            {
                var carrier = carriers[lesson];
                var issuedUtc = DateTimeOffset.UtcNow;
                var stepWatch = Stopwatch.StartNew();
                var loss = arm == "A"
                    ? executor.Execute(session, carrier.Ids, 1f, adapters.SelectMany(a => a.AdapterParameters).Select(a => a.Parameter).ToArray())
                    : executor.ExecuteMasked(session, carrier.Ids, 1f, carrier.ResponseMask);
                stepWatch.Stop();
                receipts.Add(new
                {
                    lesson = lesson + 1,
                    id = Rows[lesson].Id,
                    objective = arm == "A" ? "all shifted targets weighted 1" : "response targets weighted 1, prompt and line feed weighted 0",
                    denominator = carrier.Ids.Count - 1,
                    responseTargets = carrier.ResponseTargets.Count,
                    loss,
                    issuedUtc,
                    completedUtc = DateTimeOffset.UtcNow,
                    wallMs = stepWatch.Elapsed.TotalMilliseconds
                });

                if (lesson == 0)
                {
                    using var stream = new FileStream($"{checkpoints}/after-lesson-1.weights.bin", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                    models.Policy.SaveCheckpointToStream(stream);
                    stream.Flush(flushToDisk: true);
                }
            }

            watch.Stop();
            var allocated = GC.GetTotalAllocatedBytes(precise: true) - allocated0;

            Beat($"arm {arm}: training readout after the lessons");
            var diagAfter = carriers.Select(carrier => Score(models.Policy, carrier, ref forwards)).ToArray();

            // The final candidate: the arm's own full checkpoint, saved once.
            var finalPath = $"{checkpoints}/final.weights.bin";
            using (var stream = new FileStream(finalPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                models.Policy.SaveCheckpointToStream(stream);
                stream.Flush(flushToDisk: true);
            }

            var finalSha = Sha(File.ReadAllBytes(finalPath));
            var finalOptimizer = optimizer.ExportPortableState(bindings);
            File.WriteAllBytes($"{checkpoints}/final.optimizer.bin", finalOptimizer.ToBytes());
            File.WriteAllBytes($"{checkpoints}/final.rng.json",
                JsonSerializer.SerializeToUtf8Bytes(models.Policy.CaptureRandomState(topology, loraIdentity, finalOptimizer.Step)));

            Beat($"arm {arm}: evaluation of the trained state");
            var stateAfter = CodeDocumentationSimulationDpoExperiment.EvaluateResearch(EvaluationOptions(models, panelPath, panelSha, finalPath, finalSha),
                StudyRoot, arm, arm + "1", ct);

            process.Refresh();
            WriteNew($"{armRoot}/metrics.json", new
            {
                experimentId = "E-SFT-RESPONSE-MASK",
                arm,
                compiledVariant = CompiledVariant,
                sharedStart = new
                {
                    bundle = $"{SharedBundleRoot}/bundle.json",
                    optimizerStepImported = importedStep,
                    learningQualified = bundle.GetProperty("learning_qualified").GetBoolean(),
                    commonUpdatesHere = 0,
                    commonReadoutsHere = 0
                },
                lessons = receipts,
                diagTrain = new
                {
                    before = Rows.Select((row, i) => new { id = row.Id, score = diagBefore[i] }).ToArray(),
                    after = Rows.Select((row, i) => new { id = row.Id, score = diagAfter[i] }).ToArray(),
                    forwards
                },
                evaluation = new
                {
                    beforeState = arm + "0",
                    beforeReceipt = stateBefore.IsSuccess ? stateBefore.Value : null,
                    beforeError = stateBefore.IsSuccess ? null : stateBefore.ErrorMessage,
                    afterState = arm + "1",
                    afterReceipt = stateAfter.IsSuccess ? stateAfter.Value : null,
                    afterError = stateAfter.IsSuccess ? null : stateAfter.ErrorMessage
                },
                candidates = new
                {
                    initial = new { path = initialPath, sha256 = initialSha },
                    afterLesson1 = $"{checkpoints}/after-lesson-1.weights.bin",
                    final = new { path = finalPath, sha256 = finalSha, optimizerStep = finalOptimizer.Step }
                },
                costs = new
                {
                    twoLessonWallMs = watch.Elapsed.TotalMilliseconds,
                    twoLessonAllocatedBytes = allocated,
                    managedLiveBytes = GC.GetTotalMemory(forceFullCollection: false),
                    peakWorkingSetBytes = process.PeakWorkingSet64
                },
                budgetActual = new { updates = 2, trainingForwards = 2, backwards = 2, diagTrainForwards = forwards, evaluationStates = 2 },
                startedUtc,
                completedUtc = DateTimeOffset.UtcNow,
                limitation = "Two lessons on two pinned rows; the EXAM panel is a near transfer, not four independent laws."
            });

            Console.WriteLine($"ARM_{arm}_DONE updates=2 optimizerStep={finalOptimizer.Step} diagForwards={forwards} " +
                              $"evaluation={(stateBefore.IsSuccess && stateAfter.IsSuccess ? "ok" : "partial")}");
            return Task.FromResult(stateBefore.IsSuccess && stateAfter.IsSuccess ? 0 : 6);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"ARM_{arm}_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(5);
        }
    }

    // ── --summarize: receipts only, no model call ────────────────────────────────────────────────

    public static Task<int> SummarizeAsync(string studyRoot, CancellationToken ct)
    {
        try
        {
            if (!string.Equals(Normalize(studyRoot), StudyRoot, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"MS01_STUDY_ROOT_NOT_ASSIGNED: expected {StudyRoot}");
            if (File.Exists($"{studyRoot}/pair-summary.json"))
                throw new IOException("MS01_SUMMARY_EXISTS: the existing summary is preserved");

            var summary = new Dictionary<string, object>();
            foreach (var arm in new[] { "A", "B" })
            {
                var metrics = $"{studyRoot}/{arm}/metrics.json";
                if (!File.Exists(metrics)) throw new InvalidOperationException($"MS01_ARM_METRICS_MISSING: {arm}");
                using var document = JsonDocument.Parse(File.ReadAllText(metrics));
                summary[arm] = JsonSerializer.Deserialize<object>(document.RootElement.GetRawText())!;
            }

            WriteNew($"{studyRoot}/pair-summary.json", new
            {
                experimentId = "E-SFT-RESPONSE-MASK",
                status = "MEASURED_PAIR_SUMMARY",
                producedBy = "receipt reader; no model call, no arm re-run",
                arms = summary,
                note = "Arm losses belong to different objectives and are not comparable directly; the response NLL and the EXAM panel are the comparable axes."
            });

            Console.WriteLine("SUMMARIZE_DONE modelCalls=0");
            return Task.FromResult(0);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"SUMMARIZE_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(5);
        }
    }

    // ── shared helpers of the consumer ──────────────────────────────────────────────────────────

    private static SimulationDpoEvaluationOptions EvaluationOptions(
        SimulationDpoModelContext models, string panelPath, string panelSha, string candidatePath, string candidateSha) =>
        new(RepositoryRoot, "E-SFT-RESPONSE-MASK", panelPath, panelSha, candidatePath, candidateSha,
            GenerationSettings, token => { token.ThrowIfCancellationRequested(); return models; });

    private static JsonElement ReadCompleteBundle()
    {
        var path = $"{SharedBundleRoot}/bundle.json";
        if (!File.Exists(path))
            throw new InvalidOperationException("SHARED_START_UNAVAILABLE: the family bundle is missing; no local preparation is allowed");
        var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        if (root.GetProperty("status").GetString() != "COMPLETE" || !root.GetProperty("learning_qualified").GetBoolean())
            throw new InvalidOperationException("SHARED_START_UNAVAILABLE: the bundle is not COMPLETE and qualified");
        return root.Clone();
    }

    private static string Normalize(string path) => Path.GetFullPath(path).Replace('\\', '/').TrimEnd('/');
}
