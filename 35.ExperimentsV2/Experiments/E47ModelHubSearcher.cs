// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E47: ModelHubSearcher — find adapter in registry by domain description.
// Baseline:  exact tag match (requires query word to appear literally in tag list)
// Enhanced:  BM25 on E15-normalized description words (handles paraphrasing)
// Metric:    hit@1 + hit@3 (does the correct adapter appear in top-3?)
public sealed class E47ModelHubSearcher
{
    private static readonly (string Name, string Description, string[] Tags)[] Registry =
    [
        ("csharp-dotnet-v2",
         "C# and .NET services dependency injection async patterns EF Core",
         ["csharp","dotnet","service","async","entity"]),
        ("typescript-nestjs-v1",
         "TypeScript NestJS controllers decorators dependency injection guards",
         ["typescript","nestjs","controller","decorator","guard"]),
        ("devops-deploy-v1",
         "Kubernetes deployment Docker containers CI CD pipelines Helm charts",
         ["deploy","kubernetes","docker","container","pipeline","helm"]),
        ("devops-monitoring-v1",
         "Monitoring dashboards alerting metrics Prometheus Grafana observability",
         ["monitor","alert","metric","dashboard","observability","grafana"]),
        ("security-auth-v2",
         "JWT authentication OAuth2 authorization vulnerability review RBAC",
         ["security","auth","jwt","oauth","vulnerability","rbac"]),
        ("data-analytics-v1",
         "Query optimization data pipelines ETL processes analytics reporting",
         ["data","query","pipeline","analytics","etl","optimize"]),
        ("testing-e2e-v1",
         "End to end testing integration tests test automation Playwright",
         ["test","integration","automation","playwright","spec"]),
        ("mobile-ios-v1",
         "iOS Swift development SwiftUI mobile authentication push notifications",
         ["mobile","ios","swift","swiftui","notification"]),
        ("database-sql-v1",
         "SQL query optimization indexing strategy migration scripts stored procedures",
         ["database","sql","index","migration","stored","procedure"]),
        ("python-ml-v1",
         "Python machine learning scikit pandas dataframes model training",
         ["python","machine","learn","pandas","scikit","model"]),
    ];

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases = BuildTestCases();

        // Build BM25 index on registry descriptions
        var bm25 = new BM25EpisodeMatcher();
        bm25.Index(Registry.Select((r, i) => (r.Description, i)).ToList());

        // ── Baseline: exact tag match (first registry entry with matching tag) ──
        var t0 = Stopwatch();
        int baseHit1 = 0, baseHit3 = 0;
        foreach (var tc in testCases)
        {
            var words = tc.Query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // Rank all registry entries by tag match count
            var ranked = Enumerable.Range(0, Registry.Length)
                .Select(i => (Idx: i, Score: Registry[i].Tags.Count(t => words.Contains(t))))
                .OrderByDescending(x => x.Score)
                .ToList();
            if (ranked.Count > 0 && ranked[0].Score > 0 && ranked[0].Idx == tc.ExpectedIdx)
                baseHit1++;
            if (ranked.Take(3).Any(x => x.Score > 0 && x.Idx == tc.ExpectedIdx))
                baseHit3++;
        }
        double baseAcc1   = baseHit1 / (double)testCases.Count;
        double baseAcc3   = baseHit3 / (double)testCases.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: BM25 on normalized description words ───────────────────
        var t1 = Stopwatch();
        int enhHit1 = 0, enhHit3 = 0;
        foreach (var tc in testCases)
        {
            var results = bm25.Search(tc.Query, topK: 3);
            if (results.Count > 0 && results[0].EpisodeId == tc.ExpectedIdx) enhHit1++;
            if (results.Any(r => r.EpisodeId == tc.ExpectedIdx))             enhHit3++;
        }
        double enhAcc1    = enhHit1 / (double)testCases.Count;
        double enhAcc3    = enhHit3 / (double)testCases.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E47-MODEL-HUB-SEARCHER",
            Tier              = tier,
            BaselinePrimary   = baseAcc1,
            EnhancedPrimary   = enhAcc1,
            PrimaryMetricName = "hit_at_1",
            LowerIsBetter     = false,
            ScalingSignal     = enhAcc3,
            ScalingSignalNote = "hit@3 (sufficient to load top candidates)",
            ShouldScaleUp     = testCases.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"registry={Registry.Length}, testCases={testCases.Count}, " +
                    $"base: hit1={baseAcc1:P1} hit3={baseAcc3:P1}, " +
                    $"enh: hit1={enhAcc1:P1} hit3={enhAcc3:P1}"
        });
    }

    // Queries that paraphrase registry descriptions (exact tags not present in query)
    private static List<(string Query, int ExpectedIdx)> BuildTestCases() =>
    [
        // devops-deploy-v1 (idx=2)
        ("Deploy the application to Kubernetes cluster with resource limits",            2),
        ("Set up CI CD pipeline for automated deployment to production",                 2),
        ("Configure container orchestration for our microservice infrastructure",        2),
        // security-auth-v2 (idx=4)
        ("Review the JWT token implementation for vulnerabilities",                      4),
        ("Check the authentication flow for RBAC role compliance",                       4),
        ("Audit the OAuth integration for potential attack surfaces",                    4),
        // data-analytics-v1 (idx=5)
        ("Optimise the reporting query for better analytical performance",               5),
        ("Build an ETL data pipeline for the business intelligence dashboard",           5),
        // mobile-ios-v1 (idx=7)
        ("Add push notification support to the iOS application",                         7),
        ("Implement the mobile login flow using SwiftUI components",                     7),
        // database-sql-v1 (idx=8)
        ("Write database migration scripts for the new user schema",                     8),
        ("Add SQL indexes to speed up the slow running queries",                         8),
    ];

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
