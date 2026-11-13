// Диагностический инструмент к опыту N63-CORRECTION-COLLISION.
// Это НЕ опыт очереди, не повтор и не замена карточки: карточка ограничена четырьмя
// обновлениями, и на таком объёме у модели ноль правильных токенов ответа во всех
// состояниях. По правилу Любы «нет удачного обучения ни до, ни после — тест невалиден».
//
// Инструмент сначала делает «до» действительно обученным, и лишь затем проверяет
// коллизию двух учебных целей:
//   1. законная authority приобретается настоящим нативным SFT на ПРАВИЛЬНОМ тексте,
//      пока модель не начнёт воспроизводить его при принудительной подаче префикса;
//   2. состояние сохраняется настоящими механизмами;
//   3. оба плеча восстанавливаются из одного обученного состояния и получают
//      по одному настоящему исправляющему DPO-шагу на T1;
//   4. затем каждое плечо получает ОДИНАКОВОЕ число настоящих SFT-шагов:
//        A — устаревшим ошибочным текстом, побайтово равным отвергнутой стороне DPO;
//        B — исправленным текстом.
// Измеряем, сколько приобретённого поведения выживает. Плечо B переучивает тот же
// текст, который и был приобретён, поэтому его сохранность ожидаема почти тривиально;
// измеряемая величина — насколько сильно разрушает плечо A.
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
using Tokenizer;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;

namespace ContinuousLearning;

public static class N63CollisionProbe
{
    private const string RepositoryRoot = "C:/Projects/llm_mvp_core";
    private const string ConfigSha256 = "e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40";
    private const string B0WeightsSha256 = "a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911";
    private const string B0ManifestSha256 = "c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d";
    private const string HostRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C";

    private const string FixturePath =
        @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\replanning\possibleDifs\2026-10-20-learning-ingress-and-retention-review\ingress\fixtures\correction-collision.json";

    // Отличия инструмента от карточки, записанные явно:
    // скорость обучения приобретающей фазы выше карточной 1e-5, иначе усвоение
    // недостижимо за разумное время; число шагов задано здесь, а не карточкой.
    // Бюджет выбран так, чтобы весь инструмент укладывался примерно в две минуты:
    // длинные фоновые процессы в этой среде обрываются, и тогда доказательств нет вовсе.
    private const float AcquireLearningRate = 3e-3f;
    private const int MaxAcquireUpdates = 60;
    private const int AcquireCheckEvery = 10;
    private const int ArmSftUpdates = 20;
    private const float ArmSftLearningRate = 3e-3f;

    // Ход работы пишется на диск сразу после каждого шага: без этого у внешнего
    // наблюдателя нет признака продвижения, потому что оболочка буферизует вывод.
    private static string? _beatPath;

    private static void Beat(string message)
    {
        if (_beatPath is null) return;
        var line = $"{DateTimeOffset.Now:HH:mm:ss} {message}{Environment.NewLine}";
        using var stream = new FileStream(_beatPath, FileMode.Append, FileAccess.Write, FileShare.Read);
        var bytes = System.Text.Encoding.UTF8.GetBytes(line);
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush(flushToDisk: true);
    }

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
        if (Directory.Exists(outputRoot) || File.Exists(outputRoot))
            throw new InvalidOperationException("N63C_PROBE_RESULT_EXISTS");
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(outputRoot);
        _beatPath = Path.Combine(outputRoot, "progress.log");
        Beat("probe started");

        var token = CancellationToken.None;
        var whole = Stopwatch.StartNew();
        using var process = Process.GetCurrentProcess();

        var fixtureBytes = File.ReadAllBytes(FixturePath);
        var digest = Sha(fixtureBytes);
        var fixture = JsonSerializer.Deserialize<ProbeFixture>(fixtureBytes, ReadJson)
            ?? throw new InvalidOperationException("N63C_PROBE_FIXTURE_UNREADABLE");
        if (!string.Equals(fixture.CurrentIndexedSource, fixture.Rejected, StringComparison.Ordinal))
            throw new InvalidOperationException("N63C_PROBE_COLLISION_NOT_EXACT");

        var salt = File.ReadAllBytes(Path.Combine(HostRoot, "authority-salt.bin"));
        if (salt.Length != 32) throw new InvalidOperationException("N63C_PROBE_SALT_INVALID");

        // ── Приобретение законной authority настоящим нативным SFT ───────────────
        Beat("loading canonical B0 for the acquisition phase");
        var shared = LoadModels(token);
        foreach (var parameter in shared.Reference.Parameters()) parameter.Value.RequiresGrad = false;
        var batches = PrepareBatches(fixture, shared.Tokenizer);
        Beat("B0 loaded, batches prepared");

        var forwards = 0;
        var panelW0 = Panel(shared.Policy, batches, ref forwards);
        Beat($"W0 panel done: T1 correct={panelW0[0].ChosenCorrectTokens}/{panelW0[0].ChosenTargetDenominator}");

        var acquireTrainer = new Trainer(new TrainingConfig
        {
            BatchSize = 1, SeqLen = 2048, WarmupSteps = 0, TotalSteps = MaxAcquireUpdates,
            MaxLr = AcquireLearningRate, MinLr = AcquireLearningRate,
            GradAccumSteps = 1, GradClipNorm = 1f, WeightDecay = 0.01f
        });
        var acquireOptimizer = new AdamW(0.9f, 0.95f, 0.01f, 1e-8f);
        var correctIds = shared.Tokenizer.Encode(fixture.Prompt + "\n" + fixture.Chosen);
        var acquireLosses = new List<float>();
        var acquireProgress = new List<object>();
        var acquireUsed = 0;

        var acquireWatch = Stopwatch.StartNew();
        for (var i = 0; i < MaxAcquireUpdates; i++)
        {
            acquireLosses.Add(acquireTrainer.TrainSupervisedNextTokenStep(
                shared.Policy, acquireOptimizer, correctIds, 1f));
            acquireUsed++;
            if ((i + 1) % AcquireCheckEvery != 0) continue;

            // Ограниченная остановка по объявленному критерию усвоения.
            var probe = SideScore(shared.Policy, batches[0].Batch.ChosenIds,
                batches[0].Batch.ChosenResponseMask ?? batches[0].Batch.ChosenMask);
            forwards++;
            acquireProgress.Add(new
            {
                update = i + 1,
                loss = acquireLosses[^1],
                chosenCorrectTokens = probe.CorrectTargets,
                denominator = probe.TargetCount,
                exact = probe.CorrectTargets == probe.TargetCount
            });
            Beat($"acquire update {i + 1}: loss={acquireLosses[^1]:F6} correct={probe.CorrectTargets}/{probe.TargetCount}");
            if (probe.CorrectTargets == probe.TargetCount) break;
        }

        acquireWatch.Stop();
        Beat($"acquisition finished after {acquireUsed} updates");
        var authorityPath = Path.Combine(outputRoot, "authority-acquired.bin");
        shared.Policy.SaveCheckpoint(authorityPath);
        var panelAuthority = Panel(shared.Policy, batches, ref forwards);
        Beat($"W_auth panel done: T1 correct={panelAuthority[0].ChosenCorrectTokens}/{panelAuthority[0].ChosenTargetDenominator} exact={panelAuthority[0].TeacherForcedResponseExact}");

        var acquired = panelAuthority[0].ChosenCorrectTokens;
        var acquiredExact = panelAuthority[0].TeacherForcedResponseExact;

        // ── Два плеча из одного обученного состояния ─────────────────────────────
        var armA = RunArm("A_stale_rejected_text", fixture, fixture.CurrentIndexedSource,
            authorityPath, digest, salt, outputRoot, ref forwards, token);
        var armB = RunArm("B_corrected_text", fixture, fixture.CorrectedSource,
            authorityPath, digest, salt, outputRoot, ref forwards, token);

        whole.Stop();
        process.Refresh();
        Beat("both arms done, writing result");

        WriteNew(Path.Combine(outputRoot, "n63c-probe.json"), new
        {
            instrument = "N63C-PROBE",
            purpose = "Измерить ту же коллизию на действительно обученном «до»",
            notACardExperiment =
                "Карточка N63-CORRECTION-COLLISION не менялась и не повторялась; её частичный результат сохранён отдельно",
            whyNeeded =
                "В карточке 4 обновления; на таком объёме правильных токенов ответа 0 во всех состояниях, поэтому вердикт об обучении недействителен",
            declaredDeviationsFromCard = new[]
            {
                $"скорость обучения приобретающей фазы {AcquireLearningRate} вместо карточной 1e-5",
                $"до {MaxAcquireUpdates} SFT-обновлений приобретения с ограниченной остановкой по усвоению; фактически {acquireUsed}",
                $"по {ArmSftUpdates} SFT-обновлений на плечо вместо одного, иначе эффект коллизии не наблюдаем",
                "добавлено состояние W_auth между необученной базой и исправляющим DPO"
            },
            fixture = new { path = FixturePath, sha256 = digest, collisionExact = true },
            acquisition = new
            {
                updatesUsed = acquireUsed,
                firstLoss = acquireLosses[0],
                lastLoss = acquireLosses[^1],
                wallMs = acquireWatch.Elapsed.TotalMilliseconds,
                progress = acquireProgress,
                chosenCorrectTokensAtAuthority = acquired,
                denominator = panelAuthority[0].ChosenTargetDenominator,
                teacherForcedResponseExact = acquiredExact,
                verdict = acquired > 0
                    ? "ACQUIRED: удачное обучение до коллизии измерено"
                    : "NOT_ACQUIRED: инструмент не достиг обученного «до», вывод о разрушении делать нельзя"
            },
            panels = new
            {
                w0_untrained_base = Row(panelW0),
                w_auth_acquired = Row(panelAuthority),
                armA_after_dpo = Row(armA.AfterDpo),
                armA_after_sft = Row(armA.AfterSft),
                armB_after_dpo = Row(armB.AfterDpo),
                armB_after_sft = Row(armB.AfterSft)
            },
            retention = new
            {
                acquiredChosenCorrectTokens = acquired,
                armA_afterDpo = armA.AfterDpo[0].ChosenCorrectTokens,
                armA_afterSft = armA.AfterSft[0].ChosenCorrectTokens,
                armB_afterDpo = armB.AfterDpo[0].ChosenCorrectTokens,
                armB_afterSft = armB.AfterSft[0].ChosenCorrectTokens,
                armA_marginPath = new[] { panelAuthority[0].Margin, armA.AfterDpo[0].Margin, armA.AfterSft[0].Margin },
                armB_marginPath = new[] { panelAuthority[0].Margin, armB.AfterDpo[0].Margin, armB.AfterSft[0].Margin },
                armA_chosenNllPath = new[] { panelAuthority[0].ChosenNll, armA.AfterDpo[0].ChosenNll, armA.AfterSft[0].ChosenNll },
                armB_chosenNllPath = new[] { panelAuthority[0].ChosenNll, armB.AfterDpo[0].ChosenNll, armB.AfterSft[0].ChosenNll }
            },
            arms = new[] { armA, armB },
            budget = new
            {
                scoringForwards = forwards,
                dpoUpdates = 2,
                armSftUpdates = ArmSftUpdates * 2,
                acquireSftUpdates = acquireUsed,
                generateCalls = 0,
                externalTeacherCalls = 0
            },
            wholeProcessPeakWorkingSetBytes = process.PeakWorkingSet64,
            wholeRunSeconds = whole.Elapsed.TotalSeconds,
            limitations = new[]
            {
                "Диагностический инструмент; продуктовый код не изменён и ничего не принято в каноническое дерево",
                "Плечо B переучивает тот же текст, который был приобретён, поэтому его сохранность ожидаема почти тривиально",
                "Скорость обучения и число шагов отличаются от карточки и перечислены выше",
                "Публичный IncrementalTrainer не вызывался; паспорт учителя не подделывался",
                "Свободная генерация не измерялась: все показатели teacher-forced"
            }
        });

        Console.WriteLine(
            $"PROBE_OK acquireUpdates={acquireUsed} acquiredCorrect={acquired}/{panelAuthority[0].ChosenTargetDenominator} exact={acquiredExact} " +
            $"A {acquired}->{armA.AfterDpo[0].ChosenCorrectTokens}->{armA.AfterSft[0].ChosenCorrectTokens} " +
            $"B {acquired}->{armB.AfterDpo[0].ChosenCorrectTokens}->{armB.AfterSft[0].ChosenCorrectTokens} " +
            $"forwards={forwards}");
    }

    private static ProbeArm RunArm(
        string arm, ProbeFixture fixture, string sftResponse, string authorityPath,
        string digest, byte[] salt, string outputRoot, ref int forwards, CancellationToken token)
    {
        var armRoot = Path.Combine(outputRoot, arm);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(armRoot);
        using var process = Process.GetCurrentProcess();
        Beat($"arm {arm}: loading canonical B0 and restoring the acquired state");

        // Свежая каноническая загрузка; reference остаётся замороженной базой B0,
        // policy восстанавливается из одного приобретённого состояния.
        var models = LoadModels(token);
        foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false;
        models.Policy.LoadCheckpoint(authorityPath);
        var batches = PrepareBatches(fixture, models.Tokenizer);
        Beat($"arm {arm}: acquired state restored");

        var governance = new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + digest });
        var pair = new DpoPair(fixture.Prompt, fixture.Chosen, fixture.Rejected, "code-documentation",
            DateTimeOffset.UtcNow)
        {
            GovernanceDecision = governance,
            ChosenAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator),
            RejectedAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator)
        };
        var rich = new[] { pair };
        _ = Require(TrainingDataGovernanceAdmission.Validate(rich));
        var content = Require(DpoPairTrainingContentAuthority.Project(rich));

        var adapters = Enumerable.Range(0, models.Policy.Blocks.Count)
            .SelectMany(block => models.LoraConfig.TargetModules
                .Select(target => (LoraLinear)models.Policy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray();
        var recipient = DpoProjectLoraRecipient.Capture(models.Policy, models.Reference, adapters, models.LoraConfig);
        var profile = DpoExecutionProfile.Capture(batches[0].Batch, recipient.Bindings);

        var legal = Require(LegalTrainingTeacherAdmissionAuthority.Create([], 0, salt));
        var live = true;
        var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            legal.ValidateBatch, () => live, null,
            legal.SealLogicalCapabilityReplay, legal.VerifyLogicalCapabilityReplay,
            legal.SealLogicalCapabilityExecutableManifest,
            legal.VerifyLogicalCapabilityExecutableManifest, legal.ValidateBatch);

        StateScore[] afterDpo;
        StateScore[] afterSft;
        object dpoMetrics;
        float sftFirstLoss, sftLastLoss;
        double dpoWallMs, sftWallMs;
        long dpoAlloc, sftAlloc;

        using var lease = Require(((IServerOwnedDpoTrainingLeaseBroker)authority)
            .AcquireCodeDocumentationTrainingLease(content, "N63C-PROBE-" + arm));
        try
        {
            var mutation = Require(lease.IssueDpoTrainerMutationAdmissionCapability());
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(
                models.Policy.Config, models.LoraConfig, models.Seeds);
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
                models.LoraConfig, models.Seeds);
            var checkpointRoot = Path.Combine(armRoot, "checkpoints");
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(checkpointRoot);

            // Наблюдатель использует нативное каноническое имя файла состояния ГСЧ.
            var observer = new DpoTrainerPublicationObserver
            {
                BeforePolicyFinalMove = bundle =>
                {
                    _ = Require(RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                        checkpointRoot, bundle.Step, bundle.OptimizerState, token));
                    if (bundle.RandomState is null)
                        throw new InvalidOperationException("N63C_PROBE_RANDOM_STATE_MISSING");
                    _ = Require(RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                        Path.Combine(checkpointRoot,
                            RepositoryOwnedDpoRandomStateFile.BuildFileName(bundle.Step)),
                        bundle.RandomState));
                }
            };

            var trainer = DpoTrainer.CreateForProjectLora(models.Policy, models.Reference, checkpointRoot,
                mutation, recipient, profile, null, observer,
                step => models.Policy.CaptureRandomState(topology, loraIdentity, step),
                state => models.Policy.RestoreRandomState(state, new(topology, loraIdentity)));

            // Один настоящий исправляющий DPO-шаг на T1.
            var dpoWatch = Stopwatch.StartNew();
            var dpoAlloc0 = GC.GetTotalAllocatedBytes(precise: true);
            var timing = new DpoStepVectorTiming();
            var metrics = trainer.StepVector(new[] { batches[0] }, 1, timing);
            dpoWatch.Stop();
            dpoWallMs = dpoWatch.Elapsed.TotalMilliseconds;
            dpoAlloc = GC.GetTotalAllocatedBytes(precise: true) - dpoAlloc0;
            dpoMetrics = new { metrics, optimizerStep = trainer.ExportOptimizerState().Step };
            afterDpo = Panel(models.Policy, batches, ref forwards);
            Beat($"arm {arm}: corrective DPO step done, T1 correct={afterDpo[0].ChosenCorrectTokens}/{afterDpo[0].ChosenTargetDenominator}");

            // Затем одинаковое число настоящих SFT-шагов текстом своего плеча.
            var sftTrainer = new Trainer(new TrainingConfig
            {
                BatchSize = 1, SeqLen = 2048, WarmupSteps = 0, TotalSteps = ArmSftUpdates,
                MaxLr = ArmSftLearningRate, MinLr = ArmSftLearningRate,
                GradAccumSteps = 1, GradClipNorm = 1f, WeightDecay = 0.01f
            });
            var sftOptimizer = new AdamW(0.9f, 0.95f, 0.01f, 1e-8f);
            var sftIds = models.Tokenizer.Encode(fixture.Prompt + "\n" + sftResponse);
            var losses = new List<float>();
            var sftWatch = Stopwatch.StartNew();
            var sftAlloc0 = GC.GetTotalAllocatedBytes(precise: true);
            for (var i = 0; i < ArmSftUpdates; i++)
            {
                losses.Add(sftTrainer.TrainSupervisedNextTokenStep(models.Policy, sftOptimizer, sftIds, 1f));
                if ((i + 1) % 5 == 0) Beat($"arm {arm}: sft update {i + 1}/{ArmSftUpdates} loss={losses[^1]:F6}");
            }

            sftWatch.Stop();
            sftWallMs = sftWatch.Elapsed.TotalMilliseconds;
            sftAlloc = GC.GetTotalAllocatedBytes(precise: true) - sftAlloc0;
            sftFirstLoss = losses[0];
            sftLastLoss = losses[^1];
            afterSft = Panel(models.Policy, batches, ref forwards);
            Beat($"arm {arm}: colliding SFT done, T1 correct={afterSft[0].ChosenCorrectTokens}/{afterSft[0].ChosenTargetDenominator}");

            models.Policy.SaveCheckpoint(Path.Combine(armRoot, "after-sft.bin"));
        }
        finally
        {
            live = false;
        }

        return new ProbeArm(arm, sftResponse,
            string.Equals(sftResponse, fixture.Rejected, StringComparison.Ordinal),
            dpoMetrics, sftFirstLoss, sftLastLoss, ArmSftUpdates,
            dpoWallMs, sftWallMs, dpoAlloc, sftAlloc, afterDpo, afterSft);
    }

    private static IReadOnlyList<DpoVectorBatch> PrepareBatches(ProbeFixture fixture, BpeTokenizer tokenizer)
    {
        var preferences = new[]
        {
            Preference(fixture.Prompt, fixture.Chosen, fixture.Rejected),
            Preference(fixture.Heldout![0].Prompt, fixture.Heldout[0].Chosen, fixture.Heldout[0].Rejected),
            Preference(fixture.Heldout[1].Prompt, fixture.Heldout[1].Chosen, fixture.Heldout[1].Rejected)
        };
        return Require(LogicalDpoPreparedBatchMaterializer.Prepare(preferences, tokenizer));
    }

    private static LogicalDpoPreferenceExample Preference(string prompt, string chosen, string rejected) =>
        new(prompt, chosen, rejected)
        {
            ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator,
            RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator
        };

    private static object Row(StateScore[] panel) => new
    {
        t1 = panel[0],
        q1 = panel[1],
        q2 = panel[2]
    };

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

        if (targets == 0) throw new InvalidOperationException("N63C_PROBE_NO_RESPONSE_TARGETS");
        return new SideReadout(count > 0f ? sum / count : 0f, correct, targets, (double)rankSum / targets);
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
                throw new InvalidOperationException("N63C_PROBE_SAVED_ROOT_IDENTITY_MISMATCH");
            return loaded.Value;
        }

        var policy = Load();
        var reference = Load();
        var weights = Path.GetFullPath(Path.Combine(RepositoryRoot,
            policy.ParentManifest!.RepositoryRelativeWeightsPath));
        return new(policy.Model, reference.Model, options.Tokenizer, options.StartRequest.LoraConfig!,
            options.StartRequest.AdapterSeedAuthority!, weights, B0WeightsSha256,
            t => { t.ThrowIfCancellationRequested(); return Load().Model; });
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

    public sealed record ProbeArm(
        string Arm,
        string SftResponse,
        bool SftResponseEqualsDpoRejected,
        object DpoMetrics,
        float SftFirstLoss,
        float SftLastLoss,
        int SftUpdates,
        double DpoWallMs,
        double SftWallMs,
        long DpoAllocatedBytes,
        long SftAllocatedBytes,
        StateScore[] AfterDpo,
        StateScore[] AfterSft);

    private sealed record ProbeFixture(
        string Prompt,
        string Chosen,
        string Rejected,
        string CurrentIndexedSource,
        string CorrectedSource,
        ProbeHeldout[]? Heldout);

    private sealed record ProbeHeldout(string Id, string Prompt, string Chosen, string Rejected);
}
