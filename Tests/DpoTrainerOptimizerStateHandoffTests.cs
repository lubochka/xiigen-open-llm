// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using AgentInfrastructure;
using System.Security.Cryptography;
using System.Text.Json;
using TrainableRouter.Training;
using DPO;
using DPO.Models;
using AgentInfrastructure.Logical;
using LoRA;
using TensorCore.Core;
using TensorCore.Layers;
using TrainingLoop.Optimizers;
using LlmFromScratch.Tests;
using TransformerModel;
using TransformerModel.Config;
using Xunit;

// Связка обучения и хранения: тренер отдаёт память своего оптимизатора, она
// ложится в каталог попытки отдельным файлом и открывается обратно в новом
// тренере. Это доказывает передачу состояния между обучением и диском, а не
// качество обучения модели.
//
// Работа R2 плана ремонта 98 (находка A29-02 аудита 97). Две проверки
// продолжения с диска раньше собирали описание состояния вручную записью
// прежней четвёртой версии и падали на самом открытии этого описания, не
// доходя ни до продолжения обучения, ни до сравнения с непрерывным путём.
// Теперь обе работают на настоящем точном учебном пакете (запрос «a»,
// предпочтённый «b», отклонённый «cc», побайтовый разборщик со словарём 256),
// на установленном проектном получателе с адаптерами q_proj, и описание
// состояния пишет и читает обратно его штатный владелец с действительными
// артефактом и профилем. Проверка качества описания в продукте не ослаблена.
//
// Чего эти две проверки НЕ доказывают: они не измеряют качество обучения, не
// проверяют совместимость с прежней четвёртой версией описания (для неё нужен
// отдельный договор и отдельная проверка) и не заявляют готовность продукта.
public sealed class DpoTrainerOptimizerStateHandoffTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void ExactRecipient_RejectsActiveBaseBeforeTrainerPermit()
    {
        var (policy, reference, adapters, lora, batch) = ExactRecipientFixture();
        policy.BaseParameters[0].Value.RequiresGrad = true;
        var before = SaveCheckpoint(policy);
        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora));
        Assert.Equal(DpoProjectLoraRecipient.InvalidErrorCode, exception.ErrorCode);
        Assert.Equal(before, SaveCheckpoint(policy));
    }

    [Fact]
    public void ExactRecipient_UpdatesOnlyInstalledAdaptersAndExportsActualProfile()
    {
        var (policy, reference, adapters, lora, batch) = ExactRecipientFixture();
        var root = CreateTemporaryDirectory("dpo-exact-recipient-");
        try
        {
            var recipient = DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora);
            var profile = DpoExecutionProfile.Capture(batch, recipient.Bindings);
            DpoTrainingPublicationBundle? bundle = null;
            var failPublication = false;
            var trainer = DpoTrainer.CreateForProjectLora(policy, reference, root,
                DpoMutationAdmissionCapability.Issue(new TestDpoMutationAdmissionLease()), recipient, profile, null,
                new DpoTrainerPublicationObserver { BeforePolicyFinalMove = b =>
                { bundle = b; if (failPublication) throw new IOException("synthetic publication failure"); } }, null, null);
            var baseBefore = policy.BaseParameters.Select(p => p.Value.Data.ToArray()).ToArray();
            var referenceBefore = SaveCheckpoint(reference);
            var adaptersBefore = recipient.Bindings.ToDictionary(x => x.Key, x => x.Parameter.Value.Data.ToArray());
            trainer.Step(batch, 1);
            Assert.NotNull(bundle);
            Assert.Same(profile, bundle!.ExecutionProfile);
            Assert.Equal(recipient.Bindings.Select(x => x.Key), profile.AllowedParameterMap.Select(x => x.Key));
            Assert.Equal(recipient.Bindings.Select(x => x.Parameter.Value.Data.Length), profile.AllowedParameterMap.Select(x => x.ElementCount));
            Assert.All(trainer.ExportOptimizerState().Entries, x => Assert.Contains(x.Key, adaptersBefore.Keys));
            Assert.Contains(recipient.Bindings, x => !adaptersBefore[x.Key].SequenceEqual(x.Parameter.Value.Data));
            for (var i = 0; i < baseBefore.Length; i++) Assert.Equal(baseBefore[i], policy.BaseParameters[i].Value.Data);
            Assert.Equal(referenceBefore, SaveCheckpoint(reference));
            Assert.False(policy.BaseParameters.Any(p => p.Value.RequiresGrad));
            Assert.False(reference.Parameters().Any(p => p.Value.RequiresGrad));
            var committedWeights = SaveCheckpoint(policy);
            var committedState = trainer.ExportOptimizerState().ToBytes();
            failPublication = true;
            Assert.Throws<IOException>(() => trainer.Step(batch, 2));
            Assert.Equal(committedWeights, SaveCheckpoint(policy));
            Assert.Equal(committedState, trainer.ExportOptimizerState().ToBytes());
            Assert.Equal(referenceBefore, SaveCheckpoint(reference));
            Assert.False(File.Exists(Path.Combine(root, DpoTrainer.BuildPolicyCheckpointFileName(2))));
            Assert.False(File.Exists(Path.Combine(root, DpoTrainer.BuildMetricsFileName(2))));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Fact]
    public void ExactRecipient_PermitCallbackCannotEnableBaseBeforeForwardOrReservation()
    {
        var (policy, reference, adapters, lora, batch) = ExactRecipientFixture();
        var root = CreateTemporaryDirectory("dpo-exact-permit-");
        try
        {
            var recipient = DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora);
            var permits = 0; var reservations = 0;
            var trainer = DpoTrainer.CreateForProjectLora(policy, reference, root,
                DpoMutationAdmissionCapability.Issue(new TestDpoMutationAdmissionLease(() =>
                { if (++permits == 2) policy.BaseParameters[0].Value.RequiresGrad = true; })),
                recipient, DpoExecutionProfile.Capture(batch, recipient.Bindings), null,
                new DpoTrainerPublicationObserver { AfterReservationAcquired = () => reservations++ }, null, null);
            var before = SaveCheckpoint(policy); var refBefore = SaveCheckpoint(reference);
            var exception = Assert.Throws<DpoMutationAdmissionException>(() => trainer.Step(batch, 1));
            Assert.Equal(DpoProjectLoraRecipient.InvalidErrorCode, exception.ErrorCode);
            Assert.Equal(2, permits); Assert.Equal(0, reservations);
            Assert.Equal(before, SaveCheckpoint(policy)); Assert.Equal(refBefore, SaveCheckpoint(reference));
            Assert.Empty(Directory.GetFiles(root));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExactRecipient_MomentAllowlistCheckedBeforeFirstRandomRestore(bool foreignKey)
    {
        var (policy, reference, adapters, lora, batch) = ExactRecipientFixture();
        var root = CreateTemporaryDirectory("dpo-exact-moments-");
        try
        {
            var recipient = DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora);
            var parameter = foreignKey ? policy.GetCheckpointParameterEntries().First(x => !recipient.Bindings.Any(b => b.Key == x.Key))
                : policy.GetCheckpointParameterEntries().Single(x => x.Key == recipient.Bindings[0].Key);
            var state = new AdamWPortableState(AdamWPortableState.CurrentFormatVersion, 1,
                [new AdamWPortableParameterState(parameter.Key, parameter.Parameter.Value.Shape.Dimensions,
                    new float[parameter.Parameter.Value.Data.Length], new float[parameter.Parameter.Value.Data.Length])]);
            var profile = DpoExecutionProfile.Capture(batch, recipient.Bindings);
            // A deliberately local issuer isolates the trainer boundary. Durable
            // source issuance is separately exercised through the real owner.
            var authority = new LoraPairRandomSeedAuthority(73, "policy", "reference");
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(policy.Config, lora, authority);
            var configuration = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(lora, authority);
            var random = policy.CaptureRandomState(topology, configuration, 1);
            var proof = DpoContinuationProof.IssueVerified(profile, state, random, "synthetic-trainer-boundary", () => true);
            var restores = 0;
            var trainer = DpoTrainer.CreateForProjectLora(policy, reference, root,
                DpoMutationAdmissionCapability.Issue(new TestDpoMutationAdmissionLease()), recipient, profile, proof,
                null, step => policy.CaptureRandomState(topology, configuration, step), _ => restores++);
            if (foreignKey)
            {
                var exception = Assert.Throws<DpoMutationAdmissionException>(() => trainer.TryRestoreContinuationState(random, state));
                Assert.Equal(DpoProjectLoraRecipient.InvalidErrorCode, exception.ErrorCode);
                Assert.Equal(0, trainer.ExportOptimizerState().Step);
            }
            else
            {
                Assert.True(trainer.TryRestoreContinuationState(random, state));
                Assert.Single(trainer.ExportOptimizerState().Entries); // Sparse, not fabricated full-map moments.
                Assert.Equal(1, trainer.ExportOptimizerState().Step);
            }
            Assert.Equal(foreignKey ? 0 : 1, restores);
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    // Замена механизма сравнения битов замороженной базы (строка работ «проверки
    // целостности обучения предпочтениями не создают лишние объекты»).
    // Чувствительность: база сверяется по рисунку битов, поэтому пара чисел с
    // одинаковым рисунком обязана пройти, а любая пара с разным рисунком — быть
    // отвергнута до начала обучения. Пары подобраны так, что числовое сравнение
    // их бы пропустило: «плюс ноль» против «минус ноль», два разных «не-числа»
    // (NaN, Not a Number) с разной полезной нагрузкой, знаковая бесконечность и
    // два соседних представления единицы. Разные значения раскладываются по
    // разным местам массива — знаковый ноль в первый элемент, полезная нагрузка
    // «не-числа» в середину, остальное в последний, — иначе сравнение одного
    // только начала массива прошло бы незамеченным.
    [Theory]
    [InlineData(0x00000000u, 0x00000000u, true)]
    [InlineData(0x00000000u, 0x80000000u, false)]
    [InlineData(0x80000000u, 0x80000000u, true)]
    [InlineData(0x7fc00001u, 0x7fc00001u, true)]
    [InlineData(0x7fc00001u, 0x7fc00002u, false)]
    [InlineData(0x7f800000u, 0x7f800000u, true)]
    [InlineData(0x7f800000u, 0xff800000u, false)]
    [InlineData(0x3f800000u, 0x3f800001u, false)]
    public void ExactRecipient_FrozenBaseBitPatternsMatchScalarOracle(
        uint policyBits, uint referenceBits, bool expectedValid)
    {
        var (policy, reference, adapters, lora, _) = ExactRecipientFixture();
        // Берём один и тот же замороженный базовый тензор обеих моделей: сама
        // проверка сверяет их по имени, поэтому и здесь имя — единственная связь.
        var probe = FrozenBaseProbe(policy, reference);
        var index = FrozenBaseProbeIndex(policyBits, probe.Length);
        probe.Policy[index] = BitConverter.Int32BitsToSingle(unchecked((int)policyBits));
        probe.Reference[index] = BitConverter.Int32BitsToSingle(unchecked((int)referenceBits));
        var policyBefore = SaveCheckpoint(policy);
        var referenceBefore = SaveCheckpoint(reference);
        if (expectedValid)
        {
            var recipient = DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora);
            // Повторный вызов на том же получателе обязан быть таким же
            // успешным: ответ не зависит от того, первый это вызов или нет.
            recipient.RequireValid();
        }
        else
        {
            var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
                DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora));
            Assert.Equal(DpoProjectLoraRecipient.InvalidErrorCode, exception.ErrorCode);
        }
        // Ни успешная, ни отказавшая проверка не меняет ни одну из двух моделей.
        // Принятые «не-числа» и бесконечности в обучение не отправляются: ни
        // одна строка этой проверки не делает учебного шага.
        Assert.Equal(policyBefore, SaveCheckpoint(policy));
        Assert.Equal(referenceBefore, SaveCheckpoint(reference));
    }

    // Отрицательный контроль мутации базы: если замороженную базу обучаемой
    // модели меняют ровно на один бит уже после снятия получателя, обучение
    // обязано отказать до брони публикации, до шага оптимизатора и до записи
    // любого файла.
    [Fact]
    public void ExactRecipient_PermitCallbackCannotChangeFrozenBaseBits()
    {
        var (policy, reference, adapters, lora, batch) = ExactRecipientFixture();
        var root = CreateTemporaryDirectory("dpo-exact-base-bit-");
        try
        {
            var recipient = DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora);
            var permits = 0; var reservations = 0;
            byte[]? mutatedPolicy = null;
            var trainer = DpoTrainer.CreateForProjectLora(policy, reference, root,
                DpoMutationAdmissionCapability.Issue(new TestDpoMutationAdmissionLease(() =>
                {
                    // Второе разрешение — момент, когда тренер уже внутри своей
                    // границы. Меняем один младший бит одного числа базы и
                    // запоминаем получившиеся байты.
                    if (++permits != 2) return;
                    var data = policy.BaseParameters[0].Value.Data;
                    data[0] = BitConverter.Int32BitsToSingle(
                        BitConverter.SingleToInt32Bits(data[0]) ^ 1);
                    mutatedPolicy = SaveCheckpoint(policy);
                })),
                recipient, DpoExecutionProfile.Capture(batch, recipient.Bindings), null,
                new DpoTrainerPublicationObserver { AfterReservationAcquired = () => reservations++ }, null, null);
            var policyBefore = SaveCheckpoint(policy);
            var referenceBefore = SaveCheckpoint(reference);
            var optimizerBefore = trainer.ExportOptimizerState().ToBytes();
            var exception = Assert.Throws<DpoMutationAdmissionException>(() => trainer.Step(batch, 1));
            Assert.Equal(DpoProjectLoraRecipient.InvalidErrorCode, exception.ErrorCode);
            Assert.Equal(2, permits); Assert.Equal(0, reservations);
            Assert.NotNull(mutatedPolicy);
            // Внешнюю правку, которой тренер не делал, он и не откатывает:
            // сверяем с теми байтами, что оставил сам обработчик разрешения.
            Assert.Equal(mutatedPolicy, SaveCheckpoint(policy));
            Assert.Equal(referenceBefore, SaveCheckpoint(reference));
            // Пока база остаётся изменённой, отказывает любой путь через
            // получателя, включая выдачу памяти оптимизатора. Это строже, чем
            // просто «учебный шаг не прошёл», и это поведение сохраняется.
            var exportRefusal = Assert.Throws<DpoMutationAdmissionException>(trainer.ExportOptimizerState);
            Assert.Equal(DpoProjectLoraRecipient.InvalidErrorCode, exportRefusal.ErrorCode);
            // Возвращаем тот же один бит на место. Получатель снова действителен,
            // и теперь видно, что обучение не сделало ни одного шага оптимизатора
            // и не изменило обучаемую модель.
            var restored = policy.BaseParameters[0].Value.Data;
            restored[0] = BitConverter.Int32BitsToSingle(
                BitConverter.SingleToInt32Bits(restored[0]) ^ 1);
            Assert.Equal(policyBefore, SaveCheckpoint(policy));
            Assert.Equal(optimizerBefore, trainer.ExportOptimizerState().ToBytes());
            Assert.Empty(Directory.GetFiles(root));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    // Второй отрицательный контроль: один изменённый бит замороженного эталона
    // после снятия получателя тоже обязан быть отвергнут. Для эталона первым
    // срабатывает сохранённый отпечаток эталона — это тот же инвариант
    // неизменности, защищённый на шаг раньше поэлементной сверки.
    [Fact]
    public void ExactRecipient_RejectsPostCaptureReferenceBitMutation()
    {
        var (policy, reference, adapters, lora, _) = ExactRecipientFixture();
        var recipient = DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora);
        var policyBefore = SaveCheckpoint(policy);
        var data = reference.BaseParameters[0].Value.Data;
        data[0] = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(data[0]) ^ 1);
        var mutatedReference = SaveCheckpoint(reference);
        var exception = Assert.Throws<DpoMutationAdmissionException>(recipient.RequireValid);
        Assert.Equal(DpoProjectLoraRecipient.InvalidErrorCode, exception.ErrorCode);
        // Отказ не делает ни учебного шага, ни прохода модели, ни шага
        // оптимизатора и не правит обучаемую модель.
        Assert.Equal(policyBefore, SaveCheckpoint(policy));
        Assert.Equal(mutatedReference, SaveCheckpoint(reference));
    }

    // Замер лишних объектов: сколько байтов выделяет один вызов проверки
    // целостности на одном и том же пути. Это измерение, а не обещание
    // процента: число печатается, сравнение «до/после» делается отдельно.
    [Fact]
    public void ExactRecipient_FrozenBaseCheckAllocationCostIsMeasured()
    {
        var (policy, reference, adapters, lora, _) = ExactRecipientFixture();
        var recipient = DpoProjectLoraRecipient.Capture(policy, reference, adapters, lora);
        // Прогрев: первые вызовы платят за разовую подготовку кода и в замер не
        // входят.
        for (var warm = 0; warm < 20; warm++) recipient.RequireValid();
        const int calls = 200;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var call = 0; call < calls; call++) recipient.RequireValid();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        var baseTensors = policy.BaseParameters.Count;
        var baseNumbers = policy.BaseParameters.Sum(parameter => (long)parameter.Value.Data.Length);
        // Внутри границы замера — весь вызов проверки целостности: отпечаток
        // эталона, наборы параметров, поиск по именам и сравнение битов базы.
        // Снаружи — создание моделей, снятие получателя и любые снимки весов.
        output.WriteLine($"FROZEN_BASE_CHECK_ALLOCATION calls={calls} allocated_bytes={allocated} " +
            $"bytes_per_call={allocated / (double)calls:F1} base_tensors={baseTensors} base_numbers={baseNumbers}");
        Assert.True(allocated > 0, $"allocated_bytes={allocated}");
    }

    // Один и тот же замороженный базовый тензор обеих моделей: первый базовый
    // параметр, в котором не меньше трёх чисел, чтобы «первый», «средний» и
    // «последний» элементы были разными местами.
    private static (float[] Policy, float[] Reference, int Length) FrozenBaseProbe(
        LlmModel policy, LlmModel reference)
    {
        var baseParameter = policy.BaseParameters.First(parameter => parameter.Value.Data.Length >= 3);
        var entry = policy.GetCheckpointParameterEntries()
            .Single(x => ReferenceEquals(x.Parameter, baseParameter));
        var peer = reference.GetCheckpointParameterEntries().Single(x => x.Key == entry.Key);
        Assert.Equal(entry.Parameter.Value.Data.Length, peer.Parameter.Value.Data.Length);
        return (entry.Parameter.Value.Data, peer.Parameter.Value.Data, entry.Parameter.Value.Data.Length);
    }

    // Место записи выбирается по классу самого рисунка битов, а не по номеру
    // строки данных: знаковый ноль — первый элемент, «не-число» — середина,
    // остальное (бесконечность и соседи единицы) — последний элемент.
    private static int FrozenBaseProbeIndex(uint bits, int length)
    {
        var exponent = (bits >> 23) & 0xFFu;
        var mantissa = bits & 0x7FFFFFu;
        if (exponent == 0u && mantissa == 0u) return 0;
        if (exponent == 0xFFu && mantissa != 0u) return length / 2;
        return length - 1;
    }

    private static (LlmModel Policy, LlmModel Reference, IReadOnlyList<LoraLinear> Adapters, LoraConfig Config, DpoBatch Batch) ExactRecipientFixture()
    {
        var config = CreateTinyConfig() with { VocabSize = 256, ContextLen = 8 };
        var policy = new LlmModel(config); var reference = new LlmModel(config);
        LoadCheckpoint(reference, SaveCheckpoint(policy));
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        var applied = LoraApplicator.ApplyFreshSeededPair(policy, reference, lora, 73, "policy", "reference");
        foreach (var parameter in reference.Parameters()) parameter.Value.RequiresGrad = false;
        var admission = DpoTrainerPairCustoms.CreateAdmitted("a", "b", "cc", DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher, "exact-recipient-test").Admission;
        var tokenizer = new Tokenizer.BpeTokenizer(); tokenizer.Train("fixture", 256);
        var batch = DpoExactPairMaterial.CreateBatch(tokenizer, "a", "b", "cc", admission);
        return (policy, reference, applied.FirstModelAdapters, lora, batch);
    }

    private static ModelConfig CreateTinyConfig()
        => new()
        {
            VocabSize = 32,
            ContextLen = 8,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
        };

    internal static DpoBatch CreateSinglePairBatch()
    {
        var admittedPair = DpoTrainerPairCustoms.CreateAdmitted(
            "handoff prompt",
            "handoff chosen",
            "handoff rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "dpo-trainer-optimizer-state-handoff-tests");
        var chosenIds = new[] { 4f, 5f };
        var rejectedIds = new[] { 6f, 7f };
        var chosenMask = new[] { 1f, 1f };
        var rejectedMask = new[] { 1f, 1f };

        return new DpoBatch
        {
            ChosenIds = new Tensor(chosenIds, new TensorShape(1, chosenIds.Length)),
            RejectedIds = new Tensor(rejectedIds, new TensorShape(1, rejectedIds.Length)),
            ChosenMask = new Tensor(chosenMask, new TensorShape(1, chosenMask.Length)),
            RejectedMask = new Tensor(rejectedMask, new TensorShape(1, rejectedMask.Length)),
            Admission = admittedPair.Admission
        };
    }

    private static DpoTrainer CreateAdmittedTrainer(
        LlmModel policy,
        LlmModel reference,
        string checkpointDir)
        => new(
            policy,
            reference,
            checkpointDir,
            DpoMutationAdmissionCapability.Issue(
                new TestDpoMutationAdmissionLease()));

    private static string CreateTemporaryDirectory(string prefix)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    [Fact]
    public void FreshTrainer_BeforeAnyStep_ExportsEmptyOptimizerMemoryWithZeroStep()
    {
        var checkpointDir = CreateTemporaryDirectory("xiigen-dpo-ckpt-");
        try
        {
            var config = CreateTinyConfig();
            var trainer = CreateAdmittedTrainer(
                new LlmModel(config),
                new LlmModel(config),
                checkpointDir);

            var exported = trainer.ExportOptimizerState();

            // До первого шага у оптимизатора нет накопленных оценок, и выдумывать
            // нули вместо них нельзя.
            Assert.Empty(exported.Entries);
            Assert.Equal(0, exported.Step);
        }
        finally
        {
            DeleteDirectoryIfExists(checkpointDir);
        }
    }

    [Fact]
    public void TrainerOptimizerMemory_SurvivesAttemptFolderRoundTripIntoAnotherTrainer()
    {
        var checkpointDir = CreateTemporaryDirectory("xiigen-dpo-ckpt-");
        var attemptRoot = CreateTemporaryDirectory("xiigen-dpo-attempt-");
        var restartedCheckpointDir = CreateTemporaryDirectory(
            "xiigen-dpo-ckpt-restart-");
        try
        {
            var config = CreateTinyConfig();
            var trainer = CreateAdmittedTrainer(
                new LlmModel(config),
                new LlmModel(config),
                checkpointDir);

            // Один настоящий шаг обучения, чтобы у оптимизатора появились
            // накопленные оценки градиентов и ненулевой счётчик шага.
            trainer.Step(CreateSinglePairBatch(), step: 1);

            var exported = trainer.ExportOptimizerState();
            Assert.NotEmpty(exported.Entries);
            Assert.True(exported.Step > 0);

            // Состояние ложится в каталог попытки отдельным файлом со своей суммой.
            var written = RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                attemptRoot,
                step: 1,
                exported);
            Assert.True(written.IsSuccess);
            Assert.NotNull(written.Value);
            Assert.Equal(64, written.Value!.OptimizerStateSha256.Length);

            // Новый тренер моделирует процесс после перезапуска: своей памяти
            // оптимизатора у него нет.
            var restartedTrainer = CreateAdmittedTrainer(
                new LlmModel(config),
                new LlmModel(config),
                restartedCheckpointDir);
            Assert.Empty(restartedTrainer.ExportOptimizerState().Entries);

            var reopened = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                attemptRoot,
                step: 1,
                written.Value.OptimizerStateSha256);
            Assert.True(reopened.IsSuccess);

            Assert.True(restartedTrainer.TryRestoreOptimizerState(reopened.Value!.State));

            // После переноса новый тренер отдаёт те же байты памяти оптимизатора.
            var restoredExport = restartedTrainer.ExportOptimizerState();
            Assert.Equal(exported.Step, restoredExport.Step);
            Assert.Equal(exported.ToBytes(), restoredExport.ToBytes());
        }
        finally
        {
            DeleteDirectoryIfExists(checkpointDir);
            DeleteDirectoryIfExists(attemptRoot);
            DeleteDirectoryIfExists(restartedCheckpointDir);
        }
    }

    [Fact]
    public void RestoreOptimizerState_RefusesMissingStateAndIncompatibleModelShape()
    {
        var checkpointDir = CreateTemporaryDirectory("xiigen-dpo-ckpt-");
        var widerCheckpointDir = CreateTemporaryDirectory(
            "xiigen-dpo-ckpt-wide-");
        try
        {
            var config = CreateTinyConfig();
            var trainer = CreateAdmittedTrainer(
                new LlmModel(config),
                new LlmModel(config),
                checkpointDir);
            trainer.Step(CreateSinglePairBatch(), step: 1);
            var exported = trainer.ExportOptimizerState();

            // Отсутствующее состояние обязано быть отказом, а не тихим продолжением.
            Assert.False(trainer.TryRestoreOptimizerState(null));

            // Модель другой формы под теми же именами параметров: перенос запрещён.
            var widerConfig = CreateTinyConfig() with
            {
                HiddenDim = 16,
                FfnHiddenDim = 32,
                NumKvHeads = 1
            };
            var incompatibleTrainer = CreateAdmittedTrainer(
                new LlmModel(widerConfig),
                new LlmModel(widerConfig),
                widerCheckpointDir);

            Assert.False(incompatibleTrainer.TryRestoreOptimizerState(exported));

            // Отказ не оставляет частично перенесённого состояния.
            Assert.Empty(incompatibleTrainer.ExportOptimizerState().Entries);
        }
        finally
        {
            DeleteDirectoryIfExists(checkpointDir);
            DeleteDirectoryIfExists(widerCheckpointDir);
        }
    }

    [Fact]
    public void ContinuationRestore_RejectingOptimizerRollsBackModelOptimizerAndRng()
    {
        var config = CreateTinyConfig();
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4f,
            Dropout = 0.25f,
            TargetModules = ["q_proj"]
        };
        var authority = new LoraPairRandomSeedAuthority(
            993UL,
            "policy.step",
            "reference.step");
        var topologyHash = LoraRandomStateIdentity.ComputeTopologySha256(
            config,
            loraConfig,
            authority);
        var loraHash = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
            loraConfig,
            authority);
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        LoraApplicator.ApplyFreshSeededPair(
            policy,
            reference,
            loraConfig,
            authority.RootInitializationSeed,
            authority.PolicyDropoutStreamDomain,
            authority.ReferenceDropoutStreamDomain);
        var checkpointDir = CreateTemporaryDirectory("xiigen-dpo-continuation-rollback-");
        try
        {
            var trainer = CreateSeededTrainer(
                policy,
                reference,
                checkpointDir,
                topologyHash,
                loraHash);
            var beforePolicy = SaveCheckpoint(policy);
            var beforeOptimizer = trainer.ExportOptimizerState().ToBytes();
            var beforeRandom = policy.CaptureRandomState(
                topologyHash,
                loraHash,
                randomStateStep: 0);
            var validRandomCandidate = beforeRandom with { RandomStateStep = 1 };
            var rejectedOptimizer = new AdamWPortableState(
                AdamWPortableState.CurrentFormatVersion,
                1,
                [new AdamWPortableParameterState("not-a-policy-key", [1], [0f], [0f])]);

            Assert.False(trainer.TryRestoreContinuationState(
                validRandomCandidate,
                rejectedOptimizer));

            Assert.Equal(beforePolicy, SaveCheckpoint(policy));
            Assert.Equal(beforeOptimizer, trainer.ExportOptimizerState().ToBytes());
            var afterRandom = policy.CaptureRandomState(
                topologyHash,
                loraHash,
                randomStateStep: 0);
            Assert.Equal(beforeRandom.FormatVersion, afterRandom.FormatVersion);
            Assert.Equal(beforeRandom.Algorithm, afterRandom.Algorithm);
            Assert.Equal(beforeRandom.TopologySha256, afterRandom.TopologySha256);
            Assert.Equal(
                beforeRandom.LoraConfigurationSha256,
                afterRandom.LoraConfigurationSha256);
            Assert.Equal(beforeRandom.RandomStateStep, afterRandom.RandomStateStep);
            Assert.Equal(beforeRandom.Entries.Count, afterRandom.Entries.Count);
            for (var index = 0; index < beforeRandom.Entries.Count; index++)
                Assert.Equal(beforeRandom.Entries[index], afterRandom.Entries[index]);
        }
        finally
        {
            DeleteDirectoryIfExists(checkpointDir);
        }
    }

    [Theory]
    [InlineData(DpoTrainer.ContinuationRestoreRollbackFailedErrorCode)]
    [InlineData(DpoTrainer.ContinuationRestoreOptimizerRollbackFailedErrorCode)]
    [InlineData(DpoTrainer.ContinuationRestoreRandomRollbackFailedErrorCode)]
    public void ContinuationRestoreFailureClassifier_PreservesDedicatedRollbackCode(
        string expectedErrorCode)
    {
        var exception = new AggregateException(
            "test continuation restore failure",
            new InvalidOperationException(expectedErrorCode + ": test"));

        Assert.True(LogicalDpoExceptionBoundary.ShouldNormalize(exception));
        Assert.True(
            LogicalDpoExceptionBoundary.TryGetContinuationRestoreFailureCode(
                exception,
                out var actualErrorCode));
        Assert.Equal(expectedErrorCode, actualErrorCode);
        Assert.NotEqual("DPO_RANDOM_STATE_RESTORE_FAILED", actualErrorCode);
    }

    // Прерванное и восстановленное с диска обучение обязано выдать ровно тот
    // же следующий урок, что и непрерывный путь: те же байты обучаемой модели,
    // те же именованные моменты оптимизатора, тот же счётчик шага и то же
    // состояние потока случайных чисел.
    [Fact]
    public void DropoutContinuationMatchesUninterruptedNextUpdateForPolicyOptimizerAndRng()
        => RunDiskContinuation(checkRuntimeShapeGuards: false);

    // Тот же путь восстановления с диска, но дополнительно проверяется отказ
    // сверки описания с живой моделью: подменённая форма параметра с тем же
    // числом чисел обязана быть отвергнута до восстановления.
    [Fact]
    public void RuntimeIdentity_RejectsSameCountShapeBeforeRestore()
        => RunDiskContinuation(checkRuntimeShapeGuards: true);

    // Шаг, на котором ветвь «до сохранения» прерывается и ложится на диск.
    private const int DiskContinuationSaveStep = 1;
    // Шаг, который обе ветви делают дальше и результаты которого сравниваются.
    private const int DiskContinuationNextStep = 2;

    // Память ответов замороженного эталона выключена в обеих сравниваемых
    // ветвях. Она не относится к продолжению с диска, живёт в собственной
    // проверке и при включении сделала бы число проходов эталона разным у
    // непрерывной и восстановленной ветви.
    private const int DiskContinuationReferenceCacheCapacity = 0;

    private static void RunDiskContinuation(bool checkRuntimeShapeGuards)
    {
        // Самая маленькая пригодная модель: побайтовый словарь на 256 позиций,
        // короткий контекст, один слой.
        var config = CreateTinyConfig() with { VocabSize = 256 };
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4f,
            Dropout = 0.25f,
            TargetModules = ["q_proj"]
        };
        var authority = new LoraPairRandomSeedAuthority(
            991UL,
            "policy.step",
            "reference.step");
        var topologyHash = LoraRandomStateIdentity.ComputeTopologySha256(
            config,
            loraConfig,
            authority);
        var loraHash = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
            loraConfig,
            authority);
        // Настоящий точный учебный пакет, а не собранные вручную номера токенов.
        var batch = CreateExactContinuationBatch();
        var uninterruptedRoot = CreateTemporaryDirectory("xiigen-dpo-uninterrupted-");
        var firstRoot = CreateTemporaryDirectory("xiigen-dpo-first-");
        var resumedRoot = CreateTemporaryDirectory("xiigen-dpo-resumed-");
        try
        {
            Directory.CreateDirectory(Path.Combine(firstRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(firstRoot, "11.DPO"));
            File.WriteAllText(Path.Combine(firstRoot, "LlmFromScratch.sln"), string.Empty);
            var addressResult = RepositoryOwnedTrainingArtifactLayout.CreateModelTrainingAttemptAddress(
                firstRoot, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
                "disk-continuation", "v1", "disk-continuation:1");
            Assert.True(addressResult.IsSuccess, addressResult.ErrorMessage);
            var address = addressResult.Value!;
            Directory.CreateDirectory(address.AttemptRoot);

            // Один начальный базовый снимок копируется в обучаемую и эталонную
            // модели каждой ветви. Проектный получатель требует побитно равных
            // замороженных базовых весов у пары, а сравнение ветвей требует
            // одинакового старта.
            var baseCheckpoint = SaveCheckpoint(new LlmModel(config));

            // Ветвь первая: непрерывное обучение, два шага подряд.
            var uninterruptedPair = CreateSeededContinuationPair(
                config, loraConfig, authority, baseCheckpoint);
            var uninterrupted = CreateExactContinuationTrainer(
                uninterruptedPair, CaptureRecipient(uninterruptedPair, loraConfig),
                batch, uninterruptedRoot, topologyHash, loraHash, proof: null, out _);
            var beforeAnyStepRandom = uninterruptedPair.Policy.CaptureRandomState(
                topologyHash, loraHash, randomStateStep: 0);
            uninterrupted.Step(batch, DiskContinuationSaveStep);
            var afterFirstOptimizer = uninterrupted.ExportOptimizerState();
            var afterFirstRandom = uninterruptedPair.Policy.CaptureRandomState(
                topologyHash,
                loraHash,
                afterFirstOptimizer.Step);
            uninterrupted.Step(batch, DiskContinuationNextStep);
            var uninterruptedPolicyBytes = SaveCheckpoint(uninterruptedPair.Policy);
            var uninterruptedOptimizer = uninterrupted.ExportOptimizerState();
            var uninterruptedOptimizerBytes = uninterruptedOptimizer.ToBytes();
            var uninterruptedFinalRandom = uninterruptedPair.Policy.CaptureRandomState(
                topologyHash, loraHash, uninterruptedOptimizer.Step);
            var uninterruptedRandomFile = WriteRandomStateFile(
                uninterruptedRoot,
                uninterruptedFinalRandom);
            var uninterruptedRandomBytes = File.ReadAllBytes(
                uninterruptedRandomFile.Path);

            // Ветвь вторая: один шаг, затем сохранение всего состояния на диск.
            var firstPair = CreateSeededContinuationPair(
                config, loraConfig, authority, baseCheckpoint);
            var first = CreateExactContinuationTrainer(
                firstPair, CaptureRecipient(firstPair, loraConfig), batch, address.AttemptRoot,
                topologyHash, loraHash, proof: null, out var firstProfile);
            first.Step(batch, DiskContinuationSaveStep);
            var firstOptimizer = first.ExportOptimizerState();
            var firstRandom = firstPair.Policy.CaptureRandomState(
                topologyHash,
                loraHash,
                firstOptimizer.Step);
            // Пробные веса — ровно тот файл, который записал сам обучатель;
            // отпечаток считается по его фактическим байтам.
            var trialWeightsPath = Path.Combine(
                address.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(DiskContinuationSaveStep));
            Assert.True(File.Exists(trialWeightsPath));
            var trialWeights = File.ReadAllBytes(trialWeightsPath);
            Assert.Equal(SaveCheckpoint(firstPair.Policy), trialWeights);
            var referenceCheckpoint = SaveCheckpoint(firstPair.Reference);
            File.WriteAllBytes(
                Path.Combine(firstRoot, "reference-step-0.bin"),
                referenceCheckpoint);
            var firstOptimizerFile =
                RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                    address.AttemptRoot,
                    firstOptimizer.Step,
                    firstOptimizer);
            Assert.True(firstOptimizerFile.IsSuccess,
                firstOptimizerFile.ErrorMessage);
            var firstRandomFile = WriteRandomStateFile(address.AttemptRoot, firstRandom);
            // Описание состояния пишет и читает обратно его штатный владелец;
            // ни одно поле текущей версии не дописывается вручную.
            var writtenManifest = PersistContinuationIdentity(address, config, firstProfile,
                firstOptimizerFile.Value!, firstRandomFile, firstRandom, trialWeights,
                referenceCheckpoint, batch);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.CurrentFormatVersion,
                writtenManifest.FormatVersion);

            // Ветвь третья: всё состояние открывается с диска в новом обучателе.
            var resumedPolicy = ReopenContinuationFromDisk(
                firstRoot, config, loraConfig, authority, resumedRoot, batch, out var resumed,
                checkRuntimeShapeGuards);
            var reopenedState = resumed.ExportOptimizerState();
            Assert.Equal(2, reopenedState.FormatVersion);
            // Открытие с диска вернуло именно сохранённое состояние, а не пустое.
            Assert.Equal(firstOptimizer.ToBytes(), reopenedState.ToBytes());
            Assert.Equal(DiskContinuationSaveStep, reopenedState.Step);
            Assert.NotEmpty(reopenedState.Entries);
            foreach (var entry in reopenedState.Entries)
                Assert.Equal(firstPair.Policy.GetCheckpointParameterEntries().Single(p => p.Key == entry.Key)
                    .Parameter.Value.Shape.Dimensions, entry.Dimensions);
            // Веса после открытия с диска совпадают с ветвью до сохранения.
            Assert.Equal(trialWeights, SaveCheckpoint(resumedPolicy));

            // Главное: следующий одинаковый урок после открытия с диска.
            resumed.Step(batch, DiskContinuationNextStep);

            // 1. Байты обучаемой модели совпали с непрерывным путём.
            Assert.Equal(uninterruptedPolicyBytes, SaveCheckpoint(resumedPolicy));
            var resumedOptimizer = resumed.ExportOptimizerState();
            // 2. Счётчик шага оптимизатора совпал.
            Assert.Equal(uninterruptedOptimizer.Step, resumedOptimizer.Step);
            Assert.Equal(DiskContinuationNextStep, resumedOptimizer.Step);
            // 3. Именованные моменты оптимизатора совпали по каждому имени.
            Assert.Equal(
                uninterruptedOptimizer.Entries.Select(e => e.Key).Order(StringComparer.Ordinal),
                resumedOptimizer.Entries.Select(e => e.Key).Order(StringComparer.Ordinal));
            foreach (var expected in uninterruptedOptimizer.Entries)
            {
                var actual = resumedOptimizer.Entries.Single(e => e.Key == expected.Key);
                Assert.Equal(expected.Dimensions, actual.Dimensions);
                Assert.Equal(expected.FirstMoment, actual.FirstMoment);
                Assert.Equal(expected.SecondMoment, actual.SecondMoment);
            }
            // 4. Полные байты памяти оптимизатора совпали.
            Assert.Equal(uninterruptedOptimizerBytes, resumedOptimizer.ToBytes());
            // 5. Состояние потока случайных чисел совпало по типизированным
            //    полям: устройство, настройка адаптеров, шаг и сами записи.
            //    Запись опознаётся по ключу адаптера, а не по месту в массиве;
            //    договор сравнения общий на весь проект проверок, чтобы второго
            //    несовместимого договора не появилось.
            var resumedFinalRandom = resumedPolicy.CaptureRandomState(
                topologyHash, loraHash, resumedOptimizer.Step);
            RandomStateFieldAssertions.AssertEqual(uninterruptedFinalRandom, resumedFinalRandom);
            var resumedRandomFile = WriteRandomStateFile(resumedRoot, resumedFinalRandom);
            Assert.Equal(uninterruptedRandomBytes, File.ReadAllBytes(resumedRandomFile.Path));

            // 6. Позиция потока действительно сдвигается шагом обучения, и
            //    типовое сравнение отвергает и исходную, и сдвинутую позицию —
            //    без единого дополнительного учебного шага.
            Assert.NotEmpty(afterFirstRandom.Entries);
            Assert.NotEqual(
                beforeAnyStepRandom.Entries[0].ConsumedCursor,
                afterFirstRandom.Entries[0].ConsumedCursor);
            Assert.NotEqual(
                afterFirstRandom.Entries[0].ConsumedCursor,
                resumedFinalRandom.Entries[0].ConsumedCursor);
            Assert.NotEqual(beforeAnyStepRandom.Entries[0], resumedFinalRandom.Entries[0]);
            Assert.NotEqual(afterFirstRandom.Entries[0], resumedFinalRandom.Entries[0]);
        }
        finally
        {
            DeleteDirectoryIfExists(uninterruptedRoot);
            DeleteDirectoryIfExists(firstRoot);
            DeleteDirectoryIfExists(resumedRoot);
        }
    }

    /// <summary>Настоящий точный учебный пакет на уже имеющемся примере задания.</summary>
    private static DpoBatch CreateExactContinuationBatch()
    {
        // Тот же пример, что и в соседней точной оснастке файла: запрос «a»,
        // предпочтённый ответ «b», отклонённый ответ «cc».
        var admission = DpoTrainerPairCustoms.CreateAdmitted(
            "a", "b", "cc", DpoPairSideAuthorKind.User, DpoPairSideAuthorKind.OpenTeacher,
            "dpo-trainer-disk-continuation-tests").Admission;
        // Побайтовый разборщик со словарём на 256 позиций.
        var tokenizer = new Tokenizer.BpeTokenizer();
        tokenizer.Train("fixture", 256);
        return DpoExactPairMaterial.CreateBatch(tokenizer, "a", "b", "cc", admission);
    }

    /// <summary>Одна сравниваемая ветвь: обучаемая и эталонная модели плюс её адаптеры.</summary>
    private sealed record ContinuationPair(
        LlmModel Policy, LlmModel Reference, IReadOnlyList<LoraLinear> Adapters);

    /// <summary>
    /// Обе модели ветви стартуют с одного базового снимка, получают одинаково
    /// засеянные адаптеры, а эталон полностью замораживается.
    /// </summary>
    private static ContinuationPair CreateSeededContinuationPair(
        ModelConfig config, LoraConfig loraConfig,
        LoraPairRandomSeedAuthority authority, byte[] baseCheckpoint)
    {
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        LoadCheckpoint(policy, baseCheckpoint);
        LoadCheckpoint(reference, baseCheckpoint);
        var applied = LoraApplicator.ApplyFreshSeededPair(
            policy,
            reference,
            loraConfig,
            authority.RootInitializationSeed,
            authority.PolicyDropoutStreamDomain,
            authority.ReferenceDropoutStreamDomain);
        foreach (var parameter in reference.Parameters()) parameter.Value.RequiresGrad = false;
        return new ContinuationPair(policy, reference, applied.FirstModelAdapters);
    }

    /// <summary>Проектный получатель обучения: только установленные адаптеры пары.</summary>
    private static DpoProjectLoraRecipient CaptureRecipient(
        ContinuationPair pair, LoraConfig loraConfig)
        => DpoProjectLoraRecipient.Capture(pair.Policy, pair.Reference, pair.Adapters, loraConfig);

    /// <summary>
    /// Обучатель с установленным проектным получателем: обновляются только
    /// установленные адаптеры, профиль исполнения снимается с настоящего
    /// пакета и привязок, обычный шаг оптимизатора сохраняется.
    /// </summary>
    private static DpoTrainer CreateExactContinuationTrainer(
        ContinuationPair pair, DpoProjectLoraRecipient recipient, DpoBatch batch,
        string checkpointDir, string topologyHash, string loraHash,
        DpoContinuationProof? proof, out DpoExecutionProfile profile)
    {
        // Для восстановленной ветви профиль приходит из прочитанного с диска
        // описания, для остальных снимается с пакета и привязок.
        profile = proof?.Profile ?? DpoExecutionProfile.Capture(batch, recipient.Bindings);
        return DpoTrainer.CreateForProjectLora(
            pair.Policy,
            pair.Reference,
            checkpointDir,
            DpoMutationAdmissionCapability.Issue(new TestDpoMutationAdmissionLease()),
            recipient,
            profile,
            proof,
            null,
            step => pair.Policy.CaptureRandomState(topologyHash, loraHash, step),
            state => pair.Policy.RestoreRandomState(
                state, new LlmModelRandomStateIdentity(topologyHash, loraHash)),
            DiskContinuationReferenceCacheCapacity);
    }

    private sealed record SavedReferenceIdentity(string Path, string Sha256, long ByteLength);

    /// <summary>Содержимое запечатанной точной пары, по которому считается её отпечаток.</summary>
    private sealed record SavedExactPairIdentity(
        string Prompt, string Chosen, string Rejected, string AdmissionFingerprintSha256,
        string MaterialDigestSha256, string TokenizerSha256, int TokenizerVocabSize);

    /// <summary>Содержимое описания учебного набора, по которому считается его отпечаток.</summary>
    private sealed record SavedExactDatasetIdentity(
        string DomainSlug, string DesignPatternSlug, string PairSha256);

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    /// <summary>
    /// Записывает описание состояния обучения штатным владельцем записи и
    /// возвращает прочитанную обратно запись. Все отпечатки и длины считаются
    /// по фактически записанным на диск байтам.
    /// </summary>
    private static CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest
        PersistContinuationIdentity(
            RepositoryOwnedModelTrainingAttemptAddress address, ModelConfig config,
            DpoExecutionProfile profile,
            RepositoryOwnedDpoOptimizerStateFileReadback optimizer,
            RepositoryOwnedDpoRandomStateWriteReadback randomFile,
            LlmModelRandomStateSnapshot random, byte[] trialWeights, byte[] reference,
            DpoBatch batch)
    {
        // Отпечаток эталонных весов и их опись: это настоящие байты на диске.
        var referenceSha = Sha256Hex(reference);
        var referenceIdentityPath = Path.Combine(address.RepositoryRoot, "reference-identity.json");
        File.WriteAllBytes(referenceIdentityPath, JsonSerializer.SerializeToUtf8Bytes(
            new SavedReferenceIdentity("reference-step-0.bin", referenceSha, reference.LongLength)));
        var referenceIdentityBytes = File.ReadAllBytes(referenceIdentityPath);

        // Запечатанная точная пара и её описание тоже лежат настоящими файлами,
        // поэтому их отпечатки измерены, а не выдуманы.
        var material = batch.ExactMaterial!;
        Directory.CreateDirectory(Path.Combine(address.RepositoryRoot, "dpo"));
        var pairBytes = JsonSerializer.SerializeToUtf8Bytes(new SavedExactPairIdentity(
            material.Prompt, material.Chosen, material.Rejected,
            material.AdmissionFingerprintSha256, material.DigestSha256,
            material.TokenizerSha256, material.TokenizerVocabSize));
        File.WriteAllBytes(Path.Combine(address.RepositoryRoot, "dpo", "pair.json"), pairBytes);
        var pairSha = Sha256Hex(pairBytes);
        var datasetBytes = JsonSerializer.SerializeToUtf8Bytes(new SavedExactDatasetIdentity(
            address.DomainSlug, address.DesignPatternSlug, pairSha));
        File.WriteAllBytes(Path.Combine(address.RepositoryRoot, "dpo", "manifest.json"), datasetBytes);

        // Паспорт принятой родительской модели указывает на реально записанные
        // эталонные веса и на их опись; номер пары — отпечаток содержимого пары.
        var passport = new CodeDocumentationSavedModelAssessmentModelPassport(
            address.DomainSlug, address.DesignPatternSlug, address.ModelIdSlug, address.ModelVersion,
            CheckpointStep: 1,
            RepositoryRelativeManifestPath: "reference-identity.json",
            ManifestSha256: Sha256Hex(referenceIdentityBytes),
            RepositoryRelativeWeightsPath: "reference-step-0.bin",
            WeightsSha256: referenceSha,
            WeightsByteLength: reference.LongLength,
            MaximumModelSerializationBytes: reference.LongLength);
        // Оценка качества принадлежит проверкам издателя обучения: владелец
        // описания состояния читает из исходной записи только поля опознания.
        var baseline = new CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState(
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.CurrentFormatVersion,
            address.TrainingRunId, address.TrainingRunIdSha256,
            address.AttemptRootRelativePath, address.AttemptClaimRelativePath,
            address.DomainSlug, address.DesignPatternSlug, address.ModelIdSlug, address.ModelVersion,
            passport, "dpo/manifest.json", Sha256Hex(datasetBytes),
            pairSha, "dpo/pair.json", pairSha, BaselineQuality: null!);

        var optimizerPath = Path.Combine(
            address.AttemptRoot,
            RepositoryOwnedDpoOptimizerStateFile.BuildFileName(optimizer.State.Step));
        var trialPath = Path.Combine(
            address.AttemptRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(DiskContinuationSaveStep));
        var artifact = new DpoTrainingExecutionArtifact(
            ProcessedPairCount: 1,
            DpoStepCount: DiskContinuationSaveStep,
            PolicyCheckpointPath: trialPath,
            MetricsPath: Path.Combine(
                address.AttemptRoot, DpoTrainer.BuildMetricsFileName(DiskContinuationSaveStep)),
            LegalTeacherPolicyIdentityRef: "test:disk-continuation-exact-pair",
            PostStepOptimizerState: optimizer.State,
            PostStepOptimizerStateIdentity: new LogicalDpoTrainingOptimizerStateIdentity(
                optimizer.State.FormatVersion, optimizer.State.Step,
                optimizer.OptimizerStateSha256, optimizer.OptimizerStateByteLength),
            OptimizerStatePath: optimizerPath,
            OptimizerStateSha256: optimizer.OptimizerStateSha256,
            OptimizerStateByteLength: optimizer.OptimizerStateByteLength,
            OptimizerStateStep: optimizer.State.Step,
            OptimizerStateFormatVersion: optimizer.State.FormatVersion,
            OptimizerSettings: DpoTrainer.ProductionOptimizerSettings,
            PostStepRandomState: random,
            RandomStatePath: randomFile.Path,
            RandomStateSha256: randomFile.Sha256,
            RandomStateByteLength: randomFile.ByteLength,
            RandomStateStep: random.RandomStateStep,
            RandomStateFormatVersion: (int)random.FormatVersion,
            RandomStateRequired: true,
            RandomStateTopologySha256: random.TopologySha256,
            RandomStateLoraConfigurationSha256: random.LoraConfigurationSha256,
            TrialWeightsPath: trialPath,
            TrialWeightsSha256: Sha256Hex(trialWeights),
            TrialWeightsByteLength: trialWeights.LongLength,
            ExecutionProfile: profile);

        var written = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
            .WriteAndReadBack(address, baseline, artifact, config, DpoTrainer.ProductionOptimizerSettings);
        Assert.True(written.IsSuccess, written.ErrorMessage);
        return written.Value!.Manifest;
    }

    private static LlmModel ReopenContinuationFromDisk(
        string repositoryRoot, ModelConfig config, LoraConfig loraConfig,
        LoraPairRandomSeedAuthority authority, string resumedRoot, DpoBatch batch,
        out DpoTrainer resumed, bool checkRuntimeShapeGuards)
    {
        var address = RepositoryOwnedTrainingArtifactLayout.CreateModelTrainingAttemptAddress(
            repositoryRoot, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
            "disk-continuation", "v1", "disk-continuation:1").Value!;
        var reopenedManifest = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
            .OpenExisting(address);
        Assert.True(reopenedManifest.IsSuccess, reopenedManifest.ErrorMessage);
        var manifest = reopenedManifest.Value!.Manifest;
        // С диска прочитана запись текущей версии с настоящим профилем точного
        // материала, а не прежняя четвёртая с пустыми полями.
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.CurrentFormatVersion,
            manifest.FormatVersion);
        Assert.Equal(DpoExactPairMaterial.Version, manifest.MaterialVersion);
        Assert.Equal(DpoExactPairMaterial.FramingVersion, manifest.FramingVersion);
        Assert.Equal(DpoExactPairMaterial.ObjectiveVersion, manifest.ObjectiveVersion);
        Assert.NotEmpty(manifest.AllowedParameterMap!);
        var savedReference = JsonSerializer.Deserialize<SavedReferenceIdentity>(
            File.ReadAllText(Path.Combine(repositoryRoot, "reference-identity.json")))!;
        var topologyHash = LoraRandomStateIdentity.ComputeTopologySha256(config, loraConfig, authority);
        var loraHash = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(loraConfig, authority);
        // Отпечатки совместимости среды принадлежат только этой управляемой
        // оснастке: служба запуска их не сверяет с описанием состояния.
        var fixtureSha = new string('a', 64);
        var startRequest = new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(
            repositoryRoot, address.DomainSlug, address.DesignPatternSlug,
            address.ModelIdSlug, address.ModelVersion, config,
            new RepositoryOwnedModelRuntimeCompatibility(
                TrainingSourceProvenance.Identify(config).Sha256,
                fixtureSha, fixtureSha, fixtureSha, fixtureSha, "disk-continuation-test"),
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
            LoraConfig: loraConfig, AdapterSeedAuthority: authority);
        var loaded = CodeDocumentationRepositoryOwnedModelCheckpointStartService
            .LoadTrainingAttemptCheckpoint(startRequest, address, manifest.DpoStepCount);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var resumedPolicy = loaded.Value!.Model;
        var resumedReference = new LlmModel(config);
        var referenceTopologyPeer = new LlmModel(config);
        LoraApplicator.ApplyFreshSeededPair(
            referenceTopologyPeer,
            resumedReference,
            loraConfig,
            authority.RootInitializationSeed,
            authority.PolicyDropoutStreamDomain,
            authority.ReferenceDropoutStreamDomain);
        var trialValidation = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
            .ValidateTrialCheckpointAgainstManifest(manifest, address, loaded.Value);
        Assert.True(trialValidation.IsSuccess, trialValidation.ErrorMessage);
        var referenceBytes = File.ReadAllBytes(Path.Combine(repositoryRoot, savedReference.Path));
        Assert.Equal(savedReference.ByteLength, referenceBytes.LongLength);
        Assert.Equal(savedReference.Sha256, Sha256Hex(referenceBytes));
        LoadCheckpoint(resumedReference, referenceBytes);
        foreach (var parameter in resumedReference.Parameters()) parameter.Value.RequiresGrad = false;
        var reopenedOptimizer =
            RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                address.AttemptRoot,
                manifest.OptimizerStateStep,
                manifest.OptimizerStateSha256);
        Assert.True(reopenedOptimizer.IsSuccess,
            reopenedOptimizer.ErrorMessage);
        Assert.Equal(manifest.OptimizerStateByteLength, reopenedOptimizer.Value!.OptimizerStateByteLength);
        var runtime = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.MatchesRuntime(
            manifest, address, config, resumedPolicy.GetCheckpointParameterEntries()
                .Select(e => new AdamWParameterBinding(e.Key, e.Parameter)).ToArray(),
            DpoTrainer.ProductionOptimizerSettings, reopenedOptimizer.Value.State);
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        Assert.Equal(2, reopenedOptimizer.Value.State.FormatVersion);
        foreach (var entry in reopenedOptimizer.Value.State.Entries)
            Assert.Equal(resumedPolicy.GetCheckpointParameterEntries().Single(p => p.Key == entry.Key)
                .Parameter.Value.Shape.Dimensions, entry.Dimensions);
        if (checkRuntimeShapeGuards)
            AssertRuntimeShapeGuards(manifest, address, config, resumedPolicy,
                resumedReference, reopenedOptimizer.Value.State);
        var reopenedRandom = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
            .OpenAndValidateExistingAttemptRandomState(config, loraConfig, authority, null,
                address, manifest, CancellationToken.None);
        Assert.True(reopenedRandom.IsSuccess,
            reopenedRandom.ErrorMessage);
        // Адаптеры восстановленной модели берутся с самих установленных мест.
        var canonicalLora = LoraApplicator.ValidateAndCanonicalize(loraConfig);
        var resumedAdapters = Enumerable.Range(0, resumedPolicy.Blocks.Count)
            .SelectMany(block => canonicalLora.TargetModules.Select(target =>
                (LoraLinear)resumedPolicy.GetAttachedProjectionAdapter(block, target)!))
            .ToArray();
        var resumedPair = new ContinuationPair(resumedPolicy, resumedReference, resumedAdapters);
        var resumedRecipient = CaptureRecipient(resumedPair, loraConfig);
        // Профиль исполнения приходит из прочитанного с диска описания и обязан
        // совпасть с профилем, снятым с текущего пакета и привязок.
        var profileFromDisk = DpoExecutionProfile.FromVerifiedManifest(
            manifest.MaterialVersion, manifest.FramingVersion, manifest.ObjectiveVersion,
            manifest.AllowedParameterMap!);
        Assert.True(profileFromDisk.Matches(
            DpoExecutionProfile.Capture(batch, resumedRecipient.Bindings)));
        // Доказательство продолжения выдаётся только после канонического чтения
        // с диска: его источник — путь и отпечаток именно этого описания.
        var manifestSha = reopenedManifest.Value.ManifestSha256;
        var proof = DpoContinuationProof.IssueVerified(
            profileFromDisk, reopenedOptimizer.Value.State, reopenedRandom.Value,
            address.RepositoryRoot + "|" + reopenedManifest.Value.ManifestRelativePath + "|" + manifestSha,
            () =>
            {
                var current = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .OpenExisting(address);
                return current.IsSuccess && current.Value is not null &&
                    StringComparer.Ordinal.Equals(current.Value.ManifestSha256, manifestSha);
            });
        resumed = CreateExactContinuationTrainer(
            resumedPair, resumedRecipient, batch, resumedRoot, topologyHash, loraHash, proof, out _);
        Assert.True(resumed.TryRestoreContinuationState(reopenedRandom.Value, reopenedOptimizer.Value.State));
        return resumedPolicy;
    }

    private static void AssertRuntimeShapeGuards(
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest manifest,
        RepositoryOwnedModelTrainingAttemptAddress address, ModelConfig config,
        LlmModel policy, LlmModel reference, AdamWPortableState state)
    {
        var bindings = policy.GetCheckpointParameterEntries()
            .Select(p => new AdamWParameterBinding(p.Key, p.Parameter)).ToArray();
        var changed = state.Entries.First(e => e.Dimensions.Count == 2 && e.Dimensions[0] != e.Dimensions[1]);
        var index = Array.FindIndex(bindings, b => b.Key == changed.Key);
        var weights = SaveCheckpoint(policy);
        var referenceWeights = SaveCheckpoint(reference);
        var bytes = state.ToBytes();
        var files = Directory.GetFiles(address.RepositoryRoot, "*", SearchOption.AllDirectories)
            .ToDictionary(p => p, File.ReadAllBytes);
        OperationResult<bool> Validate(AdamWParameterBinding[] candidates,
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest? candidateManifest = null)
            => CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.MatchesRuntime(
                candidateManifest ?? manifest, address, config, candidates,
                DpoTrainer.ProductionOptimizerSettings, state);
        Assert.True(Validate(bindings).IsSuccess);
        Assert.True(bindings.Length > state.Entries.Count); // real sparse LoRA control
        var wrongShape = bindings.ToArray();
        wrongShape[index] = new AdamWParameterBinding(changed.Key, new Parameter(new Tensor(
            bindings[index].Parameter.Value.Data.ToArray(), new TensorShape(changed.Dimensions.Reverse().ToArray()))));
        Assert.Equal(changed.FirstMoment.Count, wrongShape[index].Parameter.Value.Data.Length);
        Assert.False(changed.Dimensions.SequenceEqual(wrongShape[index].Parameter.Value.Shape.Dimensions));
        var refused = Validate(wrongShape);
        Assert.False(refused.IsSuccess);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.InvalidErrorCode, refused.ErrorCode);
        // Таблица испорченных наборов привязок. У каждой строки свой точный код
        // отказа: нарушение опознания привязок даёт общий отказ сверки, а
        // пропажа разрешённого параметра — отказ по карте разрешённых
        // параметров, потому что эта карта проверяется раньше форм.
        foreach (var (reason, invalid, expectedErrorCode) in new (string, AdamWParameterBinding[], string)[]
        {
            ("тот же параметр добавлен дважды",
                bindings.Append(bindings[0]).ToArray(),
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.InvalidErrorCode),
            ("тот же параметр под чужим именем",
                bindings.Append(new AdamWParameterBinding("duplicate-parameter", bindings[0].Parameter)).ToArray(),
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.InvalidErrorCode),
            ("пустая привязка в списке",
                bindings.Append(null!).ToArray(),
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.InvalidErrorCode),
            ("привязка с пустым именем",
                bindings.Append(new AdamWParameterBinding("", new Parameter(new Tensor(new TensorShape(1))))).ToArray(),
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.InvalidErrorCode),
            ("разрешённый обучаемый параметр пропал из набора",
                bindings.Where(b => b.Key != changed.Key).ToArray(),
                DpoExecutionProfile.MismatchErrorCode)
        })
        {
            var result = Validate(invalid);
            Assert.False(result.IsSuccess, reason);
            Assert.Equal(expectedErrorCode, result.ErrorCode);
        }
        var legacy = Validate(bindings, manifest with { OptimizerStateFormatVersion = 1 });
        Assert.False(legacy.IsSuccess);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.InvalidErrorCode, legacy.ErrorCode);
        Assert.Equal(weights, SaveCheckpoint(policy));
        Assert.Equal(referenceWeights, SaveCheckpoint(reference));
        Assert.Equal(bytes, state.ToBytes());
        Assert.Equal(files.Keys.Order(), Directory.GetFiles(address.RepositoryRoot, "*", SearchOption.AllDirectories).Order());
        foreach (var file in files) Assert.Equal(file.Value, File.ReadAllBytes(file.Key));
    }

    [Fact]
    public void RestoreOptimizerState_SameCountShapeMismatch_PreservesTrainedState()
    {
        var config = CreateTinyConfig();
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0.25f, TargetModules = ["q_proj"] };
        var authority = new LoraPairRandomSeedAuthority(994UL, "policy.step", "reference.step");
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(config, lora, authority);
        var configuration = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(lora, authority);
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        LoraApplicator.ApplyFreshSeededPair(policy, reference, lora, authority.RootInitializationSeed,
            authority.PolicyDropoutStreamDomain, authority.ReferenceDropoutStreamDomain);
        var root = CreateTemporaryDirectory("xiigen-dpo-shape-restore-");
        try
        {
            var trainer = CreateSeededTrainer(policy, reference, root, topology, configuration);
            // Three tokens score a position with two causal attention keys.
            // With only two tokens the scored first position has a singleton
            // softmax, so q_proj's gradient (and both moments) stays zero.
            var batch = CreateSinglePairBatch() with
            {
                ChosenIds = new Tensor([4f, 5f, 8f], new TensorShape(1, 3)),
                RejectedIds = new Tensor([6f, 7f, 9f], new TensorShape(1, 3)),
                ChosenMask = new Tensor([1f, 1f, 1f], new TensorShape(1, 3)),
                RejectedMask = new Tensor([1f, 1f, 1f], new TensorShape(1, 3))
            };
            trainer.Step(batch, 1);
            var state = trainer.ExportOptimizerState();
            Assert.True(state.Step > 0);
            Assert.Contains(state.Entries, e => e.FirstMoment.Any(v => v != 0f));
            var changed = state.Entries.First(e => e.Dimensions.Count == 2 &&
                e.Dimensions[0] != e.Dimensions[1] && e.FirstMoment.Any(v => v != 0f));
            var candidate = new AdamWPortableState(2, state.Step, state.Entries.Select(e =>
                new AdamWPortableParameterState(e.Key,
                    e == changed ? e.Dimensions.Reverse().ToArray() : e.Dimensions.ToArray(),
                    e.FirstMoment.ToArray(), e.SecondMoment.ToArray())).ToArray());
            var reopened = AdamWPortableState.FromBytes(candidate.ToBytes());
            Assert.NotNull(reopened);
            var modelBytes = new[] { SaveCheckpoint(policy), SaveCheckpoint(reference) };
            var models = new[] { policy, reference };
            var rng = models.Select(m => m.CaptureRandomState(topology, configuration, state.Step)).ToArray();
            var parameters = models.SelectMany(m => m.Parameters()).ToArray();
            var gradients = parameters.Select(p => p.Grad).ToArray();
            var gradientData = gradients.Select(g => g?.Data.ToArray()).ToArray();
            var flags = parameters.Select(p => p.Value.RequiresGrad).ToArray();
            Assert.False(trainer.TryRestoreOptimizerState(reopened));
            Assert.Equal(state.ToBytes(), trainer.ExportOptimizerState().ToBytes());
            for (var i = 0; i < models.Length; i++)
            {
                Assert.Equal(modelBytes[i], SaveCheckpoint(models[i]));
                Assert.Equal(rng[i].Entries, models[i].CaptureRandomState(topology, configuration, state.Step).Entries);
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                Assert.Same(gradients[i], parameters[i].Grad);
                Assert.Equal(gradientData[i], parameters[i].Grad?.Data);
                Assert.Equal(flags[i], parameters[i].Value.RequiresGrad);
            }
        }
        finally
        {
            DeleteDirectoryIfExists(root);
            Assert.False(Directory.Exists(root));
        }
    }

    [Fact]
    public void PublicationFailureRestoresRngAndLeavesAcceptedParentUntouched()
    {
        var config = CreateTinyConfig();
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4f,
            Dropout = 0.25f,
            TargetModules = ["q_proj"]
        };
        var authority = new LoraPairRandomSeedAuthority(
            992UL,
            "policy.step",
            "reference.step");
        var topologyHash = LoraRandomStateIdentity.ComputeTopologySha256(
            config,
            loraConfig,
            authority);
        var loraHash = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
            loraConfig,
            authority);
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        LoraApplicator.ApplyFreshSeededPair(
            policy,
            reference,
            loraConfig,
            authority.RootInitializationSeed,
            authority.PolicyDropoutStreamDomain,
            authority.ReferenceDropoutStreamDomain);
        var checkpointRoot = CreateTemporaryDirectory("xiigen-dpo-rollback-");
        var beforePolicy = SaveCheckpoint(policy);
        var beforeRandom = policy.CaptureRandomState(topologyHash, loraHash, 0);
        try
        {
            var trainer = new DpoTrainer(
                policy,
                reference,
                checkpointRoot,
                DpoMutationAdmissionCapability.Issue(
                    new TestDpoMutationAdmissionLease()),
                captureRandomState: step => policy.CaptureRandomState(
                    topologyHash,
                    loraHash,
                    step),
                restoreRandomState: state => policy.RestoreRandomState(
                    state,
                    new LlmModelRandomStateIdentity(topologyHash, loraHash)),
                publicationObserver: new DpoTrainerPublicationObserver
                {
                    BeforePolicyFinalMove = _ => throw new InvalidOperationException(
                        "test publication failure")
                });

            Assert.Throws<InvalidOperationException>(() =>
                trainer.Step(CreateSinglePairBatch(), 1));

            Assert.Equal(beforePolicy, SaveCheckpoint(policy));
            var afterFailureRandom = policy.CaptureRandomState(
                topologyHash, loraHash, 0);
            Assert.Equal(
                beforeRandom.Entries[0].ConsumedCursor,
                afterFailureRandom.Entries[0].ConsumedCursor);
            Assert.Equal(beforeRandom.Entries[0], afterFailureRandom.Entries[0]);
            Assert.Empty(trainer.ExportOptimizerState().Entries);
            Assert.False(File.Exists(Path.Combine(
                checkpointRoot,
                DpoTrainer.BuildMetricsFileName(1))));
        }
        finally
        {
            DeleteDirectoryIfExists(checkpointRoot);
        }
    }

    internal static DpoTrainer CreateSeededTrainer(
        LlmModel policy,
        LlmModel reference,
        string checkpointDir,
        string topologyHash,
        string loraHash)
        => new(
            policy,
            reference,
            checkpointDir,
            DpoMutationAdmissionCapability.Issue(
                new TestDpoMutationAdmissionLease()),
            captureRandomState: step => policy.CaptureRandomState(
                topologyHash,
                loraHash,
                step),
            restoreRandomState: state => policy.RestoreRandomState(
                state,
                new LlmModelRandomStateIdentity(topologyHash, loraHash)));

    private static byte[] SaveCheckpoint(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return stream.ToArray();
    }

    private static void LoadCheckpoint(LlmModel model, byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        model.LoadCheckpoint(stream);
    }

    private static RepositoryOwnedDpoRandomStateWriteReadback
        WriteRandomStateFile(
            string root,
            LlmModelRandomStateSnapshot state)
    {
        var path = Path.Combine(
            root,
            RepositoryOwnedDpoRandomStateFile.BuildFileName(
                state.RandomStateStep));
        var written = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
            path,
            state);
        Assert.True(written.IsSuccess, written.ErrorMessage);
        return written.Value!;
    }

    private sealed class TestDpoMutationAdmissionLease(Action? onAcquire = null) : IDpoMutationAdmissionIssuer
    {
        IDisposable IDpoMutationAdmissionIssuer
            .AcquireDpoTrainerMutationPermit()
        {
            onAcquire?.Invoke();
            return new CancellationTokenSource();
        }
    }
}
