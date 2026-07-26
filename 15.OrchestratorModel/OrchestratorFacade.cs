// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel;

using AgentInfrastructure;

// Main pipeline for subproject 15.OrchestratorModel.
// Named OrchestratorFacade to avoid a C# naming conflict with the namespace OrchestratorModel.
// Orchestrates: Classify → Decompose → StopFilter → Sort → Launch → Merge → QualityCheck → Store.
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class OrchestratorFacade : BaseService
{
    private readonly IEpisodeClassifier              _classifier;
    private readonly IEpisodeDecomposer              _decomposer;
    private readonly IStopSignalDetector             _stopDetector;
    private readonly AgentAlgorithms.DependencyGraph _graph;
    private readonly IAgentLauncher                  _launcher;
    private readonly IResultMerger                   _merger;
    private readonly IQualityOrchestrator            _quality;
    private readonly IEpisodeMemory                  _memory;

    public OrchestratorFacade(
        ISessionStorage                  storage,
        IEpisodeClassifier               classifier,
        IEpisodeDecomposer               decomposer,
        IStopSignalDetector              stopDetector,
        AgentAlgorithms.DependencyGraph  graph,
        IAgentLauncher                   launcher,
        IResultMerger                    merger,
        IQualityOrchestrator             quality,
        IEpisodeMemory                   memory)
        : base(storage)
    {
        _classifier   = classifier;
        _decomposer   = decomposer;
        _stopDetector = stopDetector;
        _graph        = graph;
        _launcher     = launcher;
        _merger       = merger;
        _quality      = quality;
        _memory       = memory;
    }

    // Process a task end-to-end: classify → decompose → sort → launch → merge → quality check.
    public async Task<OperationResult<string>> ProcessAsync(string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<string>.Fail("Task description cannot be empty.", "EMPTY_TASK");

        // Step 1: classify the task domain.
        var classifyResult = _classifier.ClassifyWithConfidence(taskDescription);
        if (!classifyResult.IsSuccess)
            return OperationResult<string>.Fail(classifyResult.ErrorMessage, "CLASSIFY_FAILED");

        // Step 2: decompose into sub-episodes.
        var decompResult = _decomposer.Decompose(taskDescription, classifyResult.Value!.Domain);
        if (!decompResult.IsSuccess)
            return OperationResult<string>.Fail(decompResult.ErrorMessage, "DECOMPOSE_FAILED");

        // Step 3: filter out sub-tasks that should not be further decomposed.
        var subTasks = decompResult.Value!;
        // AgentInfrastructure.OperationResult<bool>.Value is T (not Nullable<T>) for value types —
        // cannot use ??. Check IsSuccess first; treat detection failure as "do not stop".
        var tasks    = subTasks
            .Where(t => {
                var stop = _stopDetector.ShouldStop(t.Description);
                return !(stop.IsSuccess && stop.Value!);
            })
            .ToList();

        // Fallback: if all tasks were filtered out, run all of them.
        if (tasks.Count == 0) tasks = subTasks.ToList();

        // Step 4: sort by dependency levels (E35 DependencyGraph).
        var sortResult    = _graph.Sort(tasks);
        var orderedLevels = sortResult.IsSuccess
            ? sortResult.Value!
            : (IReadOnlyList<IReadOnlyList<AgentAlgorithms.SubTask>>)[tasks];

        // Step 5: launch agents level by level, passing context forward.
        var allResults = new List<(AgentAlgorithms.SubTask Task, string Result)>();
        string context = "";
        foreach (var level in orderedLevels)
        {
            var levelResults = await Task.WhenAll(
                level.Select(t => _launcher.LaunchAsync(t, context)));

            foreach (var (task, res) in level.Zip(levelResults))
            {
                allResults.Add((task,
                    res.IsSuccess ? res.Value! : $"[{task.RequiredRole} failed]"));
            }

            // Context for the next level is the last completed result.
            context = allResults.Last().Result;
        }

        // Step 6: merge all agent results.
        var mergeResult = _merger.Merge(allResults);
        if (!mergeResult.IsSuccess)
            return OperationResult<string>.Fail(mergeResult.ErrorMessage, "MERGE_FAILED");

        // Step 7: run quality checks.
        string outputType   = classifyResult.Value!.Domain == TaskDomain.CODE_Q
            ? "CODE_SERVICE" : "PLAN_DOC";
        var qualityResult   = _quality.Check(taskDescription, mergeResult.Value!, outputType);
        if (qualityResult.IsSuccess && !qualityResult.Value!.Passed)
        {
            string failedChecks = string.Join(", ", qualityResult.Value!.FailedChecks);
            return OperationResult<string>.Fail(
                $"Quality checks failed: {failedChecks}", "QUALITY_FAILED");
        }

        // Step 8: store episode in memory for future recall.
        _memory.Store(taskDescription, subTasks);

        return OperationResult<string>.Ok(mergeResult.Value!);
    }
}
