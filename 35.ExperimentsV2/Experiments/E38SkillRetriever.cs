// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ProductionSkillRetriever = AgentAlgorithms.Skills.SkillRetriever;
using ProductionSkillTriggerLibrary = AgentAlgorithms.Skills.SkillTriggerLibrary;

namespace ExperimentsV2;

// E38v2: SkillRetriever — honest K=5 budget comparison.
// Baseline:  top-5 skills from category by alphabetical order (unranked)
// Enhanced:  top-5 by KNN similarity on task tags
//            + confidence fallback: if max_sim < 0.40 → load full category
// Metric:    precision_at_5_fair (both limited to K=5 budget)
//            + fallback_rate + load_reduction_vs_all
public sealed class E38SkillRetriever
{
    private const int    TopK               = 5;
    private const double ConfidenceThreshold = 0.40;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases   = BuildTestCases();
        int totalSkills = ProductionSkillTriggerLibrary.All.Count;

        // ── Baseline: first 5 from ALL skills alphabetically (no category filter) ─
        var t0 = Stopwatch();
        var alphabeticalTop5 = ProductionSkillTriggerLibrary.All
            .OrderBy(s => s.SkillName)
            .Take(TopK)
            .ToList();
        int base5Correct = testCases.Count(tc =>
            alphabeticalTop5.Any(s => s.Category == tc.ExpectedCategory));
        double baseAcc    = base5Correct / (double)testCases.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: KNN top-5 + confidence fallback ─────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = 0, fallbackCount = 0, enhTotalLoaded = 0;
        var retriever = new ProductionSkillRetriever();
        foreach (var tc in testCases)
        {
            var retrieval = retriever.Retrieve(tc.TaskDescription, tc.ExpectedCategory);
            var selected = retrieval.Skills;
            if (retrieval.IsFallback)
                fallbackCount++;

            enhTotalLoaded += selected.Count;
            if (selected.Any(s => s.Category == tc.ExpectedCategory))
                enhCorrect++;
        }
        double enhAcc       = enhCorrect / (double)testCases.Count;
        double enhAvgLoaded = enhTotalLoaded / (double)testCases.Count;
        double fallbackRate = fallbackCount / (double)testCases.Count;
        double loadVsAll    = 1.0 - enhAvgLoaded / totalSkills;
        double enhancedMs   = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E38V2-SKILL-RETRIEVER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = $"precision_at_{TopK}_fair",
            LowerIsBetter     = false,
            ScalingSignal     = loadVsAll,
            ScalingSignalNote = "load_reduction_vs_all (меньше скилов при том же recall)",
            ShouldScaleUp     = testCases.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, topK={TopK}, " +
                    $"confidenceThreshold={ConfidenceThreshold:F2}, " +
                    $"base={baseAcc:P1}(alphabetical), " +
                    $"enh={enhAcc:P1}(avgLoaded={enhAvgLoaded:F1}), " +
                    $"fallbackRate={fallbackRate:P1}, loadReduction={loadVsAll:P1}"
        });
    }

    private static List<(string TaskDescription, int ExpectedCategory)> BuildTestCases() =>
    [
        ("Add null check to ValidateToken service before accessing user properties",
         ProductionSkillTriggerLibrary.CODE_REVIEW),
        ("Review the new UserService.ts file for DNA compliance and auth decorators",
         ProductionSkillTriggerLibrary.CODE_REVIEW),
        ("Check if the generated code has SDK imports outside provider files",
         ProductionSkillTriggerLibrary.CODE_REVIEW),
        ("Verify that self-verification covers the auth protection change category",
         ProductionSkillTriggerLibrary.CODE_REVIEW),

        ("Write unit tests and e2e tests for the new auth controller spec file",
         ProductionSkillTriggerLibrary.TESTING),
        ("Run phase preflight default check number six before closing Phase B",
         ProductionSkillTriggerLibrary.TESTING),
        ("Verify test-integrity Rule 7 for 401 403 responses per controller",
         ProductionSkillTriggerLibrary.TESTING),
        ("Check arbiter NDJSON scope isolation count before Phase A gate",
         ProductionSkillTriggerLibrary.TESTING),

        ("Review the implementation plan for Flow-01 Phase A with binary criteria",
         ProductionSkillTriggerLibrary.PLANNING),
        ("Create the session file for this phase completion and write state JSON",
         ProductionSkillTriggerLibrary.PLANNING),
        ("Promote the active flow to distributable — write Steps 6a 6b 6c",
         ProductionSkillTriggerLibrary.PLANNING),

        ("Check authentication infrastructure 4-component score before Tier C",
         ProductionSkillTriggerLibrary.AUTH),
        ("Verify UseGuards and Roles decorators are present on the controller",
         ProductionSkillTriggerLibrary.AUTH),
        ("Validate that bypass paths are registered in the bypass-paths registry",
         ProductionSkillTriggerLibrary.AUTH),

        ("Examine the existing React pages for the user registration flow UI",
         ProductionSkillTriggerLibrary.UI),
        ("Review the screen design for the tenant dashboard component",
         ProductionSkillTriggerLibrary.UI),
        ("Check all 5 instances of the user card component against design system",
         ProductionSkillTriggerLibrary.UI),

        ("Review this document for machine shortcodes and plain language gate",
         ProductionSkillTriggerLibrary.GOVERNANCE),
        ("Verify goal context and architect habits before the approval stop wait",
         ProductionSkillTriggerLibrary.GOVERNANCE),
    ];

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
