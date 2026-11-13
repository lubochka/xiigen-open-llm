// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler.Prediction;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Deterministically labels a C# file with the likely next edit action.
public static class ActionLabelGenerator
{
    // FIX-KHC transfer, plan 24-file-action-shared-parse.md, from KH-COST-01.
    // Разбор исходника живёт ЗДЕСЬ, у разметчика, и только здесь. Подготовка
    // уроков зовёт этого помощника, а не разборщик напрямую, поэтому настройки
    // разбора остаются у одного хозяина: будущая правка настроек внутри
    // разметчика не разойдётся молча с путём подготовки уроков.
    // Внутренний, а не публичный: единственные вызывающие — в этой же сборке и
    // в проекте проверок, которому сборка открыта атрибутом видимости.
    internal static SyntaxNode Parse(string csharpSource)
        => CSharpSyntaxTree.ParseText(csharpSource).GetRoot();

    public const int ADD_NULL_CHECK = 0;
    public const int ADD_LOOP = 1;
    public const int REFACTOR_EXTRACT = 2;
    public const int ADD_METHOD = 3;

    public static readonly string[] Labels = ["ADD_NULL_CHECK", "ADD_LOOP", "REFACTOR_EXTRACT", "ADD_METHOD"];

    // Assign the highest-priority deterministic action label for one source file.
    public static int Label(string csharpSource)
    {
        // FIX-KHC transfer, plan 24-file-action-shared-parse.md, from KH-COST-01.
        // Строковый путь сохраняет СВОЙ разбор и прежнюю обработку пустого текста:
        // старые вызывающие ничего не замечают. Тело правила переехало в перегрузку
        // ниже, которая принимает уже разобранное дерево.
        return Label(Parse(csharpSource));
    }

    // FIX-KHC transfer: то же правило метки по УЖЕ разобранному дереву.
    // Договор перегрузки: передаётся неизменяемое дерево того же исходника с теми
    // же настройками разбора. Тело не разбирает текст заново и дерево не меняет.
    //
    // Внутренняя, а не публичная, и это тот же принятый образец, что у кэша
    // соседей в этой папке: перегрузка с уже готовым входом остаётся внутренней,
    // а её постоянная проверка живёт в проекте проверок, которому сборка открыта
    // атрибутом видимости внутренних членов. Публичной её делать нельзя ещё и
    // потому, что условие правильности живёт в договоре, а не в подписи: вектор
    // по произвольному поддереву нормировался бы по этому поддереву и дал бы
    // правдоподобно неверную учебную строку, чего извне до переноса было нельзя.
    internal static int Label(SyntaxNode root)
    {
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

        foreach (var method in methods)
        {
            if (method.Body is null)
                continue;

            var hasMemberAccess = method.Body.DescendantNodes().Any(node => node.Kind() == SyntaxKind.SimpleMemberAccessExpression);
            // FIX-NG: признаком защиты от пустого значения считается пустой литерал
            // либо сопоставление с образцом, КРОМЕ предусмотренного исключения ниже.
            // Прежнее условие принимало за защиту любое сопоставление с образцом,
            // поэтому обычная проверка величины целого числа тоже глушила совет и
            // учебная метка выходила ложной.
            var hasNullCondition = method.Body.DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Any(statement => statement.Condition.DescendantNodesAndSelf()
                    .Any(node => node.Kind() == SyntaxKind.NullLiteralExpression ||
                        // Сопоставление с образцом остаётся защитой, если оно не является
                        // сравнением величины по параметру прямого целочисленного типа.
                        (node is IsPatternExpressionSyntax pattern &&
                         !IsNonNullableIntRelationalPattern(method, pattern))));
            if (hasMemberAccess && !hasNullCondition)
                return ADD_NULL_CHECK;
        }

        foreach (var method in methods)
        {
            if (method.Body is null)
                continue;

            var hasCollection = method.ParameterList.Parameters.Any(parameter =>
                parameter.Type?.ToString().Contains("IEnumerable", StringComparison.Ordinal) == true ||
                parameter.Type?.ToString().Contains("[]", StringComparison.Ordinal) == true ||
                parameter.Type?.ToString().Contains("List<", StringComparison.Ordinal) == true);
            var hasLoop = method.Body.DescendantNodes().Any(node =>
                node.Kind() == SyntaxKind.ForEachStatement ||
                node.Kind() == SyntaxKind.ForStatement ||
                node.Kind() == SyntaxKind.WhileStatement);
            if (hasCollection && !hasLoop)
                return ADD_LOOP;
        }

        foreach (var method in methods)
        {
            if (method.Body is null)
                continue;

            var distinctStatementKinds = method.Body.DescendantNodes()
                .Select(node => node.Kind().ToString())
                .Where(kind => kind.EndsWith("Statement", StringComparison.Ordinal))
                .Distinct()
                .Count();
            if (distinctStatementKinds > 6)
                return REFACTOR_EXTRACT;
        }

        return ADD_METHOD;
    }

    // FIX-NG: распознаёт ровно один предусмотренный образец — сравнение величины
    // по параметру прямого целочисленного типа, которое защитой от пустого
    // значения не является. Такое сравнение проходит и при пустом получателе:
    // при `n = 1, x = пусто` условие `n is >0` выполнено, а `x.Length` падает.
    // Проверяется написание типа параметра, а не семантическая модель: разрешение
    // имён, затенение, псевдоним System.Int32, локальные переменные, составные
    // образцы `and/or/not` и межпроцедурный анализ пустоты сюда не входят.
    private static bool IsNonNullableIntRelationalPattern(
        MethodDeclarationSyntax method,
        IsPatternExpressionSyntax pattern)
    {
        // Исключение действует только для сравнения величины по простому имени:
        // образец с типом и любая иная форма остаются защитой от пустого значения.
        if (pattern.Pattern is not RelationalPatternSyntax ||
            pattern.Expression is not IdentifierNameSyntax identifier)
            return false;
        // Имя из образца должно совпасть с параметром, чей тип записан прямым int:
        // у типа с пустотой `int?` образец действительно отсекает пустое значение,
        // поэтому такой параметр под исключение не попадает.
        return method.ParameterList.Parameters.Any(parameter =>
            parameter.Identifier.ValueText == identifier.Identifier.ValueText &&
            parameter.Type is PredefinedTypeSyntax predefined &&
            // RawKind сравнивается напрямую: этот файл не подключает пространство
            // имён с расширением IsKind для SyntaxToken.
            predefined.Keyword.RawKind == (int)SyntaxKind.IntKeyword);
    }

    // Build the normalized SyntaxKind frequency vector used by FileActionKnnPredictor.
    public static float[] FeatureVector(string csharpSource)
    {
        // FIX-KHC transfer: строковый путь сохраняет свой разбор и делегирует.
        return FeatureVector(Parse(csharpSource));
    }

    // FIX-KHC transfer: тот же вектор признаков по УЖЕ разобранному дереву.
    // Договор перегрузки тот же: дерево того же исходника, без повторного разбора.
    // Внутренняя по той же причине, что и перегрузка метки выше.
    internal static float[] FeatureVector(SyntaxNode root)
    {
        var nodes = root.DescendantNodes().ToArray();
        if (nodes.Length == 0)
            return new float[TargetKinds.Length];

        var counts = new float[TargetKinds.Length];
        for (var index = 0; index < TargetKinds.Length; index++)
            counts[index] = nodes.Count(node => node.Kind() == TargetKinds[index]) / (float)nodes.Length;

        return counts;
    }

    private static readonly SyntaxKind[] TargetKinds =
    [
        SyntaxKind.IfStatement, SyntaxKind.ElseClause,
        SyntaxKind.ForStatement, SyntaxKind.ForEachStatement,
        SyntaxKind.WhileStatement, SyntaxKind.ReturnStatement,
        SyntaxKind.ThrowStatement, SyntaxKind.TryStatement,
        SyntaxKind.InvocationExpression, SyntaxKind.SimpleMemberAccessExpression,
        SyntaxKind.SimpleAssignmentExpression, SyntaxKind.NullLiteralExpression,
        SyntaxKind.IsPatternExpression, SyntaxKind.LocalDeclarationStatement,
        SyntaxKind.ObjectCreationExpression, SyntaxKind.AwaitExpression,
        SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.ConditionalExpression,
        SyntaxKind.MethodDeclaration, SyntaxKind.PropertyDeclaration,
        SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration,
        SyntaxKind.Parameter, SyntaxKind.Argument,
        SyntaxKind.BitwiseAndExpression, SyntaxKind.LogicalAndExpression,
        SyntaxKind.LogicalOrExpression, SyntaxKind.NotEqualsExpression,
        SyntaxKind.EqualsExpression, SyntaxKind.GenericName
    ];
}

// KNN predictor over file-level AST action vectors.
public sealed class FileActionKnnPredictor
{
    private readonly List<(float[] Features, int Label)> _train = [];
    private readonly int _k;

    public FileActionKnnPredictor(int k = 3)
    {
        _k = k;
    }

    // Replace all file-action examples with the caller-provided training set.
    public void Train(IReadOnlyList<(float[] Features, int Label)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // Predict the nearest voted action label for one feature vector.
    public int Predict(float[] query)
    {
        if (_train.Count == 0)
            return ActionLabelGenerator.ADD_METHOD;

        return _train
            .OrderBy(example => CosineDistance(query, example.Features))
            .Take(_k)
            .GroupBy(example => example.Label)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .First()
            .Key;
    }

    // Cosine distance for normalized syntax-kind vectors.
    private static double CosineDistance(float[] a, float[] b)
    {
        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;
        var length = Math.Min(a.Length, b.Length);
        for (var index = 0; index < length; index++)
        {
            dot += a[index] * b[index];
            leftNorm += a[index] * a[index];
            rightNorm += b[index] * b[index];
        }

        var denominator = Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm);
        return denominator < 1e-9 ? 1.0 : 1.0 - dot / denominator;
    }

    public int TrainingSize => _train.Count;

    public int TrainingCount => _train.Count;
}
