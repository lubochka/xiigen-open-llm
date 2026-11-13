// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// MC01 consumer of card E-SFT-MOMENT-CONTINUITY. It performs zero common updates: the learned start
// comes from the published bundle of family h512-dt-joint. The single factor sits at the symmetric
// session boundary after lesson one: arm A deliberately starts the second lesson on a fresh optimizer,
// arm B carries the exported supervised moments and step across the same boundary. Everything else -
// the candidate save, the fresh verified model, the typed weights validation, the random-state restore,
// the new trainer and the new optimizer instance - is identical in both arms.

namespace ContinuousLearning;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DPO;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TrainableRouter.Training;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO.Models;
using ModelServer;

public static class MC01ExperimentBridge
{
    public const string CompiledVariant =
#if MC01_VARIANT_B
        "B";
#else
        "A";
#endif

    private const string RepositoryRoot = "C:/Projects/llm_mvp_core";
    private const string HostRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MC01";
    private const string StudyRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MC01/results/attempt-0001";
    private const string SharedBundleRoot =
        "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-joint/v1";
    private const string ConfigSha256 = "e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40";
    private const string B0ManifestSha256 = "c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d";
    private const string B0WeightsSha256 = "a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911";
    private const string FamilyId = "h512-dt-joint";
    private const int SharedOptimizerStep = 1000;
    private const int TotalSteps = 1004;
    private const float LearningRate = 0.001f;

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    private sealed record Row(string Id, string Prompt, string Chosen, string Rejected);

    private sealed record Carrier(string Text, List<int> Ids, int PromptByteCount, int Boundary,
        List<int> ResponseTargets, float[] ResponseMask);

    private sealed record ResponseScore(int ResponseTargets, int CorrectTargets, bool ResponseExact,
        float MeanNll, double MeanGoldRank);

    // The two pinned lessons and the four pinned exam cases of this card family.
    private static readonly Row[] Rows =
    [
        new("D-T1", "Explain in one short English sentence what the method returns. Describe argument order. Code: int Delta(int first,int second){return first-second;}",
            "Returns the first argument minus the second argument.", "Returns the second argument minus the first argument."),
        new("D-T2", "Explain in one short English sentence what the method returns. Describe argument order. Code: int Delta(int first,int second){return second-first;}",
            "Returns the second argument minus the first argument.", "Returns the first argument minus the second argument.")
    ];

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

    // ── --prepare: freeze the inputs and verify the shared bundle; no forward, update or generation ──

    public static Task<int> PrepareStudyAsync(string studyRoot, CancellationToken ct)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            if (!string.Equals(Normalize(studyRoot), StudyRoot, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"MC01_STUDY_ROOT_NOT_ASSIGNED: expected {StudyRoot}");
            if (CompiledVariant != "A") throw new InvalidOperationException("MC01_PREPARE_REQUIRES_VARIANT_A");
            if (File.Exists($"{studyRoot}/study.json"))
                throw new IOException("MC01_STUDY_EXISTS_OR_UNKNOWN: the existing study is preserved");

            var bundle = ReadCompleteBundle();
            Directory.CreateDirectory($"{studyRoot}/inputs");
            var examText = string.Concat(Exam.Select(row => JsonSerializer.Serialize(new
            { id = row.Id, prompt = row.Prompt, chosen = row.Chosen, rejected = row.Rejected }) + "\n"));
            var examBytes = new UTF8Encoding(false).GetBytes(examText);
            using (var stream = new FileStream($"{studyRoot}/inputs/exam.jsonl", FileMode.CreateNew, FileAccess.Write, FileShare.None))
                stream.Write(examBytes);

            var models = LoadModels(ct);
            var vocabulary = TokenBytes(models.Tokenizer);
            var carriers = Rows.Select(row => BuildCarrier(row, models.Tokenizer, vocabulary, models.Policy.Config.ContextLen)).ToArray();

            WriteNew($"{studyRoot}/inputs.json", new
            {
                train = Rows.Select((row, i) => new
                {
                    row.Id, row.Prompt, row.Chosen, row.Rejected,
                    tokens = carriers[i].Ids.Count, promptBytes = carriers[i].PromptByteCount,
                    boundary = carriers[i].Boundary, responseTargets = carriers[i].ResponseTargets.Count, ids = carriers[i].Ids
                }).ToArray(),
                exam = Exam.Select(row => new { row.Id, row.Prompt, row.Chosen, row.Rejected }).ToArray(),
                panel = new { path = $"{studyRoot}/inputs/exam.jsonl", sha256 = Sha(examBytes), bytes = examBytes.Length },
                generationSettings = GenerationSettings,
                tokenizerStateSha256 = models.Tokenizer.ComputeStateSha256()
            });

            WriteNew($"{studyRoot}/study.json", new
            {
                experimentId = "E-SFT-MOMENT-CONTINUITY",
                attemptRoot = StudyRoot,
                factor = "supervised optimizer moments and step across the session boundary: arm A resets to a fresh optimizer before lesson two, arm B carries the exported state",
                sharedFirstLesson = "lesson one is the identical full-sequence supervised update in both arms; no response mask here",
                armOrder = new[] { "A", "B" },
                sharedStart = new
                {
                    familyId = FamilyId,
                    bundle = $"{SharedBundleRoot}/bundle.json",
                    bundleSha256 = Sha(File.ReadAllBytes($"{SharedBundleRoot}/bundle.json")),
                    status = bundle.GetProperty("status").GetString(),
                    learningQualified = bundle.GetProperty("learning_qualified").GetBoolean(),
                    optimizerStep = bundle.GetProperty("optimizerStep").GetInt32()
                },
                expectedSteps = new { afterLessonOneBothArms = 1001, afterLessonTwoArmA = 1, afterLessonTwoArmB = 1002 },
                budget = new { commonUpdates = 0, commonReadouts = 0, armUpdates = 2, diagTrainForwardsPerArm = 4, evaluationStatesPerArm = 2 },
                noRetry = true,
                owner = new { pid = process.Id, startedUtc = DateTimeOffset.UtcNow }
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

    // ── --arm A|B: lesson one, the symmetric boundary, then the factor and lesson two ──────────────

    public static Task<int> RunArmAsync(string arm, string armRoot, CancellationToken ct)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var startedUtc = DateTimeOffset.UtcNow;
            if (arm != CompiledVariant)
                throw new InvalidOperationException($"MC01_VARIANT_MISMATCH: binary is {CompiledVariant}, arm {arm} requested");
            if (!string.Equals(Normalize(armRoot), $"{StudyRoot}/{arm}", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"MC01_ARM_ROOT_NOT_ASSIGNED: expected {StudyRoot}/{arm}");
            if (Directory.Exists(armRoot)) throw new IOException("MC01_ARM_EXISTS_OR_UNKNOWN: preserved");
            if (!File.Exists($"{StudyRoot}/study.json")) throw new InvalidOperationException("MC01_STUDY_NOT_PREPARED");

            var bundle = ReadCompleteBundle();
            var panelPath = $"{StudyRoot}/inputs/exam.jsonl";
            var panelSha = Sha(File.ReadAllBytes(panelPath));
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(armRoot);
            var checkpoints = $"{armRoot}/checkpoints";
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(checkpoints);
            WriteNew($"{armRoot}/issued.json", new
            {
                experimentId = "E-SFT-MOMENT-CONTINUITY", arm, compiledVariant = CompiledVariant,
                factor = arm == "A" ? "fresh supervised optimizer before lesson two" : "carried supervised moments and step before lesson two",
                sharedBundleSha256 = Sha(File.ReadAllBytes($"{SharedBundleRoot}/bundle.json")),
                pid = process.Id, issuedUtc = startedUtc
            });

            // Shared start: canonical construction and validation, then the published weights and state.
            Beat($"arm {arm}: canonical load and restore of the shared learned start");
            var models = LoadModels(ct);
            foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;
            models.Policy.LoadCheckpoint($"{SharedBundleRoot}/S0.weights.bin");
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(models.LoraConfig, models.Seeds);
            var sharedRng = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
                File.ReadAllBytes($"{SharedBundleRoot}/S0.rng.json")) ?? throw new InvalidOperationException("MC01_RNG_UNREADABLE");
            models.Policy.RestoreRandomState(sharedRng, new LlmModelRandomStateIdentity(topology, loraIdentity));

            var bindings = AdapterBindings(models.Policy, models.LoraConfig);
            var optimizer = new AdamW(0.9f, 0.95f, 0f, 1e-8f);
            var sharedState = AdamWPortableState.FromBytes(File.ReadAllBytes($"{SharedBundleRoot}/S0.optimizer.bin"))
                ?? throw new InvalidOperationException("MC01_SHARED_OPTIMIZER_UNREADABLE");
            if (!optimizer.TryImportPortableState(bindings, sharedState))
                throw new InvalidOperationException("MC01_SHARED_OPTIMIZER_IMPORT_REFUSED");
            var stepBeforeLessonOne = optimizer.ExportPortableState(bindings).Step;
            if (stepBeforeLessonOne != SharedOptimizerStep)
                throw new InvalidOperationException($"MC01_SHARED_STEP_UNEXPECTED: {stepBeforeLessonOne}");

            var vocabulary = TokenBytes(models.Tokenizer);
            var carriers = Rows.Select(row => BuildCarrier(row, models.Tokenizer, vocabulary, models.Policy.Config.ContextLen)).ToArray();
            var initialPath = $"{checkpoints}/initial.weights.bin";
            File.Copy($"{SharedBundleRoot}/S0.weights.bin", initialPath);
            var initialSha = Sha(File.ReadAllBytes(initialPath));

            var forwards = 0;
            Beat($"arm {arm}: training readout of the restored start");
            var diagBefore = carriers.Select(carrier => Score(models.Policy, carrier, ref forwards)).ToArray();
            var stateBefore = CodeDocumentationSimulationDpoExperiment.EvaluateResearch(
                EvaluationOptions(models, panelPath, panelSha, initialPath, initialSha), StudyRoot, arm, arm + "0", ct);

            // Lesson one: identical in both arms.
            Beat($"arm {arm}: lesson one on {Rows[0].Id}");
            var trainerOne = new Trainer(TrainingConfiguration());
            var sessionOne = new IncrementalTrainingSession(models.Policy, models.Tokenizer, trainerOne, optimizer,
                B0WeightsSha256, models.Tokenizer.ComputeStateSha256(), TrainingSourceProvenance.Identify(models.Policy.Config));
            var executor = new ProductionSupervisedTrainingStepExecutor();
            var adapterParameters = AttachedAdapters(models.Policy, models.LoraConfig)
                .SelectMany(adapter => adapter.AdapterParameters).Select(parameter => parameter.Parameter).ToArray();
            var lessonOneWatch = Stopwatch.StartNew();
            var lessonOneLoss = executor.Execute(sessionOne, carriers[0].Ids, 1f, adapterParameters);
            lessonOneWatch.Stop();
            var stepAfterLessonOne = optimizer.ExportPortableState(bindings).Step;

            // The symmetric session boundary, identical in both arms: export the state before teardown,
            // save the full candidate and the random state, then reopen a fresh verified model.
            var boundaryWatch = Stopwatch.StartNew();
            var exportedBytes = optimizer.ExportPortableState(bindings).ToBytes();
            var optimizerPath = $"{checkpoints}/after-lesson-1.optimizer.bin";
            using (var stream = new FileStream(optimizerPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { stream.Write(exportedBytes); stream.Flush(flushToDisk: true); }

            var candidatePath = $"{checkpoints}/after-lesson-1.weights.bin";
            using (var stream = new FileStream(candidatePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            { models.Policy.SaveCheckpointToStream(stream); stream.Flush(flushToDisk: true); }

            var candidateSha = Sha(File.ReadAllBytes(candidatePath));
            var boundaryRng = models.Policy.CaptureRandomState(topology, loraIdentity, stepAfterLessonOne);
            var boundaryRngBytes = JsonSerializer.SerializeToUtf8Bytes(boundaryRng);
            using (var stream = new FileStream($"{checkpoints}/after-lesson-1.rng.json", FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { stream.Write(boundaryRngBytes); stream.Flush(flushToDisk: true); }

            Beat($"arm {arm}: reopening a fresh verified model at the session boundary");
            var reopened = LoadModels(ct);
            foreach (var parameter in reopened.Reference.Parameters()) parameter.Value.RequiresGrad = false;
            using (var verify = new FileStream(candidatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                verify.Position = 0;
                var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(verify, reopened.Policy.Config,
                    new FullCheckpointLoraTopology(8, new[] { "q_proj", "v_proj" }));
                if (!checkedArtifact.IsSuccess || checkedArtifact.Value != true)
                    throw new InvalidOperationException("MC01_CANDIDATE_VALIDATION_FAILED: " + checkedArtifact.ErrorCode);
                verify.Position = 0;
                reopened.Policy.LoadCheckpoint(verify);
            }

            var reopenedTopology = LoraRandomStateIdentity.ComputeTopologySha256(reopened.Policy.Config, reopened.LoraConfig, reopened.Seeds);
            var reopenedLora = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(reopened.LoraConfig, reopened.Seeds);
            reopened.Policy.RestoreRandomState(boundaryRng, new LlmModelRandomStateIdentity(reopenedTopology, reopenedLora));
            var newBindings = AdapterBindings(reopened.Policy, reopened.LoraConfig);
            var trainerTwo = new Trainer(TrainingConfiguration());
            var optimizerTwo = new AdamW(0.9f, 0.95f, 0f, 1e-8f);
            boundaryWatch.Stop();

            // ── the single factor ───────────────────────────────────────────────────────────────────
            var importWatch = Stopwatch.StartNew();
            object factorEvidence;
            var portable = AdamWPortableState.FromBytes(exportedBytes)
                ?? throw new InvalidOperationException("MC01_EXPORTED_STATE_UNREADABLE");
            if (CompiledVariant == "B")
            {
                // Arm B carries the supervised moments and step into the new optimizer and new parameters.
                if (!optimizerTwo.TryImportPortableState(newBindings, portable))
                    throw new InvalidOperationException("MC01_CARRY_IMPORT_REFUSED: the experiment ends without a fresh fallback");
                var roundtrip = optimizerTwo.ExportPortableState(newBindings);
                var identical = roundtrip.Step == portable.Step && roundtrip.ToBytes().SequenceEqual(exportedBytes);
                if (!identical) throw new InvalidOperationException("MC01_CARRY_ROUNDTRIP_MISMATCH");
                if (roundtrip.Step != stepAfterLessonOne)
                    throw new InvalidOperationException($"MC01_CARRY_STEP_UNEXPECTED: {roundtrip.Step}");
                factorEvidence = new
                {
                    treatment = "carry", importAttempted = true, importAccepted = true,
                    roundtripIdentical = identical, stepBeforeSecondLesson = roundtrip.Step,
                    entries = roundtrip.Entries.Count
                };
            }
            else
            {
                // Arm A keeps the exported bytes as evidence but deliberately starts fresh.
                var fresh = optimizerTwo.ExportPortableState(newBindings);
                factorEvidence = new
                {
                    treatment = "reset", importAttempted = false, importAccepted = false,
                    exportedBytesPreserved = optimizerPath, stepBeforeSecondLesson = fresh.Step,
                    entries = fresh.Entries.Count
                };
            }

            importWatch.Stop();

            // Weights before the second lesson, so the real change of the second update is measurable.
            var beforeSecond = AdapterValues(reopened.Policy, reopened.LoraConfig);

            Beat($"arm {arm}: lesson two on {Rows[1].Id}");
            var sessionTwo = new IncrementalTrainingSession(reopened.Policy, reopened.Tokenizer, trainerTwo, optimizerTwo,
                B0WeightsSha256, reopened.Tokenizer.ComputeStateSha256(), TrainingSourceProvenance.Identify(reopened.Policy.Config));
            var adapterParametersTwo = AttachedAdapters(reopened.Policy, reopened.LoraConfig)
                .SelectMany(adapter => adapter.AdapterParameters).Select(parameter => parameter.Parameter).ToArray();
            var lessonTwoWatch = Stopwatch.StartNew();
            var lessonTwoLoss = executor.Execute(sessionTwo, carriers[1].Ids, 1f, adapterParametersTwo);
            lessonTwoWatch.Stop();
            var finalState = optimizerTwo.ExportPortableState(newBindings);
            var afterSecond = AdapterValues(reopened.Policy, reopened.LoraConfig);

            var deltaNorm = 0.0;
            var maxAbsDelta = 0.0;
            for (var i = 0; i < beforeSecond.Length; i++)
            {
                var difference = afterSecond[i] - beforeSecond[i];
                deltaNorm += (double)difference * difference;
                maxAbsDelta = Math.Max(maxAbsDelta, Math.Abs(difference));
            }

            deltaNorm = Math.Sqrt(deltaNorm);

            Beat($"arm {arm}: training readout after the two lessons");
            var carriersTwo = Rows.Select(row => BuildCarrier(row, reopened.Tokenizer, TokenBytes(reopened.Tokenizer), reopened.Policy.Config.ContextLen)).ToArray();
            var diagAfter = carriersTwo.Select(carrier => Score(reopened.Policy, carrier, ref forwards)).ToArray();

            var finalPath = $"{checkpoints}/final.weights.bin";
            using (var stream = new FileStream(finalPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            { reopened.Policy.SaveCheckpointToStream(stream); stream.Flush(flushToDisk: true); }

            var finalSha = Sha(File.ReadAllBytes(finalPath));
            File.WriteAllBytes($"{checkpoints}/final.optimizer.bin", finalState.ToBytes());
            File.WriteAllBytes($"{checkpoints}/final.rng.json",
                JsonSerializer.SerializeToUtf8Bytes(reopened.Policy.CaptureRandomState(reopenedTopology, reopenedLora, finalState.Step)));

            var stateAfter = CodeDocumentationSimulationDpoExperiment.EvaluateResearch(
                EvaluationOptions(reopened, panelPath, panelSha, finalPath, finalSha), StudyRoot, arm, arm + "1", ct);

            process.Refresh();
            WriteNew($"{armRoot}/metrics.json", new
            {
                experimentId = "E-SFT-MOMENT-CONTINUITY", arm, compiledVariant = CompiledVariant,
                sharedStart = new
                {
                    bundle = $"{SharedBundleRoot}/bundle.json",
                    optimizerStepImported = stepBeforeLessonOne,
                    learningQualified = bundle.GetProperty("learning_qualified").GetBoolean(),
                    commonUpdatesHere = 0, commonReadoutsHere = 0
                },
                steps = new
                {
                    beforeLessonOne = stepBeforeLessonOne,
                    afterLessonOne = stepAfterLessonOne,
                    beforeLessonTwo = CompiledVariant == "B" ? stepAfterLessonOne : 0,
                    afterLessonTwo = finalState.Step
                },
                factor = factorEvidence,
                lessons = new object[]
                {
                    new { lesson = 1, id = Rows[0].Id, loss = lessonOneLoss, wallMs = lessonOneWatch.Elapsed.TotalMilliseconds, shared = true },
                    new { lesson = 2, id = Rows[1].Id, loss = lessonTwoLoss, wallMs = lessonTwoWatch.Elapsed.TotalMilliseconds, shared = false }
                },
                boundary = new
                {
                    candidate = new { path = candidatePath, sha256 = candidateSha },
                    optimizerExport = new { path = optimizerPath, bytes = exportedBytes.Length, sha256 = Sha(exportedBytes), step = portable.Step, entries = portable.Entries.Count },
                    randomState = new { path = $"{checkpoints}/after-lesson-1.rng.json", sha256 = Sha(boundaryRngBytes) },
                    sessionReloadWallMs = boundaryWatch.Elapsed.TotalMilliseconds,
                    factorWallMs = importWatch.Elapsed.TotalMilliseconds
                },
                secondLessonEffect = new
                {
                    adapterTensorValues = beforeSecond.Length,
                    deltaL2Norm = deltaNorm,
                    maxAbsoluteDelta = maxAbsDelta,
                    finalOptimizerStep = finalState.Step,
                    finalOptimizerEntries = finalState.Entries.Count
                },
                diagTrain = new
                {
                    before = Rows.Select((row, i) => new { id = row.Id, score = diagBefore[i] }).ToArray(),
                    after = Rows.Select((row, i) => new { id = row.Id, score = diagAfter[i] }).ToArray(),
                    forwards
                },
                evaluation = new
                {
                    beforeState = arm + "0", beforeReceipt = stateBefore.IsSuccess ? stateBefore.Value : null,
                    beforeError = stateBefore.IsSuccess ? null : stateBefore.ErrorMessage,
                    afterState = arm + "1", afterReceipt = stateAfter.IsSuccess ? stateAfter.Value : null,
                    afterError = stateAfter.IsSuccess ? null : stateAfter.ErrorMessage
                },
                candidates = new
                {
                    initial = new { path = initialPath, sha256 = initialSha },
                    afterLessonOne = new { path = candidatePath, sha256 = candidateSha },
                    final = new { path = finalPath, sha256 = finalSha }
                },
                costs = new
                {
                    lessonOneWallMs = lessonOneWatch.Elapsed.TotalMilliseconds,
                    lessonTwoWallMs = lessonTwoWatch.Elapsed.TotalMilliseconds,
                    boundaryWallMs = boundaryWatch.Elapsed.TotalMilliseconds,
                    factorWallMs = importWatch.Elapsed.TotalMilliseconds,
                    managedLiveBytes = GC.GetTotalMemory(forceFullCollection: false),
                    peakWorkingSetBytes = process.PeakWorkingSet64
                },
                budgetActual = new { updates = 2, trainingForwards = 2, backwards = 2, diagTrainForwards = forwards, evaluationStates = 2 },
                startedUtc, completedUtc = DateTimeOffset.UtcNow,
                limitation = "Two lessons on two pinned rows after a shared start; the exam panel is a near transfer, not four independent laws."
            });

            Console.WriteLine($"ARM_{arm}_DONE steps={stepBeforeLessonOne}->{stepAfterLessonOne}->{finalState.Step} " +
                              $"deltaL2={deltaNorm:E3} evaluation={(stateBefore.IsSuccess && stateAfter.IsSuccess ? "ok" : "partial")}");
            return Task.FromResult(stateBefore.IsSuccess && stateAfter.IsSuccess ? 0 : 6);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"ARM_{arm}_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(5);
        }
    }

    // ── --summarize: receipts only ─────────────────────────────────────────────────────────────────

    public static Task<int> SummarizeAsync(string studyRoot, CancellationToken ct)
    {
        try
        {
            if (!string.Equals(Normalize(studyRoot), StudyRoot, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"MC01_STUDY_ROOT_NOT_ASSIGNED: expected {StudyRoot}");
            if (File.Exists($"{studyRoot}/pair-summary.json"))
                throw new IOException("MC01_SUMMARY_EXISTS: preserved");

            var arms = new Dictionary<string, object>();
            string? candidateA = null, candidateB = null, rngA = null, rngB = null;
            foreach (var arm in new[] { "A", "B" })
            {
                var path = $"{studyRoot}/{arm}/metrics.json";
                if (!File.Exists(path)) throw new InvalidOperationException($"MC01_ARM_METRICS_MISSING: {arm}");
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                arms[arm] = JsonSerializer.Deserialize<object>(document.RootElement.GetRawText())!;
                var boundary = document.RootElement.GetProperty("boundary");
                var candidate = boundary.GetProperty("candidate").GetProperty("sha256").GetString();
                var rng = boundary.GetProperty("randomState").GetProperty("sha256").GetString();
                if (arm == "A") { candidateA = candidate; rngA = rng; } else { candidateB = candidate; rngB = rng; }
            }

            WriteNew($"{studyRoot}/pair-summary.json", new
            {
                experimentId = "E-SFT-MOMENT-CONTINUITY",
                status = "MEASURED_PAIR_SUMMARY",
                producedBy = "receipt reader; no model call, no arm re-run",
                negativeControl = new
                {
                    description = "before the second lesson the policy bits and the random state must be identical in both arms; only the optimizer state may differ",
                    candidateSha256A = candidateA, candidateSha256B = candidateB,
                    candidateIdentical = candidateA == candidateB,
                    randomStateSha256A = rngA, randomStateSha256B = rngB,
                    randomStateIdentical = rngA == rngB
                },
                arms,
                note = "A numerical difference between the arms is not required to be nonzero; the absence of the expected identical first state would invalidate the comparison."
            });

            Console.WriteLine($"SUMMARIZE_DONE candidateIdentical={(candidateA == candidateB)} rngIdentical={(rngA == rngB)}");
            return Task.FromResult(0);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"SUMMARIZE_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(5);
        }
    }

    // ── shared helpers, identical in both arms ────────────────────────────────────────────────────

    private static TrainingConfig TrainingConfiguration() => new()
    {
        BatchSize = 1, SeqLen = 2048, WarmupSteps = 0, TotalSteps = TotalSteps,
        MaxLr = LearningRate, MinLr = LearningRate, GradAccumSteps = 1, GradClipNorm = 1f, WeightDecay = 0f
    };

    private static SimulationDpoEvaluationOptions EvaluationOptions(
        SimulationDpoModelContext models, string panelPath, string panelSha, string candidatePath, string candidateSha) =>
        new(RepositoryRoot, "E-SFT-MOMENT-CONTINUITY", panelPath, panelSha, candidatePath, candidateSha,
            GenerationSettings, token => { token.ThrowIfCancellationRequested(); return models; });

    private static LoraLinear[] AttachedAdapters(LlmModel policy, LoraConfig config) =>
        Enumerable.Range(0, policy.Blocks.Count)
            .SelectMany(block => config.TargetModules.Select(target => (LoraLinear)policy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray();

    // Bindings come from the checkpoint entries filtered by reference identity of the attached q/v
    // adapter parameters, with ordinal sorting and unique keys. No zip by array order, no base tensors.
    private static IReadOnlyList<AdamWParameterBinding> AdapterBindings(LlmModel policy, LoraConfig config)
    {
        var allowed = AttachedAdapters(policy, config)
            .SelectMany(adapter => adapter.AdapterParameters).Select(parameter => parameter.Parameter)
            .ToHashSet(ReferenceEqualityComparer.Instance);
        var bindings = policy.GetCheckpointParameterEntries()
            .Where(entry => allowed.Contains(entry.Parameter))
            .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
            .OrderBy(binding => binding.Key, StringComparer.Ordinal)
            .ToArray();
        if (bindings.Length != 16 || bindings.Select(binding => binding.Key).Distinct(StringComparer.Ordinal).Count() != 16)
            throw new InvalidOperationException($"MC01_BINDINGS_UNEXPECTED: {bindings.Length}");
        return bindings;
    }

    private static float[] AdapterValues(LlmModel policy, LoraConfig config) =>
        AdapterBindings(policy, config).SelectMany(binding => binding.Parameter.Value.Data).ToArray();

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

        if (vocabulary.Count == 0) throw new InvalidOperationException("MC01_TOKENIZER_VOCABULARY_EMPTY");
        return vocabulary;
    }

    private static Carrier BuildCarrier(Row row, BpeTokenizer tokenizer, Dictionary<int, byte[]> vocabulary, int contextLength)
    {
        var text = row.Prompt + "\n" + row.Chosen;
        var utf8 = new UTF8Encoding(false, true);
        var textBytes = utf8.GetBytes(text);
        var ids = tokenizer.Encode(text);
        if (ids.Count < 2 || ids.Count > contextLength + 1)
            throw new InvalidOperationException("MC01_CARRIER_LENGTH_OUT_OF_RANGE");

        var spans = new List<(int Start, int End)>(ids.Count);
        var cursor = 0;
        foreach (var id in ids)
        {
            if (!vocabulary.TryGetValue(id, out var bytes))
                throw new InvalidOperationException($"MC01_TOKEN_NOT_IN_VOCABULARY: {id}");
            spans.Add((cursor, cursor + bytes.Length));
            cursor += bytes.Length;
        }

        if (cursor != textBytes.Length) throw new InvalidOperationException("MC01_CARRIER_BYTES_MISMATCH");
        var rebuilt = new byte[cursor];
        var offset = 0;
        foreach (var id in ids)
        {
            var bytes = vocabulary[id];
            Array.Copy(bytes, 0, rebuilt, offset, bytes.Length);
            offset += bytes.Length;
        }

        if (!rebuilt.SequenceEqual(textBytes)) throw new InvalidOperationException("MC01_CARRIER_BYTES_MISMATCH");

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

        if (responseTargets.Count == 0) throw new InvalidOperationException("MC01_NO_RESPONSE_TARGETS");
        return new Carrier(text, ids, utf8.GetByteCount(row.Prompt), boundary, responseTargets, mask);
    }

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
            var loaded = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(options.StartRequest, cancellationToken);
            if (!loaded.IsSuccess || loaded.Value is null) throw new InvalidOperationException(loaded.ErrorCode);
            var parent = loaded.Value.ParentManifest;
            if (parent is null || parent.Step != 1 || parent.ParentCheckpoint is not null ||
                parent.ManifestSha256 != B0ManifestSha256 || parent.WeightsSha256 != B0WeightsSha256 ||
                parent.DpoApplication != RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate ||
                loaded.Value.Model.GetCheckpointParameterEntries().Count != 55 ||
                loaded.Value.Model.BaseParameters.Count != 39)
                throw new InvalidOperationException("MC01_SAVED_ROOT_IDENTITY_MISMATCH");
            return loaded.Value;
        }

        var policy = Load();
        var reference = Load();
        var weights = Path.GetFullPath(Path.Combine(RepositoryRoot, policy.ParentManifest!.RepositoryRelativeWeightsPath));
        return new(policy.Model, reference.Model, options.Tokenizer, options.StartRequest.LoraConfig!,
            options.StartRequest.AdapterSeedAuthority!, weights, B0WeightsSha256,
            token => { token.ThrowIfCancellationRequested(); return Load().Model; });
    }

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

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void WriteNew(string path, object payload)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, payload, Json);
    }

    private static void Beat(string message) => Console.WriteLine($"[{DateTimeOffset.UtcNow:HH:mm:ss}] {message}");
}
