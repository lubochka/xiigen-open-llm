// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.RegularExpressions;
using AgentInfrastructure;         // OperationResult<T>
using AgentInfrastructure.Logical; // ILogicalUnit, LogicalUnitRequest, LogicalExecutionFlags

public sealed record SubmitUserTaskRequest(
    string UserPrompt,
    string ActiveProjectRoot,
    string[] LinkedProjectRoots,
    string UserFacingEntrypoint);

public sealed record ProjectUnderstanding(
    string[] ProjectRoots,
    string[] StackFacts,
    string[] CodeEvidenceFacts,
    string[] MissingEvidence);

public sealed record TaskOrchestrationIntent(
    string Goal,
    string[] RequestedWorkItems,
    string[] UserNamedObjects);

public sealed record TaskEvidencePlan(
    string[] EvidenceQuestions,
    string[] EvidenceGaps);

public sealed record TaskUnitSelection(
    string[] Units,
    string[] Reasons);

public sealed record HumanTaskRoute(
    string UserAnswer,
    bool HasEnoughStructure,
    string[] Warnings);

public sealed record TaskOrchestrationResponse(
    string Status,
    TaskOrchestrationIntent Intent,
    ProjectUnderstanding Project,
    TaskEvidencePlan EvidencePlan,
    TaskUnitSelection UnitSelection,
    HumanTaskRoute HumanRoute);

public sealed class SubmitUserTaskApi
{
    private readonly TaskOrchestrationUnit _taskOrchestrationUnit;

    public SubmitUserTaskApi(TaskOrchestrationUnit? taskOrchestrationUnit = null)
    {
        _taskOrchestrationUnit = taskOrchestrationUnit ?? new TaskOrchestrationUnit();
    }

    public Task<OperationResult<TaskOrchestrationResponse>> SubmitAsync(
        SubmitUserTaskRequest request,
        CancellationToken ct = default)
        => _taskOrchestrationUnit.OrchestrateAsync(request, ct);
}

public sealed class TaskOrchestrationUnit
{
    // _intentBuilder parses the user prompt into a structured intent.
    private readonly IIntentBuilderUnit _intentBuilder;
    // _projectScanner scans the file system to produce project understanding evidence.
    private readonly IProjectScannerUnit _projectScanner;
    // _evidencePlanner generates the evidence question list and gap list.
    private readonly IEvidencePlannerUnit _evidencePlanner;
    // _unitSelector chooses which execution units are needed for this task.
    private readonly IUnitSelectorUnit _unitSelector;
    // _routeComposer assembles the full human-facing answer route string.
    private readonly IRouteComposerUnit _routeComposer;
    // _verifier checks that the route has enough structure to return to the user.
    private readonly IOrchestrationVerifierUnit _verifier;

    // When called without arguments, TaskOrchestrationUnit creates Bootstrap instances —
    // same behaviour as before TD1. When called with trained implementations from
    // 25.ModelServer, the trained implementations are used instead (design rule A-LM-03).
    public TaskOrchestrationUnit(
        IIntentBuilderUnit? intentBuilder = null,
        IProjectScannerUnit? projectScanner = null,
        IEvidencePlannerUnit? evidencePlanner = null,
        IUnitSelectorUnit? unitSelector = null,
        IRouteComposerUnit? routeComposer = null,
        IOrchestrationVerifierUnit? verifier = null)
    {
        // Each Bootstrap unit preserves the exact static logic from the original class.
        _intentBuilder  = intentBuilder  ?? new BootstrapIntentBuilderUnit();
        _projectScanner = projectScanner ?? new BootstrapProjectScannerUnit();
        _evidencePlanner = evidencePlanner ?? new BootstrapEvidencePlannerUnit();
        _unitSelector   = unitSelector   ?? new BootstrapUnitSelectorUnit();
        _routeComposer  = routeComposer  ?? new BootstrapRouteComposerUnit();
        _verifier       = verifier       ?? new BootstrapOrchestrationVerifierUnit();
    }

    public async Task<OperationResult<TaskOrchestrationResponse>> OrchestrateAsync(
        SubmitUserTaskRequest request,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<TaskOrchestrationResponse>.Fail("Task orchestration cancelled.", "TASK_ORCHESTRATION_CANCELLED");

        if (request is null)
            return OperationResult<TaskOrchestrationResponse>.Fail("User task request is required.", "USER_TASK_REQUEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.UserPrompt))
            return OperationResult<TaskOrchestrationResponse>.Fail("User prompt is required.", "USER_TASK_PROMPT_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.ActiveProjectRoot))
            return OperationResult<TaskOrchestrationResponse>.Fail("Active project root is required.", "USER_TASK_PROJECT_REQUIRED");

        // Use the unit id prefix "wiki-task" to identify all orchestration pipeline steps.
        const string DomainTag = "project-wiki";
        var emptyMeta = (IReadOnlyDictionary<string, object>)new Dictionary<string, object>();

        // Step 1: parse the user prompt into a structured intent through the interface.
        var intentResult = await _intentBuilder.ExecuteAsync(
            new LogicalUnitRequest<string>("run-" + Guid.NewGuid().ToString("N")[..8],
                "intent-builder", "intent-builder", DomainTag,
                request.UserPrompt, new LogicalExecutionFlags(), emptyMeta), ct);
        if (!intentResult.IsSuccess)
            return intentResult.MapError<TaskOrchestrationResponse>();
        var intent = intentResult.Value!.Output;

        // Step 2: scan the file system to produce project understanding evidence.
        var projectResult = await _projectScanner.ExecuteAsync(
            new LogicalUnitRequest<SubmitUserTaskRequest>("run-" + Guid.NewGuid().ToString("N")[..8],
                "project-scanner", "project-scanner", DomainTag,
                request, new LogicalExecutionFlags(), emptyMeta), ct);
        if (!projectResult.IsSuccess)
            return projectResult.MapError<TaskOrchestrationResponse>();
        var project = projectResult.Value!.Output;

        // Step 3: generate the evidence question list and gap list.
        var evidencePlanResult = await _evidencePlanner.ExecuteAsync(
            new LogicalUnitRequest<EvidencePlannerInput>("run-" + Guid.NewGuid().ToString("N")[..8],
                "evidence-planner", "evidence-planner", DomainTag,
                new EvidencePlannerInput(intent, project), new LogicalExecutionFlags(), emptyMeta), ct);
        if (!evidencePlanResult.IsSuccess)
            return evidencePlanResult.MapError<TaskOrchestrationResponse>();
        var evidencePlan = evidencePlanResult.Value!.Output;

        // Step 4: select the execution units needed for this task.
        var unitSelectionResult = await _unitSelector.ExecuteAsync(
            new LogicalUnitRequest<UnitSelectorInput>("run-" + Guid.NewGuid().ToString("N")[..8],
                "unit-selector", "unit-selector", DomainTag,
                new UnitSelectorInput(intent, project, evidencePlan), new LogicalExecutionFlags(), emptyMeta), ct);
        if (!unitSelectionResult.IsSuccess)
            return unitSelectionResult.MapError<TaskOrchestrationResponse>();
        var units = unitSelectionResult.Value!.Output;

        // Step 5: compose the human-facing answer route string.
        var routeResult = await _routeComposer.ExecuteAsync(
            new LogicalUnitRequest<RouteComposerInput>("run-" + Guid.NewGuid().ToString("N")[..8],
                "route-composer", "route-composer", DomainTag,
                new RouteComposerInput(request, intent, project, evidencePlan, units),
                new LogicalExecutionFlags(), emptyMeta), ct);
        if (!routeResult.IsSuccess)
            return routeResult.MapError<TaskOrchestrationResponse>();
        var route = routeResult.Value!.Output;

        // Step 6: verify the route has enough structure to return to the user.
        var verifyResult = await _verifier.ExecuteAsync(
            new LogicalUnitRequest<VerifierInput>("run-" + Guid.NewGuid().ToString("N")[..8],
                "orchestration-verifier", "orchestration-verifier", DomainTag,
                new VerifierInput(route, intent, project, evidencePlan),
                new LogicalExecutionFlags(), emptyMeta), ct);
        if (!verifyResult.IsSuccess)
            return verifyResult.MapError<TaskOrchestrationResponse>();

        // Use the verifier outcome to set the final orchestration status.
        var status = verifyResult.Value!.Output.Passes ? "ORCHESTRATED" : "NEEDS_MORE_EVIDENCE";
        var response = new TaskOrchestrationResponse(status, intent, project, evidencePlan, units, route);
        return OperationResult<TaskOrchestrationResponse>.Ok(response);
    }

    // BuildIntent and ExtractWorkItems have moved to BootstrapIntentBuilderUnit
    // in BootstrapOrchestrationUnits.cs. TaskOrchestrationUnit no longer needs them.
}

public sealed class ProjectUnderstandingUnit
{
    public Task<OperationResult<ProjectUnderstanding>> UnderstandAsync(
        SubmitUserTaskRequest request,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ProjectUnderstanding>.Fail("Project understanding cancelled.", "PROJECT_UNDERSTANDING_CANCELLED"));

        var roots = new[] { request.ActiveProjectRoot }
            .Concat(request.LinkedProjectRoots ?? [])
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var stackFacts = new List<string>();
        var codeFacts = new List<string>();
        var missing = new List<string>();

        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
            {
                missing.Add($"Project root not found: {root}");
                continue;
            }

            var projectName = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .Take(6000)
                .ToArray();

            if (files.Any(path => Path.GetFileName(path).Equals("angular.json", StringComparison.OrdinalIgnoreCase)))
                stackFacts.Add($"{projectName}: Angular workspace evidence found.");
            if (files.Any(path => Path.GetFileName(path).Equals("package.json", StringComparison.OrdinalIgnoreCase)))
                stackFacts.Add($"{projectName}: JavaScript/TypeScript package evidence found.");
            if (files.Any(path => path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)))
                stackFacts.Add($"{projectName}: .NET solution/project evidence found.");
            if (files.Any(path => path.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)))
                stackFacts.Add($"{projectName}: SQL file evidence found.");
            if (files.Any(path => path.EndsWith(".drawio", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".drawio.xml", StringComparison.OrdinalIgnoreCase)))
                codeFacts.Add($"{projectName}: diagram evidence found.");
            if (files.Any(path => path.Contains($"{Path.DirectorySeparatorChar}Controllers{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
                codeFacts.Add($"{projectName}: backend controller evidence found.");
            if (files.Any(path => path.Contains($"{Path.DirectorySeparatorChar}Services{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
                codeFacts.Add($"{projectName}: service layer evidence found.");
            if (files.Any(path => path.Contains($"{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}app{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
                codeFacts.Add($"{projectName}: Angular app source evidence found.");
            if (files.Any(path => path.Contains($"{Path.DirectorySeparatorChar}.github{Path.DirectorySeparatorChar}workflows{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
                codeFacts.Add($"{projectName}: CI workflow evidence found.");
            codeFacts.AddRange(ReadProjectWikiEvidenceFacts(root, projectName));
            if (!files.Any(path => path.Contains($"{Path.DirectorySeparatorChar}.xiigen{Path.DirectorySeparatorChar}wiki-domain{Path.DirectorySeparatorChar}evidence", StringComparison.OrdinalIgnoreCase)))
                missing.Add($"{projectName}: project-local wiki evidence folder was not found inside the project.");
        }

        if (roots.Length < 2)
            missing.Add("No linked repository was provided; cross-stack business-system reasoning is limited.");

        var result = new ProjectUnderstanding(
            roots,
            stackFacts.Distinct(StringComparer.Ordinal).ToArray(),
            codeFacts.Distinct(StringComparer.Ordinal).ToArray(),
            missing.Distinct(StringComparer.Ordinal).ToArray());
        return Task.FromResult(OperationResult<ProjectUnderstanding>.Ok(result));
    }

    private static string[] ReadProjectWikiEvidenceFacts(string root, string projectName)
    {
        var evidenceRoots = new[]
            {
                Path.Combine(root, ".xiigen", "wiki-domain", "evidence"),
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "xiigen",
                    "llm mvp",
                    "localwikis",
                    projectName,
                    "evidence")
            }
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var facts = new List<string>();
        foreach (var evidenceRoot in evidenceRoots)
        {
            var files = Directory.EnumerateFiles(evidenceRoot, "*", SearchOption.AllDirectories)
                .Where(IsReadableEvidenceFile)
                .Take(20);
            foreach (var file in files)
            {
                foreach (var line in ReadEvidenceLines(file).Take(3))
                    facts.Add($"{projectName}: wiki evidence says {line}");
            }
        }

        return facts.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static bool IsReadableEvidenceFile(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> ReadEvidenceLines(string path)
    {
        foreach (var raw in File.ReadLines(path))
        {
            var line = Regex.Replace(raw.Trim(), @"\s+", " ");
            if (line.Length < 12 || line.Length > 220)
                continue;
            if (line is "{" or "}" or "[" or "]")
                continue;
            if (line.StartsWith("//", StringComparison.Ordinal))
                continue;

            yield return line.Trim('-', '*', ' ', '"', ',');
        }
    }
}

public sealed class EvidencePlanner
{
    public TaskEvidencePlan PlanEvidence(TaskOrchestrationIntent intent, ProjectUnderstanding project)
    {
        var questions = new List<string>
        {
            "What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?",
            "Which requested work items can be grounded in code evidence, and which require attached external input or user decision?",
            "Where can the system reuse existing project structure before planning new implementation?"
        };
        questions.AddRange(intent.RequestedWorkItems.Select(item => $"What project evidence is needed before planning: {item}?"));

        var gaps = project.MissingEvidence.ToList();
        foreach (var item in intent.RequestedWorkItems)
        {
            if (RequiresExternalInput(item))
                gaps.Add($"External input required before final implementation detail: {item}.");
        }

        return new TaskEvidencePlan(
            questions.Distinct(StringComparer.Ordinal).ToArray(),
            gaps.Distinct(StringComparer.Ordinal).ToArray());
    }

    private static bool RequiresExternalInput(string item)
    {
        var text = item.ToLowerInvariant();
        return text.Contains("figma", StringComparison.Ordinal) ||
               text.Contains("audio", StringComparison.Ordinal) ||
               text.Contains("diagram", StringComparison.Ordinal) ||
               text.Contains("hosting", StringComparison.Ordinal) ||
               text.Contains("platform", StringComparison.Ordinal);
    }
}

public sealed class UnitSelector
{
    public TaskUnitSelection SelectUnits(
        TaskOrchestrationIntent intent,
        ProjectUnderstanding project,
        TaskEvidencePlan evidencePlan)
    {
        var units = new List<string>
        {
            "TaskOrchestrationUnit",
            "ProjectUnderstandingUnit",
            "EvidencePlanner",
            "UnitSelector",
            "HumanRouteComposer",
            "VerificationAbstentionUnit"
        };
        if (project.ProjectRoots.Length > 1)
            units.Add("CrossStackBusinessFlowGraphUnit");
        if (intent.RequestedWorkItems.Any(item => item.Contains("RAG", StringComparison.OrdinalIgnoreCase) || item.Contains("context", StringComparison.OrdinalIgnoreCase)))
            units.Add("WikiContextRoutingUnit");
        if (evidencePlan.EvidenceGaps.Length > 0)
            units.Add("MissingEvidenceAbstentionUnit");

        var reasons = units.Select(unit => $"{unit}: required for user-task orchestration inside llm_mvp_core.").ToArray();
        return new TaskUnitSelection(units.Distinct(StringComparer.Ordinal).ToArray(), reasons);
    }
}

public sealed class HumanRouteComposer
{
    public HumanTaskRoute Compose(
        SubmitUserTaskRequest request,
        TaskOrchestrationIntent intent,
        ProjectUnderstanding project,
        TaskEvidencePlan evidencePlan,
        TaskUnitSelection units)
    {
        var lines = new List<string>
        {
            "Final XIIGen answer:",
            "",
            "I understood the task as follows:",
            intent.Goal,
            "",
            "What I see in the project:"
        };
        lines.AddRange(project.StackFacts.Concat(project.CodeEvidenceFacts).DefaultIfEmpty("No sufficient code evidence yet in the discovered projects.").Select(fact => $"- {fact}"));
        lines.Add("");
        lines.Add("How to proceed with the work:");
        lines.Add("1. First confirm the current system: active repo, linked repos, UI/backend/storage/flow surfaces.");
        lines.Add("2. Break the user task into work parts and for each part check reuse before rewrite.");
        var index = 3;
        foreach (var item in intent.RequestedWorkItems)
        {
            lines.Add($"{index}. For part \"{item}\" find evidence in the project, determine owner surface, required inputs, expected output and the risk of missing evidence.");
            index++;
        }
        lines.Add($"{index}. Assemble the overall route: what we do first, what depends on evidence, where a feedback loop is needed, what can be handed to the executor.");
        index++;
        lines.Add($"{index}. Before implementation, verify that the answer is tied to real code and is not a generic plan.");
        lines.Add("");
        lines.Add("What questions XIIGen should ask the project wiki/RAG:");
        lines.AddRange(evidencePlan.EvidenceQuestions.Select(question => $"- {question}"));
        lines.Add("");
        lines.Add("What must not be glossed over:");
        lines.AddRange(evidencePlan.EvidenceGaps.DefaultIfEmpty("No critical missing evidence found at this level.").Select(gap => $"- {gap}"));
        lines.Add("");
        lines.Add("Inside XIIGen the following are used for this:");
        lines.AddRange(units.Units.Select(unit => $"- {unit}"));

        var answer = string.Join(Environment.NewLine, lines);
        var enough = intent.RequestedWorkItems.Length > 0 && project.ProjectRoots.Length > 0;
        return new HumanTaskRoute(answer, enough, enough ? [] : ["Human route is missing requested work items or project roots."]);
    }
}

public sealed class OrchestrationVerifier
{
    public bool Verify(
        HumanTaskRoute route,
        TaskOrchestrationIntent intent,
        ProjectUnderstanding project,
        TaskEvidencePlan evidencePlan)
        => route.HasEnoughStructure &&
           !string.IsNullOrWhiteSpace(route.UserAnswer) &&
           intent.RequestedWorkItems.Length > 0 &&
           project.ProjectRoots.Length > 0 &&
           evidencePlan.EvidenceQuestions.Length > 0;
}
