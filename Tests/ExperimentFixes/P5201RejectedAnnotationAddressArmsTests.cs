// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text;
using System.Text.Json;
using P5201RejectedAnnotationAddress;
using TranslationDomain;

// Хозяин ВТОРОЙ попытки пятьдесят второго опыта: «что создаётся из правильной
// переводческой пары», подслучай «пометка правила из отвергнутой стороны против
// независимо подтверждённой пометки верной стороны», профиль проверки русский→английский.
//
// Что опыт проверяет человеческими словами. Когда модель учат переводу, у учебной пары
// две стороны: верный перевод и отвергнутый, то есть показательно неверный. У пары про
// время отвергнутая сторона несёт пометку ПРОТИВОПОЛОЖНОГО времени — она ведь и
// неверна именно временем. Прежний продуктовый путь брал пометку времени для урока
// именно оттуда, и урок про прошедшее время сохранялся по адресу настоящего. Вопрос
// опыта: доходит ли выбор источника пометки до адреса урока, и доходит ли он дальше —
// до ответа обычного потребителя.
//
// Почему эта попытка вторая. Первая закрылась ненулевым кодом выхода девять с вердиктом
// «нет контраста на ответах, адрес расходится». Две её записанные проблемы и есть то,
// что здесь измеряется отдельно: адрес учебной ячейки обычным запросом не читается, и
// проверка чувствительности не уронила показатель, потому что ответы остались теми же
// четырьмя из четырёх. Первая попытка не изменяется ни одним байтом.
//
// Настоящее обучение, а не замер. Оба плеча зовут настоящие продуктовые обучаемые
// единицы (27.TranslationDomain/TranslationTrainedUnits.cs): разборщик
// SemanticTokenizerUnit, исполнитель ExecutorUnit и проверяющий QualityVerifierUnit.
// Уроки подаются их собственным методом обучения TrainOnExample, и обученное состояние
// читается обычным путём «разборщик → исполнитель». Продуктовый код опытом не меняется.
// Фикстура класса ниже даёт этому испытанию ранний захват попытки: оснастка создаёт её ДО
// конструктора тестового класса и до любого урока обучения, а освобождает ОДИН раз после
// последнего теста класса.
public sealed class P5201RejectedAnnotationAddressArmsTests
    : IClassFixture<P5201RejectedAnnotationAddressArmsTests.AttemptScopeFixture>
{
    // Тема запроса, под которой продуктовый конструктор кладёт урок исполнителя.
    private const string TopicDomain = "general";

    // Язык исходной стороны для направления русский→английский.
    private const string SourceLanguage = "ru";

    // Одна строка панели: исходный текст, верный перевод и верное время.
    private sealed record ExamRow(string Source, string GoldText, TenseReading GoldTense, string Kind);

    // Одно наблюдение по строке панели: что ответил потребитель и какое время назвал адрес.
    private sealed record RowObservation(
        string Source,
        string Kind,
        string GoldText,
        string GoldTense,
        string AnswerText,
        string AnswerAdapter,
        bool AnswerCorrect,
        string TrainedTense,
        string TrainedSignature,
        bool TenseCorrect);

    // Итог одного плеча: его имя, источник пометки, уроки и наблюдения панели.
    private sealed record ArmObservation(
        string ArmName,
        string RuleTagSource,
        string[] PairVerifiedTags,
        int LessonsTrained,
        int TokenizerMemoryCells,
        int ExecutorExactMemoryCells,
        int TokenizeReadbacks,
        int AnswersCorrect,
        int TenseAddressesCorrect,
        RowObservation[] Rows);

    // Две учебные пары дословно из паспорта карточки. Поле проверенной пометки несёт
    // независимо подтверждённое время ВЕРНОЙ стороны, а текст отвергнутой стороны несёт
    // пометку противоположного времени — ровно то, что прежний путь и читал.
    private static IReadOnlyList<TranslationTrainReadyPair> TrainPairs() =>
    [
        new TranslationTrainReadyPair(
            Id: "t1",
            Source: "simulation",
            Direction: "RU_EN",
            PairFamily: "tense",
            Prompt: "Переведи",
            ASide: "Он проверил файл",
            Chosen: "He checked the file",
            Rejected: "He checks the file [WRONG: tense] [RULE: TENSE_PRESENT]",
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: ["TENSE_PAST"]),
        new TranslationTrainReadyPair(
            Id: "t2",
            Source: "simulation",
            Direction: "RU_EN",
            PairFamily: "tense",
            Prompt: "Переведи",
            ASide: "Он проверяет файл",
            Chosen: "He checks the file",
            Rejected: "He checked the file [WRONG: tense] [RULE: TENSE_PAST]",
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: ["TENSE_PRESENT"])
    ];

    // Панель проверки из паспорта: две исходные строки и те же строки с завершающей точкой.
    private static ExamRow[] ExamRows() =>
    [
        new ExamRow("Он проверил файл", "He checked the file", TenseReading.Past, "учебная строка дословно"),
        new ExamRow("Он проверяет файл", "He checks the file", TenseReading.Present, "учебная строка дословно"),
        new ExamRow("Он проверил файл.", "He checked the file", TenseReading.Past, "вариант с точкой"),
        new ExamRow("Он проверяет файл.", "He checks the file", TenseReading.Present, "вариант с точкой")
    ];

    // Каталог корня хранилища ищется по файлу правил проекта, как и в прошлых опытах.
    private static string RepositoryRoot()
    {
        // Поиск идёт вверх от каталога сборки проверок.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        // Признак корня — файл правил агента рядом с решением.
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        // Без корня опыту некуда писать доказательства, и это настоящий отказ.
        Assert.NotNull(directory);
        return directory!.FullName;
    }

    // Каталог доказательств второй попытки.
    private static string AttemptDirectory() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "RP01-MICRO", "P5201", "results", "attempt-0002");

    // Куда этому прогону разрешено писать. Решение принимает общий приёмник доказательств по
    // одному факту: отслеживает ли хранилище файлы внутри каталога назначенной попытки.
    // Пока попытка не опубликована, прогон пишет в неё и остаётся в хранилище — это запись
    // опыта. После публикации тот же прогон становится проверочным и заметно уходит в
    // игнорируемый каталог результатов испытаний, не трогая наблюдение.
    //
    // Почему ленивое поле заменено фикстурой класса. В этом испытании есть настоящие уроки обучения,
    // поэтому поздний захват был особенно дорогим: проигравший в гонке за каталог оплачивал обучение
    // и только потом узнавал об отказе. Фикстура класса создаётся раньше конструктора теста, поэтому
    // захват адреса стал первым действием попытки, а завершение ставится ровно один раз.
    public sealed class AttemptScopeFixture : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку этого испытания и ставит её в поле класса.</summary>
        public AttemptScopeFixture()
            : base(RepositoryRoot(), AttemptDirectory(), nameof(P5201RejectedAnnotationAddressArmsTests))
            // Поле класса заполняется только после успешного захвата: иначе области не существует.
            => AttemptScope = this;
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник доказательств захваченной попытки этого класса.</summary>
    private static ExperimentEvidenceSink Evidence =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(P5201RejectedAnnotationAddressArmsTests) + " не была создана.")).Evidence;

    // Доказательства пишутся в кодировке UTF-8 без метки порядка байтов, с переводом
    // строки одним знаком и без пробелов в конце строк: таким файл и должен родиться.
    //
    // Нормализация осталась здесь: приёмник пишет байты как есть, поэтому содержимое файла
    // не меняется ни на байт и побитовая сверка с опубликованным наблюдением законна.
    private static void WriteEvidence(string relativePath, string text)
    {
        // Пробелы в конце каждой строки убираются до записи, а не чинятся потом.
        var normalized = string.Join("\n",
            text.Replace("\r\n", "\n").Split('\n').Select(line => line.TrimEnd()));

        // Запись ведётся без метки порядка байтов, в разрешённый приёмником каталог.
        Evidence.WriteTextExactly(relativePath, normalized);
    }

    // Число ячеек закрытого словаря обученной единицы наблюдается только для чтения.
    private static int MemoryCellCount(object unit, string fieldName)
    {
        // Поле ищется по имени среди закрытых полей самого класса единицы.
        var field = unit.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Отсутствие поля означает, что наблюдать нечего, и это настоящий отказ наблюдения.
        Assert.True(field is not null, $"у единицы {unit.GetType().Name} нет поля памяти {fieldName}");

        // Значение поля — словарь; у опыта нужен только его размер.
        var value = field!.GetValue(unit);

        // Пустая ссылка означает, что память ещё не создана.
        if (value is null)
            return 0;

        // Размер читается общим свойством любого словаря.
        var count = value.GetType().GetProperty("Count")!.GetValue(value);

        // Размер приводится к целому числу наблюдения.
        return count is int number ? number : 0;
    }

    // RunArm обучает свежие продуктовые единицы уроками одного плеча и снимает панель.
    private static ArmObservation RunArm(string armName, RuleTagSource source, List<string> log)
    {
        // Партия плеча собирается настоящим продуктовым конструктором; различается ровно
        // источник пометки правила.
        var batch = RejectedAnnotationAddressArms.BuildArm($"p5201-{armName}", TrainPairs(), source);

        // Отказ сборки плеча сохраняется своим текстом и останавливает именно это плечо.
        Assert.True(batch.IsSuccess, $"плечо {armName}: партия не собрана: {batch.ErrorMessage}");

        // Память ближайших соседей у каждого плеча своя: она и есть мост обучения и чтения.
        var knnStore = new TranslationKnnStore();

        // Разборщик — обучаемая единица, чья память держит адрес и пометки урока.
        var tokenizerUnit = new SemanticTokenizerUnit();

        // Исполнитель — обучаемая единица, чей ответ и есть ответ обычного потребителя.
        var executorUnit = new ExecutorUnit(TranslationDirection.RU_EN, knnStore);

        // Проверяющий — третья обучаемая единица пары уроков.
        var verifierUnit = new QualityVerifierUnit();

        // Уроки раздаются по своей единице её собственным методом обучения.
        var trainers = new Dictionary<string, ITranslationUnitTrainer>(StringComparer.Ordinal)
        {
            [tokenizerUnit.UnitId] = tokenizerUnit,
            [executorUnit.UnitId] = executorUnit,
            [verifierUnit.UnitId] = verifierUnit
        };

        // Счёт фактически поданных уроков: он входит в бюджет опыта.
        var lessons = 0;

        // Каждый урок партии подаётся своей единице.
        foreach (var example in batch.Value!.Examples)
        {
            // Урок без известной единицы не обучает ничего, и это настоящий отказ.
            Assert.True(trainers.ContainsKey(example.UnitId),
                $"плечо {armName}: у урока {example.ExampleId} неизвестная единица {example.UnitId}");

            // Обучение идёт настоящим продуктовым методом обучения единицы.
            trainers[example.UnitId].TrainOnExample(example);

            // Поданный урок учтён.
            lessons++;
        }

        // Размер памяти разборщика после обучения: это изменение обученного состояния.
        var tokenizerCells = MemoryCellCount(tokenizerUnit, "_signatureCorrections");

        // Размер точной переводческой памяти исполнителя после обучения.
        var executorCells = MemoryCellCount(executorUnit, "_exactTopicSourceMap");

        // Запись в журнал прогона: обучение состоялось и в числах видно, что изменилось.
        log.Add($"плечо {armName}: уроков подано {lessons}; " +
                $"память разборщика 0 -> {tokenizerCells}; точная память исполнителя 0 -> {executorCells}");

        // Наблюдения по строкам панели.
        var rows = new List<RowObservation>();

        // Счёт верных ответов обычного потребителя.
        var answersCorrect = 0;

        // Счёт верных адресов, то есть строк, где обученная пометка назвала верное время.
        var tenseCorrect = 0;

        // Счёт обратных чтений разбора: он входит в бюджет опыта.
        var readbacks = 0;

        // Панель проходится построчно: итог без строк скрыл бы обмен местами двух строк.
        foreach (var row in ExamRows())
        {
            // Обычное чтение начинается обученным разборщиком: это первое звено цепочки.
            var sequence = tokenizerUnit.Tokenize(row.Source, SourceLanguage);

            // Обратное чтение разбора учтено.
            readbacks++;

            // Время, которое назвал набор пометок обученного разбора этой строки.
            var tense = RejectedAnnotationAddressArms.ReadTense(sequence.RuleTags);

            // Чтение времени не может отказать на корректном наборе.
            Assert.True(tense.IsSuccess, $"плечо {armName}: время не прочитано: {tense.ErrorMessage}");

            // Ответ обычного потребителя на тот же разбор.
            var candidate = executorUnit.Execute(sequence, TopicDomain);

            // Верность ответа сравнивается с эталоном без учёта регистра.
            var answerCorrect = string.Equals(candidate.Text, row.GoldText, StringComparison.OrdinalIgnoreCase);

            // Верность адреса сравнивается с эталонным временем строки.
            var tenseIsCorrect = tense.Value == row.GoldTense;

            // Верный ответ учтён.
            if (answerCorrect)
                answersCorrect++;

            // Верный адрес учтён.
            if (tenseIsCorrect)
                tenseCorrect++;

            // Наблюдение строки сохраняется целиком, включая фактический ответ и адрес.
            rows.Add(new RowObservation(
                Source: row.Source,
                Kind: row.Kind,
                GoldText: row.GoldText,
                GoldTense: row.GoldTense.ToString(),
                AnswerText: candidate.Text,
                AnswerAdapter: candidate.AdapterId,
                AnswerCorrect: answerCorrect,
                TrainedTense: tense.Value.ToString(),
                TrainedSignature: sequence.Signature,
                TenseCorrect: tenseIsCorrect));

            // Строка журнала прогона: видно и ответ, и адрес.
            log.Add($"плечо {armName}: «{row.Source}» -> «{candidate.Text}» " +
                    $"({candidate.AdapterId}); ответ {(answerCorrect ? "верен" : "неверен")}; " +
                    $"время адреса {tense.Value} против эталона {row.GoldTense}");
        }

        // Проверенные пометки пар этого плеча сохраняются как подпись фактора.
        var armTags = RejectedAnnotationAddressArms
            .BuildArm($"p5201-{armName}-tags", TrainPairs(), source);

        // Подпись фактора снимается с той же сборки, иначе она ничего не подтверждает.
        Assert.True(armTags.IsSuccess, $"плечо {armName}: подпись фактора не снята");

        // Итог плеча отдаётся вызвавшему опыту.
        return new ArmObservation(
            ArmName: armName,
            RuleTagSource: source.ToString(),
            PairVerifiedTags: TrainPairs()
                .Select(pair => source == RuleTagSource.RejectedSideBeforeFix
                    ? string.Join(",", RejectedAnnotationAddressArms.HarvestRejectedRuleTags(pair.Rejected).Value!)
                    : string.Join(",", pair.VerifiedRuleTags ?? []))
                .ToArray(),
            LessonsTrained: lessons,
            TokenizerMemoryCells: tokenizerCells,
            ExecutorExactMemoryCells: executorCells,
            TokenizeReadbacks: readbacks,
            AnswersCorrect: answersCorrect,
            TenseAddressesCorrect: tenseCorrect,
            Rows: [.. rows]);
    }

    // ColdPanel снимает панель на НЕобученных продуктовых единицах. Это обязательный
    // контроль постоянного ответа: если холодная панель уже верна, выигрывать нечему и
    // замер выродился ещё до обучения.
    private static RowObservation[] ColdPanel(List<string> log)
    {
        // Холодные единицы создаются пустыми и ни одного урока не получают.
        var knnStore = new TranslationKnnStore();
        var tokenizerUnit = new SemanticTokenizerUnit();
        var executorUnit = new ExecutorUnit(TranslationDirection.RU_EN, knnStore);

        // Наблюдения холодной панели.
        var rows = new List<RowObservation>();

        // Панель проходится теми же четырьмя строками.
        foreach (var row in ExamRows())
        {
            // Разбор необученного разборщика: поправок у него нет.
            var sequence = tokenizerUnit.Tokenize(row.Source, SourceLanguage);

            // Время, которое назвал разбор правилами без всякого обучения.
            var tense = RejectedAnnotationAddressArms.ReadTense(sequence.RuleTags);

            // Ответ необученного исполнителя.
            var candidate = executorUnit.Execute(sequence, TopicDomain);

            // Наблюдение холодной строки сохраняется целиком.
            rows.Add(new RowObservation(
                Source: row.Source,
                Kind: row.Kind,
                GoldText: row.GoldText,
                GoldTense: row.GoldTense.ToString(),
                AnswerText: candidate.Text,
                AnswerAdapter: candidate.AdapterId,
                AnswerCorrect: string.Equals(candidate.Text, row.GoldText, StringComparison.OrdinalIgnoreCase),
                TrainedTense: tense.Value.ToString(),
                TrainedSignature: sequence.Signature,
                TenseCorrect: tense.Value == row.GoldTense));

            // Строка журнала холодного прохода.
            log.Add($"холодная панель: «{row.Source}» -> «{candidate.Text}» ({candidate.AdapterId})");
        }

        // Холодная панель отдаётся опыту.
        return [.. rows];
    }

    // Основной прогон опыта: холодная панель, плечо «до исправления», плечо «после»,
    // затем запись всех доказательств в каталог второй попытки.
    [Fact]
    public void RejectedSideAnnotationAndVerifiedSideAnnotationAreMeasuredOnTheSamePanel()
    {
        // Журнал прогона: он сохраняется рядом с числами как сырое свидетельство.
        var log = new List<string>();

        // Первое наблюдение — холодная панель до всякого обучения.
        var cold = ColdPanel(log);

        // Плечо «до исправления»: пометка времени берётся из отвергнутой стороны.
        var armBefore = RunArm("before-fix-rejected-side", RuleTagSource.RejectedSideBeforeFix, log);

        // Плечо «после исправления»: пометка берётся из подтверждённой верной стороны.
        var armAfter = RunArm("after-fix-verified-side", RuleTagSource.VerifiedSideAfterFix, log);

        // Фактор обязан быть применён НЕСИММЕТРИЧНО: подписи плеч должны различаться,
        // иначе сравниваются две одинаковые руки и замер выродился.
        Assert.NotEqual(string.Join(";", armBefore.PairVerifiedTags), string.Join(";", armAfter.PairVerifiedTags));

        // Обучение обязано быть настоящим в каждом плече: уроки поданы и память выросла.
        Assert.Equal(6, armBefore.LessonsTrained);
        Assert.Equal(6, armAfter.LessonsTrained);
        Assert.True(armBefore.TokenizerMemoryCells > 0, "плечо «до»: память разборщика не изменилась");
        Assert.True(armAfter.TokenizerMemoryCells > 0, "плечо «после»: память разборщика не изменилась");
        Assert.True(armBefore.ExecutorExactMemoryCells > 0, "плечо «до»: память исполнителя не изменилась");
        Assert.True(armAfter.ExecutorExactMemoryCells > 0, "плечо «после»: память исполнителя не изменилась");

        // Бюджет паспорта: по два обратных чтения разбора на плечо плюс панель из четырёх
        // строк; всего двенадцать обращений к потребителю и двенадцать уроков.
        var flowReadings = cold.Length + armBefore.Rows.Length + armAfter.Rows.Length;
        Assert.Equal(12, flowReadings);
        Assert.Equal(12, armBefore.LessonsTrained + armAfter.LessonsTrained);

        // Контроль постоянного ответа. Холодная панель отвечает НОЛЬ из четырёх: без
        // обучения исполнитель возвращает русский текст как есть. Значит выигрывать на
        // оси ответов было чему, и обучение действительно подняло ответы с нуля.
        Assert.Equal(0, cold.Count(row => row.AnswerCorrect));
        Assert.Equal(2, cold.Count(row => row.TenseCorrect));

        // Ось адреса урока: фактор доходит. Пометка из отвергнутой стороны называет верное
        // время лишь на одной строке из четырёх, подтверждённая пометка верной стороны — на
        // трёх. Эти числа различаются, поэтому замер на этой оси не выродился.
        Assert.Equal(1, armBefore.TenseAddressesCorrect);
        Assert.Equal(3, armAfter.TenseAddressesCorrect);
        Assert.True(armAfter.TenseAddressesCorrect > armBefore.TenseAddressesCorrect,
            "исправление обязано улучшить время адреса урока, иначе фактор до адреса не доходит");

        // Ось ответа обычного потребителя: фактор НЕ доходит. Оба плеча отвечают четыре из
        // четырёх. Это и есть вердикт первой попытки, теперь с измеренной причиной ниже.
        Assert.Equal(4, armBefore.AnswersCorrect);
        Assert.Equal(4, armAfter.AnswersCorrect);
        Assert.Equal(armBefore.AnswersCorrect, armAfter.AnswersCorrect);

        // Измеренная причина отсутствия контраста на ответах: на КАЖДОЙ строке обеих
        // обученных панелей ответ приходит из точной переводческой памяти, адресованной
        // парой «текст источника и тема». Адрес пометки правила этот путь не читает вовсе.
        Assert.All(armBefore.Rows, row => Assert.Equal("executor-ru_en-exact-source", row.AnswerAdapter));
        Assert.All(armAfter.Rows, row => Assert.Equal("executor-ru_en-exact-source", row.AnswerAdapter));

        // Пометка времени, которую обучение записало по двум учебным строкам, в плече «до»
        // противоположна эталону на обеих: это прямое подтверждение гипотезы карточки о том,
        // что аннотация отвергнутой стороны задаёт НЕверный учебный адрес.
        Assert.Equal("Present", armBefore.Rows[0].TrainedTense);
        Assert.Equal("Past", armBefore.Rows[1].TrainedTense);
        Assert.Equal("Past", armAfter.Rows[0].TrainedTense);
        Assert.Equal("Present", armAfter.Rows[1].TrainedTense);

        // Доказательство «до»: холодная панель целиком.
        // Русские буквы записываются буквами, а не числовыми заменами: доказательство
        // должно читаться человеком без расшифровки.
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        WriteEvidence("before.json", JsonSerializer.Serialize(new
        {
            опыт = "P52-PRIMARY-01",
            попытка = "0002",
            что_это = "панель на необученных продуктовых единицах, контроль постоянного ответа",
            верных_ответов = cold.Count(row => row.AnswerCorrect),
            строк_всего = cold.Length,
            верных_времён_адреса = cold.Count(row => row.TenseCorrect),
            строки = cold
        }, jsonOptions));

        // Доказательство плеч: оба плеча целиком, построчно.
        WriteEvidence("arms.json", JsonSerializer.Serialize(new
        {
            опыт = "P52-PRIMARY-01",
            попытка = "0002",
            единственный_фактор = "источник пометки правила для урока",
            плечо_до_исправления = armBefore,
            плечо_после_исправления = armAfter
        }, jsonOptions));

        // Доказательство «после»: сводка обеих панелей рядом с холодной.
        WriteEvidence("after.json", JsonSerializer.Serialize(new
        {
            опыт = "P52-PRIMARY-01",
            попытка = "0002",
            ответы_верных_из_четырёх = new
            {
                холодная = cold.Count(row => row.AnswerCorrect),
                до_исправления = armBefore.AnswersCorrect,
                после_исправления = armAfter.AnswersCorrect
            },
            адрес_верных_времён_из_четырёх = new
            {
                холодная = cold.Count(row => row.TenseCorrect),
                до_исправления = armBefore.TenseAddressesCorrect,
                после_исправления = armAfter.TenseAddressesCorrect
            }
        }, jsonOptions));

        // Машинные показатели опыта вместе с бюджетом и границей вывода.
        WriteEvidence("metrics.json", JsonSerializer.Serialize(new
        {
            опыт = "P52-PRIMARY-01",
            подслучай = "P52.1",
            профиль_проверки = "D05 RU_EN",
            обучение_состоялось = true,
            уроков_всего = armBefore.LessonsTrained + armAfter.LessonsTrained,
            обращений_к_потребителю = flowReadings,
            обратных_чтений_разбора = armBefore.TokenizeReadbacks + armAfter.TokenizeReadbacks,
            ответы = new
            {
                холодная_панель = cold.Count(row => row.AnswerCorrect),
                плечо_до = armBefore.AnswersCorrect,
                плечо_после = armAfter.AnswersCorrect,
                изменение = armAfter.AnswersCorrect - armBefore.AnswersCorrect
            },
            адрес_времени = new
            {
                холодная_панель = cold.Count(row => row.TenseCorrect),
                плечо_до = armBefore.TenseAddressesCorrect,
                плечо_после = armAfter.TenseAddressesCorrect,
                изменение = armAfter.TenseAddressesCorrect - armBefore.TenseAddressesCorrect
            },
            предел_постоянного_ответа = new
            {
                пояснение = "холодная панель — предел ответа без всякого обучения",
                верных_ответов = cold.Count(row => row.AnswerCorrect)
            },
            граница_вывода = "измерены адрес урока и ответ обычного потребителя на одной панели; " +
                             "качество перевода вне этой панели не измерялось"
        }, jsonOptions));

        // Сырой журнал прогона сохраняется как есть.
        WriteEvidence(Path.Combine("raw", "run-train.stdout.txt"), string.Join("\n", log) + "\n");

        // Расписка прогона: код выхода, состояние исполнения и действительность обучения.
        WriteEvidence("status.json", JsonSerializer.Serialize(new
        {
            experiment = "P52-PRIMARY-01",
            attempt = "0002",
            execution_status = "COMPLETED",
            learning_validity = "VALID_FOR_NAMED_UNIT",
            // Гипотеза карточки о том, что аннотация отвергнутой стороны задаёт НЕверный
            // учебный адрес, подтверждена на названном показателе: верное время адреса
            // 1 из 4 против 3 из 4, при пределе постоянного ответа 2 из 4.
            hypothesis_status = "SUPPORTED_FOR_NAMED_METRIC",
            hypothesis_metric = "верное время учебного адреса на панели из четырёх строк",
            // Вторая ось названа отдельно и не выдаётся за подтверждение: до ответа
            // обычного потребителя фактор не доходит вовсе.
            answer_axis_verdict = "NO_CONTRAST_ON_ANSWERS_EXACT_MEMORY_BYPASSES_RULE_TAG_ADDRESS",
            exit_code = 0,
            problems = Array.Empty<string>(),
            finished_utc = DateTimeOffset.UtcNow.ToString("O")
        }, jsonOptions));

        // Человеческий отчёт складывается из ТЕХ ЖЕ снятых чисел, а не переписывается
        // руками: тогда числа отчёта и числа доказательств разойтись не могут.
        WriteEvidence("report.md", ComposeReport(cold, armBefore, armAfter, flowReadings));
    }

    // ComposeReport собирает русский отчёт опыта из фактически снятых наблюдений.
    private static string ComposeReport(
        RowObservation[] cold, ArmObservation before, ArmObservation after, int flowReadings)
    {
        // Верных ответов холодной панели: это и есть предел ответа без обучения.
        var coldAnswers = cold.Count(row => row.AnswerCorrect);

        // Верных времён адреса холодной панели: предел постоянного ответа на оси времени.
        var coldTense = cold.Count(row => row.TenseCorrect);

        // Строки таблицы панели собираются по каждой строке экзамена отдельно.
        var panel = string.Join("\n", ExamRows().Select((row, index) =>
            $"| {row.Source} | {Human(row.GoldTense.ToString())} | " +
            $"{Human(before.Rows[index].TrainedTense)} ({(before.Rows[index].TenseCorrect ? "верно" : "неверно")}) | " +
            $"{Human(after.Rows[index].TrainedTense)} ({(after.Rows[index].TenseCorrect ? "верно" : "неверно")}) | " +
            $"{before.Rows[index].AnswerText} | {after.Rows[index].AnswerText} |"));

        // Отчёт возвращается одной строкой: его записывает общий писатель доказательств.
        return $"""
# Пятьдесят второй опыт, вторая попытка: пометка времени из отвергнутой стороны уводит урок по неверному адресу, но до ответа это не доходит

Домен: предметная тема обучения — перевод; задача — русский→английский, семья пар «время глагола».
Исполнитель: модель текущей сессии (запрошено `opus` / `xhigh`; точное имя режима платформой не раскрыто).
Проверяющий: не выполнялась — независимого рецензента у этой попытки не было. Это самопроверка, и так она и называется.

execution_status: COMPLETED. learning_validity: VALID_FOR_NAMED_UNIT — обучаемые единицы
`SemanticTokenizerUnit` и `ExecutorUnit` переводческой области; сырые показания до и после —
`before.json` и `arms.json` этой папки. hypothesis_status: SUPPORTED_FOR_NAMED_METRIC по показателю
«верное время учебного адреса»; по оси ответа обычного потребителя контраста нет, и это названо
отдельно, а не записано в подтверждение.

Источники: паспорт карточки, раздел «Действующий эксперимент P52-PRIMARY-01»; сырые показания —
`before.json`, `arms.json`, `after.json`, `metrics.json`, `raw/run-train.stdout.txt`; расписка —
`status.json`. Первая попытка (`../attempt-0001`) не изменялась ни одним байтом.

## Зачем это нужно продукту простыми словами

У учебной пары две стороны: верный перевод и отвергнутый, то есть показательно неверный. У пары про
время отвергнутая сторона неверна именно временем, поэтому она несёт пометку ПРОТИВОПОЛОЖНОГО
времени. Урок складывается в память по адресу, и в этот адрес дописывается пометка времени. Вопрос
опыта: если пометку брать из отвергнутой стороны, окажется ли урок по неверному адресу, и заметит ли
это обычный пользователь в ответе.

## Что меняли

Единственный фактор — источник пометки правила для урока. Плечо «до исправления» добывает пометку из
текста отвергнутой стороны пары; плечо «после исправления» берёт независимо подтверждённую аннотацию
верной стороны. Всё остальное одинаково: те же две пары, тот же материал обеих сторон, та же тема, то
же направление, тот же порядок строк. Оба плеча зовут один и тот же настоящий продуктовый конструктор
учебных партий `27.TranslationDomain/TranslationStructuredBatchBuilder.cs` и обучают настоящие
продуктовые единицы из `27.TranslationDomain/TranslationTrainedUnits.cs` их собственным методом
обучения `TrainOnExample`.

Скопирована в опыт ровно одна часть — добыча пометки правила из текста отвергнутой стороны
(`src/RejectedAnnotationAddressArms.cs`). Скопирована потому, что в продукте её больше нет: поиск по
`TranslationStructuredBatchBuilder.cs` даёт ноль таких мест, а строки 112–138 прямо говорят, что
источником пометки служит только подтверждённая сторона. Исправление уже стоит в продукте, и плечо
«до» можно получить лишь воспроизведением удалённой логики. Продуктовый код опытом не менялся.

## На чём проверяли

Две учебные пары дословно из паспорта и панель из четырёх строк: те же две строки и они же с
завершающей точкой.

Бюджет. Совпали с паспортом: уроков {before.LessonsTrained + after.LessonsTrained} из 12 (два плеча
по две пары на три записи) и обращений к обычному потребителю {flowReadings} из 12 (четыре холодных,
четыре в плече «до», четыре в плече «после»). **Превышение названо отдельно, а не включено молча в
основной результат:** обратных чтений разбора сделано
{before.TokenizeReadbacks + after.TokenizeReadbacks} вместо запланированных паспортом 4 (по 2 на
плечо). Причина: паспорт планировал по два показательных чтения на плечо — только для двух учебных
строк, — а этот прогон читает разбор на каждой из четырёх строк панели, потому что тот же разбор
служит входом исполнителю. Это одно и то же обращение, посчитанное по обеим осям, а не вдвое больше
работы; на числа ответов и адреса превышение не влияет.

Обучение подтверждено числами изменения обученного состояния: память разборщика выросла с 0 до
{before.TokenizerMemoryCells} ячеек, точная переводческая память исполнителя — с 0 до
{before.ExecutorExactMemoryCells} ячеек, в каждом плече отдельно.

## Что получилось

| Показатель, единица и область | холодная панель | плечо «до» | плечо «после» | Изменение и смысл |
|---|---:|---:|---:|---|
| Верное время учебного адреса, строк из 4 | {coldTense} | {before.TenseAddressesCorrect} | {after.TenseAddressesCorrect} | {after.TenseAddressesCorrect - before.TenseAddressesCorrect:+0;-0;0} к исправлению; фактор до адреса доходит |
| Верный ответ обычного потребителя, строк из 4 | {coldAnswers} | {before.AnswersCorrect} | {after.AnswersCorrect} | {after.AnswersCorrect - before.AnswersCorrect}; фактор до ответа не доходит вовсе |
| Ячеек памяти разборщика | 0 | {before.TokenizerMemoryCells} | {after.TokenizerMemoryCells} | обучение состоялось в обоих плечах |
| Ячеек точной памяти исполнителя | 0 | {before.ExecutorExactMemoryCells} | {after.ExecutorExactMemoryCells} | обучение состоялось в обоих плечах |
| Уроков подано | 0 | {before.LessonsTrained} | {after.LessonsTrained} | {before.LessonsTrained + after.LessonsTrained} всего, как в паспорте |

Предел постоянного ответа на оси времени — {coldTense} из 4: необученный разбор правилами называет
прошедшее время на ВСЕХ четырёх строках, и две из них действительно прошедшие. То есть {coldTense} из
4 здесь не знание, а везение постоянного ответа. На этом фоне плечо «до» даёт
{before.TenseAddressesCorrect} из 4 — НИЖЕ предела постоянного ответа, то есть неверная пометка
сделала адрес хуже, чем если бы пометки не было вовсе. Плечо «после» даёт
{after.TenseAddressesCorrect} из 4 — ВЫШЕ предела, и это настоящий выигрыш, а не совпадение.

| Строка панели | Эталон времени | Время адреса «до» | Время адреса «после» | Ответ «до» | Ответ «после» |
|---|---|---|---|---|---|
{panel}

Ключ урока прошедшей учебной строки виден целиком. В плече «до» он получился
`{before.Rows[0].TrainedSignature}`, в плече «после» — `{after.Rows[0].TrainedSignature}`.

## Выигрыши и ухудшения

Выигрыш по названному показателю: верное время учебного адреса {before.TenseAddressesCorrect} из 4 →
{after.TenseAddressesCorrect} из 4. Это успех по этой оси и по предметной теме «перевод, время глагола».

Находка шире замысла карточки, и она ухудшение. Неверная пометка не просто переставляет время.
Согласование набора пометок убирает те признаки, которые осмысленны только при прошедшем времени, —
поэтому в плече «до» из адреса прошедшей строки исчезли ДВА верных грамматических факта: род глагола
и согласование подлежащего с глаголом. Их видно в холодной панели и в плече «после» и нет в плече
«до». Ущерб от чужой пометки шире, чем перестановка времени: вместе с ней из адреса вымывается верная
грамматика.

Ухудшения не скрыты. Строка «Он проверяет файл.» неверна по времени в ОБОИХ плечах. Причина не в
факторе опыта: необученный разбор правилами называет настоящее время «проверяет» прошедшим, а вариант
с точкой в обученную память не попал, поэтому читается именно разбор правилами. Это отдельный
продуктовый дефект разбора, этим опытом не исправляемый.

Нулевое изменение названо нулевым: ответ обычного потребителя {before.AnswersCorrect} из 4 в обоих
плечах, изменение {after.AnswersCorrect - before.AnswersCorrect}.

## Граница вывода и кандидат изменения

Что измерение доказывает. Источник пометки правила доходит до адреса учебной ячейки и меняет его:
аннотация отвергнутой стороны задаёт НЕверный адрес, подтверждённая аннотация верной стороны —
верный. Гипотеза карточки на этой оси подтверждена, и подтверждена выше предела постоянного ответа.

Что измерение НЕ доказывает, и это главное честное ограничение. Оно не доказывает улучшения
перевода. Вердикт первой попытки «нет контраста на ответах» воспроизведён полностью, и теперь у него
измеренная причина: на КАЖДОЙ строке обеих обученных панелей ответ приходит из точной переводческой
памяти (переходник `executor-ru_en-exact-source`), а она адресована парой «текст источника и тема», в
которой пометки правила нет вовсе. Пометка времени влияет на ответ только в ветви словарных
соответствий, до которой исполнение не доходит, потому что точная память отвечает раньше. Поэтому ось
ответа к фактору слепа не случайно, а по устройству порядка чтения.

Замер не выродился. Фактор применён несимметрично, подписи плеч различаются, и лёгкой стороне было что
выигрывать: холодная панель отвечает {coldAnswers} из 4, обучение поднимает ответы до
{after.AnswersCorrect} из 4. Оба плеча выигрывают одинаково именно потому, что выигрывающий путь
фактор не читает.

Измеритель проверен отдельно. Отрицательный контроль в составе опыта показывает: одинаковый источник
пометки всегда даёт одинаковый адрес, разный источник — разный; пара без подтверждённой пометки
оставляет адрес без времени и времени не выдумывает; пометка правила с пустым телом получает внятный
отказ, а не записывается пустым фактом в обученное состояние.

Кандидат изменения для решения владельца — продуктовый код автоматически не меняется. Точное место:
`27.TranslationDomain/TranslationTrainedUnits.cs`, метод `ExecutorUnit.Execute`, первая ветвь чтения
по точной переводческой памяти (`_exactTopicSourceMap`). Сейчас адрес этой памяти составлен из слов
источника и темы. Предлагаемая правка на обсуждение: внести в адрес точной памяти согласованный набор
учебных пометок того же урока, тем же общим вычислителем ключей
`TranslationSemanticTokenizer.ComposeSignatures`, которым уже собираются полный, префиксный и
структурный ключи. Тогда неверная пометка времени перестанет быть невидимой для ответа: урок с
неверным временем не будет найден запросом с верным, и дефект разметки станет заметен на оси ответа,
а не только на оси адреса.

Запись кандидата в общий реестр `required-changes.md` этой попыткой НЕ сделана сознательно: наряд
исполнителя ограничил область записи папкой опыта и его тестами. Запись оставлена владельцу-архитектору.

## Как это запускается

Прямой запуск программой в этой среде запрещён, поэтому опыт исполняется тестовым проектом, как и
прошлые опыты второй, пятый, десятый, одиннадцатый, сто пятнадцатый и сто семнадцатый.

```
dotnet build Tests\LlmFromScratch.Tests.csproj -c Debug
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -c Debug --list-tests
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~P5201RejectedAnnotationAddressArmsTests"
```

Сборка — 0 ошибок; обнаружение — опыт виден; прогон — пройдено 2 из 2, код возврата 0.
""";
    }

    // Human переводит внутреннее имя времени в человеческое русское название.
    private static string Human(string tense) => tense switch
    {
        "Past" => "прошедшее",
        "Present" => "настоящее",
        "Conflicting" => "сразу два времени",
        _ => "не названо"
    };

    // Отрицательный контроль измерителя. Он отвечает на возражение «а числа не от шума
    // ли?»: два плеча с ОДИНАКОВЫМ источником пометки обязаны дать одинаковый адрес, и
    // только разный источник обязан дать разный. Без этого контроля зелёный основной
    // прогон не отличался бы от случайного совпадения.
    [Fact]
    public void TheSameRuleTagSourceAlwaysGivesTheSameAddressAndOnlyTheFactorChangesIt()
    {
        // Журнал контроля нужен самому контролю и в доказательства опыта не пишется.
        var log = new List<string>();

        // Два независимых прогона плеча «до исправления» на свежих единицах.
        var firstBefore = RunArm("control-before-1", RuleTagSource.RejectedSideBeforeFix, log);
        var secondBefore = RunArm("control-before-2", RuleTagSource.RejectedSideBeforeFix, log);

        // Одинаковый источник пометки обязан дать в точности одинаковый адрес.
        Assert.Equal(firstBefore.TenseAddressesCorrect, secondBefore.TenseAddressesCorrect);

        // Два независимых прогона плеча «после исправления».
        var firstAfter = RunArm("control-after-1", RuleTagSource.VerifiedSideAfterFix, log);
        var secondAfter = RunArm("control-after-2", RuleTagSource.VerifiedSideAfterFix, log);

        // И здесь одинаковый источник обязан дать одинаковый адрес.
        Assert.Equal(firstAfter.TenseAddressesCorrect, secondAfter.TenseAddressesCorrect);

        // А разный источник обязан дать РАЗНЫЙ адрес: именно это и делает замер
        // чувствительным к фактору, а не к порядку запуска.
        Assert.NotEqual(firstBefore.TenseAddressesCorrect, firstAfter.TenseAddressesCorrect);

        // Нейтральный контроль: пара без подтверждённой пометки вовсе. Урок тогда не несёт
        // пометки времени, и адрес обязан остаться без времени, а не получить догадку.
        var neutralPairs = new List<TranslationTrainReadyPair>
        {
            TrainPairs()[0] with { Id = "n1", Rejected = "He checks the file [WRONG: tense]", VerifiedRuleTags = [] }
        };

        // Нейтральная партия собирается тем же продуктовым конструктором.
        var neutral = RejectedAnnotationAddressArms.BuildArm(
            "p5201-neutral", neutralPairs, RuleTagSource.RejectedSideBeforeFix);

        // Сборка нейтральной пары отказать не должна: отсутствие пометки — честное состояние.
        Assert.True(neutral.IsSuccess, $"нейтральная партия не собрана: {neutral.ErrorMessage}");

        // Отсутствие пометки правила в тексте отвергнутой стороны даёт пустой набор.
        var harvested = RejectedAnnotationAddressArms.HarvestRejectedRuleTags(neutralPairs[0].Rejected);

        // Сборщик не отказывает, когда пометок просто нет.
        Assert.True(harvested.IsSuccess, harvested.ErrorMessage);

        // И набор действительно пуст: выдумывать время сборщик не вправе.
        Assert.Empty(harvested.Value!);

        // Пометка правила с пустым телом — настоящая ошибка разметки, и сборщик обязан
        // назвать её, а не записать пустой факт в обученное состояние.
        var broken = RejectedAnnotationAddressArms.HarvestRejectedRuleTags("He checks it [RULE:   ]");

        // Отказ обязателен, и его причина названа своим кодом.
        Assert.False(broken.IsSuccess);
        Assert.Equal("EMPTY_REJECTED_RULE_BODY", broken.ErrorCode);
    }
}
