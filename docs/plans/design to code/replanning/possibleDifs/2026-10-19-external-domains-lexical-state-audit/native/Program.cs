using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SemanticCompiler;
using SemanticCompiler.Completion;
using SemanticCompiler.Prediction;

// Diagnostic artifact. It calls the actual repository implementation.
// No product files, checkpoints, datasets, or provider endpoints are changed.
var cases = new[]
{
    (Id: "url", Source: """class C { string F(string user) { var endpoint = "https://example.test"; return user.Length.ToString(); } }"""),
    (Id: "interpolation", Source: """class C { string F(string user) { return $"{user.Length}"; } }"""),
    (Id: "plain_literal_control", Source: """class C { int F(string user) { var message = "plain"; return user.Length; } }"""),
    (Id: "real_comment_control", Source: """class C { int F(string user) { /* explanation */ return user.Length; } }""")
};
var preprocessing = cases.Select(item =>
{
    var normalized = RegexPreprocessor.Normalize(item.Source);
    var rawTree = CSharpSyntaxTree.ParseText(item.Source);
    var normalizedTree = CSharpSyntaxTree.ParseText(normalized);
    return new
    {
        item.Id,
        item.Source,
        Normalized = normalized,
        RawSyntaxErrors = rawTree.GetDiagnostics().Count(d => d.Severity == DiagnosticSeverity.Error),
        NormalizedSyntaxErrors = normalizedTree.GetDiagnostics().Count(d => d.Severity == DiagnosticSeverity.Error),
        RawMemberAccess = Count(rawTree, SyntaxKind.SimpleMemberAccessExpression),
        NormalizedMemberAccess = Count(normalizedTree, SyntaxKind.SimpleMemberAccessExpression),
        RawInvocation = Count(rawTree, SyntaxKind.InvocationExpression),
        NormalizedInvocation = Count(normalizedTree, SyntaxKind.InvocationExpression),
        RawActionLabel = ActionLabelGenerator.Labels[ActionLabelGenerator.Label(item.Source)],
        NormalizedActionLabel = ActionLabelGenerator.Labels[ActionLabelGenerator.Label(normalized)],
        FeatureCosine = Cosine(ActionLabelGenerator.FeatureVector(item.Source), ActionLabelGenerator.FeatureVector(normalized))
    };
}).ToArray();

// A tiny representation/consumer probe, not an independently judged C# quality test.
// Both arms use exactly the same six raw training sources and fresh facades.
var training = Enumerable.Range(0, 3)
    .Select(i => "class Member" + i + """ { string F(string user) { return $"{user.Length}"; } }""")
    .Concat(Enumerable.Range(0, 3)
        .Select(i => "class Literal" + i + """ { string F(string user) { return "fixed"; } }"""))
    .ToArray();
var source = """class Unseen { string F(string account) { return $"{account.Length}"; } }""";
var constControl = """class Control { string F(string account) { return "fixed"; } }""";
var off = RunFacade(false, training, source, constControl);
var on = RunFacade(true, training, source, constControl);

var ordinary = MethodFrequencyTable.ExtractMethodCalls(
    "class C { void F() { cache.Add(1); cache.Add(2); } }");
var conditional = MethodFrequencyTable.ExtractMethodCalls(
    "class C { void F() { cache?.Add(1); cache?.Add(2); } }");
var conditionalTable = new MethodFrequencyTable();
conditionalTable.Train(["class C { void F() { cache?.Add(1); cache?.Add(2); } }"]);
var ordinaryTable = new MethodFrequencyTable();
ordinaryTable.Train(["class C { void F() { cache.Add(1); cache.Add(2); } }"]);

var caseTable = new MethodFrequencyTable();
caseTable.Train(["class C { void F() { service.Read(); service.Read(); service.Read(); service.Read(); service.Read(); service.read(); } }"]);
var receiverCalls = MethodFrequencyTable.ExtractMethodCalls(
    "class C { void F() { db.Users.Add(1); db.Log.Write(1); } }");
var report = new
{
    SourceRef = Environment.GetEnvironmentVariable("KNN_AUDIT_SOURCE_REF") ?? "UNSPECIFIED",
    Scope = "Actual C# implementation diagnostics and a tiny same-source feature-recovery probe",
    TeacherCalled = false,
    CodexIndependentJudgeCalled = false,
    DpoRun = false,
    SemanticQualityMeasured = false,
    Preprocessing = preprocessing,
    Facade = new { PreprocessingOff = off, PreprocessingOn = on },
    CompletionObservation = new
    {
        OrdinaryCalls = ordinary.Value?.Select(x => new { x.Receiver, x.Method }),
        ConditionalCalls = conditional.Value?.Select(x => new { x.Receiver, x.Method }),
        OrdinaryTotalPairs = ordinaryTable.TotalPairs,
        ConditionalTotalPairs = conditionalTable.TotalPairs,
        OrdinaryRank = ordinaryTable.RankMethods("cache", ["Other", "Add"]),
        ConditionalRank = conditionalTable.RankMethods("cache", ["Other", "Add"]),
        CaseDistinctRankRareFirst = caseTable.RankMethods("service", ["read", "Read"]),
        CaseDistinctRankFrequentFirst = caseTable.RankMethods("service", ["Read", "read"]),
        QualifiedReceivers = receiverCalls.Value?.Select(x => new { x.Receiver, x.Method })
    }
};
Console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

static int Count(SyntaxTree tree, SyntaxKind kind) =>
    tree.GetRoot().DescendantNodes().Count(node => node.Kind() == kind);

static double Cosine(float[] left, float[] right)
{
    double dot = 0, l2 = 0, r2 = 0;
    for (var i = 0; i < Math.Min(left.Length, right.Length); i++)
    {
        dot += (double)left[i] * right[i];
        l2 += (double)left[i] * left[i];
        r2 += (double)right[i] * right[i];
    }
    return l2 == 0 || r2 == 0 ? 0 : dot / Math.Sqrt(l2 * r2);
}

static object RunFacade(bool preprocess, string[] training, string query, string control)
{
    var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions
    {
        UseRegexPreprocessor = preprocess,
        UseFileActionKnn = true,
        UseMethodKnn = false,
        UseStructuralKnn = false
    });
    facade.TrainFileActionKnn(training);
    var result = facade.Compile(query);
    var controlResult = facade.Compile(control);
    return new
    {
        QuerySucceeded = result.IsSuccess,
        QueryError = result.ErrorMessage,
        QueryStack = result.Value?.Stack.ToString(),
        CSharpQueryDetected = result.Value?.Stack == StackLabel.CSharp,
        QueryAction = result.Value?.PredictedFileAction,
        ExpectedQueryAction = preprocess ? "ADD_METHOD" : "ADD_NULL_CHECK",
        QueryExpectationMatched = result.Value?.PredictedFileAction == (preprocess ? "ADD_METHOD" : "ADD_NULL_CHECK"),
        ControlSucceeded = controlResult.IsSuccess,
        ControlError = controlResult.ErrorMessage,
        ControlStack = controlResult.Value?.Stack.ToString(),
        ControlAction = controlResult.Value?.PredictedFileAction,
        ExpectedControlAction = "ADD_METHOD",
        ControlExpectationMatched = controlResult.Value?.PredictedFileAction == "ADD_METHOD"
    };
}
