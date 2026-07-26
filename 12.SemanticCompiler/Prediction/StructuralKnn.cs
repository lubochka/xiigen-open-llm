// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler.Prediction;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Structural vector for one AST node position.
public readonly record struct StructuralVector
{
    public short AstNodeType { get; init; }
    public short Parent1Type { get; init; }
    public short Parent2Type { get; init; }
    public short Parent3Type { get; init; }
    public byte PositionInParent { get; init; }
    public byte SiblingCount { get; init; }
    public byte DepthInMethod { get; init; }
    public byte ScopeDepth { get; init; }
    public short FileId { get; init; }
    public byte SyntacticRole { get; init; }

    // Weighted distance keeps syntactic role and parent structure dominant.
    public double DistanceTo(StructuralVector other) =>
        (SyntacticRole != other.SyntacticRole ? 4.0 : 0) +
        (Parent1Type != other.Parent1Type ? 3.0 : 0) +
        (AstNodeType != other.AstNodeType ? 2.0 : 0) +
        (Parent2Type != other.Parent2Type ? 2.0 : 0) +
        (Parent3Type != other.Parent3Type ? 1.5 : 0) +
        (PositionInParent != other.PositionInParent ? 0.3 : 0) +
        Math.Abs(ScopeDepth - other.ScopeDepth) * 0.3 +
        Math.Abs(DepthInMethod - other.DepthInMethod) * 0.2;
}

// Builds E09 structural vectors from Roslyn syntax nodes.
public static class StructuralVectorExtractor
{
    // Extract one compact vector from a Roslyn node.
    public static StructuralVector Extract(SyntaxNode node, short fileId = 0)
    {
        var ancestors = new List<SyntaxNode>();
        var current = node.Parent;
        while (current is not null && ancestors.Count < 3)
        {
            ancestors.Add(current);
            current = current.Parent;
        }

        var positionInParent = (byte)0;
        var siblingCount = (byte)0;
        if (node.Parent is not null)
        {
            var siblings = node.Parent.ChildNodes().ToList();
            siblingCount = (byte)Math.Min(siblings.Count, 255);
            var index = siblings.IndexOf(node);
            positionInParent = (byte)Math.Min(index >= 0 ? index : 0, 255);
        }

        var depthInMethod = 0;
        var walker = node.Parent;
        while (walker is not null)
        {
            if (walker.IsKind(SyntaxKind.MethodDeclaration))
                break;

            depthInMethod++;
            walker = walker.Parent;
        }

        var scopeDepth = 0;
        walker = node.Parent;
        while (walker is not null)
        {
            if (walker.IsKind(SyntaxKind.Block))
                scopeDepth++;

            walker = walker.Parent;
        }

        return new StructuralVector
        {
            AstNodeType = NodeTypeId(node),
            Parent1Type = ancestors.Count > 0 ? NodeTypeId(ancestors[0]) : (short)0,
            Parent2Type = ancestors.Count > 1 ? NodeTypeId(ancestors[1]) : (short)0,
            Parent3Type = ancestors.Count > 2 ? NodeTypeId(ancestors[2]) : (short)0,
            PositionInParent = positionInParent,
            SiblingCount = siblingCount,
            DepthInMethod = depthInMethod > 255 ? (byte)255 : (byte)depthInMethod,
            ScopeDepth = scopeDepth > 255 ? (byte)255 : (byte)scopeDepth,
            FileId = fileId,
            SyntacticRole = SyntacticRoleMapper.GetRole(node)
        };
    }

    // Convert Roslyn syntax kind to the compact token id used by E09.
    public static short NodeTypeId(SyntaxNode node)
    {
        return (short)((int)node.Kind() % 200);
    }
}

// KNN predictor over structural AST vectors.
public sealed class StructuralKnnPredictor
{
    private readonly List<(StructuralVector Vector, int NextToken)> _training = [];
    private readonly int _k;

    public StructuralKnnPredictor(int k = 5)
    {
        _k = k;
    }

    // Replace all training examples with caller-provided vector/next-token pairs.
    public void Train(IReadOnlyList<(StructuralVector vector, int nextToken)> pairs)
    {
        _training.Clear();
        _training.AddRange(pairs);
    }

    // Train from source text by walking Roslyn nodes and pairing each node with its successor.
    public void TrainFromSource(string csharpSource, short fileId = 0)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
        {
            Train([]);
            return;
        }

        var nodes = CSharpSyntaxTree.ParseText(csharpSource).GetRoot().DescendantNodes().ToArray();
        var pairs = new List<(StructuralVector vector, int nextToken)>(Math.Max(0, nodes.Length - 1));
        for (var index = 0; index < nodes.Length - 1; index++)
        {
            var vector = StructuralVectorExtractor.Extract(nodes[index], fileId);
            var nextToken = StructuralVectorExtractor.NodeTypeId(nodes[index + 1]);
            pairs.Add((vector, nextToken));
        }

        Train(pairs);
    }

    // Predict the next AST token id from one structural context.
    public int Predict(StructuralVector context)
    {
        if (_training.Count == 0)
            return 0;

        var nearest = _training
            .Select(item => (Distance: context.DistanceTo(item.Vector), item.NextToken))
            .OrderBy(item => item.Distance)
            .Take(_k)
            .ToArray();

        return nearest
            .GroupBy(item => item.NextToken)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .First()
            .Key;
    }

    // Predict directly from a Roslyn node using the same extraction as training.
    public int PredictFromNode(SyntaxNode node, short fileId = 0)
    {
        return Predict(StructuralVectorExtractor.Extract(node, fileId));
    }

    // Average distance to nearest training neighbour for scaling diagnostics.
    public double AverageNearestNeighborDistance(IReadOnlyList<StructuralVector> queries, int sampleSize = 100)
    {
        var sample = queries.Take(sampleSize).ToArray();
        if (sample.Length == 0 || _training.Count == 0)
            return 0;

        return sample.Average(query => _training.Min(trained => query.DistanceTo(trained.Vector)));
    }

    public int TrainingSize => _training.Count;
}

// Maps syntax nodes into the 30-role E09 structural-role vocabulary.
internal static class SyntacticRoleMapper
{
    // Return role 0 when the parent context is unknown.
    public static byte GetRole(SyntaxNode node)
    {
        var parent = node.Parent;
        if (parent is null)
            return 0;

        return (byte)(parent switch
        {
            AssignmentExpressionSyntax assignment when assignment.Left == node => 1,
            AssignmentExpressionSyntax assignment when assignment.Right == node => 2,
            BinaryExpressionSyntax binary when binary.Left == node => 3,
            BinaryExpressionSyntax binary when binary.Right == node => 4,
            InvocationExpressionSyntax invocation when invocation.Expression == node => 5,
            ArgumentSyntax => 6,
            IfStatementSyntax statement when statement.Condition == node => 7,
            IfStatementSyntax statement when statement.Statement == node => 8,
            ElseClauseSyntax => 9,
            ReturnStatementSyntax statement when statement.Expression == node => 10,
            VariableDeclarationSyntax => 11,
            EqualsValueClauseSyntax equals when equals.Value == node => 12,
            MemberAccessExpressionSyntax access when access.Expression == node => 13,
            MemberAccessExpressionSyntax access when access.Name == node => 14,
            ParameterListSyntax => 15,
            MethodDeclarationSyntax declaration when declaration.ReturnType == node => 17,
            MethodDeclarationSyntax => 16,
            WhileStatementSyntax statement when statement.Condition == node => 18,
            WhileStatementSyntax => 19,
            ForStatementSyntax statement when statement.Declaration == node => 20,
            ForStatementSyntax statement when statement.Condition == node => 21,
            ForStatementSyntax statement when node is ExpressionSyntax && statement.Incrementors.Any(inc => ReferenceEquals(inc, node)) => 22,
            ForStatementSyntax => 23,
            CastExpressionSyntax cast when cast.Type == node => 24,
            CastExpressionSyntax cast when cast.Expression == node => 25,
            SimpleLambdaExpressionSyntax lambda when lambda.Body == node => 27,
            SimpleLambdaExpressionSyntax => 26,
            ParenthesizedLambdaExpressionSyntax lambda when lambda.Body == node => 27,
            ParenthesizedLambdaExpressionSyntax => 26,
            InitializerExpressionSyntax => 28,
            CollectionExpressionSyntax => 28,
            BlockSyntax => 29,
            _ => 0
        });
    }
}
