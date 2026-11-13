using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// StructuralVectorExtractor строит структурный вектор для каждого AST-узла.
// Вектор описывает где находится узел в дереве: не просто глубину,
// а путь через три уровня предков.
public static class StructuralVectorExtractor
{
    // Извлечь вектор для одного AST-узла.
    public static StructuralVector Extract(SyntaxNode node, short fileId = 0)
    {
        var ancestors = new List<SyntaxNode>();
        var current = node.Parent;
        while (current != null && ancestors.Count < 3)
        {
            ancestors.Add(current);
            current = current.Parent;
        }

        short p1 = ancestors.Count > 0 ? NodeTypeId(ancestors[0]) : (short)0;
        short p2 = ancestors.Count > 1 ? NodeTypeId(ancestors[1]) : (short)0;
        short p3 = ancestors.Count > 2 ? NodeTypeId(ancestors[2]) : (short)0;

        byte posInParent = 0;
        byte siblingCount = 0;
        if (node.Parent != null)
        {
            var siblings = node.Parent.ChildNodes().ToList();
            siblingCount = (byte)Math.Min(siblings.Count, 255);
            int idx = siblings.IndexOf(node);
            posInParent = (byte)Math.Min(idx >= 0 ? idx : 0, 255);
        }

        byte depthInMethod = 0;
        var walker = node.Parent;
        while (walker != null)
        {
            if (walker.IsKind(SyntaxKind.MethodDeclaration)) break;
            depthInMethod++;
            walker = walker.Parent;
        }

        byte scopeDepth = 0;
        walker = node.Parent;
        while (walker != null)
        {
            if (walker.IsKind(SyntaxKind.Block)) scopeDepth++;
            walker = walker.Parent;
        }

        return new StructuralVector
        {
            AstNodeType      = NodeTypeId(node),
            Parent1Type      = p1,
            Parent2Type      = p2,
            Parent3Type      = p3,
            PositionInParent = posInParent,
            SiblingCount     = siblingCount,
            DepthInMethod    = depthInMethod > 255 ? (byte)255 : (byte)depthInMethod,
            ScopeDepth       = scopeDepth > 255 ? (byte)255 : (byte)scopeDepth,
            FileId           = fileId,
            SyntacticRole    = SyntacticRoleMapper.GetRole(node)   // E09
        };
    }

    private static short NodeTypeId(SyntaxNode node)
        => (short)((int)node.Kind() % 200);
}

// Структурный вектор — описание места токена в дереве компилятора.
// Используется для K-NN поиска похожих контекстов.
public readonly record struct StructuralVector
{
    public short AstNodeType      { get; init; }
    public short Parent1Type      { get; init; }
    public short Parent2Type      { get; init; }
    public short Parent3Type      { get; init; }
    public byte  PositionInParent { get; init; }
    public byte  SiblingCount     { get; init; }
    public byte  DepthInMethod    { get; init; }
    public byte  ScopeDepth       { get; init; }
    public short FileId           { get; init; }
    public byte  SyntacticRole    { get; init; }   // E09: 30 values, 0=UNKNOWN

    // Взвешенное расстояние между двумя векторами.
    // E09: SyntacticRole weight=4.0 (highest); PositionInParent reduced 1.0→0.3.
    public double DistanceTo(StructuralVector other) =>
        (SyntacticRole    != other.SyntacticRole    ? 4.0 : 0) +
        (Parent1Type      != other.Parent1Type      ? 3.0 : 0) +
        (AstNodeType      != other.AstNodeType      ? 2.0 : 0) +
        (Parent2Type      != other.Parent2Type      ? 2.0 : 0) +
        (Parent3Type      != other.Parent3Type      ? 1.5 : 0) +
        (PositionInParent != other.PositionInParent ? 0.3 : 0) +
        Math.Abs(ScopeDepth    - other.ScopeDepth)    * 0.3 +
        Math.Abs(DepthInMethod - other.DepthInMethod) * 0.2;
}
