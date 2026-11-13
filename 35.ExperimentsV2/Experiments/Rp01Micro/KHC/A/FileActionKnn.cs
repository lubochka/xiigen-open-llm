// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2.Rp01Micro.KHC.A;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Deterministically labels a C# file with the likely next edit action.
public static class ActionLabelGenerator
{
    public const int ADD_NULL_CHECK = 0;
    public const int ADD_LOOP = 1;
    public const int REFACTOR_EXTRACT = 2;
    public const int ADD_METHOD = 3;

    public static readonly string[] Labels = ["ADD_NULL_CHECK", "ADD_LOOP", "REFACTOR_EXTRACT", "ADD_METHOD"];

    // Assign the highest-priority deterministic action label for one source file.
    public static int Label(string csharpSource)
    {
        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

        foreach (var method in methods)
        {
            if (method.Body is null)
                continue;

            var hasMemberAccess = method.Body.DescendantNodes().Any(node => node.Kind() == SyntaxKind.SimpleMemberAccessExpression);
            var hasNullCondition = method.Body.DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Any(statement => statement.Condition.DescendantNodesAndSelf()
                    .Any(node => node.Kind() == SyntaxKind.NullLiteralExpression || node.Kind() == SyntaxKind.IsPatternExpression));
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

    // Build the normalized SyntaxKind frequency vector used by FileActionKnnPredictor.
    public static float[] FeatureVector(string csharpSource)
    {
        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
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
