using System.Diagnostics;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

var root = @"C:\Users\lubao\Documents\xiigen\llm mvp";
var bundleId = "legacy-docs-skills-from-old-code";
var bundle = Path.Combine(root, @"external-sources-training-sets\claude-export\batch-0000\legacy-docs-skills-from-old-code");
var ledger = Path.Combine(root, @"teaching-ledger\claude-export\batch-0000\legacy-docs-skills-from-old-code");
var ledgerVariant = Environment.GetEnvironmentVariable("TEACHING_LEDGER_VARIANT");
if (!string.IsNullOrWhiteSpace(ledgerVariant))
{
    ledger = Path.Combine(ledger, ledgerVariant);
}

var checkpointRoot = Path.Combine(ledger, "checkpoints-cumulative", "active");
var snapshotPath = Path.Combine(ledger, "teaching-registry-snapshot-active.json");
var stableLedgerPath = Path.Combine(ledger, "teaching-ledger-active.jsonl");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
Directory.CreateDirectory(ledger);
Directory.CreateDirectory(checkpointRoot);

var allRows = LoadPromptRows(bundle);
var snapshotLoad = TeachingRegistryPersistence.Load(snapshotPath);
if (!snapshotLoad.IsSuccess)
{
    throw new InvalidOperationException(snapshotLoad.ErrorMessage);
}

if (args.Length > 0 && string.Equals(args[0], "bulk", StringComparison.OrdinalIgnoreCase))
{
    var endRow = args.Length > 1 && int.TryParse(args[1], out var parsedEnd)
        ? parsedEnd
        : allRows.Length;
    var startRow = args.Length > 2 && int.TryParse(args[2], out var parsedStart)
        ? parsedStart
        : (snapshotLoad.Value?.RowIndex ?? 0) + 1;

    if (startRow < 1 || endRow > allRows.Length || startRow > endRow)
    {
        throw new InvalidOperationException($"Bulk range {startRow}..{endRow} is outside available range 1..{allRows.Length}.");
    }

    var bulk = await RunRowsAsync(startRow, endRow);
    Console.WriteLine(JsonSerializer.Serialize(bulk, jsonOptions));
    Console.WriteLine(bulk.SummaryPath);
    return;
}

var nextRowNumber = args.Length > 0 && int.TryParse(args[0], out var explicitRowNumber)
    ? explicitRowNumber
    : (snapshotLoad.Value?.RowIndex ?? 0) + 1;
if (nextRowNumber < 1 || nextRowNumber > allRows.Length)
{
    throw new InvalidOperationException($"Row {nextRowNumber} is outside available range 1..{allRows.Length}.");
}

var single = await RunRowsAsync(nextRowNumber, nextRowNumber);
Console.WriteLine(JsonSerializer.Serialize(single.Rows[0], jsonOptions));
Console.WriteLine(single.Rows[0].SummaryPath);

async Task<BulkRunSummary> RunRowsAsync(int startRow, int endRow)
{
    var session = TeachingSessionBuilder.Open(
        new RunnerNullSessionStorage(),
        bundleId,
        checkpointRoot,
        snapshotPath);
    if (!session.IsSuccess)
    {
        throw new InvalidOperationException(session.ErrorMessage);
    }

    var teachingSession = session.Value!;
    var compiler = new PromptRoutingInputCompilerUnit();
    var singleRouter = new SubPhaseRouterUnit(teachingSession.Topology);
    var router = new MultiSubPhaseRouterUnit(teachingSession.Topology, singleRouter);
    var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);
    var orchestrator = new AdaptiveLogicalOrchestrator(
        teachingSession.Topology,
        compiler,
        singleRouter,
        teachingSession.Flows,
        teachingSession.Units,
        router,
        capabilityTrainer: trainer);

    var rows = new List<RowRunSummary>();
    var bulkStopwatch = Stopwatch.StartNew();
    for (var rowNumber = startRow; rowNumber <= endRow; rowNumber++)
    {
        var rowSummary = await RunRowAsync(
            rowNumber,
            teachingSession,
            trainer,
            orchestrator,
            compiler,
            router);
        rows.Add(rowSummary);
    }

    bulkStopwatch.Stop();
    var aggregate = BuildBulkAggregate(startRow, endRow, bulkStopwatch.Elapsed.TotalMilliseconds, rows);
    var summaryPath = Path.Combine(ledger, $"bulk-summary-active-{startRow:0000}-{endRow:0000}.json");
    var bulk = new BulkRunSummary(
        startRow,
        endRow,
        rows.Count,
        rows.Count(row => string.Equals(row.Decision, "ACCEPT_ROW", StringComparison.Ordinal)),
        rows.Count(row => string.Equals(row.Decision, "REVIEW_ROW", StringComparison.Ordinal)),
        rows.Count(row => string.Equals(row.Verification.OverallVerdict, "PASS", StringComparison.Ordinal)),
        rows.Sum(row => row.Verification.KnowledgeEdgeCount),
        aggregate,
        rows.ToArray(),
        summaryPath);

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(bulk, jsonOptions));
    File.WriteAllText(Path.Combine(ledger, "latest-bulk-summary.json"), JsonSerializer.Serialize(bulk, jsonOptions));
    return bulk;
}

async Task<RowRunSummary> RunRowAsync(
    int rowNumber,
    TeachingSession teachingSession,
    DefaultLogicalCapabilityTrainer trainer,
    AdaptiveLogicalOrchestrator orchestrator,
    PromptRoutingInputCompilerUnit compiler,
    MultiSubPhaseRouterUnit router)
{
    var row = allRows[rowNumber - 1];
    var rowSlug = Slug(row.RowId);
    var runId = $"legacy-docs-skills-cumulative-row-{rowNumber:0000}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    var flags = new LogicalExecutionFlags();
    var request = BuildRequest(row, runId, bundle, bundleId, "single-process-bulk-cumulative");

    var registryBefore = RegistrySnapshot(teachingSession.Flows, teachingSession.Units);
    var process = Process.GetCurrentProcess();
    var availableBefore = ReadAvailableBytes(ledger);
    var beforeCpu = process.TotalProcessorTime;
    var beforeWorkingSet = process.WorkingSet64;
    var beforeAllocated = GC.GetTotalAllocatedBytes(precise: true);
    var stopwatch = Stopwatch.StartNew();

    var compiled = await compiler.ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
        runId,
        PromptRoutingInputCompilerUnit.Id,
        PromptRoutingInputCompilerUnit.Role,
        PromptRoutingInputCompilerUnit.DomainTag,
        request,
        flags,
        request.Metadata));
    if (!compiled.IsSuccess)
    {
        throw new InvalidOperationException(compiled.ErrorMessage);
    }

    var routed = await router.ExecuteAsync(new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
        runId,
        MultiSubPhaseRouterUnit.Id,
        MultiSubPhaseRouterUnit.Role,
        MultiSubPhaseRouterUnit.DomainTag,
        compiled.Value!.Output,
        flags,
        request.Metadata));
    if (!routed.IsSuccess)
    {
        throw new InvalidOperationException(routed.ErrorMessage);
    }

    var syntheticKnowledgeEdges = routed.Value!.Output
        .Select(decision => BuildKnowledgeEdge(request, decision))
        .ToArray();

    var trainingStopwatch = Stopwatch.StartNew();
    var teaching = await trainer.TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
        runId,
        request,
        syntheticKnowledgeEdges,
        teachingSession.Topology,
        teachingSession.Flows,
        teachingSession.Units));
    trainingStopwatch.Stop();
    if (!teaching.IsSuccess)
    {
        throw new InvalidOperationException(teaching.ErrorMessage);
    }

    var verification = await orchestrator.RunAsync(request);
    stopwatch.Stop();
    process.Refresh();

    var afterCpu = process.TotalProcessorTime;
    var afterWorkingSet = process.WorkingSet64;
    var afterAllocated = GC.GetTotalAllocatedBytes(precise: true);
    var availableAfter = ReadAvailableBytes(ledger);
    var registryAfter = RegistrySnapshot(teachingSession.Flows, teachingSession.Units);
    var elapsedMs = Math.Max(1, stopwatch.Elapsed.TotalMilliseconds);
    var cpuPercent = ((afterCpu - beforeCpu).TotalMilliseconds / elapsedMs) * 100.0 / Math.Max(1, Environment.ProcessorCount);
    var checkpointBytes = Directory.Exists(checkpointRoot)
        ? Directory.EnumerateFiles(checkpointRoot, "*", SearchOption.AllDirectories).Sum(path => new FileInfo(path).Length)
        : 0L;

    var save = teachingSession.SaveSnapshot(rowNumber);
    if (!save.IsSuccess)
    {
        throw new InvalidOperationException(save.ErrorMessage);
    }

    var regressionRows = allRows
        .Take(rowNumber - 1)
        .Reverse()
        .Take(3)
        .Reverse()
        .ToArray();
    var regression = new List<RegressionSummary>();
    foreach (var previous in regressionRows)
    {
        var previousRunId = $"legacy-docs-skills-regression-row-{rowNumber:0000}-{Slug(previous.RowId)}";
        var previousRequest = BuildRequest(previous, previousRunId, bundle, bundleId, "single-process-bulk-regression");
        var previousResult = await orchestrator.RunAsync(previousRequest);
        regression.Add(new RegressionSummary(
            previous.RowId,
            previous.Prompt,
            previousResult.IsSuccess,
            previousResult.Value?.OverallVerdict,
            previousResult.Value?.SelectedFlowId,
            previousResult.Value?.BranchResults.Count(branch => branch.OverallVerdict == "KNOWLEDGE_EDGE") ?? 0,
            SelectedUnitIds(previousResult.Value?.BranchResults ?? []),
            previousRunId));
    }

    var verificationKnowledgeEdges = verification.Value?.BranchResults.Count(branch => branch.OverallVerdict == "KNOWLEDGE_EDGE") ?? 0;
    var regressionPassed = regression.All(item =>
        item.Success &&
        string.Equals(item.OverallVerdict, "PASS", StringComparison.Ordinal) &&
        item.KnowledgeEdgeCount == 0);

    var resources = new ResourceSummary(
        Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3),
        Math.Round(trainingStopwatch.Elapsed.TotalMilliseconds, 3),
        Math.Round(cpuPercent, 2),
        Math.Round((afterWorkingSet - beforeWorkingSet) / 1024.0 / 1024.0, 3),
        Math.Round((afterAllocated - beforeAllocated) / 1024.0 / 1024.0, 3),
        Math.Round((availableBefore - availableAfter) / 1024.0 / 1024.0, 3),
        checkpointBytes);

    var summaryPath = Path.Combine(ledger, $"row-summary-active-{rowNumber:0000}-{rowSlug}.json");
    var summary = new RowRunSummary(
        rowNumber,
        row.RowId,
        "single-process-bulk-cumulative-with-regression",
        runId,
        snapshotPath,
        row.Prompt,
        compiled.Value.Output.CompiledPredicateIds,
        routed.Value.Output.Select(decision => decision.PredictedSubPhaseId).ToArray(),
        routed.Value.Output.Length,
        routed.Value.Output.Select(decision => new RouterDecisionSummary(
            decision.PredictedPhaseId,
            decision.PredictedSubPhaseId,
            decision.Confidence,
            decision.EvidencePredicates)).ToArray(),
        resources,
        registryBefore,
        registryAfter,
        new RegistrationSummary(
            teaching.Value!.RegisteredFlowIds,
            teaching.Value.RegisteredUnitIds,
            teaching.Value.RegisteredSubPhaseIds,
            teaching.Value.TrainingTimeMs),
        new VerificationSummary(
            verification.IsSuccess,
            verification.Value?.OverallVerdict,
            verification.Value?.SelectedFlowId,
            verificationKnowledgeEdges,
            SelectedUnitIds(verification.Value?.BranchResults ?? [])),
        regression.ToArray(),
        verification.IsSuccess &&
        verification.Value?.OverallVerdict == "PASS" &&
        verificationKnowledgeEdges == 0 &&
        regressionPassed &&
        stopwatch.Elapsed.TotalMilliseconds <= 10_000
            ? "ACCEPT_ROW"
            : "REVIEW_ROW",
        summaryPath);

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, jsonOptions));
    File.WriteAllText(Path.Combine(ledger, "latest-row-summary.json"), JsonSerializer.Serialize(summary, jsonOptions));
    File.AppendAllText(stableLedgerPath, JsonSerializer.Serialize(summary) + Environment.NewLine);
    Console.WriteLine($"row {rowNumber:0000}: {summary.Decision}, {summary.Verification.OverallVerdict}, branches={summary.BranchCount}, elapsed={summary.Resources.ElapsedMs:0.0}ms, training={summary.Resources.TrainingElapsedMs:0.0}ms");
    return summary;
}

static BulkAggregateSummary BuildBulkAggregate(int startRow, int endRow, double bulkElapsedMs, IReadOnlyList<RowRunSummary> rows)
{
    var latestByFamily = rows
        .SelectMany(row => row.RegisteredThisRow.RegisteredUnitIds)
        .Select(TryParseGeneratedUnit)
        .Where(parsed => parsed is not null)
        .Select(parsed => parsed!.Value)
        .GroupBy(parsed => parsed.FamilyId, StringComparer.Ordinal)
        .ToDictionary(
            group => group.Key,
            group => new
            {
                Count = group.Count(),
                MaxStep = group.Max(item => item.RowNumber)
            },
            StringComparer.Ordinal);

    return new BulkAggregateSummary(
        startRow,
        endRow,
        Math.Round(bulkElapsedMs / 1000.0, 3),
        Math.Round(rows.Sum(row => row.Resources.ElapsedMs) / 1000.0, 3),
        Math.Round(rows.Average(row => row.Resources.ElapsedMs) / 1000.0, 3),
        Math.Round(rows.Max(row => row.Resources.ElapsedMs) / 1000.0, 3),
        Math.Round(rows.Sum(row => row.Resources.TrainingElapsedMs) / 1000.0, 3),
        Math.Round(rows.Average(row => row.Resources.TrainingElapsedMs) / 1000.0, 3),
        Math.Round(rows.Max(row => row.Resources.TrainingElapsedMs) / 1000.0, 3),
        Math.Round(rows.Max(row => row.Resources.WorkingSetDeltaMb), 3),
        rows.LastOrDefault()?.Resources.CheckpointBytes ?? 0,
        latestByFamily.ToDictionary(pair => pair.Key, pair => pair.Value.Count, StringComparer.Ordinal),
        latestByFamily.ToDictionary(pair => pair.Key, pair => pair.Value.MaxStep, StringComparer.Ordinal));
}

static (string FamilyId, int RowNumber)? TryParseGeneratedUnit(string unitId)
{
    const string rowMarker = "-row-";
    var rowIndex = unitId.LastIndexOf(rowMarker, StringComparison.Ordinal);
    if (rowIndex < 0 || rowIndex + rowMarker.Length + 4 > unitId.Length)
    {
        return null;
    }

    var rowText = unitId.Substring(rowIndex + rowMarker.Length, 4);
    if (!int.TryParse(rowText, out var rowNumber))
    {
        return null;
    }

    return (unitId.Substring(0, rowIndex), rowNumber);
}

static PromptRow[] LoadPromptRows(string bundle)
{
    var sessionMapPath = Path.Combine(bundle, "session-map.json");
    using var document = JsonDocument.Parse(File.ReadAllText(sessionMapPath));
    return document.RootElement.GetProperty("primary_prompts")
        .EnumerateArray()
        .Select((row, index) => new PromptRow(
            row.GetProperty("RowId").GetString() ?? $"row-{index + 1:0000}",
            row.GetProperty("prompt").GetString() ?? string.Empty))
        .ToArray();
}

static LogicalOrchestratorRequest BuildRequest(PromptRow row, string runId, string bundle, string bundleId, string cycleMode)
{
    return new LogicalOrchestratorRequest(
        runId,
        AdaptiveLogicalOrchestrator.ScenarioId,
        row.Prompt,
        new LogicalExecutionFlags(),
        new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["source_bundle"] = bundle,
            ["source_row_id"] = row.RowId,
            ["bundle_id"] = bundleId,
            ["cycle_mode"] = cycleMode
        });
}

static LogicalFlowResult BuildKnowledgeEdge(LogicalOrchestratorRequest request, SubPhaseRoutingDecision decision)
{
    var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["knowledge_edge"] = true.ToString(),
        ["missing_sub_phase"] = decision.PredictedSubPhaseId,
        ["missing_flow"] = true.ToString(),
        ["routed_phase_id"] = decision.PredictedPhaseId,
        ["routing_confidence"] = decision.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
        ["knowledge_edge_reason"] = "single-process bulk cumulative teaching updates capability for routed sub-phase"
    };

    return new LogicalFlowResult(
        AdaptiveLogicalOrchestrator.KnowledgeEdgeFlowId,
        request.ScenarioId,
        "KNOWLEDGE_EDGE",
        $"Teaching edge for routed sub-phase '{decision.PredictedSubPhaseId}'.",
        new LogicalFlowSummary(
            AdaptiveLogicalOrchestrator.KnowledgeEdgeFlowId,
            1,
            0,
            0,
            "knowledge_edge",
            false,
            true,
            false,
            "TEACH_CUMULATIVE_CAPABILITY",
            decision.Confidence,
            "single-process bulk cumulative teaching",
            $"missing_sub_phase={decision.PredictedSubPhaseId}",
            0,
            0),
        metrics,
        decision.EvidencePredicates,
        ["knowledge_edge"]);
}

static string[] SelectedUnitIds(LogicalFlowResult[] branches)
{
    return branches
        .Select(branch => branch.Metrics.TryGetValue("learned_unit_id", out var unitId) ? unitId : "")
        .Where(unitId => !string.IsNullOrWhiteSpace(unitId))
        .Distinct(StringComparer.Ordinal)
        .ToArray();
}

static RegistrySummary RegistrySnapshot(InMemoryLogicalFlowRegistry flows, InMemoryLogicalUnitRegistry units)
{
    return new RegistrySummary(
        flows.ListDescriptors().Count,
        units.ListDescriptors().Count,
        flows.ListDescriptors().Count(descriptor =>
            descriptor.FlowId.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, StringComparison.Ordinal)),
        units.ListDescriptors().Count(descriptor => descriptor.IsLearned));
}

static long ReadAvailableBytes(string path)
{
    var root = Path.GetPathRoot(path);
    return DriveInfo.GetDrives()
        .Where(drive => drive.IsReady && string.Equals(drive.Name, root, StringComparison.OrdinalIgnoreCase))
        .Select(drive => drive.AvailableFreeSpace)
        .FirstOrDefault();
}

static string Slug(string value)
{
    var safeValue = string.IsNullOrWhiteSpace(value) ? "row" : value.Trim();
    var chars = safeValue.Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-').ToArray();
    return string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
}

public sealed record PromptRow(string RowId, string Prompt);

public sealed record RouterDecisionSummary(
    string PredictedPhaseId,
    string PredictedSubPhaseId,
    double Confidence,
    string[] EvidencePredicates);

public sealed record ResourceSummary(
    double ElapsedMs,
    double TrainingElapsedMs,
    double CpuPercent,
    double WorkingSetDeltaMb,
    double ManagedAllocationDeltaMb,
    double DiskDeltaMb,
    long CheckpointBytes);

public sealed record RegistrySummary(
    int FlowCount,
    int UnitCount,
    int GeneratedFlowCount,
    int LearnedUnitCount);

public sealed record RegistrationSummary(
    string[] RegisteredFlowIds,
    string[] RegisteredUnitIds,
    string[] RegisteredSubPhaseIds,
    double TrainingTimeMs);

public sealed record VerificationSummary(
    bool Success,
    string? OverallVerdict,
    string? SelectedFlowId,
    int KnowledgeEdgeCount,
    string[] SelectedUnitIds);

public sealed record RegressionSummary(
    string RowId,
    string Prompt,
    bool Success,
    string? OverallVerdict,
    string? SelectedFlowId,
    int KnowledgeEdgeCount,
    string[] SelectedUnitIds,
    string RegressionRunId);

public sealed record RowRunSummary(
    int RowNumber,
    string RowId,
    string CycleMode,
    string RunId,
    string SnapshotPath,
    string Prompt,
    string[] CompilerPredicates,
    string[] RouterSubPhases,
    int BranchCount,
    RouterDecisionSummary[] RouterOutput,
    ResourceSummary Resources,
    RegistrySummary RegistryBefore,
    RegistrySummary RegistryAfter,
    RegistrationSummary RegisteredThisRow,
    VerificationSummary Verification,
    RegressionSummary[] RegressionRows,
    string Decision,
    string SummaryPath);

public sealed record BulkAggregateSummary(
    int StartRow,
    int EndRow,
    double WallClockElapsedSec,
    double TotalRowElapsedSec,
    double AvgRowElapsedSec,
    double MaxRowElapsedSec,
    double TotalTrainingSec,
    double AvgTrainingSec,
    double MaxTrainingSec,
    double MaxWorkingSetDeltaMb,
    long FinalCheckpointBytes,
    Dictionary<string, int> UnitFamilyCounts,
    Dictionary<string, int> UnitFamilyMaxRows);

public sealed record BulkRunSummary(
    int StartRow,
    int EndRow,
    int RowCount,
    int AcceptedRows,
    int ReviewRows,
    int PassRows,
    int KnowledgeEdgeCount,
    BulkAggregateSummary Aggregate,
    RowRunSummary[] Rows,
    string SummaryPath);

file sealed class RunnerNullSessionStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
    {
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
    {
        return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));
    }

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
    {
        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
    }
}
