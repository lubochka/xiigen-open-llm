// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Diagnostics;
using AgentInfrastructure;
using ModelServer.Contracts;
using ModelServer.Remote;

public sealed class CodeDocumentationTrainingRunner
{
    private readonly RoutingProfileTrainer _profileTrainer;

    public CodeDocumentationTrainingRunner(RoutingProfileTrainer? profileTrainer = null)
    {
        _profileTrainer = profileTrainer ?? new RoutingProfileTrainer();
    }

    public async Task<OperationResult<CodeDocumentationTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<CodeDocumentationTrainingExample> trainingExamples,
        IReadOnlyList<CodeDocumentationTrainingExample> evaluationExamples,
        CancellationToken ct = default)
    {
        if (trainingExamples is null)
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Training examples are required.", "TRAINING_EXAMPLES_REQUIRED");

        if (evaluationExamples is null)
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Evaluation examples are required.", "EVALUATION_EXAMPLES_REQUIRED");

        if (trainingExamples.Count != 50)
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Code documentation training batches must contain exactly 50 examples.", "TRAINING_BATCH_SIZE_NOT_50");

        if (evaluationExamples.Count != 50)
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Code documentation held-out batches must contain exactly 50 examples.", "HELDOUT_BATCH_SIZE_NOT_50");

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);

        var routingTrainingExamples = trainingExamples.Select(ToRoutingTrainingExample).ToArray();
        var profiles = _profileTrainer.Train(routingTrainingExamples);
        if (!profiles.IsSuccess)
            return profiles.MapError<CodeDocumentationTrainingRunResult>();

        var runtime = await BuildRuntimeAsync(profiles.Value!, ct);
        if (!runtime.IsSuccess)
            return runtime.MapError<CodeDocumentationTrainingRunResult>();

        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);
        var cases = new List<CodeDocumentationTrainingCaseResult>();
        var decisions = new List<RoutingDecisionRecord>();

        foreach (var example in evaluationExamples)
        {
            if (ct.IsCancellationRequested)
                return OperationResult<CodeDocumentationTrainingRunResult>.Fail("Code documentation training was cancelled.", "CODE_DOC_TRAINING_CANCELLED");

            var routed = await flow.RouteDocumentationAsync(example.Request, ct);
            if (!routed.IsSuccess)
                return routed.MapError<CodeDocumentationTrainingRunResult>();

            var value = routed.Value!;
            var routedCorrect = string.Equals(value.ChosenUnitId, example.ExpectedUnitId, StringComparison.Ordinal);
            var sourceCorrect = value.SourceGate.IsPassed == example.ExpectSourceGatePass;
            var versionCorrect = value.VersionGate.IsPassed == example.ExpectVersionGatePass;
            var transitCorrect = value.TransitGate.IsPassed == example.ExpectTransitGatePass;
            cases.Add(new CodeDocumentationTrainingCaseResult(
                example.ExampleId,
                example.ExpectedUnitId,
                value.ChosenUnitId,
                routedCorrect,
                sourceCorrect,
                versionCorrect,
                transitCorrect,
                value.SourceGate.Score,
                value.VersionGate.Score,
                value.TransitGate.BottomUpOrderScore,
                value.TransitGate.CalleeTransitScore,
                value.TransitGate.LocalVsDelegatedScore,
                value.RoutingConfidence,
                value.Verdict));

            decisions.Add(new RoutingDecisionRecord(
                $"code-doc-eval-{example.ExampleId}",
                example.ExampleId,
                BuildPrompt(example.Request),
                BuildTaskSlice(example.Request),
                example.ExpectedUnitId,
                routedCorrect ? 1 : 0,
                BuildRejectedCandidates(example.ExpectedUnitId),
                example.CorrectReason,
                example.WrongRouteConsequence,
                DateTimeOffset.UtcNow));

            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
        }

        var pairs = new RoutingDpoPairBuilder().BuildPairs(decisions);
        if (!pairs.IsSuccess)
            return pairs.MapError<CodeDocumentationTrainingRunResult>();

        process.Refresh();
        stopwatch.Stop();
        var cpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
        var routedCount = cases.Count(item => item.RoutedCorrect);
        var sourceCount = cases.Count(item => item.SourceGateCorrect);
        var versionCount = cases.Count(item => item.VersionGateCorrect);
        var transitCount = cases.Count(item => item.TransitGateCorrect);

        var result = new CodeDocumentationTrainingRunResult(
            trainingExamples.Count,
            evaluationExamples.Count,
            routedCount,
            sourceCount,
            versionCount,
            transitCount,
            routedCount / (double)evaluationExamples.Count,
            sourceCount / (double)evaluationExamples.Count,
            versionCount / (double)evaluationExamples.Count,
            transitCount / (double)evaluationExamples.Count,
            Average(cases.Select(item => item.BottomUpOrderScore)),
            Average(cases.Select(item => item.CalleeTransitScore)),
            Average(cases.Select(item => item.LocalVsDelegatedScore)),
            cpuMs,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            pairs.Value!,
            cases.ToArray());

        return OperationResult<CodeDocumentationTrainingRunResult>.Ok(result);
    }

    private static RoutingTrainingExample ToRoutingTrainingExample(CodeDocumentationTrainingExample example)
        => new(
            example.ExampleId,
            BuildPrompt(example.Request),
            BuildTaskSlice(example.Request),
            null,
            "documentation",
            [
                OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
                OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
            ],
            example.ExpectedUnitId,
            OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
            example.CorrectReason,
            example.WrongRouteConsequence);

    private static async Task<OperationResult<DistributedRoutingRuntime>> BuildRuntimeAsync(
        IReadOnlyDictionary<string, UnitScoringProfile> profiles,
        CancellationToken ct)
    {
        var locator = new InMemoryRemoteUnitLocator();
        foreach (var manifest in BuildDefaultManifests("https://models.local"))
        {
            var registered = await locator.RegisterAsync(manifest, ct);
            if (!registered.IsSuccess)
                return registered.MapError<DistributedRoutingRuntime>();
        }

        var flow = new TrainableRoutingFlow(
            new KeywordUnitScorer(profiles),
            new InMemoryRoutingDecisionLog());

        return OperationResult<DistributedRoutingRuntime>.Ok(new DistributedRoutingRuntime(flow, locator, profiles));
    }

    private static RemoteUnitManifest[] BuildDefaultManifests(string serverBaseUrl)
        =>
        [
            BuildManifest(
                OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                "orchestration-planning-unit",
                "orchestration",
                "v309",
                serverBaseUrl,
                "/models/unit-orchestration-plan-v309",
                "manifest-orchestration-v309",
                "orchestration/v309-plan-routing",
                OrchestrationRoutingCurriculum.V309TrainingRunId,
                OrchestrationRoutingCurriculum.V309DatasetSha256,
                "server://tokenizers/orchestration-v309/tokenizer.json"),
            BuildManifest(
                OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
                "translation-unit",
                "translation",
                "v1",
                serverBaseUrl,
                "/models/unit-ru-en-general",
                "manifest-translation-ru-en-v1",
                "translation/ru-en-general",
                "translation-placeholder-run",
                "translation-placeholder-dataset",
                "server://tokenizers/translation-general/tokenizer.json"),
            BuildManifest(
                OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                "documentation-unit",
                "documentation",
                "v1",
                serverBaseUrl,
                "/models/unit-dotnet-legacy-doc-l1",
                "manifest-dotnet-doc-v1",
                "documentation/dotnet-legacy-l1",
                "documentation-runner-adapter-v1",
                "documentation-runner-adapter-dataset",
                "server://tokenizers/dotnet-doc/tokenizer.json")
        ];

    private static RemoteUnitManifest BuildManifest(
        string unitId,
        string unitRole,
        string domainTag,
        string version,
        string serverBaseUrl,
        string endpoint,
        string manifestSha256,
        string trainingBranch,
        string trainingRunId,
        string sourceDatasetSha256,
        string tokenizerPath)
        => new(
            unitId,
            unitRole,
            domainTag,
            version,
            serverBaseUrl,
            endpoint,
            manifestSha256,
            trainingBranch,
            trainingRunId,
            sourceDatasetSha256,
            new RemoteModelConfig(2048, 96, 64, 2, 4, 4, 128),
            new RemoteLoraConfig(2, 4f, 0.05f, ["q_proj", "v_proj"]),
            tokenizerPath,
            DateTimeOffset.UtcNow);

    private static UnitScoringResult[] BuildRejectedCandidates(string expectedUnitId)
        =>
        [
            new UnitScoringResult(
                expectedUnitId == OrchestrationRoutingCurriculum.OrchestrationV309UnitId
                    ? OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
                    : OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                "wrong-specialist-unit",
                "wrong-domain",
                0,
                0.01)
        ];

    private static string BuildPrompt(CodeDocumentationRequest request)
        => $"Document legacy C# code bottom-up for {request.StackLabel} {request.TargetFramework} {request.CSharpLanguageVersion}. Symbol: {request.SymbolId}. Level: {request.DocumentationLevel}.";

    private static string BuildTaskSlice(CodeDocumentationRequest request)
        => string.Join(
            " ",
            [
                "documentation",
                "legacy dotnet code documentation",
                "bottom-up callee caller propagation",
                request.AppSide,
                request.RuntimeFamily,
                request.TargetFramework,
                request.CSharpLanguageVersion,
                request.WebFramework,
                request.FrontendFramework,
                request.StackLabel,
                request.ArtifactKind,
                request.DocumentationLevel
            ]);

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}

public sealed record CodeDocumentationTrainingExample(
    string ExampleId,
    CodeDocumentationRequest Request,
    string ExpectedUnitId,
    bool ExpectVersionGatePass,
    bool ExpectTransitGatePass,
    string CorrectReason,
    string WrongRouteConsequence,
    bool ExpectSourceGatePass = true);

public sealed record CodeDocumentationTrainingRunResult(
    int TrainingExampleCount,
    int EvaluationExampleCount,
    int RoutedCorrect,
    int SourceCorrect,
    int VersionCorrect,
    int TransitCorrect,
    double RoutingAccuracy,
    double SourceGateAccuracy,
    double VersionGateAccuracy,
    double TransitGateAccuracy,
    double BottomUpOrderAccuracy,
    double CalleeSummaryTransitAccuracy,
    double LocalVsDelegatedAccuracy,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    RoutingDpoPair[] DpoPairs,
    CodeDocumentationTrainingCaseResult[] Cases);

public sealed record CodeDocumentationTrainingCaseResult(
    string ExampleId,
    string ExpectedUnitId,
    string ActualUnitId,
    bool RoutedCorrect,
    bool SourceGateCorrect,
    bool VersionGateCorrect,
    bool TransitGateCorrect,
    double SourceScore,
    double VersionScore,
    double BottomUpOrderScore,
    double CalleeTransitScore,
    double LocalVsDelegatedScore,
    double RoutingConfidence,
    string Verdict);
