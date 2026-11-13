// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler.Prediction;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Packed method signature plus a bag of structural syntax kinds.
public sealed record PackedMethod(
    string PackedString,
    HashSet<string> KindBag,
    short FileId,
    int IndexInFile);

// Extracts method-level structural signatures for E13.
public static class MethodExtractor
{
    // Extract all methods from one C# source file.
    public static OperationResult<IReadOnlyList<PackedMethod>> Extract(string csharpSource, short fileId = 0)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<IReadOnlyList<PackedMethod>>.Fail("Empty source.", "EMPTY_SOURCE");

        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        if (methods.Count == 0)
            return OperationResult<IReadOnlyList<PackedMethod>>.Fail("No methods found.", "NO_METHODS");

        var packed = new List<PackedMethod>(methods.Count);
        for (var index = 0; index < methods.Count; index++)
        {
            var method = methods[index];
            packed.Add(new PackedMethod(
                PackedString: PackMethod(method),
                KindBag: ExtractKindBag(method),
                FileId: fileId,
                IndexInFile: index));
        }

        return OperationResult<IReadOnlyList<PackedMethod>>.Ok(packed);
    }

    // Pack a method into a readable structural signature.
    private static string PackMethod(MethodDeclarationSyntax method)
    {
        var parameters = string.Join(",", method.ParameterList.Parameters.Select(parameter => parameter.Type?.ToString() ?? "?"));
        var bodyKinds = method.Body?
            .DescendantNodes()
            .Select(node => AbbreviateKind(node.Kind().ToString()))
            .Where(kind => kind != "?")
            .Distinct()
            .Take(12)
            .ToArray() ?? [];
        var kinds = bodyKinds.Length == 0 ? "Empty" : string.Join(" ", bodyKinds);
        return $"fn {method.ReturnType} {method.Identifier}({parameters}) [{kinds}]";
    }

    // Build the unordered structural feature set used by the KNN distance.
    private static HashSet<string> ExtractKindBag(MethodDeclarationSyntax method)
    {
        var bag = new HashSet<string>(StringComparer.Ordinal);
        if (method.Body is null)
            return bag;

        foreach (var node in method.Body.DescendantNodes())
        {
            var abbreviation = AbbreviateKind(node.Kind().ToString());
            if (abbreviation != "?")
                bag.Add(abbreviation);
        }

        return bag;
    }

    // Collapse Roslyn kind names into the compact E13 method vocabulary.
    private static string AbbreviateKind(string kind) => kind switch
    {
        "IfStatement" => "If",
        "ElseClause" => "Else",
        "ReturnStatement" => "Ret",
        "ForStatement" => "For",
        "ForEachStatement" => "ForEach",
        "WhileStatement" => "While",
        "InvocationExpression" => "Call",
        "AssignmentExpression" => "Assign",
        "SimpleAssignmentExpression" => "Assign",
        "NullLiteralExpression" => "NullCheck",
        "ThrowStatement" => "Throw",
        "TryStatement" => "Try",
        "CatchClause" => "Catch",
        "LocalDeclarationStatement" => "Decl",
        "BinaryExpression" => "BinOp",
        "MemberAccessExpression" => "Member",
        "ObjectCreationExpression" => "New",
        "AwaitExpression" => "Await",
        "LambdaExpression" => "Lambda",
        "ConditionalExpression" => "Ternary",
        _ when kind.Length <= 6 => kind,
        _ when kind.EndsWith("Expression", StringComparison.Ordinal) => kind[..^10],
        _ when kind.EndsWith("Statement", StringComparison.Ordinal) => kind[..^9],
        _ => "?"
    };
}

// KNN predictor for the canonical type of the next method.
public sealed class MethodKnnPredictor
{
    private readonly List<(PackedMethod Current, PackedMethod Next)> _pairs = [];
    private readonly int _k;

    public MethodKnnPredictor(int k = 5)
    {
        _k = k;
    }

    // Train from ordered method lists, preserving only adjacent methods in the same file.
    public void Train(IReadOnlyList<PackedMethod> methods)
    {
        _pairs.Clear();
        for (var index = 0; index < methods.Count - 1; index++)
        {
            if (methods[index].FileId == methods[index + 1].FileId &&
                methods[index + 1].IndexInFile == methods[index].IndexInFile + 1)
            {
                _pairs.Add((methods[index], methods[index + 1]));
            }
        }
    }

    // Predict the canonical type of the method that should follow the query.
    public string Predict(PackedMethod query)
    {
        if (_pairs.Count == 0)
            return "Mixed";

        return _pairs
            .OrderBy(pair => JaccardDistance(query.KindBag, pair.Current.KindBag))
            .Take(_k)
            .GroupBy(pair => CanonicalType(pair.Next.KindBag))
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .First()
            .Key;
    }

    // Jaccard distance over method kind bags.
    public static double JaccardDistance(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0)
            return 0;

        var intersection = a.Count(b.Contains);
        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0 : 1.0 - intersection / (double)union;
    }

    // Average nearest-neighbour distance for scaling diagnostics.
    public double AverageNearestNeighbourDistance(IReadOnlyList<PackedMethod> queries)
    {
        if (_pairs.Count == 0 || queries.Count == 0)
            return 1.0;

        return queries
            .Take(50)
            .Average(query => _pairs.Min(pair => JaccardDistance(query.KindBag, pair.Current.KindBag)));
    }

    // Canonical method type used by the E13 vote.
    public static string CanonicalType(HashSet<string> kinds)
    {
        if (kinds.Contains("Try") || kinds.Contains("Catch"))
            return "ErrorHandling";
        if (kinds.Contains("If") && kinds.Contains("Ret"))
            return "Guard";
        if (kinds.Contains("ForEach") || kinds.Contains("For"))
            return "Loop";
        if (kinds.Contains("Await"))
            return "Async";
        if (kinds.Count == 0)
            return "Simple";
        return "Mixed";
    }

    public int TrainingSize => _pairs.Count;

    public int TrainingPairs => _pairs.Count;
}
