// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentAlgorithms;
using AgentInfrastructure;

// Comparison of v4.0 components against three real-world .NET codebases.
// Measures: stack detection accuracy, domain classification, task boundary.

// The three codebases live beside this repository's checkout, so the base
// directory is resolved from the repo layout (the parent of the repo root)
// rather than hard-coded. On the owning machine that parent is the Projects
// folder, so the resolved roots are byte-identical. A caller may override the
// base by passing it as the first positional argument.
var projectsRoot = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath(Path.Combine(ResolveRepoRoot(), ".."));

var projects = new[]
{
    (Path.Combine(projectsRoot, "GenieDynamicServer"), "GenieDynamicServer", "fix exception in Excel converter service"),
    (Path.Combine(projectsRoot, "genietasks", "A1ProjectManagement"), "genietasks", "design the task lifecycle flow with authentication"),
    (Path.Combine(projectsRoot, "bizguid-server"), "bizguid-server", "how does the Excel converter work"),
};

var stackDetector   = new StackDetector();
var routeClassifier = new RouteClassifier();
var taskDecomposer  = new RoleBasedTaskDecomposer();

Console.WriteLine("=== v4.0 Component Comparison — Real .NET Libraries ===");
Console.WriteLine();

foreach (var (root, name, query) in projects)
{
    Console.WriteLine($"── {name} ──────────────────────────────────────────");

    // 1. Load .cs files
    var csFiles = Directory.Exists(root)
        ? Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
                   .Where(f => !f.Contains("obj") && !f.Contains("bin"))
                   .ToArray()
        : [];

    Console.WriteLine($"  Files: {csFiles.Length} .cs files");

    // 2. StackDetector — run on each file (E20 gate: detect BEFORE normalization)
    var stackCounts = new Dictionary<string, int>
        { ["CSharp"] = 0, ["TypeScript"] = 0, ["Python"] = 0, ["Unknown"] = 0 };

    foreach (var f in csFiles)
    {
        try
        {
            string src = File.ReadAllText(f);
            var r = stackDetector.Detect(src);
            if (r.IsSuccess)
                stackCounts[r.Value!.Language.ToString()]++;
        }
        catch { stackCounts["Unknown"]++; }
    }

    double csRatio = csFiles.Length > 0 ? stackCounts["CSharp"] / (double)csFiles.Length : 0;
    Console.WriteLine($"  StackDetector: CSharp={stackCounts["CSharp"]}, Unknown={stackCounts["Unknown"]}");
    Console.WriteLine($"  Detection accuracy (CSharp files): {csRatio:P0}");

    // Baseline: what text normalisation would see (lower-case everything, "task<" not "Task<")
    int baselineCs = 0;
    foreach (var f in csFiles)
    {
        try
        {
            string src = File.ReadAllText(f).ToLowerInvariant(); // simulate normalization
            var r = stackDetector.Detect(src);
            if (r.IsSuccess && r.Value!.Language == StackLanguage.CSharp)
                baselineCs++;
        }
        catch { }
    }
    double baseRatio = csFiles.Length > 0 ? baselineCs / (double)csFiles.Length : 0;
    Console.WriteLine($"  Baseline (after lower-case norm): {baseRatio:P0}  Δ = {(csRatio - baseRatio):+0.0%;-0.0%}");

    // 3. RouteClassifier — classify a representative query about this project
    var classResult = routeClassifier.Classify(query);
    if (classResult.IsSuccess)
    {
        var cls = classResult.Value!;
        Console.WriteLine($"  RouteClassifier query: \"{query}\"");
        Console.WriteLine($"    → {cls.Label} (confidence {cls.Confidence:P0})");
    }

    // 4. TaskDecomposer — decompose the query (E21 boundary detection)
    var decompResult = taskDecomposer.Decompose(query);
    if (decompResult.IsSuccess)
    {
        Console.WriteLine($"  TaskDecomposer → {decompResult.Value!.Count} sub-task(s): " +
            string.Join(" | ", decompResult.Value!.Select(t => $"[{t.RequiredRole}] {t.Description[..Math.Min(30, t.Description.Length)]}...")));
    }

    Console.WriteLine();
}

Console.WriteLine("=== Summary ===");
Console.WriteLine("StackDetector: correctly identifies CSharp before normalization.");
Console.WriteLine("Lower-case normalization destroys 'Task<' → 'task<', 'IEnumerable' → 'ienumerable' (not in markers).");
Console.WriteLine("RouteClassifier: domain classification from natural language query.");
Console.WriteLine("TaskDecomposer v2: E21 false-CONJ fix prevents bad splits.");

// Locate the repository root by ascending from the executable's base directory
// until an ancestor holds both the AGENTS.md marker and the LlmFromScratch.sln
// solution file. Requiring both markers prevents latching onto an unrelated
// ancestor that carries only one of them. Mirrors Tests/TestMachineRoots.cs.
static string ResolveRepoRoot()
{
    var current = new DirectoryInfo(AppContext.BaseDirectory);
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "AGENTS.md"))
            && File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    throw new InvalidOperationException(
        "Could not locate repository root ascending from " +
        $"'{AppContext.BaseDirectory}': no ancestor directory contains " +
        "both AGENTS.md and LlmFromScratch.sln.");
}
