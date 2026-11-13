// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка переноса кандидата, измеренного независимо тремя опытами: порог
// уверенности обучаемой памяти соседей назван честно и НЕ используется как
// отказ.
//
// Что было неверно. Признак назывался `UseConfidenceFloor`, а порог —
// `ConfidenceFloor`. Оба читались как жёсткий минимум, которого обязан достичь
// ответ. На деле ниже порога единица возвращает ответ ближайшего соседа и лишь
// добавляет предупреждение «край знания», и это предупреждение — ВХОД УЧЕБНОГО
// ЦИКЛА: оркестратор заказывает по нему учебный пример и готовит учебные ветви.
//
// Поведение этим переносом НЕ изменено, и это главное. Изменены только имена,
// и потому поведенческого различения у приёмки нет по устройству. Вместо него
// приёмка держит ИНВАРИАНТ: сигнал нигде в боевом коде не используется как
// отказ. Такая проверка падает в тот день, когда кто-нибудь превратит его в
// гейт, — и это ровно то, что надо защитить.
//
// Почему гейтом его делать нельзя — два измеренных основания, раздельно.
//
// Первое, число. Девяносто третий опыт наблюдал четыре случая ниже порога:
// уверенности 0,6761, 0,6, 0,6, 0,6 при пороге 0,70. Все двенадцать ответов
// того прогона были ВЕРНЫМИ, значит все четыре предупреждения ложные, а точность
// сигнала — ноль из четырёх. Отказ на этом сигнале отверг бы четыре верных
// ответа из четырёх наблюдённых.
//
// Второе, разрыв обратной связи. Отказ лишил бы учебный цикл его входа:
// планировщик уроков (`AdaptiveLogicalOrchestrator`, заказ учебного примера по
// ветви с этим признаком) и отбор учебных ветвей перестали бы срабатывать ровно
// там, где обучение нужнее.
//
// Чего эта приёмка НЕ доказывает. Она не измеряет, сколько ответов изменится в
// работающем продукте: этого не измерил ни один из трёх опытов. Четыре случая —
// это четыре случая на одной панели, а не частота. Устранение дефекта договора
// улучшением качества не является и так не называется.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LowConfidenceLessonSignalContractTests
{
    // Единственное боевое место, где признак читается вместе с порогом.
    private const string SignalReader =
        "21.AgentInfrastructure/Logical/KnnLogicalUnit.cs";

    // Средство замера, чьё поле рецепта переименовывать нельзя.
    private const string ProbeProgram = "tools/Rp01MechanismProbe/Program.cs";

    // Записанный файл рецепта, с которым это поле связывается по имени.
    private const string RecordedRecipe =
        ".kiro/xiigen-slices/work/results/MICRO-KNN-ORDER-20261016-01/recipe.json";

    [Fact]
    public void HonestNameIsTheRealFlagAndTheOldNameOnlyForwardsToIt()
    {
        var type = typeof(LogicalExecutionFlags);

        // Настоящее имя говорит про заказ урока, а не про минимум.
        var honest = type.GetProperty(nameof(LogicalExecutionFlags.UseLowConfidenceLessonSignal));
        Assert.NotNull(honest);

        // Прежнее имя сохранено как псевдоним и помечено устаревшим: двадцать
        // замороженных хозяев опытов берут договор ссылкой и называют его.
#pragma warning disable CS0618
        var legacy = type.GetProperty(nameof(LogicalExecutionFlags.UseConfidenceFloor));
#pragma warning restore CS0618
        Assert.NotNull(legacy);
        Assert.NotNull(legacy!.GetCustomAttribute<ObsoleteAttribute>());

        // У псевдонима НЕТ своего хранилища, поэтому разойтись с настоящим
        // признаком он не может по устройству.
        var legacyBackingFields = type
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(field => field.Name.Contains("UseConfidenceFloor", StringComparison.Ordinal));
        Assert.Empty(legacyBackingFields);

        // Задал любое имя — читается одна и та же величина, в обе стороны.
#pragma warning disable CS0618
        var viaHonest = new LogicalExecutionFlags { UseLowConfidenceLessonSignal = false };
        var viaLegacy = new LogicalExecutionFlags { UseConfidenceFloor = false };
        Assert.False(viaHonest.UseConfidenceFloor);
        Assert.False(viaLegacy.UseLowConfidenceLessonSignal);

        // И выражение изменения работает через каждое из двух имён.
        var baseline = new LogicalExecutionFlags();
        Assert.False((baseline with { UseLowConfidenceLessonSignal = false }).UseConfidenceFloor);
        Assert.False((baseline with { UseConfidenceFloor = false }).UseLowConfidenceLessonSignal);
#pragma warning restore CS0618
    }

    [Fact]
    public void FlagNameListCarriesTheHonestNameAndNotTheAlias()
    {
        // Псевдоним не отдельное поведение, поэтому в перечень имён он не
        // внесён, и счёт имён там остался прежним.
        Assert.Contains(
            nameof(LogicalExecutionFlags.UseLowConfidenceLessonSignal),
            LogicalExecutionFlags.OptimizationFlagNames);
        Assert.DoesNotContain(
            "UseConfidenceFloor",
            LogicalExecutionFlags.OptimizationFlagNames);
    }

    [Fact]
    public void SignalIsNeverUsedAsARefusalInProductionCode()
    {
        // Это главное утверждение переноса. Сигнал читается ровно в одном
        // боевом месте, и там он собирает ПРЕДУПРЕЖДЕНИЕ. Если кто-нибудь
        // превратит его в отказ — вернёт отказ, выбросит исключение или уйдёт
        // раньше по этому условию, — проверка упадёт.
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(
            Path.Combine(root, SignalReader.Replace('/', Path.DirectorySeparatorChar)));
        var tree = CSharpSyntaxTree.ParseText(source);
        Assert.Empty(tree.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        // Единственное чтение признака во всём файле.
        var read = Assert.Single(tree.GetRoot()
            .DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .Where(access => access.Name.Identifier.ValueText
                == nameof(LogicalExecutionFlags.UseLowConfidenceLessonSignal)));

        // Ближайший объемлющий оператор, в котором это чтение участвует.
        var statement = read.Ancestors().OfType<StatementSyntax>().First();

        // Чтение признака обязано участвовать в НАЗНАЧЕНИИ предупреждений, а не
        // в условии выхода: отказ, исключение или ранний возврат по этому
        // условию и есть превращение сигнала в гейт.
        var declaration = Assert.IsType<LocalDeclarationStatementSyntax>(statement);
        var declarator = Assert.Single(declaration.Declaration.Variables);
        Assert.Equal("warnings", declarator.Identifier.ValueText);

        // И внутри самого назначения нет ни отказа, ни исключения, ни выхода.
        var assigned = declarator.Initializer?.Value;
        Assert.NotNull(assigned);
        var text = assigned!.ToFullString();
        Assert.DoesNotContain(".Fail(", text, StringComparison.Ordinal);
        Assert.DoesNotContain("throw", text, StringComparison.Ordinal);
        Assert.DoesNotContain("return", text, StringComparison.Ordinal);
    }

    [Fact]
    public void NoProductionFileTurnsTheSignalIntoARefusal()
    {
        // Сплошной свип: во всём боевом коде ни одно чтение признака не стоит
        // в условии, рядом с которым возвращается отказ или бросается
        // исключение. Свип собран по ИМЕНИ ЧИТАЕМОГО ЧЛЕНА, а не по именам
        // файлов, поэтому новый потребитель, названный иначе, не выпадет.
        var root = FindRepositoryRoot();
        var offenders = new List<string>();
        var readSites = 0;
        foreach (var path in Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsOutsideProductionScope(root, path)))
        {
            var source = File.ReadAllText(path);
            if (!source.Contains(
                    nameof(LogicalExecutionFlags.UseLowConfidenceLessonSignal),
                    StringComparison.Ordinal))
            {
                continue;
            }

            var tree = CSharpSyntaxTree.ParseText(source);
            foreach (var access in tree.GetRoot()
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Where(access => access.Name.Identifier.ValueText
                    == nameof(LogicalExecutionFlags.UseLowConfidenceLessonSignal)))
            {
                readSites++;
                var statement = access.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (statement is null)
                    continue;

                // Отказ, исключение или ранний выход в том же операторе, где
                // читается признак, — это гейт.
                var statementText = statement.ToFullString();
                var looksLikeRefusal =
                    statementText.Contains(".Fail(", StringComparison.Ordinal) ||
                    statementText.Contains("throw ", StringComparison.Ordinal) ||
                    (statement is IfStatementSyntax branch &&
                     branch.Statement.ToFullString().Contains("return", StringComparison.Ordinal));
                if (looksLikeRefusal)
                {
                    offenders.Add(
                        Path.GetRelativePath(root, path).Replace('\\', '/') +
                        ":" +
                        (access.GetLocation().GetLineSpan().StartLinePosition.Line + 1));
                }
            }
        }

        // Если чтений не осталось вовсе, проверка обязана упасть громко, а не
        // стать тихо бессодержательной.
        Assert.True(readSites > 0, "Не найдено ни одного чтения признака в боевом коде.");
        Assert.Empty(offenders);
    }

    [Fact]
    public void ProbeRecipeFieldNamesStillMatchTheRecordedRecipeKeys()
    {
        // Приёмка запрета на переименование полей рецепта. Эти поля связываются
        // с записанным файлом по именам, и переименование рассоединило бы их
        // МОЛЧА, с подстановкой значения по умолчанию, а сверка файла по свёртке
        // этого не поймала бы. Проверка падает в тот день, когда имена разведут.
        var root = FindRepositoryRoot();
        var recipePath = Path.Combine(
            root,
            RecordedRecipe.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(
            File.Exists(recipePath),
            $"Записанный рецепт не найден: {RecordedRecipe}");

        using var document = JsonDocument.Parse(File.ReadAllBytes(recipePath));
        var recordedKeys = document.RootElement
            .EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Имена полей берутся из фактического текста средства замера, а не
        // переписываются здесь: иначе проверка согласилась бы сама с собой.
        var probeSource = File.ReadAllText(
            Path.Combine(root, ProbeProgram.Replace('/', Path.DirectorySeparatorChar)));
        var probeTree = CSharpSyntaxTree.ParseText(probeSource);
        var recipeRecord = Assert.Single(probeTree.GetRoot()
            .DescendantNodes()
            .OfType<RecordDeclarationSyntax>()
            .Where(record => record.Identifier.ValueText == "Recipe"));
        var recipeFieldNames = recipeRecord.ParameterList!.Parameters
            .Select(parameter => parameter.Identifier.ValueText)
            .ToArray();

        // Оба поля, про которые стоит запрет, обязаны и дальше совпадать с
        // ключами записанного файла.
        foreach (var guarded in new[] { "ConfidenceFloor", "UseConfidenceFloor" })
        {
            Assert.Contains(guarded, recipeFieldNames);
            Assert.Contains(guarded, recordedKeys);
        }

        // И запрет обязан быть написан рядом словами, а не держаться в памяти.
        Assert.Contains(
            "ПЕРЕИМЕНОВЫВАТЬ НЕЛЬЗЯ",
            probeSource,
            StringComparison.Ordinal);
        Assert.Contains(RecordedRecipe, probeSource, StringComparison.Ordinal);
    }

    [Fact]
    public void RouterConfidenceFloorIsADifferentMechanismAndStaysUntouched()
    {
        // Граница области. Второй, несвязанный механизм носит то же имя
        // `ConfidenceFloor` и СЕРИАЛИЗУЕТСЯ ПО ИМЕНИ в профилях маршрутизации.
        // Свип по имени задел бы его и сломал бы сохранённые профили молча.
        // Эта проверка закрепляет, что перенос его не тронул.
        var root = FindRepositoryRoot();
        var trainer = File.ReadAllText(Path.Combine(
            root,
            Path.Combine("26.TrainableRouter", "Training", "RoutingProfileTrainer.cs")));
        Assert.Contains("ConfidenceFloor", trainer, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "LessonRequestConfidenceThreshold",
            trainer,
            StringComparison.Ordinal);
    }

    // Опытные хозяева, их изолированные копии и каталоги сборки не являются
    // боевым кодом продукта. Проверки исключены намеренно: они вправе называть
    // признак в утверждениях.
    private static bool IsOutsideProductionScope(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        return relative.StartsWith("experiments-v2/", StringComparison.Ordinal) ||
               relative.StartsWith("35.ExperimentsV2/", StringComparison.Ordinal) ||
               relative.StartsWith("Tests/", StringComparison.Ordinal) ||
               relative.StartsWith("TestResults/", StringComparison.Ordinal) ||
               relative.StartsWith(".kiro/", StringComparison.Ordinal) ||
               relative.StartsWith(".claire/", StringComparison.Ordinal) ||
               relative.Contains("/bin/", StringComparison.Ordinal) ||
               relative.Contains("/obj/", StringComparison.Ordinal);
    }

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
            "LOW_CONFIDENCE_LESSON_SIGNAL_TESTS_REPOSITORY_ROOT_NOT_FOUND");
    }
}
