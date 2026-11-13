// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка переноса отчётного читателя: отчёт обучения не превращает
// неизмеренную величину в ноль.
//
// Что было неверно. Отчёт `LogicalDpoTrainingResult` объявлял потерю и точность
// последнего шага обязательными числами (`float`), а место построения отчёта
// приводило отсутствие квитанции шага к нулю (`lastMetrics?.Loss ?? 0f`). Ноль
// в поле измеренной величины читается как измеренный факт. Для потери он врёт в
// ЛЕСТНУЮ сторону — нулевая потеря есть наилучший возможный исход обучения; для
// точности в ОБРАТНУЮ — нулевая точность читается как полный провал. Поэтому
// обе величины разведены вместе, одним решением: это одна работа, а не две.
//
// Почему эта приёмка состоит из двух разных утверждений, а не одного.
// Договор и место построения защищают разное, и ни одно из двух не покрывает
// другое:
//   (1) договорное утверждение проверяет, что отсутствие ВЫРАЗИМО: поля
//       объявлены необязательными, и отсутствие доходит до читателя
//       отсутствием. На прежнем договоре это утверждение падает, потому что
//       `float` не может быть пустым никогда;
//   (2) структурное утверждение проверяет, что отсутствие НЕ ПРИВОДИТСЯ к
//       числу в самом месте построения. Оно нужно потому, что измеренный факт
//       таков: выражение `lastMetrics?.Loss ?? 0f` при необязательном поле
//       `float?` компилируется совершенно законно. То есть сам по себе
//       необязательный договор повторного введения приведения НЕ запрещает, и
//       договорного утверждения для приёмки недостаточно.
//
// Почему приёмка не поведенческая по боевому пути, и это измерено, а не
// предположено. Отсутствие квитанции на пути построения отчёта сегодня
// НЕДОСТИЖИМО: счёт шагов проверяется диапазоном от одного до пяти
// (`LogicalDpoTrainingUnit.ValidateMaxStepCount`) до входа в цикл, единственное
// место вызова передаёт именно проверенное значение, цикл идёт от первого шага
// до счёта включительно и потому делает хотя бы один шаг, а тело шага либо
// присваивает квитанцию, либо возвращает отказ и до построения отчёта не
// доходит. Значит прогон обучения, у которого обе величины отсутствуют, через
// боевой путь воспроизвести нельзя, и проверка, которая пыталась бы это сделать,
// проверяла бы недостижимое состояние. Правка от этого не теряет смысла:
// устраняется не наблюдаемое сегодня число, а ложь, которую унаследовал бы
// первый будущий читатель — тот, кто разрешит нулевой бюджет шагов или сложит
// отчёт из частично прочитанных квитанций.
//
// Чего эта приёмка НЕ доказывает. Она не запускает обучение и не измеряет
// качество. Она не утверждает, что какое-либо сообщаемое сегодня число
// изменилось: при существующей квитанции отчёт несёт ту же величину, что и
// прежде. Утверждается ровно одно: отсутствие измерения больше не выдаётся за
// измеренный ноль.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using AgentInfrastructure.Logical;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DpoTrainingReportKeepsAbsentMeasurementAbsentTests
{
    // Боевое место построения отчёта обучения.
    private const string ReportBuilder =
        "18.CapabilityTraining/LlmIngredientLogicalUnits.cs";

    [Theory]
    [InlineData("LastLoss")]
    [InlineData("LastAccuracy")]
    public void ReportFieldCanExpressAnUnmeasuredValue(string fieldName)
    {
        // Берётся фактический тип продуктовой сборки, а не копия в проверке.
        var property = typeof(LogicalDpoTrainingResult).GetProperty(
            fieldName,
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);

        // Поле обязано быть необязательной одинарной точностью. На прежнем
        // договоре здесь стоял `float`, у которого состояния «не измерено» нет
        // вовсе, поэтому это утверждение падает на прежнем коде.
        Assert.Equal(typeof(float?), property!.PropertyType);
    }

    [Fact]
    public void AbsentMeasurementStaysAbsentInsteadOfBecomingZero()
    {
        // Отчёт, у которого обе величины не измерены. На прежнем договоре такой
        // отчёт нельзя было даже построить: обязательное число требовало
        // подставить ноль, и именно эта подстановка была дефектом.
        var report = new LogicalDpoTrainingResult(
            PreferenceCount: 1,
            DpoStepCount: 1,
            LoraAdapterCount: 1,
            LastLoss: null,
            LastAccuracy: null,
            TrainingTimeMs: 1,
            MetricFilePaths: [],
            IngredientIds: ["11.DPO:DpoTrainer"]);

        // Отсутствие доходит до читателя отсутствием.
        Assert.Null(report.LastLoss);
        Assert.Null(report.LastAccuracy);

        // И это не то же самое, что измеренный ноль: отчёт с настоящим нулём
        // отличим от отчёта без измерения. Если бы отсутствие приводилось к
        // нулю, эти два отчёта стали бы неразличимы — ровно та потеря
        // различимости, которую устраняет перенос.
        var measuredZero = report with { LastLoss = 0f, LastAccuracy = 0f };
        Assert.NotNull(measuredZero.LastLoss);
        Assert.NotNull(measuredZero.LastAccuracy);
        Assert.NotEqual(report, measuredZero);
    }

    [Theory]
    [InlineData("LastLoss")]
    [InlineData("LastAccuracy")]
    public void ReportBuilderReadsTheReceiptWithoutCoercingAbsenceToANumber(
        string fieldName)
    {
        // Аргумент отчёта берётся из фактического текста боевого файла.
        var argument = ReportArgumentFor(fieldName);

        // Аргумент обязан быть условным чтением квитанции и ничем больше:
        // `<квитанция>?.<поле>`. Любое слияние с числом здесь — это прежний
        // дефект, и оно обязано падать, хотя при необязательном поле
        // компилировалось бы законно.
        var access = Assert.IsType<ConditionalAccessExpressionSyntax>(argument);
        var binding = Assert.IsType<MemberBindingExpressionSyntax>(
            access.WhenNotNull);
        Assert.Equal(fieldName.Replace("Last", ""), binding.Name.Identifier.ValueText);

        // Ни одного слияния с пустым значением внутри этого аргумента.
        Assert.Empty(argument
            .DescendantNodesAndSelf()
            .OfType<BinaryExpressionSyntax>()
            .Where(node => node.IsKind(SyntaxKind.CoalesceExpression)));
    }

    [Fact]
    public void NoProductionReportConstructionCoercesTheseFieldsToANumber()
    {
        // Перечень мест построения собран по имени создаваемого типа, а не по
        // имени объемлющего метода: место, названное иначе, иначе выпало бы.
        var root = FindRepositoryRoot();
        var offenders = new List<string>();
        var inspected = 0;
        foreach (var path in Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsOutsideProductionScope(root, path)))
        {
            var source = File.ReadAllText(path);
            if (!source.Contains("new LogicalDpoTrainingResult(", StringComparison.Ordinal))
                continue;

            var tree = CSharpSyntaxTree.ParseText(source);
            foreach (var creation in tree.GetRoot()
                .DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>()
                .Where(IsReportCreation))
            {
                inspected++;
                // Потеря и точность последнего шага берутся тем же разбором,
                // что и выше: по имени, если аргумент назван, иначе по месту
                // объявления с проверкой, что место не занято чужим именем.
                // Слияние с числом в любом из них возвращает прежний дефект.
                var arguments = creation.ArgumentList?.Arguments ?? default;
                foreach (var fieldName in new[] { "LastLoss", "LastAccuracy" })
                {
                    var argument = ArgumentFor(arguments, fieldName);
                    if (argument is null)
                        continue;

                    var coercions = argument
                        .DescendantNodesAndSelf()
                        .OfType<BinaryExpressionSyntax>()
                        .Where(node => node.IsKind(SyntaxKind.CoalesceExpression));
                    if (coercions.Any())
                    {
                        offenders.Add(
                            Path.GetRelativePath(root, path).Replace('\\', '/') +
                            ":" +
                            (argument.GetLocation()
                                .GetLineSpan().StartLinePosition.Line + 1) +
                            " (" + fieldName + ")");
                    }
                }
            }
        }

        // Если место построения исчезло совсем, молчать нельзя: проверка должна
        // упасть громко, а не стать тихо бессодержательной.
        Assert.True(
            inspected > 0,
            "No production construction of the DPO training report was found.");
        Assert.Empty(offenders);
    }

    // Аргумент отчёта, стоящий на месте названного поля, из фактического текста.
    private static ExpressionSyntax ReportArgumentFor(string fieldName)
    {
        var full = Path.Combine(
            FindRepositoryRoot(),
            ReportBuilder.Replace('/', Path.DirectorySeparatorChar));
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(full));
        Assert.Empty(tree.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        // Ровно одно место построения отчёта на файл: появление второго уронит
        // проверку здесь, а не заставит её тихо проверить не то.
        var creation = Assert.Single(tree.GetRoot()
            .DescendantNodes()
            .OfType<ObjectCreationExpressionSyntax>()
            .Where(IsReportCreation));

        var argument = ArgumentFor(creation.ArgumentList!.Arguments, fieldName);
        Assert.NotNull(argument);
        return argument!;
    }

    // Аргумент, соответствующий названному полю записи. Сначала по ИМЕНИ, если
    // аргумент назван: тогда перестановка аргументов проверку не обманет. Если
    // имени нет — по месту объявления записи, но с явной сверкой, что это место
    // не занято чужим именем; иначе позиционный разбор читал бы не то поле и
    // проверка прошла бы по неверному аргументу, то есть молча проверила бы не
    // то. Это ровно тот класс ошибки, который в этой работе уже встречался:
    // идиома законна, разбор молчит, ответ неверен.
    private static ExpressionSyntax? ArgumentFor(
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        string fieldName)
    {
        foreach (var argument in arguments)
        {
            if (argument.NameColon?.Name.Identifier.ValueText == fieldName)
                return argument.Expression;
        }

        // Порядок объявления записи: счёт пар, счёт шагов, счёт переходников,
        // затем потеря и точность последнего шага.
        var index = fieldName switch
        {
            "LastLoss" => 3,
            "LastAccuracy" => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(fieldName))
        };
        if (arguments.Count <= index)
            return null;

        var occupant = arguments[index].NameColon?.Name.Identifier.ValueText;
        Assert.True(
            occupant is null || occupant == fieldName,
            $"Argument {index} is named '{occupant}', not '{fieldName}'.");
        return arguments[index].Expression;
    }

    // Построение отчёта опознаётся по имени создаваемого типа.
    private static bool IsReportCreation(ObjectCreationExpressionSyntax creation)
        => creation.Type is IdentifierNameSyntax name &&
           name.Identifier.ValueText == "LogicalDpoTrainingResult";

    // Опытные хозяева, их изолированные копии, проверки и каталоги сборки не
    // являются боевым кодом продукта.
    private static bool IsOutsideProductionScope(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        return relative.StartsWith("experiments-v2/", StringComparison.Ordinal) ||
               relative.StartsWith("35.ExperimentsV2/", StringComparison.Ordinal) ||
               relative.StartsWith("Tests/", StringComparison.Ordinal) ||
               relative.StartsWith("TestResults/", StringComparison.Ordinal) ||
               relative.Contains("/bin/", StringComparison.Ordinal) ||
               relative.Contains("/obj/", StringComparison.Ordinal);
    }

    // Поднимается от папки сборки до корня хранилища, чтобы пути не зависели
    // от места запуска. Тот же приём, что у соседних проверок этой папки.
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "DPO_TRAINING_REPORT_TESTS_REPOSITORY_ROOT_NOT_FOUND");
    }
}
