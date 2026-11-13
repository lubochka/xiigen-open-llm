// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Остаток приёмки переноса «логиты неподвижной опорной модели вычисляются один
// раз» (карточка 13-dpo-reference-cache.md, поправка от 27 октября на строке 80:
// «состояние случайного потока сравнивать по полям заголовка и каждой записи, а
// не по текстовой записи; для проверки сравнителя брать копии уже снятых
// снимков, ноль дополнительных обновлений и прямых проходов»).
//
// Что уже закрыто другими проверками и здесь НЕ повторяется. Чувствительность
// сравнителя к подменам ВНУТРИ записей и измерение слепоты прежнего текстового
// сравнения живут в `Tests/DpoReferenceScoreCacheTests.cs`
// (`RandomStateFieldComparisonSensitivityTests`). Жизненный цикл памяти ответов
// на прогретом владельце — принятое восстановление, отклонённое восстановление и
// сбой уже начатого восстановления — живёт там же, у него своя учебная доза в
// три шага, и он сюда не переносится.
//
// Что закрывается здесь, и чего не было ни в одной проверке.
//
// Первое: заголовок снимка. Сравнитель его сравнивает шестью отдельными
// фактами, но ни одна проверка не показывала, что он действительно откажет при
// подмене одного только заголовка. Заодно измеряется граница прежнего дефекта:
// текстовая запись заголовок как раз ВИДЕЛА, и слепа она была ровно внутри
// записей. Без этого измерения легко решить, что прежнее сравнение не видело
// вообще ничего, и тем самым переоценить находку.
//
// Второе: цена сверки сохранённых копий. Прежние проверки цены считали её
// внутри прогретого сценария с тремя учебными шагами. Здесь сверка копий идёт
// отдельно, на холодном владельце, и её цена читается по действительным
// счётчикам продукта: ёмкость и четыре счётчика памяти ответов, счётчик шага
// оптимизатора и значения весов обучаемой модели. Ни один учебный шаг и ни один
// прямой проход здесь не вызывается вовсе.
//
// Третье: перенос позиции случайного потока через сохранение и восстановление —
// там, где поток вообще есть, — и честно названный предел там, где его нет.
//
// Предел продукта, который здесь назван прямо, а не додуман. При нулевой
// вероятности отсева позиция потока не хранится и не восстанавливается:
// `00.TensorCore/Layers/Dropout.cs:33-41` возвращает при `_p == 0f`
// фиксированную пару нулей, а `00.TensorCore/Layers/Dropout.cs:43-51` при
// `_p == 0f` молча выходит, ничего не восстановив. Поэтому
// `10.LoRA/LoraLinear.cs:128-143` и `10.LoRA/LoraLinear.cs:145-171` переносят в
// такой настройке постоянный ноль, а снимок модели
// (`03.TransformerModel/LlmModel.cs:286-319`) получает нулевую позицию у каждой
// записи. Учебная оснастка этого переноса создана именно с нулевым отсевом
// (карточка запрещает включать отсев ради утверждения), поэтому «последователь-
// ность воспроизводится после восстановления» на ней наблюдать нечем. Это
// свойство проверяется у владельца самого потока при ненулевом отсеве, без
// модели и без обучения, и его область названа в самой проверке.
//
// Бюджет. Ни одного учебного шага, ни одного прямого прохода через модель, ни
// одного обратного прохода, ни одного обновления оптимизатора. Третья проверка
// берёт случайность у отсеивающего слоя напрямую: это не проход модели.
//
// Чего это покрытие НЕ доказывает. Оно не измеряет качество обучения, не
// заявляет равенства расходуемой случайной траектории двух учебных прогонов, не
// проверяет продолжение с диска и не говорит ничего о попаданиях прогретой
// памяти ответов.

namespace LlmFromScratch.Tests.ExperimentFixes;

using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using TensorCore.Layers;
using TensorCore.Randomness;
using Tokenizer;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;
using Xunit.Abstractions;

public sealed class DpoReferenceCacheSavedCopyComparisonCostTests
{
    // Самая маленькая пригодная модель: словарь байтов, короткий контекст, один слой.
    private static readonly ModelConfig Config = new()
    {
        VocabSize = 256, ContextLen = 8, HiddenDim = 8,
        NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16
    };

    // Зерно отсеивающего потока третьей проверки: закреплено, чтобы числа были одни и те же.
    private const ulong StreamSeed = 20261113UL;

    // Число значений в тензоре третьей проверки: один отсеиваемый элемент — одно взятое число.
    private const int StreamDraws = 64;

    // Вывод измерений попадает в журнал прогона, а не остаётся в голове исполнителя.
    private readonly ITestOutputHelper _output;

    public DpoReferenceCacheSavedCopyComparisonCostTests(ITestOutputHelper output) => _output = output;

    // Заголовок снимка: пять подмен, каждая по одному полю заголовка.
    //
    // Сравнитель обязан назвать именно подменённое поле. И отдельно измеряется
    // прежнее текстовое сравнение: заголовок оно видело, поэтому прежний дефект
    // был слепотой внутри записей, а не слепотой вообще. Снимки собираются
    // руками, без модели и без диска: цена этой проверки — ноль по построению.
    [Fact]
    public void HeaderSubstitutionsAreNamedByTheFieldComparisonAndTheOldTextComparisonSawExactlyThose()
    {
        // Исходный снимок один на все подмены заголовка.
        var expected = HandBuiltSnapshot();
        // Шапка измеряемой таблицы: по строке на каждую подмену заголовка.
        _output.WriteLine("| подмена заголовка | расхождений у сравнителя по полям | прежнее сравнение по тексту |");

        // Каждая подмена проверяется отдельно: имя поля в расхождении обязано совпасть.
        foreach (var (name, field, mutated) in HeaderSubstitutions(expected))
        {
            // Сравнитель по полям обязан назвать хотя бы одно расхождение.
            var differences = RandomStateFieldAssertions.Differences(expected, mutated);
            Assert.NotEmpty(differences);
            // И притом назвать именно подменённое поле заголовка, а не какое-то другое.
            Assert.Contains(differences, difference => difference.Contains(field, StringComparison.Ordinal));
            // Утверждающая обёртка на этой подмене обязана отказать договором, а не упасть сама.
            var failure = Assert.ThrowsAny<Exception>(
                () => RandomStateFieldAssertions.AssertEqual(expected, mutated));
            // Отказ обязан быть отказом именно этого договора сравнения.
            Assert.StartsWith(
                "Состояние случайного потока различается по полям:", failure.Message, StringComparison.Ordinal);

            // Измерение прежнего сравнения: текстовая запись печатает поля заголовка,
            // поэтому подмену заголовка она обязана была заметить. Ожидание выписано
            // от правила печати записи, а не снято с наблюдённого вывода.
            var oldTextComparisonSawIt = !string.Equals(
                expected.ToString(), mutated.ToString(), StringComparison.Ordinal);
            Assert.True(
                oldTextComparisonSawIt,
                $"подмена «{name}»: прежнее сравнение по тексту её не заметило, запись измерения нужно поправить");
            // Строка измеренной таблицы попадает в журнал прогона.
            _output.WriteLine($"| {name} | {differences.Count} | заметило |");
        }

        // Контраст, который и есть граница прежнего дефекта: та же текстовая запись
        // не видит подмену ВНУТРИ записи, хотя заголовок видела во всех пяти случаях.
        var cursorMutated = WithEntries(
            expected,
            expected.Entries[0] with { ConsumedCursor = expected.Entries[0].ConsumedCursor + 1 },
            expected.Entries[1]);
        // Прежнее сравнение считает такой снимок тем же самым: записи в текст не попадают.
        Assert.Equal(expected.ToString(), cursorMutated.ToString());
        // А сравнитель по полям называет именно позицию потока.
        Assert.Contains(
            RandomStateFieldAssertions.Differences(expected, cursorMutated),
            difference => difference.Contains("израсходованная позиция потока", StringComparison.Ordinal));
        // Итог измерения в журнал: пять заголовочных подмен видны, подмена внутри записи — нет.
        _output.WriteLine("| изменена позиция потока внутри записи | 1 | пропустило |");
    }

    // Сверка сохранённых копий: продукт отвергает подменённые и ничего не стоит.
    //
    // Отрицательные примеры берутся ровно те, что названы в карточке: всё
    // прежнее, изменена одна позиция потока; и отдельно — всё прежнее, изменён
    // один факт заголовка. Оба прогоняются через продуктовый путь допуска
    // продолжения, а не через оснастку проверки. Цена читается по действительным
    // счётчикам продукта.
    [Fact]
    public void ComparingSavedCopiesRefusesTheMutatedOnesAndCostsNoForwardPassAndNoUpdate()
    {
        // Временные каталоги результатов убираются после проверки.
        using var roots = new TempRoots();
        // Пара моделей, адаптеры, зёрна и токенизатор одной маленькой оснастки.
        var fixture = Fixture();
        // Получатель обучения — именно адаптеры обучаемой модели.
        var recipient = DpoProjectLoraRecipient.Capture(
            fixture.Policy, fixture.Reference, fixture.Adapters, fixture.Lora);
        // Отпечаток формы модели: им снимок привязан к этой топологии.
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds);
        // Отпечаток настройки адаптеров: им снимок привязан к этой настройке дообучения.
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        // Профиль исполнения снимается с настоящей пары точного материала.
        var profile = DpoExecutionProfile.Capture(Batches(fixture.Tokenizer)[0].Batch, recipient.Bindings);

        // Сохранённая копия состояния случайного потока: снята у настоящей модели.
        var savedRandom = fixture.Policy.CaptureRandomState(topology, identity, 1);
        // Сохранённая копия состояния оптимизатора того же получателя.
        var savedOptimizer = SavedOptimizerSnapshot(recipient);
        // Доказательство продолжения выдаётся на эту пару сохранённых копий.
        var proof = DpoContinuationProof.IssueVerified(
            profile, savedOptimizer, savedRandom, "fix-lc-saved-copy-comparison-cost", () => true);

        // Владелец с назначенной ёмкостью памяти ответов и настоящим швом случайного состояния.
        var owner = DpoTrainer.CreateForProjectLora(
            fixture.Policy, fixture.Reference, roots.Next("saved-copy-cost"),
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient, profile, proof, null,
            step => fixture.Policy.CaptureRandomState(topology, identity, step),
            state => fixture.Policy.RestoreRandomState(state, new(topology, identity)),
            2);

        // Состояние до сверки: ёмкость два и ни одного обращения к памяти ответов.
        Assert.Equal((2, 0, 0, 0, 0), owner.ReferenceScoreCacheCounters);
        // Шаг оптимизатора до сверки: ноль, потому что ни одного обновления не было.
        Assert.Equal(0, owner.ExportOptimizerState().Step);
        // Байты оптимизатора и значения весов запоминаются для сравнения после сверки.
        var optimizerBefore = owner.ExportOptimizerState().ToBytes();
        var weightsBefore = Values(fixture.Policy);

        // Первая копия: те же значения в отдельных экземплярах записей. Так
        // проверяется сравнение по значению, а не совпадение ссылок на набор.
        var identicalCopy = savedRandom with
        {
            Entries = savedRandom.Entries.Select(entry => entry with { }).ToList()
        };
        // Вторая копия: всё прежнее, изменена одна позиция потока.
        var cursorCopy = savedRandom with
        {
            Entries = savedRandom.Entries
                .Select((entry, index) => index == 0
                    ? entry with { ConsumedCursor = entry.ConsumedCursor + 1 }
                    : entry)
                .ToList()
        };
        // Третья копия: всё прежнее, изменён один факт заголовка — шаг снимка.
        var headerCopy = savedRandom with { RandomStateStep = savedRandom.RandomStateStep + 1 };

        // Договор сравнения и продуктовый допуск связываются прямо здесь: сначала
        // сравнитель называет расхождение, потом продукт обязан на нём отказать.
        Assert.Empty(RandomStateFieldAssertions.Differences(savedRandom, identicalCopy));
        Assert.Contains(
            RandomStateFieldAssertions.Differences(savedRandom, cursorCopy),
            difference => difference.Contains("израсходованная позиция потока", StringComparison.Ordinal));
        Assert.Contains(
            RandomStateFieldAssertions.Differences(savedRandom, headerCopy),
            difference => difference.Contains("шаг снимка", StringComparison.Ordinal));

        // Копия с одной изменённой позицией потока обязана быть отвергнута допуском.
        Assert.Throws<DpoMutationAdmissionException>(
            () => owner.TryRestoreContinuationState(cursorCopy, savedOptimizer));
        // Копия с одним изменённым фактом заголовка — тоже.
        Assert.Throws<DpoMutationAdmissionException>(
            () => owner.TryRestoreContinuationState(headerCopy, savedOptimizer));

        // Оба отказа случились до мутации: оптимизатор и веса не двигались.
        Assert.Equal(optimizerBefore, owner.ExportOptimizerState().ToBytes());
        Assert.Equal(weightsBefore, Values(fixture.Policy));

        // Неизменённая копия обязана быть принята: иначе отказы выше ничего не
        // различали бы и проверка была бы пустой.
        Assert.True(owner.TryRestoreContinuationState(identicalCopy, savedOptimizer));
        // Принятое восстановление перенесло шаг сохранённой копии, и только его.
        Assert.Equal(savedOptimizer.Step, owner.ExportOptimizerState().Step);
        // Веса обучаемой модели сверка копий не трогала ни в одном из трёх случаев.
        Assert.Equal(weightsBefore, Values(fixture.Policy));
        // Замороженный эталон остался замороженным: сверка копий его не касалась.
        Assert.All(fixture.Reference.Parameters(), parameter => Assert.False(parameter.Value.RequiresGrad));

        // Главное о цене: вся сверка трёх копий не стоила ни одного обращения к
        // памяти ответов эталона. Счётчики продукта остались ровно теми же.
        Assert.Equal((2, 0, 0, 0, 0), owner.ReferenceScoreCacheCounters);
        // Измеренная цена попадает в журнал прогона человеческой строкой.
        _output.WriteLine(
            "цена сверки трёх сохранённых копий: обращений к памяти ответов 0, попаданий 0,"
            + " промахов 0, записей 0; учебных шагов не вызывалось, значит прямых проходов"
            + " и обновлений оптимизатора ноль");
    }

    // Перенос позиции случайного потока: там, где поток есть, и предел там, где его нет.
    //
    // Область этой проверки названа прямо: это договор самого отсеивающего слоя,
    // владеющего потоком, а не утверждение о равенстве расходуемой случайности
    // внутри учебного шага. Модель здесь не участвует, обучения нет.
    [Fact]
    public void TheStreamPositionResumesWhereThereIsAStreamAndIsNotStoredWithoutDropout()
    {
        // Поток с ненулевым отсевом и закреплённым зерном: здесь позиция реально движется.
        var stream = new Dropout(0.25f, StreamSeed);
        // Вход из единиц: любое изменение на выходе создано только отсевом.
        var input = new Tensor(
            Enumerable.Repeat(1f, StreamDraws).ToArray(), new TensorShape(8, 8));

        // Первое взятие случайности: это и есть «начало потока».
        var firstDraw = stream.Forward(input, training: true).Data.ToArray();
        // Сохранённая копия состояния потока ровно после первого взятия.
        var saved = stream.CaptureDeterministicState();
        // Второе взятие: продолжение того же потока, а не его начало.
        var secondDraw = stream.Forward(input, training: true).Data.ToArray();
        // Возврат к сохранённой копии состояния.
        stream.RestoreDeterministicState(saved);
        // Третье взятие после возврата обязано повторить ровно второе.
        var afterRestore = stream.Forward(input, training: true).Data.ToArray();

        // Главное свойство: после восстановления последовательность воспроизводится.
        Assert.Equal(secondDraw, afterRestore);
        // И она отличается от начала потока, иначе «воспроизводится» было бы пустым словом.
        Assert.NotEqual(firstDraw, secondDraw);
        // «Начинается заново» — это другое: новый поток того же зерна даёт первое взятие.
        var freshStream = new Dropout(0.25f, StreamSeed);
        Assert.Equal(firstDraw, freshStream.Forward(input, training: true).Data.ToArray());

        // Позиция считается по взятым числам: одно число на каждый элемент входа.
        // Ожидание выведено из тела слоя, а не снято с наблюдённого вывода.
        Assert.Equal(new SplitMix64RandomState(StreamSeed, (ulong)StreamDraws), saved);
        // После возврата и третьего взятия позиция снова на двух порциях.
        Assert.Equal(
            new SplitMix64RandomState(StreamSeed, (ulong)(StreamDraws * 2)),
            stream.CaptureDeterministicState());

        // Названный предел продукта: при нулевом отсеве позиции нет вовсе.
        var withoutDropout = new Dropout(0f, StreamSeed);
        // Снятие даёт фиксированную пару нулей, а не зерно и позицию.
        Assert.Equal(new SplitMix64RandomState(0, 0), withoutDropout.CaptureDeterministicState());
        // Восстановление ненулевой позиции молча ничего не меняет.
        withoutDropout.RestoreDeterministicState(new SplitMix64RandomState(StreamSeed, 7UL));
        Assert.Equal(new SplitMix64RandomState(0, 0), withoutDropout.CaptureDeterministicState());

        // Та же оснастка переноса создана с нулевым отсевом, поэтому у снимка
        // настоящей модели позиция каждой записи — ноль. Это следствие предела
        // выше, и оно здесь проверено на настоящем снимке, а не предположено.
        var fixture = Fixture();
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds);
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        var snapshot = fixture.Policy.CaptureRandomState(topology, identity, 1);
        // Записей столько, сколько установленных адаптеров, и список не пуст.
        Assert.NotEmpty(snapshot.Entries);
        // У каждой записи позиция потока нулевая: расходовать поток нечем.
        Assert.All(snapshot.Entries, entry => Assert.Equal(0UL, entry.ConsumedCursor));
        // Наблюдение в журнал: зёрна записей печатаются как наблюдение, не как ожидание.
        foreach (var entry in snapshot.Entries)
            _output.WriteLine(
                $"запись «{entry.AdapterKey}»: область потока «{entry.StreamDomain}»,"
                + $" зерно установки {entry.InitializationSeed}, зерно отсева {entry.DropoutSeed},"
                + $" позиция потока {entry.ConsumedCursor} (наблюдение)");
    }

    /// <summary>Снимок, собранный руками: заголовок и две записи двух адаптеров.</summary>
    private static LlmModelRandomStateSnapshot HandBuiltSnapshot() => new(
        LlmModelRandomStateSnapshot.CurrentFormatVersion,
        LlmModelRandomStateSnapshot.CurrentAlgorithm,
        new List<LlmModelRandomStateEntry>
        {
            new("blocks.0.attn.q_proj", "policy.dropout", 11UL, 22UL, 33UL),
            new("blocks.0.attn.v_proj", "policy.dropout", 44UL, 55UL, 66UL)
        },
        "topology-sha256-of-the-fixture",
        "lora-configuration-sha256-of-the-fixture",
        2);

    /// <summary>Тот же снимок с другим набором записей и тем же заголовком.</summary>
    private static LlmModelRandomStateSnapshot WithEntries(
        LlmModelRandomStateSnapshot source,
        params LlmModelRandomStateEntry[] entries)
        => source with { Entries = new List<LlmModelRandomStateEntry>(entries) };

    /// <summary>Пять подмен заголовка: по одному факту заголовка на случай.</summary>
    private static IReadOnlyList<(string Name, string Field, LlmModelRandomStateSnapshot Mutated)>
        HeaderSubstitutions(LlmModelRandomStateSnapshot source) =>
    [
        ("изменена версия записи", "версия записи",
            source with { FormatVersion = source.FormatVersion + 1 }),
        ("изменено устройство потока", "устройство потока",
            source with { Algorithm = source.Algorithm + "-changed" }),
        ("изменён отпечаток топологии", "отпечаток топологии",
            source with { TopologySha256 = source.TopologySha256 + "-changed" }),
        ("изменён отпечаток настройки адаптеров", "отпечаток настройки адаптеров",
            source with { LoraConfigurationSha256 = source.LoraConfigurationSha256 + "-changed" }),
        ("изменён шаг снимка", "шаг снимка",
            source with { RandomStateStep = source.RandomStateStep + 1 })
    ];

    // Допустимая сохранённая копия состояния оптимизатора того же получателя:
    // настоящий ключ связки и настоящая форма параметра, моменты нулевые.
    private static AdamWPortableState SavedOptimizerSnapshot(DpoProjectLoraRecipient recipient)
    {
        // Первая связка получателя: её ключ и форма заведомо допустимы.
        var binding = recipient.Bindings[0];
        // Длина данных параметра: по ней создаются оба нулевых момента.
        var length = binding.Parameter.Value.Data.Length;
        // Снимок одного параметра на шаге один.
        return new AdamWPortableState(
            AdamWPortableState.CurrentFormatVersion,
            1,
            [new AdamWPortableParameterState(
                binding.Key,
                binding.Parameter.Value.Shape.Dimensions,
                new float[length],
                new float[length])]);
    }

    // Все числа модели в порядке объявления одним плоским списком.
    private static List<float> Values(LlmModel model)
    {
        // Накопитель значений всех параметров.
        var values = new List<float>();
        // Значения добавляются параметр за параметром.
        foreach (var parameter in model.Parameters())
            values.AddRange(parameter.Value.Data);
        return values;
    }

    // Одна маленькая оснастка: пара моделей, адаптеры с нулевым отсевом, зёрна, токенизатор.
    private sealed record ModelFixture(
        LlmModel Policy, LlmModel Reference, BpeTokenizer Tokenizer,
        LoraConfig Lora, LoraPairRandomSeedAuthority Seeds, IReadOnlyList<LoraLinear> Adapters);

    private static ModelFixture Fixture()
    {
        // Обучаемая и замороженная модели одного размера.
        var policy = PlainModel();
        var reference = PlainModel();
        // Настройка адаптеров ровно та, что в соседних проверках этого переноса.
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        // Закреплённые зёрна: пара адаптеров создаётся воспроизводимо.
        var seeds = new LoraPairRandomSeedAuthority(73, "tiny.policy", "tiny.reference");
        // Создание пары адаптеров — подготовка, а не обучение.
        var applied = LoraApplicator.ApplyFreshSeededPair(
            policy, reference, lora, seeds.RootInitializationSeed,
            seeds.PolicyDropoutStreamDomain, seeds.ReferenceDropoutStreamDomain);
        // Эталон заморожен: его параметры не получают градиента.
        foreach (var parameter in reference.Parameters())
            parameter.Value.RequiresGrad = false;
        return new(policy, reference, Tokenizer(), lora, seeds, applied.FirstModelAdapters);
    }

    // Две пары точного материала: нужны только для снятия профиля исполнения.
    private static IReadOnlyList<DpoVectorBatch> Batches(BpeTokenizer tokenizer)
        => new (string Prompt, string Chosen, string Rejected)[] { ("a", "b", "cc"), ("d", "ee", "f") }
            .Select(row => new DpoVectorBatch(
                DpoExactPairMaterial.CreateBatch(
                    tokenizer, row.Prompt, row.Chosen, row.Rejected,
                    DpoTrainerPairCustoms.CreateAdmitted(
                        row.Prompt, row.Chosen, row.Rejected,
                        DpoPairSideAuthorKind.DeterministicGenerator,
                        DpoPairSideAuthorKind.DeterministicGenerator, "simulation").Admission),
                [1f, 0f]))
            .ToArray();

    // Токенизатор обучается на крошечной строке: это подготовка данных.
    private static BpeTokenizer Tokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("fixture", 256);
        return tokenizer;
    }

    // Маленькая модель с воспроизводимым заполнением чисел.
    private static LlmModel PlainModel()
    {
        // Создание модели заданной конфигурации.
        var model = new LlmModel(Config);
        // Закреплённое зерно: одинаковые числа при каждом прогоне.
        var random = new Random(73);
        // Векторы смещений заполняются единицей, матрицы — малыми числами вокруг нуля.
        foreach (var parameter in model.Parameters())
            for (var i = 0; i < parameter.Value.Data.Length; i++)
                parameter.Value.Data[i] = parameter.Value.Shape.Rank == 1
                    ? 1
                    : (float)(random.NextDouble() - .5) * .15f;
        return model;
    }

    // Выданное сервером разрешение в проверке: настоящий внешний допуск здесь не нужен.
    private sealed class TestLease : IDpoMutationAdmissionIssuer
    {
        // Разрешение выдаётся как обычный освобождаемый объект.
        public IDisposable AcquireDpoTrainerMutationPermit() => new CancellationTokenSource();

        // Собственных ресурсов у этой выдачи нет.
        public void Dispose()
        {
        }
    }

    // Временные каталоги результатов, которые убираются после проверки.
    private sealed class TempRoots : IDisposable
    {
        // Общий корень всех каталогов одной проверки.
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "fix-lc-cost-" + Guid.NewGuid().ToString("N"));

        // Очередной именованный каталог внутри общего корня.
        public string Next(string name)
        {
            // Полный путь собирается от общего корня.
            var path = Path.Combine(_root, name);
            // Каталог создаётся заранее: создатель обучателя может ожидать его существования.
            Directory.CreateDirectory(path);
            return path;
        }

        // Уборка общего корня после проверки.
        public void Dispose()
        {
            try
            {
                // Корень удаляется целиком вместе со всем содержимым.
                if (Directory.Exists(_root))
                    Directory.Delete(_root, recursive: true);
            }
            catch (IOException)
            {
                // Временный каталог не должен ронять проверку.
            }
        }
    }
}
