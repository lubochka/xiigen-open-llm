// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using P5801CorrectiveLessonCounterExample;
using TranslationDomain;

// Хозяин ВТОРОЙ попытки пятьдесят восьмого опыта: «обратная связь стала уроком или только
// пометкой о состоянии», подслучай «урок строится из фактического ошибочного ответа»,
// профиль проверки английский→русский.
//
// Что проверяется человеческими словами. Машину учат переводу, она отвечает на экзамене, и
// на одной строке отвечает неверно. По этой ошибке составляют исправляющий урок. Вопрос
// карточки: что должно лежать в уроке на месте «вот так неправильно» — заготовленная
// строка корпуса или настоящий ошибочный ответ машины, и меняет ли этот выбор хоть что-то.
//
// Две оси измеряются РАЗДЕЛЬНО, и каждая названа своим именем.
// Первая ось — содержание урока. Она невырождена: выбор источника доходит до урока и
// меняет его. Вторая ось — ответ обычного потребителя. Она вырождена ПО УСТРОЙСТВУ
// ПРОДУКТА, и это доказано чтением кода до прогона: метод обучения исполнителя
// `ExecutorUnit.TrainOnExample` (27.TranslationDomain/TranslationTrainedUnits.cs, строки
// 570-631) разбирает только сторону «выбрано» и сторону «отвергнуто» не читает вовсе.
//
// Контроль измерителя обязателен именно поэтому: третье плечо подаёт сторону «отвергнуто»
// с явной фразовой пометкой, которая по устройству попадает в сторону «выбрано». Если
// обученное состояние у него изменится, значит измеритель видит настоящую разницу, когда
// она есть, и равенство состояний в плечах A и B — свойство продукта, а не слепота опыта.
//
// Продуктовый код, первая попытка, карточка и указатель опытов этим опытом не меняются.
// Фикстура класса ниже даёт этому испытанию ранний захват попытки: оснастка создаёт её ДО
// конструктора тестового класса и до любого урока обучения, а освобождает ОДИН раз после
// последнего теста класса.
public sealed class P5801CorrectiveLessonCounterExampleArmsTests
    : IClassFixture<P5801CorrectiveLessonCounterExampleArmsTests.AttemptScopeFixture>
{
    // Направление этого профиля проверки.
    private const TranslationDirection Direction = TranslationDirection.EN_RU;

    // Язык исходной стороны для направления английский→русский.
    private const string SourceLanguage = "en";

    // Тема запроса, под которой продуктовый сборщик кладёт урок исполнителя.
    private const string TopicDomain = "general";

    // Одна строка набора: имя, запрос и правильный перевод.
    private sealed record CorpusRow(string Id, string Source, string Gold);

    // Наблюдение по одной строке панели.
    private sealed record PanelRow(string Id, string Source, string Gold, string Answer, string Adapter, bool Correct);

    // Итог одного плеча опыта.
    private sealed record ArmResult(
        string ArmName,
        string RejectedSource,
        string RejectedSideText,
        bool CounterExampleUsable,
        string LessonChosenText,
        string LessonRejectedText,
        string LessonRejectedType,
        string StateAfterSetupHash,
        string StateAfterCorrectiveHash,
        bool StateChangedByCorrectiveLesson,
        int CorrectiveLessons,
        int PanelCorrectAfter,
        PanelRow[] PanelAfter);

    // Предел постоянного ответа: что набрал бы наилучший ЕДИНСТВЕННЫЙ постоянный ответ,
    // который вход не читает вовсе. Это НЕ наблюдённый счёт холодной панели: холодный счёт
    // говорит, сколько необученная модель ответила верно НА САМОМ ДЕЛЕ, а предел — сколько
    // можно набрать вообще без обучения и без чтения входа. Две разные величины, и совпасть
    // они могут только случайно.
    private sealed record ConstantAnswerCeilingResult(
        int Correct,
        int PanelRows,
        string BestConstantAnswer,
        int CandidatesConsidered,
        int CandidatesTiedAtBest);

    // Расписка прогона опыта. Состояние исполнения, действительность обучения, состояние
    // гипотезы, код возврата и список проблем выводятся ОТСЮДА и больше ниоткуда. Литерал
    // рядом с ними не умеет отказать: до этой правки три поля расписки были написаны руками
    // внутри самой проверки, и будущий испорченный прогон записался бы как завершённый с
    // нулевым кодом возврата и пустым списком проблем.
    private sealed record ExperimentReceipt(
        bool Valid,
        string ExecutionStatus,
        string LearningValidity,
        string HypothesisStatus,
        int ExitCode,
        string[] Problems);

    // Замороженный учебный набор: восемь строк, дословно из эталона первой попытки
    // `results/attempt-0001/before.json`, записанного ДО её прогона. Набор не меняется.
    private static CorpusRow[] TrainRows() =>
    [
        new CorpusRow("t1", "Anna opened the door", "Анна открыла дверь"),
        new CorpusRow("t2", "Anna closed the window", "Анна закрыла окно"),
        new CorpusRow("t3", "Boris opened the window", "Борис открыл окно"),
        new CorpusRow("t4", "Boris closed the door", "Борис закрыл дверь"),
        new CorpusRow("t5", "Anna did not open the door", "Анна не открыла дверь"),
        new CorpusRow("t6", "Anna did not close the window", "Анна не закрыла окно"),
        new CorpusRow("t7", "Boris did not open the window", "Борис не открыл окно"),
        new CorpusRow("t8", "Boris did not close the door", "Борис не закрыл дверь")
    ];

    // Замороженная панель экзамена: восемь строк, тот же источник и тот же порядок.
    private static CorpusRow[] ExamRows() =>
    [
        new CorpusRow("e1", "Anna opened the window", "Анна открыла окно"),
        new CorpusRow("e2", "Anna closed the door", "Анна закрыла дверь"),
        new CorpusRow("e3", "Boris opened the door", "Борис открыл дверь"),
        new CorpusRow("e4", "Boris closed the window", "Борис закрыл окно"),
        new CorpusRow("e5", "Anna did not open the window", "Анна не открыла окно"),
        new CorpusRow("e6", "Anna did not close the door", "Анна не закрыла дверь"),
        new CorpusRow("e7", "Boris did not open the door", "Борис не открыл дверь"),
        new CorpusRow("e8", "Boris did not close the window", "Борис не закрыл окно")
    ];

    // Корень хранилища ищется по файлу правил проекта, как и в прошлых опытах.
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

    // Каталог доказательств ВТОРОЙ попытки. Первая попытка не затрагивается.
    private static string AttemptDirectory() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "RP01-MICRO", "P5801", "results", "attempt-0002");

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
            : base(RepositoryRoot(), AttemptDirectory(), nameof(P5801CorrectiveLessonCounterExampleArmsTests))
            // Поле класса заполняется только после успешного захвата: иначе области не существует.
            => AttemptScope = this;
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник доказательств захваченной попытки этого класса.</summary>
    private static ExperimentEvidenceSink Evidence =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(P5801CorrectiveLessonCounterExampleArmsTests) + " не была создана.")).Evidence;

    // Доказательства пишутся в кодировке UTF-8 без метки порядка байтов, с переводом строки
    // одним знаком и без пробелов в конце строк: таким файл и должен родиться.
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

    // Настройки записи доказательств: русские буквы записываются буквами.
    private static JsonSerializerOptions ReadableJson() => new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // Одна обучаемая связка: разборщик, исполнитель, проверяющий и общая память соседей.
    private sealed record UnitSet(
        SemanticTokenizerUnit Tokenizer,
        ExecutorUnit Executor,
        QualityVerifierUnit Verifier,
        TranslationKnnStore KnnStore);

    // NewUnits создаёт свежую связку продуктовых обучаемых единиц с пустой памятью.
    private static UnitSet NewUnits()
    {
        // Память ближайших соседей — мост между обучением и чтением.
        var store = new TranslationKnnStore();

        // Три настоящие продуктовые обучаемые единицы на одной памяти.
        return new UnitSet(
            new SemanticTokenizerUnit(),
            new ExecutorUnit(Direction, store),
            new QualityVerifierUnit(),
            store);
    }

    // TrainBatch раздаёт уроки партии своим единицам их собственным методом обучения
    // и возвращает число фактически поданных уроков.
    private static int TrainBatch(UnitSet units, TranslationLearningBatch batch)
    {
        // Уроки раздаются по имени единицы, которой они назначены.
        var trainers = new Dictionary<string, ITranslationUnitTrainer>(StringComparer.Ordinal)
        {
            [units.Tokenizer.UnitId] = units.Tokenizer,
            [units.Executor.UnitId] = units.Executor,
            [units.Verifier.UnitId] = units.Verifier
        };

        // Счёт фактически поданных уроков: он входит в бюджет опыта.
        var trained = 0;

        // Каждый урок подаётся своей единице настоящим продуктовым методом обучения.
        foreach (var example in batch.Examples)
        {
            // Урок без известной единицы не обучает ничего, и это настоящий отказ.
            Assert.True(trainers.ContainsKey(example.UnitId),
                $"у урока {example.ExampleId} неизвестная единица {example.UnitId}");

            // Обучение идёт продуктовым методом обучения единицы.
            trainers[example.UnitId].TrainOnExample(example);

            // Поданный урок учтён.
            trained++;
        }

        // Число уроков отдаётся вызвавшему плечу.
        return trained;
    }

    // BuildSetupBatch собирает установочную партию из восьми учебных строк замороженного
    // набора. Сторона «отвергнуто» у всех восьми — строка корпуса, как и в первой попытке.
    private static TranslationLearningBatch BuildSetupBatch(string batchId)
    {
        // Восемь пар вида, который требует продуктовый сборщик.
        var pairs = TrainRows().Select(row => new TranslationTrainReadyPair(
            Id: row.Id,
            Source: "simulation",
            Direction: "EN_RU",
            PairFamily: "feedback_setup",
            Prompt: "Переведи",
            ASide: row.Source,
            Chosen: row.Gold,
            Rejected: CorrectiveLessonCounterExampleArms.PreparedCorpusRejectedLine,
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: [])).ToArray();

        // Установочная партия собирается настоящим продуктовым сборщиком.
        return TranslationStructuredBatchBuilder.BuildGeneralBatch(
            batchId, pairs, heldOut: false, signatureMode: TranslationSignatureMode.Structural);
    }

    // ПРАВИЛО ВЕРНОСТИ ОТВЕТА этого опыта, записанное в ОДНОМ месте: полное совпадение текста
    // без учёта регистра. Им судится панель экзамена, контроль обучения на учебных строках и
    // предел постоянного ответа. Отдельная копия правила внутри расчёта предела измеряла бы
    // предел другой меркой, чем саму панель, и сравнение двух чисел перестало бы быть
    // законным: именно поэтому правило вынесено, а не продублировано.
    private static bool AnswerIsCorrect(string answer, string gold)
        => string.Equals(answer, gold, StringComparison.OrdinalIgnoreCase);

    // ComputeConstantAnswerCeiling переносит образец расчёта из пятьдесят второго опыта
    // (`experiments-v2/RP01-MICRO/P5201/Program.cs`, метод `ConstantAnswerCeiling`, строка
    // 1349): кандидатами берутся различные ожидаемые ответы панели ПЛЮС пустая строка, чтобы
    // стратегия «постоянно молчать» тоже считалась; каждый кандидат оценивается тем же
    // правилом `AnswerIsCorrect`, которым опыт судит правильность; лучший выигрывает, и
    // записываются счёт, сам лучший постоянный ответ и независимый знаменатель.
    //
    // Расчёт аналитический и не делает НИ ОДНОГО вызова модели — так же, как во втором
    // эталоне (`experiments-v2/RP01-MICRO/P7101/Program.cs`, строки 497-509, где предел
    // назван прямо и выведено правило: плечо с такой же долей от постоянного ответчика
    // неотличимо). Поэтому он отвечает на вопрос «сколько можно набрать, не обучаясь и не
    // глядя на вход», а не на вопрос «сколько набрала необученная модель»: второе и есть
    // холодный счёт, и подставлять его сюда — значит выдать одну величину за другую.
    private static ConstantAnswerCeilingResult ComputeConstantAnswerCeiling()
    {
        // Панель берётся та же замороженная, по которой считается и счёт плеч.
        var panel = ExamRows();

        // Кандидаты: различные ожидаемые ответы панели и пустая строка как стратегия молчания.
        var candidates = panel
            .Select(row => row.Gold)
            .Distinct(StringComparer.Ordinal)
            .Append(string.Empty)
            .ToArray();

        // Лучший результат начинается ниже любого возможного счёта, включая ноль: иначе
        // панель, где постоянный ответ не угадывает ничего, не отличалась бы от непосчитанной.
        var best = (Correct: -1, Answer: string.Empty);

        // Сколько кандидатов набрали тот же лучший счёт. Это записывается, а не
        // предполагается: при полностью различных ожидаемых ответах их столько же, сколько
        // строк, и читатель видит, что предел достигается любым отдельным ответом.
        var tiedAtBest = 0;

        // Каждый кандидат оценивается как постоянный ответчик на всей панели.
        foreach (var candidate in candidates)
        {
            // Сколько строк панели угадал бы этот постоянный ответ тем же правилом верности.
            var correct = panel.Count(row => AnswerIsCorrect(candidate, row.Gold));

            // Строгое сравнение оставляет ПЕРВОГО из равных, а порядок панели закреплён,
            // поэтому записанный лучший ответ воспроизводится от прогона к прогону.
            if (correct > best.Correct)
            {
                best = (correct, candidate);
                tiedAtBest = 1;
            }
            else if (correct == best.Correct)
            {
                tiedAtBest++;
            }
        }

        // Предел отдаётся вместе с независимым знаменателем и самим лучшим ответом.
        return new ConstantAnswerCeilingResult(
            Correct: best.Correct,
            PanelRows: panel.Length,
            BestConstantAnswer: best.Answer,
            CandidatesConsidered: candidates.Length,
            CandidatesTiedAtBest: tiedAtBest);
    }

    // DecideReceipt — ЕДИНСТВЕННЫЙ валидатор действительности этого прогона. Он отвечает на
    // один вопрос — можно ли вообще считать прогон состоявшимся измерением — и его ответ
    // определяет сразу все поля расписки. Образец перенесён из сорок восьмого опыта
    // (`Tests/ExperimentFixes/P4801WeightingCurriculumCeilingTests.cs`, запись
    // `ComparisonVerdict` строки 100-105 и метод `DecideComparison` строки 118-157):
    // список проблем собирается по правилам, действительность есть РОВНО отсутствие проблем,
    // отдельного флага, способного разойтись со списком причин, нет.
    //
    // Что считается проблемой и почему именно это. Проблема — только то, без чего прогон
    // перестаёт быть измерением. Обучение обязано состояться, иначе измерять нечего. Панель
    // обязана быть непустой. Расчёт предела обязан дать кандидата. Каждое число верных
    // обязано лежать в границах панели. Равный старт обязан держаться, иначе плечи начинали с
    // разного и сравнивать их нельзя. Контроль измерителя обязан сработать, иначе равенство
    // состояний плеч A и B неотличимо от слепоты опыта.
    //
    // Чего здесь СОЗНАТЕЛЬНО нет. Условие «счёт панели экзамена не превысил предел
    // постоянного ответа» проблемой НЕ делается. Действительность этого опыта не опирается на
    // панель экзамена: вторая ось объявлена вырожденной по устройству продукта, и это сам
    // результат опыта, а не его поломка. Состоявшееся обучение доказывает контроль обучения
    // на САМИХ учебных строках, поэтому «обучение не состоялось» считается по нему, а не по
    // панели. Непревышение предела записывается отдельно как граница вывода.
    private static ExperimentReceipt DecideReceipt(
        int trainColdCorrect,
        int trainAfterCorrect,
        int coldCorrect,
        ConstantAnswerCeilingResult ceiling,
        ArmResult armA,
        ArmResult armB,
        ArmResult armControl)
    {
        // Список проблем собирается по всем правилам; пустой список и есть действительность.
        var problems = new List<string>();

        // Размер панели берётся из самого расчёта предела: знаменатель один для всех чисел.
        var panelRows = ceiling.PanelRows;

        // Обучение обязано состояться, и доказывает это контроль обучения на самих учебных
        // строках, а НЕ панель экзамена: панель собрана из других предложений.
        if (trainAfterCorrect <= trainColdCorrect)
            problems.Add($"обучение не состоялось: на самих учебных строках до обучения верных " +
                         $"{trainColdCorrect}, после обучения {trainAfterCorrect} — роста нет");

        // Пустая панель означает, что мерить было нечего, и любое число оказалось бы выдумкой.
        if (panelRows <= 0)
            problems.Add("панель экзамена пуста: измерять нечего, и любое число было бы выдумано");

        // Расчёт предела обязан рассмотреть хотя бы одного кандидата и дать неотрицательный счёт.
        if (ceiling.CandidatesConsidered <= 0 || ceiling.Correct < 0)
            problems.Add($"расчёт предела постоянного ответа не дал кандидата: кандидатов " +
                         $"{ceiling.CandidatesConsidered}, лучший счёт {ceiling.Correct}");

        // Каждое число верных обязано лежать в диапазоне от нуля до размера панели.
        foreach (var (name, value) in new (string Name, int Value)[]
                 {
                     ("холодная панель", coldCorrect),
                     ("плечо A", armA.PanelCorrectAfter),
                     ("плечо B", armB.PanelCorrectAfter),
                     ("контрольное плечо", armControl.PanelCorrectAfter),
                     ("предел постоянного ответа", ceiling.Correct)
                 })
        {
            // Число вне границ панели не бывает наблюдением: это порча счёта или знаменателя.
            if (value < 0 || value > panelRows)
                problems.Add($"«{name}»: верных {value} вне диапазона от 0 до {panelRows}");
        }

        // Равный старт: после установочных уроков все три плеча обязаны совпасть побайтово.
        if (!string.Equals(armA.StateAfterSetupHash, armB.StateAfterSetupHash, StringComparison.Ordinal))
            problems.Add("равный старт нарушен: обученные состояния плеч A и B после установки различны");

        // То же требование к контрольному плечу: иначе контроль измеряет не ту же точку старта.
        if (!string.Equals(armA.StateAfterSetupHash, armControl.StateAfterSetupHash, StringComparison.Ordinal))
            problems.Add("равный старт нарушен: обученные состояния плеча A и контрольного плеча " +
                         "после установки различны");

        // Контроль измерителя обязан увидеть изменение состояния там, где оно есть.
        if (!armControl.StateChangedByCorrectiveLesson)
            problems.Add("контроль измерителя не сработал: корректирующий урок не изменил обученное " +
                         "состояние контрольного плеча");

        // И это изменение обязано отличать контрольное плечо от плеча A.
        if (string.Equals(armControl.StateAfterCorrectiveHash, armA.StateAfterCorrectiveHash, StringComparison.Ordinal))
            problems.Add("контроль измерителя не сработал: состояние контрольного плеча совпало с " +
                         "состоянием плеча A, значит опыт не способен увидеть разницу состояния");

        // Действительность — это ровно отсутствие проблем.
        var valid = problems.Count == 0;

        // Состояние гипотезы выводится из первой оси, а не назначается: гипотеза карточки
        // говорит, что урок из фактического ошибочного ответа даёт пригодный контрпример, а
        // заготовленная строка корпуса — нет. Неподтверждение здесь законный исход опыта, а
        // не поломка, поэтому в список проблем оно не входит.
        var hypothesisSupported = !armA.CounterExampleUsable && armB.CounterExampleUsable;

        // Все поля расписки выводятся из одного решения.
        return new ExperimentReceipt(
            Valid: valid,
            ExecutionStatus: valid ? "COMPLETED" : "INCOMPLETE_EXPERIMENT_INVARIANT_VIOLATED",
            LearningValidity: valid ? "VALID_FOR_NAMED_UNIT" : "INVALID_EXPERIMENT_INVARIANT_VIOLATED",
            HypothesisStatus: !valid
                ? "OPEN_EXPERIMENT_INVARIANT_VIOLATED"
                : hypothesisSupported ? "SUPPORTED_FOR_NAMED_METRIC" : "NOT_SUPPORTED_FOR_NAMED_METRIC",
            ExitCode: valid ? 0 : 1,
            Problems: [.. problems]);
    }

    // RunPanel прогоняет замороженную панель через обычную цепочку «разборщик → исполнитель»
    // и возвращает построчные наблюдения.
    private static PanelRow[] RunPanel(UnitSet units)
    {
        // Наблюдения панели в порядке самой панели.
        var rows = new List<PanelRow>();

        // Панель проходится построчно: итог без строк скрыл бы обмен местами двух строк.
        foreach (var row in ExamRows())
        {
            // Обычное чтение начинается обученным разборщиком — это первое звено цепочки.
            var sequence = units.Tokenizer.Tokenize(row.Source, SourceLanguage);

            // Ответ обычного потребителя на тот же разбор.
            var candidate = units.Executor.Execute(sequence, TopicDomain);

            // Верность ответа судится общим правилом опыта, тем же, что и предел постоянного ответа.
            rows.Add(new PanelRow(
                Id: row.Id,
                Source: row.Source,
                Gold: row.Gold,
                Answer: candidate.Text,
                Adapter: candidate.AdapterId,
                Correct: AnswerIsCorrect(candidate.Text, row.Gold)));
        }

        // Панель отдаётся целиком.
        return [.. rows];
    }

    // RunTrainControl прогоняет через обычную цепочку сами УЧЕБНЫЕ строки. Это назначенный
    // карточкой контроль обучения: единица обязана отвечать на то, чему её учили. Панель
    // экзамена собрана из других предложений, поэтому рост точности на ней не гарантирован,
    // и его отсутствие само по себе не означает отсутствия обучения.
    private static PanelRow[] RunTrainControl(UnitSet units)
    {
        // Наблюдения по учебным строкам в их собственном порядке.
        var rows = new List<PanelRow>();

        // Каждая учебная строка предъявляется обычному потребителю как обычный запрос.
        foreach (var row in TrainRows())
        {
            // Та же цепочка «разборщик → исполнитель», что и на экзамене.
            var sequence = units.Tokenizer.Tokenize(row.Source, SourceLanguage);

            // Ответ обычного потребителя на учебную строку.
            var candidate = units.Executor.Execute(sequence, TopicDomain);

            // Верность судится тем же общим правилом опыта, что и на панели экзамена.
            rows.Add(new PanelRow(
                Id: row.Id,
                Source: row.Source,
                Gold: row.Gold,
                Answer: candidate.Text,
                Adapter: candidate.AdapterId,
                Correct: AnswerIsCorrect(candidate.Text, row.Gold)));
        }

        // Контроль обучения отдаётся целиком.
        return [.. rows];
    }

    // StateHash снимает слепок обученного состояния исполнителя и возвращает его как текст.
    // Сравнение этих текстов — и есть точная проверка равенства обученных состояний.
    private static string StateHash(UnitSet units)
    {
        // Слепок состояния берётся продуктовым средством выгрузки обученной единицы.
        var checkpoint = units.Executor.ExportCheckpoint();

        // Слепок приводится к тексту с устойчивым порядком полей.
        var json = JsonSerializer.Serialize(checkpoint, new JsonSerializerOptions
        {
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        });

        // Текст слепка и есть сравниваемое значение состояния.
        return json;
    }

    // RunArm обучает свежую связку установочной партией, добавляет один корректирующий урок
    // выбранного плеча и снимает обе оси.
    private static ArmResult RunArm(
        string armName,
        CorrectiveRejectedSource source,
        string wrongExamSource,
        string wrongExamGold,
        string actualWrongAnswer,
        List<string> log)
    {
        // Свежая связка единиц: у плеча своя память, не общая с другими плечами.
        var units = NewUnits();

        // Установочная партия одна и та же у всех плеч.
        var setupLessons = TrainBatch(units, BuildSetupBatch($"P5801-setup-{armName}"));

        // Слепок состояния ПОСЛЕ установки и ДО корректирующего урока: именно он должен
        // совпасть у всех плеч, иначе плечи начинали с разного и сравнение недействительно.
        var stateAfterSetup = StateHash(units);

        // Текст стороны «отвергнуто» этого плеча.
        var rejectedSide = CorrectiveLessonCounterExampleArms.BuildRejectedSide(source, actualWrongAnswer);

        // Отказ составления стороны «отвергнуто» останавливает плечо здесь же.
        Assert.True(rejectedSide.IsSuccess, $"плечо {armName}: {rejectedSide.ErrorMessage}");

        // Корректирующий урок собирается настоящим продуктовым сборщиком.
        var corrective = CorrectiveLessonCounterExampleArms.BuildCorrectiveBatch(
            $"P5801-corrective-{armName}", wrongExamSource, wrongExamGold, rejectedSide.Value!);

        // Отказ сборки корректирующего урока — наблюдение, а не повод подменить данные.
        Assert.True(corrective.IsSuccess, $"плечо {armName}: {corrective.ErrorMessage}");

        // ОСЬ ПЕРВАЯ: содержание урока. Есть ли в нём пригодный контрпример.
        var counterExample = CorrectiveLessonCounterExampleArms.MeasureCounterExample(
            corrective.Value!, Direction);

        // Измерение первой оси отказать не может на собранной партии.
        Assert.True(counterExample.IsSuccess, $"плечо {armName}: {counterExample.ErrorMessage}");

        // Корректирующий урок подаётся тем же продуктовым методом обучения.
        var correctiveLessons = TrainBatch(units, corrective.Value!);

        // Слепок состояния ПОСЛЕ корректирующего урока.
        var stateAfterCorrective = StateHash(units);

        // ОСЬ ВТОРАЯ: ответ обычного потребителя после корректирующего урока.
        var panelAfter = RunPanel(units);

        // Число верных ответов панели после вмешательства.
        var correctAfter = panelAfter.Count(row => row.Correct);

        // Строки журнала прогона: видно и содержание урока, и ответ, и изменение состояния.
        log.Add($"плечо {armName}: установочных уроков {setupLessons}, корректирующих {correctiveLessons}; " +
                $"сторона «отвергнуто» = «{rejectedSide.Value}»");
        log.Add($"плечо {armName}: в уроке «выбрано» = «{counterExample.Value!.ChosenText}», " +
                $"«отвергнуто» = «{counterExample.Value!.RejectedText}», тип отказа «{counterExample.Value!.RejectedType}»; " +
                $"пригодный контрпример: {(counterExample.Value!.CounterExampleIsUsable ? "есть" : "НЕТ")}");
        log.Add($"плечо {armName}: обученное состояние корректирующим уроком " +
                $"{(stateAfterSetup != stateAfterCorrective ? "ИЗМЕНЕНО" : "не изменено")}; " +
                $"панель после: верных {correctAfter} из {panelAfter.Length}");

        // Итог плеча отдаётся опыту.
        return new ArmResult(
            ArmName: armName,
            RejectedSource: source.ToString(),
            RejectedSideText: rejectedSide.Value!,
            CounterExampleUsable: counterExample.Value!.CounterExampleIsUsable,
            LessonChosenText: counterExample.Value!.ChosenText,
            LessonRejectedText: counterExample.Value!.RejectedText,
            LessonRejectedType: counterExample.Value!.RejectedType,
            StateAfterSetupHash: stateAfterSetup,
            StateAfterCorrectiveHash: stateAfterCorrective,
            StateChangedByCorrectiveLesson: stateAfterSetup != stateAfterCorrective,
            CorrectiveLessons: correctiveLessons,
            PanelCorrectAfter: correctAfter,
            PanelAfter: panelAfter);
    }

    // Основной прогон опыта: холодная панель, установка, выбор ошибочной строки, два плеча,
    // контроль измерителя, затем запись доказательств в НОВУЮ попытку.
    [Fact]
    public void PreparedCorpusLineAndActualWrongOutputAreMeasuredOnBothAxesSeparately()
    {
        // Журнал прогона сохраняется рядом с числами как сырое свидетельство.
        var log = new List<string>();

        // ВЕЛИЧИНА ПЕРВАЯ — НАБЛЮДЁННЫЙ ХОЛОДНЫЙ СЧЁТ: панель на совсем необученных единицах.
        // Это НАБЛЮДЕНИЕ: сколько необученная модель ответила верно на самом деле.
        var coldUnits = NewUnits();
        var coldPanel = RunPanel(coldUnits);
        var coldCorrect = coldPanel.Count(row => row.Correct);
        log.Add($"наблюдённый холодный счёт (необученная модель на панели): верных {coldCorrect} " +
                $"из {coldPanel.Length}");

        // ВЕЛИЧИНА ВТОРАЯ — ПРЕДЕЛ ПОСТОЯННОГО ОТВЕТА: сколько набрал бы наилучший
        // единственный постоянный ответ, который вход не читает вовсе. Это РАСЧЁТ по частотам
        // ожидаемых ответов, без единого вызова модели, и с холодным счётом он совпадает
        // только случайно. До этой правки сюда подставлялся холодный счёт, то есть одна
        // величина выдавалась за другую.
        var ceiling = ComputeConstantAnswerCeiling();
        log.Add($"предел постоянного ответа (расчёт, без вызовов модели): верных {ceiling.Correct} " +
                $"из {ceiling.PanelRows}; лучший постоянный ответ «{ceiling.BestConstantAnswer}»; " +
                $"кандидатов {ceiling.CandidatesConsidered}, из них с тем же лучшим счётом " +
                $"{ceiling.CandidatesTiedAtBest}");

        // Установка: та же партия из восьми строк на отдельной связке — она нужна, чтобы
        // выбрать первую ошибочную строку панели по закреплённому порядку.
        var setupUnits = NewUnits();
        var setupLessons = TrainBatch(setupUnits, BuildSetupBatch("P5801-setup-probe"));
        var panelAfterSetup = RunPanel(setupUnits);
        var correctAfterSetup = panelAfterSetup.Count(row => row.Correct);
        log.Add($"после установочных уроков ({setupLessons} уроков): верных {correctAfterSetup} из {panelAfterSetup.Length}");

        // Сырое свидетельство установки записывается СРАЗУ, до любой проверки: если
        // проверка ниже не пройдёт, наблюдение всё равно останется на диске, а не исчезнет.
        WriteEvidence("setup-probe.json", JsonSerializer.Serialize(new
        {
            опыт = "P58-PRIMARY-01",
            попытка = "0002",
            что_это = "установочный прогон: холодная панель и панель после восьми учебных строк",
            холодная_панель = new { верных = coldCorrect, всего = coldPanel.Length, строки = coldPanel },
            после_установки = new
            {
                уроков = setupLessons,
                верных = correctAfterSetup,
                всего = panelAfterSetup.Length,
                строки = panelAfterSetup
            }
        }, ReadableJson()));

        // КОНТРОЛЬ ОБУЧЕНИЯ, назначенный карточкой: единица обязана отвечать на то, чему её
        // учили. Это и есть доказательство, что обучение состоялось, а рост точности на
        // экзамене из ДРУГИХ предложений не обязателен и доказательством не является.
        var trainControlCold = RunTrainControl(coldUnits);
        var trainControlAfter = RunTrainControl(setupUnits);
        var trainColdCorrect = trainControlCold.Count(row => row.Correct);
        var trainAfterCorrect = trainControlAfter.Count(row => row.Correct);
        log.Add($"контроль обучения на самих учебных строках: до обучения верных {trainColdCorrect} из " +
                $"{trainControlCold.Length}, после обучения {trainAfterCorrect} из {trainControlAfter.Length}");

        // Контроль обучения тоже сохраняется сразу, до проверок.
        WriteEvidence("train-control.json", JsonSerializer.Serialize(new
        {
            опыт = "P58-PRIMARY-01",
            попытка = "0002",
            что_это = "контроль обучения: ответы обычного потребителя на САМИ учебные строки",
            зачем = "панель экзамена собрана из других предложений, поэтому доказательством " +
                    "состоявшегося обучения служит ответ на то, чему учили",
            до_обучения = new { верных = trainColdCorrect, всего = trainControlCold.Length, строки = trainControlCold },
            после_обучения = new { верных = trainAfterCorrect, всего = trainControlAfter.Length, строки = trainControlAfter }
        }, ReadableJson()));

        // Обученное состояние обязано измениться относительно холодного.
        var coldState = StateHash(coldUnits);
        var setupState = StateHash(setupUnits);
        log.Add($"обученное состояние после установки {(coldState != setupState ? "ИЗМЕНЕНО" : "не изменено")} " +
                $"относительно холодного");
        Assert.NotEqual(coldState, setupState);

        // И единица обязана отвечать на учебные строки лучше, чем до обучения. Без этого
        // установка была бы записью в память, которую обычный потребитель не читает.
        Assert.True(trainAfterCorrect > trainColdCorrect,
            $"обучение не состоялось: на самих учебных строках до {trainColdCorrect} из " +
            $"{trainControlCold.Length}, после {trainAfterCorrect} из {trainControlAfter.Length}");

        // Первая ошибочная строка панели по закреплённому порядку и есть предмет исправления.
        var wrongRow = panelAfterSetup.FirstOrDefault(row => !row.Correct);

        // Если ошибок нет вовсе, карточка требует записать это и не создавать обучение ради
        // прохождения. Тогда опыт останавливается здесь честным наблюдением.
        Assert.True(wrongRow is not null,
            "на панели нет ни одной ошибки: по карточке это итог без нового обучения");
        log.Add($"выбрана первая ошибочная строка «{wrongRow!.Id}»: запрос «{wrongRow.Source}», " +
                $"эталон «{wrongRow.Gold}», фактический ответ «{wrongRow.Answer}»");

        // ПЛЕЧО A: сторона «отвергнуто» — заготовленная строка корпуса.
        var armA = RunArm("A-prepared-corpus-line", CorrectiveRejectedSource.PreparedCorpusString,
            wrongRow.Source, wrongRow.Gold, wrongRow.Answer, log);

        // ПЛЕЧО B: сторона «отвергнуто» — фактический ошибочный ответ той же строки.
        var armB = RunArm("B-actual-wrong-output", CorrectiveRejectedSource.ActualWrongOutput,
            wrongRow.Source, wrongRow.Gold, wrongRow.Answer, log);

        // КОНТРОЛЬ ИЗМЕРИТЕЛЯ: сторона «отвергнуто» с явной фразовой пометкой. Она по
        // устройству продукта попадает в сторону «выбрано», поэтому обученное состояние
        // ОБЯЗАНО измениться. Это доказывает, что измеритель видит настоящую разницу.
        var armControl = RunArm("C-phrase-marker-instrument-control",
            CorrectiveRejectedSource.PhraseMarkerInstrumentControl,
            wrongRow.Source, wrongRow.Gold, wrongRow.Answer, log);

        // РАСПИСКА ПРОГОНА ВЫВОДИТСЯ ЗДЕСЬ, единственным валидатором, и дальше только
        // читается. Литералов «завершено», «ноль» и «проблем нет» внизу больше нет: документ,
        // который пишет завершённость литералом, не умеет сообщить о незавершённости.
        var receipt = DecideReceipt(
            trainColdCorrect, trainAfterCorrect, coldCorrect, ceiling, armA, armB, armControl);
        log.Add($"расписка прогона: {receipt.ExecutionStatus}, код возврата {receipt.ExitCode}, " +
                $"проблем {receipt.Problems.Length}");

        // Каждая найденная проблема попадает в журнал по-человечески, а не только в расписку.
        foreach (var problem in receipt.Problems)
            log.Add("проблема прогона: " + problem);

        // РАВНЫЙ СТАРТ: после установочных уроков все три плеча обязаны иметь одно и то же
        // обученное состояние. Иначе плечи начинали с разного и сравнение недействительно.
        Assert.Equal(armA.StateAfterSetupHash, armB.StateAfterSetupHash);
        Assert.Equal(armA.StateAfterSetupHash, armControl.StateAfterSetupHash);

        // ОСЬ ПЕРВАЯ, НЕВЫРОЖДЕННАЯ: фактор доходит до содержания урока и меняет его.
        // Плечо A получает контрпример, равный правильному ответу, то есть не получает его
        // вовсе; плечо B получает настоящий отличающийся контрпример.
        Assert.False(armA.CounterExampleUsable,
            "плечо A обязано остаться без пригодного контрпримера: это и есть измеряемый дефект");
        Assert.True(armB.CounterExampleUsable,
            "плечо B обязано получить настоящий контрпример, иначе фактор не применён");
        Assert.Equal(armA.LessonChosenText, armA.LessonRejectedText);
        Assert.NotEqual(armB.LessonChosenText, armB.LessonRejectedText);

        // Тексты урока названы прямо: в плече A «отвергнуто» равно эталону, в плече B —
        // фактическому ошибочному ответу машины.
        Assert.Equal(wrongRow.Gold, armA.LessonRejectedText);
        Assert.Equal(wrongRow.Answer, armB.LessonRejectedText);

        // ОСЬ ВТОРАЯ, ВЫРОЖДЕННАЯ ПО УСТРОЙСТВУ ПРОДУКТА. Здесь важна точная формулировка,
        // и первое измерение меня поправило: корректирующий урок ВСЁ-ТАКИ меняет обученное
        // состояние — через сторону «выбрано», которую единица читает и запоминает.
        Assert.NotEqual(armA.StateAfterSetupHash, armA.StateAfterCorrectiveHash);
        Assert.NotEqual(armB.StateAfterSetupHash, armB.StateAfterCorrectiveHash);
        Assert.True(armA.StateChangedByCorrectiveLesson);
        Assert.True(armB.StateChangedByCorrectiveLesson);

        // Но вырожденность в другом, и вот она: изменение состояния у двух плеч
        // ПОБАЙТОВО ОДИНАКОВО. Фактор опыта — сторона «отвергнуто» — не оставляет в
        // обученном состоянии никакого следа вовсе, потому что единица её не читает.
        Assert.Equal(armA.StateAfterCorrectiveHash, armB.StateAfterCorrectiveHash);

        // Поэтому и ответы панели у двух плеч совпадают построчно, а не только в итоге.
        Assert.Equal(armA.PanelCorrectAfter, armB.PanelCorrectAfter);
        for (var line = 0; line < armA.PanelAfter.Length; line++)
            Assert.Equal(armA.PanelAfter[line].Answer, armB.PanelAfter[line].Answer);

        // КОНТРОЛЬ ИЗМЕРИТЕЛЯ ОБЯЗАН СРАБОТАТЬ: у третьего плеча сторона «отвергнуто» несёт
        // фразовую пометку, которая по устройству попадает в сторону «выбрано». Его состояние
        // обязано ОТЛИЧАТЬСЯ от состояния плеч A и B. Без этой строки равенство состояний
        // выше нельзя было бы отличить от слепоты измерителя.
        Assert.NotEqual(armA.StateAfterCorrectiveHash, armControl.StateAfterCorrectiveHash);
        Assert.True(armControl.StateChangedByCorrectiveLesson,
            "контроль измерителя не сработал: значит опыт не способен увидеть разницу состояния, " +
            "и вывод о вырожденности второй оси недействителен");

        // Доказательство «до»: холодная панель и панель после установки.
        WriteEvidence("before.json", JsonSerializer.Serialize(new
        {
            опыт = "P58-PRIMARY-01",
            попытка = "0002",
            подслучай = "P58.1",
            профиль_проверки = "D04 EN_RU",
            набор = "заморожен; дословно из эталона первой попытки before.json",
            холодная_панель = new { верных = coldCorrect, всего = coldPanel.Length, строки = coldPanel },
            // Предел постоянного ответа записывается рядом с холодной панелью, но отдельным
            // полем: это расчёт, а не наблюдение, и путать их нельзя.
            предел_постоянного_ответа = new
            {
                верных = ceiling.Correct,
                всего = ceiling.PanelRows,
                лучший_постоянный_ответ = ceiling.BestConstantAnswer,
                кандидатов_рассмотрено = ceiling.CandidatesConsidered,
                кандидатов_с_тем_же_лучшим_счётом = ceiling.CandidatesTiedAtBest,
                вызовов_модели = 0
            },
            после_установочных_уроков = new
            {
                уроков = setupLessons,
                верных = correctAfterSetup,
                всего = panelAfterSetup.Length,
                строки = panelAfterSetup
            },
            выбранная_ошибочная_строка = wrongRow
        }, ReadableJson()));

        // Доказательство плеч: все три плеча целиком.
        WriteEvidence("arms.json", JsonSerializer.Serialize(new
        {
            опыт = "P58-PRIMARY-01",
            попытка = "0002",
            единственный_фактор = "источник стороны «отвергнуто» корректирующего урока",
            // Равный старт ВЫВОДИТСЯ из снятых слепков, а не утверждается литералом: литерал
            // «подтверждён» остался бы истиной и у прогона с разошедшимся стартом.
            равный_старт_подтверждён =
                armA.StateAfterSetupHash == armB.StateAfterSetupHash &&
                armA.StateAfterSetupHash == armControl.StateAfterSetupHash,
            плечо_A_строка_корпуса = armA,
            плечо_B_фактический_ошибочный_ответ = armB,
            контроль_измерителя_фразовая_пометка = armControl
        }, ReadableJson()));

        // Показатели обеих осей рядом, с честным названием каждой.
        WriteEvidence("metrics.json", JsonSerializer.Serialize(new
        {
            опыт = "P58-PRIMARY-01",
            подслучай = "P58.1",
            профиль_проверки = "D04 EN_RU",
            // Состоявшееся обучение ВЫВОДИТСЯ из контроля обучения, а не утверждается
            // литералом: литерал «состоялось» пережил бы и прогон, где обучения не было.
            обучение_состоялось = trainAfterCorrect > trainColdCorrect,
            уроков_установочных_на_плечо = setupLessons,
            // Число корректирующих уроков берётся из фактически поданной партии плеча.
            уроков_корректирующих_на_плечо = armA.CorrectiveLessons,
            ось_1_содержание_урока = new
            {
                состояние = "НЕВЫРОЖДЕНА: фактор доходит и меняет урок",
                пригодных_контрпримеров_плечо_A = armA.CounterExampleUsable ? 1 : 0,
                пригодных_контрпримеров_плечо_B = armB.CounterExampleUsable ? 1 : 0,
                отвергнуто_в_уроке_плечо_A = armA.LessonRejectedText,
                отвергнуто_в_уроке_плечо_B = armB.LessonRejectedText,
                пояснение = "в плече A сторона «отвергнуто» равна правильному ответу, значит " +
                            "контрпримера в уроке нет вовсе; в плече B это настоящий ошибочный ответ"
            },
            ось_2_ответ_потребителя = new
            {
                состояние = "ВЫРОЖДЕНА ПО УСТРОЙСТВУ ПРОДУКТА, а не из-за набора",
                верных_холодная = coldCorrect,
                верных_после_установки = correctAfterSetup,
                верных_плечо_A = armA.PanelCorrectAfter,
                верных_плечо_B = armB.PanelCorrectAfter,
                изменение_между_плечами = armB.PanelCorrectAfter - armA.PanelCorrectAfter,
                урок_изменил_состояние_плечо_A = armA.StateChangedByCorrectiveLesson,
                урок_изменил_состояние_плечо_B = armB.StateChangedByCorrectiveLesson,
                состояния_двух_плеч_побайтово_одинаковы =
                    armA.StateAfterCorrectiveHash == armB.StateAfterCorrectiveHash,
                в_чём_именно_вырожденность = "корректирующий урок состояние МЕНЯЕТ — через сторону " +
                    "«выбрано», которую единица читает и запоминает. Но изменение у двух плеч " +
                    "побайтово одинаково: фактор опыта, сторона «отвергнуто», не оставляет в " +
                    "обученном состоянии никакого следа вовсе",
                причина_из_кода = "27.TranslationDomain/TranslationTrainedUnits.cs, метод " +
                                  "ExecutorUnit.TrainOnExample, строки 570-631: разбирается только " +
                                  "сторона «выбрано»; сторона «отвергнуто» не читается вовсе"
            },
            // ДВЕ РАЗНЫЕ ВЕЛИЧИНЫ, каждая под своим именем. До этой правки на месте предела
            // стоял наблюдённый холодный счёт, то есть одно число записывалось дважды: один
            // раз верно, второй — под чужим именем.
            наблюдённый_холодный_счёт = new
            {
                что_это = "НАБЛЮДЕНИЕ: сколько необученная модель ответила верно НА САМОМ ДЕЛЕ",
                верных = coldCorrect,
                всего = coldPanel.Length,
                как_получено = "прогон панели через обычную цепочку на совсем необученных единицах"
            },
            контроль_постоянного_ответа = new
            {
                что_это = "РАСЧЁТ: сколько набрал бы наилучший единственный постоянный ответ, " +
                          "который вход не читает вовсе — максимум по частотам ожидаемых ответов",
                чем_отличается_от_холодного_счёта = "это две разные величины: холодный счёт — " +
                    "наблюдение необученной модели; предел постоянного ответа — расчётный максимум " +
                    "стратегии «всегда отвечать одно и то же». Совпасть они могут только случайно",
                верных = ceiling.Correct,
                всего = ceiling.PanelRows,
                лучший_постоянный_ответ = ceiling.BestConstantAnswer,
                кандидатов_рассмотрено = ceiling.CandidatesConsidered,
                кандидатов_с_тем_же_лучшим_счётом = ceiling.CandidatesTiedAtBest,
                вызовов_модели = 0,
                почему_столько = "все ожидаемые ответы панели различны, поэтому любой единственный " +
                                 "постоянный ответ совпадает ровно с одной строкой, а пустая строка — " +
                                 "ни с одной",
                вывод = "плечо с таким же счётом на этой панели неотличимо от постоянного ответа"
            },
            // Непревышение предела — это ГРАНИЦА ВЫВОДА, а не поломка опыта: вторая ось
            // объявлена вырожденной по устройству продукта, и это сам результат опыта.
            граница_панели_экзамена = new
            {
                счёт_плеч_превышает_предел_постоянного_ответа =
                    armA.PanelCorrectAfter > ceiling.Correct && armB.PanelCorrectAfter > ceiling.Correct,
                что_это_значит = armA.PanelCorrectAfter > ceiling.Correct
                    ? "счёт плеч выше предела постоянного ответа, значит панель экзамена что-то доказывает"
                    : "счёт плеч НЕ выше предела постоянного ответа: на панели экзамена плечи " +
                      "неотличимы от наилучшего постоянного ответа, и панель экзамена доказательством " +
                      "состоявшегося обучения не является",
                чем_доказано_обучение = "контролем обучения на САМИХ учебных строках: " +
                    $"верных {trainColdCorrect} из {trainControlCold.Length} до обучения и " +
                    $"{trainAfterCorrect} из {trainControlAfter.Length} после"
            },
            контроль_измерителя = new
            {
                пояснение = "плечо с явной фразовой пометкой: она попадает в сторону «выбрано», " +
                            "её единица читает, поэтому состояние обязано измениться",
                состояние_изменено = armControl.StateChangedByCorrectiveLesson,
                вывод = "измеритель видит настоящую разницу состояния, значит равенство состояний " +
                        "в плечах A и B — свойство продукта, а не слепота опыта"
            },
            граница_вывода = "измерены содержание корректирующего урока и ответ обычного потребителя " +
                             "на одной замороженной панели; качество перевода вне этой панели не измерялось"
        }, ReadableJson()));

        // Сырой журнал прогона сохраняется как есть.
        WriteEvidence(Path.Combine("raw", "run-train.stdout.txt"), string.Join("\n", log) + "\n");

        // Расписка прогона: код выхода, состояние исполнения и действительность обучения.
        // Документ собирается отдельным способом `ComposeStatusPayload` — см. причину там.
        WriteEvidence("status.json", JsonSerializer.Serialize(
            ComposeStatusPayload(receipt, ceiling, coldCorrect, armA, armB, armControl), ReadableJson()));

        // Человеческий отчёт складывается из ТЕХ ЖЕ снятых чисел, а не переписывается
        // руками: тогда числа отчёта и числа доказательств разойтись не могут.
        WriteEvidence("report.md", ComposeReport(
            coldCorrect, correctAfterSetup, setupLessons,
            trainColdCorrect, trainAfterCorrect,
            ceiling, receipt, wrongRow, armA, armB, armControl));

        // Расписка проверяется последней, уже после записи всех показаний: так испорченный
        // прогон остаётся на диске вместе со своими причинами, а не исчезает вместе с отказом.
        Assert.True(receipt.Valid,
            "прогон признан недействительным: " + string.Join("; ", receipt.Problems));
    }

    // ComposeStatusPayload собирает документ расписки ИЗ решения валидатора и больше ниоткуда.
    //
    // Зачем он выделен отдельным способом, а не оставлен внутри прогона. На ЗДОРОВЫХ данных
    // литерал «COMPLETED» и вывод валидатора совпадают, поэтому подмену вывода литералом на
    // здоровом прогоне поймать невозможно в принципе. Поймать её можно только позвав сборку
    // документа с НЕДЕЙСТВИТЕЛЬНОЙ распиской и потребовав, чтобы документ сообщил
    // незавершённость. Для этого сборку надо уметь позвать отдельно — ровно поэтому она здесь.
    // Пока три поля стояли литералами внутри прогона, такой проверки не существовало вовсе.
    private static object ComposeStatusPayload(
        ExperimentReceipt receipt,
        ConstantAnswerCeilingResult ceiling,
        int coldCorrect,
        ArmResult armA,
        ArmResult armB,
        ArmResult armControl)
        => new
        {
            experiment = "P58-PRIMARY-01",
            attempt = "0002",
            subcase = "P58.1",
            // ВСЕ поля расписки ниже выводятся из единственного валидатора `DecideReceipt` и
            // больше ниоткуда. Прежде здесь стояли литералы «COMPLETED», 0 и пустой список
            // проблем, и такой документ физически не мог сообщить о незавершённости.
            execution_status = receipt.ExecutionStatus,
            learning_validity = receipt.LearningValidity,
            // Гипотеза карточки о том, что урок должен строиться из фактического ошибочного
            // ответа, выводится из первой оси: пригодных контрпримеров у плеча A против плеча B.
            hypothesis_status = receipt.HypothesisStatus,
            hypothesis_metric = "пригодность контрпримера в корректирующем уроке",
            // Вторая ось названа отдельно и не выдаётся за подтверждение.
            answer_axis_verdict = "DEGENERATE_BY_PRODUCT_DESIGN_EXECUTOR_NEVER_READS_REJECTED_SIDE",
            instrument_control_detected_real_state_change = armControl.StateChangedByCorrectiveLesson,
            // Предел постоянного ответа попадает и в расписку: читатель видит, с чем сравнивать
            // счёт панели, не открывая другой файл.
            constant_answer_ceiling = ceiling.Correct,
            constant_answer_ceiling_denominator = ceiling.PanelRows,
            observed_cold_panel_correct = coldCorrect,
            exam_panel_beats_constant_answer_ceiling =
                armA.PanelCorrectAfter > ceiling.Correct && armB.PanelCorrectAfter > ceiling.Correct,
            exit_code = receipt.ExitCode,
            problems = receipt.Problems,
            finished_utc = DateTimeOffset.UtcNow.ToString("O")
        };

    // ComposeReport собирает русский отчёт опыта из фактически снятых наблюдений.
    private static string ComposeReport(
        int coldCorrect, int correctAfterSetup, int setupLessons,
        int trainColdCorrect, int trainAfterCorrect,
        ConstantAnswerCeilingResult ceiling, ExperimentReceipt receipt,
        PanelRow wrongRow, ArmResult armA, ArmResult armB, ArmResult armControl)
    {
        // Превышает ли счёт плеч предел постоянного ответа — вопрос, на который отчёт обязан
        // ответить честно, а не умолчать. Он решается один раз здесь и дальше только читается.
        var beatsCeiling = armA.PanelCorrectAfter > ceiling.Correct
                           && armB.PanelCorrectAfter > ceiling.Correct;

        // Если расписка нашла проблемы, отчёт начинается с них, а не с вывода опыта.
        var problemsBanner = receipt.Valid
            ? string.Empty
            : "> **ПРОГОН НЕДЕЙСТВИТЕЛЕН.** Расписка сообщила незавершённость, причины:\n" +
              string.Join("\n", receipt.Problems.Select(problem => "> - " + problem)) +
              "\n\n";
        // Строки панели после вмешательства, построчно по обоим плечам.
        var panel = string.Join("\n", armA.PanelAfter.Select((row, index) =>
            $"| {row.Id} {row.Source} | {row.Gold} | {row.Answer} | {armB.PanelAfter[index].Answer} | " +
            $"{(row.Answer == armB.PanelAfter[index].Answer ? "совпали" : "РАЗОШЛИСЬ")} |"));

        // Отчёт возвращается одной строкой: его записывает общий писатель доказательств.
        return $"""
# Пятьдесят восьмой опыт, вторая попытка: настоящий ошибочный ответ делает урок осмысленным, но до обученной памяти он не доходит по устройству продукта

Домен: предметная тема обучения — перевод; задача — английский→русский, семья пар «обратная связь как урок».
Исполнитель: модель текущей сессии (запрошено `opus` / `xhigh`; точное имя режима платформой не раскрыто).
Проверяющий: не выполнялась — независимого рецензента у этой попытки не было. Это самопроверка, и так она и называется.

{problemsBanner}execution_status: {receipt.ExecutionStatus}. learning_validity: {receipt.LearningValidity} —
обучаемые единицы `SemanticTokenizerUnit` и `ExecutorUnit` переводческой области.
hypothesis_status: {receipt.HypothesisStatus} по показателю «пригодность контрпримера в
корректирующем уроке»; по оси ответа обычного потребителя фактор не доходит, и это названо
отдельно, а не записано в подтверждение. Код возврата {receipt.ExitCode}, проблем
{receipt.Problems.Length}. Все эти поля выводит единственный валидатор прогона, а не литерал
внутри проверки: документ, который пишет «завершено» литералом, не умеет сообщить о
незавершённости.

Источники: паспорт карточки, раздел «Действующий эксперимент P58-PRIMARY-01» и подслучай
«урок строится из фактического ошибочного ответа»; набор заморожен и взят дословно из эталона
первой попытки. Сырые показания — `setup-probe.json`, `train-control.json`, `before.json`,
`arms.json`, `metrics.json`, `raw/run-train.stdout.txt`; расписка — `status.json`.
Первая попытка (`../attempt-0001`) не изменялась ни одним байтом.

## Главное, что надо понять с первого чтения

Отрицательный результат на второй оси — **это настоящий измеренный результат, а не недостаток
набора и не неудача опыта**. Вырожденность второй оси есть **свойство устройства продукта**:
единица, которая учится, физически не читает ту сторону урока, где лежит «вот так
неправильно». Поэтому любой выбор содержимого этой стороны для неё невидим. Опыт ответил на
вопрос карточки полностью; ответ оказался «фактор до этой оси не доходит, и вот почему».
Такой ответ ценнее зелёного числа, полученного подгонкой набора: зелёное число здесь можно
было бы получить, только подменив вопрос.

## Зачем это нужно продукту простыми словами

Машину учат переводу, она отвечает на экзамене и на одной строке отвечает неверно. По этой
ошибке составляют исправляющий урок, где рядом стоят правильный ответ и пример неправильного.
Вопрос: что должно лежать на месте неправильного примера — заготовленная строка корпуса или
настоящий ошибочный ответ самой машины, и меняет ли этот выбор хоть что-нибудь.

## Что меняли

Единственный фактор — источник стороны «отвергнуто» корректирующего урока. Плечо A берёт
заготовленную строку корпуса `[WRONG: missing translation]`. Плечо B берёт фактический
ошибочный ответ той же строки. Всё остальное одинаково: тот же замороженный набор, тот же
правильный ответ, тот же номер пары, тот же способ построения подписи, те же восемь
установочных уроков. Оба плеча зовут настоящий продуктовый сборщик учебных партий
`27.TranslationDomain/TranslationStructuredBatchBuilder.cs` и обучают настоящие продуктовые
единицы из `27.TranslationDomain/TranslationTrainedUnits.cs` их собственным методом обучения.
Копий продуктовой логики в опыте нет вовсе: ничего из нужного в продукте не удалено.

## На чём проверяли

Замороженный набор: восемь учебных строк и восемь экзаменационных, дословно из эталона первой
попытки. Бюджет: установочных уроков {setupLessons} на плечо (восемь учебных пар по три записи
на пару), корректирующих {armA.CorrectiveLessons} на плечо (одна пара по три записи) — это число
взято из фактически поданной партии, а не написано рядом рукой. Плеч три: два по карточке и одно
контрольное для измерителя.

**Равный старт доказан:** после установочных уроков обученное состояние всех трёх плеч
побайтово совпало, поэтому плечи начинали с одного и того же.

**Обучение настоящее, и доказано контролем обучения:** на самих учебных строках обычный
потребитель отвечал верно {trainColdCorrect} из 8 до обучения и {trainAfterCorrect} из 8 после.
Это и есть доказательство, что урок дошёл до памяти и читается обычным путём. Рост точности на
экзамене из ДРУГИХ предложений не требуется и доказательством обучения не является — первое
измерение меня в этом поправило, и я исправил не данные, а свою проверку.

## Что получилось

| Показатель, единица и область | холодная | после установки | плечо A | плечо B | Смысл |
|---|---:|---:|---:|---:|---|
| Пригодных контрпримеров в уроке, из 1 | — | — | {(armA.CounterExampleUsable ? 1 : 0)} | {(armB.CounterExampleUsable ? 1 : 0)} | ось 1: фактор доходит, плечи различны |
| Верных ответов на учебных строках, из 8 | {trainColdCorrect} | {trainAfterCorrect} | — | — | обучение состоялось |
| Верных ответов экзамена, из 8 | {coldCorrect} | {correctAfterSetup} | {armA.PanelCorrectAfter} | {armB.PanelCorrectAfter} | ось 2: плечи равны |
| Обученное состояние изменено уроком | — | — | {(armA.StateChangedByCorrectiveLesson ? "да" : "нет")} | {(armB.StateChangedByCorrectiveLesson ? "да" : "нет")} | урок усвоен в обоих |
| Состояния двух плеч совпадают побайтово | — | — | — | — | {(armA.StateAfterCorrectiveHash == armB.StateAfterCorrectiveHash ? "да — следа фактора нет" : "нет")} |

Выбранная для исправления строка — первая ошибочная по закреплённому порядку: запрос
«{wrongRow.Source}», эталон «{wrongRow.Gold}», фактический ответ машины «{wrongRow.Answer}».

**Ось первая, невырожденная — содержание урока.** В плече A сборщик положил в сторону
«отвергнуто» текст «{armA.LessonRejectedText}», то есть **тот же самый правильный ответ**:
урок предъявляет правильный ответ как пример неправильного, и контрпримера в нём нет вовсе.
В плече B там стоит «{armB.LessonRejectedText}» — настоящий ошибочный ответ машины. Пригодных
контрпримеров ноль против одного. Гипотеза карточки на этой оси подтверждена.

Почему плечо A вырождается, видно в коде: строка корпуса начинается со скобки, поэтому явный
отвергнутый ответ из неё не извлекается; дальше тип отказа «{armA.LessonRejectedType}»
сверяется со списком признаков пропуска в `IsOmissionLikeRejectedType`, где есть `omission`,
`missed`, `lost`, `drop`, `marker`, но **слова `missing` нет** — «missing» не равно «missed».
Не найдя признака, сборщик возвращает правильный перевод.

**Ось вторая, вырожденная по устройству — ответ обычного потребителя.** Корректирующий урок
состояние МЕНЯЕТ в обоих плечах: правильный ответ запоминается, и экзамен поднимается с
{correctAfterSetup} до {armA.PanelCorrectAfter} из 8. Но изменение у двух плеч **побайтово
одинаково**, и ответы совпадают построчно. Фактор опыта не оставляет в обученном состоянии
никакого следа.

| Строка экзамена | Эталон | Ответ плеча A | Ответ плеча B | Сверка |
|---|---|---|---|---|
{panel}

## Контроли

**Контроль постоянного ответа, и здесь были перепутаны две разные величины.** В прежней
записи на месте предела постоянного ответа стоял наблюдённый холодный счёт — одно число под
двумя именами. Это разные вещи. *Наблюдённый холодный счёт* — сколько необученная модель
ответила верно на самом деле: **{coldCorrect} из {ceiling.PanelRows}**. *Предел постоянного
ответа* — сколько набрал бы наилучший единственный постоянный ответ, который вход не читает
вовсе: **{ceiling.Correct} из {ceiling.PanelRows}**, и этот ответ назван прямо —
«{ceiling.BestConstantAnswer}». Предел посчитан аналитически, без единого вызова модели, по
{ceiling.CandidatesConsidered} кандидатам (все различные ожидаемые ответы панели плюс пустая
строка как стратегия молчания); тот же лучший счёт дают {ceiling.CandidatesTiedAtBest} из них,
потому что все ожидаемые ответы панели различны и любой из них совпадает ровно с одной строкой.

**Что из этого следует для панели экзамена, честно.** Плечи дают на панели
{armA.PanelCorrectAfter} и {armB.PanelCorrectAfter} из {ceiling.PanelRows}, а предел
постоянного ответа равен {ceiling.Correct}. {(beatsCeiling
    ? "Счёт плеч выше предела, значит на панели экзамена есть что доказывать."
    : "Счёт плеч НЕ выше предела, значит на панели экзамена плечи неотличимы от постоянного ответа, и панель экзамена доказательством состоявшегося обучения не является.")}
При прежней записи предела ({coldCorrect} вместо {ceiling.Correct}) выходило, будто плечи
обыграли постоянный ответ; это было следствием подмены величины, а не измерением.

**Чем обучение доказано на самом деле.** Контролем обучения на САМИХ учебных строках:
{trainColdCorrect} из 8 до обучения и {trainAfterCorrect} из 8 после. Этот контроль от панели
экзамена независим, поэтому вывод опыта по первой оси стоит на нём и не рушится.

**Контроль измерителя, и он сработал.** Третье плечо подаёт сторону «отвергнуто» с явной
фразовой пометкой, которая по устройству продукта попадает в сторону «выбрано», а её единица
читает. Его обученное состояние отличается от состояний плеч A и B. Значит измеритель видит
настоящую разницу состояния, когда она есть, и равенство состояний в плечах A и B — свойство
продукта, а не слепота опыта. Без этого контроля вывод второй оси был бы недействителен.

**Отрицательный контроль измерителя отдельным испытанием.** Одинаковый источник стороны
«отвергнуто» всегда даёт одинаковый урок и одинаковое состояние; разный источник даёт разное
содержание урока. Пустой фактический ответ получает внятный отказ — выдумывать его запрещено.
Партия без урока исполнителя тоже получает отказ, а не нулевой показатель.

## Граница вывода

Опыт доказывает: выбор источника стороны «отвергнуто» полностью определяет, есть ли в
корректирующем уроке контрпример вообще, и заготовленная строка корпуса оставляет урок без
контрпримера. Опыт НЕ доказывает улучшения перевода: на оси ответа фактор невидим, поэтому
никакого выигрыша качества здесь нет и быть не может, пока устройство не изменится.

## Находки, переданные владельцу; продуктовый код опытом не менялся

**Первая, узкая — логическая ошибка в списке признаков.**
`27.TranslationDomain/TranslationStructuredBatchBuilder.cs`, метод
`IsOmissionLikeRejectedType`, строки 466–482. Список содержит `omission`, `missed`, `lost`,
`drop`, `marker`, но не содержит `missing`, хотя стандартная строка собственного корпуса —
`[WRONG: missing translation]`. Из-за этого продукт не распознаёт свою же пометку и строит
вырожденный урок. Измеренное основание для переноса — числа этой попытки.

**Вторая, структурная — решение об устройстве, не правка.**
`27.TranslationDomain/TranslationTrainedUnits.cs`, метод `ExecutorUnit.TrainOnExample`,
строки 570–631. Он разбирает только сторону «выбрано»; сторона «отвергнуто» не читается
вовсе. Проверено чтением кода до прогона и подтверждено измерением: состояния плеч побайтово
равны. Единица, которая учится на предпочтениях, но никогда не смотрит на отвергнутую
сторону, по предпочтениям учиться не может в принципе. Это записано как находка; решение
выносится отдельно.

## Как это запускается

Прямой запуск программой в этой среде запрещён, поэтому опыт исполняется тестовым проектом.

```
dotnet build Tests\LlmFromScratch.Tests.csproj -c Debug
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -c Debug --list-tests
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~P5801CorrectiveLessonCounterExampleArmsTests"
```

Сборка — 0 ошибок; обнаружение — опыт виден. Числа прогона смотрите в файле результатов
прогона: писать их здесь рукой нельзя, иначе отчёт снова начнёт утверждать то, чего не
измерял. Код возврата самого опыта — {receipt.ExitCode}, и он выведен валидатором.

## Отрицательные контроли перенесённых образцов

Рядом с опытом стоят отдельные испытания, которые краснеют на испорченной логике: предел
постоянного ответа обязан равняться {ceiling.Correct}, а не нулю и не наблюдённому холодному
счёту; расчёт предела обязан пользоваться тем же правилом верности, что и панель; расписка
обязана сообщить незавершённость при не состоявшемся обучении, пустой панели, счёте вне
границ панели, нарушенном равном старте и не сработавшем контроле измерителя. Зелёный прогон
без этих контролей доказательством не считается.
""";
    }

    // Отрицательный контроль измерителя отдельным испытанием: одинаковый источник стороны
    // «отвергнуто» обязан дать одинаковое содержание урока и одинаковое состояние, а разный —
    // разное содержание. Без этого зелёный основной прогон не отличался бы от случайности.
    [Fact]
    public void TheSameRejectedSourceAlwaysGivesTheSameLessonAndOnlyTheFactorChangesIt()
    {
        // Журнал контроля нужен самому контролю и в доказательства опыта не пишется.
        var log = new List<string>();

        // Строка панели и её фактический ошибочный ответ берутся из установочного прогона.
        var probe = NewUnits();
        TrainBatch(probe, BuildSetupBatch("P5801-control-probe"));
        var wrong = RunPanel(probe).First(row => !row.Correct);

        // Два независимых прогона плеча A на свежих связках.
        var firstA = RunArm("control-A-1", CorrectiveRejectedSource.PreparedCorpusString,
            wrong.Source, wrong.Gold, wrong.Answer, log);
        var secondA = RunArm("control-A-2", CorrectiveRejectedSource.PreparedCorpusString,
            wrong.Source, wrong.Gold, wrong.Answer, log);

        // Одинаковый источник обязан дать в точности одинаковый урок и одинаковое состояние.
        Assert.Equal(firstA.LessonRejectedText, secondA.LessonRejectedText);
        Assert.Equal(firstA.StateAfterCorrectiveHash, secondA.StateAfterCorrectiveHash);
        Assert.Equal(firstA.CounterExampleUsable, secondA.CounterExampleUsable);

        // Два независимых прогона плеча B.
        var firstB = RunArm("control-B-1", CorrectiveRejectedSource.ActualWrongOutput,
            wrong.Source, wrong.Gold, wrong.Answer, log);
        var secondB = RunArm("control-B-2", CorrectiveRejectedSource.ActualWrongOutput,
            wrong.Source, wrong.Gold, wrong.Answer, log);

        // И здесь одинаковый источник обязан дать одинаковый урок.
        Assert.Equal(firstB.LessonRejectedText, secondB.LessonRejectedText);
        Assert.Equal(firstB.StateAfterCorrectiveHash, secondB.StateAfterCorrectiveHash);

        // А разный источник обязан дать РАЗНОЕ содержание урока: именно это делает замер
        // чувствительным к фактору, а не к порядку запуска.
        Assert.NotEqual(firstA.LessonRejectedText, firstB.LessonRejectedText);
        Assert.NotEqual(firstA.CounterExampleUsable, firstB.CounterExampleUsable);

        // Выдумывать фактический ошибочный ответ запрещено: пустое значение — это отказ.
        var refused = CorrectiveLessonCounterExampleArms.BuildRejectedSide(
            CorrectiveRejectedSource.ActualWrongOutput, "   ");

        // Отказ обязателен, и его причина названа своим кодом.
        Assert.False(refused.IsSuccess);
        Assert.Equal("ACTUAL_WRONG_OUTPUT_REQUIRED", refused.ErrorCode);

        // Измерение на партии без урока исполнителя обязано отказать, а не вернуть ноль.
        var emptyBatch = new TranslationLearningBatch("empty", TranslationMachine.DomainTag, [], []);
        var emptyMeasure = CorrectiveLessonCounterExampleArms.MeasureCounterExample(emptyBatch, Direction);

        // Отказ измерения назван своим кодом, а не выдан за нулевой показатель.
        Assert.False(emptyMeasure.IsSuccess);
        Assert.Equal("EMPTY_BATCH", emptyMeasure.ErrorCode);
    }

    // ==================================================================================
    // ОТРИЦАТЕЛЬНЫЕ КОНТРОЛИ ДВУХ ПЕРЕНЕСЁННЫХ ОБРАЗЦОВ: расчёта предела постоянного
    // ответа и валидатора расписки. Без них зелёный прогон не отличался бы от прогона с
    // прежними дефектами: подменённой величиной и литеральной расписочной завершённостью.
    // ==================================================================================

    // PreparedArm собирает плечо с ЗАДАННЫМИ показаниями. Он нужен затем, что испорченное
    // обученное состояние или счёт вне границ панели нельзя получить настоящим прогоном:
    // валидатор обязан отказать и на таких показаниях, а добыть их можно только подготовкой.
    private static ArmResult PreparedArm(
        string armName,
        string stateAfterSetup,
        string stateAfterCorrective,
        bool counterExampleUsable,
        int panelCorrectAfter)
        => new(
            ArmName: armName,
            RejectedSource: "подготовлено для отрицательного контроля",
            RejectedSideText: "—",
            CounterExampleUsable: counterExampleUsable,
            LessonChosenText: "правильный ответ",
            LessonRejectedText: counterExampleUsable ? "ошибочный ответ" : "правильный ответ",
            LessonRejectedType: "—",
            StateAfterSetupHash: stateAfterSetup,
            StateAfterCorrectiveHash: stateAfterCorrective,
            StateChangedByCorrectiveLesson: stateAfterSetup != stateAfterCorrective,
            CorrectiveLessons: 3,
            PanelCorrectAfter: panelCorrectAfter,
            PanelAfter: []);

    // HealthyArms — три плеча со здоровыми показаниями: равный старт у всех трёх, у плеч A и B
    // одинаковое состояние после урока, у контрольного плеча — отличное. Это тот самый случай,
    // который опубликованная попытка и наблюдала.
    private static (ArmResult A, ArmResult B, ArmResult Control) HealthyArms()
    {
        // Одна и та же точка старта у всех трёх плеч.
        const string Setup = "СОСТОЯНИЕ-ПОСЛЕ-УСТАНОВКИ";

        // Плечи A и B после корректирующего урока приходят в одно и то же состояние.
        const string AfterArms = "СОСТОЯНИЕ-ПОСЛЕ-УРОКА-ПЛЕЧИ-A-И-B";

        // Контрольное плечо обязано прийти в ДРУГОЕ состояние: это и есть контроль измерителя.
        const string AfterControl = "СОСТОЯНИЕ-ПОСЛЕ-УРОКА-КОНТРОЛЬНОЕ-ПЛЕЧО";

        // Счёт панели берётся такой же, как в наблюдении: один верный ответ из восьми.
        return (
            PreparedArm("A", Setup, AfterArms, counterExampleUsable: false, panelCorrectAfter: 1),
            PreparedArm("B", Setup, AfterArms, counterExampleUsable: true, panelCorrectAfter: 1),
            PreparedArm("C", Setup, AfterControl, counterExampleUsable: true, panelCorrectAfter: 1));
    }

    // AssertReceiptRefused: расписка обязана сообщить незавершённость всеми своими полями
    // согласованно и назвать причину человеческими словами. Согласованность важна: поле,
    // отставшее от списка причин, вернуло бы ровно прежний дефект.
    private static void AssertReceiptRefused(ExperimentReceipt receipt, params string[] expectedFragments)
    {
        // Испорченный прогон не может быть признан действительным.
        Assert.False(receipt.Valid, "валидатор признал испорченный прогон действительным");

        // Состояние исполнения обязано сообщить незавершённость, а не «COMPLETED».
        Assert.Equal("INCOMPLETE_EXPERIMENT_INVARIANT_VIOLATED", receipt.ExecutionStatus);

        // Действительность обучения не может остаться действительной.
        Assert.Equal("INVALID_EXPERIMENT_INVARIANT_VIOLATED", receipt.LearningValidity);

        // Гипотеза при недействительном прогоне остаётся открытой, а не подтверждённой.
        Assert.Equal("OPEN_EXPERIMENT_INVARIANT_VIOLATED", receipt.HypothesisStatus);

        // Код возврата обязан стать ненулевым.
        Assert.NotEqual(0, receipt.ExitCode);

        // Список проблем обязан быть непустым: отказ без причины нечитаем.
        Assert.NotEmpty(receipt.Problems);

        // Причины сводятся в одну строку только для поиска названных человеческих слов.
        var joined = string.Join(" | ", receipt.Problems);

        // Каждая ожидаемая причина обязана присутствовать в тексте отказа.
        foreach (var fragment in expectedFragments)
            Assert.Contains(fragment, joined, StringComparison.Ordinal);
    }

    // ПЕРЕНЕСЁННЫЙ РАСЧЁТ, КОНТРОЛЬ ОДИН. На панели из восьми РАЗЛИЧНЫХ ожидаемых ответов
    // предел постоянного ответа равен ровно единице: любой единственный постоянный ответ
    // совпадает ровно с одной строкой, а пустая строка — ни с одной.
    [Fact]
    public void ConstantAnswerCeilingIsOneBecauseEveryExpectedPanelAnswerIsDistinct()
    {
        // Панель и её ожидаемые ответы берутся из замороженного набора опыта.
        var panel = ExamRows();

        // Основание ожидания называется прямо: ожидаемые ответы различны все восемь.
        Assert.Equal(panel.Length, panel.Select(row => row.Gold).Distinct(StringComparer.Ordinal).Count());

        // Расчёт предела перенесённым образцом.
        var ceiling = ComputeConstantAnswerCeiling();

        // Предел равен единице, а знаменатель — размеру панели.
        Assert.Equal(1, ceiling.Correct);
        Assert.Equal(panel.Length, ceiling.PanelRows);

        // Лучший постоянный ответ назван, и он действительно один из ожидаемых ответов панели.
        Assert.Contains(ceiling.BestConstantAnswer, panel.Select(row => row.Gold));

        // Кандидатов ровно столько: различные ожидаемые ответы плюс пустая строка.
        Assert.Equal(panel.Length + 1, ceiling.CandidatesConsidered);

        // Тот же лучший счёт дают все ожидаемые ответы, но не пустая строка.
        Assert.Equal(panel.Length, ceiling.CandidatesTiedAtBest);
    }

    // ПЕРЕНЕСЁННЫЙ РАСЧЁТ, КОНТРОЛЬ ДВА. Это прямой контроль прежнего дефекта: предел не ноль
    // и не равен наблюдённому холодному счёту. Если кто-нибудь снова подставит в поле предела
    // холодный счёт, этот контроль покраснеет.
    [Fact]
    public void ConstantAnswerCeilingIsNotZeroAndIsNotTheObservedColdPanelCount()
    {
        // Расчётная величина.
        var ceiling = ComputeConstantAnswerCeiling();

        // Наблюдённая величина снимается настоящим прогоном необученных единиц.
        var coldCorrect = RunPanel(NewUnits()).Count(row => row.Correct);

        // Предел не ноль: ноль означал бы, что постоянным ответом не угадать ничего.
        Assert.NotEqual(0, ceiling.Correct);

        // И предел не равен наблюдённому холодному счёту: это две разные величины.
        Assert.NotEqual(coldCorrect, ceiling.Correct);

        // Названо и численно: холодный счёт ноль, предел единица.
        Assert.Equal(0, coldCorrect);
        Assert.Equal(1, ceiling.Correct);
    }

    // ПЕРЕНЕСЁННЫЙ РАСЧЁТ, КОНТРОЛЬ ТРИ. Предел — расчёт, а не наблюдение: он не зависит ни от
    // обученности единиц, ни от повторного запуска, и не делает ни одного вызова модели.
    [Fact]
    public void ConstantAnswerCeilingIsAnalyticalAndStableAcrossRepeatedComputation()
    {
        // Два независимых расчёта обязаны совпасть во всех полях.
        var first = ComputeConstantAnswerCeiling();
        var second = ComputeConstantAnswerCeiling();

        // Записи сравниваются целиком: расхождение любого поля сделало бы предел ненадёжным.
        Assert.Equal(first, second);

        // Обучение связки на предел не влияет вовсе: он считается по ожидаемым ответам панели.
        var trained = NewUnits();
        TrainBatch(trained, BuildSetupBatch("P5801-ceiling-independence"));
        Assert.Equal(first, ComputeConstantAnswerCeiling());
    }

    // ПЕРЕНЕСЁННЫЙ РАСЧЁТ, КОНТРОЛЬ ЧЕТЫРЕ. Расчёт предела и оценка панели обязаны судить
    // одним и тем же правилом. Проверяется по наблюдаемому следствию: правило не различает
    // регистр, поэтому лучший постоянный ответ в ином регистре даёт тот же самый счёт.
    [Fact]
    public void ConstantAnswerCeilingUsesTheSameCorrectnessRuleAsThePanelScoring()
    {
        // Панель и предел.
        var panel = ExamRows();
        var ceiling = ComputeConstantAnswerCeiling();

        // Правило опыта не различает регистр — это проверяется прямо на самом правиле.
        Assert.True(AnswerIsCorrect(panel[0].Gold.ToUpperInvariant(), panel[0].Gold));

        // Значит тот же постоянный ответ в ином регистре угадывает столько же строк панели,
        // сколько насчитал предел: расчёт и оценка панели пользуются одной меркой.
        var upperCased = panel.Count(row => AnswerIsCorrect(ceiling.BestConstantAnswer.ToUpperInvariant(), row.Gold));
        Assert.Equal(ceiling.Correct, upperCased);

        // А заведомо отсутствующий в панели ответ не угадывает ничего.
        Assert.Equal(0, panel.Count(row => AnswerIsCorrect("этого ответа в панели нет", row.Gold)));
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ. На здоровых показаниях расписка даёт те
    // же значения, что и опубликованная попытка: завершено, обучение действительно, гипотеза
    // подтверждена по названному показателю, код возврата ноль, проблем нет.
    [Fact]
    public void ReceiptReportsCompletedOnHealthyMeasurements()
    {
        // Здоровые плечи и настоящий расчёт предела.
        var (armA, armB, armControl) = HealthyArms();

        // Расписка выводится валидатором.
        var receipt = DecideReceipt(
            trainColdCorrect: 0, trainAfterCorrect: 8, coldCorrect: 0,
            ComputeConstantAnswerCeiling(), armA, armB, armControl);

        // Все поля обязаны совпасть с наблюдением опубликованной попытки.
        Assert.True(receipt.Valid);
        Assert.Equal("COMPLETED", receipt.ExecutionStatus);
        Assert.Equal("VALID_FOR_NAMED_UNIT", receipt.LearningValidity);
        Assert.Equal("SUPPORTED_FOR_NAMED_METRIC", receipt.HypothesisStatus);
        Assert.Equal(0, receipt.ExitCode);
        Assert.Empty(receipt.Problems);
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ ОДИН. Не состоявшееся обучение обязано сделать прогон
    // недействительным, и считается оно по контролю обучения, а не по панели экзамена.
    [Fact]
    public void ReceiptReportsIncompleteWhenTrainingDidNotHappen()
    {
        // Здоровые плечи: портится только контроль обучения.
        var (armA, armB, armControl) = HealthyArms();

        // После обучения верных не больше, чем до: обучения не было.
        var receipt = DecideReceipt(
            trainColdCorrect: 8, trainAfterCorrect: 8, coldCorrect: 0,
            ComputeConstantAnswerCeiling(), armA, armB, armControl);

        // Расписка обязана отказать и назвать причину.
        AssertReceiptRefused(receipt, "обучение не состоялось", "до обучения верных 8", "после обучения 8");
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ ДВА. Пустая панель означает, что мерить было нечего.
    [Fact]
    public void ReceiptReportsIncompleteWhenTheExamPanelIsEmpty()
    {
        // Здоровые плечи, но знаменатель панели нулевой.
        var (armA, armB, armControl) = HealthyArms();

        // Предел с пустой панелью: счёт ноль, строк ноль.
        var emptyPanelCeiling = new ConstantAnswerCeilingResult(
            Correct: 0, PanelRows: 0, BestConstantAnswer: string.Empty,
            CandidatesConsidered: 1, CandidatesTiedAtBest: 1);

        // Расписка обязана отказать: нулевая панель не бывает измерением.
        var receipt = DecideReceipt(
            trainColdCorrect: 0, trainAfterCorrect: 8, coldCorrect: 0,
            emptyPanelCeiling, armA, armB, armControl);

        // Причина названа человеческими словами.
        AssertReceiptRefused(receipt, "панель экзамена пуста");
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ ТРИ. Расчёт предела, не давший кандидата, обязан
    // остановить прогон, а не записаться нулём.
    [Fact]
    public void ReceiptReportsIncompleteWhenTheCeilingComputationGaveNoCandidate()
    {
        // Здоровые плечи, испорчен только расчёт предела.
        var (armA, armB, armControl) = HealthyArms();

        // Ни одного кандидата и отрицательный лучший счёт — именно так выглядит несчитанный предел.
        var noCandidateCeiling = new ConstantAnswerCeilingResult(
            Correct: -1, PanelRows: 8, BestConstantAnswer: string.Empty,
            CandidatesConsidered: 0, CandidatesTiedAtBest: 0);

        // Расписка обязана отказать.
        var receipt = DecideReceipt(
            trainColdCorrect: 0, trainAfterCorrect: 8, coldCorrect: 0,
            noCandidateCeiling, armA, armB, armControl);

        // Названы и отсутствие кандидата, и выход счёта за границы панели.
        AssertReceiptRefused(receipt,
            "расчёт предела постоянного ответа не дал кандидата",
            "«предел постоянного ответа»: верных -1 вне диапазона");
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ ЧЕТЫРЕ. Счёт верных вне границ панели — это порча
    // счёта или знаменателя, а не наблюдение.
    [Fact]
    public void ReceiptReportsIncompleteWhenACorrectCountIsOutsideThePanelRange()
    {
        // Плечо B получает счёт больше размера панели.
        var (armA, _, armControl) = HealthyArms();
        var brokenB = PreparedArm("B", armA.StateAfterSetupHash, armA.StateAfterCorrectiveHash,
            counterExampleUsable: true, panelCorrectAfter: 9);

        // Расписка обязана отказать и назвать плечо.
        var receipt = DecideReceipt(
            trainColdCorrect: 0, trainAfterCorrect: 8, coldCorrect: 0,
            ComputeConstantAnswerCeiling(), armA, brokenB, armControl);

        // Причина называет и плечо, и число, и границы.
        AssertReceiptRefused(receipt, "«плечо B»: верных 9 вне диапазона от 0 до 8");
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ ПЯТЬ. Нарушенный равный старт делает сравнение плеч
    // недействительным: плечи начинали с разного.
    [Fact]
    public void ReceiptReportsIncompleteWhenTheEqualStartIsBroken()
    {
        // Плечо B стартует из другого состояния.
        var (armA, _, armControl) = HealthyArms();
        var driftedB = PreparedArm("B", "ДРУГОЙ-СТАРТ", armA.StateAfterCorrectiveHash,
            counterExampleUsable: true, panelCorrectAfter: 1);

        // Расписка обязана отказать.
        var receipt = DecideReceipt(
            trainColdCorrect: 0, trainAfterCorrect: 8, coldCorrect: 0,
            ComputeConstantAnswerCeiling(), armA, driftedB, armControl);

        // Причина названа прямо.
        AssertReceiptRefused(receipt, "равный старт нарушен", "плеч A и B");
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ ШЕСТЬ. Не сработавший контроль измерителя обязан
    // остановить прогон: без него равенство состояний плеч A и B неотличимо от слепоты опыта.
    [Fact]
    public void ReceiptReportsIncompleteWhenTheInstrumentControlDidNotFire()
    {
        // Контрольное плечо приходит в то же состояние, что и плечо A: измеритель слеп.
        var (armA, armB, _) = HealthyArms();
        var blindControl = PreparedArm("C", armA.StateAfterSetupHash, armA.StateAfterCorrectiveHash,
            counterExampleUsable: true, panelCorrectAfter: 1);

        // Расписка обязана отказать.
        var receipt = DecideReceipt(
            trainColdCorrect: 0, trainAfterCorrect: 8, coldCorrect: 0,
            ComputeConstantAnswerCeiling(), armA, armB, blindControl);

        // Причина названа человеческими словами.
        AssertReceiptRefused(receipt,
            "контроль измерителя не сработал",
            "состояние контрольного плеча совпало с состоянием плеча A");
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ СЕМЬ. Состояние гипотезы выводится из ПЕРВОЙ ОСИ, а не
    // назначается литералом: при перевёрнутом результате первой оси расписка обязана сказать
    // «не подтверждена», оставаясь при этом действительным прогоном.
    [Fact]
    public void ReceiptHypothesisStatusIsDerivedFromTheFirstAxisAndNotFixed()
    {
        // Перевёрнутая первая ось: контрпример есть у плеча A и отсутствует у плеча B.
        var (healthyA, healthyB, armControl) = HealthyArms();
        var flippedA = PreparedArm("A", healthyA.StateAfterSetupHash, healthyA.StateAfterCorrectiveHash,
            counterExampleUsable: true, panelCorrectAfter: 1);
        var flippedB = PreparedArm("B", healthyB.StateAfterSetupHash, healthyB.StateAfterCorrectiveHash,
            counterExampleUsable: false, panelCorrectAfter: 1);

        // Расписка выводится тем же валидатором.
        var receipt = DecideReceipt(
            trainColdCorrect: 0, trainAfterCorrect: 8, coldCorrect: 0,
            ComputeConstantAnswerCeiling(), flippedA, flippedB, armControl);

        // Прогон действителен: неподтверждение гипотезы — законный исход опыта, а не поломка.
        Assert.True(receipt.Valid);
        Assert.Equal("COMPLETED", receipt.ExecutionStatus);
        Assert.Equal(0, receipt.ExitCode);
        Assert.Empty(receipt.Problems);

        // Но состояние гипотезы обязано измениться вслед за первой осью.
        Assert.Equal("NOT_SUPPORTED_FOR_NAMED_METRIC", receipt.HypothesisStatus);
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ ВОСЕМЬ — САМЫЙ ВАЖНЫЙ ИЗ НИХ. Сам ДОКУМЕНТ расписки
    // обязан сообщить незавершённость, когда валидатор нашёл проблему.
    //
    // Почему без этого контроля дефект нельзя было поймать. На здоровых данных литерал
    // «COMPLETED» и вывод валидатора дают одно и то же, поэтому здоровый зелёный прогон
    // одинаков и с валидатором, и с литералами. Разойтись они могут только на испорченной
    // расписке — и именно этот случай проверяется здесь. Если кто-нибудь вернёт три литерала
    // в сборку документа, не тронув валидатор, покраснеет ровно эта проверка.
    [Fact]
    public void StatusDocumentReportsIncompleteWhenTheValidatorFoundAProblem()
    {
        // Здоровые плечи и настоящий расчёт предела: портится только контроль обучения.
        var (armA, armB, armControl) = HealthyArms();
        var ceiling = ComputeConstantAnswerCeiling();

        // Недействительная расписка: после обучения верных не больше, чем до.
        var broken = DecideReceipt(
            trainColdCorrect: 8, trainAfterCorrect: 8, coldCorrect: 0,
            ceiling, armA, armB, armControl);

        // Валидатор обязан был отказать — иначе проверяется не то, что задумано.
        Assert.False(broken.Valid);

        // Документ собирается ТЕМ ЖЕ способом, которым его пишет прогон опыта.
        var json = JsonSerializer.Serialize(
            ComposeStatusPayload(broken, ceiling, coldCorrect: 0, armA, armB, armControl),
            ReadableJson());

        // Документ читается обратно: проверяется то, что действительно попало бы в файл.
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Состояние исполнения обязано сообщить незавершённость.
        Assert.Equal("INCOMPLETE_EXPERIMENT_INVARIANT_VIOLATED",
            root.GetProperty("execution_status").GetString());

        // Действительность обучения и состояние гипотезы обязаны последовать за отказом.
        Assert.Equal("INVALID_EXPERIMENT_INVARIANT_VIOLATED",
            root.GetProperty("learning_validity").GetString());
        Assert.Equal("OPEN_EXPERIMENT_INVARIANT_VIOLATED",
            root.GetProperty("hypothesis_status").GetString());

        // Код возврата обязан стать ненулевым.
        Assert.NotEqual(0, root.GetProperty("exit_code").GetInt32());

        // И список проблем обязан назвать причину человеческими словами.
        Assert.NotEqual(0, root.GetProperty("problems").GetArrayLength());
        Assert.Contains("обучение не состоялось", root.GetProperty("problems")[0].GetString());
    }

    // ПЕРЕНЕСЁННЫЙ ВАЛИДАТОР, КОНТРОЛЬ ДЕВЯТЬ — положительный контроль того же документа.
    // На здоровых показаниях документ обязан нести ровно те же значения, что опубликованная
    // попытка, и обе величины — наблюдённый холодный счёт и предел постоянного ответа —
    // обязаны стоять в нём раздельно и не совпадать.
    [Fact]
    public void StatusDocumentCarriesCompletedAndBothQuantitiesSeparatelyOnHealthyMeasurements()
    {
        // Здоровые плечи, настоящий расчёт предела, наблюдённый холодный счёт ноль.
        var (armA, armB, armControl) = HealthyArms();
        var ceiling = ComputeConstantAnswerCeiling();
        var receipt = DecideReceipt(
            trainColdCorrect: 0, trainAfterCorrect: 8, coldCorrect: 0,
            ceiling, armA, armB, armControl);

        // Документ собирается тем же способом, что и в прогоне опыта.
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(
            ComposeStatusPayload(receipt, ceiling, coldCorrect: 0, armA, armB, armControl),
            ReadableJson()));
        var root = document.RootElement;

        // Те же значения, что в опубликованной попытке: завершено, код ноль, проблем нет.
        Assert.Equal("COMPLETED", root.GetProperty("execution_status").GetString());
        Assert.Equal("VALID_FOR_NAMED_UNIT", root.GetProperty("learning_validity").GetString());
        Assert.Equal("SUPPORTED_FOR_NAMED_METRIC", root.GetProperty("hypothesis_status").GetString());
        Assert.Equal(0, root.GetProperty("exit_code").GetInt32());
        Assert.Equal(0, root.GetProperty("problems").GetArrayLength());

        // ДВЕ ВЕЛИЧИНЫ СТОЯТ РАЗДЕЛЬНО, и это главное требование починки.
        var ceilingInDocument = root.GetProperty("constant_answer_ceiling").GetInt32();
        var coldInDocument = root.GetProperty("observed_cold_panel_correct").GetInt32();

        // Предел равен единице, холодный счёт — нулю, и они НЕ равны друг другу.
        Assert.Equal(1, ceilingInDocument);
        Assert.Equal(0, coldInDocument);
        Assert.NotEqual(coldInDocument, ceilingInDocument);

        // Знаменатель предела записан независимо и равен размеру панели.
        Assert.Equal(ExamRows().Length, root.GetProperty("constant_answer_ceiling_denominator").GetInt32());

        // И документ честно говорит, что счёт плеч предела не превысил.
        Assert.False(root.GetProperty("exam_panel_beats_constant_answer_ceiling").GetBoolean());
    }
}
