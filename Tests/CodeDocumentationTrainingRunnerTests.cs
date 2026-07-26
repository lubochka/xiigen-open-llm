// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text.Json;
using TrainableRouter.Training;

public sealed class CodeDocumentationTrainingRunnerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RequiresExactlyFiftyTrainAndHeldOutRows()
    {
        var runner = new CodeDocumentationTrainingRunner();
        var examples = BuildExamples(49, "train");
        var heldOut = BuildExamples(50, "heldout");

        var result = await runner.TrainAndEvaluateAsync(examples, heldOut);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_BATCH_SIZE_NOT_50", result.ErrorCode);
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_TrainsRoutesAndMeasuresFiftyHeldOutRows()
    {
        var runner = new CodeDocumentationTrainingRunner();
        var training = BuildExamples(50, "train");
        var heldOut = BuildExamples(50, "heldout");

        var result = await runner.TrainAndEvaluateAsync(training, heldOut);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingExampleCount);
        Assert.Equal(50, result.Value.EvaluationExampleCount);
        Assert.Equal(50, result.Value.RoutedCorrect);
        Assert.Equal(50, result.Value.SourceCorrect);
        Assert.Equal(50, result.Value.VersionCorrect);
        Assert.Equal(50, result.Value.TransitCorrect);
        Assert.Equal(1.0, result.Value.RoutingAccuracy);
        Assert.Equal(1.0, result.Value.SourceGateAccuracy);
        Assert.Equal(1.0, result.Value.VersionGateAccuracy);
        Assert.Equal(1.0, result.Value.TransitGateAccuracy);
        Assert.Equal(1.0, result.Value.BottomUpOrderAccuracy);
        Assert.Equal(1.0, result.Value.CalleeSummaryTransitAccuracy);
        Assert.Equal(1.0, result.Value.LocalVsDelegatedAccuracy);
        Assert.True(result.Value.CpuMilliseconds >= 0);
        Assert.True(result.Value.ElapsedMilliseconds >= 0);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
        Assert.Equal(50, result.Value.DpoPairs.Length);
        Assert.All(result.Value.DpoPairs, pair => Assert.Contains("[WRONG:", pair.Rejected));
        Assert.All(result.Value.Cases, item => Assert.Equal(OrchestrationRoutingCurriculum.DocumentationDotNetUnitId, item.ActualUnitId));
    }

    [Fact]
    public async Task CodeDocumentationBatchFileRunner_ReadsJsonlAndRunsFiftyTrainFiftyHeldOutRows()
    {
        var root = Path.Combine(Path.GetTempPath(), $"code-doc-batch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var trainPath = Path.Combine(root, "TRAIN-PAIRS.jsonl");
        var heldOutPath = Path.Combine(root, "HELDOUT-ROWS.jsonl");
        await File.WriteAllLinesAsync(trainPath, BuildRows(50, "file-train").Select(row => JsonSerializer.Serialize(row, JsonOptions)));
        await File.WriteAllLinesAsync(heldOutPath, BuildRows(50, "file-heldout").Select(row => JsonSerializer.Serialize(row, JsonOptions)));

        var result = await new CodeDocumentationBatchFileRunner().RunAsync(trainPath, heldOutPath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingExampleCount);
        Assert.Equal(50, result.Value.EvaluationExampleCount);
        Assert.Equal(1.0, result.Value.RoutingAccuracy);
        Assert.Equal(1.0, result.Value.SourceGateAccuracy);
        Assert.Equal(1.0, result.Value.VersionGateAccuracy);
        Assert.Equal(1.0, result.Value.TransitGateAccuracy);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
    }

    [Fact]
    public async Task CodeDocumentationBatchFileRunner_RejectsRowsWithoutWrongAnnotation()
    {
        var root = Path.Combine(Path.GetTempPath(), $"code-doc-batch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var trainPath = Path.Combine(root, "TRAIN-PAIRS.jsonl");
        var heldOutPath = Path.Combine(root, "HELDOUT-ROWS.jsonl");
        var trainRows = BuildRows(50, "bad-train");
        trainRows[0] = trainRows[0] with { Rejected = "Route to orchestration instead." };
        await File.WriteAllLinesAsync(trainPath, trainRows.Select(row => JsonSerializer.Serialize(row, JsonOptions)));
        await File.WriteAllLinesAsync(heldOutPath, BuildRows(50, "bad-heldout").Select(row => JsonSerializer.Serialize(row, JsonOptions)));

        var result = await new CodeDocumentationBatchFileRunner().RunAsync(trainPath, heldOutPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_WRONG_ANNOTATION", result.ErrorCode);
    }

    private static CodeDocumentationTrainingExample[] BuildExamples(int count, string prefix)
        => Enumerable.Range(0, count)
            .Select(index =>
            {
                var request = index % 2 == 0
                    ? BuildNetCoreRequest($"{prefix}-netcore-{index:000}", index)
                    : BuildNetFrameworkRequest($"{prefix}-netfx-{index:000}", index);
                return new CodeDocumentationTrainingExample(
                    $"{prefix}-code-doc-{index:000}",
                    request,
                    OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                    true,
                    true,
                    "The request asks for bottom-up legacy .NET code documentation with version-aware routing.",
                    "the orchestration or translation unit cannot document legacy C# call transit, framework version, or caller/callee summaries.");
            })
            .ToArray();

    private static CodeDocumentationBatchRow[] BuildRows(int count, string prefix)
        => BuildExamples(count, prefix)
            .Select(example => ToRow(example))
            .ToArray();

    private static CodeDocumentationBatchRow ToRow(CodeDocumentationTrainingExample example)
        => new()
        {
            ExampleId = example.ExampleId,
            SourceId = example.Request.SourceId,
            Repository = example.Request.Repository,
            CommitSha = example.Request.CommitSha,
            FilePath = example.Request.FilePath,
            SymbolId = example.Request.SymbolId,
            CodeText = example.Request.CodeText,
            Calls = example.Request.Calls.ToArray(),
            KnownCallees = example.Request.KnownCallees.ToArray(),
            AppSide = example.Request.AppSide,
            RuntimeFamily = example.Request.RuntimeFamily,
            TargetFramework = example.Request.TargetFramework,
            CSharpLanguageVersion = example.Request.CSharpLanguageVersion,
            LanguageVersionConfidence = example.Request.LanguageVersionConfidence,
            ProjectStyle = example.Request.ProjectStyle,
            WebFramework = example.Request.WebFramework,
            WebFrameworkVersion = example.Request.WebFrameworkVersion,
            FrontendFramework = example.Request.FrontendFramework,
            FrontendFrameworkVersion = example.Request.FrontendFrameworkVersion,
            StackLabel = example.Request.StackLabel,
            ArtifactKind = example.Request.ArtifactKind,
            DocumentationLevel = example.Request.DocumentationLevel,
            Chosen = "Route to documentation and preserve bottom-up caller/callee evidence.",
            Rejected = "Route to orchestration and skip code documentation transit. [WRONG: wrong specialist - the rejected route cannot learn framework version or callee-summary propagation - callers would be documented without leaf-method evidence.]",
            ExpectedUnitId = example.ExpectedUnitId,
            ExpectVersionGatePass = example.ExpectVersionGatePass,
            ExpectTransitGatePass = example.ExpectTransitGatePass,
            CorrectReason = example.CorrectReason,
            WrongRouteConsequence = example.WrongRouteConsequence
        };

    private static CodeDocumentationRequest BuildNetCoreRequest(string requestId, int index)
        => new(
            requestId,
            "local-wizGameserver-static-eval",
            @"C:\sandbox\wizGameserver",
            "local",
            @"WebApplication2\Controllers\DynamicController.cs",
            $"DynamicController.StoreDocument_{index}",
            "public ActionResult StoreDocument(string index_name, string id, object document) { var myDocument = ParseObjectAlternative(document); return Ok(myDocument); }",
            [
                new CodeDocumentationCallEdge(
                    $"DynamicController.StoreDocument_{index}",
                    "DynamicController.ParseObjectAlternative",
                    "direct_method_call")
            ],
            [
                new CodeDocumentationCalleeSummary(
                    "DynamicController.ParseObjectAlternative",
                    "Recursively normalizes JSON object and array values before the controller indexes the parsed document.",
                    ["DynamicController.cs:204"])
            ],
            "server",
            "NET_CORE",
            "netcoreapp3.1",
            "CSharp8",
            "inferred_high",
            "sdk_style",
            "ASP.NET Core WebApi",
            "Microsoft.AspNetCore.Authentication.JwtBearer 3.1.8",
            "none",
            "none",
            "NETCORE_WEBAPI",
            "controller_action",
            "D3");

    private static CodeDocumentationRequest BuildNetFrameworkRequest(string requestId, int index)
        => new(
            requestId,
            "local-genietasks-static-eval",
            @"C:\sandbox\genietasks",
            "local",
            @"A1ProjectManagement\AuthController.cs",
            $"AuthController.Authenticate_{index}",
            "public UserBack Authenticate(LoginDetails user) { string passMD5 = user.password.ConvertToMD5(); return CreateBackUser(authUser); }",
            [
                new CodeDocumentationCallEdge($"AuthController.Authenticate_{index}", "HelpAuthClass.ConvertToMD5", "extension_method_call"),
                new CodeDocumentationCallEdge($"AuthController.Authenticate_{index}", "AuthController.CreateBackUser", "direct_method_call"),
                new CodeDocumentationCallEdge("UserBack.UserBack(Member)", "JwtManager.GenerateToken", "direct_method_call")
            ],
            [
                new CodeDocumentationCalleeSummary(
                    "HelpAuthClass.ConvertToMD5",
                    "Hashes the supplied password string with MD5 and returns a lowercase hexadecimal digest.",
                    ["AuthController.cs:114"]),
                new CodeDocumentationCalleeSummary(
                    "JwtManager.GenerateToken",
                    "Creates a JWT token with legacy claim fields from the UserBack wrapper.",
                    ["JwtManager.cs:12"]),
                new CodeDocumentationCalleeSummary(
                    "AuthController.CreateBackUser",
                    "Creates UserBack from Member and delegates token issuance to the UserBack constructor.",
                    ["AuthController.cs:211"])
            ],
            "mixed",
            "NET_FRAMEWORK",
            "net46",
            "CSharp5_or_CSharp6_review_required",
            "inferred_review_required",
            "legacy_non_sdk",
            "WebApi2 plus ASMX",
            "System.Web.Http 5.2.3 / Breeze.WebApi2 1.6.5",
            "AngularJS",
            "1.4.8",
            "NETFX_WEBAPI",
            "auth_workflow",
            "D3");
}
