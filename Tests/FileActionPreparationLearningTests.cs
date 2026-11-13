// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Diagnostics;
using SemanticCompiler;
using LlmFromScratch.Tests.ExperimentFixes;

// ПОСТОЯННАЯ ПРОВЕРКА подготовки уроков распознавателя действия над файлом
// (строка очереди переносов 40, идентификатор работы FIX-KHC, карточка о едином
// разборе для признака и действия над файлом).
//
// ЧТО ИМЕННО ЗДЕСЬ ДОКАЗЫВАЕТСЯ, человеческими словами. Перенос собирается
// разбирать исходный текст урока ОДИН раз вместо двух и передавать одно и то же
// разобранное дерево и в вычисление признаков, и в вычисление метки. Заявка
// переноса — не рост точности, а та же работа дешевле. Поэтому различающее
// свойство ровно одно: единый разбор обязан дать ТОТ ЖЕ признак и ТУ ЖЕ метку,
// что давали два разбора, и признак обязан остаться в паре со СВОЕЙ меткой.
// Именно это и проверяется ниже — по обученной таблице, а не по ответам
// экзамена.
//
// ОТКУДА ВЗЯТЫ ОЖИДАЕМЫЕ ЗНАЧЕНИЯ. Из сохранённого наблюдения опыта: файл
// набора `historical-prepared.json` содержит шесть подготовленных строк с их
// метками и векторами признаков, выписанных из поля `prepared` исходного
// наблюдения. Ни одно ожидаемое значение здесь не вычисляется продуктовым
// разметчиком: иначе проверка спрашивала бы у подсудимого его же приговор.
// Отпечатки файлов набора сверяются с паспортом при каждом чтении.
//
// ПОЧЕМУ ЗДЕСЬ НЕТ ЭКЗАМЕНА ТОЧНОСТИ ЧЕРЕЗ ОБЫЧНУЮ КОМПИЛЯЦИЮ, и это
// сознательно. Две причины, и вторая сильнее первой.
// Первая: две экзаменационные строки из четырёх до разметчика не доходят вообще
// — обычная компиляция отбраковывает их раньше предсказания отказом
// «No predicates extracted» (текст отказа захвачен у самого продукта и лежит в
// уликах работы). Вторая: две достижимые строки несут ОДНУ И ТУ ЖЕ ожидаемую
// метку `ADD_METHOD`, поэтому экзамен по ним пустой — разметчик, всегда
// отвечающий `ADD_METHOD`, прошёл бы его целиком. Пустой экзамен ничего не
// доказывает, и утверждать по нему точность нельзя. Все четыре строки набора
// при этом остаются на месте нетронутыми: недостижимые доложены как
// недостижимые, а не удалены.
//
// ПОЧЕМУ ЭТО НЕ ОБХОД ПОТРЕБИТЕЛЯ. Производственного потребителя у файловой
// метки сейчас нет вообще: единственный путь к ней — фасад проекта 12, и из
// продукта его никто не вызывает ни созданием объекта, ни через интерфейс.
// Разбор с адресами лежит в уликах работы. Поэтому обученная таблица — это не
// обходной путь вместо настоящего потребителя, а единственное место, где
// различающее свойство переноса наблюдаемо.
//
// ЧТО ЭТА ПРОВЕРКА НЕ ДЕЛАЕТ. Не утверждает точность. Не утверждает процент
// экономии: расход записывается в улики как наблюдение одного прогона. Не
// является доказательством допуска переноса в продукт.
public sealed class FileActionPreparationLearningTests
{
    // Допуск сравнения признаков: исторические числа прошли через запись в текст
    // и обратно, поэтому побитовое сравнение здесь было бы придиркой к формату
    // записи, а не к продукту. Допуск узкий: он ловит любую подмену источника
    // признаков, потому что у разных уроков признаки различаются заметно.
    private const double FeatureTolerance = 1e-6;

    [Fact]
    public void SharedParse_PreservesPreparedPairsThroughFacade()
    {
        // Набор данных читается с диска и сверяется с паспортом по отпечаткам.
        var fixture = FileActionPreparationFixture.Load();

        // Шесть учебных строк: столько назначила карточка.
        Assert.Equal(6, fixture.Train.Count);

        // Шесть исторических подготовленных строк: по одной на каждый урок.
        Assert.Equal(fixture.Train.Count, fixture.Historical.Count);

        // Настройки фасада фиксированы: распознаватель действия включён, а
        // необязательные соседи выключены, чтобы они не влияли на измерение.
        var options = new SemanticCompilerOptions
        {
            UseFileActionKnn = true,
            UseRegexPreprocessor = false,
            UseMethodKnn = false,
            UseStructuralKnn = false
        };

        // Настоящий продуктовый фасад, а не копия алгоритма.
        var facade = new SemanticCompilerFacade(options: options);

        // До обучения таблица обязана быть пустой.
        Assert.Empty(FileActionPreparationFixture.TrainedRows(facade));

        // Исходные тексты уроков в порядке набора: именно их увидит обучение.
        var trainingSources = fixture.Train.Select(row => row.Source).ToArray();

        // Счётчик выделенных байтов ЭТОГО потока на начало измеряемого окна.
        // Поток, а не процесс: проверки проекта идут параллельно, и счётчик
        // всего процесса ловил бы выделения соседей.
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        // Время процессора этого процесса на начало окна.
        var cpuBefore = Process.GetCurrentProcess().TotalProcessorTime;

        // Метка времени начала окна.
        var startedAt = Stopwatch.GetTimestamp();

        // ИЗМЕРЯЕМОЕ ОКНО: ровно один вызов подготовки и обучения.
        facade.TrainFileActionKnn(trainingSources);

        // Метка времени конца окна.
        var finishedAt = Stopwatch.GetTimestamp();

        // Время процессора на конец окна.
        var cpuAfter = Process.GetCurrentProcess().TotalProcessorTime;

        // Счётчик выделенных байтов этого же потока на конец окна.
        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

        // Обученная таблица читается целиком: пары «признаки и метка».
        var rows = FileActionPreparationFixture.TrainedRows(facade);

        // Расход окна в байтах.
        var allocatedInWindow = allocatedAfter - allocatedBefore;

        // Длительность окна в миллисекундах.
        var wallMs = (finishedAt - startedAt) * 1000.0 / Stopwatch.Frequency;

        // Время процессора окна в миллисекундах.
        var cpuMs = (cpuAfter - cpuBefore).TotalMilliseconds;

        // Наибольшее расхождение признака по всей таблице, для доказательства.
        var worstFeatureGap = 0.0;

        // Накопитель прочитанных меток в порядке обучения.
        var readLabels = new List<int>();

        // Сравнение идёт по строкам в порядке обучения, пока он совпадает.
        for (var index = 0; index < Math.Min(rows.Count, fixture.Historical.Count); index++)
        {
            // Метка, прочитанная из обученной таблицы.
            readLabels.Add(rows[index].Label);

            // Признаки той же строки обученной таблицы.
            var actual = rows[index].Features;

            // Исторические признаки того же урока.
            var expected = fixture.Historical[index].Features;

            // Пока длины не совпали, поэлементное сравнение бессмысленно.
            if (actual.Length != expected.Length)
                continue;

            // Наибольшее расхождение по элементам этой строки.
            for (var position = 0; position < expected.Length; position++)
                worstFeatureGap = Math.Max(worstFeatureGap, Math.Abs(actual[position] - expected[position]));
        }

        // Доказательство пишется ДО утверждений, чтобы красный прогон тоже
        // оставлял измеренный след, а не обрывал измерение.
        WriteObservation(fixture, rows, readLabels, worstFeatureGap, allocatedInWindow, wallMs, cpuMs);

        // ЗАМЕЧАНИЯ НАКАПЛИВАЮТСЯ, а не обрывают проверку на первом же.
        //
        // Почему так, и это не стиль. Если проверка падает на первом непрошедшем
        // условии, то все условия после него становятся НЕнаблюдаемыми, и при
        // мутации нельзя сказать, устояли они или нет. Именно на этом я один раз
        // и ошибся: записал признаки как устоявшие, хотя прогон до них не дошёл.
        // Теперь все условия вычисляются до конца, замечания собираются и
        // предъявляются одним отказом с перечислением — тогда при любой будущей
        // мутации видно каждое условие, а не первое.
        var problems = new List<string>();

        // В таблице обязано лежать ровно шесть обученных строк.
        if (rows.Count != fixture.ExpectedTrainingSizeAfterFit)
            problems.Add(
                $"строк в обученной таблице {rows.Count}, ожидалось {fixture.ExpectedTrainingSizeAfterFit}");

        // Каждая строка обязана нести вектор признаков назначенной длины.
        for (var index = 0; index < rows.Count; index++)
            if (rows[index].Features.Length != fixture.FeatureVectorLength)
                problems.Add(
                    $"строка {index}: длина вектора признаков {rows[index].Features.Length}, " +
                    $"ожидалось {fixture.FeatureVectorLength}");

        // Метки обязаны совпасть с историческими по порядку и поштучно: это и
        // есть сохранность пары «признаки и метка одного урока» со стороны метки.
        for (var index = 0; index < Math.Min(rows.Count, fixture.Historical.Count); index++)
            if (rows[index].Label != fixture.Historical[index].Label)
                problems.Add(
                    $"урок {fixture.Historical[index].Id}: метка {rows[index].Label}, " +
                    $"ожидалась {fixture.Historical[index].Label}");

        // Признаки обязаны совпасть с историческими в пределах узкого допуска:
        // это та же сохранность пары со стороны признаков. Условие вычисляется
        // ДАЖЕ если метки разошлись, иначе его исход остался бы неизвестным.
        if (worstFeatureGap > FeatureTolerance)
            problems.Add(
                $"наибольшее расхождение признака {worstFeatureGap} превысило допуск {FeatureTolerance}");

        // Один отказ с перечислением всех замечаний сразу.
        Assert.True(
            problems.Count == 0,
            "нарушены условия сохранности пары «признаки и метка»: "
            + string.Join(" | ", problems));
    }

    // Записывает наблюдение рядом с попыткой этой работы.
    private static void WriteObservation(
        FileActionPreparationFixture.Loaded fixture,
        IReadOnlyList<(float[] Features, int Label)> rows,
        IReadOnlyList<int> readLabels,
        double worstFeatureGap,
        long allocatedInWindow,
        double wallMs,
        double cpuMs)
    {
        // Каталог попытки этой работы; проверка его не придумывает.
        var attempt = Path.Combine(
            FileActionPreparationFixture.RepositoryRoot(),
            "artifacts", "fbe", "FIX-KHC", "attempt-0001");

        // Без каталога писать некуда, и это не причина падать.
        if (!Directory.Exists(attempt))
            return;

        // Человеческое доказательство: что получилось и чем это сравнивалось.
        var lines = new List<string>
        {
            "# Подготовка уроков распознавателя действия над файлом: пары и расход",
            "",
            "Строка очереди переносов 40, идентификатор работы FIX-KHC.",
            "Ожидаемые значения взяты из сохранённого наблюдения опыта, а не",
            "вычислены продуктовым разметчиком.",
            "",
            $"- строк в обученной таблице: {rows.Count}",
            $"- ожидалось строк: {fixture.ExpectedTrainingSizeAfterFit}",
            $"- длина вектора признаков, ожидаемая: {fixture.FeatureVectorLength}",
            $"- прочитанные метки по порядку обучения: {string.Join(", ", readLabels)}",
            $"- исторические метки по порядку: {string.Join(", ", fixture.Historical.Select(row => row.Label))}",
            $"- наибольшее расхождение признака: {worstFeatureGap}",
            $"- допуск сравнения признаков: {FeatureTolerance}",
            $"- выделено за окно подготовки и обучения, байт: {allocatedInWindow}",
            $"- длительность окна, мс: {wallMs:F4}",
            $"- время процессора окна, мс: {cpuMs:F4}",
            $"- отпечаток паспорта набора данных: {fixture.ManifestSha256}",
            $"- разборов исходного текста по чтению кода до переноса: {fixture.ParseSitesBeforeTransfer}",
            "",
            "Числа расхода сняты одним прогоном и устойчивым процентом не",
            "объявляются. Исторические 0,765 процента экономии этой проверкой",
            "заново не подтверждаются.",
            "",
            "Экзамен точности здесь не проводится и не утверждается: две строки",
            "до разметчика не доходят, а две достижимые несут одну и ту же",
            "ожидаемую метку, поэтому экзамен по ним пустой.",
            ""
        };

        // Запись выполняется одним вызовом.
        File.WriteAllText(Path.Combine(attempt, "prepared-pairs-observation.md"), string.Join("\n", lines));
    }
}
