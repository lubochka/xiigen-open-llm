using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentsV2;

// Maps a C# AST node to one of 30 syntactic role codes based on its parent context.
// Role 0=UNKNOWN is the default when the parent context doesn't match a known pattern.
// Used by E09: SyntacticRole weight=4.0 in DistanceTo (highest weight — most discriminative).
//
// Role table:
//  0 UNKNOWN          1 ASSIGNED_TO       2 ASSIGNED_VALUE   3 BINARY_LEFT
//  4 BINARY_RIGHT     5 CALLED_TARGET     6 ARGUMENT         7 IF_CONDITION
//  8 IF_THEN          9 IF_ELSE          10 RETURN_VALUE     11 VAR_NAME
// 12 VAR_INIT        13 MEMBER_TARGET    14 MEMBER_NAME      15 METHOD_PARAM
// 16 METHOD_BODY     17 METHOD_RETURN_TYPE 18 WHILE_CONDITION 19 WHILE_BODY
// 20 FOR_INIT        21 FOR_CONDITION    22 FOR_INCREMENT    23 FOR_BODY
// 24 CAST_TYPE       25 CAST_EXPRESSION  26 LAMBDA_PARAM     27 LAMBDA_BODY
// 28 ARRAY_ELEMENT   29 BLOCK_STATEMENT
public static class SyntacticRoleMapper
{
    public static byte GetRole(SyntaxNode node)
    {
        var parent = node.Parent;
        if (parent is null) return 0;

        return (byte)(parent switch
        {
            AssignmentExpressionSyntax a when a.Left  == node     => 1,
            AssignmentExpressionSyntax a when a.Right == node     => 2,

            BinaryExpressionSyntax b when b.Left  == node         => 3,
            BinaryExpressionSyntax b when b.Right == node         => 4,

            InvocationExpressionSyntax i when i.Expression == node => 5,
            ArgumentSyntax                                         => 6,

            IfStatementSyntax s when s.Condition == node          => 7,
            IfStatementSyntax s when s.Statement  == node         => 8,
            ElseClauseSyntax                                       => 9,

            ReturnStatementSyntax r when r.Expression == node     => 10,

            VariableDeclarationSyntax                              => 11,
            EqualsValueClauseSyntax   e when e.Value == node      => 12,

            MemberAccessExpressionSyntax m when m.Expression == node => 13,
            MemberAccessExpressionSyntax m when m.Name       == node => 14,

            ParameterListSyntax                                    => 15,

            MethodDeclarationSyntax d when d.ReturnType == node   => 17,
            MethodDeclarationSyntax                                => 16,

            WhileStatementSyntax w when w.Condition == node       => 18,
            WhileStatementSyntax                                   => 19,

            ForStatementSyntax f when f.Declaration == node       => 20,
            ForStatementSyntax f when f.Condition   == node       => 21,
            ForStatementSyntax f when
                node is ExpressionSyntax &&
                f.Incrementors.Any(inc => object.ReferenceEquals(inc, node)) => 22,
            ForStatementSyntax                                     => 23,

            CastExpressionSyntax c when c.Type       == node      => 24,
            CastExpressionSyntax c when c.Expression == node      => 25,

            SimpleLambdaExpressionSyntax l when l.Body == node    => 27,
            SimpleLambdaExpressionSyntax                           => 26,

            ParenthesizedLambdaExpressionSyntax p when p.Body == node => 27,
            ParenthesizedLambdaExpressionSyntax                    => 26,

            InitializerExpressionSyntax                            => 28,
            CollectionExpressionSyntax                             => 28,
            BlockSyntax                                            => 29,

            _                                                      => 0,
        });
    }
}
