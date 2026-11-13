// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler.Prediction;

// Пространство имён дерева разбора: нужно, чтобы назвать тип узла в выборе
// источника признаков ниже. Тот же набор подключений стоит у соседнего
// предсказателя структуры в этой же папке (Prediction/StructuralKnn.cs).
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Packed method signature plus a bag of structural syntax kinds.
public sealed record PackedMethod(
    string PackedString,
    HashSet<string> KindBag,
    short FileId,
    int IndexInFile);

// FIX-KS training material: the methods as extracted, plus a separate projection of
// each method's OUTER body — the syntax it runs itself, with local functions and
// lambdas left out. Only the target side of a pair is trained from this projection,
// so a method that is merely declared inside another method's body no longer decides
// what category that method predicts. Training-only: nothing here is a query feature.
public sealed record MethodScopeMaterial(
    IReadOnlyList<PackedMethod> Methods,
    IReadOnlyDictionary<(short FileId, int IndexInFile), HashSet<string>> TargetScopes,
    string ProjectionVersion = MethodScopeMaterial.OuterBodySyntaxV1)
{
    // The only projection this predictor knows how to train from. A material built by
    // some future projection must be rejected rather than silently trained.
    public const string OuterBodySyntaxV1 = "outer-body-syntax/v1";
}

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

    // FIX-KS: extract the same methods and, from the same single parse, the outer-body
    // projection used for the target side of a pair. The methods themselves and their
    // KindBag are byte-for-byte what Extract produces, so the query side and the
    // distance are untouched; only the extra map is new.
    public static OperationResult<MethodScopeMaterial> ExtractWithTargetScope(
        string csharpSource, short fileId = 0)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<MethodScopeMaterial>.Fail("Empty source.", "EMPTY_SOURCE");

        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        if (methods.Count == 0)
            return OperationResult<MethodScopeMaterial>.Fail("No methods found.", "NO_METHODS");

        var packed = new List<PackedMethod>(methods.Count);
        var scopes = new Dictionary<(short FileId, int IndexInFile), HashSet<string>>();
        for (var index = 0; index < methods.Count; index++)
        {
            var method = methods[index];
            packed.Add(new PackedMethod(
                PackedString: PackMethod(method),
                KindBag: ExtractKindBag(method),
                FileId: fileId,
                IndexInFile: index));
            scopes[(fileId, index)] = ExtractOuterBodyKindBag(method);
        }

        return OperationResult<MethodScopeMaterial>.Ok(
            new MethodScopeMaterial(packed, scopes));
    }

    // The outer-body projection: walk the body but do not descend into a local function
    // or a lambda, and do not count those two nodes themselves. An uncalled helper
    // declared inside the method therefore stops contributing its syntax to the
    // method's trained category. A method without a body yields an empty set, exactly
    // as the ordinary bag does — this projection claims no improvement there.
    private static HashSet<string> ExtractOuterBodyKindBag(MethodDeclarationSyntax method)
    {
        var bag = new HashSet<string>(StringComparer.Ordinal);
        if (method.Body is null)
            return bag;

        var outerNodes = method.Body.DescendantNodes(descendIntoChildren: child =>
            child is not LocalFunctionStatementSyntax
            && child is not AnonymousFunctionExpressionSyntax);

        foreach (var node in outerNodes)
        {
            if (node is LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax)
                continue;

            var abbreviation = AbbreviateKind(node.Kind().ToString());
            if (abbreviation != "?")
                bag.Add(abbreviation);
        }

        return bag;
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
    //
    // FIX-P83, опыт о методе, написанном одной строкой через стрелку (восемьдесят
    // третий опыт, 1 ноября 2026). Прежде здесь стоял ранний выход с пустым
    // набором, когда блочного тела нет, и выражение-тело не читалось вовсе.
    // Последствия измерены: пустой набор делает однострочник идеально похожим
    // (расстояние ноль) на любое объявление без тела и максимально далёким
    // (расстояние один) от своего же блочного смыслового двойника, а вид метода
    // при пустом наборе объявляется «простым».
    //
    // ПРЕДУПРЕЖДЕНИЕ ТОМУ, КТО БУДЕТ ПРАВИТЬ ЭТО ДАЛЬШЕ: две строки чтения узлов
    // ниже НЕЛЬЗЯ выравнивать между собой, это несущая несимметричность, а не
    // небрежность. У блочного тела читается поддерево БЕЗ самого узла: узел блока —
    // это фигурные скобки, смысла он не несёт, и набор блочного двойника
    // «{ return await R(); }» измеренно равен «Await, Call, Ret» без признака
    // блока. У выражения-тела читается поддерево ВМЕСТЕ с самим выражением: у
    // однострочника выражение И ЕСТЬ содержание метода, и в «=> await R()» узел
    // ожидания ровно один — корневой. Если выровнять, «=> await R()» даст ровно
    // тот же набор «Call», что обычный «=> Calc()», а «=> Calc()» вернётся к
    // пустому набору: правка потеряет смысл целиком, а не частично.
    private static HashSet<string> ExtractKindBag(MethodDeclarationSyntax method)
    {
        // Набор признаков метода: каждое сокращённое имя вида узла входит один раз.
        var bag = new HashSet<string>(StringComparer.Ordinal);

        // Узлы, которые считаются содержанием метода; источник выбирается ниже.
        IEnumerable<SyntaxNode> nodes;
        if (method.Body is not null)
            // Обычный метод с фигурными скобками: поддерево блока без самого блока.
            nodes = method.Body.DescendantNodes();
        else if (method.ExpressionBody is not null)
            // Метод одной строкой через стрелку: выражение вместе с его поддеревом.
            nodes = method.ExpressionBody.Expression.DescendantNodesAndSelf();
        else
            // Настоящее объявление без тела — абстрактное, в интерфейсе, внешнее:
            // у него нет ни блока, ни выражения, и пустой набор для него честен.
            return bag;

        // Перебираем выбранные узлы и складываем их виды в набор признаков.
        foreach (var node in nodes)
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

    // FIX-KS: train from the same adjacency rule, but take the target side's feature
    // set from the outer-body projection instead of the method's full bag. The current
    // side, the query, Jaccard, k, the vote and CanonicalType are all unchanged.
    //
    // The table is replaced only after the whole material validates, so a rejected
    // material leaves previously learned pairs answering. Each accepted target set is
    // copied, so a later change to the projection map cannot rewrite a trained target.
    public OperationResult<int> TrainWithTargetScope(MethodScopeMaterial material)
    {
        if (material.ProjectionVersion != MethodScopeMaterial.OuterBodySyntaxV1)
            return OperationResult<int>.Fail(
                $"Unsupported target scope projection '{material.ProjectionVersion}'.",
                "UNSUPPORTED_TARGET_SCOPE_VERSION");

        var seen = new HashSet<(short, int)>();
        foreach (var method in material.Methods)
        {
            var key = (method.FileId, method.IndexInFile);
            if (!seen.Add(key))
                return OperationResult<int>.Fail(
                    $"Duplicate method key {key}.", "DUPLICATE_METHOD_KEY");

            // An empty projection set is legitimate; a missing one is not.
            if (!material.TargetScopes.ContainsKey(key))
                return OperationResult<int>.Fail(
                    $"No target scope for method {key}.", "TARGET_SCOPE_MISSING_FOR_METHOD");
        }

        var built = new List<(PackedMethod Current, PackedMethod Next)>();
        for (var index = 0; index < material.Methods.Count - 1; index++)
        {
            var current = material.Methods[index];
            var next = material.Methods[index + 1];
            if (current.FileId != next.FileId || next.IndexInFile != current.IndexInFile + 1)
                continue;

            // Only the Next role gets the projected set, and only as a fresh copy. The
            // same method appearing as Current in the following pair keeps its own bag.
            var projected = material.TargetScopes[(next.FileId, next.IndexInFile)];
            built.Add((current, next with
            {
                KindBag = new HashSet<string>(projected, StringComparer.Ordinal)
            }));
        }

        _pairs.Clear();
        _pairs.AddRange(built);
        return OperationResult<int>.Ok(_pairs.Count);
    }

    // Predict the canonical type of the method that should follow the query.
    //
    // FIX-KNN-TIE-01: ровный голос разрешается РАССТОЯНИЕМ, а не написанием метки.
    //
    // Что было неверно. Голос считал только число голосов, а при равенстве решало
    // порядковое сравнение имени метки. Боевой маршрут создаёт этот предсказатель
    // с пятью соседями (`SemanticCompilerFacade.cs`, строка 46) и отвечает начиная
    // с четырёх обученных пар (строка 370), поэтому на наборе «по одной паре на
    // метку» голос выходит РОВНЫМ — по одному голосу на метку, — и побеждает
    // первая по алфавиту. Измерено восемьдесят пятым опытом на боевой настройке:
    // все четыре экзаменационных запроса получили `ErrorHandling`, хотя ближайшая
    // пара у них разная, и старая точность метки упала с четырёх из четырёх при
    // одном соседе до НУЛЯ из четырёх при пяти.
    //
    // Почему это логическая ошибка, а не настройка. У всех четырёх запросов
    // ближайшая пара совпадала с запросом ПОЛНОСТЬЮ — расстояние ноль, — и голос
    // всё равно отдавал ответ чужой метке. Написание метки к запросу отношения не
    // имеет: переименуй `ErrorHandling` в что-нибудь на «Я», и при тех же
    // расстояниях ответ сменился бы. Расстояние — это та самая мера, на которой
    // механизм построен, и до правки при равенстве голосов она выбрасывалась.
    //
    // Что стало. Расстояние считается один раз и несётся дальше вместе с парой.
    // Порядок решения: больше голосов; при равенстве — меньшее расстояние до
    // ближайшего соседа внутри группы; и только при равенстве обоих — прежнее
    // порядковое сравнение имени как последнее средство, чтобы ответ остался
    // определённым.
    //
    // Цена и границы, названные заранее. Вес голоса по расстоянию НЕ вводится:
    // это отдельная непроверенная гипотеза, записанная в реестре предложением
    // карточки опыта. Состав соседей не меняется вовсе. Два соседних
    // предсказателя в этой же папке несут тот же дефект разрешения
    // (`StructuralKnn.cs`, строки 220–221, и `FileActionKnn.cs`, строки 138–139),
    // но измеренных показаний у них нет и боевая достижимость их ровного голоса
    // не проверена, а правка неизмеренного кода запрещена — они записаны
    // отдельной строкой работ.
    public string Predict(PackedMethod query)
    {
        // Пустая обученная память отвечает прежним кодом отсутствия: правка этого
        // не касается, и обученность ничем не подменяется.
        if (_pairs.Count == 0)
            return "Mixed";

        // Расстояние до каждой обученной пары считается ОДИН раз и дальше несётся
        // рядом с самой парой. Прежде оно жило только внутри сортировки и к
        // моменту голосования было потеряно — именно поэтому при равенстве
        // голосов решать было нечем, кроме имени метки.
        var neighbours = _pairs
            .Select(pair => (Pair: pair, Distance: JaccardDistance(query.KindBag, pair.Current.KindBag)))
            // Порядок отбора соседей ПРЕЖНИЙ: по возрастанию расстояния, а при
            // равных расстояниях — порядком вставки, потому что сортировка
            // устойчива. Состав `_k` соседей этой правкой не меняется; как
            // разрешать равные расстояния при отборе — отдельный фактор.
            .OrderBy(item => item.Distance)
            .Take(_k)
            .ToArray();

        // Голос по меткам соседей: одна группа на метку, как и было.
        return neighbours
            .GroupBy(item => CanonicalType(item.Pair.Next.KindBag))
            // Первое правило прежнее и главное: больше голосов — выше.
            .OrderByDescending(group => group.Count())
            // Второе правило новое: при равенстве голосов выше та группа, в
            // которой есть сосед БЛИЖЕ к запросу. Точное совпадение теперь не
            // может проиграть метке с тем же числом голосов.
            .ThenBy(group => group.Min(item => item.Distance))
            // Третье правило — прежнее порядковое сравнение имени, и только как
            // последнее средство: когда равны и голоса, и ближайшие расстояния,
            // механизму выбор безразличен, а определённость ответа нужна.
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
