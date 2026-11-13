// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using TensorCore.Layers;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using Xunit.Abstractions;

// Что здесь проверяется, по-человечески.
//
// Обучение предпочтениями показывает модели две версии ответа на один запрос и уводит её от
// худшей к лучшей. Чтобы оценить ответ, модель читает запрос вместе с ответом и предсказывает
// каждый следующий знак: по знаку с номером i предсказывается знак с номером i + 1.
//
// Что было неправильно. Модели подавали ВСЮ строку целиком, вместе с самым последним знаком,
// хотя предсказать по последнему знаку уже нечего — за ним ничего нет. Из-за этого в окно
// модели обязана была влезть вся строка, и урок длиной на один знак больше окна отклонялся
// целиком, ещё до первого прохода. Между «запрос плюс ответ» и окном модели терялась ровно
// одна позиция, и законный урок не получал ни одного обновления весов.
//
// Что стало. При назначенном профиле сдвинутого входа модель получает строку БЕЗ последнего
// знака, а целями остаются знаки со второго по последний. Причинное внимание смотрит только
// назад, поэтому ни одна нужная строка предсказаний от этого не меняется: последний знак
// нужен только как цель, и целью он и остаётся. В окно теперь обязаны влезть длина строки
// минус один знак, то есть допустима длина окно плюс один; длина окно плюс два по-прежнему
// отклоняется тем же кодом отказа по вместимости.
//
// Что измерено в опыте (`experiments-v2/RP01-MICRO/SC01`, попытка 0001). Для строки длиной 65
// при окне 64: обновлений 0 → 200. У первой учебной строки верных знаков 3 из 49 → 4 из 49,
// последняя целевая величина потерь ниже. Полных правильных ответов 0 из 2 → 0 из 2.
//
// Чего эти проверки НЕ доказывают. Они не доказывают улучшения качества ответов: в самом
// опыте ни один полный ответ не был приобретён, а по двум строкам величина потерь ответа и
// ранг правильной цифры стали хуже. Они не доказывают пользы по памяти или времени. Это ремонт
// вместимости входа, а не освоение задачи.
//
// Границы этих проверок. Назначенная карточкой тяжёлая проверка на настоящих обученных весах
// донора (200 обновлений, 824 прямых и 404 обратных прохода) здесь НЕ выполняется и не
// заявляется: она требует отдельного наряда с донором опыта. Здесь проверены договор входа и
// целей, граница вместимости, побайтовая неизменность пути без профиля и личность памяти
// ответов эталона и восстановленного продолжения.
//
// Чувствительность к поломке. Решающая проверка обязана падать на прежней логике: там строка
// длиной окно плюс один отклонялась до первого прохода, и обновлений было ноль.
//
// Почему случаев проверки четыре, а подслучаев договора тензоров шесть. Карточка назначает ОДИН
// случай с именем `ScTensorContracts`, и внутри него — шесть конечных подслучаев; выборка
// запускателя по признакам «работа FIX-SC и случай Contract» обязана находить РОВНО ОДИН случай,
// поэтому дробить его на шесть отдельных нельзя. Сам случай ведёт список пройденных подслучаев и
// в конце сверяет его дословно с шестью назначенными именами, так что их число и состав видны из
// прогона, а не со слов. Остальные три случая — решающее утверждение переноса, побайтовая
// неизменность пути без профиля и личность памяти ответов и восстановленного продолжения; их
// карточка числом шесть не ограничивает.
public sealed class DpoShiftedContextContractTests(ITestOutputHelper output) : IDisposable
{
    // Шесть назначенных карточкой подслучаев договора тензоров, в порядке карточки.
    private static readonly string[] AssignedTensorSubcases =
    [
        "1. число оцениваемых строк: полный вход против сдвинутого",
        "2. последняя цель отличается от предпоследней",
        "3. сумма против среднего",
        "4. нулевая маска",
        "5. несовместимая форма",
        "6. прежний полный вход"
    ];

    // Крошечная модель инженерной оснастки: обучающих заявлений на ней не делается.
    // Окно нарочно маленькое: именно на его границе и живёт проверяемый ремонт.
    private static readonly ModelConfig NarrowConfig = new()
    {
        VocabSize = 256,
        ContextLen = 8,
        HiddenDim = 8,
        NumLayers = 1,
        NumHeads = 2,
        NumKvHeads = 1,
        FfnHiddenDim = 16
    };

    // Широкое окно для проверок, которым граница вместимости не нужна: они не должны зависеть
    // от неё, иначе поломка одной правки покраснила бы сразу несколько проверок.
    private static readonly ModelConfig WideConfig = new()
    {
        VocabSize = 256,
        ContextLen = 16,
        HiddenDim = 8,
        NumLayers = 1,
        NumHeads = 2,
        NumKvHeads = 1,
        FfnHiddenDim = 16
    };

    // Отдельный каталог результатов шага на каждый прогон этого класса; в дереве проекта он
    // ничего не оставляет.
    private readonly string _root = Path.Combine(
        Path.GetTempPath(), $"dpo-shifted-context-{Guid.NewGuid():N}");

    public void Dispose()
    {
        // Каталог результатов удаляется целиком: проверка не оставляет за собой мусора.
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    // ── Решающая проверка: сдвинутый вход, согласованные цели и граница окна ───────

    [Fact]
    [Trait("PlanId", "FIX-SC")]
    [Trait("Case", "Transfer")]
    public void ShiftedProfile_FeedsPrefixScoresShiftedTargetsAndAcceptsOneBeyondTheWindow()
    {
        // Строка ровно на один знак длиннее окна: запрос 4 знака, ответ 5 знаков, окно 8.
        // Именно эта длина раньше отклонялась целиком, не получив ни одного обновления.
        var tokenizer = Tokenizer();
        var oneBeyondWindow = Pair(tokenizer, "abcd", "efghi", "efghj");
        var material = oneBeyondWindow.Batch.ExactMaterial!;
        Assert.Equal(NarrowConfig.ContextLen + 1, material.SequenceLength);

        // Плечо без профиля: прежний полный вход. Урок отклоняется по вместимости окна, и
        // отказ приходит ДО первого прохода модели — веса и обновитель остаются прежними.
        var full = Fixture(NarrowConfig);
        var fullTrainer = Trainer(full, Path.Combine(_root, "full-refusal"),
            oneBeyondWindow.Batch, DpoExecutionProfile.FullSequenceInputVersion);
        var weightsBeforeRefusal = Bytes(full.Policy);
        var refusal = Assert.Throws<DpoMutationAdmissionException>(
            () => fullTrainer.StepVector([oneBeyondWindow], 1));
        Assert.Equal(DpoExactPairMaterial.CapacityErrorCode, refusal.ErrorCode);
        Assert.Equal(weightsBeforeRefusal, Bytes(full.Policy));
        Assert.Equal(0, fullTrainer.ExportOptimizerState().Step);
        Assert.False(File.Exists(Path.Combine(_root, "full-refusal", DpoTrainer.BuildMetricsFileName(1))));

        // Плечо со сдвинутым профилем: тот же урок, та же модель, то же окно.
        var shifted = Fixture(NarrowConfig);
        var shiftedTrainer = Trainer(shifted, Path.Combine(_root, "shifted-accept"),
            oneBeyondWindow.Batch, DpoExecutionProfile.ShiftedPrefixInputVersion);

        // Первое утверждение договора: модель получила ПРЕФИКС. Строк предсказаний ровно на
        // одну меньше, чем знаков, — значит последний знак модели не подавался.
        var graph = PolicyGraph(shiftedTrainer, shifted.Policy, oneBeyondWindow.Batch, chosen: true);
        Assert.Equal(material.SequenceLength - 1, GraphLogits(graph).Shape.Dimensions[1]);

        // Второе утверждение: граф сохранил ПОЛНЫЕ знаки и полную маску ответа, то есть
        // последняя цель не пропала вместе с последней позицией входа.
        Assert.Equal(material.SequenceLength, GraphIds(graph).Shape.Dimensions[1]);
        Assert.Equal(material.SequenceLength, GraphMask(graph).Shape.Dimensions[1]);
        Assert.Equal(1f, GraphMask(graph).Data[material.SequenceLength - 1]);

        // Третье утверждение: оценка собрана как вход с номером i против цели с номером i + 1.
        // Независимый эталон считает это сам: он сам строит префикс, сам прогоняет модель и
        // сам берёт для каждой цели ответа строку предсказаний на единицу раньше.
        var prefixLogits = PrefixForward(shifted.Policy, oneBeyondWindow.Batch.ChosenIds);
        var responseTargets = Enumerable
            .Range(material.PromptLength, material.ChosenLength - material.PromptLength)
            .Select(position => (Row: position - 1, Token: (int)oneBeyondWindow.Batch.ChosenIds.Data[position]))
            .ToArray();
        var expected = IndependentSum(prefixLogits.Data, NarrowConfig.VocabSize, responseTargets);

        // Четвёртое утверждение сразу: последняя цель действительно участвует. Дефектное
        // поведение — потерять её — дало бы эталон без последней цели, и сверка требует, чтобы
        // верное и дефектное числа расходились на порядки сильнее допуска сравнения.
        var withoutLastTarget = IndependentSum(prefixLogits.Data, NarrowConfig.VocabSize,
            responseTargets[..^1]);
        AssertClose(expected, GraphLogProbability(graph), withoutLastTarget,
            "оценка выбранного ответа на сдвинутом входе против потери последней цели");

        // Пятое утверждение: шаг действительно состоялся. Веса изменились, обновитель сделал
        // один шаг, и показания шага опубликованы.
        var weightsBefore = Bytes(shifted.Policy);
        var metrics = shiftedTrainer.StepVector([oneBeyondWindow], 1);
        Assert.Equal(1, metrics.Step);
        Assert.NotEqual(weightsBefore, Bytes(shifted.Policy));
        Assert.Equal(1, shiftedTrainer.ExportOptimizerState().Step);
        Assert.True(File.Exists(Path.Combine(_root, "shifted-accept", DpoTrainer.BuildMetricsFileName(1))));

        // Шестое утверждение: граница не уехала дальше. Строка длиной окно плюс два всё ещё
        // отклоняется, и тем же кодом отказа по вместимости.
        var twoBeyondWindow = Pair(tokenizer, "abcd", "efghij", "efghik");
        Assert.Equal(NarrowConfig.ContextLen + 2, twoBeyondWindow.Batch.ExactMaterial!.SequenceLength);
        var tooLong = Fixture(NarrowConfig);
        var tooLongTrainer = Trainer(tooLong, Path.Combine(_root, "shifted-too-long"),
            oneBeyondWindow.Batch, DpoExecutionProfile.ShiftedPrefixInputVersion);
        var weightsBeforeTooLong = Bytes(tooLong.Policy);
        var overflow = Assert.Throws<DpoMutationAdmissionException>(
            () => tooLongTrainer.StepVector([twoBeyondWindow], 1));
        Assert.Equal(DpoExactPairMaterial.CapacityErrorCode, overflow.ErrorCode);
        Assert.Equal(weightsBeforeTooLong, Bytes(tooLong.Policy));
        Assert.Equal(0, tooLongTrainer.ExportOptimizerState().Step);
    }

    // ── Побайтовая неизменность пути без профиля ───────────────────────────────────

    [Fact]
    [Trait("PlanId", "FIX-SC")]
    [Trait("Case", "Transfer")]
    public void WithoutShiftedProfile_ArithmeticAndTrainedStateStayByteIdentical()
    {
        // Первая половина: сама арифметика оценки. Эталон здесь — ДОСЛОВНОЕ воспроизведение
        // прежнего цикла до правки: число строк он берёт из числа строк модели, а не из длины
        // знаков, и считает в том же порядке теми же дробными действиями. Совпадение обязано
        // быть побайтовым, а не приблизительным.
        var ids = Ids([3, 1, 2, 0]);
        var mask = Mask([0f, 1f, 1f, 1f]);
        var logits = Logits(rows: 4, vocab: 4);
        Assert.Equal(OriginalLoopScore(logits, ids, mask, sumTargets: true),
            LegacyScore(logits, ids, mask, sum: true));
        Assert.Equal(OriginalLoopScore(logits, ids, mask, sumTargets: false),
            LegacyScore(logits, ids, mask, sum: false));

        // Вторая половина: целый шаг обучения. Два независимых, но одинаково построенных
        // старта; обучатель первого создан прежней формой вызова без новой настройки, второго —
        // с явно названным прежним полным входом. Обученное состояние обязано совпасть
        // побайтово: добавленная настройка со своим значением по умолчанию не меняет ничего.
        var tokenizer = Tokenizer();
        var pair = Pair(tokenizer, "ab", "cde", "cdf");

        var legacyCall = Fixture(WideConfig);
        var legacyTrainer = LegacyShapeTrainer(legacyCall, Path.Combine(_root, "legacy-call"), pair.Batch);
        var legacyMetrics = legacyTrainer.StepVector([pair], 1);

        var namedCall = Fixture(WideConfig);
        var namedTrainer = Trainer(namedCall, Path.Combine(_root, "named-call"), pair.Batch,
            DpoExecutionProfile.FullSequenceInputVersion);
        var namedMetrics = namedTrainer.StepVector([pair], 1);

        Assert.Equal(Bytes(legacyCall.Policy), Bytes(namedCall.Policy));
        Assert.Equal(legacyTrainer.ExportOptimizerState().ToBytes(), namedTrainer.ExportOptimizerState().ToBytes());
        Assert.Equal(legacyMetrics.Loss, namedMetrics.Loss);
        Assert.Equal(legacyMetrics.RewardMargin, namedMetrics.RewardMargin);
        Assert.Equal(legacyMetrics.ChosenReward, namedMetrics.ChosenReward);
        Assert.Equal(legacyMetrics.RejectedReward, namedMetrics.RejectedReward);
        Assert.Equal(legacyMetrics.ObjectiveVersion, namedMetrics.ObjectiveVersion);

        // Личность исполнения у прежнего пути тоже прежняя: профиль, снятый прежней формой
        // вызова, описывает полный вход.
        Assert.Equal(DpoExecutionProfile.FullSequenceInputVersion,
            DpoExecutionProfile.Capture(pair.Batch, Recipient(legacyCall).Bindings).InputVersion);
    }

    // ── Личность памяти ответов эталона и восстановленного продолжения ─────────────

    [Fact]
    [Trait("PlanId", "FIX-SC")]
    [Trait("Case", "Transfer")]
    public void InputVersion_IsPartOfCacheKeyAndOfRestoredContinuationIdentity()
    {
        var tokenizer = Tokenizer();
        var pair = Pair(tokenizer, "ab", "cde", "cdf");

        // Память ответов эталона различает вход. Два ключа отличаются ТОЛЬКО входом, и этого
        // достаточно, чтобы ответ, снятый на полном входе, не был выдан за ответ на префиксе.
        var onFullInput = CacheKey(pair.Batch, "reference-identity", DpoExecutionProfile.FullSequenceInputVersion);
        var onPrefix = CacheKey(pair.Batch, "reference-identity", DpoExecutionProfile.ShiftedPrefixInputVersion);
        Assert.False(onFullInput.Same(onPrefix));
        Assert.True(onFullInput.Same(
            CacheKey(pair.Batch, "reference-identity", DpoExecutionProfile.FullSequenceInputVersion)));

        var memory = new DpoReferenceScoreCache(2);
        memory.Store(onFullInput, -1.5f, -2.5f);
        Assert.False(memory.TryGet(onPrefix, out _, out _));
        Assert.True(memory.TryGet(onFullInput, out var chosen, out var rejected));
        Assert.Equal(-1.5f, chosen);
        Assert.Equal(-2.5f, rejected);

        // Под сдвинутым профилем память работает как обычно: второй шаг на той же паре читает
        // уже снятый ответ эталона и не вызывает модель заново.
        var reuse = Fixture(WideConfig);
        var reuseTrainer = Trainer(reuse, Path.Combine(_root, "cache-reuse"), pair.Batch,
            DpoExecutionProfile.ShiftedPrefixInputVersion);
        reuseTrainer.StepVector([pair], 1);
        var afterFirst = reuseTrainer.ReferenceScoreCacheCounters;
        reuseTrainer.StepVector([pair], 2);
        var afterSecond = reuseTrainer.ReferenceScoreCacheCounters;
        Assert.Equal(0, afterFirst.Hits);
        Assert.Equal(1, afterFirst.Stores);
        Assert.Equal(1, afterSecond.Hits);
        Assert.Equal(1, afterSecond.Stores);

        // Восстановленное продолжение несёт свою версию входа. Квитанция прежнего формата
        // версию входа не записывала, поэтому восстановленный профиль описывает полный вход и
        // сдвинутому исполнению не подходит.
        var recipient = Recipient(reuse);
        var fullProfile = DpoExecutionProfile.Capture(pair.Batch, recipient.Bindings);
        var shiftedProfile = DpoExecutionProfile.Capture(pair.Batch, recipient.Bindings,
            DpoExecutionProfile.ShiftedPrefixInputVersion);
        var restoredFromOldReceipt = DpoExecutionProfile.FromVerifiedManifest(
            fullProfile.MaterialVersion, fullProfile.FramingVersion, fullProfile.ObjectiveVersion,
            fullProfile.AllowedParameterMap.ToArray());
        Assert.Equal(DpoExecutionProfile.FullSequenceInputVersion, restoredFromOldReceipt.InputVersion);
        Assert.True(restoredFromOldReceipt.Matches(fullProfile));
        Assert.False(restoredFromOldReceipt.Matches(shiftedProfile));

        // Продолжение, снятое на полном входе, не может стать началом сдвинутого исполнения.
        var proofOnFullInput = DpoContinuationProof.IssueVerified(
            fullProfile, reuseTrainer.ExportOptimizerState(), null, "shifted-context-contract", () => true);
        var mismatchedProof = Fixture(WideConfig);
        Assert.Equal(DpoExecutionProfile.MismatchErrorCode,
            Assert.Throws<DpoMutationAdmissionException>(() => Trainer(mismatchedProof,
                Path.Combine(_root, "mismatched-proof"), pair.Batch,
                DpoExecutionProfile.ShiftedPrefixInputVersion, proofOnFullInput)).ErrorCode);

        // И наоборот: объявленный профиль не может расходиться с входом самого исполнения.
        var mismatchedProfile = Fixture(WideConfig);
        var mismatchedRecipient = Recipient(mismatchedProfile);
        Assert.Equal(DpoExecutionProfile.MismatchErrorCode,
            Assert.Throws<DpoMutationAdmissionException>(() => DpoTrainer.CreateForProjectLora(
                mismatchedProfile.Policy, mismatchedProfile.Reference,
                Path.Combine(_root, "mismatched-profile"),
                DpoMutationAdmissionCapability.Issue(new TestLease()), mismatchedRecipient,
                DpoExecutionProfile.Capture(pair.Batch, mismatchedRecipient.Bindings),
                null, null, null, null,
                DpoTrainer.DefaultReferenceScoreCacheCapacity,
                DpoExecutionProfile.ShiftedPrefixInputVersion)).ErrorCode);

        // Выдуманное имя входа исполнением не становится.
        Assert.Equal(DpoExecutionProfile.MismatchErrorCode,
            Assert.Throws<DpoMutationAdmissionException>(() => DpoExecutionProfile.FromVerifiedManifest(
                fullProfile.MaterialVersion, fullProfile.FramingVersion, fullProfile.ObjectiveVersion,
                fullProfile.AllowedParameterMap.ToArray(), "invented-input-v9")).ErrorCode);
    }

    // ── Договор тензоров: шесть конечных подслучаев без единого вызова модели ──────

    [Fact]
    [Trait("PlanId", "FIX-SC")]
    [Trait("Case", "Contract")]
    public void ScTensorContracts()
    {
        // Искусственные знаки, маска ответа и предсказания: модель здесь не вызывается ни разу.
        var ids = Ids([3, 1, 2, 0]);
        var responseMask = Mask([0f, 0f, 1f, 1f]);
        var fullRows = Logits(rows: 4, vocab: 4);
        var prefixRows = Logits(rows: 3, vocab: 4);

        // Список пройденных подслучаев: в конце он сверяется с шестью назначенными именами,
        // поэтому их число и состав видны из прогона, а не со слов.
        var passed = new List<string>();

        // Подслучай 1 — общий счёт оцениваемых строк. Их всегда длина знаков минус одна, и при
        // полном входе, и при сдвинутом; различается только число строк, которые дала модель.
        Assert.Equal(3, RowCount(fullRows, ids, responseMask, shiftedInput: false));
        Assert.Equal(3, RowCount(prefixRows, ids, responseMask, shiftedInput: true));
        passed.Add(AssignedTensorSubcases[0]);

        // Подслучай 2 — последняя цель жива при входе на один знак короче. Снятие ТОЛЬКО
        // последней цели обязано изменить оценку, и разница равна вкладу этой цели.
        var lastDropped = Mask([0f, 0f, 1f, 0f]);
        var withLast = ShiftedScore(prefixRows, ids, responseMask, sum: true);
        var withoutLast = ShiftedScore(prefixRows, ids, lastDropped, sum: true);
        Assert.NotEqual(withLast, withoutLast);
        // Дефектное поведение — потерять последнюю цель — дало бы разницу РОВНО ноль, поэтому
        // различаемая величина здесь равна самому вкладу последней цели. Сверка сама требует,
        // чтобы он был на порядки больше допуска, и пишет оба числа в журнал прогона.
        AssertClose(RowLogProbability(prefixRows, row: 2, vocab: 4, token: (int)ids.Data[3]),
            withLast - withoutLast, 0f, "вклад последней цели ответа против её потери");

        // И обратный проход доходит до той же последней строки: при прежней границе цикла
        // строка номер два градиента не получала бы вовсе.
        var backwardRows = Logits(rows: 3, vocab: 4);
        ShiftedBackward(backwardRows, ids, responseMask, withLast, sum: true, upstream: 1f);
        Assert.NotNull(backwardRows.Grad);
        Assert.Contains(backwardRows.Grad!.Data[(2 * 4)..(3 * 4)], value => value != 0f);
        passed.Add(AssignedTensorSubcases[1]);

        // Подслучай 3 — сумма против среднего. Целей ровно две, поэтому среднее равно половине
        // суммы; обе ветви читают одни и те же строки. Дефектное поведение — не разделить на
        // число целей — дало бы саму сумму, поэтому различаемая величина равна половине суммы.
        var meanOfTwo = ShiftedScore(prefixRows, ids, responseMask, sum: false);
        AssertClose(withLast / 2f, meanOfTwo, withLast,
            "среднее по двум целям против неразделённой суммы");
        passed.Add(AssignedTensorSubcases[2]);

        // Подслучай 4 — нулевая маска. Оценивать нечего: обе ветви дают ноль, а обратный проход
        // оставляет градиент нулевым и не идёт дальше по графу.
        var emptyMask = Mask([0f, 0f, 0f, 0f]);
        Assert.Equal(0f, ShiftedScore(prefixRows, ids, emptyMask, sum: true));
        Assert.Equal(0f, ShiftedScore(prefixRows, ids, emptyMask, sum: false));
        var untouched = Logits(rows: 3, vocab: 4);
        ShiftedBackward(untouched, ids, emptyMask, 0f, sum: true, upstream: 1f);
        Assert.All(untouched.Grad!.Data, value => Assert.Equal(0f, value));
        passed.Add(AssignedTensorSubcases[3]);

        // Подслучай 5 — несовместимая форма. Полный вход там, где назначен сдвинутый, и
        // наоборот, и маска другой длины — всё это останавливает счёт названным кодом отказа.
        Assert.Contains(DpoTrainer.TargetRowCountErrorCode,
            Assert.Throws<ArgumentException>(() => RowCount(fullRows, ids, responseMask, shiftedInput: true)).Message);
        Assert.Contains(DpoTrainer.TargetRowCountErrorCode,
            Assert.Throws<ArgumentException>(() => RowCount(prefixRows, ids, responseMask, shiftedInput: false)).Message);
        Assert.Contains(DpoTrainer.TargetRowCountErrorCode,
            Assert.Throws<ArgumentException>(() => RowCount(fullRows, ids, Mask([0f, 1f, 1f]), shiftedInput: false)).Message);
        passed.Add(AssignedTensorSubcases[4]);

        // Подслучай 6 — прежний полный вход сохранён. Оценка на полном входе совпадает
        // ПОБАЙТОВО с дословным воспроизведением прежнего цикла, и в сумме, и в среднем.
        // Здесь допуска нет вовсе: сравниваются сами дробные числа, бит в бит.
        var legacyMask = Mask([0f, 1f, 1f, 1f]);
        Assert.Equal(OriginalLoopScore(fullRows, ids, legacyMask, sumTargets: true),
            LegacyScore(fullRows, ids, legacyMask, sum: true));
        Assert.Equal(OriginalLoopScore(fullRows, ids, legacyMask, sumTargets: false),
            LegacyScore(fullRows, ids, legacyMask, sum: false));
        passed.Add(AssignedTensorSubcases[5]);

        // Назначенных карточкой подслучаев шесть, и пройдены именно они — состав и порядок
        // сверяются дословно, поэтому «четырёх случаев проверки» для договора тензоров быть
        // не может: случай один, подслучаев в нём шесть.
        Assert.Equal(AssignedTensorSubcases, passed);
        output.WriteLine($"Подслучаев договора тензоров пройдено {passed.Count} из " +
            $"{AssignedTensorSubcases.Length}: {string.Join("; ", passed)}");
    }

    // ── Независимые эталоны ────────────────────────────────────────────────────────

    // IndependentSum считает сумму логарифмов вероятностей названных целей сам, в двойной
    // точности и без обращения к продуктовым формулам.
    private static float IndependentSum(float[] logits, int vocab, (int Row, int Token)[] targets)
    {
        double sum = 0;
        foreach (var (row, token) in targets)
        {
            var window = logits.Skip(row * vocab).Take(vocab).ToArray();
            var maximum = window.Max();
            var denominator = window.Sum(value => Math.Exp(value - maximum));
            sum += logits[row * vocab + token] - maximum - Math.Log(denominator);
        }
        return (float)sum;
    }

    // RowLogProbability — логарифм вероятности одного названного знака в одной строке.
    private static float RowLogProbability(Tensor logits, int row, int vocab, int token)
        => IndependentSum(logits.Data, vocab, [(row, token)]);

    // Допуск сравнения с независимым эталоном: одна стотысячная. Он покрывает ТОЛЬКО разницу
    // одинарной точности продукта и двойной точности эталона, и ничего больше.
    private const float IndependentOracleTolerance = 1e-5f;

    // Во сколько раз различаемая величина обязана превышать допуск, чтобы допуск нельзя было
    // обвинить в сокрытии дефекта. Тысяча — это три порядка запаса.
    private const float RequiredSeparationFactor = 1000f;

    // AssertClose делает две вещи сразу, и вторая важнее первой.
    //
    // Первая: сверяет продуктовое число с независимым эталоном в пределах допуска.
    //
    // Вторая: ДОКАЗЫВАЕТ, что сам допуск не прячет проверяемый дефект. Для этого вызывающий
    // обязан назвать `defectiveAlternative` — число, которое получилось бы при дефектном
    // поведении. Проверка требует, чтобы верное и дефектное числа расходились минимум в тысячу
    // раз сильнее допуска. Если бы разница между верным и дефектным поведением была сопоставима
    // с допуском, проверка падала бы САМА и требовала сравнивать другую величину, а не
    // ослаблять ожидание. Обе величины пишутся в журнал прогона, поэтому запас виден числами.
    private void AssertClose(float expected, float actual, float defectiveAlternative, string what)
    {
        var separation = MathF.Abs(expected - defectiveAlternative);
        var factor = separation / IndependentOracleTolerance;
        output.WriteLine(
            $"{what}: эталон {expected:R}; продукт {actual:R}; расхождение {MathF.Abs(expected - actual):R}; " +
            $"дефектное значение {defectiveAlternative:R}; различаемая величина {separation:R}; " +
            $"запас против допуска {IndependentOracleTolerance:R} — {factor:R} раз.");
        Assert.True(separation >= RequiredSeparationFactor * IndependentOracleTolerance,
            $"{what}: допуск {IndependentOracleTolerance:R} сопоставим с различаемой величиной {separation:R}; " +
            "сравнивать надо другую величину, а не ослаблять ожидание");
        Assert.True(MathF.Abs(expected - actual) <= IndependentOracleTolerance,
            $"{what}: ожидалось {expected:R}, получено {actual:R}, допуск {IndependentOracleTolerance:R}, " +
            $"различаемая величина {separation:R}");
    }

    // OriginalLoopScore — ДОСЛОВНОЕ воспроизведение цикла оценки до этой правки: число строк
    // берётся из числа строк модели, порядок дробных действий и сами действия те же. Это
    // эталон побайтовой неизменности пути без профиля, а не копия новой формулы.
    private static float OriginalLoopScore(Tensor logits, Tensor ids, Tensor mask, bool sumTargets)
    {
        int t = logits.Shape.Dimensions[1];
        int v = logits.Shape.Dimensions[2];
        float sum = 0f;
        float cnt = 0f;
        for (int i = 0; i < t - 1; i++)
        {
            float maskVal = mask.Data[i + 1];
            if (maskVal == 0f) continue;
            int targetId = (int)ids.Data[i + 1];
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
                if (logits.Data[i * v + j] > maxVal)
                    maxVal = logits.Data[i * v + j];
            float logSumExp = 0f;
            for (int j = 0; j < v; j++)
                logSumExp += MathF.Exp(logits.Data[i * v + j] - maxVal);
            logSumExp = MathF.Log(logSumExp);
            float logP = logits.Data[i * v + targetId] - maxVal - logSumExp;
            sum += logP * maskVal;
            cnt += maskVal;
        }
        return sumTargets ? sum : cnt > 0f ? sum / cnt : 0f;
    }

    // PrefixForward сам строит префикс без последнего знака и сам прогоняет его через модель:
    // продуктовый путь входа здесь не используется.
    private static Tensor PrefixForward(LlmModel model, Tensor ids)
    {
        var length = ids.Shape.Dimensions[1] - 1;
        var prefix = new float[length];
        Array.Copy(ids.Data, prefix, length);
        return model.Forward(new Tensor(prefix, new TensorShape(1, length)), training: false);
    }

    // ── Узкий отражающий доступ к настоящим продуктовым помощникам ─────────────────
    // Формулы в проверку не переносятся: берутся сами продуктовые помощники, поэтому
    // изменение продукта сдвигает проверяемое число.

    private static readonly MethodInfo ProductTargetRowCount =
        typeof(DpoTrainer).GetMethod("TargetRowCount", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("DPO_SHIFTED_CONTEXT_TARGET_ROW_COUNT_MISSING");

    private static readonly MethodInfo ProductScoreTargetLogProbs =
        typeof(DpoTrainer).GetMethod("ScoreTargetLogProbs", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("DPO_SHIFTED_CONTEXT_SCORE_MISSING");

    private static readonly MethodInfo ProductLegacyComputeTargetLogProbs =
        typeof(DpoTrainer).GetMethod("ComputeTargetLogProbs", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("DPO_SHIFTED_CONTEXT_LEGACY_SCORE_MISSING");

    private static readonly MethodInfo ProductBackwardLogProbs =
        typeof(DpoTrainer).GetMethod("BackwardLogProbs", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("DPO_SHIFTED_CONTEXT_BACKWARD_MISSING");

    private static readonly MethodInfo ProductForwardPolicyBatchLogProbs =
        typeof(DpoTrainer).GetMethod("ForwardPolicyBatchLogProbs", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new MissingMethodException("DPO_SHIFTED_CONTEXT_FORWARD_MISSING");

    private static readonly Type GraphType =
        typeof(DpoTrainer).GetNestedType("PolicyLogProbGraph", BindingFlags.NonPublic)
        ?? throw new MissingMethodException("DPO_SHIFTED_CONTEXT_GRAPH_MISSING");

    private static int RowCount(Tensor logits, Tensor ids, Tensor mask, bool shiftedInput)
        => Unwrap<int>(() => ProductTargetRowCount.Invoke(null, [logits, ids, mask, shiftedInput]));

    private static float ShiftedScore(Tensor logits, Tensor ids, Tensor mask, bool sum)
        => Unwrap<float>(() => ProductScoreTargetLogProbs.Invoke(null, [logits, ids, mask, sum, true]));

    private static float LegacyScore(Tensor logits, Tensor ids, Tensor mask, bool sum)
        => Unwrap<float>(() => ProductLegacyComputeTargetLogProbs.Invoke(null, [logits, ids, mask, sum]));

    private static void ShiftedBackward(Tensor logits, Tensor ids, Tensor mask, float score, bool sum, float upstream)
    {
        // Граф собирается прежними пятью позиционными значениями, а версия входа ставится
        // отдельным полем: именно так его собирает и продуктовый прямой проход.
        var graph = Activator.CreateInstance(GraphType, [logits, ids, mask, score, sum])!;
        GraphType.GetProperty("ShiftedInput", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(graph, true);
        Unwrap<object?>(() => ProductBackwardLogProbs.Invoke(null, [graph, upstream]));
    }

    private static object PolicyGraph(DpoTrainer trainer, LlmModel model, DpoBatch batch, bool chosen)
        => Unwrap<object>(() => ProductForwardPolicyBatchLogProbs.Invoke(trainer, [model, batch, chosen]))!;

    private static Tensor GraphLogits(object graph) => (Tensor)GraphType.GetProperty("Logits")!.GetValue(graph)!;
    private static Tensor GraphIds(object graph) => (Tensor)GraphType.GetProperty("Ids")!.GetValue(graph)!;
    private static Tensor GraphMask(object graph) => (Tensor)GraphType.GetProperty("Mask")!.GetValue(graph)!;
    private static float GraphLogProbability(object graph)
        => (float)GraphType.GetProperty("LogProbability")!.GetValue(graph)!;

    // Отражение заворачивает настоящую ошибку продукта; разворачиваем её, чтобы проверка
    // видела именно продуктовый отказ, а не обёртку отражения.
    private static T Unwrap<T>(Func<object?> call)
    {
        try { return (T)call()!; }
        catch (TargetInvocationException wrapped) when (wrapped.InnerException is not null)
        {
            throw wrapped.InnerException;
        }
    }

    // ── Оснастка ───────────────────────────────────────────────────────────────────

    // Ids и Mask собирают искусственные знаки и маску ответа формы один ряд на длину.
    private static Tensor Ids(float[] values) => new(values, new TensorShape(1, values.Length));
    private static Tensor Mask(float[] values) => new(values, new TensorShape(1, values.Length));

    // Logits собирает детерминированные предсказания: два вызова дают одинаковые числа.
    private static Tensor Logits(int rows, int vocab)
    {
        var data = new float[rows * vocab];
        for (var i = 0; i < data.Length; i++) data[i] = -0.2f + 0.007f * i;
        return new Tensor(data, new TensorShape(1, rows, vocab));
    }

    // CacheKey собирает ключ памяти ответов эталона ровно теми же полями, какими его собирает
    // продуктовый тренер.
    private static DpoReferenceScoreCache.Key CacheKey(DpoBatch batch, string referenceIdentity, string inputVersion)
        => new(batch.ExactMaterial!.DigestSha256, batch.ExactMaterial.TokenizerSha256,
            batch.ExactMaterial.TokenizerVocabSize, batch.MaterialVersion,
            DpoExactPairMaterial.FramingVersion, DpoExactPairMaterial.ObjectiveVersion,
            referenceIdentity, inputVersion);

    // Pair собирает одну допущенную учебную пару продуктовым подготовщиком точных пар.
    private static DpoVectorBatch Pair(BpeTokenizer tokenizer, string prompt, string chosen, string rejected)
        => new(
            DpoExactPairMaterial.CreateBatch(tokenizer, prompt, chosen, rejected,
                DpoTrainerPairCustoms.CreateAdmitted(prompt, chosen, rejected,
                    DpoPairSideAuthorKind.DeterministicGenerator,
                    DpoPairSideAuthorKind.DeterministicGenerator,
                    "shifted-context-contract").Admission),
            [1f, 0f],
            1f);

    // Tokenizer строит крошечный побайтовый словарь инженерной оснастки: один знак письма даёт
    // ровно один знак словаря, поэтому длины строк в проверке видны прямо в тексте.
    private static BpeTokenizer Tokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("fixture", 256);
        return tokenizer;
    }

    // PlainModel строит модель с детерминированными начальными значениями: два старта,
    // построенные этим способом, побайтово одинаковы.
    private static LlmModel PlainModel(ModelConfig config)
    {
        var model = new LlmModel(config);
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
        ModelConfig Config,
        LlmModel Policy,
        LlmModel Reference,
        BpeTokenizer Tokenizer,
        LoraConfig Lora,
        LoraPairRandomSeedAuthority Seeds,
        IReadOnlyList<LoraLinear> Adapters);

    // Fixture строит такой независимый старт.
    private static ModelFixture Fixture(ModelConfig config)
    {
        var policy = PlainModel(config);
        var reference = PlainModel(config);
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        var seeds = new LoraPairRandomSeedAuthority(73, "tiny.policy", "tiny.reference");
        var applied = LoraApplicator.ApplyFreshSeededPair(
            policy, reference, lora,
            seeds.RootInitializationSeed,
            seeds.PolicyDropoutStreamDomain,
            seeds.ReferenceDropoutStreamDomain);
        // Эталон заморожен: его параметры не получают градиента ни на одном шаге.
        foreach (var parameter in reference.Parameters()) parameter.Value.RequiresGrad = false;
        return new(config, policy, reference, Tokenizer(), lora, seeds, applied.FirstModelAdapters);
    }

    // Recipient — настоящие установленные надстройки этого старта и их привязки.
    private static DpoProjectLoraRecipient Recipient(ModelFixture fixture)
        => DpoProjectLoraRecipient.Capture(fixture.Policy, fixture.Reference, fixture.Adapters, fixture.Lora);

    // Trainer собирает продуктового обучателя с НАЗВАННОЙ версией входа. Профиль снимается с
    // той же пары, которой шаг и будет обучать, чтобы профиль описывал настоящий материал.
    private static DpoTrainer Trainer(ModelFixture fixture, string path, DpoBatch profileBatch,
        string inputVersion, DpoContinuationProof? proof = null)
    {
        var recipient = Recipient(fixture);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(fixture.Config, fixture.Lora, fixture.Seeds);
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        return DpoTrainer.CreateForProjectLora(
            fixture.Policy,
            fixture.Reference,
            path,
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient,
            DpoExecutionProfile.Capture(profileBatch, recipient.Bindings, inputVersion),
            proof,
            null,
            step => fixture.Policy.CaptureRandomState(topology, identity, step),
            state => fixture.Policy.RestoreRandomState(state, new(topology, identity)),
            DpoTrainer.DefaultReferenceScoreCacheCapacity,
            inputVersion);
    }

    // LegacyShapeTrainer собирает обучателя ПРЕЖНЕЙ формой вызова: ни версии входа, ни ёмкости
    // памяти ответов. Именно этот вызов и обязан остаться побайтово прежним.
    private static DpoTrainer LegacyShapeTrainer(ModelFixture fixture, string path, DpoBatch profileBatch)
    {
        var recipient = Recipient(fixture);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(fixture.Config, fixture.Lora, fixture.Seeds);
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

    // TestLease выдаёт разрешение на изменение весов для инженерной оснастки.
    private sealed class TestLease : IDpoMutationAdmissionIssuer
    {
        public IDisposable AcquireDpoTrainerMutationPermit() => new CancellationTokenSource();
        public void Dispose() { }
    }
}
