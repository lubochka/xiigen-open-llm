// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Layers;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

// Что здесь проверяется, по-человечески.
//
// Модуль обучения предпочтениями получает набор учебных пар, у каждой есть вес — относительная
// сила её вклада. Вес ноль означает «эта пара не учит ничему».
//
// Что было неправильно. Вес каждой пары дважды проходил через нижний порог одной сотой: в
// знаменателе усреднения и в весе самой пары. Поэтому пара с весом РОВНО ноль вносила вклад
// двумя путями сразу — считала свой собственный градиент и раздувала знаменатель, отнимая у
// нужной пары почти процент её влияния. И платил набор за такую пару полную цену: четыре прямых
// и два обратных прохода.
//
// Что измерено. Опыт сто четырнадцатый (`experiments-v2/RP01-MICRO/P11401`) снял это на двух
// парах из одного восстановленного обученного старта: знаменатель 1,01 вместо 1,0; относительный
// вес нужной пары 0,990099 вместо 1,0; собственный градиент нулевой пары минус 0,0053630364;
// расхождение обученных весов с контролем 3,2 на десять в минус седьмой при побайтово разных
// отпечатках. Плечо с исключением ровно нулевого веса совпало с контрольным обучением БЕЗ лишней
// пары ПОБАЙТОВО: те же обучаемые веса, те же моменты обновителя, расхождение значений ноль.
//
// Что стало. Пара с сырым весом ровно ноль исключается до потери, градиента и усреднения.
// Знаменатель и нормированные веса считаются только по активным парам. Набор, в котором активных
// пар не осталось, — случай без обновления.
//
// Чего эти проверки НЕ доказывают. Они не доказывают улучшения качества ответов: опыт прямо
// записал, что при одном обновлении ни один ответ панели не сдвинулся и порог вырождения не
// превышен. Они не доказывают ничего об отрицательных весах, не-а-числах и бесконечностях —
// это отдельный подслучай. И они НЕ лечат вторую половину проблемы: любой ПОЛОЖИТЕЛЬНЫЙ вес
// ниже одной сотой нижний порог по-прежнему поднимает до одной сотой, поэтому вес одна
// миллионная и вес одна сотая остаются неотличимы. Это отдельный наряд.
//
// Названная разница в поведении. Набор, где ВСЕ веса равны нулю, прежде обучал в полную силу:
// порог поднимал каждый вес до одной сотой, и пары делили влияние поровну. Теперь такой набор
// не обучает ничему. Это исправление ошибки, а не ухудшение.
//
// Чувствительность к мутации. Все три проверки обязаны падать на прежней логике: там обученное
// состояние расходилось с контролем, показания шага разбавлялись весом нулевой пары, цена шага
// была вдвое выше, а набор из одних нулей публиковал обычный обученный шаг.
public sealed class DpoZeroWeightPairExclusionTests
{
    // Крошечная модель инженерной оснастки: обучающих заявлений на ней не делается.
    private static readonly ModelConfig Config = new()
    {
        VocabSize = 256,
        ContextLen = 8,
        HiddenDim = 8,
        NumLayers = 1,
        NumHeads = 2,
        NumKvHeads = 1,
        FfnHiddenDim = 16
    };

    // Запись показаний шага сравнивается целиком, поэтому её вид закрепляется один раз.
    private static readonly JsonSerializerOptions Json =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    // ── Требование 1: нулевая пара не меняет обученное состояние ───────────────

    [Fact]
    public void PairWithZeroWeight_LeavesTrainedStateIdenticalToTrainingWithoutThatPair()
    {
        var root = NewRoot("zero-weight-vs-without");

        // Два независимых старта из одного и того же построения: отдельные веса, отдельный
        // замороженный эталон, отдельные моменты обновителя. Иначе плечи влияли бы друг на друга.
        var withZero = Fixture();
        var without = Fixture();

        // Оба старта обязаны начинаться с одних и тех же байтов — иначе сравнивать нечего.
        Assert.Equal(Bytes(without.Policy), Bytes(withZero.Policy));

        // Плечо «как в бою»: нужная пара с весом единица и лишняя пара с весом ровно ноль.
        var pairsWithZero = new[]
        {
            Pair(withZero.Tokenizer, "a", "b", "cc", weight: 1f),
            Pair(withZero.Tokenizer, "d", "ee", "f", weight: 0f)
        };

        // Контрольное плечо: та же нужная пара и больше ничего.
        var pairsWithout = new[]
        {
            Pair(without.Tokenizer, "a", "b", "cc", weight: 1f)
        };

        var trainerWithZero = Trainer(withZero, Path.Combine(root, "with-zero"), pairsWithZero[0].Batch);
        var trainerWithout = Trainer(without, Path.Combine(root, "without"), pairsWithout[0].Batch);

        var metricsWithZero = trainerWithZero.StepVector(pairsWithZero, 1);
        var metricsWithout = trainerWithout.StepVector(pairsWithout, 1);

        // ГЛАВНОЕ: обученные веса совпадают побайтово. Нулевая пара не сдвинула ни одного бита.
        Assert.Equal(Bytes(without.Policy), Bytes(withZero.Policy));
        // Накопитель поправок обновителя тоже совпадает побайтово.
        Assert.Equal(
            trainerWithout.ExportOptimizerState().ToBytes(),
            trainerWithZero.ExportOptimizerState().ToBytes());
        // Обновление в обоих плечах состоялось: это исключение ВКЛАДА пары, а не всего шага.
        Assert.Equal(1, trainerWithZero.ExportOptimizerState().Step);
        Assert.Equal(1, trainerWithout.ExportOptimizerState().Step);
        // Замороженный эталон не менялся ни в одном плече: учится только разрешённое.
        Assert.Equal(Bytes(without.Reference), Bytes(withZero.Reference));
        // Показания шага тоже совпадают целиком — ни одно поле не разбавлено нулевой парой.
        Assert.Equal(
            JsonSerializer.Serialize(metricsWithout, Json),
            JsonSerializer.Serialize(metricsWithZero, Json));
    }

    // ── Требование 2: знаменатель и вес нужной пары не разбавляются ────────────

    [Fact]
    public void ZeroWeightPair_NeitherDilutesTheNeededPairNorCostsAnyModelPass()
    {
        var root = NewRoot("no-dilution-and-no-price");
        var fixture = Fixture();

        var pairs = new[]
        {
            Pair(fixture.Tokenizer, "a", "b", "cc", weight: 1f),
            Pair(fixture.Tokenizer, "d", "ee", "f", weight: 0f)
        };

        var trainer = Trainer(fixture, root, pairs[0].Batch);
        // Наблюдатель этапов шага считает фактические проходы модели: по нему и видно цену.
        var timing = new DpoStepVectorTiming();

        var metrics = trainer.StepVector(pairs, 1, timing);

        // Нужная пара получила ВЕСЬ свой вес: при одной активной паре её нормированный вес равен
        // единице, поэтому доля пар с положительным запасом может быть только нулём или единицей.
        // Прежде она равнялась 0,990099 или 0,00990099 — числам разбавленного знаменателя.
        Assert.True(
            metrics.Accuracy is 0f or 1f,
            "доля пар с положительным запасом разбавлена весом нулевой пары: " + metrics.Accuracy);

        var snapshot = timing.Snapshot();
        // Шаг завершился обычным возвратом, а не исключением.
        Assert.Equal("RETURNED", snapshot.Outcome);
        // Обработана РОВНО ОДНА пара из двух: исключённая пара в цикл не входила.
        Assert.Equal(1, snapshot.VectorBatchesCompleted);
        // Цена шага: две стороны одной пары у эталона, две у обучаемой модели, два обратных
        // прохода. Прежде было по четыре, то есть вдвое дороже при том же результате обучения.
        Assert.Equal(2, snapshot.ReferenceForward.CompletedCalls);
        Assert.Equal(2, snapshot.PolicyForward.CompletedCalls);
        Assert.Equal(2, snapshot.Backward.CompletedCalls);
        // Обновитель весов вызывается один раз на весь набор — это не менялось.
        Assert.Equal(1, snapshot.ApplyOptimizer.CompletedCalls);
    }

    // ── Требование 3: пустой активный набор — случай без обновления ────────────

    [Fact]
    public void SetWhereEveryWeightIsZero_ReportsNoUpdateInsteadOfTakingAStep()
    {
        var root = NewRoot("all-weights-zero");
        var fixture = Fixture();

        // Набор годен и допущен, но ни одна его пара не просит себя учесть.
        var pairs = new[]
        {
            Pair(fixture.Tokenizer, "a", "b", "cc", weight: 0f),
            Pair(fixture.Tokenizer, "d", "ee", "f", weight: 0f)
        };

        var trainer = Trainer(fixture, root, pairs[0].Batch);
        var weightsBefore = Bytes(fixture.Policy);
        var optimizerBefore = trainer.ExportOptimizerState().ToBytes();

        var metrics = trainer.StepVector(pairs, 1);

        // Результат ГОВОРИТ, что обновления не было: это отдельное поле, а не нули в показаниях.
        Assert.NotNull(metrics.UpdateApplied);
        Assert.False(metrics.UpdateApplied!.Value);
        // Номер шага в квитанции сохранён: видно, какой именно шаг не дал обновления.
        Assert.Equal(1, metrics.Step);
        // Накопленных величин нет, потому что ни одна пара не считалась.
        Assert.Equal(0f, metrics.Loss);
        Assert.Equal(0f, metrics.RewardMargin);

        // Веса не изменились ни на бит.
        Assert.Equal(weightsBefore, Bytes(fixture.Policy));
        // Обновитель весов не вызывался: его состояние и номер шага прежние.
        Assert.Equal(optimizerBefore, trainer.ExportOptimizerState().ToBytes());
        Assert.Equal(0, trainer.ExportOptimizerState().Step);
        // Публикации завершённого шага не было: ни файла контрольной точки, ни файла показаний.
        Assert.False(File.Exists(Path.Combine(root, DpoTrainer.BuildPolicyCheckpointFileName(1))));
        Assert.False(File.Exists(Path.Combine(root, DpoTrainer.BuildMetricsFileName(1))));
    }

    // ── Оснастка ───────────────────────────────────────────────────────────────

    // Pair собирает одну допущенную учебную пару с ЯВНО названным весом. Допуск оформляется
    // продуктовым подготовщиком точных пар: вес не заменяет допуск, и исключённая по весу пара
    // остаётся допущенной парой.
    private static DpoVectorBatch Pair(
        BpeTokenizer tokenizer,
        string prompt,
        string chosen,
        string rejected,
        float weight)
        => new(
            DpoExactPairMaterial.CreateBatch(
                tokenizer,
                prompt,
                chosen,
                rejected,
                DpoTrainerPairCustoms.CreateAdmitted(
                    prompt,
                    chosen,
                    rejected,
                    DpoPairSideAuthorKind.DeterministicGenerator,
                    DpoPairSideAuthorKind.DeterministicGenerator,
                    "simulation").Admission),
            [1f, 0f],
            weight);

    // Tokenizer строит крошечный словарь кусочков слов инженерной оснастки.
    private static BpeTokenizer Tokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("fixture", 256);
        return tokenizer;
    }

    // PlainModel строит модель с детерминированными начальными значениями: два старта,
    // построенные этим способом, побайтово одинаковы.
    private static LlmModel PlainModel()
    {
        var model = new LlmModel(Config);
        var random = new Random(73);
        foreach (var parameter in model.Parameters())
            for (var i = 0; i < parameter.Value.Data.Length; i++)
                parameter.Value.Data[i] = parameter.Value.Shape.Rank == 1
                    ? 1
                    : (float)(random.NextDouble() - .5) * .15f;

        return model;
    }

    // ModelFixture — один независимый старт: обучаемая модель, замороженный эталон, словарь,
    // настройки надстроек, источник случайности и сами надстройки.
    private sealed record ModelFixture(
        LlmModel Policy,
        LlmModel Reference,
        BpeTokenizer Tokenizer,
        LoraConfig Lora,
        LoraPairRandomSeedAuthority Seeds,
        IReadOnlyList<LoraLinear> Adapters);

    // Fixture строит такой независимый старт.
    private static ModelFixture Fixture()
    {
        var policy = PlainModel();
        var reference = PlainModel();
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        var seeds = new LoraPairRandomSeedAuthority(73, "tiny.policy", "tiny.reference");
        var applied = LoraApplicator.ApplyFreshSeededPair(
            policy, reference, lora,
            seeds.RootInitializationSeed,
            seeds.PolicyDropoutStreamDomain,
            seeds.ReferenceDropoutStreamDomain);
        // Эталон заморожен: его параметры не получают градиента ни на одном шаге.
        foreach (var parameter in reference.Parameters()) parameter.Value.RequiresGrad = false;
        return new(policy, reference, Tokenizer(), lora, seeds, applied.FirstModelAdapters);
    }

    // Trainer собирает продуктового обучателя для этого старта. Профиль исполнения снимается с
    // ТОЙ ЖЕ пары, которой шаг и будет обучать, чтобы профиль описывал настоящий материал.
    private static DpoTrainer Trainer(ModelFixture fixture, string path, DpoBatch profileBatch)
    {
        var recipient = DpoProjectLoraRecipient.Capture(
            fixture.Policy, fixture.Reference, fixture.Adapters, fixture.Lora);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds);
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        return DpoTrainer.CreateForProjectLora(
            fixture.Policy,
            fixture.Reference,
            path,
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient,
            DpoExecutionProfile.Capture(profileBatch, recipient.Bindings),
            null,
            null,
            step => fixture.Policy.CaptureRandomState(topology, identity, step),
            state => fixture.Policy.RestoreRandomState(state, new(topology, identity)));
    }

    // Bytes снимает полный снимок состояния модели: именно по нему сравниваются плечи.
    private static byte[] Bytes(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return stream.ToArray();
    }

    // NewRoot создаёт отдельный каталог инженерной проверки под деревом артефактов обучения.
    private static string NewRoot(string name)
    {
        var repository = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        // Корень хранилища опознаётся по файлу проекта обучения предпочтениями.
        Assert.True(File.Exists(Path.Combine(repository, "11.DPO", "DPO.csproj")));
        var root = Path.Combine(
            repository, "training-artifacts", "engineering-checks",
            "dpo-zero-weight-exclusion-20261102", name + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    // TestLease выдаёт разрешение на изменение весов для инженерной оснастки.
    private sealed class TestLease : IDpoMutationAdmissionIssuer
    {
        public IDisposable AcquireDpoTrainerMutationPermit() => new CancellationTokenSource();
        public void Dispose() { }
    }
}
