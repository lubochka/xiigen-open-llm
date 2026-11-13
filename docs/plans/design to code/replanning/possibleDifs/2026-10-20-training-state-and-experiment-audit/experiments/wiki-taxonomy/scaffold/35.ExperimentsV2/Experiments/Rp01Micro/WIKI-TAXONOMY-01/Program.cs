// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// Source-copy experiment harness; it invokes the native classifier and its training reader.
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectWikiDomain;
using AModel = PossibleDiffs.Baseline.ProjectWikiRequestRoutingTrainableModel;
using BModel = PossibleDiffs.Candidate.ProjectWikiRequestRoutingTrainableModel;

internal static class Program
{
    private const string ExperimentId = "WIKI-TAXONOMY-01";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static int Main(string[] args)
    {
        if (args.Length != 4 || args[0] != "--arm" || args[2] != "--output" || args[1] is not ("A" or "B"))
            throw new ArgumentException("Usage: --arm A|B --output ABSOLUTE_NEW_DIRECTORY");
        var arm = args[1];
        if (!Path.IsPathFullyQualified(args[3]))
            throw new ArgumentException("Output must be an absolute path outside the protected core experiment folder.");
        var output = Path.GetFullPath(args[3]);
        if (output.Replace('\\', '/').Contains("/docs/plans/replanning-possible-diffs/", StringComparison.Ordinal))
            throw new ArgumentException("The protected core experiment folder is not an allowed output location.");
        if (Directory.Exists(output))
            throw new IOException("Append-only execution requires a new output directory; existing results must not be replaced.");

        var fixtureBytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "fixture.json"));
        var fixture = JsonSerializer.Deserialize<Fixture>(fixtureBytes, JsonOptions)
            ?? throw new InvalidDataException("Fixture is missing.");
        if (fixture.ExperimentId != ExperimentId)
            throw new InvalidDataException("This literal-ID host received another experiment's fixture.");
        if (fixture.Train.Length != 4 || fixture.Exam.Length != 6)
            throw new InvalidDataException("Frozen microexperiment row budget changed.");
        if (fixture.Train.Concat(fixture.Exam).Select(r => r.Id).Distinct(StringComparer.Ordinal).Count() != fixture.Train.Length + fixture.Exam.Length)
            throw new InvalidDataException("Train/exam IDs overlap.");
        if (fixture.Train.Select(r => r.Prompt).Intersect(fixture.Exam.Select(r => r.Prompt), StringComparer.Ordinal).Any())
            throw new InvalidDataException("Exact train/exam prompt leakage.");

        // Gold labels stay in the harness. The native reader receives only the prompt.
        var rows = fixture.Train.Select(r => new ProjectWikiRequestRoutingDpoPair(
            r.Id, "isolated-project-wiki-route", r.Prompt, r.ExpectedRouteKind, r.Prompt,
            $"route_kind: {r.ExpectedRouteKind}\ntarget_unit_id: {r.ExpectedTargetUnitId}",
            $"[WRONG: wrong_training_unit]\nroute_kind: {(r.ExpectedRouteKind == ProjectWikiRequestRouteKind.NotProjectWiki ? ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining : ProjectWikiRequestRouteKind.NotProjectWiki)}",
            "wrong_training_unit")).ToArray();

        // Native constructor is private. Empty Train creates its untrained, zero state.
        // It presents zero rows and is excluded from the nonempty training cost below.
        var initial = Train(arm, []);
        var beforeSnapshot = initial.Snapshot();
        var beforeStateHash = StateHash(beforeSnapshot);
        var before = Evaluate(initial, fixture.Exam);
        if (beforeStateHash != StateHash(initial.Snapshot()))
            throw new InvalidOperationException("Before evaluation changed learned state.");

        var process = Process.GetCurrentProcess();
        var cpuBefore = process.TotalProcessorTime;
        var managedHeapBefore = GC.GetTotalMemory(false);
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var timer = Stopwatch.StartNew();
        var trained = Train(arm, rows);
        timer.Stop();
        var allocatedDuringTrain = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        var managedHeapAfter = GC.GetTotalMemory(false);
        process.Refresh();
        var cpuDuringTrain = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;

        var afterSnapshot = trained.Snapshot();
        var afterStateHash = StateHash(afterSnapshot);
        var after = Evaluate(trained, fixture.Exam);
        if (afterStateHash != StateHash(trained.Snapshot()))
            throw new InvalidOperationException("After evaluation changed learned state.");
        if (trained.Presentations != rows.Length * 18)
            throw new InvalidOperationException("Native epoch/presentation budget changed.");

        // Gate evidence remains the native missing-gate value. This host cannot publish a model.
        if (afterSnapshot.PublishAllowed || afterSnapshot.GatePassed || afterSnapshot.SemanticQualityClaimed)
            throw new InvalidOperationException("The microexperiment must not fabricate production eligibility.");

        var beforeMetrics = Summarize(before);
        var afterMetrics = Summarize(after);
        var snapshotBytes = JsonSerializer.SerializeToUtf8Bytes(afterSnapshot, JsonOptions);
        var report = new
        {
            experimentId = ExperimentId,
            arm,
            runtimeStatus = "MEASURED_NATIVE_SOURCE_COPY",
            coreCommit = "62bcc8f2368840e22d2ea98ef16d9bd526ad9a39",
            fixtureSha256 = Convert.ToHexString(SHA256.HashData(fixtureBytes)).ToLowerInvariant(),
            executionUtc = DateTimeOffset.UtcNow,
            nativeConsumerReference = "ProjectWikiRequestRoutingTrainingRunner -> RouteForEvaluation (source call chain; the host directly calls the same learner/reader)",
            scope = "local fresh classifier training and native held-out reader; no product routing or neural DPO claim",
            framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            observedTrainingRows = rows.Length,
            observedExamRows = fixture.Exam.Length,
            nativeEpochCount = 18,
            rawTrainCalls = 2,
            nonemptyTrainCalls = 1,
            rowPresentations = trained.Presentations,
            modelUpdateCount = trained.Updates,
            evaluationReaderCalls = fixture.Exam.Length * 2,
            initialState = new
            {
                beforeSnapshot.Labels,
                beforeSnapshot.Biases,
                featureCount = beforeSnapshot.TokenWeights.Count,
                allZero = beforeSnapshot.Biases.All(x => x == 0) && beforeSnapshot.TokenWeights.Values.All(v => v.All(x => x == 0)),
                stateSha256 = beforeStateHash
            },
            trainedState = new
            {
                afterSnapshot.Labels,
                featureCount = afterSnapshot.TokenWeights.Count,
                parameterCount = afterSnapshot.Biases.Length + afterSnapshot.TokenWeights.Values.Sum(v => v.Length),
                nonzeroParameterCount = afterSnapshot.Biases.Count(x => x != 0) + afterSnapshot.TokenWeights.Values.Sum(v => v.Count(x => x != 0)),
                stateSha256 = afterStateHash,
                stateChanged = beforeStateHash != afterStateHash,
                snapshotBytes = snapshotBytes.Length,
                snapshotMetadataTrainingRowCount = afterSnapshot.TrainingRowCount,
                snapshotMetadataExplanation = "Native missing-gate metadata stays zero; actual training row count is observedTrainingRows.",
                publishAllowed = afterSnapshot.PublishAllowed
            },
            costs = new
            {
                nonemptyTrainWallMilliseconds = timer.Elapsed.TotalMilliseconds,
                nonemptyTrainElapsedTicks = timer.ElapsedTicks,
                stopwatchFrequency = Stopwatch.Frequency,
                nonemptyTrainCpuMilliseconds = cpuDuringTrain,
                nonemptyTrainAllocatedBytesCurrentThread = allocatedDuringTrain,
                managedHeapBefore,
                managedHeapAfter,
                wholeProcessPeakWorkingSetBytes = process.PeakWorkingSet64,
                limitation = "One short fresh process per arm; timing includes cold helper/JIT costs and is noisy. PeakWorkingSet covers the process, not the training phase."
            },
            before = new { metrics = beforeMetrics, rows = before },
            after = new { metrics = afterMetrics, rows = after },
            deltaAfterMinusBefore = Difference(beforeMetrics, afterMetrics),
            groups = fixture.Exam.Select(r => r.Group).Distinct(StringComparer.Ordinal).Select(group => new
            {
                group,
                before = Summarize(before.Where(r => r.Group == group).ToArray()),
                after = Summarize(after.Where(r => r.Group == group).ToArray())
            }).ToArray(),
            successRule = "Any measured improvement in a useful metric counts as success for that metric and this domain. Accuracy growth is not required. Regressions must be reported beside improvements. Mechanism-only counters and missing-gate confidence are diagnostic, not standalone useful-quality gains.",
            productionEligibility = "NOT_EVALUATED_NOT_PUBLISHABLE"
        };

        Directory.CreateDirectory(output);
        File.WriteAllBytes(Path.Combine(output, "fixture.json"), fixtureBytes);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "source-provenance.json"), Path.Combine(output, "source-provenance.json"));
        File.WriteAllText(Path.Combine(output, "before-snapshot.json"), JsonSerializer.Serialize(beforeSnapshot, JsonOptions));
        File.WriteAllBytes(Path.Combine(output, "after-snapshot.json"), snapshotBytes);
        File.WriteAllText(Path.Combine(output, "report.json"), JsonSerializer.Serialize(report, JsonOptions));
        Console.WriteLine(Path.Combine(output, "report.json"));
        return 0;
    }

    private static IModel Train(string arm, IReadOnlyList<ProjectWikiRequestRoutingDpoPair> rows)
        => arm == "A" ? new BaselineAdapter(AModel.Train(rows)) : new CandidateAdapter(BModel.Train(rows));

    private static ExamDecision[] Evaluate(IModel model, FixtureRow[] rows)
        => rows.Select(row =>
        {
            var actual = model.Decide(row.Prompt);
            var selected = actual.DecisionStatus == ProjectWikiRequestRoutingDecisionStatus.Selected;
            return new ExamDecision(row.Id, row.Group, row.Prompt, row.ExpectedRouteKind, row.ExpectedTargetUnitId,
                actual.RouteKind, actual.TargetUnitId, actual.DecisionStatus, actual.Confidence, actual.TopTwoMargin,
                actual.Warnings, selected && actual.RouteKind == row.ExpectedRouteKind && actual.TargetUnitId == row.ExpectedTargetUnitId,
                selected && (actual.RouteKind != row.ExpectedRouteKind || actual.TargetUnitId != row.ExpectedTargetUnitId),
                !selected, actual.RouteKind == row.ExpectedRouteKind);
        }).ToArray();

    private static Metrics Summarize(ExamDecision[] rows)
    {
        var n = rows.Length;
        return new Metrics(n, rows.Count(r => r.CorrectSelected), rows.Count(r => r.WrongSelected), rows.Count(r => r.Deferred),
            n == 0 ? 0 : rows.Count(r => r.CorrectSelected) / (double)n,
            n == 0 ? 0 : rows.Count(r => r.WrongSelected) / (double)n,
            n == 0 ? 0 : rows.Count(r => r.Deferred) / (double)n,
            rows.Count(r => r.RouteLabelMatchesIgnoringStatus));
    }

    private static object Difference(Metrics before, Metrics after) => new
    {
        correctSelected = after.CorrectSelected - before.CorrectSelected,
        wrongSelected = after.WrongSelected - before.WrongSelected,
        deferred = after.Deferred - before.Deferred,
        correctSelectedRate = after.CorrectSelectedRate - before.CorrectSelectedRate,
        wrongSelectedRate = after.WrongSelectedRate - before.WrongSelectedRate,
        deferredRate = after.DeferredRate - before.DeferredRate
    };

    private static string StateHash(ProjectWikiRequestRoutingModelSnapshot state)
        => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        {
            state.Labels,
            state.Biases,
            tokenWeights = state.TokenWeights.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new { token = p.Key, weights = p.Value })
        }, JsonOptions))).ToLowerInvariant();

    private interface IModel
    {
        ProjectWikiRequestRoutingDecision Decide(string prompt);
        ProjectWikiRequestRoutingModelSnapshot Snapshot();
        int Presentations { get; }
        int Updates { get; }
    }

    private sealed class BaselineAdapter(AModel model) : IModel
    {
        public ProjectWikiRequestRoutingDecision Decide(string prompt) => model.RouteForEvaluation(prompt);
        public ProjectWikiRequestRoutingModelSnapshot Snapshot() => model.ToSnapshot();
        public int Presentations => model.AuditPresentationCount;
        public int Updates => model.AuditModelUpdateCount;
    }
    private sealed class CandidateAdapter(BModel model) : IModel
    {
        public ProjectWikiRequestRoutingDecision Decide(string prompt) => model.RouteForEvaluation(prompt);
        public ProjectWikiRequestRoutingModelSnapshot Snapshot() => model.ToSnapshot();
        public int Presentations => model.AuditPresentationCount;
        public int Updates => model.AuditModelUpdateCount;
    }
    private sealed record Fixture(string ExperimentId, FixtureRow[] Train, FixtureRow[] Exam);
    private sealed record FixtureRow(string Id, string Prompt, ProjectWikiRequestRouteKind ExpectedRouteKind, string ExpectedTargetUnitId, string Group);
    private sealed record ExamDecision(string Id, string Group, string Prompt, ProjectWikiRequestRouteKind ExpectedRouteKind,
        string ExpectedTargetUnitId, ProjectWikiRequestRouteKind ActualRouteKind, string ActualTargetUnitId,
        ProjectWikiRequestRoutingDecisionStatus DecisionStatus, double ConfidenceDiagnostic, double TopTwoMarginDiagnostic,
        string[] Warnings, bool CorrectSelected, bool WrongSelected, bool Deferred, bool RouteLabelMatchesIgnoringStatus);
    private sealed record Metrics(int Rows, int CorrectSelected, int WrongSelected, int Deferred, double CorrectSelectedRate,
        double WrongSelectedRate, double DeferredRate, int RouteLabelMatchesIgnoringStatusDiagnostic);
}
