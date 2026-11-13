// Мост опыта GRADB-LOCALITY-01.
//
// Карточка меняет ровно один скаляр: порядок обхода вложенных циклов правого градиента
// внутри умножения матриц. Плечо A — исходный порядок, плечо B — порядок строка/внутренний
// индекс с вынесенным скаляром. Всё остальное в плечах одинаково, включая общую
// измерительную обвязку участка.
//
// Этот хост — потребитель общего обученного старта семейства h512-dt-segmented. Его
// производитель назначен другой карточкой; подготовка здесь запрещена, и при отсутствии
// готового набора плечо отказывается работать без обучения.

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
using TrainingLoop.Optimizers;
using TransformerModel;

public static class GradBExperimentBridge
{
    public const string CompiledVariant =
#if KGB_ARM_B
        "B";
#else
        "A";
#endif

    private const string ExperimentId = "GRADB-LOCALITY-01";
    private const string RepositoryRoot = "C:/Projects/llm_mvp_core";
    private const string HostRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB";
    private const string SourceRoot = "C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/KGB";
    private const string StudyRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KGB/results";
    private const string SharedRoot =
        "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1";
    private const string FamilyId = "h512-dt-segmented";
    private const string ConfigSha256 = "e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40";
    private const string B0ManifestSha256 = "c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d";
    private const string B0WeightsSha256 = "a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911";
    private const string DatasetVersion = "n62-csharp-subtraction-v1";

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    private const string Prefix =
        "Explain in one short English sentence what the method returns. Describe argument order. Code: ";

    private sealed record Row(string Id, string Code, string Chosen, string Rejected)
    {
        public string Prompt => Prefix + Code;
    }

    private sealed record ResponseScore(int ResponseTargets, int CorrectTargets, bool ResponseExact,
        float MeanNll, double MeanGoldRank);

    private sealed record CarrierInfo(IReadOnlyList<int> Ids, float[] Mask, int PromptLength, int Length,
        int RejectedLength, int SequenceLength, IReadOnlyList<int> ResponseTargets, string DigestSha256);

    // Две закреплённые карточкой пары: семантический эталон — направление вычитания.
    private static readonly Row[] Rows =
    [
        new("D-T1",
            "int Delta(int first,int second){return first-second;}",
            "Returns the first argument minus the second argument.",
            "Returns the second argument minus the first argument."),
        new("D-T2",
            "int Delta(int first,int second){return second-first;}",
            "Returns the second argument minus the first argument.",
            "Returns the first argument minus the second argument.")
    ];

    // ── --arm A|B: два настоящих обновления предпочтений на восстановленном общем старте ──────────

    public static Task<int> RunArmAsync(string arm, string armRoot, CancellationToken ct)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var startedUtc = DateTimeOffset.UtcNow;
            if (arm != CompiledVariant)
                throw new InvalidOperationException($"KGB_VARIANT_MISMATCH: binary is {CompiledVariant}, arm {arm} requested");
            if (!string.Equals(Normalize(armRoot), $"{StudyRoot}/{arm}", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"KGB_ARM_ROOT_NOT_ASSIGNED: expected {StudyRoot}/{arm}");
            Directory.CreateDirectory(StudyRoot);
            if (Directory.Exists(armRoot)) throw new IOException("KGB_ARM_EXISTS_OR_UNKNOWN: preserved");

            // Отметка начала плеча создаётся один раз. Если она уже есть, прежняя попытка
            // сохраняется нетронутой: её исход читается и записывается в текущую выдачу,
            // номер запуска не меняется, новый каталог не создаётся.
            var markerPath = $"{StudyRoot}/{arm}.started.json";
            var blockedPath = $"{StudyRoot}/{arm}.blocked.json";
            object? priorAttempt = null;
            if (File.Exists(markerPath))
            {
                using var priorMarkerDocument = JsonDocument.Parse(File.ReadAllText(markerPath));
                if (priorMarkerDocument.RootElement.GetProperty("runId").GetString() != ExperimentId + ":" + arm)
                    throw new IOException("KGB_PRIOR_ATTEMPT_FOREIGN: preserved, not replayed");
                if (!File.Exists(blockedPath))
                    throw new IOException("KGB_ATTEMPT_EXISTS_OR_UNKNOWN: prior attempt has no refusal receipt; preserved, not replayed");

                using var priorBlockedDocument = JsonDocument.Parse(File.ReadAllText(blockedPath));
                var priorBlocked = priorBlockedDocument.RootElement;
                if (priorBlocked.GetProperty("status").GetString() != "SHARED_START_UNAVAILABLE" ||
                    priorBlocked.GetProperty("nativeTrainCalls").GetInt32() != 0 ||
                    priorBlocked.GetProperty("modelForwards").GetInt32() != 0 ||
                    priorBlocked.GetProperty("optimizerUpdates").GetInt32() != 0)
                    throw new IOException("KGB_ATTEMPT_EXISTS_OR_UNKNOWN: prior attempt may have reached the model; preserved, not replayed");

                priorAttempt = new
                {
                    status = "EXISTS_OR_UNKNOWN_RESOLVED",
                    marker = markerPath,
                    receipt = blockedPath,
                    receiptSha256 = Sha(File.ReadAllBytes(blockedPath)),
                    priorOutcome = "SHARED_START_UNAVAILABLE",
                    priorNativeTrainCalls = 0,
                    priorModelForwards = 0,
                    priorOptimizerUpdates = 0,
                    continuationReason = "the designated family producer published the shared start; the same run id continues and no prior evidence is deleted"
                };
            }
            else
            {
                using var marker = new FileStream(markerPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                var text = new UTF8Encoding(false).GetBytes(JsonSerializer.Serialize(new
                {
                    experimentId = ExperimentId,
                    runId = ExperimentId + ":" + arm,
                    arm,
                    compiledVariant = CompiledVariant,
                    pid = process.Id,
                    startedUtc = startedUtc.ToString("O")
                }, Json));
                marker.Write(text);
                marker.Flush(flushToDisk: true);
            }

            // Предусловие потребителя: общий обученный старт обязан быть готов и квалифицирован.
            // Подготовка здесь запрещена, другой донор не выбирается.
            var bundle = ReadCompleteBundle(arm, startedUtc, process.Id);

            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(armRoot);
            var checkpointRoot = $"{armRoot}/checkpoints";
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(checkpointRoot);
            WriteNew($"{armRoot}/issued.json", new
            {
                experimentId = ExperimentId, runId = ExperimentId + ":" + arm, arm,
                compiledVariant = CompiledVariant,
                factor = arm == "A"
                    ? "original gradB traversal batch -> inner -> column -> row"
                    : "gradB traversal batch -> row -> inner with the hoisted left scalar",
                sharedBundleSha256 = Sha(File.ReadAllBytes($"{SharedRoot}/bundle.json")),
                priorAttempt,
                sourceFingerprints = HostSourceFingerprints(),
                pid = process.Id, issuedUtc = startedUtc
            });

            Beat($"плечо {arm}: канонический загрузчик и восстановление общего обученного старта");
            var models = LoadModels(ct);
            foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;
            models.Policy.LoadCheckpoint($"{SharedRoot}/S0.weights.bin");
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(models.LoraConfig, models.Seeds);
            var sharedRng = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
                File.ReadAllBytes($"{SharedRoot}/S0.rng.json")) ?? throw new InvalidOperationException("KGB_RNG_UNREADABLE");
            models.Policy.RestoreRandomState(sharedRng, new LlmModelRandomStateIdentity(topology, loraIdentity));

            var startWeights = AdapterValues(models.Policy, models.LoraConfig);
            var startDigest = Sha(Bytes(startWeights));
            var sourceSha = Sha(File.ReadAllBytes($"{SharedRoot}/data.source.jsonl"));
            var batches = Materialize(models, sourceSha, startedUtc, out var content);
            var carriers = batches.Select(batch => Carrier(batch.Batch)).ToArray();

            var forwards = 0;
            Beat($"плечо {arm}: чтение обучающей панели до обновлений");
            var diagBefore = carriers.Select(carrier => Score(models.Policy, carrier, ref forwards)).ToArray();

            var adapters = AttachedAdapters(models.Policy, models.LoraConfig);
            var recipient = DpoProjectLoraRecipient.Capture(models.Policy, models.Reference, adapters, models.LoraConfig);
            var profile = DpoExecutionProfile.Capture(batches[0].Batch, recipient.Bindings);
            var salt = AuthoritySalt();
            var legal = Require(LegalTrainingTeacherAdmissionAuthority.Create([], 0, salt));
            var live = true;
            var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                legal.ValidateBatch, () => live, null,
                legal.SealLogicalCapabilityReplay, legal.VerifyLogicalCapabilityReplay,
                legal.SealLogicalCapabilityExecutableManifest,
                legal.VerifyLogicalCapabilityExecutableManifest, legal.ValidateBatch);

            var stepRecords = new List<object>();
            double trainWallMs;
            long trainAllocated;
            double trainCpuMs;
            int finalStep;

            using var lease = Require(((IServerOwnedDpoTrainingLeaseBroker)authority)
                .AcquireCodeDocumentationTrainingLease(content, ExperimentId + ":" + arm));
            try
            {
                var mutation = Require(lease.IssueDpoTrainerMutationAdmissionCapability());
                var observer = new DpoTrainerPublicationObserver
                {
                    BeforePolicyFinalMove = bundleState =>
                    {
                        _ = Require(RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                            checkpointRoot, bundleState.Step, bundleState.OptimizerState, ct));
                        if (bundleState.RandomState is null) throw new InvalidOperationException("KGB_RANDOM_STATE_MISSING");
                        _ = Require(RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                            Path.Combine(checkpointRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(bundleState.Step)),
                            bundleState.RandomState));
                    }
                };

                // Свежий оптимизатор предпочтений со шага 0: моменты обучения с учителем не импортируются.
                var trainer = DpoTrainer.CreateForProjectLora(models.Policy, models.Reference, checkpointRoot,
                    mutation, recipient, profile, null, observer,
                    step => models.Policy.CaptureRandomState(topology, loraIdentity, step),
                    state => models.Policy.RestoreRandomState(state, new(topology, loraIdentity)));
                if (trainer.ExportOptimizerState().Step != 0)
                    throw new InvalidOperationException("KGB_DPO_OPTIMIZER_NOT_FRESH");

                var watch = Stopwatch.StartNew();
                var allocated0 = GC.GetTotalAllocatedBytes(precise: true);
                var cpu0 = process.TotalProcessorTime;
                for (var index = 0; index < batches.Count; index++)
                {
                    ct.ThrowIfCancellationRequested();
                    var step = index + 1;
                    Beat($"плечо {arm}: обновление предпочтений {step} на уроке {Rows[index].Id}");
                    var timing = new DpoStepVectorTiming();
                    var before = AdapterValues(models.Policy, models.LoraConfig);

                    // Измеритель участка обнуляется перед каждым шагом и читается сразу после него.
                    GradBExperimentTelemetry.Reset();
                    var stepAllocated0 = GC.GetTotalAllocatedBytes(precise: true);
                    var stepCpu0 = process.TotalProcessorTime;
                    var stepWatch = Stopwatch.StartNew();
                    var metrics = trainer.StepVector(new[] { batches[index] }, step, timing);
                    stepWatch.Stop();
                    var stepCpuMs = (process.TotalProcessorTime - stepCpu0).TotalMilliseconds;
                    var stepAllocated = GC.GetTotalAllocatedBytes(precise: true) - stepAllocated0;
                    var snapshot = GradBExperimentTelemetry.Snapshot();

                    var after = AdapterValues(models.Policy, models.LoraConfig);
                    stepRecords.Add(new
                    {
                        step, id = Rows[index].Id, metrics,
                        wallMs = stepWatch.Elapsed.TotalMilliseconds,
                        cpuMs = stepCpuMs,
                        allocatedBytes = stepAllocated,
                        timing = timing.Snapshot(),
                        gradB = new
                        {
                            calls = snapshot.Calls,
                            elapsedMs = snapshot.StopwatchFrequency == 0
                                ? 0d
                                : snapshot.ElapsedTicks * 1000d / snapshot.StopwatchFrequency,
                            elapsedTicks = snapshot.ElapsedTicks,
                            multiplyAddCount = snapshot.MultiplyAddCount,
                            stopwatchFrequency = snapshot.StopwatchFrequency,
                            note = "вложенный интервал участка правого градиента; со временем шага не складывается"
                        },
                        weightDelta = Delta(before, after),
                        optimizerStep = trainer.ExportOptimizerState().Step
                    });
                }

                watch.Stop();
                trainWallMs = watch.Elapsed.TotalMilliseconds;
                trainAllocated = GC.GetTotalAllocatedBytes(precise: true) - allocated0;
                trainCpuMs = (process.TotalProcessorTime - cpu0).TotalMilliseconds;
                finalStep = trainer.ExportOptimizerState().Step;
            }
            finally
            {
                live = false;
            }

            Beat($"плечо {arm}: чтение обучающей панели после двух обновлений");
            var diagAfter = carriers.Select(carrier => Score(models.Policy, carrier, ref forwards)).ToArray();
            var endWeights = AdapterValues(models.Policy, models.LoraConfig);
            var finalPath = $"{armRoot}/final.weights.bin";
            using (var stream = new FileStream(finalPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            { models.Policy.SaveCheckpointToStream(stream); stream.Flush(flushToDisk: true); }

            process.Refresh();
            WriteNew($"{armRoot}/metrics.json", new
            {
                experimentId = ExperimentId, runId = ExperimentId + ":" + arm, arm,
                compiledVariant = CompiledVariant,
                sharedStart = new
                {
                    family = FamilyId,
                    bundle = $"{SharedRoot}/bundle.json",
                    learningQualified = bundle.GetProperty("learning_qualified").GetBoolean(),
                    commonUpdatesHere = 0, commonReadoutsHere = 0,
                    startWeightsSha256 = startDigest
                },
                carriers = Rows.Select((row, i) => new
                {
                    row.Id,
                    promptLength = carriers[i].PromptLength,
                    chosenLength = carriers[i].Length,
                    rejectedLength = carriers[i].RejectedLength,
                    paddedLength = carriers[i].SequenceLength,
                    digestSha256 = carriers[i].DigestSha256
                }).ToArray(),
                steps = stepRecords,
                diagTrain = new
                {
                    before = Rows.Select((row, i) => new { id = row.Id, score = diagBefore[i] }).ToArray(),
                    after = Rows.Select((row, i) => new { id = row.Id, score = diagAfter[i] }).ToArray(),
                    forwards
                },
                finalWeights = new
                {
                    path = finalPath,
                    sha256 = Sha(File.ReadAllBytes(finalPath)),
                    adapterValuesSha256 = Sha(Bytes(endWeights)),
                    deltaFromStart = Delta(startWeights, endWeights)
                },
                costs = new
                {
                    trainingWallMs = trainWallMs,
                    trainingCpuMs = trainCpuMs,
                    trainingAllocatedBytes = trainAllocated,
                    peakWorkingSetBytes = process.PeakWorkingSet64,
                    totalProcessorMs = process.TotalProcessorTime.TotalMilliseconds
                },
                budgetActual = new
                {
                    updates = batches.Count,
                    trainingForwards = batches.Count * 4,
                    backwards = batches.Count * 4,
                    diagTrainForwards = forwards,
                    generate = 0
                },
                finalOptimizerStep = finalStep,
                startedUtc, completedUtc = DateTimeOffset.UtcNow,
                limitation = "Два обновления предпочтений на двух закреплённых парах после общего старта; " +
                             "отложенная выборка и свободная генерация не измеряются."
            });

            Console.WriteLine($"ARM_{arm}_DONE updates={batches.Count} optimizerStep={finalStep} wallMs={trainWallMs:F0}");
            return Task.FromResult(0);
        }
        catch (SharedStartUnavailableException blocked)
        {
            Console.Error.WriteLine($"ARM_{arm}_BLOCKED SHARED_START_UNAVAILABLE: {blocked.Message}");
            return Task.FromResult(7);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"ARM_{arm}_FAILED {error.GetType().Name}: {error.Message}");
            return Task.FromResult(5);
        }
    }

    // ── вспомогательные части, повторяющие исходную нативную цепь ─────────────────────────────────

    // Отказ потребителя: общий обученный старт отсутствует или не квалифицирован.
    private sealed class SharedStartUnavailableException(string message) : Exception(message);

    private static JsonElement ReadCompleteBundle(string arm, DateTimeOffset startedUtc, int pid)
    {
        var path = $"{SharedRoot}/bundle.json";
        string? reason = null;
        if (!Directory.Exists(SharedRoot)) reason = "каталог семейства отсутствует";
        else if (!File.Exists(path)) reason = "набор семейства не опубликован";

        JsonElement root = default;
        if (reason is null)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            root = document.RootElement.Clone();
            if (root.GetProperty("status").GetString() != "COMPLETE") reason = "набор не в состоянии COMPLETE";
            else if (!root.GetProperty("learning_qualified").GetBoolean()) reason = "обучение производителя не квалифицировано";
        }

        if (reason is null) return root;

        // Частичная квитанция записывается до отказа: обучение не начиналось, бюджет не тратился.
        var blockedPath = $"{StudyRoot}/{arm}.blocked.json";
        if (!File.Exists(blockedPath))
        {
            WriteNew(blockedPath, new
            {
                experimentId = ExperimentId, runId = ExperimentId + ":" + arm, arm,
                compiledVariant = CompiledVariant,
                status = "SHARED_START_UNAVAILABLE",
                family = FamilyId,
                expectedBundle = path,
                reason,
                producerCard = "DPO-TOKENIZER-SNAPSHOT-01",
                producerHost = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/TS01",
                localPreparationAllowed = false,
                alternateDonorAllowed = false,
                nativeTrainCalls = 0, modelForwards = 0, optimizerUpdates = 0,
                pid, startedUtc = startedUtc.ToString("O"),
                note = "Потребитель не готовит общий старт сам и не выбирает другой чекпойнт; " +
                       "учебная цель остаётся открытой до публикации набора семейства."
            });
        }

        throw new SharedStartUnavailableException($"{reason}; ожидался {path}");
    }

    private static byte[] SourceJsonlBytes()
    {
        var text = string.Concat(Rows.Select(row => JsonSerializer.Serialize(new
        {
            id = row.Id,
            dataset_version = DatasetVersion,
            domain = "csharp",
            pair_kind = "CODE_TO_DOC",
            provenance = "simulation",
            prompt = row.Prompt,
            chosen = row.Chosen,
            rejected = row.Rejected
        }) + "\n"));
        return new UTF8Encoding(false).GetBytes(text);
    }

    private static IReadOnlyList<DpoVectorBatch> Materialize(
        SimulationDpoModelContext models, string sourceSha, DateTimeOffset utc, out DpoTrainingContentBatch content)
    {
        var governance = new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + sourceSha });
        var rich = Rows.Select(row => new DpoPair(row.Prompt, row.Chosen, row.Rejected, "code-documentation", utc)
        {
            GovernanceDecision = governance,
            ChosenAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator),
            RejectedAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator)
        }).ToArray();
        _ = Require(TrainingDataGovernanceAdmission.Validate(rich));
        content = Require(DpoPairTrainingContentAuthority.Project(rich));
        var preferences = rich.Select(row => new LogicalDpoPreferenceExample(row.Prompt, row.Chosen, row.Rejected)
        {
            ChosenAuthorKind = row.ChosenAuthorship!.AuthorKind,
            RejectedAuthorKind = row.RejectedAuthorship!.AuthorKind
        }).ToArray();
        var batches = Require(LogicalDpoPreparedBatchMaterializer.Prepare(preferences, models.Tokenizer));
        if (batches.Count != Rows.Length) throw new InvalidOperationException("KGB_BATCH_COUNT_UNEXPECTED");
        foreach (var batch in batches)
            _ = DpoExactPairMaterial.CaptureAndValidate(batch.Batch, models.Policy.Config.VocabSize,
                models.Policy.Config.ContextLen, models.Tokenizer.ComputeStateSha256(), DpoExactPairMaterial.Version);
        return batches;
    }

    private static CarrierInfo Carrier(DpoBatch batch)
    {
        var material = batch.ExactMaterial ?? throw new InvalidOperationException("KGB_EXACT_MATERIAL_MISSING");
        var ids = new int[material.ChosenLength];
        for (var i = 0; i < ids.Length; i++) ids[i] = (int)batch.ChosenIds.Data[i];
        var mask = new float[material.ChosenLength];
        for (var i = 0; i < mask.Length; i++) mask[i] = batch.ChosenResponseMask!.Data[i];
        var targets = new List<int>();
        for (var i = 0; i < ids.Length - 1; i++) if (mask[i + 1] != 0f) targets.Add(i);
        if (targets.Count == 0) throw new InvalidOperationException("KGB_NO_RESPONSE_TARGETS");
        return new CarrierInfo(ids, mask, material.PromptLength, material.ChosenLength,
            material.RejectedLength, material.SequenceLength, targets, material.DigestSha256);
    }

    private static ResponseScore Score(LlmModel model, CarrierInfo carrier, ref int forwards)
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

    private static LoraLinear[] AttachedAdapters(LlmModel policy, LoraConfig config) =>
        Enumerable.Range(0, policy.Blocks.Count)
            .SelectMany(block => config.TargetModules.Select(target => (LoraLinear)policy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray();

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
            throw new InvalidOperationException($"KGB_BINDINGS_UNEXPECTED: {bindings.Length}");
        return bindings;
    }

    private static float[] AdapterValues(LlmModel policy, LoraConfig config) =>
        AdapterBindings(policy, config).SelectMany(binding => binding.Parameter.Value.Data).ToArray();

    private static object Delta(float[] before, float[] after)
    {
        var norm = 0.0;
        var maximum = 0.0;
        for (var i = 0; i < before.Length; i++)
        {
            var difference = (double)after[i] - before[i];
            norm += difference * difference;
            maximum = Math.Max(maximum, Math.Abs(difference));
        }

        return new { values = before.Length, l2 = Math.Sqrt(norm), maxAbsolute = maximum };
    }

    private static byte[] Bytes(float[] values)
    {
        var bytes = new byte[values.Length * sizeof(float)];
        Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    // Соль законной authority одна на исследование и одинакова в обоих плечах.
    private static byte[] AuthoritySalt()
    {
        var path = $"{StudyRoot}/authority-salt.bin";
        if (File.Exists(path))
        {
            var existing = File.ReadAllBytes(path);
            if (existing.Length != 32) throw new InvalidOperationException("KGB_AUTHORITY_SALT_INVALID");
            return existing;
        }

        var salt = RandomNumberGenerator.GetBytes(32);
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(salt);
        stream.Flush(flushToDisk: true);
        return salt;
    }

    private static object[] HostSourceFingerprints() =>
        Directory.GetFiles($"{SourceRoot}/Common", "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles($"{SourceRoot}/{CompiledVariant}", "*.cs", SearchOption.AllDirectories))
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => (object)new
            {
                path = path.Replace('\\', '/'),
                sha256 = Sha(File.ReadAllBytes(path))
            })
            .ToArray();

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
                throw new InvalidOperationException("KGB_SAVED_ROOT_IDENTITY_MISMATCH");
            return loaded.Value;
        }

        var policy = Load();
        var reference = Load();
        var weights = Path.GetFullPath(Path.Combine(RepositoryRoot, policy.ParentManifest!.RepositoryRelativeWeightsPath));
        return new(policy.Model, reference.Model, options.Tokenizer, options.StartRequest.LoraConfig!,
            options.StartRequest.AdapterSeedAuthority!, weights, B0WeightsSha256,
            token => { token.ThrowIfCancellationRequested(); return Load().Model; });
    }

    private static T Require<T>(OperationResult<T> result) =>
        result.IsSuccess && result.Value is { } value
            ? value
            : throw new InvalidOperationException(result.ErrorCode + ":" + result.ErrorMessage);

    private static string Normalize(string path) => Path.GetFullPath(path).Replace('\\', '/').TrimEnd('/');

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void WriteNew(string path, object payload)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, payload, Json);
    }

    private static void Beat(string message) => Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] {message}");
}
