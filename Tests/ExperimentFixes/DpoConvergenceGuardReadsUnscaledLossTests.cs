// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка второй половины кандидата восемьдесят девятого опыта: какое поле
// квитанции НАСТОЯЩИЙ цикл обучения подаёт сторожу остановки.
//
// Зачем именно такая проверка. Сторож сравнивает величину с неподвижным
// порогом. Обычное поле потери в векторном пути домножается на плотность
// обучающего вектора на ветви точных пар, а награды, запас и доля верных — нет.
// Поэтому порог зависел от свойства данных, а не от хода обучения.
//
// Почему эта проверка не могла быть поведенческой. Оба существующих файла
// проверок достижимости остановки — `CapabilityTrainingDpoConvergenceReachabilityTests`
// и `DesignConversionDpoConvergenceReachabilityTests` — САМИ моделируют цикл со
// своим параметром плоской потери и зовут сторожа напрямую, через квитанцию они
// не идут вовсе. Значит какое поле передаёт настоящий цикл, не наблюдает ни одна
// существующая проверка: они проверяют поведение сторожа на подставленном
// числе, а не выбор поля на месте вызова. Эта проверка закрывает именно выбор
// поля, читая фактический текст обоих боевых циклов.
//
// Почему она не прошла бы на прежнем коде. На прежнем коде аргументом сторожа
// было само поле потери, а поля немасштабированной потери не существовало
// вовсе. Поэтому каждое утверждение ниже на прежнем коде падает.
//
// Область утверждений выбрана по измеренной правде, а не по удобству. Текстовые
// утверждения ограничены ВЕТВЬЮ НАБЛЮДЕНИЯ — тем условным оператором, внутри
// которого стоит вызов сторожа, — а не объемлющим методом. Причина измерена: в
// потребителе обучения способностей объемлющий метод содержит ещё и отчётного
// читателя `lastMetrics?.Loss ?? 0f`, который этот наряд намеренно не трогал.
// Запрет на чтение обычного поля в области метода дал бы ложное падение.
//
// Чего эта проверка НЕ доказывает. Она не запускает обучение и не измеряет
// качество: это приёмка выбора поля на месте вызова. Численная разница старого и
// нового поведения измерена отдельно дословным переносом обоих циклов
// (`Tests/ExperimentFixes/oracle-dpo-guard-reads-unscaled-loss.ps1`): на
// различающем наборе квитанций решение об остановке переворачивается в обе
// стороны — 5 шагов против 3 и 3 против 5.
//
// ГРАНИЦА СПЛОШНОГО СВИПА, НАЗВАННАЯ ПРЯМО. Утверждение
// `NoProductionCallSiteFeedsTheGuardAnyReceiptFieldNamedLoss` ищет среди
// аргументов вызова обращение к члену с именем обычного поля потери. Оно
// держит ПРЯМУЮ подачу поля и не держит подачу через промежуточную
// переменную: если будущий потребитель напишет `var x = metrics.Loss;` и
// подаст сторожу `x`, свип промолчит. Это тот же класс ошибки, который этот
// наряд встречал трижды за день: идиома законна, разбор молчит, ответ неверен.
// Свип намеренно НЕ переписан под цепочки присваиваний: отследить
// произвольную цепочку без семантической модели нельзя, а попытка сделать это
// по тексту даёт ложные падения, которые хуже отсутствующей проверки. Для двух
// известных потребителей дыра закрыта структурным утверждением
// `GuardArgumentIsTheCheckedValueOfTheUnscaledField`, которое читает, откуда
// локальная переменная получила значение. Для НОВОГО потребителя приёмкой
// является именно это структурное утверждение, и его надо на него
// распространить — добавив его путь к перечню вверху файла. Сплошной свип
// такую подмену сам не поймает.

namespace LlmFromScratch.Tests.ExperimentFixes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DpoConvergenceGuardReadsUnscaledLossTests
{
    // Оба боевых потребителя одного механизма остановки.
    private const string CapabilityTrainingGuard =
        "18.CapabilityTraining/LlmIngredientLogicalUnits.cs";
    private const string DesignConversionGuard =
        "44.DesignConversionDomain/DesignConversionDpoPolicyCheckpointBridge.cs";

    [Theory]
    [InlineData(CapabilityTrainingGuard)]
    [InlineData(DesignConversionGuard)]
    public void RealTrainingLoopFeedsTheGuardTheUnscaledLoss(string relativePath)
    {
        // Берётся фактический текст боевого файла, а не копия в проверке.
        var branch = ObservationBranch(relativePath);
        var text = branch.ToFullString();

        // 1. Величина, поданная сторожу, прочитана из поля немасштабированной
        //    потери. На прежнем коде этого поля не существовало вовсе.
        Assert.Contains("UnscaledLoss", text, StringComparison.Ordinal);

        // 2. В ветви наблюдения обычное поле потери больше не читается. Это и
        //    есть запрет прежнего поведения: на прежнем коде аргументом сторожа
        //    было именно оно.
        Assert.DoesNotContain(".Loss)", text, StringComparison.Ordinal);
        Assert.DoesNotContain(".Loss;", text, StringComparison.Ordinal);

        // 3. Пустое значение разведено с нулём: ветвь «значения нет» есть, и ни
        //    одного приведения отсутствия к числу в этой ветви нет.
        Assert.Contains("is null", text, StringComparison.Ordinal);
        Assert.DoesNotContain("?? 0", text, StringComparison.Ordinal);

        // 4. Решение при пустом значении названо вслух, а не молчит.
        Assert.Contains("TraceWarning", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(CapabilityTrainingGuard)]
    [InlineData(DesignConversionGuard)]
    public void GuardArgumentIsTheCheckedValueOfTheUnscaledField(string relativePath)
    {
        var branch = ObservationBranch(relativePath);

        // Единственный вызов сторожа в этой ветви.
        var invocation = Assert.Single(branch
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(IsGuardInvocation));

        // Аргумент ровно один: сторож сравнивает одну величину с порогом.
        var argument = Assert.Single(invocation.ArgumentList.Arguments).Expression;

        // Аргумент — проверенное значение локальной переменной, а не поле
        // квитанции напрямую: проверка пустого значения обязана стоять раньше
        // сравнения, иначе отсутствие величины снова стало бы числом.
        var access = Assert.IsType<MemberAccessExpressionSyntax>(argument);
        Assert.Equal("Value", access.Name.Identifier.ValueText);
        var localName = Assert.IsType<IdentifierNameSyntax>(access.Expression)
            .Identifier.ValueText;

        // Эта локальная переменная получает значение именно из поля
        // немасштабированной потери квитанции шага.
        var declarator = Assert.Single(branch
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Where(node => node.Identifier.ValueText == localName));
        var initializer = declarator.Initializer?.Value;
        Assert.NotNull(initializer);
        var initializerAccess = Assert.IsType<MemberAccessExpressionSyntax>(initializer!);
        Assert.Equal("UnscaledLoss", initializerAccess.Name.Identifier.ValueText);
    }

    [Fact]
    public void NoProductionCallSiteFeedsTheGuardAnyReceiptFieldNamedLoss()
    {
        // Перечень мест вызова собран по символу вызываемого члена, а не по
        // имени объемлющего метода: потребитель, названный иначе, иначе выпал бы.
        // Поэтому в перечень попадают и тонкая обёртка монитора, и неродственный
        // `ShouldStop` оркестратора — и это правильно: утверждение ниже должно
        // держать их всех, а не только два известных цикла.
        var root = FindRepositoryRoot();
        var offenders = new List<string>();
        foreach (var path in Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsOutsideProductionScope(root, path)))
        {
            var source = File.ReadAllText(path);
            if (!source.Contains(".ShouldStop(", StringComparison.Ordinal))
                continue;

            var tree = CSharpSyntaxTree.ParseText(source);
            foreach (var invocation in tree.GetRoot()
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(IsGuardInvocation))
            {
                // Запрещено подавать сторожу поле с именем `Loss` напрямую: это
                // ровно прежнее поведение, из-за которого неподвижный порог
                // зависел от плотности обучающего вектора.
                var feedsLossField = invocation.ArgumentList.Arguments
                    .Select(argument => argument.Expression)
                    .OfType<MemberAccessExpressionSyntax>()
                    .Any(access => access.Name.Identifier.ValueText == "Loss");
                if (feedsLossField)
                {
                    offenders.Add(
                        Path.GetRelativePath(root, path).Replace('\\', '/') +
                        ":" +
                        (invocation.GetLocation()
                            .GetLineSpan().StartLinePosition.Line + 1));
                }
            }
        }

        Assert.Empty(offenders);
    }

    // Ветвь наблюдения: тот условный оператор, внутри которого стоит вызов
    // сторожа и чтение поля квитанции. Область намеренно уже метода — см.
    // пояснение в шапке файла.
    private static IfStatementSyntax ObservationBranch(string relativePath)
    {
        var full = Path.Combine(
            FindRepositoryRoot(),
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(full));
        Assert.Empty(tree.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        // Ровно один вызов сторожа на файл: если их станет больше, проверка
        // упадёт здесь, а не тихо проверит не тот.
        var invocation = Assert.Single(tree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(IsGuardInvocation));

        // Берётся ближайший объемлющий условный оператор, который содержит и
        // чтение немасштабированного поля: это и есть вся ветвь наблюдения.
        return Assert.Single(invocation.Ancestors()
            .OfType<IfStatementSyntax>()
            .Where(statement => statement.ToFullString().Contains(
                "UnscaledLoss",
                StringComparison.Ordinal))
            .Take(1));
    }

    // Вызов сторожа опознаётся по имени вызываемого члена.
    private static bool IsGuardInvocation(InvocationExpressionSyntax invocation)
        => invocation.Expression is MemberAccessExpressionSyntax access &&
           access.Name.Identifier.ValueText == "ShouldStop";

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
            "DPO_CONVERGENCE_GUARD_TESTS_REPOSITORY_ROOT_NOT_FOUND");
    }
}
