// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// N61 / LC-REF-CACHE bridge. It lives inside the copied ContinuousLearning assembly because the real
// internal issuers, DpoTrainer.CreateForProjectLora and SecureLocalFileBoundary are only reachable here.
// The bridge drives the real native chain; it never fabricates a capability and never relaxes a guard.

namespace ContinuousLearning;

using System.Diagnostics;
using System.Security.Cryptography;
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

public static class N61RefCacheBridge
{
    // Which copied trainer this binary was compiled with. The runner refuses a mismatched arm.
    public const string CompiledVariant =
#if N61_VARIANT_B
        "B";
#else
        "A";
#endif

    private const string RepositoryRoot = "C:/Projects/llm_mvp_core";
    private const string ConfigSha256 = "e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40";
    private const string B0ManifestSha256 = "c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d";
    private const string B0WeightsSha256 = "a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911";
    private const string InputPath = "C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-current-learning-audit/dpo/fixtures/csharp-two-source-rows.jsonl";
    private const string InputSha256 = "75fa380a51e96083332c9609ae69ce368b04e29011fab4af0c0fc161335202c8";
    private const string RunIdPrefix = "N61-REF-CACHE";
    private const string ResultRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N61-REF-CACHE/results";

    // Shared preparation of the learned start: exactly 1000 supervised updates, 500 presentations per row.
    private const int PreparationUpdates = 1000;
    private const int PreparationTotalSteps = 1004;
    private const float PreparationLearningRate = 0.001f;

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private static readonly byte[] AuthoritySalt = SHA256.HashData(
        System.Text.Encoding.UTF8.GetBytes("N61-REF-CACHE-STUDY-SALT-2026-10-21"));

    // ── shared preparation: W0 -> 1000 supervised updates -> S0 ────────────────────────────────

    public static Task<int> PrepareLearnedStartAsync(string commonRoot, CancellationToken ct)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var nonce = Guid.NewGuid().ToString("N");
            var startedUtc = DateTimeOffset.UtcNow;

            // Any earlier claim, result or partial blocks a repeat; the existing bytes are preserved.
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(ResultRoot);
            Claim($"{ResultRoot}/execution.claim", nonce, process, startedUtc, "root");
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(commonRoot);
            Claim($"{commonRoot}/preparation.claim", nonce, process, startedUtc, "common-preparation");

            Beat("preparation: reading the two pinned source rows");
            var rows = Require(CodeDocumentationSimulationDpoExperiment.Inspect(
                InputPath, InputSha256, 2, SimulationDpoDomain.CodeDocumentation));
            if (rows.Count != 2 || rows[0].Id != "simulation-csharp-doc-f06-008" || rows[1].Id != "simulation-csharp-doc-f21-001")
                throw new InvalidOperationException("N61_FIXTURE_IDENTITY_MISMATCH");

            Beat("preparation: loading the canonical saved root W0");
            var models = LoadModels(ct);
            foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;

            // The study passport is written once and names both arms, the data, the start and the planned updates.
            WriteNew($"{ResultRoot}/study.json", new
            {
                experimentId = RunIdPrefix,
                factor = "reference-score reuse inside one trainer instance; arm A has no cache, arm B caches two pairs",
                arms = new[] { "A", "B" },
                plannedDpoUpdates = 4,
                plannedPreparationUpdates = PreparationUpdates,
                data = new { inputPath = InputPath, inputSha256 = InputSha256, rowIds = rows.Select(r => r.Id).ToArray() },
                start = new { manifestSha256 = B0ManifestSha256, weightsSha256 = B0WeightsSha256, weightsPath = models.SourceWeightsPath },
                configuration = new
                {
                    configSha256 = ConfigSha256,
                    tokenizerVocabSize = models.Tokenizer.VocabSize,
                    loraRank = models.LoraConfig.Rank,
                    loraAlpha = models.LoraConfig.Alpha,
                    loraDropout = models.LoraConfig.Dropout,
                    targetModules = models.LoraConfig.TargetModules
                },
                owner = new { nonce, pid = process.Id, startedUtc }
            });

            // The carrier is the validated exact material, so the supervised step sees the same integer ids
            // that the DPO arms will see; no padding, no inserted markers, no re-encoded concatenated text.
            var batches = PrepareBatches(rows, models.Tokenizer);
            var carriers = batches.Select(batch => Carrier(batch.Batch)).ToArray();

            Beat("preparation: readout of the untrained start W0");
            var forwards = 0;
            var panelW0 = Panel(models.Policy, batches, ref forwards);

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

            // Exactly 1000 real updates, alternating the two rows, with no early success stop.
            for (var i = 0; i < PreparationUpdates; i++)
            {
                ct.ThrowIfCancellationRequested();
                losses.Add(trainer.TrainSupervisedNextTokenStep(models.Policy, optimizer, carriers[i % 2], 1f));
                if ((i + 1) % 50 == 0) Beat($"preparation: update {i + 1}/{PreparationUpdates} loss={losses[^1]:F6}");
            }

            watch.Stop();
            var allocated = GC.GetTotalAllocatedBytes(precise: true) - allocated0;

            Beat("preparation: readout of the learned start S0");
            var panelS0 = Panel(models.Policy, batches, ref forwards);

            // S0 artifacts: checkpoint, portable optimizer state and the full random-state snapshot.
            var weightsPath = $"{commonRoot}/S0.weights.bin";
            using (var stream = new FileStream(weightsPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                models.Policy.SaveCheckpointToStream(stream);
            ValidateSavedStart(weightsPath, models, ct);

            var adapters = Adapters(models);
            var bindings = models.Policy.GetCheckpointParameterEntries()
                .Where(entry => adapters.SelectMany(a => a.AdapterParameters).Select(a => a.Parameter)
                    .ToHashSet(ReferenceEqualityComparer.Instance).Contains(entry.Parameter))
                .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
                .OrderBy(binding => binding.Key, StringComparer.Ordinal)
                .ToArray();
            var optimizerBytes = optimizer.ExportPortableState(bindings).ToBytes();
            using (var stream = new FileStream($"{commonRoot}/S0.optimizer.bin", FileMode.CreateNew, FileAccess.Write, FileShare.None))
                stream.Write(optimizerBytes);

            var topology = LoraRandomStateIdentity.ComputeTopologySha256(models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(models.LoraConfig, models.Seeds);
            var snapshot = models.Policy.CaptureRandomState(topology, loraIdentity, PreparationUpdates);
            using (var stream = new FileStream($"{commonRoot}/S0.rng.json", FileMode.CreateNew, FileAccess.Write, FileShare.None))
                stream.Write(JsonSerializer.SerializeToUtf8Bytes(snapshot));

            // Readback proof: the saved checkpoint must reload into a fresh copy with identical 55 tensors.
            var readback = models.CreateReadbackModel(ct);
            readback.LoadCheckpoint(weightsPath);
            var live = models.Policy.GetCheckpointParameterEntries();
            var restored = readback.GetCheckpointParameterEntries();
            var identical = live.Count == restored.Count && live.Count == 55;
            if (identical)
                for (var i = 0; i < live.Count && identical; i++)
                    identical = live[i].Key == restored[i].Key &&
                                live[i].Parameter.Value.Data.Length == restored[i].Parameter.Value.Data.Length &&
                                live[i].Parameter.Value.Data.SequenceEqual(restored[i].Parameter.Value.Data);

            // Qualification: at least one whole pinned response that was wrong at W0 and exact at S0.
            var acquired = new List<string>();
            for (var i = 0; i < rows.Count; i++)
                if (!panelW0[i].TeacherForcedResponseExact && panelS0[i].TeacherForcedResponseExact)
                    acquired.Add(rows[i].Id);

            var alreadyExact = panelW0.All(score => score.TeacherForcedResponseExact);
            var qualified = acquired.Count > 0;
            var failureReason = qualified ? null
                : alreadyExact ? "NO_ACQUISITION_CONTRAST: every pinned response was already exact at W0"
                : "PREPARATION_COMPLETED_LEARNING_UNQUALIFIED: no pinned response moved from wrong to exact";

            WriteNew($"{commonRoot}/preparation.json", new
            {
                experimentId = RunIdPrefix,
                stage = "COMMON_LEARNED_START",
                learning_qualified = qualified,
                completed_updates = PreparationUpdates,
                exact_by_id_W0 = rows.Select((row, i) => new { id = row.Id, exact = panelW0[i].TeacherForcedResponseExact }).ToArray(),
                exact_by_id_S0 = rows.Select((row, i) => new { id = row.Id, exact = panelS0[i].TeacherForcedResponseExact }).ToArray(),
                acquired_ids = acquired,
                failure_reason = failureReason,
                policyOrigin = "SFT-derived policy from the pinned W0 root; this is not a canonical DPO parent manifest",
                origin = new { manifestSha256 = B0ManifestSha256, weightsSha256 = B0WeightsSha256 },
                carrier = new
                {
                    source = "validated exact material ChosenIds[0..ChosenLength)",
                    lengths = batches.Select(b => b.Batch.ExactMaterial!.ChosenLength).ToArray(),
                    tokenizerSha256 = batches[0].Batch.ExactMaterial!.TokenizerSha256
                },
                readouts = new { W0 = Rows(rows, panelW0), S0 = Rows(rows, panelS0) },
                supervisedLoss = new { first = losses[0], last = losses[^1] },
                costs = new
                {
                    wallMs = watch.Elapsed.TotalMilliseconds,
                    allocatedBytes = allocated,
                    forwards,
                    peakWorkingSetBytes = Peak(process)
                },
                checkpointReadbackIdentical = identical,
                rng = new { topologySha256 = topology, loraConfigurationSha256 = loraIdentity, preparationStep = PreparationUpdates },
                owner = new { nonce, pid = process.Id, startedUtc },
                limitation = "Narrow whole-response memorisation of two pinned rows; not a general rule, not a held-out result."
            });

            Console.WriteLine($"PREPARATION_DONE qualified={qualified} updates={PreparationUpdates} acquired={acquired.Count}");
            return Task.FromResult(qualified ? 0 : 3);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"PREPARATION_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(4);
        }
    }

    // Полная проверка сохранённого артефакта: нативная схема слотов адаптеров объявляется явно,
    // иначе проверка сверяет файл только с базовой конфигурацией и отказывает на 55 параметрах.
    private static void ValidateSavedStart(string weightsPath, SimulationDpoModelContext models, CancellationToken ct)
    {
        using var verify = new FileStream(weightsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        _ = Require(LlmModel.ValidateFullCheckpointArtifact(verify, models.Policy.Config,
            new FullCheckpointLoraTopology(models.LoraConfig.Rank, models.LoraConfig.TargetModules), long.MaxValue, ct));
    }

    // Достройка квитанции подготовки без повторного обучения. Веса уже оплачены и сохранены; здесь
    // только пересчитываются показания необученного старта и сохранённого S0 — восемь проходов вперёд,
    // ни одного обновления. Состояние оптимизатора и снимок генератора воссоздать нельзя, и квитанция
    // честно помечает их как не захваченные.
    public static Task<int> CompletePreparationReceiptAsync(string commonRoot, CancellationToken ct)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var weightsPath = $"{commonRoot}/S0.weights.bin";
            if (!File.Exists(weightsPath)) throw new InvalidOperationException("N61_SAVED_START_MISSING");
            if (File.Exists($"{commonRoot}/preparation.json")) throw new InvalidOperationException("N61_RECEIPT_ALREADY_EXISTS");

            Beat("receipt: reading the two pinned source rows");
            var rows = Require(CodeDocumentationSimulationDpoExperiment.Inspect(
                InputPath, InputSha256, 2, SimulationDpoDomain.CodeDocumentation));

            Beat("receipt: loading the untrained root W0 for the before readout");
            var models = LoadModels(ct);
            foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;
            var batches = PrepareBatches(rows, models.Tokenizer);

            var forwards = 0;
            var panelW0 = Panel(models.Policy, batches, ref forwards);

            Beat("receipt: validating and loading the saved learned start S0");
            ValidateSavedStart(weightsPath, models, ct);
            models.Policy.LoadCheckpoint(weightsPath);
            var panelS0 = Panel(models.Policy, batches, ref forwards);

            var readback = models.CreateReadbackModel(ct);
            readback.LoadCheckpoint(weightsPath);
            var live = models.Policy.GetCheckpointParameterEntries();
            var restored = readback.GetCheckpointParameterEntries();
            var identical = live.Count == restored.Count && live.Count == 55;
            if (identical)
                for (var i = 0; i < live.Count && identical; i++)
                    identical = live[i].Key == restored[i].Key &&
                                live[i].Parameter.Value.Data.Length == restored[i].Parameter.Value.Data.Length &&
                                live[i].Parameter.Value.Data.SequenceEqual(restored[i].Parameter.Value.Data);

            var acquired = new List<string>();
            for (var i = 0; i < rows.Count; i++)
                if (!panelW0[i].TeacherForcedResponseExact && panelS0[i].TeacherForcedResponseExact)
                    acquired.Add(rows[i].Id);

            var alreadyExact = panelW0.All(score => score.TeacherForcedResponseExact);
            var qualified = acquired.Count > 0;
            var failureReason = qualified ? null
                : alreadyExact ? "NO_ACQUISITION_CONTRAST: every pinned response was already exact at W0"
                : "PREPARATION_COMPLETED_LEARNING_UNQUALIFIED: no pinned response moved from wrong to exact";

            WriteNew($"{commonRoot}/preparation.json", new
            {
                experimentId = RunIdPrefix,
                stage = "COMMON_LEARNED_START",
                receipt_mode = "RECOMPUTED_FROM_SAVED_WEIGHTS_NO_NEW_TRAINING",
                learning_qualified = qualified,
                completed_updates = PreparationUpdates,
                completed_updates_evidence = "preparation.log records update 1000/1000 and the S0 readout of the same process that wrote S0.weights.bin",
                optimizer_state_captured = false,
                rng_snapshot_captured = false,
                not_captured_reason = "the first run threw on the artifact validation call after the weights were written; the in-process optimizer and random state could not be reconstructed without repeating the paid training",
                exact_by_id_W0 = rows.Select((row, i) => new { id = row.Id, exact = panelW0[i].TeacherForcedResponseExact }).ToArray(),
                exact_by_id_S0 = rows.Select((row, i) => new { id = row.Id, exact = panelS0[i].TeacherForcedResponseExact }).ToArray(),
                acquired_ids = acquired,
                failure_reason = failureReason,
                policyOrigin = "SFT-derived policy from the pinned W0 root; this is not a canonical DPO parent manifest",
                origin = new { manifestSha256 = B0ManifestSha256, weightsSha256 = B0WeightsSha256 },
                carrier = new
                {
                    source = "validated exact material ChosenIds[0..ChosenLength)",
                    lengths = batches.Select(b => b.Batch.ExactMaterial!.ChosenLength).ToArray(),
                    tokenizerSha256 = batches[0].Batch.ExactMaterial!.TokenizerSha256
                },
                readouts = new { W0 = Rows(rows, panelW0), S0 = Rows(rows, panelS0) },
                costs = new { receiptForwards = forwards, updates = 0, peakWorkingSetBytes = Peak(process) },
                checkpointReadbackIdentical = identical,
                limitation = "Narrow whole-response memorisation of two pinned rows; not a general rule, not a held-out result."
            });

            Console.WriteLine($"RECEIPT_DONE qualified={qualified} acquired={acquired.Count} forwards={forwards} readback={identical}");
            return Task.FromResult(qualified ? 0 : 3);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"RECEIPT_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(6);
        }
    }

    // ── one measured arm: load S0, two StepVector over both pairs, readouts before and after ───

    public static Task<int> RunAsync(string arm, string output, CancellationToken ct)
    {
        var live = false;
        try
        {
            using var process = Process.GetCurrentProcess();
            var nonce = Guid.NewGuid().ToString("N");
            var startedUtc = DateTimeOffset.UtcNow;
            var commonRoot = $"{ResultRoot}/common";

            // Direct mode without a live owner refuses: the shared preparation must have claimed the root.
            if (!File.Exists($"{ResultRoot}/execution.claim"))
                throw new InvalidOperationException("N61_ROOT_CLAIM_MISSING");

            var preparation = JsonDocument.Parse(File.ReadAllText($"{commonRoot}/preparation.json"));
            if (!preparation.RootElement.GetProperty("learning_qualified").GetBoolean())
                throw new InvalidOperationException("N61_PREPARATION_NOT_QUALIFIED");

            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(output);
            Claim($"{output}/arm.claim", nonce, process, startedUtc, $"arm-{arm}");

            Beat($"arm {arm}: loading the canonical root and the shared learned start S0");
            var models = LoadModels(ct);
            foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;
            models.Policy.LoadCheckpoint($"{commonRoot}/S0.weights.bin");

            var rows = Require(CodeDocumentationSimulationDpoExperiment.Inspect(
                InputPath, InputSha256, 2, SimulationDpoDomain.CodeDocumentation));
            var batches = PrepareBatches(rows, models.Tokenizer);
            var plain = batches.Select(batch => batch.Batch).ToArray();

            // Existing governance and content authority admit the exact pairs before any mutation.
            var governance = new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + InputSha256 });
            var rich = rows.Select(row => new DpoPair(row.Prompt, row.Chosen, row.Rejected, "code-documentation", startedUtc)
            {
                GovernanceDecision = governance,
                ChosenAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator),
                RejectedAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator)
            }).ToArray();
            _ = Require(TrainingDataGovernanceAdmission.Validate(rich));
            var content = Require(DpoPairTrainingContentAuthority.Project(rich));

            var adapters = Adapters(models);
            var recipient = DpoProjectLoraRecipient.Capture(models.Policy, models.Reference, adapters, models.LoraConfig);
            var profile = DpoExecutionProfile.Capture(plain[0], recipient.Bindings);

            var legal = Require(LegalTrainingTeacherAdmissionAuthority.Create([], 0, AuthoritySalt));
            live = true;
            var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                legal.ValidateBatch, () => live, null,
                legal.SealLogicalCapabilityReplay, legal.VerifyLogicalCapabilityReplay,
                legal.SealLogicalCapabilityExecutableManifest,
                legal.VerifyLogicalCapabilityExecutableManifest, legal.ValidateBatch);

            using var lease = Require(((IServerOwnedDpoTrainingLeaseBroker)authority)
                .AcquireCodeDocumentationTrainingLease(content, $"{RunIdPrefix}-{arm}"));

            var mutation = Require(lease.IssueDpoTrainerMutationAdmissionCapability());
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(models.LoraConfig, models.Seeds);
            var checkpointRoot = $"{output}/checkpoints";
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(checkpointRoot);

            var observer = new DpoTrainerPublicationObserver
            {
                BeforePolicyFinalMove = bundle =>
                {
                    _ = Require(RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                        checkpointRoot, bundle.Step, bundle.OptimizerState, ct));
                    if (bundle.RandomState is null) throw new InvalidOperationException("N61_RANDOM_STATE_MISSING");
                    _ = Require(RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                        Path.Combine(checkpointRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(bundle.Step)),
                        bundle.RandomState));
                }
            };

            // Fresh DPO optimizer on both sides: the shared start is weights only, continuation proof is null.
            var trainer = DpoTrainer.CreateForProjectLora(models.Policy, models.Reference, checkpointRoot,
                mutation, recipient, profile, null, observer,
                step => models.Policy.CaptureRandomState(topology, loraIdentity, step),
                state => models.Policy.RestoreRandomState(state, new(topology, loraIdentity)));

            var forwards = 0;
            Beat($"arm {arm}: readout before training");
            var panelBefore = Panel(models.Policy, batches, ref forwards);
            var evaluateBefore = trainer.EvaluateMany(plain, 0);

            // Exactly two StepVector calls, each seeing both prepared pairs in the pinned order.
            var watch = Stopwatch.StartNew();
            var allocated0 = GC.GetTotalAllocatedBytes(precise: true);
            var timing1 = new DpoStepVectorTiming();
            var step1 = trainer.StepVector(batches, 1, timing1);
            var timing2 = new DpoStepVectorTiming();
            var step2 = trainer.StepVector(batches, 2, timing2);
            watch.Stop();
            var snapshot1 = timing1.Snapshot();
            var snapshot2 = timing2.Snapshot();
            var allocated = GC.GetTotalAllocatedBytes(precise: true) - allocated0;

            Beat($"arm {arm}: readout after training");
            var evaluateAfter = trainer.EvaluateMany(plain, 3);
            var panelAfter = Panel(models.Policy, batches, ref forwards);

            var optimizerState = trainer.ExportOptimizerState();
            WriteNew($"{output}/arm-result.json", new
            {
                experimentId = RunIdPrefix,
                arm,
                compiledVariant = CompiledVariant,
                referenceScoreReuse = CompiledVariant == "B",
                runtimeStatus = "MEASURED_EXTRACTED_NATIVE_COPY",
                executionUtc = startedUtc,
                framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                start = new { sharedLearnedStart = "S0 (weights only)", originManifestSha256 = B0ManifestSha256 },
                budget = new
                {
                    plannedDpoUpdates = 4,
                    actualOptimizerStep = optimizerState.Step,
                    stepVectorCalls = 2,
                    pairsPerStep = batches.Count,
                    evaluateManyCalls = 2,
                    panelForwards = forwards
                },
                referenceCache = ReferenceCacheCounters(trainer),
                metrics = new { step1, step2, evaluateBefore, evaluateAfter },
                readouts = new { before = Rows(rows, panelBefore), after = Rows(rows, panelAfter) },
                costs = new
                {
                    trainWallMs = watch.Elapsed.TotalMilliseconds,
                    trainAllocatedBytes = allocated,
                    // Нативные снимки фаз: число вызовов эталонного прохода прямо показывает действие фактора.
                    step1Timing = snapshot1,
                    step2Timing = snapshot2,
                    referenceForwardCompletedCalls = snapshot1.ReferenceForward.CompletedCalls + snapshot2.ReferenceForward.CompletedCalls,
                    referenceForwardMs = (snapshot1.ReferenceForward.CompletedMilliseconds ?? 0) + (snapshot2.ReferenceForward.CompletedMilliseconds ?? 0),
                    policyForwardMs = (snapshot1.PolicyForward.CompletedMilliseconds ?? 0) + (snapshot2.PolicyForward.CompletedMilliseconds ?? 0),
                    backwardMs = (snapshot1.Backward.CompletedMilliseconds ?? 0) + (snapshot2.Backward.CompletedMilliseconds ?? 0),
                    optimizerMs = (snapshot1.ApplyOptimizer.CompletedMilliseconds ?? 0) + (snapshot2.ApplyOptimizer.CompletedMilliseconds ?? 0),
                    managedLiveBytes = GC.GetTotalMemory(forceFullCollection: false),
                    peakWorkingSetBytes = Peak(process)
                },
                owner = new { nonce, pid = process.Id, startedUtc },
                limitation = "Evaluation runs on the same training pairs; this is not held-out accuracy. No Generate, no external teacher."
            });

            Console.WriteLine($"ARM_{arm}_DONE optimizerStep={optimizerState.Step} wallMs={watch.Elapsed.TotalMilliseconds:F1}");
            return Task.FromResult(0);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"ARM_{arm}_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(5);
        }
        finally
        {
            live = false;
        }
    }

    // ── shared helpers, identical in both arms ────────────────────────────────────────────────

    private static object ReferenceCacheCounters(DpoTrainer trainer)
    {
#if N61_VARIANT_B
        return new
        {
            enabled = true,
            lookups = trainer.ReferenceScoreCacheLookups,
            hits = trainer.ReferenceScoreCacheHits,
            misses = trainer.ReferenceScoreCacheMisses,
            stores = trainer.ReferenceScoreCacheStores,
            capacity = DpoTrainer.ReferenceScoreCacheCapacity
        };
#else
        _ = trainer;
        return new { enabled = false, lookups = 0, hits = 0, misses = 0, stores = 0, capacity = 0 };
#endif
    }

    private static LoraLinear[] Adapters(SimulationDpoModelContext models) =>
        Enumerable.Range(0, models.Policy.Blocks.Count)
            .SelectMany(block => models.LoraConfig.TargetModules
                .Select(target => (LoraLinear)models.Policy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray();

    private static IReadOnlyList<DpoVectorBatch> PrepareBatches(
        IReadOnlyList<SimulationDpoInputRow> rows, BpeTokenizer tokenizer)
    {
        var preferences = rows.Select(row => new LogicalDpoPreferenceExample(row.Prompt, row.Chosen, row.Rejected)
        {
            ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator,
            RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator
        }).ToArray();
        return Require(LogicalDpoPreparedBatchMaterializer.Prepare(preferences, tokenizer));
    }

    // The supervised carrier is the validated chosen side without padding.
    private static int[] Carrier(DpoBatch batch)
    {
        var length = batch.ExactMaterial!.ChosenLength;
        var ids = new int[length];
        for (var i = 0; i < length; i++) ids[i] = (int)batch.ChosenIds.Data[i];
        return ids;
    }

    private static object Rows(IReadOnlyList<SimulationDpoInputRow> rows, StateScore[] panel) =>
        rows.Select((row, index) => new { id = row.Id, score = panel[index] }).ToArray();

    private static StateScore[] Panel(LlmModel policy, IReadOnlyList<DpoVectorBatch> batches, ref int forwards)
    {
        var scores = new StateScore[batches.Count];
        for (var index = 0; index < batches.Count; index++)
        {
            var batch = batches[index].Batch;
            var chosen = SideScore(policy, batch.ChosenIds, batch.ChosenResponseMask ?? batch.ChosenMask);
            var rejected = SideScore(policy, batch.RejectedIds, batch.RejectedResponseMask ?? batch.RejectedMask);
            forwards += 2;
            scores[index] = new StateScore(
                chosen.MeanLogProbability, rejected.MeanLogProbability,
                -chosen.MeanLogProbability,
                chosen.MeanLogProbability - rejected.MeanLogProbability,
                chosen.CorrectTargets, chosen.TargetCount,
                chosen.TargetCount > 0 && chosen.CorrectTargets == chosen.TargetCount,
                chosen.MeanGoldRank);
        }

        return scores;
    }

    // Teacher-forced readout over the assigned response positions only; shifted ids, no free generation.
    private static SideReadout SideScore(LlmModel model, Tensor ids, Tensor mask)
    {
        var logits = model.Forward(ids, training: false);
        var t = logits.Shape.Dimensions[1];
        var v = logits.Shape.Dimensions[2];
        var sum = 0f;
        var count = 0f;
        var correct = 0;
        var targets = 0;
        long rankSum = 0;

        for (var i = 0; i < t - 1; i++)
        {
            var maskValue = mask.Data[i + 1];
            if (maskValue == 0f) continue;
            var targetId = (int)ids.Data[i + 1];

            var maxValue = float.NegativeInfinity;
            var best = 0;
            for (var j = 0; j < v; j++)
            {
                var value = logits.Data[i * v + j];
                if (value > maxValue) { maxValue = value; best = j; }
            }

            var logSumExp = 0f;
            for (var j = 0; j < v; j++) logSumExp += MathF.Exp(logits.Data[i * v + j] - maxValue);
            logSumExp = MathF.Log(logSumExp);
            sum += (logits.Data[i * v + targetId] - maxValue - logSumExp) * maskValue;
            count += maskValue;

            targets++;
            if (best == targetId) correct++;
            var goldLogit = logits.Data[i * v + targetId];
            var above = 0;
            for (var j = 0; j < v; j++) if (logits.Data[i * v + j] > goldLogit) above++;
            rankSum += above + 1;
        }

        if (targets == 0) throw new InvalidOperationException("N61_NO_RESPONSE_TARGETS");
        return new SideReadout(count > 0f ? sum / count : 0f, correct, targets, (double)rankSum / targets);
    }

    // The saved-root loader of the existing simulation path; no random baseline is ever constructed.
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
                throw new InvalidOperationException("N61_SAVED_ROOT_IDENTITY_MISMATCH");
            return loaded.Value;
        }

        var policy = Load();
        var reference = Load();
        var weights = Path.GetFullPath(Path.Combine(RepositoryRoot, policy.ParentManifest!.RepositoryRelativeWeightsPath));
        return new(policy.Model, reference.Model, options.Tokenizer, options.StartRequest.LoraConfig!,
            options.StartRequest.AdapterSeedAuthority!, weights, B0WeightsSha256,
            token => { token.ThrowIfCancellationRequested(); return Load().Model; });
    }

    private static void Claim(string path, string nonce, Process process, DateTimeOffset startedUtc, string scope)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, new
        {
            experimentId = RunIdPrefix,
            scope,
            nonce,
            pid = process.Id,
            processStartUtc = process.StartTime.ToUniversalTime(),
            issuedUtc = startedUtc
        }, Json);
    }

    private static long Peak(Process process)
    {
        process.Refresh();
        return process.PeakWorkingSet64;
    }

    private static void WriteNew(string path, object payload)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, payload, Json);
    }

    private static void Beat(string message) =>
        Console.WriteLine($"[{DateTimeOffset.UtcNow:HH:mm:ss}] {message}");

    private static T Require<T>(OperationResult<T> result) =>
        result.IsSuccess && result.Value is { } value
            ? value
            : throw new InvalidOperationException(result.ErrorCode + ":" + result.ErrorMessage);

    private sealed record SideReadout(float MeanLogProbability, int CorrectTargets, int TargetCount, double MeanGoldRank);

    public sealed record StateScore(
        float ChosenMeanLogProbability,
        float RejectedMeanLogProbability,
        float ChosenNll,
        float Margin,
        int ChosenCorrectTokens,
        int ChosenTargetDenominator,
        bool TeacherForcedResponseExact,
        double ChosenMeanGoldRank);
}
