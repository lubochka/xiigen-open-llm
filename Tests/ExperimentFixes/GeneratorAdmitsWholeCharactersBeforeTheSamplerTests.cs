// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// ПРИЁМКА ПЕРЕНОСА «порождение сохраняет целый ответ в кодировке Unicode»
// (пункт M02b адресного плана; человеческое имя — «состояние байтов ответа у
// генератора»).
//
// Что проверяется, по-человечески. Модель выдаёт ответ по одному номеру токена,
// а одна русская буква занимает два байта, знак валюты три, составной знак
// четыре. Раньше можно было выбрать номер, который НАЧИНАЕТ символ, и на этом
// остановиться: ответ обрывался посреди символа, а обрезка хвоста задним числом
// просто выбрасывала обломок. Теперь негодные продолжения убираются ДО выбора,
// поэтому обрывать нечего.
//
// Четыре требования назначения, по одной решающей проверке на каждое:
//   1. допуск применяется ДО выборщика, а не после;
//   2. знак конца ответа остаётся допустимым ровно там, где он законен;
//   3. на последнем шаге нельзя выбрать знак, требующий продолжения;
//   4. правило работает по ФАКТИЧЕСКИМ байтам кодировки, а не по диапазону
//      латиницы — фильтр по латинице прямо запрещён.
//
// Четыре проверки поведения: обычная генерация; неизвестные номера; конечность;
// цена.
//
// Почему сравнения ТОЧНЫЕ. Все числа ниже — это количества кандидатов, обходов
// словаря и вызовов выборщика, то есть целые числа, которые либо совпадают,
// либо нет. Допуска сравнения здесь нет нигде, и поэтому нет и вопроса, больше
// ли различимая величина допуска.
//
// Абрревиатуры при первом употреблении. UTF-8 (Unicode Transformation Format,
// 8-bit — запись знаков Unicode цепочкой байтов переменной длины). BPE (Byte
// Pair Encoding — словарь, склеивающий частые пары байтов в один номер).

namespace LlmFromScratch.Tests.ExperimentFixes;

using Inference;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Caching;
using TransformerModel.Config;

public sealed class GeneratorAdmitsWholeCharactersBeforeTheSamplerTests
{
    // Размер словаря байтового кодека: номер токена равен номеру байта.
    private const int ByteCodecVocabularySize = 256;

    // Начальный байт буквы «ж»: сам по себе не символ, требует одного продолжения.
    private const int CyrillicZheLeadByte = 208;
    // Байт продолжения буквы «ж»: в начале ответа незаконен, после 208 — законен.
    private const int CyrillicZheContinuationByte = 182;
    // Латинская «A»: законченный однобайтовый символ.
    private const int LatinCapitalA = 65;

    // Сколько номеров байтового словаря допустимы при ЦЕЛОМ состоянии ответа, когда в запасе
    // осталось не меньше ТРЁХ шагов. Состав посчитан по правилам UTF-8, а не подогнан:
    // 128 однобайтовых знаков (от 0x00 до 0x7F), 30 начальных байтов двухбайтовых знаков
    // (от 0xC2 до 0xDF), 16 начальных байтов трёхбайтовых (от 0xE0 до 0xEF) и 5 начальных
    // байтов четырёхбайтовых (от 0xF0 до 0xF4). Итого 179.
    private const int AdmittedOnCompleteStateWithWholeBudget = 179;

    // То же ЦЕЛОЕ состояние, но в запасе остался РОВНО ОДИН шаг: начала трёх- и
    // четырёхбайтовых знаков дособрать уже не успеть, поэтому остаются 128 однобайтовых и
    // 30 начал двухбайтовых. Итого 158. Разница 179 − 158 = 21 и есть цена правила остатка
    // бюджета: 16 трёхбайтовых начал плюс 5 четырёхбайтовых.
    private const int AdmittedOnCompleteStateWithOneStepLeft = 158;

    // Сколько из этих 179 лежат ЗА диапазоном латиницы: 30 + 16 + 5. Это число и доказывает,
    // что правило не фильтр по латинице.
    private const int AdmittedBeyondLatinOnCompleteState = 51;

    // Сколько номеров допустимы при НЕДОСОБРАННОМ символе: ровно байты продолжения,
    // от 0x80 до 0xBF, то есть 64 номера — и все они за диапазоном латиницы.
    private const int AdmittedOnPendingState = 64;

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // Байтовый словарь без склеек: 256 однобайтовых записей.
    private static BpeTokenizer ByteCodecVocabulary()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("abc", ByteCodecVocabularySize);
        return tokenizer;
    }

    // Обученный словарь с русским текстом: в нём есть склейки BPE, то есть целые символы и
    // куски слов живут далеко за номером 127.
    private static BpeTokenizer VocabularyWithCyrillicMerges()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            "плюс минус плюс минус знак разности результата плюс минус " +
            "sign of difference result plus minus plus minus " +
            "плюс минус плюс минус знак знак знак разность разность",
            400);
        return tokenizer;
    }

    // Крошечная настройка модели: здесь она только носитель формы вектора логитов.
    private static ModelConfig TinyConfig(int vocabSize) => new()
    {
        VocabSize = vocabSize,
        ContextLen = 128,
        HiddenDim = 32,
        NumLayers = 2,
        NumHeads = 4,
        NumKvHeads = 2,
        FfnHiddenDim = 64
    };

    // Генератор, который читает ровно заданный поток номеров и ЗАПОМИНАЕТ вектор логитов,
    // полученный выборщиком на каждом шаге. Запомненный вектор и есть доказательство того, что
    // допуск применён ДО выборщика: закрытые кандидаты видны уже на входе выборщика.
    private static TextGenerator GeneratorCapturingSamplerInput(
        BpeTokenizer tokenizer, int[] stream, List<float[]> capturedLogits,
        // Размер словаря МОДЕЛИ задаётся отдельно: он может быть больше словаря токенизатора,
        // и тогда в кандидаты попадают номера, за которыми байтов нет.
        int? modelVocabularySize = null)
    {
        var index = 0;
        return new TextGenerator(
            new LlmModel(TinyConfig(modelVocabularySize ?? tokenizer.VocabSize)),
            tokenizer,
            layers => new KvCache(layers),
            (logits, _, _, _, _) =>
            {
                // Копия входа выборщика: дальше её числа сравниваются с ожиданием.
                capturedLogits.Add((float[])logits.Data.Clone());
                // Поток номеров задаётся испытанием, а не моделью.
                return stream[Math.Min(index++, stream.Length - 1)];
            });
    }

    // Генератор с НАСТОЯЩИМ числовым выборщиком продукта: он уважает закрытых кандидатов,
    // потому что закрытый кандидат получает минус бесконечность.
    private static TextGenerator GeneratorWithProductSampler(BpeTokenizer tokenizer, ModelConfig config)
        => new(new LlmModel(config), tokenizer);

    // Сколько чисел вектора НЕ закрыто, то есть сколько кандидатов прошло допуск.
    private static int AdmittedCount(float[] logits)
        => logits.Count(float.IsFinite);

    // Сколько допущенных кандидатов лежит за диапазоном латиницы.
    private static int AdmittedBeyondLatin(float[] logits)
        => Enumerable.Range(0, logits.Length).Count(id => id >= 0x80 && float.IsFinite(logits[id]));

    // Сколько допущенных кандидатов лежит ВНУТРИ диапазона латиницы.
    private static int AdmittedInsideLatin(float[] logits)
        => Enumerable.Range(0, logits.Length).Count(id => id < 0x80 && float.IsFinite(logits[id]));

    // ── Требование 1: допуск применяется ДО выборщика ─────────────────────────

    [Fact]
    public void Requirement1_TheAdmissionReachesTheSamplerInputItself()
    {
        // Поток — законная буква «ж»: начальный байт, затем его продолжение.
        var tokenizer = ByteCodecVocabulary();
        var captured = new List<float[]>();

        var answer = GeneratorCapturingSamplerInput(
            tokenizer, [CyrillicZheLeadByte, CyrillicZheContinuationByte], captured)
            .GenerateWithWholeCharacterAdmission("abc", 2, 0f, 0, 1f, 7, CancellationToken.None);

        // Выборщик вызван по одному разу на шаг — ни одного лишнего вызова допуск не добавил.
        Assert.Equal(2, captured.Count);

        // РЕШАЮЩЕЕ. На ПЕРВОМ шаге байт продолжения 182 уже закрыт на входе выборщика: значит
        // отбор произошёл ДО выбора, а не после. Если бы допуск стоял после выборщика или не
        // стоял вовсе, здесь было бы обычное измеренное число.
        Assert.False(float.IsFinite(captured[0][CyrillicZheContinuationByte]));
        // А начальный байт 208 на первом шаге открыт: он законно начинает символ.
        Assert.True(float.IsFinite(captured[0][CyrillicZheLeadByte]));

        // РЕШАЮЩЕЕ И ПРОТИВ ФИЛЬТРА ПО ЛАТИНИЦЕ. На ВТОРОМ шаге тот же самый номер 182 уже
        // ОТКРЫТ, потому что теперь он дособирает букву «ж». Один и тот же номер закрыт в
        // начале символа и открыт после начального байта — ни один фильтр по диапазону номера
        // так вести себя не может.
        Assert.True(float.IsFinite(captured[1][CyrillicZheContinuationByte]));

        // И ответ пришёл целым и тем самым.
        Assert.Equal("ж", answer);
    }

    // ── Требование 2: правило знака конца ответа ──────────────────────────────

    [Fact]
    public void Requirement2_TheCallerEndMarkerIsClosedOnlyWhenItWouldLeaveTheAnswerUnfinished()
    {
        // Знак конца ответа берётся у ВЫЗЫВАЮЩЕГО: у самого словаря его нет вовсе. Поэтому
        // правило проверяется на переданном значении, а не на свойстве словаря.
        Assert.Null(ByteCodecVocabulary().EosToken);

        // Два прогона отличаются РОВНО одним: передан ли знак конца ответа. Поток, длина,
        // температура и зерно одинаковы, поэтому различие во входе выборщика объясняется
        // только правилом знака конца.
        var withoutMarker = new List<float[]>();
        GeneratorCapturingSamplerInput(ByteCodecVocabulary(), [LatinCapitalA, LatinCapitalA], withoutMarker)
            .GenerateDetailedWithWholeCharacterAdmission("abc", 2, 0f, 0, 1f, 7, null, CancellationToken.None);

        var withMarker = new List<float[]>();
        GeneratorCapturingSamplerInput(ByteCodecVocabulary(), [LatinCapitalA, LatinCapitalA], withMarker)
            .GenerateDetailedWithWholeCharacterAdmission("abc", 2, 0f, 0, 1f, 7,
                CyrillicZheLeadByte, CancellationToken.None);

        // Без знака конца начальный байт 208 на первом шаге ОТКРЫТ: это законное продолжение,
        // символ ещё можно дособрать на следующем шаге.
        Assert.True(float.IsFinite(withoutMarker[0][CyrillicZheLeadByte]));

        // РЕШАЮЩЕЕ. Тот же номер 208, объявленный знаком конца ответа, на том же первом шаге
        // ЗАКРЫТ: сразу после знака конца выдача прекращается, поэтому недособранный символ на
        // нём недопустим.
        Assert.False(float.IsFinite(withMarker[0][CyrillicZheLeadByte]));

        // И обратная сторона того же правила: знак конца НЕ становится недопустимым просто
        // потому, что он знак конца. Законный однобайтовый знак конца открыт и срабатывает.
        var lawfulMarker = new List<float[]>();
        var receipt = GeneratorCapturingSamplerInput(
            ByteCodecVocabulary(), [LatinCapitalA, LatinCapitalA], lawfulMarker)
            .GenerateDetailedWithWholeCharacterAdmission("abc", 2, 0f, 0, 1f, 7,
                LatinCapitalA, CancellationToken.None);

        // Знак конца открыт на входе выборщика…
        Assert.True(float.IsFinite(lawfulMarker[0][LatinCapitalA]));
        // …выбран, и выдача остановилась именно по нему, а не по пределу длины.
        Assert.Equal("EOS", receipt.StopReason);
        // Ответ законченный и ровно один знак длиной: второй шаг не состоялся.
        Assert.Equal("A", receipt.Text);
        Assert.Equal(1, receipt.GeneratedTokenCount);
        // Ни одного отброшенного байта: граница прошла по символу.
        Assert.Equal(0, receipt.HeldBackByteCount);
    }

    // ── Требование 3: правило последнего шага ─────────────────────────────────

    [Fact]
    public void Requirement3_OnTheLastStepACharacterStartThatNeedsContinuationIsClosed()
    {
        var tokenizer = ByteCodecVocabulary();
        var captured = new List<float[]>();

        // Длина ответа два токена: первый шаг не последний, второй последний.
        var answer = GeneratorCapturingSamplerInput(
            tokenizer, [LatinCapitalA, LatinCapitalA], captured)
            .GenerateWithWholeCharacterAdmission("abc", 2, 0f, 0, 1f, 7, CancellationToken.None);

        // На НЕ последнем шаге начальный байт 208 открыт: его ещё можно дособрать.
        Assert.True(float.IsFinite(captured[0][CyrillicZheLeadByte]));

        // РЕШАЮЩЕЕ. На ПОСЛЕДНЕМ шаге тот же начальный байт ЗАКРЫТ: продолжить его будет уже
        // нечем, и ответ оборвался бы посреди символа.
        Assert.False(float.IsFinite(captured[1][CyrillicZheLeadByte]));
        // А законченный однобайтовый знак на последнем шаге открыт.
        Assert.True(float.IsFinite(captured[1][LatinCapitalA]));

        // На последнем шаге при целом состоянии допущены ровно законченные однобайтовые знаки.
        Assert.Equal(128, AdmittedCount(captured[1]));

        // Ответ целый.
        Assert.Equal("AA", answer);
    }

    // ── Требование 4: правило по байтам кодировки, а не по диапазону латиницы ──

    [Fact]
    public void Requirement4_TheRuleIsNotALatinRangeFilterAndIsSometimesItsExactOpposite()
    {
        var tokenizer = ByteCodecVocabulary();

        // Поток составного знака «улыбка»: четыре байта, значит на первом шаге в запасе целых
        // три шага и начала всех длин знаков ещё дособираемы.
        var wholeBudget = new List<float[]>();
        var smileAnswer = GeneratorCapturingSamplerInput(
            tokenizer, [0xF0, 0x9F, 0x99, 0x82], wholeBudget)
            .GenerateWithWholeCharacterAdmission("abc", 4, 0f, 0, 1f, 7, CancellationToken.None);

        // Законный четырёхбайтовый знак прошёл целиком: допуск его не сломал.
        Assert.Equal("🙂", smileAnswer);

        // Первый шаг, целое состояние, бюджета хватает: допущено 179 кандидатов, а фильтр по
        // латинице допустил бы 128. Числа разные, значит это разные правила.
        Assert.Equal(AdmittedOnCompleteStateWithWholeBudget, AdmittedCount(wholeBudget[0]));
        Assert.NotEqual(128, AdmittedCount(wholeBudget[0]));
        // Из них 51 лежит ЗА диапазоном латиницы: фильтр по латинице не допустил бы ни одного.
        Assert.Equal(AdmittedBeyondLatinOnCompleteState, AdmittedBeyondLatin(wholeBudget[0]));

        // Поток «ж»: два байта, поэтому второй шаг застаёт символ НЕДОСОБРАННЫМ.
        var captured = new List<float[]>();
        GeneratorCapturingSamplerInput(
            tokenizer, [CyrillicZheLeadByte, CyrillicZheContinuationByte], captured)
            .GenerateDetailedWithWholeCharacterAdmission("abc", 2, 0f, 0, 1f, 7, null,
                CancellationToken.None);

        // РЕШАЮЩЕЕ. Второй шаг, символ недособран: допущены РОВНО байты продолжения — 64
        // номера, и ВСЕ они за диапазоном латиницы, а внутри латиницы не допущено НИ ОДНОГО.
        // Это прямая противоположность фильтру по латинице, а не его смягчение.
        Assert.Equal(AdmittedOnPendingState, AdmittedCount(captured[1]));
        Assert.Equal(AdmittedOnPendingState, AdmittedBeyondLatin(captured[1]));
        Assert.Equal(0, AdmittedInsideLatin(captured[1]));

        // И на обученном словаре со склейками законный русский ответ проходит целиком: фильтр
        // по латинице выбросил бы его склейки, которые лежат за номером 127.
        var merged = VocabularyWithCyrillicMerges();
        var mergedStream = merged.Encode("минус").ToArray();
        Assert.Contains(mergedStream, identifier => identifier >= 0x80);

        var mergedCaptured = new List<float[]>();
        var mergedAnswer = GeneratorCapturingSamplerInput(merged, mergedStream, mergedCaptured)
            .GenerateWithWholeCharacterAdmission("плюс", mergedStream.Length, 0f, 0, 1f, 7,
                CancellationToken.None);

        // Ответ со склейками пришёл тем же и целым.
        Assert.Equal("минус", mergedAnswer);
        // И на последнем шаге среди допущенных есть номера за диапазоном латиницы: это целые
        // символы и склейки, которые правило по байтам допускает, а диапазонное отвергло бы.
        Assert.True(AdmittedBeyondLatin(mergedCaptured[^1]) > 0,
            "на обученном словаре на последнем шаге не допущено ни одного номера за 127: " +
            "правило выродилось в диапазонное, и это тот самый запрещённый фильтр");
    }

    // ── Поведение 1: обычная генерация не сломалась ───────────────────────────

    [Fact]
    public void Behaviour1_OrdinaryGenerationWithTheProductSamplerStaysLawfulAndUnchanged()
    {
        // Настоящий числовой выборщик продукта, жадная ветвь, одинаковое зерно: сравниваются
        // путь БЕЗ допуска и путь С допуском на одной и той же модели и одном словаре.
        var tokenizer = ByteCodecVocabulary();
        var config = TinyConfig(tokenizer.VocabSize);
        var model = new LlmModel(config);

        var withoutAdmission = new TextGenerator(model, tokenizer)
            .GenerateDetailed("abc", 6, 0f, 0, 1f, 11, null, CancellationToken.None);
        var withAdmission = new TextGenerator(model, tokenizer)
            .GenerateDetailedWithWholeCharacterAdmission("abc", 6, 0f, 0, 1f, 11, null,
                CancellationToken.None);

        // Число выданных токенов одинаково: допуск не укорачивает и не удлиняет выдачу.
        Assert.Equal(withoutAdmission.GeneratedTokenCount, withAdmission.GeneratedTokenCount);
        // Причина остановки одинакова.
        Assert.Equal(withoutAdmission.StopReason, withAdmission.StopReason);
        // С допуском ни один байт не отброшен: ответ закончен уже на выходе цикла, и обрезать
        // хвост не приходится. Это и есть смысл переноса — обрывать нечего.
        Assert.Equal(0, withAdmission.HeldBackByteCount);
        // И в ответе нет знака замены, которым разбор обозначает недособранный символ.
        Assert.DoesNotContain(((char)0xFFFD).ToString(), withAdmission.Text);
    }

    // ── Поведение 2: неизвестные номера ──────────────────────────────────────

    [Fact]
    public void Behaviour2_AnUnknownIdentifierIsClosedByRangeAndRefusedByItsOwnCode()
    {
        // Словарь знает номера от 0 до 255; модель объявляет на один номер больше, поэтому
        // номер 256 попадает в кандидаты, но байтов за ним нет.
        var tokenizer = ByteCodecVocabulary();
        var captured = new List<float[]>();
        // Испытательный выборщик намеренно возвращает номер, которого словарь не знает.
        var generator = GeneratorCapturingSamplerInput(
            tokenizer, [ByteCodecVocabularySize], captured, modelVocabularySize: ByteCodecVocabularySize + 1);

        // Отказ обязан быть явным и типизированным, с кодом именно про НОМЕР.
        var refusal = Assert.Throws<InvalidOperationException>(() =>
            generator.GenerateWithWholeCharacterAdmission("abc", 1, 0f, 0, 1f, 7,
                CancellationToken.None));
        Assert.Equal("TEXT_GENERATOR_UTF8_ADMISSION_IDENTIFIER_UNKNOWN", refusal.Message);

        // РЕШАЮЩЕЕ ПРОТИВ ПОДМЕНЫ ПРИЗНАКА. Неизвестный номер закрыт уже в маске, то есть по
        // признаку промежутка известных номеров. Если бы признак заменили на счёт байтов,
        // неизвестный номер дал бы ноль байтов, выглядел бы как «ничего не добавляет» и был бы
        // ОТКРЫТ — именно это утверждение и упало бы.
        Assert.False(float.IsFinite(captured[0][ByteCodecVocabularySize]));
        // А известные номера на том же шаге открыты, то есть закрыт именно неизвестный.
        Assert.True(float.IsFinite(captured[0][LatinCapitalA]));
    }

    // ── Поведение 3: конечность ──────────────────────────────────────────────

    [Fact]
    public void Behaviour3_TheAdmissionAlwaysLeavesACandidateSoTheLoopCannotHang()
    {
        var tokenizer = ByteCodecVocabulary();
        var cost = new Utf8AdmissionCost();

        // Длинный ответ настоящим выборщиком: каждый шаг строит маску заново.
        var receipt = GeneratorWithProductSampler(tokenizer, TinyConfig(tokenizer.VocabSize))
            .GenerateDetailedWithWholeCharacterAdmission("abc", 12, 0f, 0, 1f, 5, null,
                CancellationToken.None, cost);

        // Выдача дошла до предела длины: цикл завершился, а не завис.
        Assert.Equal(12, receipt.GeneratedTokenCount);
        Assert.Equal("LENGTH_LIMIT", receipt.StopReason);

        // РЕШАЮЩЕЕ. На КАЖДОМ шаге оставался хотя бы один допустимый кандидат. Ноль допущенных
        // означал бы явный отказ до розыгрыша, а не бесконечный цикл; здесь же видно, что и
        // отказываться не пришлось.
        Assert.True(cost.MinimumAdmittedCandidates >= 1,
            $"на каком-то шаге допустимых кандидатов не осталось: {cost.MinimumAdmittedCandidates}");
        // Масок построено ровно по числу шагов.
        Assert.Equal(12, cost.MaskBuilds);

        // И структурное основание конечности, названное числом: при недособранном символе
        // допустимы РОВНО 64 байта продолжения. Они есть в словаре всегда, когда в нём есть
        // хоть один начальный байт многобайтового знака, поэтому состояние «кандидатов нет»
        // на байтовом словаре недостижимо.
        var pending = new List<float[]>();
        GeneratorCapturingSamplerInput(tokenizer,
            [CyrillicZheLeadByte, CyrillicZheContinuationByte], pending)
            .GenerateDetailedWithWholeCharacterAdmission("abc", 2, 0f, 0, 1f, 7, null,
                CancellationToken.None);
        Assert.Equal(AdmittedOnPendingState, AdmittedCount(pending[1]));
    }

    // ── Поведение 4: цена ────────────────────────────────────────────────────

    [Fact]
    public void Behaviour4_TheAdmissionCostIsOneVocabularySweepPlusOnePassPerStep()
    {
        var tokenizer = ByteCodecVocabulary();
        var cost = new Utf8AdmissionCost();

        // Законный поток «ж»: ровно два шага.
        GeneratorCapturingSamplerInputWithCost(tokenizer,
            [CyrillicZheLeadByte, CyrillicZheContinuationByte], cost);

        // Снимок словаря построен ОДИН раз на всё порождение, а не на каждом шаге. Это главное
        // свойство цены: иначе обход словаря повторялся бы по числу выданных токенов.
        Assert.Equal(1, cost.SnapshotBuilds);
        // Обойдено ровно столько номеров, сколько их в словаре.
        Assert.Equal(ByteCodecVocabularySize, cost.SnapshotIdentifiersScanned);
        // Удержано ровно по одному байту на номер: байтовый словарь.
        Assert.Equal(ByteCodecVocabularySize, cost.SnapshotRetainedBytes);

        // Масок построено по одной на шаг, и опрошено ровно словарь на шаг.
        Assert.Equal(2, cost.MaskBuilds);
        Assert.Equal(2L * ByteCodecVocabularySize, cost.CandidateClassifications);

        // Закрыто за оба шага: на первом 256 − 158 = 98 (в запасе один шаг, поэтому начала
        // трёх- и четырёхбайтовых знаков тоже закрыты), на втором 256 − 64 = 192. Итого 290.
        Assert.Equal(
            (ByteCodecVocabularySize - AdmittedOnCompleteStateWithOneStepLeft) +
            (ByteCodecVocabularySize - AdmittedOnPendingState),
            cost.MaskedCandidateTotal);

        // РЕШАЮЩЕЕ ПРО РАСХОД СЛУЧАЙНОСТИ. Выборщик вызван ровно по разу на шаг — столько же,
        // сколько и без допуска. Случайная ветвь выборщика берёт ровно один розыгрыш на вызов,
        // поэтому равное число вызовов означает равное число розыгрышей: обход словаря
        // случайность не расходует.
        Assert.Equal(cost.MaskBuilds, cost.SamplerInvocations);
    }

    // Оснастка для проверки цены: тот же захват входа выборщика, но со счётом цены.
    private static void GeneratorCapturingSamplerInputWithCost(
        BpeTokenizer tokenizer, int[] stream, Utf8AdmissionCost cost)
    {
        var index = 0;
        var generator = new TextGenerator(
            new LlmModel(TinyConfig(tokenizer.VocabSize)),
            tokenizer,
            layers => new KvCache(layers),
            (_, _, _, _, _) => stream[Math.Min(index++, stream.Length - 1)]);
        generator.GenerateDetailedWithWholeCharacterAdmission("abc", stream.Length, 0f, 0, 1f, 7,
            null, CancellationToken.None, cost);
    }

    // ── Путь БЕЗ допуска остался побайтово прежним ───────────────────────────

    [Fact]
    public void TheRouteWithoutTheAdmissionStaysByteIdentical()
    {
        // Числа ниже — это закреплённое ПРЕЖНЕЕ поведение, измеренное на дереве до переноса:
        // поток «начальный байт, продолжение, снова начальный байт» при пределе длины три даёт
        // законченную букву, один отброшенный байт и три выданных токена. Если бы допуск
        // просочился в путь по умолчанию, начальный байт на последнем шаге был бы закрыт, и
        // эти три числа изменились бы.
        var tokenizer = ByteCodecVocabulary();
        var index = 0;
        var receipt = new TextGenerator(
            new LlmModel(TinyConfig(tokenizer.VocabSize)),
            tokenizer,
            layers => new KvCache(layers),
            (_, _, _, _, _) => new[] { CyrillicZheLeadByte, CyrillicZheContinuationByte, CyrillicZheLeadByte }
                [Math.Min(index++, 2)])
            .GenerateDetailed("abc", 3, 0f, 0, 1f, 7, null, CancellationToken.None);

        // Текст — одна законченная буква.
        Assert.Equal("ж", receipt.Text);
        // Один байт отброшен как незаконченный хвост.
        Assert.Equal(1, receipt.HeldBackByteCount);
        // Выданных токенов три: учёт выдачи прежний.
        Assert.Equal(3, receipt.GeneratedTokenCount);
        // Причина остановки прежняя.
        Assert.Equal("LENGTH_LIMIT", receipt.StopReason);
    }
}
