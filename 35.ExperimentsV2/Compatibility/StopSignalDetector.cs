// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// Compatibility surface for historical E33 fixtures.
// Runtime stop-signal classification delegates to SemanticCompiler.StopSignalDetector.
public static class StopSignalDetector
{
    public const int CONTINUE = SemanticCompiler.StopSignalDetector.CONTINUE;
    public const int STOP_PHASES = SemanticCompiler.StopSignalDetector.STOP_PHASES;
    public const int STOP_TECH = SemanticCompiler.StopSignalDetector.STOP_TECH;
    public const int STOP_SINGLE = SemanticCompiler.StopSignalDetector.STOP_SINGLE;
    public const int STOP_NOLOOP = SemanticCompiler.StopSignalDetector.STOP_NOLOOP;

    public static readonly string[] SignalNames = SemanticCompiler.StopSignalDetector.SignalNames;

    public static int PredictByRules(string taskDescription, bool hasIterations, bool hasTechTerms)
        => SemanticCompiler.StopSignalDetector.Detect(taskDescription, hasIterations, hasTechTerms);

    public static List<(string Description, bool HasIter, bool HasTech, int Signal)> GenerateExamples(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new (string Desc, bool Iter, bool Tech, int Signal)[]
        {
            ("First read the requirements, then decide the architecture, then implement it", false, false, STOP_PHASES),
            ("Check the configuration, then run the tests, then deploy the result", false, false, STOP_PHASES),
            ("Open the file, parse the content, write the output to disk", false, false, STOP_PHASES),
            ("Configure the DI container to resolve the scoped service correctly", false, true, STOP_TECH),
            ("Fix the JWT token validation to handle the RS256 algorithm edge case", false, true, STOP_TECH),
            ("Produce the architecture diagram that will govern all subsequent decisions", false, false, STOP_SINGLE),
            ("Create the single requirements document approved first pass", false, false, STOP_SINGLE),
            ("Analyse the module dependencies to understand the coupling", false, false, STOP_NOLOOP),
            ("Write the summary of findings from the code review", false, false, STOP_NOLOOP),
            ("Implement the authentication flow with multiple rounds of review and correction", true, false, CONTINUE),
            ("Design the database schema, review with team, iterate based on feedback", true, false, CONTINUE),
            ("Build the search feature: first version, review output, three rounds of fixes", true, false, CONTINUE),
            ("Migrate the platform in stages, validate each stage before proceeding", false, false, CONTINUE),
        };

        var result = new List<(string, bool, bool, int)>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }
}
