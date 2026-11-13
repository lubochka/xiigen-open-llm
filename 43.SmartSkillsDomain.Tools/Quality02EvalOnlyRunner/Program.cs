// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Tools.Quality02EvalOnlyRunner;

using SmartSkillsDomain;

internal static class Program
{
    private const int Success = 0;
    private const int UsageError = 2;
    private const int RuntimeError = 3;

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
        {
            WriteUsage();
            return args.Length == 0 ? UsageError : Success;
        }

        var parsed = ParseArgs(args);
        if (!parsed.TryGetValue("input", out var inputPath) ||
            !parsed.TryGetValue("output-dir", out var outputDirectory))
        {
            WriteUsage();
            return UsageError;
        }

        if (!TryReadBool(parsed, "synthetic-fixture-only", defaultValue: true, out var syntheticFixtureOnly) ||
            !TryReadBool(parsed, "actual-approved-eval-executed", defaultValue: false, out var actualApprovedEvalExecuted))
        {
            Console.Error.WriteLine("Boolean flags must be true or false.");
            return UsageError;
        }

        var options = new ExtensionEvalOnlyQualityRunnerOptions
        {
            InputJsonlPath = inputPath,
            OutputDirectory = outputDirectory,
            RunId = parsed.TryGetValue("run-id", out var runId) ? runId : string.Empty,
            PhaseId = parsed.TryGetValue("phase-id", out var phaseId)
                ? phaseId
                : "QUALITY-02-EVAL-ONLY-RUNNER-IMPLEMENTATION-NO-EVAL-EXECUTION",
            EvidenceScope = parsed.TryGetValue("evidence-scope", out var evidenceScope)
                ? evidenceScope
                : ExtensionEvalOnlyQualityRunner.LocalPrivateEvalOnlyEvidenceScope,
            SyntheticFixtureOnly = syntheticFixtureOnly,
            ActualApprovedEvalExecuted = actualApprovedEvalExecuted
        };
        var runner = new ExtensionEvalOnlyQualityRunner();
        var result = runner.Run(options);
        if (!result.IsSuccess || result.Value is null)
        {
            Console.Error.WriteLine($"{result.ErrorCode}: {result.ErrorMessage}");
            return RuntimeError;
        }

        Console.WriteLine(string.Join(Environment.NewLine, result.Value.OutputRefs));
        return Success;
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            var key = args[index];
            if (!key.StartsWith("--", StringComparison.Ordinal))
                continue;
            var name = key[2..];
            if (index + 1 >= args.Length)
                continue;
            values[name] = args[++index];
        }

        return values;
    }

    private static bool TryReadBool(
        Dictionary<string, string> values,
        string key,
        bool defaultValue,
        out bool result)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            result = defaultValue;
            return true;
        }

        return bool.TryParse(raw, out result);
    }

    private static void WriteUsage()
        => Console.Error.WriteLine("Usage: dotnet run --project 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner -- --input <rows.jsonl> --output-dir <dir> [--run-id <id>] [--phase-id <id>] [--evidence-scope local_private_eval_only] [--synthetic-fixture-only true|false] [--actual-approved-eval-executed true|false]");
}
