// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «до выбора навыка должен дойти именно обученный
// сигнал» (карточка «Дополнения к переносу: кто читает выученное состояние и как
// оно сохраняет смысл», номер 99 в папке docs/plans/replanning-possible-diffs/
// fixes-based-on-experiments, раздел «До выбора навыка должен дойти именно
// обученный сигнал»).
//
// ЧТО ЗДЕСЬ ПРОВЕРЯЕТСЯ ПО-ЧЕЛОВЕЧЕСКИ. Когда пользователь просит «сначала
// проверь код, потом меняй» или наоборот, продукт обязан выбрать и загрузить
// навык с нужным порядком шагов. До этого переноса выбор навыка опирался только
// на совпадение словесных признаков (мера Жаккара) и порог 0,40: типизированного
// обученного сигнала в выборе не было вовсе. Этот класс проверяет, что ответ
// НАСТОЯЩЕЙ обученной памяти доходит до обычного выбора навыка и меняет и
// выбранный навык, и фактически загруженное тело с порядком шагов.
//
// ГРАНИЦА ДЕТЕРМИНИРОВАННОГО И ОБУЧЕННОГО. Мера Жаккара и порог 0,40 остаются
// детерминированным базовым основанием (baseline, внутренняя деталь). Обученную
// власть несёт отдельная единица разрешённого уровня «логическая единица»
// (KnnLogicalUnit — обученная память ближайших соседей): у неё есть вход
// обучения, собственное обученное состояние и чтение этого состояния. Её ответ
// приходит в выбор как СЛАГАЕМОЕ к базовой мере.
//
// ЧТО ЭТО ПОКРЫТИЕ НЕ ДОКАЗЫВАЕТ. Оно не доказывает качества всех 48 навыков
// продуктового каталога: проверяются композиционные инварианты на назначенной
// паре навыков. Оно не доказывает сохранения и холодного восстановления
// обученного состояния: в продукте для этого маршрута ещё нет ни сохранения
// контрольной точки, ни её загрузки, и этот класс обучает один живой объект.
// Оно не доказывает включения моста в дефолтном узле продукта: карточка это
// прямо запрещает до приёмки полного каталога и совместимости отката, и по
// умолчанию обученного сигнала в запросе нет. Уверенность обученной памяти —
// это шкала близости соседа и балл ранжирования, а НЕ вероятность правильного
// ответа.
//
// БЮДЖЕТ ЭТОГО КЛАССА. Ровно одно обучение на восьми уроках (один вложенный
// подбор словаря кусочков слов и одно построение указателя соседей). Главная
// проверка: 36 обращений к обученной памяти (12 + 12 + 12), 24 обращения к
// выбору навыка, 8 попыток загрузить тело назначенного навыка. Блок допуска
// полного каталога: 4 обращения к обученной памяти и 19 обращений к выбору
// навыка. Структурная проверка договора сигнала: 0 обращений к обученной памяти
// — она не является новым обучением.

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentAlgorithms.Skills;
using AgentInfrastructure.Logical;

// Одна строка панели одного считывания: опознание, вид, вход и эталонное имя навыка.
internal sealed record LearnedSkillPanelRow(string RowId, string Kind, string Input, string GoldSkillName);

// Замороженное тело навыка: имя продуктового навыка и его порядок шагов.
// Порядок шагов — данные этого покрытия, а не продуктовый каталог: у
// продуктового описания навыка шагов нет.
internal sealed record FrozenSkillBody(string SkillName, string[] Steps);

// Общая обученная память на весь класс: обучение ровно одно.
public sealed class LearnedSkillSelectionFixture
{
    // Предметная тема обучения: оркестрация порядка работы с кодом.
    internal const string DomainTag = "orchestration-route";

    // Опознание обученной модели и её роль в составе логических единиц.
    internal const string UnitId = "p3201-u-or-route-unit";
    internal const string UnitRole = "skill-route-selector";

    // Два назначенных навыка — реальные имена продуктового каталога.
    // Маршрут «сначала проверка» и маршрут «сначала изменение».
    internal const string ReviewSkillName = "generated-code-review";
    internal const string BuildSkillName = "dna-compliance-guard";

    // Замороженные тела назначенных навыков: порядок шагов противоположен.
    internal static readonly FrozenSkillBody ReviewBody = new(ReviewSkillName, ["review", "implement"]);
    internal static readonly FrozenSkillBody BuildBody = new(BuildSkillName, ["implement", "review"]);

    // Восемь уроков набора «U-OR», дословно из исходника опыта P32
    // (experiments-v2/RP01-MICRO/P3201/Program.cs, строки 85-95); имена навыков
    // здесь продуктовые, потому что сигнал разрешён только для существующего навыка.
    internal static readonly (string Id, string Input, string SkillName)[] Lessons =
    [
        ("u1", "review code then implement change", ReviewSkillName),
        ("u2", "inspect code then implement change", ReviewSkillName),
        ("u3", "review module before editing it", ReviewSkillName),
        ("u4", "inspect module before editing it", ReviewSkillName),
        ("u5", "implement change then review code", BuildSkillName),
        ("u6", "implement change then inspect code", BuildSkillName),
        ("u7", "edit module before reviewing it", BuildSkillName),
        ("u8", "edit module before inspecting it", BuildSkillName)
    ];

    // Панель одного считывания: восемь контрольных входов обучения и четыре
    // экзаменационные строки, дословно из того же исходника (строки 99-105).
    internal static readonly LearnedSkillPanelRow[] Panel =
    [
        new("t-u1", "TRAIN_CONTROL", "review code then implement change", ReviewSkillName),
        new("t-u2", "TRAIN_CONTROL", "inspect code then implement change", ReviewSkillName),
        new("t-u3", "TRAIN_CONTROL", "review module before editing it", ReviewSkillName),
        new("t-u4", "TRAIN_CONTROL", "inspect module before editing it", ReviewSkillName),
        new("t-u5", "TRAIN_CONTROL", "implement change then review code", BuildSkillName),
        new("t-u6", "TRAIN_CONTROL", "implement change then inspect code", BuildSkillName),
        new("t-u7", "TRAIN_CONTROL", "edit module before reviewing it", BuildSkillName),
        new("t-u8", "TRAIN_CONTROL", "edit module before inspecting it", BuildSkillName),
        new("e1", "EXAM", "review code before implementing change", ReviewSkillName),
        new("e2", "EXAM", "inspect code before implementing change", ReviewSkillName),
        new("e3", "EXAM", "implement change before reviewing code", BuildSkillName),
        new("e4", "EXAM", "implement change before inspecting code", BuildSkillName)
    ];

    // Обученная память, общая для всех проверок класса.
    internal KnnLogicalUnit<string, string> Unit { get; }

    // Выбранная конфигурация: та модель и та тема, для которых сигнал законен.
    internal SkillRouteConfiguration Configuration { get; }

    // Одно обучение на весь класс: восемь уроков одной партией.
    public LearnedSkillSelectionFixture()
    {
        // Ответ единицы — имя навыка. Строка неизменяема, поэтому память владеет
        // ответом без отдельной политики копирования.
        var examples = Lessons
            .Select(lesson => new LogicalKnnTrainingExample<string, string>(
                lesson.Id,
                lesson.Input,
                lesson.SkillName,
                [],
                [DomainTag],
                ["U-OR:" + lesson.Id]))
            .ToList();

        // Настройки обучения взяты из опыта P32: размерность 64, один сосед, порог 0.
        var trained = KnnLogicalUnit<string, string>.Train(
            UnitId,
            UnitRole,
            DomainTag,
            examples,
            input => input,
            vectorDimensions: 64,
            k: 1,
            lessonRequestConfidenceThreshold: 0.0);

        // Отказ обучения останавливает класс здесь же, а не притворяется памятью.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        Unit = trained.Value!;

        // Конфигурация берётся из описи обученной модели, а не выдумывается.
        Configuration = new SkillRouteConfiguration(Unit.Manifest.UnitId, Unit.Manifest.DomainTag);
    }

    // Загрузка замороженного тела по выбранному имени навыка. Отсутствие тела —
    // честный отказ, а не подстановка удобного навыка.
    internal static FrozenSkillBody? LoadBody(string skillName)
    {
        // Назначенный навык «сначала проверка».
        if (string.Equals(skillName, ReviewSkillName, StringComparison.Ordinal))
            return ReviewBody;

        // Назначенный навык «сначала изменение».
        if (string.Equals(skillName, BuildSkillName, StringComparison.Ordinal))
            return BuildBody;

        // Любое другое имя назначенного тела не имеет.
        return null;
    }

    // Настоящее исполнение шагов загруженного тела по порядку.
    internal static string[] InvokeBody(FrozenSkillBody body)
        => [.. body.Steps.Select(step => step + ":done")];
}

public sealed class LearnedSkillSelectionTests(LearnedSkillSelectionFixture fixture)
    : IClassFixture<LearnedSkillSelectionFixture>
{
    // Общая обученная память класса: подменять её внутри проверок нельзя.
    private readonly KnnLogicalUnit<string, string> _unit = fixture.Unit;

    // Выбранная модель и предметная тема этой обученной памяти.
    private readonly SkillRouteConfiguration _configuration = fixture.Configuration;

    // Порог уверенности продуктового выбора навыка, переписанный сюда как
    // ожидание договора, а не прочитанный из продукта.
    private const double ConfidenceThreshold = 0.40;

    // Главная проверка переноса: обученный сигнал меняет и выбранный навык, и
    // фактически загруженное тело с порядком шагов.
    //
    // Расход: 36 обращений к обученной памяти (12 + 12 + 12), 24 обращения к
    // выбору навыка (12 + 12), 8 попыток загрузить тело назначенного навыка.
    [Fact]
    public async Task LearnedRoute_ChangesSelectedAndLoadedSkill()
    {
        // Счётчики расхода ведутся честно и проверяются в конце.
        var memoryReads = 0;
        var retrieveCalls = 0;
        var bodyLoadAttempts = 0;
        var bodyExecutions = 0;
        var bodyRefusals = 0;

        var retriever = new SkillRetriever();

        // ── Предусловие измерения: одного детерминированного основания не хватает ──
        // Если бы какой-то навык каталога сам достигал порога, разница между
        // плечами объяснялась бы не обученным сигналом. Это чтение каталога, а
        // не обращение к обученной памяти, поэтому в бюджет запросов не входит.
        foreach (var row in LearnedSkillSelectionFixture.Panel)
        {
            foreach (var trigger in SkillTriggerLibrary.All)
            {
                // ScoreForKnn возвращает ноль для всего, что ниже порога 0,40.
                Assert.Equal(0.0, retriever.ScoreForKnn(row.Input, trigger.Condition));
            }
        }

        // ── Состояние сразу после обучения: 12 обращений к обученной памяти ──
        var s0CorrectTrain = 0;
        var s0CorrectExam = 0;
        foreach (var row in LearnedSkillSelectionFixture.Panel)
        {
            var answer = await AskAsync(row.Input, "S0-" + row.RowId);
            memoryReads++;

            // Чтение обязано пройти через общий конверт результата.
            Assert.True(answer.IsSuccess, answer.ErrorMessage);

            // Эталон выписан из панели, а не прочитан из ответа.
            if (string.Equals(answer.Value!.Output, row.GoldSkillName, StringComparison.Ordinal))
            {
                if (string.Equals(row.Kind, "TRAIN_CONTROL", StringComparison.Ordinal)) s0CorrectTrain++;
                if (string.Equals(row.Kind, "EXAM", StringComparison.Ordinal)) s0CorrectExam++;
            }
        }

        // Приобретённое знание ненулевое: восемь точных возвратов и четыре переноса.
        Assert.Equal(8, s0CorrectTrain);
        Assert.Equal(4, s0CorrectExam);

        // ── Плечо A: прежний порядок, обученный сигнал вычислен и не подключён ──
        var armASelected = new List<string[]>();
        var armAFallbacks = 0;
        foreach (var row in LearnedSkillSelectionFixture.Panel)
        {
            // Стоимость чтения памяти оплачена в обоих плечах, чтобы единственным
            // отличием осталось подключение сигнала к выбору.
            var answer = await AskAsync(row.Input, "A-" + row.RowId);
            memoryReads++;
            Assert.True(answer.IsSuccess, answer.ErrorMessage);

            // Сигнал в выбор не передаётся: это прежнее поведение продукта.
            var retrieval = retriever.Retrieve(row.Input);
            retrieveCalls++;

            var names = retrieval.Skills.Select(skill => skill.SkillName).ToArray();
            armASelected.Add(names);
            if (retrieval.IsFallback) armAFallbacks++;

            // Назначенный навык прежним порядком не выбирается ни на одной строке.
            Assert.DoesNotContain(LearnedSkillSelectionFixture.ReviewSkillName, names);
            Assert.DoesNotContain(LearnedSkillSelectionFixture.BuildSkillName, names);

            // На экзаменационных строках делается настоящая попытка загрузить тело.
            if (string.Equals(row.Kind, "EXAM", StringComparison.Ordinal))
            {
                bodyLoadAttempts++;
                var body = LearnedSkillSelectionFixture.LoadBody(names[0]);

                // Тела назначенного навыка нет: это честный отказ загрузки.
                Assert.Null(body);
                bodyRefusals++;
            }
        }

        // Прежний порядок на всех двенадцати строках уходит в откат к обязательным навыкам.
        Assert.Equal(12, armAFallbacks);

        // ── Плечо B: тот же сигнал подключён ДО выбора и порога ──
        var armBSelected = new List<string[]>();
        var armBCorrect = 0;
        var armBExamExecuted = 0;
        foreach (var row in LearnedSkillSelectionFixture.Panel)
        {
            var answer = await AskAsync(row.Input, "B-" + row.RowId);
            memoryReads++;
            Assert.True(answer.IsSuccess, answer.ErrorMessage);

            // Сигнал построен из ОТВЕТА обученной памяти, а не из эталона панели.
            var signal = new LearnedSkillRouteSignal(
                answer.Value!.Output,
                answer.Value!.Confidence,
                _configuration.ModelId,
                _configuration.DomainId);

            // Второго скрытого обращения к модели внутри выбора нет: сигнал уже вычислен.
            var retrieval = retriever.Retrieve(row.Input, null, signal, _configuration);
            retrieveCalls++;

            var names = retrieval.Skills.Select(skill => skill.SkillName).ToArray();
            armBSelected.Add(names);

            // Обученное слагаемое перевело выбор через порог: отката больше нет.
            Assert.False(retrieval.IsFallback);

            // Названный обученной памятью навык действительно попал в выбор.
            Assert.Contains(answer.Value!.Output, names);

            // Правильность считается по эталону панели, а не по ответу памяти.
            if (names.Contains(row.GoldSkillName, StringComparer.Ordinal)) armBCorrect++;

            // На экзаменационных строках тело выбранного навыка загружается и исполняется.
            if (string.Equals(row.Kind, "EXAM", StringComparison.Ordinal))
            {
                bodyLoadAttempts++;
                var body = LearnedSkillSelectionFixture.LoadBody(answer.Value!.Output);

                // Тело назначенного навыка найдено в замороженном каталоге.
                Assert.NotNull(body);

                // Порядок шагов исполняется фактически, а не объявляется.
                var executed = LearnedSkillSelectionFixture.InvokeBody(body!);
                bodyExecutions++;
                armBExamExecuted++;

                // Ожидаемый порядок шагов выписан из эталона панели.
                var expectedBody = string.Equals(row.GoldSkillName, LearnedSkillSelectionFixture.ReviewSkillName, StringComparison.Ordinal)
                    ? LearnedSkillSelectionFixture.ReviewBody
                    : LearnedSkillSelectionFixture.BuildBody;
                Assert.Equal(expectedBody.Steps.Select(step => step + ":done").ToArray(), executed);
            }
        }

        // Все двенадцать строк выбрали эталонный навык.
        Assert.Equal(12, armBCorrect);

        // Четыре экзаменационные строки действительно исполнили шаги.
        Assert.Equal(4, armBExamExecuted);

        // ── Различающее условие: плечи расходятся на всех двенадцати строках ──
        var differingRows = 0;
        for (var index = 0; index < LearnedSkillSelectionFixture.Panel.Length; index++)
        {
            if (!armASelected[index].SequenceEqual(armBSelected[index], StringComparer.Ordinal))
                differingRows++;
        }

        // Единственный изменённый фактор — подключение обученного сигнала.
        Assert.Equal(12, differingRows);

        // ── Фактический расход совпадает с назначенным ──
        Assert.Equal(36, memoryReads);
        Assert.Equal(24, retrieveCalls);
        Assert.Equal(8, bodyLoadAttempts);
        Assert.Equal(4, bodyExecutions);
        Assert.Equal(4, bodyRefusals);
    }

    // Отдельный блок допуска полного каталога: тот же обученный экземпляр, четыре
    // экзаменационные строки и четыре явно названных режима, затем три численные
    // границы порога.
    //
    // Расход: 4 обращения к обученной памяти (по одному на экзаменационную строку,
    // дальше сигналы переиспользуются как данные) и 19 обращений к выбору навыка
    // (16 режимных + 3 граничных). Эти 19 обращений НЕ доказывают качества всех
    // 48 навыков каталога: они проверяют композиционные инварианты.
    [Fact]
    public async Task FullCatalogueAdmission_FourNamedModes_AndThreeNumericBoundaries()
    {
        var memoryReads = 0;
        var retrieveCalls = 0;
        var retriever = new SkillRetriever();

        // Обязательные навыки каталога: они обязаны присутствовать в каждом выданном списке.
        var alwaysRequired = SkillTriggerLibrary.AlwaysRequired.Select(skill => skill.SkillName).ToArray();

        // Четыре экзаменационные строки панели.
        var examRows = LearnedSkillSelectionFixture.Panel
            .Where(row => string.Equals(row.Kind, "EXAM", StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(4, examRows.Length);

        // Обученные сигналы вычисляются ровно один раз на четырёх запросах.
        var signals = new List<LearnedSkillRouteSignal>();
        foreach (var row in examRows)
        {
            var answer = await AskAsync(row.Input, "ADM-" + row.RowId);
            memoryReads++;
            Assert.True(answer.IsSuccess, answer.ErrorMessage);
            signals.Add(new LearnedSkillRouteSignal(
                answer.Value!.Output,
                answer.Value!.Confidence,
                _configuration.ModelId,
                _configuration.DomainId));
        }

        // Полные списки выбранных навыков по каждому режиму — для отчёта и сверки.
        var noSignalSelections = new List<string[]>();
        var validSignalSelections = new List<string[]>();
        var belowThresholdSelections = new List<string[]>();
        var unknownSkillRefusals = 0;

        for (var index = 0; index < examRows.Length; index++)
        {
            var row = examRows[index];
            var signal = signals[index];

            // Режим 1: без сигнала. Прежнее поведение — откат к обязательным навыкам.
            var noSignal = retriever.Retrieve(row.Input);
            retrieveCalls++;
            Assert.True(noSignal.IsFallback);
            noSignalSelections.Add(noSignal.Skills.Select(skill => skill.SkillName).ToArray());

            // Режим 2: законный обученный сигнал. Выбор проходит порог и берёт названный навык.
            var valid = retriever.Retrieve(row.Input, null, signal, _configuration);
            retrieveCalls++;
            Assert.False(valid.IsFallback);
            var validNames = valid.Skills.Select(skill => skill.SkillName).ToArray();
            Assert.Contains(signal.SkillId, validNames);
            validSignalSelections.Add(validNames);

            // Режим 3: неизвестное имя навыка. Ожидается именно отказ, а не произвольный список.
            var unknown = signal with { SkillId = "skill-that-does-not-exist" };
            var refusal = Assert.Throws<ArgumentException>(() =>
                retriever.Retrieve(row.Input, null, unknown, _configuration));
            retrieveCalls++;
            Assert.Contains("skill-that-does-not-exist", refusal.Message, StringComparison.Ordinal);
            unknownSkillRefusals++;

            // Режим 4: законная, но подпороговая оценка. Карточка прямо
            // предупреждает, что подпороговый обученный сигнал НЕ обязан включать
            // откат, если другой реальный кандидат уже имеет достаточную базовую
            // меру, и этот существующий путь здесь действительно есть. Измерено на
            // этой сборке: названный навык проходит порог начиная с оценки 0,15 на
            // строках «e1» и «e2» (их базовая мера примерно 0,25) и только с 0,40 на
            // строках «e3» и «e4» (их базовая мера примерно нулевая). Поэтому для
            // режима «ниже порога» берётся 0,10 — оценка, которая ниже точки
            // продвижения у ВСЕХ четырёх строк. Прежнее ожидание этой проверки
            // (0,30 и обязательный откат) было неверным по карточке и исправлено
            // здесь, а не подгонкой продукта.
            var below = signal with { Score = 0.10 };
            var belowResult = retriever.Retrieve(row.Input, null, below, _configuration);
            retrieveCalls++;
            Assert.True(belowResult.IsFallback);
            belowThresholdSelections.Add(belowResult.Skills.Select(skill => skill.SkillName).ToArray());
        }

        // Все четыре строки дали отказ на неизвестном имени навыка.
        Assert.Equal(4, unknownSkillRefusals);

        // Обязательные навыки присутствуют в каждом выданном списке всех режимов.
        foreach (var selection in noSignalSelections.Concat(validSignalSelections).Concat(belowThresholdSelections))
        {
            foreach (var required in alwaysRequired)
                Assert.Contains(required, selection);
        }

        // Откат выдаёт ровно обязательные навыки, а законный сигнал — больше их.
        foreach (var selection in noSignalSelections)
            Assert.Equal(alwaysRequired.Length, selection.Length);
        foreach (var selection in validSignalSelections)
            Assert.True(selection.Length > alwaysRequired.Length);

        // ── Три численные границы порога на фиксированном каталоге ──
        // Нужна строка, у которой базовая мера названного навыка практически нулевая:
        // тогда ключ ранжирования равен самой оценке сигнала и граница 0,40
        // проверяется именно как граница. По измерению этой сборки такими являются
        // строки «e3» и «e4» — их названный навык проходит порог только с 0,40, то
        // есть его базовая мера лежит в пределах одной сотой от нуля. Берётся «e3».
        // Новых обращений к обученной модели здесь нет: оценки заданы прямо.
        var boundaryIndex = Array.FindIndex(examRows, row => string.Equals(row.RowId, "e3", StringComparison.Ordinal));
        Assert.True(boundaryIndex >= 0);
        var boundaryInput = examRows[boundaryIndex].Input;
        var boundarySkill = signals[boundaryIndex].SkillId;

        // Отдельного запроса на «базовая мера почти нулевая» не нужно: это
        // доказывает сама первая граница ниже. Если оценка 0,39 оставляет выбор в
        // откате, то базовая мера названного навыка строго меньше одной сотой,
        // иначе сумма достигла бы порога 0,40. Бюджет блока остаётся 19 запросов.

        // Ниже порога: 0,39 — откат.
        var justBelow = retriever.Retrieve(
            boundaryInput, null,
            new LearnedSkillRouteSignal(boundarySkill, 0.39, _configuration.ModelId, _configuration.DomainId),
            _configuration);
        retrieveCalls++;
        Assert.True(justBelow.IsFallback);

        // Ровно порог: 0,40 — порог не строгий сверху, выбор проходит.
        var atThreshold = retriever.Retrieve(
            boundaryInput, null,
            new LearnedSkillRouteSignal(boundarySkill, ConfidenceThreshold, _configuration.ModelId, _configuration.DomainId),
            _configuration);
        retrieveCalls++;
        Assert.False(atThreshold.IsFallback);
        Assert.Contains(boundarySkill, atThreshold.Skills.Select(skill => skill.SkillName));

        // Выше порога: 0,41 — выбор проходит.
        var justAbove = retriever.Retrieve(
            boundaryInput, null,
            new LearnedSkillRouteSignal(boundarySkill, 0.41, _configuration.ModelId, _configuration.DomainId),
            _configuration);
        retrieveCalls++;
        Assert.False(justAbove.IsFallback);

        // Фактический расход совпадает с назначенным.
        Assert.Equal(4, memoryReads);
        Assert.Equal(19, retrieveCalls);
    }

    // Структурная проверка договора сигнала. Обращений к обученной памяти — ноль,
    // поэтому новым обучением она не является.
    [Fact]
    public void InvalidLearnedSignal_IsRefusedBeforeSkillList()
    {
        var retriever = new SkillRetriever();
        const string input = "review code before implementing change";
        var validSkill = LearnedSkillSelectionFixture.ReviewSkillName;

        // Отсутствующий сигнал сохраняет исходное поведение полностью.
        var withoutSignal = retriever.Retrieve(input);
        var withNullSignal = retriever.Retrieve(input, null, null, _configuration);
        Assert.Equal(
            withoutSignal.Skills.Select(skill => skill.SkillName),
            withNullSignal.Skills.Select(skill => skill.SkillName));
        Assert.Equal(withoutSignal.IsFallback, withNullSignal.IsFallback);

        // Нулевая оценка законна и прежнего поведения не меняет.
        var zeroSignal = new LearnedSkillRouteSignal(validSkill, 0.0, _configuration.ModelId, _configuration.DomainId);
        var withZero = retriever.Retrieve(input, null, zeroSignal, _configuration);
        Assert.Equal(
            withoutSignal.Skills.Select(skill => skill.SkillName),
            withZero.Skills.Select(skill => skill.SkillName));
        Assert.Equal(withoutSignal.IsFallback, withZero.IsFallback);

        // Бессмысленная оценка — ошибка диапазона аргумента, а не максимальный приоритет.
        foreach (var badScore in new[] { double.NaN, double.PositiveInfinity, 1.5, -0.1 })
        {
            var bad = new LearnedSkillRouteSignal(validSkill, badScore, _configuration.ModelId, _configuration.DomainId);
            Assert.Throws<ArgumentOutOfRangeException>(() => retriever.Retrieve(input, null, bad, _configuration));
        }

        // Неизвестное имя навыка отвергается до выдачи списка навыков.
        var unknownSkill = new LearnedSkillRouteSignal("no-such-skill", 0.9, _configuration.ModelId, _configuration.DomainId);
        Assert.Throws<ArgumentException>(() => retriever.Retrieve(input, null, unknownSkill, _configuration));

        // Чужая модель отвергается.
        var foreignModel = new LearnedSkillRouteSignal(validSkill, 0.9, "other-model", _configuration.DomainId);
        Assert.Throws<ArgumentException>(() => retriever.Retrieve(input, null, foreignModel, _configuration));

        // Чужая предметная тема отвергается.
        var foreignDomain = new LearnedSkillRouteSignal(validSkill, 0.9, _configuration.ModelId, "other-domain");
        Assert.Throws<ArgumentException>(() => retriever.Retrieve(input, null, foreignDomain, _configuration));

        // Сигнал без выбранной конфигурации привязать не к чему.
        var orphan = new LearnedSkillRouteSignal(validSkill, 0.9, _configuration.ModelId, _configuration.DomainId);
        Assert.Throws<ArgumentException>(() => retriever.Retrieve(input, null, orphan, null));

        // Привязка для обычного потребителя отказывает типизированно, без исключения.
        Assert.Equal(
            LearnedSkillRouteSignalBinder.ScoreOutOfRangeCode,
            LearnedSkillRouteSignalBinder.Bind(
                new LearnedSkillRouteSignal(validSkill, 2.0, _configuration.ModelId, _configuration.DomainId),
                _configuration).ErrorCode);
        Assert.Equal(
            LearnedSkillRouteSignalBinder.UnknownSkillCode,
            LearnedSkillRouteSignalBinder.Bind(unknownSkill, _configuration).ErrorCode);
        Assert.Equal(
            LearnedSkillRouteSignalBinder.ConfigurationMismatchCode,
            LearnedSkillRouteSignalBinder.Bind(foreignModel, _configuration).ErrorCode);
        Assert.Equal(
            LearnedSkillRouteSignalBinder.ConfigurationMissingCode,
            LearnedSkillRouteSignalBinder.Bind(orphan, null).ErrorCode);

        // Отсутствующий сигнал — законный случай, а не отказ.
        var noSignalBinding = LearnedSkillRouteSignalBinder.Bind(null, _configuration);
        Assert.True(noSignalBinding.IsSuccess);
        Assert.Null(noSignalBinding.Value);
    }

    // Обычный потребитель маршрута навыка: тот самый путь, которым пользуется узел
    // продукта. Обращений к обученной памяти — ноль: сигнал подаётся как данные.
    [Fact]
    public void OrdinaryConsumer_AppliesAcceptedSignal_AndFormatsRefusal()
    {
        var retriever = new SkillRetriever();
        const string input = "review code before implementing change";
        var validSkill = LearnedSkillSelectionFixture.ReviewSkillName;

        // Без сведений запроса обученного слагаемого нет: дефолтный путь не меняется.
        var byDefault = LearnedSkillRouteConsumer.Select(retriever, input, null);
        Assert.False(byDefault.LearnedSignalApplied);
        Assert.Equal(string.Empty, byDefault.RefusalCode);
        Assert.True(byDefault.IsFallback);

        // Законный сигнал в сведениях запроса доходит до выбора и применяется.
        var accepted = LearnedSkillRouteConsumer.Select(retriever, input, new Dictionary<string, object>
        {
            [LearnedSkillRouteSignalBinder.SignalMetadataKey] =
                new LearnedSkillRouteSignal(validSkill, 0.83, _configuration.ModelId, _configuration.DomainId),
            [LearnedSkillRouteSignalBinder.ConfigurationMetadataKey] = _configuration
        });
        Assert.True(accepted.LearnedSignalApplied);
        Assert.Equal(string.Empty, accepted.RefusalCode);
        Assert.False(accepted.IsFallback);
        Assert.Contains(validSkill, accepted.Skills.Select(skill => skill.SkillName));

        // Чужая тема: потребитель ОФОРМЛЯЕТ отказ и не объявляет откат обучением.
        var refused = LearnedSkillRouteConsumer.Select(retriever, input, new Dictionary<string, object>
        {
            [LearnedSkillRouteSignalBinder.SignalMetadataKey] =
                new LearnedSkillRouteSignal(validSkill, 0.83, _configuration.ModelId, "other-domain"),
            [LearnedSkillRouteSignalBinder.ConfigurationMetadataKey] = _configuration
        });
        Assert.False(refused.LearnedSignalApplied);
        Assert.Equal(LearnedSkillRouteSignalBinder.ConfigurationMismatchCode, refused.RefusalCode);
        Assert.True(refused.IsFallback);
        Assert.DoesNotContain(validSkill, refused.Skills.Select(skill => skill.SkillName));

        // Сигнал без конфигурации тоже оформляется как отказ, а не как молчаливый откат.
        var orphan = LearnedSkillRouteConsumer.Select(retriever, input, new Dictionary<string, object>
        {
            [LearnedSkillRouteSignalBinder.SignalMetadataKey] =
                new LearnedSkillRouteSignal(validSkill, 0.83, _configuration.ModelId, _configuration.DomainId)
        });
        Assert.False(orphan.LearnedSignalApplied);
        Assert.Equal(LearnedSkillRouteSignalBinder.ConfigurationMissingCode, orphan.RefusalCode);
        Assert.True(orphan.IsFallback);
    }

    // Один запрос к обученной памяти обычным открытым входом логической единицы.
    private async Task<AgentInfrastructure.OperationResult<LogicalUnitResult<string>>> AskAsync(string input, string runId)
    {
        // Запрос собирается тем же договором, которым пользуется продукт.
        var request = new LogicalUnitRequest<string>(
            runId,
            LearnedSkillSelectionFixture.UnitId,
            LearnedSkillSelectionFixture.UnitRole,
            LearnedSkillSelectionFixture.DomainTag,
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

        // Чтение обученного состояния выполняется штатным открытым входом.
        return await _unit.ExecuteAsync(request);
    }
}
