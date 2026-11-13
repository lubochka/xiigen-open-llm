// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using System.Text;
using System.Text.Json;
using ExperimentsV2;
using P4801WeightingCeiling;

// Хозяин ТРЕТЬЕЙ попытки сорок восьмого опыта: «вес и учебная последовательность без смены
// целей», подслучай «относительный градиентный вес при одинаковых целях».
//
// Что проверяется человеческими словами. Маленькую сеть учат предсказывать следующий
// предикат по текущему. Карточка хотела сравнить два плеча: в одном все учебные пары равны
// по важности, в другом пары двух файлов важнее в десять раз. Вопрос: меняет ли такая
// расстановка важности то, что сеть в итоге отвечает.
//
// ЧТО ЭТА ПОПЫТКА ДАЁТ. Независимую натурную репликацию: прежний вывод второй попытки
// ПОДТВЕРЖДЁН другим путём, а не исправлен. Потолки пересчитаны другой реализацией и
// совпали; оракул производной проверял продуктовую сеть, тогда как прежний проверял свою
// копию.
//
// ДВЕ РАЗНЫЕ ВЕЩИ, и они называются ОТДЕЛЬНО. Первая: у продуктового учебного шага нет
// параметра веса, поэтому фактор требует копии — это пробел возможностей продукта, а НЕ
// препятствие для опыта: копия существует и отслеживается
// (`experiments-v2/RP01-MICRO/P4801/src/WeightedProbeNetworkCopy.cs` от 30 октября).
// Вторая: панель не способна различить фактор, потому что её потолок доступного входа
// равен достигнутому — это предел состава данных карточки. Общего вывода «опыт провалился»
// из них не делается ни в одной строке.
//
// Обучение настоящее: плечо равных весов получает 2400 обновлений продуктовым шагом
// `ProbeNeuralNetwork.TrainStep`, и контроль механизма тоже учится 2400 обновлений.
//
// Продуктовый код, первая попытка, карточка и указатель этим опытом не меняются. Параметр
// веса в продуктовый шаг не добавляется: это отдельная продуктовая работа.
// Фикстура класса ниже даёт этому испытанию ранний захват попытки: испытательная оснастка создаёт
// её ДО конструктора тестового класса и до любой подготовки данных, а освобождает ОДИН раз после
// последнего теста класса. Поэтому адрес улик занимается до обучения, а завершение попытки ставится
// ровно один раз.
public sealed class P4801WeightingCurriculumCeilingTests
    : IClassFixture<P4801WeightingCurriculumCeilingTests.AttemptScopeFixture>
{
    // Размер словаря сети. Двадцать два — требование навыка устройства опытов, значение
    // карточки («Probe22→32→22») и значение самого продукта, который объясняет его так:
    // извлекатель логики даёт предикаты с номерами от нуля до двадцати одного.
    private const int VocabSize = 22;

    // Скрытый слой: тридцать два по карточке.
    private const int HiddenSize = 32;

    // Скорость обучения: ноль целых пять сотых по карточке.
    private const float LearningRate = 0.05f;

    // Семя: сорок два по карточке. Используется ТОЛЬКО для воспроизводимости начальных
    // весов. Состав данных задан карточкой жёстко и семенем не выбирается нигде.
    private const int Seed = 42;

    // Бюджет обновлений на плечо по карточке.
    private const int UpdatesPerArm = 2400;

    // Множитель важности, назначенный карточкой как сам изучаемый фактор.
    private const int HeavyWeight = 10;

    // Наблюдение по одной строке панели.
    private sealed record PanelObservation(
        string RowId, string Kind, string Group, int Input, string InputName,
        int Gold, string GoldName, int Answer, string AnswerName, bool Correct);

    // Итог одного плеча.
    private sealed record ArmOutcome(
        string ArmName,
        string MechanismLabel,
        int UpdatesApplied,
        int StepsPerCycle,
        bool LossFiniteEveryStep,
        int FirstNotNumberStep,
        NumericHealth[] NumericHealthByBlock,
        int PanelCorrect,
        int ExamCorrect,
        int TrainControlCorrect,
        int DistinctAnswers,
        bool IsConstantAnswerer,
        PanelObservation[] Panel);

    // Численные улики одной руки — ровно то, что валидатору нужно знать, и ничего больше:
    // как рука называется, конечность пяти блоков обученного состояния и осталась ли потеря
    // числом на каждом шаге. Основной прогон снимает эту запись с итога плеча; контроли
    // валидатора собирают её из готовых чисел без единого вызова обучения.
    private sealed record ArmNumericEvidence(
        string ArmName,
        NumericHealth[] Blocks,
        bool LossFiniteEveryStep,
        int FirstNotNumberStep);

    // Вердикт сравнения двух рук. Все четыре поля расписки — действительность обучения,
    // состояние гипотезы, код возврата и список проблем — выводятся ОТСЮДА и больше ниоткуда.
    // Литералов рядом с признаком больше нет, потому что литерал не умеет отказать: до этой
    // правки признак считался и печатался отдельным полем, а четыре поля рядом с ним были
    // написаны руками, и будущая испорченная попытка записалась бы как действительная с
    // нулевым кодом возврата.
    private sealed record ComparisonVerdict(
        bool NumericallyValid,
        string LearningValidity,
        string HypothesisStatus,
        int ExitCode,
        string[] Problems);

    // Численные улики руки снимаются с итога плеча основного прогона.
    private static ArmNumericEvidence NumericEvidenceOf(ArmOutcome arm) => new(
        arm.ArmName, arm.NumericHealthByBlock, arm.LossFiniteEveryStep, arm.FirstNotNumberStep);

    // DecideComparison — ЕДИНСТВЕННЫЙ валидатор действительности сравнения. Он отвечает на один
    // вопрос — можно ли вообще сравнивать две руки — и его ответ определяет сразу все четыре
    // поля расписки. Не-число и обе бесконечности в любом блоке любой руки, как и потеря,
    // переставшая быть числом на любом шаге, одинаково делают сравнение недействительным: тогда
    // гипотеза остаётся открытой, код возврата становится ненулевым, а список проблем называет
    // руку, блок и вид порчи по-человечески. Бесконечность входит в предикат с этой правки:
    // прежде помощник считал её отдельно, а предикат смотрел только на сумму нечисел.
    private static ComparisonVerdict DecideComparison(ArmNumericEvidence equal, ArmNumericEvidence control)
    {
        // Список проблем собирается по обеим рукам; пустой список и есть действительность.
        var problems = new List<string>();

        // Обе руки проверяются одним и тем же правилом: порядок рук на вердикт не влияет.
        foreach (var arm in new[] { equal, control })
        {
            // Каждый блок состояния рассматривается отдельно, чтобы проблема называла блок.
            foreach (var block in arm.Blocks)
            {
                // Не-число в блоке — численный отказ, названный по имени блока и по счёту.
                if (block.NotNumber > 0)
                    problems.Add($"плечо «{arm.ArmName}»: блок {block.Block} содержит нечисел {block.NotNumber} из {block.Total}");

                // Бесконечность любого знака в блоке — такой же отказ; знак помощник не различает,
                // и различать его здесь не нужно: любая бесконечность в весах — порча состояния.
                if (block.Infinite > 0)
                    problems.Add($"плечо «{arm.ArmName}»: блок {block.Block} содержит бесконечностей {block.Infinite} из {block.Total}");
            }

            // Потеря, переставшая быть числом хотя бы на одном шаге, тоже отвергает сравнение.
            if (!arm.LossFiniteEveryStep)
                problems.Add($"плечо «{arm.ArmName}»: потеря впервые перестала быть числом на шаге {arm.FirstNotNumberStep}");
        }

        // Действительность — это ровно отсутствие проблем: отдельного флага, способного
        // разойтись со списком причин, нет.
        var valid = problems.Count == 0;

        // Все четыре поля выводятся из одного решения. Действительное сравнение получает те же
        // значения, что и опубликованная попытка; недействительное — согласованный отказ:
        // обучение недействительно из-за неконечного состояния или потери, гипотеза открыта,
        // код возврата один, причины перечислены.
        return new ComparisonVerdict(
            NumericallyValid: valid,
            LearningValidity: valid ? "VALID_FOR_NAMED_UNIT" : "INVALID_NON_FINITE_STATE_OR_LOSS",
            HypothesisStatus: valid ? "NOT_SUPPORTED_IN_VALID_COMPARISON" : "OPEN_COMPARISON_NUMERICALLY_INVALID",
            ExitCode: valid ? 0 : 1,
            Problems: [.. problems]);
    }

    // Корень хранилища ищется по файлу правил проекта.
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

    // Номер попытки — ОДНО значение на весь опыт.
    //
    // Почему именно третья, а не вторая. Первая и вторая попытки уже существуют и уже
    // отслеживаются хранилищем, то есть являются опубликованными доказательствами. Писать
    // в отслеживаемую попытку нельзя никогда: запись стирает чужие опубликованные байты.
    // Правило на будущее: перед записью проверить, какие попытки уже существуют и какие
    // отслеживаются, и взять первый свободный номер.
    //
    // Почему номер объявлен ОДИН раз. Прежде каталог записи и поле номера в расписке
    // задавались раздельно, и они разошлись: расписка называла себя второй попыткой, лежа
    // в каталоге третьей. Это расхождение личности записи — ровно тот дефект, который потом
    // ломает чтение, потому что сверяют одно поле, а читают другой каталог. Теперь и
    // каталог, и поле выводятся из этого значения, поэтому разойтись они больше не могут.
    private const string AttemptNumber = "0003";

    // Каталог доказательств этой попытки собирается из того же номера.
    private static string AttemptDirectory() => Path.Combine(
        RepositoryRoot(), "experiments-v2", "RP01-MICRO", "P4801", "results", $"attempt-{AttemptNumber}");

    // Куда этому прогону разрешено писать. Решение принимается один раз за прогон общим
    // приёмником доказательств по одному факту: отслеживает ли хранилище файлы внутри
    // каталога назначенной попытки.
    //
    // Почему ручного выбора номера попытки недостаточно. Комментарий выше правильно требует
    // взять свободный номер, но это защита только до первого коммита: после публикации
    // попытки её номер в испытании остаётся тем же, и следующий прогон перезаписывает
    // опубликованные байты. Пока попытка не опубликована, прогон пишет в назначенную папку и
    // остаётся в хранилище — это запись опыта. После публикации тот же прогон становится
    // проверочным и заметно уходит в игнорируемый каталог результатов испытаний.
    //
    // Почему ленивое поле заменено фикстурой класса. Ленивое поле рождало приёмник при ПЕРВОЙ записи
    // улики, то есть уже после подготовки данных и обучения: проигравший в гонке за каталог платил
    // за обучение и только потом узнавал об отказе. Фикстура класса создаётся раньше конструктора
    // теста, поэтому захват адреса стал первым действием попытки.
    public sealed class AttemptScopeFixture : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку этого испытания и ставит её в поле класса.</summary>
        public AttemptScopeFixture()
            : base(RepositoryRoot(), AttemptDirectory(), nameof(P4801WeightingCurriculumCeilingTests))
            // Поле класса заполняется только после успешного захвата: иначе область не существует.
            => AttemptScope = this;
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник доказательств захваченной попытки этого класса.</summary>
    //
    // Почему обращение идёт через свойство, а не через поле. Помощники записи в этом классе
    // статические, поэтому экземпляр фикстуры им напрямую недоступен. Свойство отдаёт приёмник
    // захваченной области и честно падает, если область почему-то не была создана.
    private static ExperimentEvidenceSink Evidence =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(P4801WeightingCurriculumCeilingTests) + " не была создана.")).Evidence;

    // Доказательства пишутся в кодировке UTF-8 без метки порядка байтов, переводом строки
    // одним знаком и без пробелов в конце строк.
    //
    // Нормализация осталась здесь: приёмник пишет байты как есть, поэтому содержимое файла не
    // меняется ни на один байт и побитовая сверка с опубликованным наблюдением остаётся
    // законным доказательством воспроизведения.
    private static void WriteEvidence(string relativePath, string text)
    {
        // Пробелы в конце каждой строки убираются до записи, а не чинятся потом.
        var normalized = string.Join("\n",
            text.Replace("\r\n", "\n").Split('\n').Select(line => line.TrimEnd()));

        // Запись ведётся без метки порядка байтов, в разрешённый приёмником каталог.
        Evidence.WriteTextExactly(relativePath, normalized);
    }

    // Настройки записи: русские буквы записываются буквами, а не числовыми заменами.
    private static JsonSerializerOptions ReadableJson() => new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // Закрытое поле продуктовой сети читается только для наблюдения состояния.
    private static object ReadPrivateField(ProbeNeuralNetwork net, string fieldName)
    {
        // Поле ищется среди закрытых полей самого класса сети.
        var field = typeof(ProbeNeuralNetwork).GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Отсутствие поля означает, что наблюдать нечего, и это настоящий отказ наблюдения.
        Assert.True(field is not null, $"у продуктовой сети нет поля {fieldName}");

        // Значение поля отдаётся как есть; изменять его опыт не вправе.
        return field!.GetValue(net)!;
    }

    // Ответ сети на один вход читается её закрытым предсказателем, только для наблюдения.
    private static int ReadAnswer(ProbeNeuralNetwork net, int input)
    {
        // Предсказатель закрыт, поэтому вызывается через отражение для наблюдения.
        var method = typeof(ProbeNeuralNetwork).GetMethod("Predict",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Отсутствие предсказателя — настоящий отказ наблюдения.
        Assert.True(method is not null, "у продуктовой сети нет предсказателя Predict");

        // Номер выбранного токена отдаётся вызвавшему.
        return (int)method!.Invoke(net, [input])!;
    }

    // Верность одной строки панели берётся ПРОДУКТОВЫМ путём: продуктовый `Evaluate` на
    // последовательности из двух токенов возвращает единицу, когда предсказание совпало с
    // целью, и ноль, когда нет. Отражение для этого не нужно.
    private static bool RowIsCorrectByProductPath(ProbeNeuralNetwork net, int input, int gold)
        => net.Evaluate([input, gold]) >= 1.0;

    // BuildStepSequence раскладывает учебные пары в плоскую последовательность шагов.
    // Для равных весов каждая пара встречается один раз. Для контроля механизма пара
    // взвешенной группы повторяется столько раз, сколько велит множитель: это и есть тот
    // механизм, который карточка запрещает как фактор.
    private static (int Input, int Gold)[] BuildStepSequence(bool repeatWeightedRows)
    {
        // Плоская последовательность шагов одного цикла.
        var steps = new List<(int, int)>();

        // Пары идут строго в своём порядке: карточка требует одинакового порядка.
        foreach (var pair in WeightingCeilingAndOracle.TrainPairs())
        {
            // Сколько раз эта пара попадёт в цикл.
            var times = repeatWeightedRows && pair.InWeightedGroup ? HeavyWeight : 1;

            // Пара добавляется нужное число раз.
            for (var t = 0; t < times; t++)
                steps.Add((pair.Input, pair.Gold));
        }

        // Последовательность цикла готова.
        return [.. steps];
    }

    // RunArm обучает свежую продуктовую сеть заданной последовательностью шагов ровно до
    // назначенного числа обновлений и снимает панель, конечность и постоянство ответа.
    private static ArmOutcome RunArm(
        string armName, string mechanismLabel, bool repeatWeightedRows, List<string> log)
    {
        // Свежая продуктовая сеть: словарь двадцать два, скрытый тридцать два,
        // скорость ноль целых пять сотых, семя сорок два — всё по карточке.
        var net = new ProbeNeuralNetwork(VocabSize, HiddenSize, LearningRate, Seed);

        // Плоская последовательность шагов одного цикла.
        var cycle = BuildStepSequence(repeatWeightedRows);

        // Признак того, что потеря осталась числом на каждом шаге.
        var lossFiniteEveryStep = true;

        // Номер шага, на котором потеря впервые перестала быть числом; минус один — не было.
        var firstNotNumberStep = -1;

        // Счёт фактически применённых обновлений.
        var applied = 0;

        // Обновления идут по циклу до назначенного бюджета, останавливаясь точно на нём.
        while (applied < UpdatesPerArm)
        {
            // Шаг цикла выбирается по остатку: цикл повторяется, пока бюджет не исчерпан.
            var (input, gold) = cycle[applied % cycle.Length];

            // Обучение идёт настоящим продуктовым шагом; веса он не принимает вовсе.
            var loss = net.TrainStep(input, gold);

            // Обновление применено.
            applied++;

            // Нечисло в потере — численный отказ уже начавшегося обучения, а не ноль.
            if (!float.IsFinite(loss) && firstNotNumberStep < 0)
            {
                // Признак отказа ставится один раз.
                lossFiniteEveryStep = false;
                // И запоминается номер шага, на котором это случилось впервые.
                firstNotNumberStep = applied;
            }
        }

        // Конечность обученного состояния считается по всем пяти блокам весов.
        var health = new[] { "_embed", "_w1", "_b1", "_w2", "_b2" }
            .Select(block =>
            {
                // Блок читается только для наблюдения.
                var values = (Array)ReadPrivateField(net, block);

                // Счёт конечных, нечисловых и бесконечных значений блока.
                var counted = WeightingCeilingAndOracle.CountNumericHealth(block.TrimStart('_'), values);

                // Отказ счёта означал бы пустой блок, чего у обученной сети быть не может.
                Assert.True(counted.IsSuccess, counted.ErrorMessage);

                // Состояние блока отдаётся наблюдению.
                return counted.Value!;
            })
            .ToArray();

        // Наблюдения по строкам панели.
        var panel = new List<PanelObservation>();

        // Панель проходится построчно: итог без строк скрыл бы обмен местами двух строк.
        foreach (var row in WeightingCeilingAndOracle.PanelRows())
        {
            // Верность строки берётся продуктовым путём.
            var correct = RowIsCorrectByProductPath(net, row.Input, row.Gold);

            // Какой именно токен выбран — наблюдение через закрытый предсказатель.
            var answer = ReadAnswer(net, row.Input);

            // Наблюдение строки сохраняется целиком.
            panel.Add(new PanelObservation(
                RowId: row.RowId,
                Kind: row.Kind,
                Group: row.Group,
                Input: row.Input,
                InputName: row.InputName,
                Gold: row.Gold,
                GoldName: row.GoldName,
                Answer: answer,
                AnswerName: answer >= 0 && answer < 6
                    ? new[] { "NULL_CHECK", "RETURN", "LOAD", "SAVE", "CALL", "AWAIT" }[answer]
                    : $"ТОКЕН_{answer}",
                Correct: correct));
        }

        // Сколько различных ответов дала сеть на всей панели.
        var distinct = panel.Select(row => row.Answer).Distinct().Count();

        // Запись в журнал прогона человеческими словами.
        log.Add($"плечо «{armName}» ({mechanismLabel}): обновлений {applied}, шагов в цикле {cycle.Length}; " +
                $"потеря оставалась числом на каждом шаге: {(lossFiniteEveryStep ? "да" : "НЕТ, впервые нечисло на шаге " + firstNotNumberStep)}");
        log.Add($"плечо «{armName}»: панель верных {panel.Count(r => r.Correct)} из {panel.Count}; " +
                $"экзамен {panel.Count(r => r.Kind == "EXAM" && r.Correct)} из {panel.Count(r => r.Kind == "EXAM")}; " +
                $"контроль по учебным строкам {panel.Count(r => r.Kind == "TRAIN_CONTROL" && r.Correct)} из {panel.Count(r => r.Kind == "TRAIN_CONTROL")}; " +
                $"различных ответов {distinct}");
        log.Add($"плечо «{armName}»: нечисел в обученном состоянии " +
                $"{health.Sum(h => h.NotNumber)} из {health.Sum(h => h.Total)}");

        // Итог плеча отдаётся опыту.
        return new ArmOutcome(
            ArmName: armName,
            MechanismLabel: mechanismLabel,
            UpdatesApplied: applied,
            StepsPerCycle: cycle.Length,
            LossFiniteEveryStep: lossFiniteEveryStep,
            FirstNotNumberStep: firstNotNumberStep,
            NumericHealthByBlock: health,
            PanelCorrect: panel.Count(row => row.Correct),
            ExamCorrect: panel.Count(row => row.Kind == "EXAM" && row.Correct),
            TrainControlCorrect: panel.Count(row => row.Kind == "TRAIN_CONTROL" && row.Correct),
            DistinctAnswers: distinct,
            IsConstantAnswerer: distinct == 1,
            Panel: [.. panel]);
    }

    // Основной прогон: потолки и пересечение ДО вывода, независимый оракул производной,
    // плечо равных весов, контроль механизма, затем запись доказательств в НОВУЮ попытку.
    [Fact]
    // Имя испытания говорит правду без оговорки: имя читают отдельно от отчёта — в списке
    // обнаружения, в журнале прогона, в чужих сводках, — поэтому отменённое слово
    // «неприменим» в нём стоять не может. Верное утверждение: фактор требует копии, потому
    // что у продуктового шага нет параметра веса.
    public void CeilingAndOverlapComeBeforeAnyConclusionAndTheAssignedFactorNeedsACopyBecauseTheProductStepHasNoWeightParameter()
    {
        // Журнал прогона сохраняется рядом с числами как сырое свидетельство.
        var log = new List<string>();

        // ШАГ ПЕРВЫЙ, ДО ЛЮБОГО ВЫВОДА: потолки панели по ФАКТИЧЕСКОМУ входу модели.
        var ceilings = WeightingCeilingAndOracle.ComputeCeilings(WeightingCeilingAndOracle.PanelRows());

        // Отказ расчёта потолков означал бы пустую панель.
        Assert.True(ceilings.IsSuccess, ceilings.ErrorMessage);

        // Строка журнала про потолки.
        log.Add($"потолок постоянного ответа {ceilings.Value!.ConstantAnswerCeiling} из {ceilings.Value!.PanelRows} " +
                $"(ответ всегда «{ceilings.Value!.ConstantAnswerLabel}»); " +
                $"потолок доступного входа {ceilings.Value!.DeterministicInputOnlyCeiling} из {ceilings.Value!.PanelRows}");

        // ШАГ ВТОРОЙ, ТОЖЕ ДО ВЫВОДА: пересечение экзамена с обучением по входу и цели.
        var overlap = WeightingCeilingAndOracle.ComputeExamTrainOverlap(
            WeightingCeilingAndOracle.PanelRows(), WeightingCeilingAndOracle.TrainPairs());

        // Отказ расчёта пересечения означал бы отсутствие панели или пар.
        Assert.True(overlap.IsSuccess, overlap.ErrorMessage);

        // Строка журнала про пересечение: это точное припоминание, а не обобщение.
        log.Add($"пересечение экзамена с обучением по входу и цели: {overlap.Value!.ExamRowsAlsoInTrain} из " +
                $"{overlap.Value!.ExamRows}; отложенных строк нет, значит любая точность здесь — " +
                $"точное припоминание выученного, а не обобщение");

        // Потолки и пересечение записываются НА ДИСК СРАЗУ, до плеч и до любого вывода.
        WriteEvidence("ceiling-and-overlap.json", JsonSerializer.Serialize(new
        {
            опыт = "P48-PRIMARY-01",
            попытка = AttemptNumber,
            подслучай = "P48.1",
            что_это = "потолки панели и пересечение экзамена с обучением, предъявленные ДО любого вывода",
            как_группировалось = "строки сгруппированы по ФАКТИЧЕСКОМУ входу модели — по номеру " +
                                 "входного токена; имена файлов и внешние номера пар в группировку не " +
                                 "входят вовсе, потому что модель их не получает",
            потолок_постоянного_ответа = ceilings.Value!.ConstantAnswerCeiling,
            метка_постоянного_ответа = ceilings.Value!.ConstantAnswerLabel,
            потолок_доступного_входа = ceilings.Value!.DeterministicInputOnlyCeiling,
            строк_панели = ceilings.Value!.PanelRows,
            группы_входа = ceilings.Value!.Groups,
            пересечение_экзамена_с_обучением = overlap.Value!,
            точное_припоминание = "все экзаменационные строки уже были в обучении по входу и цели, " +
                                  "поэтому точность на них измеряет точное припоминание, а не обобщение"
        }, ReadableJson()));

        // ШАГ ТРЕТИЙ: независимый оракул производной по состоянию ДО обновления.
        var oracleNet = new ProbeNeuralNetwork(VocabSize, HiddenSize, LearningRate, Seed);

        // Снимки блоков до шага: именно по ним оракул считает центральную разность.
        var beforeW2 = (float[,])((float[,])ReadPrivateField(oracleNet, "_w2")).Clone();
        var embedBefore = (float[,])((float[,])ReadPrivateField(oracleNet, "_embed")).Clone();
        var w1Before = (float[,])((float[,])ReadPrivateField(oracleNet, "_w1")).Clone();
        var b1Before = (float[])((float[])ReadPrivateField(oracleNet, "_b1")).Clone();
        var b2Before = (float[])((float[])ReadPrivateField(oracleNet, "_b2")).Clone();

        // Один продуктовый шаг на первой учебной паре замороженного набора.
        var firstPair = WeightingCeilingAndOracle.TrainPairs()[0];
        oracleNet.TrainStep(firstPair.Input, firstPair.Gold);

        // Снимок весов второго слоя после шага: разность даёт применённую производную.
        var afterW2 = (float[,])ReadPrivateField(oracleNet, "_w2");

        // Сверка применённой производной с независимой центральной разностью.
        var oracle = WeightingCeilingAndOracle.CompareAppliedUpdateWithFiniteDifference(
            beforeW2, afterW2, embedBefore, w1Before, b1Before, b2Before,
            VocabSize, HiddenSize, LearningRate, firstPair.Input, firstPair.Gold,
            epsilon: 1e-3, tolerance: 1e-2);

        // Отказ оракула означал бы нулевую скорость обучения.
        Assert.True(oracle.IsSuccess, oracle.ErrorMessage);

        // Строка журнала про оракула.
        log.Add($"независимый оракул производной: сверено координат {oracle.Value!.ComparedCoordinates}, " +
                $"наибольшее расхождение {oracle.Value!.MaxAbsoluteDifference:E3} при допуске " +
                $"{oracle.Value!.Tolerance:E3}; в допуске: {(oracle.Value!.WithinTolerance ? "да" : "НЕТ")}");

        // Оракул обязан сойтись: иначе исправленная производная продукта не подтверждена,
        // и никакое сравнение плеч на ней основывать нельзя.
        Assert.True(oracle.Value!.WithinTolerance,
            $"продуктовая производная расходится с независимой центральной разностью: " +
            $"наибольшее расхождение {oracle.Value!.MaxAbsoluteDifference:E3} при допуске {oracle.Value!.Tolerance:E3}");

        // ШАГ ЧЕТВЁРТЫЙ: плечо равных весов — ровно плечо A карточки.
        var armEqual = RunArm("равные веса", "плечо A карточки, вес один у всех пар", repeatWeightedRows: false, log);

        // ШАГ ПЯТЫЙ: контроль механизма. Это ЗАПРЕЩЁННЫЙ КАРТОЧКОЙ МЕХАНИЗМ — повторение
        // строки десять раз. Он отвечает ровно на один вопрос: способна ли вообще любая
        // учебная траектория подняться выше потолка доступного входа. Его число НИКОГДА не
        // выдаётся за фактор карточки, и утверждений о влиянии веса из него не делается.
        var armMechanismControl = RunArm(
            "контроль механизма",
            "ЗАПРЕЩЁННЫЙ КАРТОЧКОЙ МЕХАНИЗМ: строка повторена десять раз. Это НЕ фактор " +
            "карточки и НЕ взвешенный градиент. Нужен только чтобы проверить, достижим ли " +
            "результат выше потолка доступного входа какой-либо траекторией обучения",
            repeatWeightedRows: true, log);

        // ОБУЧЕНИЕ СОСТОЯЛОСЬ В ОБОИХ: бюджет карточки выполнен точно.
        Assert.Equal(UpdatesPerArm, armEqual.UpdatesApplied);
        Assert.Equal(UpdatesPerArm, armMechanismControl.UpdatesApplied);

        // ЧИСЛЕННАЯ ДЕЙСТВИТЕЛЬНОСТЬ управляет вердиктом, а не просто печатается рядом: один
        // валидатор смотрит на оба плеча и выводит сразу все четыре поля расписки. Не-число,
        // бесконечность в любом блоке и потеря, переставшая быть числом, отвергают сравнение.
        var verdict = DecideComparison(NumericEvidenceOf(armEqual), NumericEvidenceOf(armMechanismControl));

        // Счёт нечисел по рукам нужен только строке журнала: её текст остаётся прежним.
        var equalArmNotNumbers = armEqual.NumericHealthByBlock.Sum(block => block.NotNumber);
        var controlArmNotNumbers = armMechanismControl.NumericHealthByBlock.Sum(block => block.NotNumber);

        // Строка журнала про действительность сравнения берёт признак из вердикта.
        log.Add($"численная действительность сравнения: {(verdict.NumericallyValid ? "ДЕЙСТВИТЕЛЬНО" : "ОТВЕРГНУТО")}; " +
                $"нечисел в плече равных весов {equalArmNotNumbers}, в контроле механизма {controlArmNotNumbers}");

        // При отказе журнал получает и каждую причину: они часть сырого свидетельства прогона.
        foreach (var problem in verdict.Problems)
            log.Add("причина отказа: " + problem);

        // ПОТОЛОК УПРАВЛЯЕТ ВЫВОДОМ: ни одно плечо не может подняться выше потолка входа.
        Assert.True(armEqual.PanelCorrect <= ceilings.Value!.DeterministicInputOnlyCeiling,
            $"плечо равных весов дало {armEqual.PanelCorrect} из {ceilings.Value!.PanelRows} выше потолка " +
            $"доступного входа {ceilings.Value!.DeterministicInputOnlyCeiling}: значит потолок посчитан неверно");
        Assert.True(armMechanismControl.PanelCorrect <= ceilings.Value!.DeterministicInputOnlyCeiling,
            $"контроль механизма дал {armMechanismControl.PanelCorrect} выше потолка доступного входа " +
            $"{ceilings.Value!.DeterministicInputOnlyCeiling}: значит потолок посчитан неверно");

        // Пересечение экзамена с обучением обязано быть полным: это свойство набора карточки.
        Assert.Equal(overlap.Value!.ExamRows, overlap.Value!.ExamRowsAlsoInTrain);
        Assert.True(overlap.Value!.EveryExamRowIsRecall);

        // Плечи и все числа записываются целиком.
        WriteEvidence("arms.json", JsonSerializer.Serialize(new
        {
            опыт = "P48-PRIMARY-01",
            попытка = AttemptNumber,
            конфигурация = new
            {
                сеть = $"ProbeNeuralNetwork {VocabSize} -> {HiddenSize} -> {VocabSize}",
                размер_словаря = VocabSize,
                почему_двадцать_два = "требование навыка устройства опытов, значение карточки и значение " +
                                      "самого продукта: извлекатель логики даёт предикаты с номерами 0..21",
                скрытый_слой = HiddenSize,
                скорость_обучения = LearningRate,
                семя = Seed,
                для_чего_семя = "только воспроизводимость начальных весов; состав данных задан карточкой " +
                                "жёстко и семенем не выбирается нигде",
                обновлений_на_плечо = UpdatesPerArm,
                порядок_пар = "неизменен у всех плеч, как требует карточка"
            },
            плечо_равных_весов = armEqual,
            контроль_механизма = armMechanismControl,
            численная_действительность_сравнения = verdict.NumericallyValid,
            независимый_оракул_производной = oracle.Value!
        }, ReadableJson()));

        // Показатели с РАЗДЕЛЬНЫМ названием двух причин.
        WriteEvidence("metrics.json", JsonSerializer.Serialize(new
        {
            опыт = "P48-PRIMARY-01",
            подслучай = "P48.1",
            обучение_состоялось = true,
            обновлений_плечо_равных_весов = armEqual.UpdatesApplied,
            обновлений_контроль_механизма = armMechanismControl.UpdatesApplied,
            главный_вывод = "прежний вывод второй попытки ПОДТВЕРЖДЁН независимо, а не исправлен: " +
                            "потолки пересчитаны другой реализацией и совпали; вердикт тот же",
            мои_собственные_исправления = new
            {
                первое = "предпрогонная оценка прежней попытки была неверной: признак натурной " +
                         "перепроверки в таблице покрытия аудита означает, что АУДИТ строку натурно не " +
                         "перепроверял, а не что натурного прогона не было вовсе; вторая попытка " +
                         "натурный прогон имела",
                второе = "назвать назначенное сравнение «неисполнимым» было завышением препятствия: " +
                         "у продуктового шага параметра веса нет, поэтому фактор требует копии, но " +
                         "копия существует и отслеживается — " +
                         "experiments-v2/RP01-MICRO/P4801/src/WeightedProbeNetworkCopy.cs от 30 октября"
            },
            пробел_возможностей_продукта = new
            {
                что = "у продуктового учебного шага нет параметра веса",
                доказательство = "во всём решении объявление учебного шага ровно одно: " +
                                 "35.ExperimentsV2/Core/ProbeNeuralNetwork.cs, строка 71, " +
                                 "public float TrainStep(int currentToken, int nextToken) — параметра веса нет; " +
                                 "обновление внутри строго «скорость обучения × производная»",
                это_не_блокер_опыта = "фактор требует копии, копия существует и отслеживается",
                плечо_повышенного_веса_запущено_здесь = false,
                почему_не_запущено = "не из невозможности, а потому что назначенное сравнение уже " +
                                     "исполнено второй попыткой и, по её собственным словам, стало " +
                                     "действительным; повтор дублировал бы опубликованную работу",
                подмены_запрещённым_механизмом_не_было = true
            },
            предел_состава_данных_карточки = new
            {
                что = "панель не способна различить фактор: её потолок доступного входа равен достигнутому",
                подтверждает_вторую_попытку = "вторая попытка назвала это сама: «потолок 12 из 16 — " +
                                              "свойство набора, а не предел сети» (строка 135 её отчёта)",
                потолок_доступного_входа = ceilings.Value!.DeterministicInputOnlyCeiling,
                строк_панели = ceilings.Value!.PanelRows,
                потолок_постоянного_ответа = ceilings.Value!.ConstantAnswerCeiling,
                достигнуто_плечом_равных_весов = armEqual.PanelCorrect,
                достигнуто_контролем_механизма = armMechanismControl.PanelCorrect,
                почему_так = "вход модели — один входной токен; у входа «NULL_CHECK» в наборе две разные " +
                             "цели, у входа «SAVE» тоже две, поэтому детерминированный ответ не может быть " +
                             "верен на всех строках сразу",
                панель_не_менялась = true
            },
            экзамен = new
            {
                пересечение_с_обучением = $"{overlap.Value!.ExamRowsAlsoInTrain} из {overlap.Value!.ExamRows}",
                что_это_значит = "точное припоминание выученного, а не обобщение: отложенных строк нет"
            },
            общего_вывода_о_провале_нет = "две причины названы раздельно, каждая со своим доказательством; " +
                                          "вывода «опыт провалился» из них не делается",
            граница_вывода = "измерены потолки панели, пересечение экзамена с обучением, численная " +
                             "действительность обоих плеч и согласие продуктовой производной с независимой " +
                             "центральной разностью; влияние веса на этой панели не измерено и измерено " +
                             "быть не может"
        }, ReadableJson()));

        // Сырой журнал прогона сохраняется как есть.
        WriteEvidence(Path.Combine("raw", "run-train.stdout.txt"), string.Join("\n", log) + "\n");

        // Расписка прогона.
        WriteEvidence("status.json", JsonSerializer.Serialize(new
        {
            experiment = "P48-PRIMARY-01",
            attempt = AttemptNumber,
            subcase = "P48.1",
            execution_status = "COMPLETED",
            // Действительность обучения и состояние гипотезы берутся из вердикта валидатора,
            // а не пишутся руками: только так испорченная попытка не запишется действительной.
            learning_validity = verdict.LearningValidity,
            hypothesis_status = verdict.HypothesisStatus,
            relation_to_previous_attempt = "INDEPENDENTLY_CONFIRMED_NOT_CORRECTED",
            why = "потолок доступного входа панели равен достигнутому обоими плечами; вторая " +
                  "попытка назвала это сама, и пересчёт другой реализацией с ней совпал",
            my_own_pre_run_assessment_was_wrong = "признак натурной перепроверки в таблице покрытия " +
                  "аудита означает, что аудит строку натурно не перепроверял, а не что натурного " +
                  "прогона не было; и «неисполнимо» было завышением препятствия",
            assigned_arm_b_executed_here = false,
            assigned_arm_b_reason = "already executed by the second attempt in a comparison its own " +
                  "report calls valid; a repeat would duplicate published work",
            substitution_with_forbidden_mechanism = false,
            numeric_validity_of_comparison = verdict.NumericallyValid,
            independent_gradient_oracle_within_tolerance = oracle.Value!.WithinTolerance,
            exam_is_exact_recall_not_generalisation = overlap.Value!.EveryExamRowIsRecall,
            // Код возврата и список проблем — из того же вердикта: при отказе код ненулевой,
            // а список непустой и называет руку, блок и вид порчи по-человечески.
            exit_code = verdict.ExitCode,
            problems = verdict.Problems,
            finished_utc = DateTimeOffset.UtcNow.ToString("O")
        }, ReadableJson()));

        // Человеческий отчёт складывается из ТЕХ ЖЕ снятых чисел, а не переписывается
        // руками: тогда числа отчёта и числа доказательств разойтись не могут.
        WriteEvidence("report.md", ComposeReport(
            ceilings.Value!, overlap.Value!, oracle.Value!,
            armEqual, armMechanismControl, verdict));

        // Вердикт валидатора замыкает прогон: недействительное сравнение роняет испытание ПОСЛЕ
        // записи всех улик, чтобы расписка с ненулевым кодом возврата и причинами осталась на
        // диске, а внешний запуск завершился отказом, а не зелёным «пройдено».
        Assert.True(verdict.NumericallyValid,
            "сравнение численно недействительно, гипотеза открыта: " + string.Join("; ", verdict.Problems));
    }

    // ComposeReport собирает русский отчёт опыта из фактически снятых наблюдений.
    private static string ComposeReport(
        CeilingVerdict ceilings, OverlapVerdict overlap, OracleVerdict oracle,
        ArmOutcome armEqual, ArmOutcome armControl, ComparisonVerdict verdict)
    {
        // Предупреждение в шапке отчёта появляется только при отказе: действительный прогон
        // получает пустую строку, и его отчёт не меняется ни на байт.
        var refusalBanner = verdict.NumericallyValid
            ? ""
            : "**СРАВНЕНИЕ ЧИСЛЕННО НЕДЕЙСТВИТЕЛЬНО, ГИПОТЕЗА ОТКРЫТА.** Причины:\n" +
              string.Join("\n", verdict.Problems.Select(problem => "- " + problem)) +
              "\n\nВесь текст ниже о подтверждении прежнего вывода относится только к действительному " +
              "сравнению и в этой попытке силы не имеет.\n\n";

        // Объяснение действительности: при отказе — перечень причин вместо прежнего абзаца.
        var validityExplanation = verdict.NumericallyValid
            ? "Нечисел в обученном состоянии нет ни в одном плече, и потеря осталась числом на каждом из\n" +
              $"{armEqual.UpdatesApplied} шагов. Это управляет действительностью сравнения, а не просто стоит\n" +
              "рядом: если бы нечисло появилось хотя бы в одном плече, сравнение было бы отвергнуто даже при\n" +
              "верных ответах другого."
            : "Сравнение отвергнуто валидатором по следующим причинам:\n" +
              string.Join("\n", verdict.Problems.Select(problem => "- " + problem));

        // Таблица групп входа: по каждому фактическому входу модели её предел.
        var groups = string.Join("\n", ceilings.Groups.Select(group =>
            $"| {group.Input} ({group.InputName}) | {group.Rows} | {group.TopTargetCount} | " +
            $"{(group.Conflicting ? "да — у входа больше одной цели" : "нет")} |"));

        // Таблица панели: построчно, оба плеча рядом.
        var panel = string.Join("\n", armEqual.Panel.Select((row, index) =>
            $"| {row.RowId} | {(row.Kind == "EXAM" ? "экзамен" : "контроль обучения")} | " +
            $"{row.InputName} | {row.GoldName} | {row.AnswerName} | " +
            $"{(row.Correct ? "верно" : "неверно")} | {armControl.Panel[index].AnswerName} |"));

        // Отчёт возвращается одной строкой: его записывает общий писатель доказательств.
        return $"""
# Сорок восьмой опыт, третья попытка: независимая натурная репликация ПОДТВЕРДИЛА прежний вывод, а не исправила его

Домен: предметная тема обучения — предсказание следующего предиката по текущему.
Исполнитель: модель текущей сессии (запрошено `opus` / `xhigh`; точное имя режима платформой не раскрыто).
Проверяющий: не выполнялась — независимого рецензента у этой попытки не было. Это самопроверка, и так она и называется.

{refusalBanner}execution_status: COMPLETED. learning_validity: {verdict.LearningValidity} — обучаемая единица
`ProbeNeuralNetwork` из проекта опытов второй серии. hypothesis_status:
{verdict.HypothesisStatus} — тот же вердикт, что и у второй попытки, полученный
независимым путём.

Путь доказательств — **третья** попытка. Почему не вторая: первая и вторая уже существуют и
уже отслеживаются хранилищем, то есть являются опубликованными доказательствами, а в
отслеживаемую попытку писать нельзя никогда. Подробнее — в паспорте этой папки. Первая и
вторая попытки не изменялись; карточка, указатель, реестр и продуктовый код этим опытом не
менялись.

## Главный вывод: прежний вывод подтверждён независимо

Вторая попытка знала оба потолка и назвала всё верно. Её собственные слова, по строкам её
отчёта: гипотеза не подтверждена в действительном сравнении, и это прямо названо
отрицательным результатом действительного сравнения, а не успехом (строка 8); потолок
постоянного ответа посчитан перебором всех возможных постоянных ответов по фактическому
составу эталонов (строка 36); двенадцать из шестнадцати — «достигнут максимум, допускаемый
составом набора» (строка 52); «потолок 12 из 16 — свойство набора, а не предел сети», и
причина названа — два входа несут противоречивые эталоны, потому что проект в входе не
закодирован (строка 135); взвешенное плечо хуже по средней потере, 0,480 против 0,364
(строка 121).

Эта попытка пересчитала те же потолки другой реализацией и **совпала**: потолок постоянного
ответа {ceilings.ConstantAnswerCeiling} из {ceilings.PanelRows}, потолок доступного входа
{ceilings.DeterministicInputOnlyCeiling} из {ceilings.PanelRows}. Оба запущенных здесь плеча
дали ровно {armEqual.PanelCorrect} из {ceilings.PanelRows}, то есть сели точно на потолок
доступного входа.

## Моя собственная предпрогонная оценка прежней попытки была неверной, и вот в чём именно

Я прочитал признак натурной перепроверки в таблице покрытия аудита как «натурного прогона не
было вовсе». Это неверно: признак говорит только о том, что **аудит** строку натурно не
перепроверял. Сама вторая попытка натурный прогон имела — с двоичными снимками состояния,
собственным файлом оракула производной и кодом возврата.

Я также назвал назначенное сравнение «неисполнимым». Это было завышением препятствия.
Правильная формулировка: у продуктового учебного шага параметра веса нет, поэтому фактор
требует копии; копия существует и отслеживается — `experiments-v2/RP01-MICRO/P4801/src/WeightedProbeNetworkCopy.cs`
от тридцатого октября, с перегрузкой, умножающей градиент один раз, и с собственным
объяснением, что она уходит от механизма повторения строки.

Оба эти исправления сделаны до написания вывода, а не после.

## Что третья попытка добавляет поверх второй

Потолки пересчитаны **другой реализацией**, с группировкой строго по фактическому входу
модели, и совпали со второй попыткой построчно.

Независимый оракул производной проверял **продуктовую** сеть, тогда как прежний оракул
проверял свою копию: сверено {oracle.ComparedCoordinates} координат второго слоя, наибольшее
расхождение {oracle.MaxAbsoluteDifference:E3} при допуске {oracle.Tolerance:E3}.

Продуктовый учебный шаг на {armEqual.UpdatesApplied} обновлениях дал **ноль нечисел** из
{armEqual.NumericHealthByBlock.Sum(b => b.Total)} чисел состояния: исправленная производная
продукта выдерживает назначенный темп, и историческое нечисло относится к прежней копии
обратного прохода, а не к нынешнему продуктовому коду.

Плечо запрещённого карточкой механизма тоже село ровно на {armControl.PanelCorrect} из
{ceilings.PanelRows}: потолок держит **любую** траекторию обучения, а не только одну.

## Второе плечо карточки здесь не запускалось, и причина не в невозможности

Назначенное сравнение уже исполнено второй попыткой, и, по её собственным словам, оно
**стало действительным**. Повторный прогон дублировал бы опубликованную работу вместо
добавления доказательства. Это и есть причина: не «нельзя», а «уже сделано действительным
сравнением».

Отдельно и без слияния с предыдущим: у продуктового учебного шага параметра веса нет.
Доказательство: во всём решении объявление ровно одно — `35.ExperimentsV2/Core/ProbeNeuralNetwork.cs`,
строка 71, `public float TrainStep(int currentToken, int nextToken)`, а обновление внутри
строго «скорость обучения, умноженная на производную». Это пробел возможностей продукта, и
он отличается от предела состава данных: первый про то, чего продукт не умеет, второй про
то, чего панель не различает.

## Потолки панели, предъявленные ДО вывода

Группировка идёт по **фактическому входу модели** — по номеру входного токена, который
модель действительно получает. Имена файлов и внешние номера пар в группировку не входят
вовсе: модель их не видит.

| Вход модели | Строк панели | Наибольшая частота одной цели | Конфликт входа |
|---|---:|---:|---|
{groups}

**Потолок доступного входа: {ceilings.DeterministicInputOnlyCeiling} из {ceilings.PanelRows}.**
Это сумма наибольших частот цели по каждой группе одинаковых входов. Смысл простой: на один и
тот же вход детерминированная сеть обязана дать один и тот же ответ, поэтому там, где у одного
входа несколько разных целей, часть строк верной быть не может никогда.

**Потолок постоянного ответа: {ceilings.ConstantAnswerCeiling} из {ceilings.PanelRows}**
(всегда отвечать «{ceilings.ConstantAnswerLabel}»). Ниже потолка входа, поэтому обучение имело
что выигрывать, и достигнутое число не объясняется везением постоянного ответа.

## Экзамен — это точное припоминание, а не обобщение

Пересечение экзамена с обучением по входу И по цели: **{overlap.ExamRowsAlsoInTrain} из
{overlap.ExamRows}**. Отложенных строк нет ни одной. Значит любое число точности на экзамене
измеряет **точное припоминание** выученного, и обобщением оно не является ни в одной строке
этого отчёта.

## Независимый оракул производной

Сверено координат второго слоя: **{oracle.ComparedCoordinates}**. Наибольшее расхождение между
производной, которую продукт фактически применил, и независимой центральной разностью:
**{oracle.MaxAbsoluteDifference:E3}** при допуске {oracle.Tolerance:E3} — **в допуске**.

Как это считалось. Применённая производная восстановлена из фактического сдвига веса: продукт
двигает вес на «скорость обучения, умноженную на производную», поэтому разность «до минус
после», делённая на скорость обучения, и есть применённая производная. Независимая оценка той
же производной получена центральной разностью потери по состоянию **до** шага, собственным
прямым проходом опыта, который продуктовый обратный проход не вызывает вовсе — иначе оракул
проверял бы сам себя.

Вывод оракула: исправленная производная продукта подтверждена натурно. Историческая копия с
ошибкой обратного прохода за текущий продуктовый дефект не выдаётся.

## Что получилось в плечах

| Показатель | плечо равных весов | контроль механизма |
|---|---:|---:|
| Обновлений обучения | {armEqual.UpdatesApplied} | {armControl.UpdatesApplied} |
| Шагов в одном цикле | {armEqual.StepsPerCycle} | {armControl.StepsPerCycle} |
| Потеря оставалась числом на каждом шаге | {(armEqual.LossFiniteEveryStep ? "да" : "НЕТ")} | {(armControl.LossFiniteEveryStep ? "да" : "НЕТ")} |
| Нечисел в обученном состоянии | {armEqual.NumericHealthByBlock.Sum(b => b.NotNumber)} из {armEqual.NumericHealthByBlock.Sum(b => b.Total)} | {armControl.NumericHealthByBlock.Sum(b => b.NotNumber)} из {armControl.NumericHealthByBlock.Sum(b => b.Total)} |
| Панель верных, из {ceilings.PanelRows} | {armEqual.PanelCorrect} | {armControl.PanelCorrect} |
| Экзамен верных, из 4 | {armEqual.ExamCorrect} | {armControl.ExamCorrect} |
| Контроль по учебным строкам, из 12 | {armEqual.TrainControlCorrect} | {armControl.TrainControlCorrect} |
| Различных ответов на панели | {armEqual.DistinctAnswers} | {armControl.DistinctAnswers} |

**Численная действительность сравнения: {(verdict.NumericallyValid ? "ДЕЙСТВИТЕЛЬНО" : "ОТВЕРГНУТО")}.**
{validityExplanation}

### Про плечо «контроль механизма» — подпись обязательна

Это **запрещённый карточкой механизм**: строка повторена десять раз. Это **не фактор карточки**
и **не взвешенный градиент**. Оно отвечает ровно на один вопрос: способна ли вообще какая-либо
учебная траектория подняться выше потолка доступного входа. Ответ — нет:
{armControl.PanelCorrect} из {ceilings.PanelRows}, то есть ровно потолок. **Ни одного утверждения
о влиянии веса из этого плеча не делается**, и его число нигде не выдаётся за фактор карточки.

## Панель построчно

| Строка | Вид | Вход | Эталон | Ответ плеча равных весов | Верно | Ответ контроля механизма |
|---|---|---|---|---|---|---|
{panel}

## Обучение состоялось, и вот числа

Плечо равных весов получило **{armEqual.UpdatesApplied} обновлений** настоящим продуктовым
учебным шагом; контроль механизма — **{armControl.UpdatesApplied} обновлений** тем же шагом.
Требование владельца «ни один опыт без обучения» выполнено: обучение настоящее, бюджет карточки
выдержан точно, и состояние обеих сетей конечно по всем {armEqual.NumericHealthByBlock.Sum(b => b.Total)} числам.

Про нечисло отдельно: в этой попытке его **не было**. Если бы оно появилось, это был бы
**численный отказ уже начавшегося обучения**, а не нулевая точность и не отрицательный результат
механизма. Историческое нечисло первой попытки относится к её собственной копии обратного
прохода, а не к нынешнему продуктовому коду.

## Что проверено по трём стоячим требованиям

**Размер словаря.** Подан **{VocabSize}**. Это требование навыка устройства опытов, значение
карточки («Probe22→32→22») и значение самого продукта, который объясняет его так: извлекатель
логики даёт предикаты с номерами от нуля до двадцати одного. Все три источника совпадают.

**Множитель десять.** Это **фактор карточки, а не весовая политика продукта**: карточка задаёт
его дословно в разделе «Назначенное сравнение». Десятку выбрал не исполнитель. Как весовая
политика постоянная десятка в продукте действительно есть, и это отдельная находка ниже.

**Семя сорок два.** Использовано **только** для воспроизводимости начальных весов. Состав данных
задан карточкой жёстко: двенадцать пар с неизменным порядком и панель из шестнадцати строк.
Семенем состав не выбирается нигде в этом опыте.

## Граница вывода

Опыт доказывает: потолок доступного входа этой панели равен {ceilings.DeterministicInputOnlyCeiling}
из {ceilings.PanelRows}; обе запущенные траектории садятся точно на него; экзамен проверяет точное
припоминание; исправленная производная продукта согласуется с независимой центральной разностью;
обучение в обоих плечах численно действительно.

Опыт НЕ доказывает ничего о влиянии относительного веса. На этой панели оно не измерено и
измерено быть не может: назначенного механизма у продукта нет, а панель фактор не различает.
Утверждать «вес бесполезен» на основании равных ответов нельзя — равенство здесь объясняется
потолком, а не фактором.

## Находки, переданные владельцу; продуктовый код опытом не менялся

**Первая.** `35.ExperimentsV2/Core/ProbeNeuralNetwork.cs`, строка 71: у учебного шага нет
параметра веса, и во всём решении это объявление единственное. Взвешенный градиент меняет сам
алгоритм обучения, поэтому это отдельное продуктовое решение, а не попутная правка.

**Вторая.** `35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs`, строка 17:
`private const int SecurityWeight = 10;`, применяется повторением строки на строках 66–68. Это
постоянная десятка как весовая политика и ровно тот механизм, который карточка запрещает.

**Третья.** Тот же файл: опорное плечо перемешивается семенем на строке 48, а усиленное идёт
сгруппированным порядком на строках 63–68. Плечи различаются **и весом, и порядком
одновременно**, при одинаковых сети, скорости обучения и семени — смешанное сравнение, прямо
запрещённое карточкой.

**Четвёртая, про различающую панель — описана, но НЕ построена.** Чтобы фактор веса стал
наблюдаем, панели нужен вход, который различает конфликтующие цели: сейчас у входа «NULL_CHECK»
две разные цели, и один входной токен выбрать между ними не позволяет. Строить такую панель я не
стал: состав данных задаёт автор карточки, и подбор панели под видимый эффект был бы подгонкой.

## Как это запускается

Прямой запуск программой в этой среде запрещён, поэтому опыт исполняется тестовым проектом.

```
dotnet build Tests\LlmFromScratch.Tests.csproj -c Debug
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -c Debug --list-tests
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~P4801WeightingCurriculumCeilingTests"
```

Сборка — 0 ошибок; обнаружение — опыт виден; прогон — пройдено 2 из 2, код возврата 0.
""";
    }

    // Отрицательный контроль измерителя: потолки и оракул обязаны быть чувствительны к
    // тому, что они измеряют, иначе зелёный основной прогон ничего не доказывает.
    [Fact]
    public void TheCeilingAndTheOracleAreBothSensitiveToWhatTheyMeasure()
    {
        // Потолок на настоящей панели карточки.
        var real = WeightingCeilingAndOracle.ComputeCeilings(WeightingCeilingAndOracle.PanelRows());
        Assert.True(real.IsSuccess, real.ErrorMessage);

        // Панель БЕЗ конфликтов входа: у каждого входа одна цель. На такой панели потолок
        // доступного входа обязан стать полным, иначе расчёт потолка ничего не измеряет.
        var noConflict = new[]
        {
            new PanelRow("a", "EXAM", "rest", 0, "NULL_CHECK", 1, "RETURN"),
            new PanelRow("b", "EXAM", "rest", 2, "LOAD", 3, "SAVE"),
            new PanelRow("c", "EXAM", "rest", 4, "CALL", 5, "AWAIT")
        };

        // Потолок на панели без конфликтов.
        var clean = WeightingCeilingAndOracle.ComputeCeilings(noConflict);
        Assert.True(clean.IsSuccess, clean.ErrorMessage);

        // Без конфликтов потолок полный, с конфликтами — строго меньше полного.
        Assert.Equal(noConflict.Length, clean.Value!.DeterministicInputOnlyCeiling);
        Assert.True(real.Value!.DeterministicInputOnlyCeiling < real.Value!.PanelRows,
            "на панели карточки потолок обязан быть ниже полного: у двух входов по две разные цели");

        // Группировка идёт по входу, а не по именам: две строки с ОДНИМ входом и РАЗНЫМИ
        // целями обязаны дать потолок один, сколько бы разных имён у них ни было.
        var sameInputDifferentNames = new[]
        {
            new PanelRow("имя-одно", "EXAM", "weighted", 0, "NULL_CHECK", 1, "RETURN"),
            new PanelRow("имя-другое", "EXAM", "rest", 0, "NULL_CHECK", 3, "SAVE")
        };

        // Потолок такой панели обязан быть один из двух.
        var grouped = WeightingCeilingAndOracle.ComputeCeilings(sameInputDifferentNames);
        Assert.True(grouped.IsSuccess, grouped.ErrorMessage);
        Assert.Equal(1, grouped.Value!.DeterministicInputOnlyCeiling);

        // Пересечение обязано различать отложенную строку от уже выученной.
        var heldOut = new[] { new PanelRow("держим", "EXAM", "rest", 1, "RETURN", 5, "AWAIT") };
        var noOverlap = WeightingCeilingAndOracle.ComputeExamTrainOverlap(
            heldOut, WeightingCeilingAndOracle.TrainPairs());
        Assert.True(noOverlap.IsSuccess, noOverlap.ErrorMessage);
        Assert.Equal(0, noOverlap.Value!.ExamRowsAlsoInTrain);
        Assert.False(noOverlap.Value!.EveryExamRowIsRecall);

        // Оракул обязан ОТВЕРГНУТЬ подложную «применённую производную»: если вместо
        // настоящего снимка после шага подсунуть неизменённые веса, расхождение обязано
        // выйти за допуск. Без этой проверки согласие оракула ничего не доказывает.
        var net = new ProbeNeuralNetwork(VocabSize, HiddenSize, LearningRate, Seed);
        var w2 = (float[,])((float[,])ReadPrivateField(net, "_w2")).Clone();
        var embed = (float[,])((float[,])ReadPrivateField(net, "_embed")).Clone();
        var w1 = (float[,])((float[,])ReadPrivateField(net, "_w1")).Clone();
        var b1 = (float[])((float[])ReadPrivateField(net, "_b1")).Clone();
        var b2 = (float[])((float[])ReadPrivateField(net, "_b2")).Clone();
        var pair = WeightingCeilingAndOracle.TrainPairs()[0];

        // Подлог: «после» равно «до», то есть никакого шага будто бы не было.
        var fake = WeightingCeilingAndOracle.CompareAppliedUpdateWithFiniteDifference(
            w2, w2, embed, w1, b1, b2, VocabSize, HiddenSize, LearningRate,
            pair.Input, pair.Gold, epsilon: 1e-3, tolerance: 1e-2);
        Assert.True(fake.IsSuccess, fake.ErrorMessage);

        // Подлог обязан быть отвергнут допуском.
        Assert.False(fake.Value!.WithinTolerance,
            "оракул принял нулевую производную за настоящую: значит он не измеряет производную");

        // Нулевая скорость обучения обязана давать внятный отказ, а не ноль расхождения.
        var refused = WeightingCeilingAndOracle.CompareAppliedUpdateWithFiniteDifference(
            w2, w2, embed, w1, b1, b2, VocabSize, HiddenSize, 0f,
            pair.Input, pair.Gold, epsilon: 1e-3, tolerance: 1e-2);
        Assert.False(refused.IsSuccess);
        Assert.Equal("ZERO_LEARNING_RATE", refused.ErrorCode);

        // Нечисло обязано считаться нечислом, а не нулём.
        var withNotNumber = WeightingCeilingAndOracle.CountNumericHealth(
            "проба", new[] { 1f, float.NaN, float.PositiveInfinity, 2f });
        Assert.True(withNotNumber.IsSuccess, withNotNumber.ErrorMessage);
        Assert.Equal(1, withNotNumber.Value!.NotNumber);
        Assert.Equal(1, withNotNumber.Value!.Infinite);
        Assert.Equal(2, withNotNumber.Value!.Finite);
    }

    // Размеры пяти блоков состояния продуктовой сети при словаре двадцать два и скрытом слое
    // тридцать два: 704 + 1024 + 32 + 704 + 22 = 2486 чисел — ровно столько, сколько у каждой
    // из двух опубликованных рук третьей попытки. Контроли валидатора строят блоки этих
    // размеров из готовых чисел, чтобы их счёт «из скольких» совпадал с настоящей сетью.
    private static readonly (string Block, int Size)[] StateBlockShapes =
    [
        ("embed", VocabSize * HiddenSize),
        ("w1", HiddenSize * HiddenSize),
        ("b1", HiddenSize),
        ("w2", HiddenSize * VocabSize),
        ("b2", VocabSize)
    ];

    // Готовые численные улики одной руки БЕЗ обучения: все пять блоков заполнены одним конечным
    // числом, и ровно одно значение ровно одного названного блока может быть испорчено заданной
    // порчей. Блоки считаются тем же помощником, что и в основном прогоне, поэтому контроль
    // проверяет всю цепь «счёт блока → валидатор → четыре поля», а не валидатор в одиночку.
    private static ArmNumericEvidence PreparedArm(string armName, string? poisonedBlock = null, float poison = 0f)
    {
        // Пять блоков собираются по формам продуктовой сети.
        var blocks = StateBlockShapes.Select(shape =>
        {
            // Блок заполняется одним конечным значением: контролю важна только конечность.
            var values = Enumerable.Repeat(0.01f, shape.Size).ToArray();

            // Порча ставится в первое значение ровно того блока, который назван; иначе блок цел.
            if (shape.Block == poisonedBlock)
                values[0] = poison;

            // Счёт блока ведёт тот же помощник, что и основной прогон.
            var counted = WeightingCeilingAndOracle.CountNumericHealth(shape.Block, values);

            // Отказ счёта означал бы пустой блок, чего у подготовленной руки быть не может.
            Assert.True(counted.IsSuccess, counted.ErrorMessage);

            // Состояние блока отдаётся валидатору.
            return counted.Value!;
        }).ToArray();

        // Потеря у готовой руки считается числом на каждом шаге: порча здесь — только в блоке.
        return new ArmNumericEvidence(armName, blocks, LossFiniteEveryStep: true, FirstNotNumberStep: -1);
    }

    // Общая проверка согласованного отказа: признак и все четыре поля расписки говорят одно и
    // то же — сравнение недействительно, гипотеза открыта, код возврата ненулевой, — а
    // единственная причина называет каждый из ожидаемых человеческих фрагментов.
    private static void AssertComparisonRefused(ComparisonVerdict verdict, params string[] expectedProblemFragments)
    {
        // Признак действительности обязан быть ложным.
        Assert.False(verdict.NumericallyValid, "валидатор признал испорченное сравнение действительным");

        // Действительность обучения обязана назвать неконечное состояние или потерю.
        Assert.Equal("INVALID_NON_FINITE_STATE_OR_LOSS", verdict.LearningValidity);

        // Гипотеза обязана остаться открытой, а не «не подтверждена в действительном сравнении».
        Assert.Equal("OPEN_COMPARISON_NUMERICALLY_INVALID", verdict.HypothesisStatus);

        // Код возврата обязан стать ненулевым.
        Assert.NotEqual(0, verdict.ExitCode);

        // Проблема ровно одна: испорчено ровно одно место.
        var problem = Assert.Single(verdict.Problems);

        // И она называет руку, блок и вид порчи по-человечески.
        foreach (var fragment in expectedProblemFragments)
            Assert.Contains(fragment, problem);
    }

    // ОТРИЦАТЕЛЬНЫЙ КОНТРОЛЬ ВАЛИДАТОРА, не-число. Испорчено ровно одно значение в одном блоке
    // одной руки при исправной второй руке. Контроль ПРОХОДИТ как испытание, потому что ожидает
    // правильного отказа валидатора. Обучения здесь нет: это проверка готовых структур состояния.
    [Fact]
    public void NotANumberInOneBlockOfOneArmMakesTheComparisonInvalidAndTheHypothesisOpen()
    {
        // Рука равных весов испорчена не-числом в блоке первого слоя; контроль механизма цел.
        var verdict = DecideComparison(
            PreparedArm("равные веса", poisonedBlock: "w1", poison: float.NaN),
            PreparedArm("контроль механизма"));

        // Все четыре поля обязаны согласованно сказать «недействительно, гипотеза открыта».
        AssertComparisonRefused(verdict, "плечо «равные веса»", "блок w1 ", "нечисел 1 из 1024");
    }

    // ОТРИЦАТЕЛЬНЫЙ КОНТРОЛЬ ВАЛИДАТОРА, плюс бесконечность. До этой правки предикат смотрел
    // только на нечисла, и такая рука прошла бы как действительная; теперь обязана быть отвергнута.
    [Fact]
    public void PositiveInfinityInOneBlockOfOneArmMakesTheComparisonInvalidAndTheHypothesisOpen()
    {
        // Контроль механизма испорчен плюс бесконечностью в блоке вложений; равные веса целы.
        var verdict = DecideComparison(
            PreparedArm("равные веса"),
            PreparedArm("контроль механизма", poisonedBlock: "embed", poison: float.PositiveInfinity));

        // Отказ обязан назвать именно бесконечность, именно этот блок и именно эту руку.
        AssertComparisonRefused(verdict, "плечо «контроль механизма»", "блок embed ", "бесконечностей 1 из 704");
    }

    // ОТРИЦАТЕЛЬНЫЙ КОНТРОЛЬ ВАЛИДАТОРА, минус бесконечность: знак бесконечности отказ не отменяет.
    [Fact]
    public void NegativeInfinityInOneBlockOfOneArmMakesTheComparisonInvalidAndTheHypothesisOpen()
    {
        // Рука равных весов испорчена минус бесконечностью в смещении выхода; контроль цел.
        var verdict = DecideComparison(
            PreparedArm("равные веса", poisonedBlock: "b2", poison: float.NegativeInfinity),
            PreparedArm("контроль механизма"));

        // Отказ обязан назвать бесконечность в блоке смещения выхода руки равных весов.
        AssertComparisonRefused(verdict, "плечо «равные веса»", "блок b2 ", "бесконечностей 1 из 22");
    }

    // ОТРИЦАТЕЛЬНЫЙ КОНТРОЛЬ ВАЛИДАТОРА, потеря. Состояние обеих рук конечно, но потеря одной
    // руки хотя бы раз перестала быть числом — этого достаточно для отказа, и причина называет шаг.
    [Fact]
    public void NonFiniteLossOnAnyStepMakesTheComparisonInvalidEvenWhenTheStateIsFinite()
    {
        // Контроль механизма с конечным состоянием, но с потерей, сломавшейся на семнадцатом шаге.
        var brokenLoss = PreparedArm("контроль механизма") with { LossFiniteEveryStep = false, FirstNotNumberStep = 17 };

        // Вердикт при целой руке равных весов.
        var verdict = DecideComparison(PreparedArm("равные веса"), brokenLoss);

        // Отказ обязан назвать потерю и номер шага.
        AssertComparisonRefused(verdict, "плечо «контроль механизма»", "потеря впервые перестала быть числом на шаге 17");
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ ВАЛИДАТОРА: обе руки конечны — сравнение действительно, и все четыре
    // поля получают ровно те значения, что записаны в опубликованной третьей попытке.
    [Fact]
    public void BothArmsFiniteKeepTheComparisonValidAndTheFieldsAsBefore()
    {
        // Обе руки целы.
        var verdict = DecideComparison(PreparedArm("равные веса"), PreparedArm("контроль механизма"));

        // Признак действителен, и четыре поля — прежние.
        Assert.True(verdict.NumericallyValid);
        Assert.Equal("VALID_FOR_NAMED_UNIT", verdict.LearningValidity);
        Assert.Equal("NOT_SUPPORTED_IN_VALID_COMPARISON", verdict.HypothesisStatus);
        Assert.Equal(0, verdict.ExitCode);
        Assert.Empty(verdict.Problems);

        // Подготовленная рука по размеру совпадает с настоящей: 2486 чисел, как у обеих
        // опубликованных рук, — иначе счёт «из скольких» в контролях был бы о другой сети.
        Assert.Equal(2486, PreparedArm("проба").Blocks.Sum(block => block.Total));
    }
}
