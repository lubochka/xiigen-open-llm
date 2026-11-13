// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Tools.Quality03ProofContractRunner;

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
        if (!parsed.TryGetValue("evidence-pack", out var evidencePack) ||
            !parsed.TryGetValue("proof-contract-pack", out var proofContractPack) ||
            !parsed.TryGetValue("output-dir", out var outputDirectory))
        {
            WriteUsage();
            return UsageError;
        }

        var options = new Quality03ProofContractRunnerOptions
        {
            EvidencePackDirectory = evidencePack,
            ProofContractPackDirectory = proofContractPack,
            SecondRaterPacketPath = parsed.TryGetValue("second-rater-packet", out var secondRaterPacket)
                ? secondRaterPacket
                : null,
            OutputDirectory = outputDirectory,
            RunId = parsed.TryGetValue("run-id", out var runId) ? runId : string.Empty,
            WorkOrderId = parsed.TryGetValue("work-order-id", out var workOrderId)
                ? workOrderId
                : "QUALITY-03-PROOF-CONTRACT-RUNNER-CODE-IMPLEMENTATION-09"
        };

        var result = new SmartSkillsDomain.Quality03ProofContractRunner().Run(options);
        if (!result.IsSuccess || result.Value is null)
        {
            Console.Error.WriteLine($"{result.ErrorCode}: {result.ErrorMessage}");
            return RuntimeError;
        }

        Console.WriteLine(string.Join(Environment.NewLine, result.Value.OutputRefs));
        Console.WriteLine($"blocking_proof_refs_pass={result.Value.BlockingProofRefsPass}/{result.Value.BlockingProofRefsTotal}");
        Console.WriteLine($"runner_ready_rows={result.Value.RunnerReadyRows}");
        Console.WriteLine($"quality_pass_rows={result.Value.QualityPassRows}");
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
            if (index + 1 >= args.Length)
                continue;
            values[key[2..]] = args[++index];
        }

        return values;
    }

    private static void WriteUsage()
        => Console.Error.WriteLine("Usage: dotnet run --project 43.SmartSkillsDomain.Tools/Quality03ProofContractRunner -- --evidence-pack <dir> --proof-contract-pack <dir> --output-dir <dir> [--second-rater-packet <path>] [--run-id <id>] [--work-order-id <id>]");
}
