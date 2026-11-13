// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка кандидата сотого опыта, подтверждённого сто первым независимо: в ответ
// не попадает то, что символом не является.
//
// ЧТО ИСПРАВЛЕНО В САМОМ ДОГОВОРЕ ПРИЁМКИ 2 ноября 2026 (ремонт «неверная цель
// исправления в Unicode-приёмке», пункт M02a адресного плана; человеческое имя
// пункта — «сначала исправить приёмку, потом генератор»).
//
// Чем прежний договор был опасен. Прежняя первая проверка требовала, чтобы
// КАЖДЫЙ отдельный номер токена сам по себе удовлетворял равенству
// «разобрать и собрать обратно даёт тот же единственный номер». Для байтового
// словаря это неверное общее требование: буква «ж» — это байты 208 и 182,
// «é» — 195 и 169, «🙂» — 240, 159, 153 и 130. Ни один из этих байтов НЕ
// является законченным символом в одиночку, а последовательность целиком
// совершенно законна. На словаре из 256 байтов такая поштучная проверка
// оставляет ровно 128 латинских байтов. Опасность была именно в договоре: если
// сделать его зелёным простым запретом незавершённых байтовых токенов, из
// продукта выпадут законные русские ответы — то есть приёмка приказала бы
// сломать продукт.
//
// Что стало договором. Законность принадлежит ПОСЛЕДОВАТЕЛЬНОСТИ и её
// состоянию, а не отдельному номеру. Правило продукта читается так: «весь
// ответ целиком разбирается в законченный текст», то есть в ответе нет знака
// замены U+FFFD, которым разбор обозначает недособранный символ. Это правило
// пропускает «ж» из двух байтов и отвергает одинокий байт продолжения 182 —
// ровно то, что требовалось.
//
// Почему проверка идёт через ГЕНЕРАТОР, а не через выборщик. Допуск обязан
// знать словарь, уже выбранный префикс ответа, признак конца и остаток
// бюджета. Выборщик — чистый числовой алгоритм, ему эти сведения неизвестны, и
// глобального запрета номеров от 128 он получать не должен. Поэтому
// однотокенный случай сотого опыта проверяется генератором при длине ответа
// один, а общая генерация проверяет целую последовательность. Поток номеров
// задаётся точно через внутренний вход генератора с подставляемым выборщиком —
// тем же способом, которым это делает соседняя приёмка признака конца ответа.
//
// Что здесь зелёное и что красное. Положительные случаи (законные
// многобайтовые ответы), встречный контроль «поштучное правило отвергает
// законный русский ответ» и проверки состояния последовательности зелёные уже
// сейчас: они говорят правду о действующем кодеке. Отрицательные случаи
// генератора красные НАМЕРЕННО и ждут отдельной работы — состояния UTF-8 у
// генератора (пункт M02b того же плана, человеческое имя — «состояние байтов
// ответа у генератора»). Делать их зелёными запретом незавершённых байтовых
// токенов ЗАПРЕЩЕНО: именно этот запрет убивает русский ответ, и встречный
// контроль ниже это доказывает числом.
//
// Оснастка логитов НЕ придумана: взяты точные измеренные значения из сырых
// данных сто первого опыта (`experiments-v2/RP01-MICRO/P10101/results/
// attempt-0001/repair.json`, 20 строк по пяти снимкам весов).
//
// Чего эта приёмка НЕ доказывает. Она не измеряет частоту обломка в работающем
// обученном продукте: сто первый опыт доказал достижимость и механизм, а не
// частоту. Она не утверждает, что правка даёт ВЕРНЫЙ ответ: она утверждает
// ровно одно — в ответ не попадает то, что символом не является. Она не
// реализует и не проверяет сам допуск в генераторе: этого кода ещё нет.
//
// > Пометка 9 ноября 2026 (исполнитель переноса «порождение сохраняет целый
// > ответ в кодировке Unicode», пункт M02b). Исходный текст выше сохранён как
// > есть; две его фразы стали ИСТОРИЧЕСКИМИ, и вот чем именно.
// >
// > Первое. Фраза «этого кода ещё нет» больше не описывает дерево: допуск у
// > порождения появился — `09.Inference/Utf8AnswerAdmission.cs` и отбор
// > допустимых продолжений внутри `TextGenerator.GenerateCore` до вызова
// > выборщика. Поэтому чтение ответа ниже идёт через вход с допуском, и
// > положительные случаи теперь проверяют ДВА свойства сразу: что законный
// > многобайтовый ответ проходит и что допуск его не ломает.
// >
// > Второе. Фраза «отрицательные случаи генератора красные НАМЕРЕННО»
// > относится к состоянию до этой работы. Измеренная история: сохранённый
// > журнал `run-byte-answer-admission-contract-gate-contract.txt` показывает
// > все пять красными; на дереве перед этой работой красными остались ДВЕ
// > (поток 208,65 и неизвестный номер), потому что три из пяти закрыла
// > обрезка незаконченного хвоста, вошедшая отдельным ремонтом. Обрезка — это
// > не допуск: она действует ПОСЛЕ выбора. Числа: было 20 прошло / 2
// > провалено из 22, стало 22 прошло / 0 провалено из 22.
// >
// > Третье, и это единственная правка смысла проверок. Договор назвал законным
// > исходом «явный типизированный отказ до выбора», но код проверок такой
// > отказ наблюдать не мог: при броске исполнение до утверждения не доходит, и
// > проверка падала бы на самом законном поведении. Поэтому в двух
// > отрицательных проверках добавлена ВТОРАЯ ветвь — «отказ с ожидаемым
// > кодом». Прежние утверждения не сняты и не ослаблены: они по-прежнему
// > действуют на ветви без отказа. У неизвестного номера сверх прежнего
// > закреплён точный код отказа, то есть проверка стала строже, а не слабее.
// >
// > Имена проверок с окончанием «ждёт допуска у порождения» НЕ переименованы
// > НАМЕРЕННО: на них ссылается сохранённый журнал выше, и переименование
// > оборвало бы эту связь. Окончание читать как историческое.

namespace LlmFromScratch.Tests.ExperimentFixes;

using Inference;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Caching;
using TransformerModel.Config;

public sealed class AnswerReadingRejectsIncompleteCharacterIdentifiersTests
{
    // Размер словаря байтового кодека, на котором исполнялись оба опыта.
    private const int ByteCodecVocabularySize = 256;

    // Идентификатор обломка байта, найденный сотым опытом и измеренный сто
    // первым: 182 это 0xB6, то есть байт продолжения многобайтовой
    // последовательности, который в одиночку символа не образует.
    private const int ByteFragmentIdentifier = 182;

    // Первый байт буквы «ж»: в одиночку он тоже не символ, но именно после него
    // байт 182 становится законным. Эта пара и есть причина, по которой
    // поштучное правило неверно.
    private const int CyrillicZheLeadByte = 208;

    // Знак замены: разбор ставит его там, где байты не собрались в символ.
    // Его отсутствие в ответе и есть проверяемое правило «ответ законченный».
    private const char ReplacementCharacter = '�';

    // Пол вектора логитов: ниже любого измеренного значения, чтобы выбор
    // определялся только измеренными идентификаторами.
    private const float LogitFloor = -10f;

    // Одна измеренная строка чтения: номер снимка весов, имя строки, логиты
    // четырёх значимых идентификаторов и фактический выбор продукта.
    private sealed record MeasuredRow(
        int Updates,
        string Row,
        float Plus,
        float Minus,
        float Fragment,
        float? Star,
        int ProductChoice);

    // Шесть строк, где продукт выбрал обломок байта. Все шесть — экзаменационные
    // подсказки на снимках 25, 50 и 100 обновлений.
    private static MeasuredRow[] RowsWhereProductChoseAByteFragment() =>
    [
        new(25,  "E1", 3.500521f,  4.056986f,  4.0735264f, null,       182),
        new(25,  "E2", 3.659883f,  4.1266756f, 4.1366158f, null,       182),
        new(50,  "E1", 3.3086722f, 3.954266f,  4.1517653f, null,       182),
        new(50,  "E2", 3.46388f,   4.0243306f, 4.1993217f, null,       182),
        new(100, "E1", 2.9433384f, 3.736191f,  4.240066f,  3.8619545f, 182),
        new(100, "E2", 3.0801659f, 3.7988818f, 4.2586083f, 3.8656182f, 182)
    ];

    // Четырнадцать строк, где продукт выбрал законный символ. Их правка обязана
    // оставить ровно как есть — это сторона «не ломает законное».
    private static MeasuredRow[] RowsWhereProductChoseAWholeCharacter() =>
    [
        new(0,   "T1", 5.654635f,  3.7933986f, 2.8904266f, null, 43),
        new(0,   "T2", 4.5910234f, 4.392597f,  3.6110332f, null, 43),
        new(0,   "E1", 3.6537466f, 4.132603f,  3.9909968f, null, 45),
        new(0,   "E2", 3.8132536f, 4.199436f,  4.067336f,  null, 45),
        new(10,  "T1", 5.6673927f, 3.7805018f, 2.9227278f, null, 43),
        new(10,  "T2", 4.5501223f, 4.3783965f, 3.6431305f, null, 43),
        new(10,  "E1", 3.607746f,  4.11095f,   4.018319f,  null, 45),
        new(10,  "E2", 3.767511f,  4.178944f,  4.0906167f, null, 45),
        new(25,  "T1", 5.6916633f, 3.744548f,  2.9926689f, null, 43),
        new(25,  "T2", 4.451876f,  4.339107f,  3.7093925f, null, 43),
        new(50,  "T1", 5.7137275f, 3.6665435f, 3.103953f,  null, 43),
        new(50,  "T2", 4.265446f,  4.2554216f, 3.806877f,  null, 43),
        new(100, "T1", 5.7025146f, 3.455347f,  3.250236f,  null, 43),
        new(100, "T2", 3.8863244f, 4.0511503f, 3.9215121f, null, 45)
    ];

    // Вектор логитов ровно той формы, какую генератор отдаёт выборщику: один
    // вектор длиной в размер словаря.
    private static Tensor BuildLogits(MeasuredRow row)
    {
        var data = new float[ByteCodecVocabularySize];
        for (int i = 0; i < data.Length; i++) data[i] = LogitFloor;
        data[43] = row.Plus;
        data[45] = row.Minus;
        data[ByteFragmentIdentifier] = row.Fragment;
        if (row.Star.HasValue) data[42] = row.Star.Value;
        return new Tensor(data, new TensorShape(ByteCodecVocabularySize));
    }

    // Словарь байтового кодека без обучения: 256 однобайтовых записей, ровно как
    // в общем обученном старте обоих опытов.
    private static BpeTokenizer ByteCodecVocabulary()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("abc", ByteCodecVocabularySize);
        return tokenizer;
    }

    // Обученный словарь с русским текстом: в нём есть склейки BPE, то есть целые
    // символы и куски слов живут далеко за номером 127. Нужен для случая
    // «кириллица со склейками» и для проверки, отвергающей диапазонное правило.
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

    // Крошечная настройка модели: она здесь только носитель формы. Числа выбора
    // подставляются потоком ниже, а не берутся у модели, поэтому обучение и
    // воспроизводимость весов на эту приёмку не влияют.
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

    // Генератор, который читает ровно заданный поток номеров. Это обычный
    // продуктовый путь чтения ответа: подставляется только источник числа, сам
    // разбор ответа остаётся продуктовым.
    private static TextGenerator GeneratorReadingExactly(BpeTokenizer tokenizer, int[] stream)
    {
        var index = 0;
        return new TextGenerator(
            new LlmModel(TinyConfig(tokenizer.VocabSize)),
            tokenizer,
            layers => new KvCache(layers),
            (_, _, _, _, _) => stream[Math.Min(index++, stream.Length - 1)]);
    }

    // Читает ответ генератором на заданном потоке номеров. Длина ответа равна
    // длине потока, температура ноль — то есть жадная ветвь, ровно тот путь
    // чтения, которым получены все шесть измеренных строк.
    private static string ReadAnswer(BpeTokenizer tokenizer, int[] stream)
    {
        // Вход С ДОПУСКОМ целого символа: именно он отбирает допустимые продолжения до выбора
        // токена. Остальные части пути — разбор ответа, сборка байтов и граница текста —
        // остаются продуктовыми и прежними.
        return GeneratorReadingExactly(tokenizer, stream).GenerateWithWholeCharacterAdmission(
            "abc", stream.Length, 0f, 0, 1f, 7, CancellationToken.None);
    }

    // ReadAnswerWithoutAdmission читает тот же поток ПРЕЖНИМ путём, без допуска. Нужен как
    // сторона сравнения: на нём видно, что путь без допуска остался побайтово прежним.
    private static string ReadAnswerWithoutAdmission(BpeTokenizer tokenizer, int[] stream)
    {
        return GeneratorReadingExactly(tokenizer, stream).Generate(
            "abc", stream.Length, 0f, 0, 1f, 7, CancellationToken.None);
    }

    // Проверяемое правило продукта: ответ целиком законченный текст, то есть в
    // нём нет знака замены. Правило ничего не знает про диапазоны номеров и
    // поэтому остаётся верным и на обученном словаре.
    private static bool WholeAnswerIsCompleteText(string answer)
    {
        return !answer.Contains(ReplacementCharacter);
    }

    // ОТВЕРГНУТАЯ поштучная формулировка. Оставлена в файле намеренно: ниже она
    // служит встречным контролем, который доказывает её негодность числом.
    private static bool EncodesBackToItself(BpeTokenizer tokenizer, int identifier)
    {
        var decoded = tokenizer.Decode([identifier]);
        var encoded = tokenizer.Encode(decoded);
        return encoded.Count == 1 && encoded[0] == identifier;
    }

    // Состояние допуска последовательности: испытательный образец того, чем
    // обязан стать допуск в генераторе (пункт M02b, «состояние байтов ответа»).
    // Здесь он нужен, чтобы ОТРИЦАТЕЛЬНЫЕ случаи были выражены как состояние
    // последовательности, а не как запрет номера.
    private enum SequenceStatus
    {
        // Байты собрались в законченный текст: ответ можно завершать.
        ValidComplete,
        // Байты законны, но символ ещё не закончен: продолжать можно, завершать нельзя.
        ValidPrefix,
        // Байты незаконны как начало или продолжение символа: кандидат непригоден.
        Invalid
    }

    // Разбирает поток байтов слева направо по правилам UTF-8 и говорит, чем он
    // кончился. Неизвестный номер словаря непригоден сразу: за ним нет байтов.
    //
    // ОБЛАСТЬ ПРИМЕНЕНИЯ — только словарь байтового кодека, где номер токена
    // РАВЕН номеру байта (`BpeTokenizer` строит первые 256 записей именно так).
    // Для склеенных записей обученного словаря нужен снимок «номер → байты»
    // самого словаря, а такого открытого входа у продукта ещё НЕТ: он назначен
    // будущей работой (пункт M02b). Брать байты через разбор НЕЛЬЗЯ, и это
    // измерено на собственной ошибке: `Decode([182])` отдаёт знак замены, чьи
    // собственные байты 239,191,189 образуют законный трёхбайтовый символ, и
    // одинокий обломок ложно выглядел бы законченным. Поэтому здесь байты
    // берутся из номера напрямую, а принадлежность к байтовому кодеку
    // проверяется явно.
    private static SequenceStatus ClassifySequence(BpeTokenizer tokenizer, int[] identifiers)
    {
        // Предпосылка области применения: это именно байтовый кодек.
        Assert.Equal(ByteCodecVocabularySize, tokenizer.VocabSize);

        // Сколько байтов продолжения ещё ожидается у текущего символа.
        var pending = 0;

        foreach (var identifier in identifiers)
        {
            // Номер за пределами словаря не имеет байтов и в ответ не годится.
            if (identifier < 0 || identifier >= tokenizer.VocabSize) return SequenceStatus.Invalid;

            // На байтовом кодеке номер токена равен номеру байта.
            var bytes = new[] { (byte)identifier };

            foreach (var value in bytes)
            {
                // Байт продолжения: законен только когда символ ещё не закончен.
                if ((value & 0xC0) == 0x80)
                {
                    if (pending == 0) return SequenceStatus.Invalid;
                    pending--;
                    continue;
                }

                // Любой начальный байт посреди незакончённого символа незаконен.
                if (pending > 0) return SequenceStatus.Invalid;

                // Однобайтовый символ.
                if (value < 0x80) { pending = 0; continue; }
                // Двухбайтовый символ: ожидается один байт продолжения.
                if ((value & 0xE0) == 0xC0) { pending = 1; continue; }
                // Трёхбайтовый символ: ожидается два байта продолжения.
                if ((value & 0xF0) == 0xE0) { pending = 2; continue; }
                // Четырёхбайтовый символ: ожидается три байта продолжения.
                if ((value & 0xF8) == 0xF0) { pending = 3; continue; }

                // Остальные значения не начинают ни один законный символ.
                return SequenceStatus.Invalid;
            }
        }

        // Ответ законченный только когда ни один символ не остался недособранным.
        return pending == 0 ? SequenceStatus.ValidComplete : SequenceStatus.ValidPrefix;
    }

    // ── Положительная сторона: законные многобайтовые ответы проходят ──────────

    [Theory]
    // Двухбайтовая кириллица: байты 208 и 182, тот самый случай, который
    // поштучное правило запрещало.
    [InlineData("ж")]
    // Двухбайтовая латиница с диакритикой: байты 195 и 169.
    [InlineData("é")]
    // Трёхбайтовый символ: байты 226, 130 и 172.
    [InlineData("€")]
    // Четырёхбайтовый символ: байты 240, 159, 153 и 130.
    [InlineData("🙂")]
    // Целое русское слово: несколько двухбайтовых символов подряд.
    [InlineData("файл")]
    // Латиница: однобайтовый случай, который обязан остаться законным.
    [InlineData("abc")]
    public void LawfulAnswersPassWholeSequenceReading(string lawfulAnswer)
    {
        // Словарь байтового кодека — тот же, на котором исполнялись оба опыта.
        var tokenizer = ByteCodecVocabulary();
        // Поток номеров — продуктовая запись этого самого ответа.
        var stream = tokenizer.Encode(lawfulAnswer).ToArray();

        // Предпосылка проверки: ответ действительно многобайтовый или
        // однобайтовый, но в любом случае записан именно этими номерами.
        Assert.NotEmpty(stream);

        // Обычное чтение ответа генератором на этом потоке.
        var answer = ReadAnswer(tokenizer, stream);

        // Ответ пришёл целым: это и есть проверяемое правило.
        Assert.True(
            WholeAnswerIsCompleteText(answer),
            $"законный ответ «{lawfulAnswer}» прочитан как незаконченный текст: «{answer}»");
        // И он тот самый, а не какой-то другой законченный текст.
        Assert.Equal(lawfulAnswer, answer);
    }

    [Fact]
    public void LawfulCyrillicWithMergesPassesWholeSequenceReading()
    {
        // Обученный словарь: у него есть склейки BPE, то есть номера целых
        // кусков слов лежат далеко за 127. Поштучное правило их бы не отвергло,
        // а диапазонное — отвергло бы; проверяется, что проходит и этот случай.
        var tokenizer = VocabularyWithCyrillicMerges();
        // Русское слово из учебного корпуса этого словаря.
        const string lawfulAnswer = "минус";
        var stream = tokenizer.Encode(lawfulAnswer).ToArray();

        // Предпосылка: хотя бы один номер потока — склейка за диапазоном 0x80.
        Assert.Contains(stream, identifier => identifier >= 0x80);

        var answer = ReadAnswer(tokenizer, stream);

        // Ответ со склейками тоже обязан прийти целым и тем же.
        Assert.True(
            WholeAnswerIsCompleteText(answer),
            $"русский ответ со склейками прочитан как незаконченный текст: «{answer}»");
        Assert.Equal(lawfulAnswer, answer);
    }

    // ── Встречный контроль: поштучный запрет убивает русский ответ ─────────────

    [Theory]
    // Для каждого из этих законных ответов поштучное правило отвергает хотя бы
    // один его номер, то есть запрещает ответ целиком.
    [InlineData("ж")]
    [InlineData("é")]
    [InlineData("€")]
    [InlineData("🙂")]
    [InlineData("файл")]
    public void ThePerIdentifierRuleRejectsLawfulAnswersAndThereforeMustNotBecomeTheFix(
        string lawfulAnswer)
    {
        var tokenizer = ByteCodecVocabulary();
        var stream = tokenizer.Encode(lawfulAnswer).ToArray();

        // Последовательность законна: это уже доказано положительной стороной,
        // здесь она подтверждается состоянием разбора.
        Assert.Equal(SequenceStatus.ValidComplete, ClassifySequence(tokenizer, stream));

        // А поштучное правило отвергает хотя бы один номер этой законной
        // последовательности. Значит «запретить незавершённые байтовые токены» —
        // это не починка, а потеря законного ответа.
        Assert.Contains(stream, identifier => !EncodesBackToItself(tokenizer, identifier));
    }

    [Fact]
    public void TheAsciiOnlyMutationLeavesOnlyLatinAndIsThereforeTheWrongRule()
    {
        // Числовая цена отвергнутого правила, измеренная на том же словаре: из
        // 256 номеров поштучное правило оставляет ровно 128 латинских байтов, и
        // совпадает с диапазоном «меньше 0x80» без расхождений. Именно это
        // совпадение и показывает, что поштучное правило — переодетый
        // ASCII-фильтр, а не допуск символа.
        var tokenizer = ByteCodecVocabulary();
        var admitted = 0;
        var disagreements = 0;

        for (int identifier = 0; identifier < ByteCodecVocabularySize; identifier++)
        {
            var byRoundTrip = EncodesBackToItself(tokenizer, identifier);
            var byRange = identifier < 0x80;
            if (byRoundTrip) admitted++;
            if (byRoundTrip != byRange) disagreements++;
        }

        // Ровно 128 оставленных номеров и ноль расхождений с диапазоном.
        Assert.Equal(0, disagreements);
        Assert.Equal(128, admitted);
        // Обломок, найденный опытами, поштучным правилом отвергается…
        Assert.False(EncodesBackToItself(tokenizer, ByteFragmentIdentifier));
        // …но он же необходим как продолжение буквы «ж», и это ключ ко всему
        // ремонту договора: один и тот же номер незаконен в начале символа и
        // законен после начального байта.
        Assert.Equal(
            SequenceStatus.ValidComplete,
            ClassifySequence(tokenizer, [CyrillicZheLeadByte, ByteFragmentIdentifier]));
        Assert.Equal(
            SequenceStatus.Invalid,
            ClassifySequence(tokenizer, [ByteFragmentIdentifier]));
    }

    // ── Состояние последовательности: что обязан различать будущий допуск ─────

    [Fact]
    public void SequenceAdmissionAcceptsLawfulSequencesAndNamesEachUnlawfulOne()
    {
        var tokenizer = ByteCodecVocabulary();

        // Законная пара: буква «ж» целиком — ответ можно завершать.
        Assert.Equal(
            SequenceStatus.ValidComplete,
            ClassifySequence(tokenizer, [CyrillicZheLeadByte, ByteFragmentIdentifier]));

        // Байт продолжения в начале ответа: непригоден.
        Assert.Equal(
            SequenceStatus.Invalid,
            ClassifySequence(tokenizer, [ByteFragmentIdentifier, 65]));

        // Неизвестный номер словаря: за ним нет байтов, непригоден.
        Assert.Equal(
            SequenceStatus.Invalid,
            ClassifySequence(tokenizer, [ByteCodecVocabularySize]));

        // Неверное продолжение после начального байта: латинская «A» вместо
        // байта продолжения — непригодно.
        Assert.Equal(
            SequenceStatus.Invalid,
            ClassifySequence(tokenizer, [CyrillicZheLeadByte, 65]));

        // Незакончённый символ в конце: сам префикс законен, но завершать ответ
        // на нём нельзя. Именно это различие и отделяет «можно продолжать» от
        // «можно закончить», и поэтому признак конца требует законченного вида.
        Assert.Equal(
            SequenceStatus.ValidPrefix,
            ClassifySequence(tokenizer, [CyrillicZheLeadByte]));
    }

    // ── Границы продукта, которые ремонт договора обязан сохранить ─────────────

    [Fact]
    public void ThePureNumericSamplerKeepsEveryLawfulChoiceUnchanged()
    {
        // Сторона «не ломает законное» на измеренных данных: там, где
        // наибольший логит уже у законного номера, выбор обязан остаться
        // прежним. Четырнадцать допустимых однотокенных чтений сохраняются.
        var tokenizer = ByteCodecVocabulary();

        foreach (var row in RowsWhereProductChoseAWholeCharacter())
        {
            var chosen = Sampler.Sample(BuildLogits(row), 0f, 0, 1f);

            // Выбор совпадает с измеренным выбором продукта.
            Assert.Equal(row.ProductChoice, chosen);
            // И он действительно законченный символ сам по себе.
            Assert.Equal(SequenceStatus.ValidComplete, ClassifySequence(tokenizer, [chosen]));
        }
    }

    [Fact]
    public void ThePureNumericSamplerMustNotReceiveAGlobalBanOnBytesAboveLatin()
    {
        // Выбранное архитектурное решение, записанное как проверка: выборщик
        // остаётся ЧИСТЫМ числовым алгоритмом и глобального запрета номеров от
        // 128 не получает. Он не знает ни словаря, ни префикса ответа, ни
        // остатка бюджета, поэтому допуск символа в нём разместить нельзя — и
        // именно поэтому на этих шести измеренных строках он по-прежнему
        // отдаёт наибольший логит, то есть обломок 182. Исправлять эту строку
        // запретом внутри выборщика ЗАПРЕЩЕНО: допуск принадлежит генератору.
        foreach (var row in RowsWhereProductChoseAByteFragment())
        {
            var chosen = Sampler.Sample(BuildLogits(row), 0f, 0, 1f);

            // Чистый числовой выбор: наибольший логит, без допуска символа.
            Assert.Equal(ByteFragmentIdentifier, chosen);
        }
    }

    [Fact]
    public void TheRangeFormulationMustNotBecomeTheRuleBecauseItRejectsWholeCharacters()
    {
        // Эта проверка закрепляет архитектурное решение, а не поведение: на
        // обученном словаре целые символы и склейки живут за номером 127, и
        // диапазонное правило выбросило бы их из ответа — то есть воспроизвело
        // бы структурную недостижимость, которую сотый опыт измерил на своём
        // плече «ровно K»: из розыгрыша систематически выпадает всё, что не
        // латиница.
        var tokenizer = VocabularyWithCyrillicMerges();

        var wholeCharactersBeyondTheRange = 0;
        for (int identifier = 0; identifier < tokenizer.VocabSize; identifier++)
        {
            if (identifier >= 0x80 && EncodesBackToItself(tokenizer, identifier))
                wholeCharactersBeyondTheRange++;
        }

        // Хотя бы один целый символ за диапазоном — уже достаточное основание
        // отвергнуть диапазонную формулировку. Точное число зависит от корпуса,
        // поэтому закрепляется неравенство, а не замеренное 59.
        Assert.True(
            wholeCharactersBeyondTheRange > 0,
            "на обученном словаре не нашлось целых символов за номером 127: " +
            "оснастка обучения перестала содержать нелатинский текст, и эта " +
            "проверка потеряла смысл — её нужно починить, а не ослабить");
    }

    // ── Отрицательная сторона генератора: КРАСНОЕ до работы M02b ──────────────

    [Theory]
    // Байт продолжения в начале ответа: ответа из него быть не может.
    [InlineData(new[] { ByteFragmentIdentifier }, "байт продолжения в начале ответа")]
    // Незакончённый символ в конце ответа: признак конца требует законченности.
    [InlineData(new[] { CyrillicZheLeadByte }, "незакончённый символ в конце ответа")]
    // Неверное продолжение после начального байта.
    [InlineData(new[] { CyrillicZheLeadByte, 65 }, "неверное продолжение после начального байта")]
    // Законное начало и обломок в конце: ответ обрывается недособранным символом.
    [InlineData(
        new[] { 65, CyrillicZheLeadByte },
        "законное начало и незакончённый символ в конце")]
    public void GeneratorNeverReturnsAnIncompleteAnswer_PendingGeneratorAdmission(
        int[] stream,
        string caseName)
    {
        // ЭТА ПРОВЕРКА КРАСНАЯ НАМЕРЕННО и ждёт отдельной работы: состояния
        // UTF-8 у генератора (пункт M02b адресного плана, человеческое имя —
        // «состояние байтов ответа у генератора»). Сейчас генератор отдаёт
        // байты как есть, и разбор ставит знак замены.
        //
        // ЧЕМ ЭТУ ПРОВЕРКУ ЗЕЛЕНИТЬ НЕЛЬЗЯ: запретом номеров от 128 или
        // поштучным правилом. Такой запрет ломает положительные случаи выше, и
        // встречный контроль это показывает числом. Законная починка — допуск
        // по состоянию последовательности, с отказом до выбора, когда
        // пригодных кандидатов не осталось.
        var tokenizer = ByteCodecVocabulary();

        // Предпосылка: поток действительно незаконен как ответ целиком, то есть
        // проверка требует отказа в правильном случае, а не в произвольном.
        Assert.NotEqual(SequenceStatus.ValidComplete, ClassifySequence(tokenizer, stream));

        // Испытательный выборщик здесь НАМЕРЕННО возвращает номер, которого в маске допуска
        // нет: только так незаконный поток вообще можно довести до генератора. Поэтому
        // законных исходов два, и оба обязаны быть безопасными.
        string? answer = null;
        var refusal = Record.Exception(() => answer = ReadAnswer(tokenizer, stream));

        if (refusal is null)
        {
            // Ветвь без отказа: прежнее правило продукта действует без изменений.
            Assert.True(
                WholeAnswerIsCompleteText(answer!),
                $"{caseName}: в ответ попал незакончённый символ, прочитано «{answer}»");
        }
        else
        {
            // Ветвь отказа: он обязан быть явным и типизированным, с кодом допуска, а не
            // случайным падением где-то в арифметике.
            Assert.IsType<InvalidOperationException>(refusal);
            Assert.StartsWith("TEXT_GENERATOR_UTF8_ADMISSION_", refusal.Message);
        }
    }

    [Fact]
    public void GeneratorRefusesAnUnknownIdentifierInsteadOfReadingItAsEmpty_PendingGeneratorAdmission()
    {
        // ЭТА ПРОВЕРКА КРАСНАЯ НАМЕРЕННО (пункт M02b). Неизвестный номер не
        // имеет байтов в словаре, и разбор молча отдаёт пустоту. Молчаливая
        // пустота хуже отказа: вызывающий не узнаёт, что ответ потерян.
        // Законная починка — явный типизированный отказ до выбора.
        var tokenizer = ByteCodecVocabulary();
        // Номер ровно за границей словаря.
        var stream = new[] { ByteCodecVocabularySize };

        // Предпосылка: номер действительно вне словаря.
        Assert.True(stream[0] >= tokenizer.VocabSize);

        string? answer = null;
        var refusal = Record.Exception(() => answer = ReadAnswer(tokenizer, stream));

        if (refusal is not null)
        {
            // Явный типизированный отказ — именно он назван законной починкой выше. Код
            // закреплён ТОЧНО, и это строже прежнего утверждения: отказ обязан сообщать, что
            // негоден сам НОМЕР, а не цепочка байтов. Разные причины — разные коды.
            Assert.IsType<InvalidOperationException>(refusal);
            Assert.Equal("TEXT_GENERATOR_UTF8_ADMISSION_IDENTIFIER_UNKNOWN", refusal.Message);
        }
        else
        {
            // Непустой ответ или отказ — но не молчаливая пустота на месте символа.
            Assert.False(
                answer!.Length == 0,
                "неизвестный номер прочитан как пустота: вызывающий не узнаёт, что " +
                "ответ потерян, а должен получить явный отказ");
        }
    }
}
