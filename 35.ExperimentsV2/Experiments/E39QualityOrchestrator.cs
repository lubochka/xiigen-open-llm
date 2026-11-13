// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E39: QualityOrchestrator — predict which checks to run for an output type.
// Baseline:  run all 16 checks always
// Enhanced:  classify output type → run only relevant checks
// Key metric: recall MUST be >= 0.95 — never miss a required check.
//             precision improves from 60% → 80% (fewer unnecessary checks).
public sealed class E39QualityOrchestrator
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases   = BuildTestCases();
        int totalChecks = QualityCheckLibrary.All.Count; // 16

        // ── Baseline: all 16 checks always ───────────────────────────────────
        var t0 = Stopwatch();
        double baseAvgPrecision = testCases.Average(tc =>
        {
            int needed = QualityCheckLibrary.ForOutputType(tc.OutputType).Count;
            return needed / (double)totalChecks;
        });
        double baseRecall = 1.0;
        double baseF1     = 2 * baseRecall * baseAvgPrecision / (baseRecall + baseAvgPrecision);
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: classify output type → targeted checks ─────────────────
        var t1 = Stopwatch();
        double enhTotalRecall = 0; double enhTotalPrecision = 0; int enhTotalLoaded = 0;

        foreach (var tc in testCases)
        {
            int predictedType = PredictOutputType(tc.TaskDescription);
            var predicted     = QualityCheckLibrary.GetCheckVector(predictedType);
            var actual        = QualityCheckLibrary.GetCheckVector(tc.OutputType);

            int tp = Enumerable.Range(0, totalChecks).Count(i => predicted[i] && actual[i]);
            int fp = Enumerable.Range(0, totalChecks).Count(i => predicted[i] && !actual[i]);
            int fn = Enumerable.Range(0, totalChecks).Count(i => !predicted[i] && actual[i]);

            double recall    = (tp + fn) > 0 ? tp / (double)(tp + fn) : 1.0;
            double precision = (tp + fp) > 0 ? tp / (double)(tp + fp) : 1.0;

            enhTotalRecall    += recall;
            enhTotalPrecision += precision;
            enhTotalLoaded    += predicted.Count(p => p);
        }

        double enhRecall    = enhTotalRecall    / testCases.Count;
        double enhPrecision = enhTotalPrecision / testCases.Count;
        double enhF1        = 2 * enhRecall * enhPrecision / (enhRecall + enhPrecision);
        double enhAvgLoaded = enhTotalLoaded / (double)testCases.Count;
        double loadReduction = 1.0 - enhAvgLoaded / totalChecks;
        double enhancedMs   = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E39-QUALITY-ORCHESTRATOR",
            Tier              = tier,
            BaselinePrimary   = baseF1,
            EnhancedPrimary   = enhF1,
            PrimaryMetricName = "check_f1",
            LowerIsBetter     = false,
            ScalingSignal     = enhRecall,
            ScalingSignalNote = "recall (>= 0.95 required — must not miss a needed check)",
            ShouldScaleUp     = testCases.Count < 15 || enhRecall < 0.95,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, total_checks={totalChecks}, " +
                    $"baseF1={baseF1:F2}(recall=100%,prec={baseAvgPrecision:P0}), " +
                    $"enhF1={enhF1:F2}(recall={enhRecall:P0},prec={enhPrecision:P0}), " +
                    $"avgLoaded={enhAvgLoaded:F1}/{totalChecks}, loadReduction={loadReduction:P1}"
        });
    }

    private static int PredictOutputType(string taskDescription)
    {
        var lower = taskDescription.ToLowerInvariant();
        if (lower.Contains("react") || lower.Contains("jsx") ||
            lower.Contains("component") || lower.Contains("ui page") ||
            lower.Contains("screen"))
            return QualityCheckLibrary.REACT_PAGE;
        if (lower.Contains(".service.ts") || lower.Contains(".cs") ||
            lower.Contains("engine/flows") || lower.Contains("service file") ||
            lower.Contains("null check") || lower.Contains("implementation"))
            return QualityCheckLibrary.CODE_SERVICE;
        if (lower.Contains("plan") || lower.Contains("session file") ||
            lower.Contains("phase complete") || lower.Contains("document"))
            return QualityCheckLibrary.PLAN_DOC;
        if (lower.Contains("analysis") || lower.Contains("reconnaissance") ||
            lower.Contains("architecture") || lower.Contains("review"))
            return QualityCheckLibrary.ANALYSIS_DOC;
        return QualityCheckLibrary.ANY_OUTPUT;
    }

    private static List<(string TaskDescription, int OutputType)> BuildTestCases() => new()
    {
        ("Implement the ValidateToken method in AuthService with null checks",
         QualityCheckLibrary.CODE_SERVICE),
        ("Generate service code for Phase B of Flow-01 with DNA compliance",
         QualityCheckLibrary.CODE_SERVICE),
        ("Write the UserService.ts with scope isolation and auth decorators",
         QualityCheckLibrary.CODE_SERVICE),
        ("Produce the implementation plan for Phase A with binary done criteria",
         QualityCheckLibrary.PLAN_DOC),
        ("Create the session file for this phase completion with state JSON",
         QualityCheckLibrary.PLAN_DOC),
        ("Write the plan review document for the auth system design",
         QualityCheckLibrary.PLAN_DOC),
        ("Build the tenant dashboard React page with proper UI/UX compliance",
         QualityCheckLibrary.REACT_PAGE),
        ("Create the user registration screen component for the flow",
         QualityCheckLibrary.REACT_PAGE),
        ("Analyse the authentication architecture and produce reconnaissance findings",
         QualityCheckLibrary.ANALYSIS_DOC),
        ("Review the codebase structure and identify gaps before planning",
         QualityCheckLibrary.ANALYSIS_DOC),
        ("Examine the existing flows for portability gaps and produce report",
         QualityCheckLibrary.ANALYSIS_DOC),
        ("Respond to the question about how tenant isolation works",
         QualityCheckLibrary.ANY_OUTPUT),
        ("Explain the purpose of the DataProcessResult pattern",
         QualityCheckLibrary.ANY_OUTPUT),
    };

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
