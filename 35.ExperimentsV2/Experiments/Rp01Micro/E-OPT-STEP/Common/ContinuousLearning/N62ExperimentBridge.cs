// Мост опыта N62-R1. Живёт внутри настоящей экспериментальной ContinuousLearning,
// поэтому пользуется её существующими внутренними выдающими типами. Он НЕ вызывает
// CodeDocumentationSimulationDpoExperiment.Run: его main, калибровка и guards не меняются.
// Повторяет нативную подготовку и конечный цикл по шагам карточки, меняется только
// реализация сравнения замороженной базы внутри DpoTrainer плеча.
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using LoRA;
using ModelServer;
using TrainableRouter.Training;
using TransformerModel;

namespace ContinuousLearning;

/// <summary>Неизменяемый запрос одного плеча опыта N62-R1.</summary>
public sealed record N62RunRequest(
    string ExperimentId,
    string Arm,
    string RepositoryRoot,
    string StudyRoot,
    string ArmOutputDirectory,
    string InputPath,
    string InputSha256,
    int ExpectedPairCount,
    string ConfigSha256,
    string B0WeightsSha256,
    string B0ManifestSha256,
    string AuthoritySaltPath);

public static class N62ExperimentBridge
{
    private static readonly JsonSerializerOptions Json =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static void Run(N62RunRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Arm is not ("A" or "B")) throw new ArgumentException("N62_ARM_INVALID");

        // Каталог плеча создаётся один раз; существующий — отказ с сохранением старого.
        if (Directory.Exists(request.ArmOutputDirectory) || File.Exists(request.ArmOutputDirectory))
            throw new InvalidOperationException("N62_ARM_OUTPUT_EXISTS");
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(request.ArmOutputDirectory);

        var issuedMarker = Path.Combine(request.StudyRoot, request.Arm + ".issued.json");
        if (File.Exists(issuedMarker)) throw new InvalidOperationException("N62_ARM_ALREADY_ISSUED");

        var utcStart = DateTimeOffset.UtcNow;
        var wholeStart = Stopwatch.GetTimestamp();

        // Шаг 1. Нативная проверка входа: точный хэш, число пар, дубликаты, ничьи.
        var rows = Require(CodeDocumentationSimulationDpoExperiment.Inspect(
            request.InputPath, request.InputSha256, request.ExpectedPairCount,
            SimulationDpoDomain.CodeDocumentation));

        // Шаг 2. Загрузка B0 узкой композицией прежнего инструмента, без его main.
        var models = LoadModels(request, cancellationToken);

        // Шаг 3. Повтор нативной подготовки.
        if (HashFile(models.SourceWeightsPath) != models.SourceWeightsSha256 ||
            ReferenceEquals(models.Policy, models.Reference) || models.LoraConfig.Dropout != 0)
            throw new InvalidOperationException("N62_SAVED_ROOT_INVALID");
        foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;

        var governance = new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + request.InputSha256 });
        var rich = rows.Select(row => new DpoPair(row.Prompt, row.Chosen, row.Rejected, "code-documentation", utcStart)
        {
            GovernanceDecision = governance,
            ChosenAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator),
            RejectedAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator)
        }).ToArray();
        _ = Require(TrainingDataGovernanceAdmission.Validate(rich));
        var content = Require(DpoPairTrainingContentAuthority.Project(rich));

        var preferences = rich.Select(row => new LogicalDpoPreferenceExample(row.Prompt, row.Chosen, row.Rejected)
        {
            ChosenAuthorKind = row.ChosenAuthorship!.AuthorKind,
            RejectedAuthorKind = row.RejectedAuthorship!.AuthorKind
        }).ToArray();
        var batches = Require(LogicalDpoPreparedBatchMaterializer.Prepare(preferences, models.Tokenizer));

        var materials = new List<object>();
        for (var index = 0; index < batches.Count; index++)
        {
            var captured = DpoExactPairMaterial.CaptureAndValidate(batches[index].Batch,
                models.Policy.Config.VocabSize, models.Policy.Config.ContextLen,
                models.Tokenizer.ComputeStateSha256(), DpoExactPairMaterial.Version);
            var material = captured.ExactMaterial!;
            materials.Add(new
            {
                id = rows[index].Id,
                material.DigestSha256,
                material.PromptLength,
                chosenResponseTokens = material.ChosenLength - material.PromptLength,
                rejectedResponseTokens = material.RejectedLength - material.PromptLength,
                material.SequenceLength
            });
        }

        // Шаг 4. Состав получателя — реально подключённые адаптеры q/v.
        var beforePolicy = Snapshot(models.Policy);
        var beforeReference = Snapshot(models.Reference);
        var adapters = Enumerable.Range(0, models.Policy.Blocks.Count)
            .SelectMany(block => models.LoraConfig.TargetModules
                .Select(target => (LoraLinear)models.Policy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray();
        var recipient = DpoProjectLoraRecipient.Capture(models.Policy, models.Reference, adapters, models.LoraConfig);
        var profile = DpoExecutionProfile.Capture(batches[0].Batch, recipient.Bindings);

        // Шаг 5. Настоящие authority и lease. Соль одна на study, общая для плеч.
        var authoritySalt = File.ReadAllBytes(request.AuthoritySaltPath);
        if (authoritySalt.Length != 32) throw new InvalidOperationException("N62_AUTHORITY_SALT_INVALID");
        var legal = Require(LegalTrainingTeacherAdmissionAuthority.Create([], 0, authoritySalt));
        var live = true;
        var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            legal.ValidateBatch, () => live, null,
            legal.SealLogicalCapabilityReplay, legal.VerifyLogicalCapabilityReplay,
            legal.SealLogicalCapabilityExecutableManifest,
            legal.VerifyLogicalCapabilityExecutableManifest, legal.ValidateBatch);

        var trainingRunId = request.ExperimentId + ":" + request.Arm;
        object? result = null;
        string? error = null;
        var issued = 0;
        var completed = 0;
        var timings = new List<DpoStepVectorTiming>(request.ExpectedPairCount);

        using var lease = Require(((IServerOwnedDpoTrainingLeaseBroker)authority)
            .AcquireCodeDocumentationTrainingLease(content, trainingRunId));
        try
        {
            var mutation = Require(lease.IssueDpoTrainerMutationAdmissionCapability());

            // Шаг 6. Личности RNG, приватный каталог состояния и наблюдатель публикации.
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(models.LoraConfig, models.Seeds);
            var checkpointRoot = Path.Combine(request.ArmOutputDirectory, "checkpoints");
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(checkpointRoot);

            var artifacts = new List<object>();
            var observer = new DpoTrainerPublicationObserver
            {
                ArtifactOwnershipProven = proof =>
                    artifacts.Add(new { proof.Step, proof.Kind, proof.Path, proof.Sha256, proof.Length }),
                BeforePolicyFinalMove = bundle =>
                {
                    var saved = Require(RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                        checkpointRoot, bundle.Step, bundle.OptimizerState, cancellationToken));
                    if (bundle.RandomState is null) throw new InvalidOperationException("N62_RANDOM_STATE_MISSING");
                    var random = Require(RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                        Path.Combine(checkpointRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(bundle.Step)),
                        bundle.RandomState));
                    WriteNew(Path.Combine(checkpointRoot, $"state-{bundle.Step:D6}.json"), new
                    {
                        bundle.Step, saved.OptimizerStateSha256, saved.OptimizerStateByteLength,
                        random.Sha256, random.ByteLength, bundle.TrialWeightsSha256, bundle.TrialWeightsByteLength
                    });
                },
                PublicationFailed = () => artifacts.Add(new { effect = "RECONCILE_BEFORE_RETRY" })
            };

            // Шаг 7. Один trainer на плечо, оба RNG-обратных вызова непустые.
            var trainer = DpoTrainer.CreateForProjectLora(models.Policy, models.Reference, checkpointRoot,
                mutation, recipient, profile, null, observer,
                step => models.Policy.CaptureRandomState(topology, loraIdentity, step),
                state => models.Policy.RestoreRandomState(state, new(topology, loraIdentity)));
            var optimizerSettings = trainer.OptimizerSettings;

            WriteNew(issuedMarker, new
            {
                request.ExperimentId, request.Arm, request.InputSha256,
                pairIds = rows.Select(x => x.Id), utc = DateTimeOffset.UtcNow,
                status = "ISSUED_EFFECT_UNKNOWN_UNTIL_COMPLETED"
            });

            // Снимок A0/B0: биты, RNG и переносимое состояние оптимизатора на шаге 0.
            var zeroOptimizer = trainer.ExportOptimizerState();
            WriteNew(Path.Combine(request.ArmOutputDirectory, "arm-zero-state.json"), new
            {
                step = zeroOptimizer.Step,
                optimizerBytesSha256 = Sha(zeroOptimizer.ToBytes()),
                policySha256 = Combined(beforePolicy),
                referenceSha256 = Combined(beforeReference)
            });

            // Шаг 8. Конечный цикл: ровно rows.Count итераций, batch 1.
            using var process = Process.GetCurrentProcess();
            var cpu0 = process.TotalProcessorTime;
            var gc0 = new[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };
            var alloc0 = GC.GetAllocatedBytesForCurrentThread();
            var watch = Stopwatch.StartNew();

            for (var offset = 0; offset < rows.Count; offset++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var step = completed + 1;
                if (trainer.ExportOptimizerState().Step != step - 1)
                    throw new InvalidOperationException("N62_UPDATE_BUDGET_EXCEEDED");
                var id = $"step-{step:D6}";
                WriteNew(Path.Combine(request.ArmOutputDirectory, id + ".issued.json"), new
                {
                    request.Arm, step, pairId = rows[offset].Id,
                    utc = DateTimeOffset.UtcNow,
                    optimizerBefore = trainer.ExportOptimizerState().Step
                });

                var timing = new DpoStepVectorTiming();
                timings.Add(timing);
                issued++;
                var metrics = trainer.StepVector(batches.Skip(offset).Take(1).ToArray(), step, timing);
                completed++;

                WriteNew(Path.Combine(request.ArmOutputDirectory, id + ".completed.json"), new
                {
                    request.Arm, step, metrics,
                    optimizerAfter = trainer.ExportOptimizerState().Step,
                    utc = DateTimeOffset.UtcNow
                });
            }

            watch.Stop();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - alloc0;
            process.Refresh();
            var cpuMs = (process.TotalProcessorTime - cpu0).TotalMilliseconds;
            var gcCounts = new[]
            {
                GC.CollectionCount(0) - gc0[0], GC.CollectionCount(1) - gc0[1], GC.CollectionCount(2) - gc0[2]
            };
            var peakWorkingSet = process.PeakWorkingSet64;

            live = false;
            var finalStep = completed;

            // Конечный readback через третью каноническую загрузку.
            var checkpoint = Path.Combine(checkpointRoot, DpoTrainer.BuildPolicyCheckpointFileName(finalStep));
            var afterPolicy = Snapshot(models.Policy);
            var afterReference = Snapshot(models.Reference);
            object readback;
            using (var stream = File.OpenRead(checkpoint))
            {
                _ = Require(LlmModel.ValidateFullCheckpointArtifact(stream, models.Policy.Config,
                    new FullCheckpointLoraTopology(models.LoraConfig.Rank, models.LoraConfig.TargetModules),
                    stream.Length, cancellationToken));
                stream.Position = 0;
                var fresh = models.CreateReadbackModel(cancellationToken);
                fresh.LoadCheckpoint(stream);
                var optimizer = Require(RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                    checkpointRoot, finalStep, cancellationToken: cancellationToken));
                var randomPath = Path.Combine(checkpointRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(finalStep));
                var random = Require(RepositoryOwnedDpoRandomStateFile.OpenAndValidate(randomPath,
                    HashFile(randomPath), new FileInfo(randomPath).Length, finalStep, topology, loraIdentity));
                fresh.RestoreRandomState(random, new(topology, loraIdentity));
                var freshSnapshot = Snapshot(fresh);
                readback = new
                {
                    optimizerStep = optimizer.State.Step,
                    optimizerSlots = optimizer.State.Entries.Select(x => x.Key).ToArray(),
                    optimizerEqualLive = optimizer.State.ToBytes().SequenceEqual(trainer.ExportOptimizerState().ToBytes()),
                    freshWeightsEqualLive = Combined(freshSnapshot) == Combined(afterPolicy),
                    randomStateReopened = true
                };
            }

            // Нулевой сдвиг LoRA записывается как наблюдение, а не как провал целостности.
            var changedLora = beforePolicy.Count(x => !x.Value.IsBase && x.Value.Hash != afterPolicy[x.Key].Hash);
            var changedBase = beforePolicy.Count(x => x.Value.IsBase && x.Value.Hash != afterPolicy[x.Key].Hash);

            result = new
            {
                experiment = request.ExperimentId,
                arm = request.Arm,
                factor = request.Arm == "A"
                    ? "original element-wise LINQ bit comparison of frozen base"
                    : "bitwise byte-span comparison of frozen base",
                core_commit = "62bcc8f2368840e22d2ea98ef16d9bd526ad9a39",
                measured_path = "11.DPO/DpoTrainer.cs::DpoProjectLoraRecipient.RequireValid",
                actual_learner = "11.DPO/DpoTrainer.cs::DpoTrainer.StepVector -> project-LoRA AdamW",
                lessons = rows.Select(x => x.Id).ToArray(),
                optimizer_settings = optimizerSettings,
                step_vector_calls_issued = issued,
                step_vector_calls_completed = completed,
                base_parameter_arrays = beforePolicy.Count(x => x.Value.IsBase),
                adapter_parameter_arrays = beforePolicy.Count(x => !x.Value.IsBase),
                train_cost = new
                {
                    allocated_bytes = allocated,
                    elapsed_ms = watch.Elapsed.TotalMilliseconds,
                    process_cpu_ms = cpuMs,
                    gc_collections = gcCounts
                },
                process_peak_working_set_bytes = peakWorkingSet,
                state = new
                {
                    policy_before_sha256 = Combined(beforePolicy),
                    policy_after_sha256 = Combined(afterPolicy),
                    reference_before_sha256 = Combined(beforeReference),
                    reference_after_sha256 = Combined(afterReference),
                    reference_unchanged = Combined(beforeReference) == Combined(afterReference),
                    changed_adapter_arrays = changedLora,
                    changed_base_arrays = changedBase,
                    base_bitwise_frozen = changedBase == 0
                },
                readback,
                artifacts,
                materials,
                step_timing = timings.Select(x => x.Snapshot()).ToArray(),
                source_weights_sha256_after = HashFile(models.SourceWeightsPath),
                limitations = new[]
                {
                    "Один процесс на плечо и фиксированный порядок A->B: время, CPU и пик процесса остаются наблюдениями без установленной причинности",
                    "Evaluate/Generate не вызываются; качество ответов здесь не измеряется",
                    "Нулевой сдвиг LoRA записан как наблюдение, а не как провал целостности"
                }
            };
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            error = exception is DpoMutationAdmissionException admission
                ? admission.ErrorCode
                : exception.GetType().Name + ":" + exception.Message;
        }
        finally
        {
            live = false;
        }

        WriteNew(Path.Combine(request.ArmOutputDirectory, "arm-result.json"), new
        {
            request.ExperimentId, request.Arm,
            utcStart, utcEnd = DateTimeOffset.UtcNow,
            wholeRunSeconds = Stopwatch.GetElapsedTime(wholeStart).TotalSeconds,
            status = error is null ? "COMPLETED" : "FAILED_PRESERVE_EVIDENCE",
            error, issued, completed,
            result
        });

        if (error is not null) throw new InvalidOperationException("N62_ARM_FAILED:" + error);
    }

    // Узкая композиция загрузчика: ровно то, что делает прежний инструмент, без его main.
    private static SimulationDpoModelContext LoadModels(N62RunRequest request, CancellationToken cancellationToken)
    {
        var verified = CodeDocumentationGenerationConfiguration.ReadVerified(name => name switch
        {
            "XIIGEN_CODE_DOCUMENTATION_GENERATION_ENABLED" => "true",
            "XIIGEN_CODE_DOCUMENTATION_GENERATION_CONFIG_SHA256" => request.ConfigSha256,
            _ => null
        }, request.RepositoryRoot, cancellationToken);
        if (!verified.IsSuccess || verified.Value is null) throw new InvalidOperationException(verified.ErrorCode);
        var options = verified.Value;

        CodeDocumentationRepositoryOwnedModelCheckpointStart Load()
        {
            var loaded = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(
                options.StartRequest, cancellationToken);
            if (!loaded.IsSuccess || loaded.Value is null) throw new InvalidOperationException(loaded.ErrorCode);
            var parent = loaded.Value.ParentManifest;
            if (parent is null || parent.Step != 1 || parent.ParentCheckpoint is not null ||
                parent.ManifestSha256 != request.B0ManifestSha256 || parent.WeightsSha256 != request.B0WeightsSha256 ||
                parent.DpoApplication != RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate ||
                loaded.Value.Model.GetCheckpointParameterEntries().Count != 55 ||
                loaded.Value.Model.BaseParameters.Count != 39)
                throw new InvalidOperationException("N62_SAVED_ROOT_IDENTITY_MISMATCH");
            return loaded.Value;
        }

        var policy = Load();
        var reference = Load();
        var weights = Path.GetFullPath(Path.Combine(request.RepositoryRoot,
            policy.ParentManifest!.RepositoryRelativeWeightsPath));
        if (HashFile(weights) != request.B0WeightsSha256)
            throw new InvalidOperationException("N62_SAVED_B0_CHANGED");
        return new(policy.Model, reference.Model, options.Tokenizer, options.StartRequest.LoraConfig!,
            options.StartRequest.AdapterSeedAuthority!, weights, request.B0WeightsSha256,
            token => { token.ThrowIfCancellationRequested(); return Load().Model; });
    }

    private sealed record ParameterState(string Hash, bool IsBase);

    private static Dictionary<string, ParameterState> Snapshot(LlmModel model)
    {
        var bases = model.BaseParameters.ToHashSet(ReferenceEqualityComparer.Instance);
        return model.GetCheckpointParameterEntries().ToDictionary(
            entry => entry.Key,
            entry => new ParameterState(
                Sha(System.Runtime.InteropServices.MemoryMarshal.AsBytes(entry.Parameter.Value.Data.AsSpan()).ToArray()),
                bases.Contains(entry.Parameter)),
            StringComparer.Ordinal);
    }

    private static string Combined(Dictionary<string, ParameterState> snapshot)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var key in snapshot.Keys.OrderBy(x => x, StringComparer.Ordinal))
        {
            hash.AppendData(System.Text.Encoding.UTF8.GetBytes(key));
            hash.AppendData(Convert.FromHexString(snapshot[key].Hash));
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static T Require<T>(OperationResult<T> result) =>
        result.IsSuccess && result.Value is { } value
            ? value
            : throw new InvalidOperationException(result.ErrorCode + ":" + result.ErrorMessage);

    private static void WriteNew(string path, object payload)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, payload, Json);
    }

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
