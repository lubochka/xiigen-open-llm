// Мост опыта N63-CORRECTION-COLLISION. Живёт внутри настоящей экспериментальной
// ContinuousLearning, поэтому пользуется её существующими внутренними выдающими типами.
// Он НЕ вызывает CodeDocumentationSimulationDpoExperiment.Run и НЕ вызывает публичный
// IncrementalTrainer с выдуманным паспортом учителя.
//
// Что меняем: только источник текста следующего supervised-урока, когда он побайтово
// совпадает с отвергнутым ответом исправляющего DPO. Алгоритмы A и B одинаковы.
//   A: SFT.Response = ошибочный текст (тот же, что DPO отверг);
//   B: SFT.Response = исправленный текст (тот же, что DPO предпочёл).
// Порядок DPO→SFT, модель, reference, оптимизаторы и панель измерений одинаковы.
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using LoRA;
using ModelServer;
using TensorCore.Core;
using TrainableRouter.Training;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;

namespace ContinuousLearning;

public static class N63CollisionBridge
{
    // Пины паспорта карточки. Их проверяет нативный загрузчик, здесь они только передаются.
    private const string RepositoryRoot = "C:/Projects/llm_mvp_core";
    private const string ConfigSha256 = "e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40";
    private const string B0WeightsSha256 = "a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911";
    private const string B0ManifestSha256 = "c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d";
    private const string HostRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C";

    // Назначенный карточкой design fixture. Читается один раз, его байты дают
    // nativeContentDigest для governance-привязки. Поле TeacherProvenanceProvided=false
    // не переписывается в true.
    private const string FixturePath =
        @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\replanning\possibleDifs\2026-10-20-learning-ingress-and-retention-review\ingress\fixtures\correction-collision.json";

    private static readonly JsonSerializerOptions Json =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private static readonly JsonSerializerOptions ReadJson = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static void Run(string outputRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
        if (!Path.IsPathFullyQualified(outputRoot))
            throw new ArgumentException("N63C_OUTPUT_NOT_ABSOLUTE");

        // Занятый результат не перезаписывается; ранее выданная попытка не повторяется.
        if (Directory.Exists(outputRoot) || File.Exists(outputRoot))
            throw new InvalidOperationException("N63C_RESULT_EXISTS_OR_UNKNOWN");
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(outputRoot);

        var utcStart = DateTimeOffset.UtcNow;
        var whole = Stopwatch.StartNew();
        var cancellationToken = CancellationToken.None;

        // ── Шаг 1. Прочитать design fixture ровно один раз ───────────────────────
        var fixtureBytes = File.ReadAllBytes(FixturePath);
        var nativeContentDigest = Sha(fixtureBytes);
        var fixture = JsonSerializer.Deserialize<CollisionFixture>(fixtureBytes, ReadJson)
            ?? throw new InvalidOperationException("N63C_FIXTURE_UNREADABLE");

        // Буквальные три triplets карточки и точное побайтовое совпадение коллизии.
        RequireLiteral(fixture.Prompt, "// file: Abs.cs", "T1.Prompt");
        RequireLiteral(fixture.Chosen, "static int Abs(int n)=>n<0?-n:n;", "T1.Chosen");
        RequireLiteral(fixture.Rejected, "static int Abs(int n)=>n<0?n:-n;", "T1.Rejected");
        RequireLiteral(fixture.CurrentIndexedSource, "static int Abs(int n)=>n<0?n:-n;", "CurrentIndexedSource");
        RequireLiteral(fixture.CorrectedSource, "static int Abs(int n)=>n<0?-n:n;", "CorrectedSource");
        if (fixture.Heldout is not { Length: 2 }) throw new InvalidOperationException("N63C_HELDOUT_SHAPE");
        RequireLiteral(fixture.Heldout[0].Prompt, "// file: Magnitude.cs", "Q1.Prompt");
        RequireLiteral(fixture.Heldout[0].Chosen, "static int Magnitude(int x)=>x<0?-x:x;", "Q1.Chosen");
        RequireLiteral(fixture.Heldout[0].Rejected, "static int Magnitude(int x)=>x<0?x:-x;", "Q1.Rejected");
        RequireLiteral(fixture.Heldout[1].Prompt, "// file: Sign.cs", "Q2.Prompt");
        RequireLiteral(fixture.Heldout[1].Chosen, "static int Sign(int x)=>x<0?-1:1;", "Q2.Chosen");
        RequireLiteral(fixture.Heldout[1].Rejected, "static int Sign(int x)=>x<0?1:-1;", "Q2.Rejected");

        // Коллизия существует только при побайтовом равенстве устаревшего SFT-ответа
        // и отвергнутой стороны DPO. Иначе это другая гипотеза, а не эта.
        var collisionExact = string.Equals(fixture.CurrentIndexedSource, fixture.Rejected, StringComparison.Ordinal);
        if (!collisionExact) throw new InvalidOperationException("N63C_COLLISION_NOT_EXACT");
        if (string.Equals(fixture.CurrentIndexedSource, fixture.CorrectedSource, StringComparison.Ordinal))
            throw new InvalidOperationException("N63C_TREATMENT_NOT_DISTINCT");

        // Одна соль study на оба плеча; это не RNG обучаемого.
        var saltPath = Path.Combine(HostRoot, "authority-salt.bin");
        var salt = File.ReadAllBytes(saltPath);
        if (salt.Length != 32) throw new InvalidOperationException("N63C_AUTHORITY_SALT_INVALID");

        // ── Шаг 2. Два плеча в одном процессе, порядок A→B один раз ──────────────
        var armA = RunArm("A", fixture, fixture.CurrentIndexedSource, nativeContentDigest, salt,
            outputRoot, utcStart, cancellationToken);
        var armB = RunArm("B", fixture, fixture.CorrectedSource, nativeContentDigest, salt,
            outputRoot, utcStart, cancellationToken);

        whole.Stop();
        using var process = Process.GetCurrentProcess();
        process.Refresh();

        WriteNew(Path.Combine(outputRoot, "n63c-result.json"), new
        {
            experiment = "N63-CORRECTION-COLLISION",
            factor = "Только источник текста следующего supervised-урока при побайтовом совпадении с отвергнутым ответом исправляющего DPO",
            armA_sft_response = "ошибочный текст, совпадающий с DPO.Rejected",
            armB_sft_response = "исправленный текст, совпадающий с DPO.Chosen",
            writer_state_reader =
                "StepVector и нативный SFT TrainSupervisedNextTokenStep → те же q/v-LoRA → teacher-forced оценки policy, не свободная генерация",
            order = "DPO → SFT, одинаковый в обоих плечах",
            autonomous_or_incremental_pipeline_called = false,
            public_incremental_trainer_called = false,
            fixture = new
            {
                path = FixturePath,
                sha256 = nativeContentDigest,
                status = fixture.Status,
                teacherProvenanceProvided = fixture.SourceLabels?.TeacherProvenanceProvided,
                dpoAdmissionProvided = fixture.SourceLabels?.DpoAdmissionProvided,
                synthetic = fixture.SourceLabels?.Synthetic,
                collisionExact
            },
            budget = new
            {
                dpo_step_vector_calls = armA.DpoUpdates + armB.DpoUpdates,
                sft_updates = armA.SftUpdates + armB.SftUpdates,
                total_optimizer_updates = armA.DpoUpdates + armA.SftUpdates + armB.DpoUpdates + armB.SftUpdates,
                side_score_calls = armA.SideScoreCalls + armB.SideScoreCalls,
                declared_updates = 4,
                declared_side_scores = 36,
                generate_calls = 0,
                evaluate_many_calls = 0,
                external_teacher_calls = 0,
                warmup_or_repeat = 0
            },
            arms = new[] { armA, armB },
            whole_process_peak_working_set_bytes = process.PeakWorkingSet64,
            managed_heap_bytes_end = GC.GetTotalMemory(false),
            axis_honesty =
                "Один процесс и фиксированный порядок A→B: время, процессорное время и пик процесса сохранены как наблюдения с неустановленной причинностью; пик общий, не по плечам",
            utcStart,
            utcEnd = DateTimeOffset.UtcNow,
            wholeRunSeconds = whole.Elapsed.TotalSeconds,
            limitations = new[]
            {
                "Коллизия создана явно; достижимость такой пары в нынешнем продуктовом continuous pipeline не доказана",
                "Публичный IncrementalTrainer.TrainAsync не вызывался: в fixture нет typed teacher provenance и server authority, подделка запрещена",
                "После SFT состояние — экспериментальный дамп, не допущенный производственный кандидат",
                "Свободная генерация не измерялась; Q1 — близкое переименование, Q2 — другое правило, это два задания, а не широкое понимание C#",
                "Чтение показателей — один прямой проход на сторону с той же формулой маскированного среднего log-вероятности, что в DpoTrainer.EvaluateAverageNextTokenLogProbability; потокенные поля взяты из тех же проходов и не добавляют бюджета"
            }
        });

        Console.WriteLine(
            $"N63C_OK updates={armA.DpoUpdates + armA.SftUpdates + armB.DpoUpdates + armB.SftUpdates} " +
            $"sideScores={armA.SideScoreCalls + armB.SideScoreCalls} " +
            $"A margin T1 {armA.T1.S0.Margin:F6}->{armA.T1.S1.Margin:F6}->{armA.T1.S2.Margin:F6} " +
            $"B margin T1 {armB.T1.S0.Margin:F6}->{armB.T1.S1.Margin:F6}->{armB.T1.S2.Margin:F6} " +
            $"A chosenCorrect {armA.T1.S0.ChosenCorrectTokens}->{armA.T1.S1.ChosenCorrectTokens}->{armA.T1.S2.ChosenCorrectTokens} " +
            $"B chosenCorrect {armB.T1.S0.ChosenCorrectTokens}->{armB.T1.S1.ChosenCorrectTokens}->{armB.T1.S2.ChosenCorrectTokens}");
    }

    // ── Одно плечо ───────────────────────────────────────────────────────────────
    private static ArmOutcome RunArm(
        string arm,
        CollisionFixture fixture,
        string sftResponse,
        string nativeContentDigest,
        byte[] salt,
        string outputRoot,
        DateTimeOffset utcStart,
        CancellationToken cancellationToken)
    {
        var armRoot = Path.Combine(outputRoot, arm);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(armRoot);

        using var process = Process.GetCurrentProcess();

        // Загрузка B0: отдельные независимые policy и reference из того же сохранённого корня.
        var loadWatch = Stopwatch.StartNew();
        var loadCpu0 = process.TotalProcessorTime;
        var loadAlloc0 = GC.GetTotalAllocatedBytes(precise: true);
        var models = LoadModels(cancellationToken);
        loadWatch.Stop();
        var loadCost = new PhaseCost(
            loadWatch.Elapsed.TotalMilliseconds,
            (process.TotalProcessorTime - loadCpu0).TotalMilliseconds,
            GC.GetTotalAllocatedBytes(precise: true) - loadAlloc0);

        if (HashFile(models.SourceWeightsPath) != B0WeightsSha256 ||
            ReferenceEquals(models.Policy, models.Reference) || models.LoraConfig.Dropout != 0)
            throw new InvalidOperationException("N63C_SAVED_ROOT_INVALID");
        foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;

        // ── Настоящая пара и governance только для TRAIN T1 ──────────────────────
        var governance = new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + nativeContentDigest });
        var trainPair = new DpoPair(fixture.Prompt, fixture.Chosen, fixture.Rejected, "code-documentation", utcStart)
        {
            GovernanceDecision = governance,
            ChosenAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator),
            RejectedAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator)
        };
        var rich = new[] { trainPair };
        _ = Require(TrainingDataGovernanceAdmission.Validate(rich));
        var content = Require(DpoPairTrainingContentAuthority.Project(rich));

        // Один раз приготовить exact token/mask для T1, Q1 и Q2. Обучение получит
        // только индекс 0; экзаменационные строки в TRAIN не попадают.
        var preferences = new[]
        {
            Preference(fixture.Prompt, fixture.Chosen, fixture.Rejected),
            Preference(fixture.Heldout![0].Prompt, fixture.Heldout[0].Chosen, fixture.Heldout[0].Rejected),
            Preference(fixture.Heldout[1].Prompt, fixture.Heldout[1].Chosen, fixture.Heldout[1].Rejected)
        };
        var batches = Require(LogicalDpoPreparedBatchMaterializer.Prepare(preferences, models.Tokenizer));
        if (batches.Count != 3) throw new InvalidOperationException("N63C_PREPARED_BATCH_COUNT");

        var tokenizerSha = models.Tokenizer.ComputeStateSha256();
        var labels = new[] { "T1", "Q1", "Q2" };
        var materials = new List<object>();
        for (var index = 0; index < batches.Count; index++)
        {
            var captured = DpoExactPairMaterial.CaptureAndValidate(batches[index].Batch,
                models.Policy.Config.VocabSize, models.Policy.Config.ContextLen,
                tokenizerSha, DpoExactPairMaterial.Version);
            var material = captured.ExactMaterial!;
            materials.Add(new
            {
                id = labels[index],
                material.DigestSha256,
                material.PromptLength,
                chosenResponseTokens = material.ChosenLength - material.PromptLength,
                rejectedResponseTokens = material.RejectedLength - material.PromptLength,
                material.SequenceLength
            });
        }

        // Состав получателя — реально подключённые адаптеры q/v каждого блока.
        var beforePolicy = Snapshot(models.Policy);
        var beforeReference = Snapshot(models.Reference);
        var adapters = Enumerable.Range(0, models.Policy.Blocks.Count)
            .SelectMany(block => models.LoraConfig.TargetModules
                .Select(target => (LoraLinear)models.Policy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray();
        var recipient = DpoProjectLoraRecipient.Capture(models.Policy, models.Reference, adapters, models.LoraConfig);
        var profile = DpoExecutionProfile.Capture(batches[0].Batch, recipient.Bindings);

        // ── Настоящие authority и lease; always-true валидатора нет ──────────────
        var legal = Require(LegalTrainingTeacherAdmissionAuthority.Create([], 0, salt));
        var live = true;
        var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            legal.ValidateBatch, () => live, null,
            legal.SealLogicalCapabilityReplay, legal.VerifyLogicalCapabilityReplay,
            legal.SealLogicalCapabilityExecutableManifest,
            legal.VerifyLogicalCapabilityExecutableManifest, legal.ValidateBatch);

        var runId = "N63C-" + arm;
        var sideScoreCalls = 0;
        PanelRow? t1 = null, q1 = null, q2 = null;
        object? dpoMetrics = null;
        float sftLoss = 0f;
        var dpoCost = PhaseCost.Empty;
        var sftCost = PhaseCost.Empty;
        var scoringCost = PhaseCost.Empty;
        var saveCost = PhaseCost.Empty;
        var artifacts = new List<object>();
        string? error = null;
        object? states = null;
        object? trainableComposition = null;
        object? sftArtifacts = null;

        using var lease = Require(((IServerOwnedDpoTrainingLeaseBroker)authority)
            .AcquireCodeDocumentationTrainingLease(content, runId));
        try
        {
            var mutation = Require(lease.IssueDpoTrainerMutationAdmissionCapability());

            var topology = LoraRandomStateIdentity.ComputeTopologySha256(
                models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(models.LoraConfig, models.Seeds);
            var checkpointRoot = Path.Combine(armRoot, "checkpoints");
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(checkpointRoot);

            var observer = new DpoTrainerPublicationObserver
            {
                ArtifactOwnershipProven = proof =>
                    artifacts.Add(new { proof.Step, proof.Kind, proof.Path, proof.Sha256, proof.Length }),
                BeforePolicyFinalMove = bundle =>
                {
                    var saved = Require(RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                        checkpointRoot, bundle.Step, bundle.OptimizerState, cancellationToken));
                    if (bundle.RandomState is null) throw new InvalidOperationException("N63C_RANDOM_STATE_MISSING");
                    var random = Require(RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                        Path.Combine(checkpointRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(bundle.Step)),
                        bundle.RandomState));
                    WriteNew(Path.Combine(checkpointRoot, $"state-{bundle.Step:D6}.json"), new
                    {
                        bundle.Step,
                        saved.OptimizerStateSha256,
                        saved.OptimizerStateByteLength,
                        randomStateSha256 = random.Sha256,
                        randomStateByteLength = random.ByteLength,
                        bundle.TrialWeightsSha256,
                        bundle.TrialWeightsByteLength,
                        executionProfile = profile.ToString(),
                        stage = "DPO"
                    });
                },
                PublicationFailed = () => artifacts.Add(new { effect = "RECONCILE_BEFORE_RETRY" })
            };

            var trainer = DpoTrainer.CreateForProjectLora(models.Policy, models.Reference, checkpointRoot,
                mutation, recipient, profile, null, observer,
                step => models.Policy.CaptureRandomState(topology, loraIdentity, step),
                state => models.Policy.RestoreRandomState(state, new(topology, loraIdentity)));

            // ── S0: панель до любого обновления ──────────────────────────────────
            var scoreWatch = Stopwatch.StartNew();
            var scoreCpu0 = process.TotalProcessorTime;
            var scoreAlloc0 = GC.GetTotalAllocatedBytes(precise: true);
            var s0 = ScoreAll(models.Policy, batches, ref sideScoreCalls);

            // ── Один настоящий исправляющий DPO-шаг ровно на T1 ──────────────────
            scoreWatch.Stop();
            var dpoWatch = Stopwatch.StartNew();
            var dpoCpu0 = process.TotalProcessorTime;
            var dpoAlloc0 = GC.GetTotalAllocatedBytes(precise: true);
            var timing = new DpoStepVectorTiming();
            if (trainer.ExportOptimizerState().Step != 0) throw new InvalidOperationException("N63C_DPO_STEP_NOT_ZERO");
            var metrics = trainer.StepVector(new[] { batches[0] }, 1, timing);
            dpoWatch.Stop();
            dpoCost = new PhaseCost(
                dpoWatch.Elapsed.TotalMilliseconds,
                (process.TotalProcessorTime - dpoCpu0).TotalMilliseconds,
                GC.GetTotalAllocatedBytes(precise: true) - dpoAlloc0);
            dpoMetrics = new { metrics, stepTiming = timing.Snapshot(), optimizerStep = trainer.ExportOptimizerState().Step };

            // ── S1: панель после исправляющего DPO, до SFT ───────────────────────
            scoreWatch.Start();
            var s1 = ScoreAll(models.Policy, batches, ref sideScoreCalls);
            scoreWatch.Stop();

            // До SFT: база и reference не изменились, обучаемы только q/v-LoRA.
            var afterDpoPolicy = Snapshot(models.Policy);
            var afterDpoReference = Snapshot(models.Reference);
            if (Combined(beforeReference) != Combined(afterDpoReference))
                throw new InvalidOperationException("N63C_REFERENCE_CHANGED_BY_DPO");
            if (beforePolicy.Count(x => x.Value.IsBase && x.Value.Hash != afterDpoPolicy[x.Key].Hash) != 0)
                throw new InvalidOperationException("N63C_BASE_CHANGED_BY_DPO");
            var trainableKeys = models.Policy.GetCheckpointParameterEntries()
                .Where(entry => entry.Parameter.Value.RequiresGrad)
                .Select(entry => entry.Key)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            var baseSet = models.Policy.BaseParameters.ToHashSet(ReferenceEqualityComparer.Instance);
            if (models.Policy.GetCheckpointParameterEntries()
                .Any(entry => entry.Parameter.Value.RequiresGrad && baseSet.Contains(entry.Parameter)))
                throw new InvalidOperationException("N63C_FROZEN_BASE_IS_TRAINABLE");
            trainableComposition = new { count = trainableKeys.Length, keys = trainableKeys };

            // ── Один настоящий SFT-шаг тем же нативным методом ───────────────────
            // Отдельный свежий тренер и отдельный свежий AdamW: моменты DPO не переносятся.
            var sftConfig = new TrainingConfig
            {
                BatchSize = 1,
                SeqLen = 2048,
                WarmupSteps = 0,
                TotalSteps = 1,
                MaxLr = 1e-5f,
                MinLr = 1e-5f,
                GradAccumSteps = 1,
                GradClipNorm = 1f,
                WeightDecay = 0.01f
            };
            var sftTrainer = new Trainer(sftConfig);
            var sftAdamW = new AdamW(0.9f, 0.95f, 0.01f, 1e-8f);
            var sftText = fixture.Prompt + "\n" + sftResponse;
            var sftIds = models.Tokenizer.Encode(sftText);
            if (sftIds.Count < 2 || sftIds.Count > models.Policy.Config.ContextLen + 1)
                throw new InvalidOperationException("N63C_SFT_LENGTH_OUT_OF_RANGE");
            if (sftAdamW.ExportPortableState(recipient.Bindings).Step != 0)
                throw new InvalidOperationException("N63C_SFT_OPTIMIZER_NOT_EMPTY");

            var sftWatch = Stopwatch.StartNew();
            var sftCpu0 = process.TotalProcessorTime;
            var sftAlloc0 = GC.GetTotalAllocatedBytes(precise: true);
            sftLoss = sftTrainer.TrainSupervisedNextTokenStep(models.Policy, sftAdamW, sftIds, 1f);
            sftWatch.Stop();
            sftCost = new PhaseCost(
                sftWatch.Elapsed.TotalMilliseconds,
                (process.TotalProcessorTime - sftCpu0).TotalMilliseconds,
                GC.GetTotalAllocatedBytes(precise: true) - sftAlloc0);

            // ── S2: панель после конфликтующего SFT ──────────────────────────────
            scoreWatch.Start();
            var s2 = ScoreAll(models.Policy, batches, ref sideScoreCalls);
            scoreWatch.Stop();
            scoringCost = new PhaseCost(
                scoreWatch.Elapsed.TotalMilliseconds,
                (process.TotalProcessorTime - scoreCpu0).TotalMilliseconds - dpoCost.CpuMs - sftCost.CpuMs,
                GC.GetTotalAllocatedBytes(precise: true) - scoreAlloc0 - dpoCost.AllocatedBytes - sftCost.AllocatedBytes);

            t1 = new PanelRow("T1", s0[0], s1[0], s2[0]);
            q1 = new PanelRow("Q1", s0[1], s1[1], s2[1]);
            q2 = new PanelRow("Q2", s0[2], s1[2], s2[2]);

            // ── Сохранить S2 отдельно; нативный DPO-чекпойнт не затирается ───────
            var saveWatch = Stopwatch.StartNew();
            var saveCpu0 = process.TotalProcessorTime;
            var saveAlloc0 = GC.GetTotalAllocatedBytes(precise: true);
            var sftWeightsPath = Path.Combine(armRoot, "after-sft.bin");
            models.Policy.SaveCheckpoint(sftWeightsPath);
            var sftOptimizerBytes = sftAdamW.ExportPortableState(recipient.Bindings).ToBytes();
            var sftOptimizerPath = Path.Combine(armRoot, "after-sft.optimizer.bin");
            using (var stream = new FileStream(sftOptimizerPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                stream.Write(sftOptimizerBytes, 0, sftOptimizerBytes.Length);
            var sftRandom = Require(RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                Path.Combine(armRoot, "after-sft.rng.json"),
                models.Policy.CaptureRandomState(topology, loraIdentity, 1)));
            saveWatch.Stop();
            saveCost = new PhaseCost(
                saveWatch.Elapsed.TotalMilliseconds,
                (process.TotalProcessorTime - saveCpu0).TotalMilliseconds,
                GC.GetTotalAllocatedBytes(precise: true) - saveAlloc0);

            var afterSftPolicy = Snapshot(models.Policy);
            var afterSftReference = Snapshot(models.Reference);
            sftArtifacts = new
            {
                stage = "SFT",
                weightsPath = sftWeightsPath,
                weightsSha256 = HashFile(sftWeightsPath),
                weightsByteLength = new FileInfo(sftWeightsPath).Length,
                optimizerPath = sftOptimizerPath,
                optimizerSha256 = Sha(sftOptimizerBytes),
                optimizerByteLength = sftOptimizerBytes.LongLength,
                optimizerStep = sftAdamW.ExportPortableState(recipient.Bindings).Step,
                randomStateSha256 = sftRandom.Sha256,
                randomStateByteLength = sftRandom.ByteLength,
                note = "Отдельный оптимизатор SFT со своим шагом 1; нативный глобальный шаг DPO остаётся 1 и не суммируется",
                dpoGlobalStepAfterSft = trainer.ExportOptimizerState().Step,
                admittedProductionCandidate = false
            };

            states = new
            {
                policy_b0_sha256 = Combined(beforePolicy),
                policy_after_dpo_sha256 = Combined(afterDpoPolicy),
                policy_after_sft_sha256 = Combined(afterSftPolicy),
                reference_b0_sha256 = Combined(beforeReference),
                reference_after_sft_sha256 = Combined(afterSftReference),
                reference_unchanged_whole_path = Combined(beforeReference) == Combined(afterSftReference),
                base_parameter_arrays = beforePolicy.Count(x => x.Value.IsBase),
                adapter_parameter_arrays = beforePolicy.Count(x => !x.Value.IsBase),
                changed_adapter_arrays_by_dpo =
                    beforePolicy.Count(x => !x.Value.IsBase && x.Value.Hash != afterDpoPolicy[x.Key].Hash),
                changed_adapter_arrays_by_sft =
                    afterDpoPolicy.Count(x => !x.Value.IsBase && x.Value.Hash != afterSftPolicy[x.Key].Hash),
                changed_base_arrays_whole_path =
                    beforePolicy.Count(x => x.Value.IsBase && x.Value.Hash != afterSftPolicy[x.Key].Hash),
                base_bitwise_frozen =
                    beforePolicy.Count(x => x.Value.IsBase && x.Value.Hash != afterSftPolicy[x.Key].Hash) == 0,
                model_layers = models.Policy.Config.NumLayers,
                source_weights_sha256_after = HashFile(models.SourceWeightsPath)
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

        var outcome = new ArmOutcome(
            arm,
            sftResponse,
            string.Equals(sftResponse, fixture.Rejected, StringComparison.Ordinal),
            error is null ? "COMPLETED" : "FAILED_PRESERVE_EVIDENCE",
            error,
            error is null ? 1 : 0,
            error is null ? 1 : 0,
            sideScoreCalls,
            sftLoss,
            t1 ?? PanelRow.Missing("T1"),
            q1 ?? PanelRow.Missing("Q1"),
            q2 ?? PanelRow.Missing("Q2"),
            loadCost,
            dpoCost,
            sftCost,
            scoringCost,
            saveCost,
            dpoMetrics,
            sftArtifacts,
            trainableComposition,
            states,
            materials.ToArray(),
            artifacts.ToArray());

        WriteNew(Path.Combine(armRoot, "arm-result.json"), outcome);
        if (error is not null) throw new InvalidOperationException("N63C_ARM_FAILED:" + error);
        return outcome;
    }

    private static LogicalDpoPreferenceExample Preference(string prompt, string chosen, string rejected) =>
        new(prompt, chosen, rejected)
        {
            ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator,
            RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator
        };

    // ── Чтение показателей ───────────────────────────────────────────────────────
    // Один прямой проход на сторону. Маскированное среднее log-вероятности считается
    // той же формулой, что в DpoTrainer.EvaluateAverageNextTokenLogProbability (policy,
    // training:false, только response targets). Потокенные поля C/T/E берутся из тех
    // же логитов, поэтому бюджет остаётся ровно 36 проходов.
    private static StateScore[] ScoreAll(
        LlmModel policy, IReadOnlyList<DpoVectorBatch> batches, ref int sideScoreCalls)
    {
        var scores = new StateScore[batches.Count];
        for (var index = 0; index < batches.Count; index++)
        {
            var batch = batches[index].Batch;
            var chosenMask = batch.ChosenResponseMask ?? batch.ChosenMask;
            var rejectedMask = batch.RejectedResponseMask ?? batch.RejectedMask;
            var chosen = SideScore(policy, batch.ChosenIds, chosenMask);
            sideScoreCalls++;
            var rejected = SideScore(policy, batch.RejectedIds, rejectedMask);
            sideScoreCalls++;
            scores[index] = new StateScore(
                chosen.MeanLogProbability,
                rejected.MeanLogProbability,
                -chosen.MeanLogProbability,
                chosen.MeanLogProbability - rejected.MeanLogProbability,
                chosen.CorrectTargets,
                chosen.TargetCount,
                chosen.TargetCount > 0 && chosen.CorrectTargets == chosen.TargetCount,
                chosen.MeanGoldRank);
        }

        return scores;
    }

    private static SideReadout SideScore(LlmModel model, Tensor ids, Tensor mask)
    {
        if (ids.Shape.Rank != 2 || mask.Shape.Rank != 2 ||
            ids.Shape.Dimensions[0] != 1 || mask.Shape.Dimensions[0] != 1 ||
            ids.Shape.Dimensions[1] != mask.Shape.Dimensions[1])
            throw new ArgumentException("N63C_SCORE_SHAPE");

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

            var logP = logits.Data[i * v + targetId] - maxValue - logSumExp;
            sum += logP * maskValue;
            count += maskValue;

            targets++;
            if (best == targetId) correct++;
            var goldLogit = logits.Data[i * v + targetId];
            var above = 0;
            for (var j = 0; j < v; j++) if (logits.Data[i * v + j] > goldLogit) above++;
            rankSum += above + 1;
        }

        if (targets == 0) throw new InvalidOperationException("N63C_NO_RESPONSE_TARGETS");
        return new SideReadout(count > 0f ? sum / count : 0f, correct, targets, (double)rankSum / targets);
    }

    // ── Узкая композиция канонического загрузчика B0 ─────────────────────────────
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
                throw new InvalidOperationException("N63C_SAVED_ROOT_IDENTITY_MISMATCH");
            return loaded.Value;
        }

        var policy = Load();
        var reference = Load();
        var weights = Path.GetFullPath(Path.Combine(RepositoryRoot,
            policy.ParentManifest!.RepositoryRelativeWeightsPath));
        if (HashFile(weights) != B0WeightsSha256)
            throw new InvalidOperationException("N63C_SAVED_B0_CHANGED");
        return new(policy.Model, reference.Model, options.Tokenizer, options.StartRequest.LoraConfig!,
            options.StartRequest.AdapterSeedAuthority!, weights, B0WeightsSha256,
            token => { token.ThrowIfCancellationRequested(); return Load().Model; });
    }

    // ── Мелкие помощники ────────────────────────────────────────────────────────
    private sealed record ParameterState(string Hash, bool IsBase);

    private static Dictionary<string, ParameterState> Snapshot(LlmModel model)
    {
        var bases = model.BaseParameters.ToHashSet(ReferenceEqualityComparer.Instance);
        return model.GetCheckpointParameterEntries().ToDictionary(
            entry => entry.Key,
            entry => new ParameterState(
                Sha(System.Runtime.InteropServices.MemoryMarshal
                    .AsBytes(entry.Parameter.Value.Data.AsSpan()).ToArray()),
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

    private static void RequireLiteral(string? actual, string expected, string field)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"N63C_FIXTURE_LITERAL_MISMATCH:{field}");
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

    // ── Записи результата ───────────────────────────────────────────────────────
    private sealed record SideReadout(float MeanLogProbability, int CorrectTargets, int TargetCount, double MeanGoldRank);

    public sealed record StateScore(
        float ChosenMeanLogProbability,
        float RejectedMeanLogProbability,
        float ChosenNll,
        float Margin,
        int ChosenCorrectTokens,
        int ChosenTargetDenominator,
        bool TeacherForcedResponseExact,
        double ChosenMeanGoldRank)
    {
        public static StateScore Missing => new(0f, 0f, 0f, 0f, 0, 0, false, 0d);
    }

    public sealed record PanelRow(string Id, StateScore S0, StateScore S1, StateScore S2)
    {
        public static PanelRow Missing(string id) =>
            new(id, StateScore.Missing, StateScore.Missing, StateScore.Missing);
    }

    public sealed record PhaseCost(double WallMs, double CpuMs, long AllocatedBytes)
    {
        public static PhaseCost Empty => new(0d, 0d, 0L);
    }

    public sealed record ArmOutcome(
        string Arm,
        string SftResponse,
        bool SftResponseEqualsDpoRejected,
        string Status,
        string? Error,
        int DpoUpdates,
        int SftUpdates,
        int SideScoreCalls,
        float SftLossBeforeItsUpdate,
        PanelRow T1,
        PanelRow Q1,
        PanelRow Q2,
        PhaseCost LoadCost,
        PhaseCost DpoCost,
        PhaseCost SftCost,
        PhaseCost ScoringCost,
        PhaseCost SaveCost,
        object? DpoMetrics,
        object? SftArtifacts,
        object? TrainableComposition,
        object? States,
        object[] Materials,
        object[] DpoArtifacts);

    private sealed record CollisionFixture(
        string? Status,
        string? Domain,
        string Prompt,
        string Chosen,
        string Rejected,
        string CurrentIndexedSource,
        string CorrectedSource,
        string? Treatment,
        CollisionSourceLabels? SourceLabels,
        CollisionHeldout[]? Heldout);

    private sealed record CollisionSourceLabels(
        string? ChosenOrigin,
        string? RejectedOrigin,
        bool Synthetic,
        bool TeacherProvenanceProvided,
        bool DpoAdmissionProvided);

    private sealed record CollisionHeldout(string Id, string Prompt, string Chosen, string Rejected);
}
