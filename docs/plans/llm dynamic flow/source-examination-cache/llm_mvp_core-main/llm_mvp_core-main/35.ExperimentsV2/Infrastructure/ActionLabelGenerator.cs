using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentsV2;

// ActionLabelGenerator assigns an action-type label to a file based on
// deterministic AST analysis — no git history or manual annotation required.
// Priority: ADD_NULL_CHECK > ADD_LOOP > REFACTOR_EXTRACT > ADD_METHOD
public static class ActionLabelGenerator
{
    public const int ADD_NULL_CHECK   = 0;
    public const int ADD_LOOP         = 1;
    public const int REFACTOR_EXTRACT = 2;
    public const int ADD_METHOD       = 3;

    public static readonly string[] Labels =
        ["ADD_NULL_CHECK", "ADD_LOOP", "REFACTOR_EXTRACT", "ADD_METHOD"];

    public static int Label(string csharpSource)
    {
        var root    = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

        // Rule 1: ADD_NULL_CHECK — member access without null guard in any if-condition
        foreach (var m in methods)
        {
            if (m.Body == null) continue;
            bool hasMemberAccess = m.Body.DescendantNodes()
                .Any(n => n.Kind() == SyntaxKind.SimpleMemberAccessExpression);
            bool hasNullIfCondition = m.Body.DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Any(ifStmt => ifStmt.Condition.DescendantNodesAndSelf()
                    .Any(n => n.Kind() == SyntaxKind.NullLiteralExpression ||
                              n.Kind() == SyntaxKind.IsPatternExpression));
            if (hasMemberAccess && !hasNullIfCondition)
                return ADD_NULL_CHECK;
        }

        // Rule 2: ADD_LOOP — method accepts collection but contains no loop
        foreach (var m in methods)
        {
            if (m.Body == null) continue;
            bool hasCollection = m.ParameterList.Parameters
                .Any(p => p.Type?.ToString().Contains("IEnumerable") == true ||
                          p.Type?.ToString().Contains("[]") == true ||
                          p.Type?.ToString().Contains("List<") == true);
            bool hasLoop = m.Body.DescendantNodes()
                .Any(n => n.Kind() == SyntaxKind.ForEachStatement ||
                          n.Kind() == SyntaxKind.ForStatement ||
                          n.Kind() == SyntaxKind.WhileStatement);
            if (hasCollection && !hasLoop)
                return ADD_LOOP;
        }

        // Rule 3: REFACTOR_EXTRACT — method body has > 6 distinct statement types
        foreach (var m in methods)
        {
            if (m.Body == null) continue;
            int distinct = m.Body.DescendantNodes()
                .Select(n => n.Kind().ToString())
                .Where(k => k.EndsWith("Statement"))
                .Distinct()
                .Count();
            if (distinct > 6)
                return REFACTOR_EXTRACT;
        }

        // Rule 4: ADD_METHOD — small class (< 4 methods)
        if (methods.Count < 4)
            return ADD_METHOD;

        return ADD_METHOD;
    }

    // Normalised frequency vector of 30 key SyntaxKind values for cosine K-NN.
    public static float[] FeatureVector(string csharpSource)
    {
        var root  = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var nodes = root.DescendantNodes().ToList();
        int total = nodes.Count;
        if (total == 0) return new float[TargetKinds.Length];

        var counts = new float[TargetKinds.Length];
        for (int i = 0; i < TargetKinds.Length; i++)
            counts[i] = nodes.Count(n => n.Kind() == TargetKinds[i]) / (float)total;
        return counts;
    }

    private static readonly SyntaxKind[] TargetKinds =
    [
        SyntaxKind.IfStatement,             SyntaxKind.ElseClause,
        SyntaxKind.ForStatement,            SyntaxKind.ForEachStatement,
        SyntaxKind.WhileStatement,          SyntaxKind.ReturnStatement,
        SyntaxKind.ThrowStatement,          SyntaxKind.TryStatement,
        SyntaxKind.InvocationExpression,    SyntaxKind.SimpleMemberAccessExpression,
        SyntaxKind.SimpleAssignmentExpression, SyntaxKind.NullLiteralExpression,
        SyntaxKind.IsPatternExpression,     SyntaxKind.LocalDeclarationStatement,
        SyntaxKind.ObjectCreationExpression, SyntaxKind.AwaitExpression,
        SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.ConditionalExpression,
        SyntaxKind.MethodDeclaration,       SyntaxKind.PropertyDeclaration,
        SyntaxKind.ClassDeclaration,        SyntaxKind.InterfaceDeclaration,
        SyntaxKind.Parameter,              SyntaxKind.Argument,
        SyntaxKind.BitwiseAndExpression,   SyntaxKind.LogicalAndExpression,
        SyntaxKind.LogicalOrExpression,    SyntaxKind.NotEqualsExpression,
        SyntaxKind.EqualsExpression,       SyntaxKind.GenericName,
    ];
}
