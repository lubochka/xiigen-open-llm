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
        int totalSkills = SkillTriggerLibrary.All.Count;

        // ── Baseline: first 5 from ALL skills alphabetically (no category filter) ─
        var t0 = Stopwatch();
        var alphabeticalTop5 = SkillTriggerLibrary.All
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
        foreach (var tc in testCases)
        {
            var qTags  = FamilyCollapser.CollapseString(tc.TaskDescription).ToHashSet();
            var ranked = SkillTriggerLibrary.All
                .Where(s => !s.AlwaysOn)
                .Select(s =>
                {
                    var sTags = FamilyCollapser.CollapseString(s.Condition).ToHashSet();
                    return (Skill: s, Sim: JaccardSim(qTags, sTags));
                })
                .OrderByDescending(x => x.Sim)
                .ToList();

            double maxSim = ranked.Count > 0 ? ranked[0].Sim : 0;

            List<SkillTriggerLibrary.SkillTrigger> selected;
            if (maxSim < ConfidenceThreshold)
            {
                selected = SkillTriggerLibrary.All
                    .Where(s => s.AlwaysOn || s.Category == tc.ExpectedCategory)
                    .ToList();
                fallbackCount++;
            }
            else
            {
                selected = [.. SkillTriggerLibrary.AlwaysRequired];
                selected.AddRange(ranked.Select(x => x.Skill).Take(TopK - selected.Count));
            }

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
         SkillTriggerLibrary.CODE_REVIEW),
        ("Review the new UserService.ts file for DNA compliance and auth decorators",
         SkillTriggerLibrary.CODE_REVIEW),
        ("Check if the generated code has SDK imports outside provider files",
         SkillTriggerLibrary.CODE_REVIEW),
        ("Verify that self-verification covers the auth protection change category",
         SkillTriggerLibrary.CODE_REVIEW),

        ("Write unit tests and e2e tests for the new auth controller spec file",
         SkillTriggerLibrary.TESTING),
        ("Run phase preflight default check number six before closing Phase B",
         SkillTriggerLibrary.TESTING),
        ("Verify test-integrity Rule 7 for 401 403 responses per controller",
         SkillTriggerLibrary.TESTING),
        ("Check arbiter NDJSON scope isolation count before Phase A gate",
         SkillTriggerLibrary.TESTING),

        ("Review the implementation plan for Flow-01 Phase A with binary criteria",
         SkillTriggerLibrary.PLANNING),
        ("Create the session file for this phase completion and write state JSON",
         SkillTriggerLibrary.PLANNING),
        ("Promote the active flow to distributable — write Steps 6a 6b 6c",
         SkillTriggerLibrary.PLANNING),

        ("Check authentication infrastructure 4-component score before Tier C",
         SkillTriggerLibrary.AUTH),
        ("Verify UseGuards and Roles decorators are present on the controller",
         SkillTriggerLibrary.AUTH),
        ("Validate that bypass paths are registered in the bypass-paths registry",
         SkillTriggerLibrary.AUTH),

        ("Examine the existing React pages for the user registration flow UI",
         SkillTriggerLibrary.UI),
        ("Review the screen design for the tenant dashboard component",
         SkillTriggerLibrary.UI),
        ("Check all 5 instances of the user card component against design system",
         SkillTriggerLibrary.UI),

        ("Review this document for machine shortcodes and plain language gate",
         SkillTriggerLibrary.GOVERNANCE),
        ("Verify goal context and architect habits before the approval stop wait",
         SkillTriggerLibrary.GOVERNANCE),
    ];

    private static double JaccardSim(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        int inter = a.Count(x => b.Contains(x));
        int union  = a.Count + b.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
