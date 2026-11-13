using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentsV2;

// MethodExtractor разбивает C# файл на методы и упаковывает каждый
// в короткую структурную строку вида:
//   fn bool ValidateToken(string) [If NullCheck Ret]
//
// Эта строка — "структурная подпись метода". Два метода с одинаковой
// структурой будут близки в пространстве признаков E13.
public static class MethodExtractor
{
    public static OperationResult<IReadOnlyList<PackedMethod>> Extract(
        string csharpSource, short fileId = 0)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<IReadOnlyList<PackedMethod>>.Fail("Пустой источник");

        var root    = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

        if (methods.Count == 0)
            return OperationResult<IReadOnlyList<PackedMethod>>.Fail("Нет методов в файле");

        var result = new List<PackedMethod>(methods.Count);
        for (int i = 0; i < methods.Count; i++)
        {
            var m       = methods[i];
            var kindBag = ExtractKindBag(m);
            result.Add(new PackedMethod(
                PackedString: PackMethod(m),
                KindBag:      kindBag,
                FileId:       fileId,
                IndexInFile:  i));
        }

        return OperationResult<IReadOnlyList<PackedMethod>>.Ok(result.AsReadOnly());
    }

    private static string PackMethod(MethodDeclarationSyntax m)
    {
        string parms = string.Join(",",
            m.ParameterList.Parameters.Select(p => p.Type?.ToString() ?? "?"));

        var bodyKinds = m.Body?
            .DescendantNodes()
            .Select(n => AbbreviateKind(n.Kind().ToString()))
            .Where(s => s != "?")
            .Distinct()
            .Take(12)
            .ToList() ?? [];

        string kinds = bodyKinds.Count > 0 ? string.Join(" ", bodyKinds) : "Empty";
        return $"fn {m.ReturnType} {m.Identifier}({parms}) [{kinds}]";
    }

    private static HashSet<string> ExtractKindBag(MethodDeclarationSyntax m)
    {
        var bag = new HashSet<string>();
        if (m.Body == null) return bag;
        foreach (var node in m.Body.DescendantNodes())
        {
            string abbr = AbbreviateKind(node.Kind().ToString());
            if (abbr != "?") bag.Add(abbr);
        }
        return bag;
    }

    private static string AbbreviateKind(string kind) => kind switch
    {
        "IfStatement"                 => "If",
        "ElseClause"                  => "Else",
        "ReturnStatement"             => "Ret",
        "ForStatement"                => "For",
        "ForEachStatement"            => "ForEach",
        "WhileStatement"              => "While",
        "InvocationExpression"        => "Call",
        "AssignmentExpression"        => "Assign",
        "SimpleAssignmentExpression"  => "Assign",
        "NullLiteralExpression"       => "NullCheck",
        "ThrowStatement"              => "Throw",
        "TryStatement"                => "Try",
        "CatchClause"                 => "Catch",
        "LocalDeclarationStatement"   => "Decl",
        "BinaryExpression"            => "BinOp",
        "MemberAccessExpression"      => "Member",
        "ObjectCreationExpression"    => "New",
        "AwaitExpression"             => "Await",
        "LambdaExpression"            => "Lambda",
        "ConditionalExpression"       => "Ternary",
        _ when kind.Length <= 6                  => kind,
        _ when kind.EndsWith("Expression")       => kind[..^10],
        _ when kind.EndsWith("Statement")        => kind[..^9],
        _ => "?"
    };
}

// Упакованный метод: структурная строка + bag-of-kinds + позиция в файле
public sealed record PackedMethod(
    string          PackedString,
    HashSet<string> KindBag,
    short           FileId,
    int             IndexInFile);
