// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Tools.Quality03NonInterRaterProofProducers;

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
            !parsed.TryGetValue("source-proof-contract-pack", out var sourceProofContractPack) ||
            !parsed.TryGetValue("output-dir", out var outputDirectory))
        {
            WriteUsage();
            return UsageError;
        }

        var options = new Quality03NonInterRaterProofProducerOptions
        {
            EvidencePackDirectory = evidencePack,
            SourceProofContractPackDirectory = sourceProofContractPack,
            OutputDirectory = outputDirectory,
            RunId = parsed.TryGetValue("run-id", out var runId) ? runId : string.Empty,
            WorkOrderId = parsed.TryGetValue("work-order-id", out var workOrderId)
                ? workOrderId
                : "QUALITY-03-NON-INTER-RATER-PROOF-DATA-AND-RUNNERS-10A",
            ApprovedProofBundleRoot = parsed.TryGetValue("approved-proof-bundle-root", out var proofBundleRoot) ? proofBundleRoot : string.Empty,
            ApprovedTrainSourceCorpusManifestRef = parsed.TryGetValue("approved-train-source-corpus-ref", out var trainRef) ? trainRef : string.Empty,
            ApprovedHeldOutSplitManifestRef = parsed.TryGetValue("approved-held-out-split-ref", out var heldOutRef) ? heldOutRef : string.Empty,
            ApprovedRawPrivateCorpusRef = parsed.TryGetValue("approved-raw-private-corpus-ref", out var rawRef) ? rawRef : string.Empty
        };

        var result = new Quality03NonInterRaterProofProducer().Run(options);
        if (!result.IsSuccess || result.Value is null)
        {
            Console.Error.WriteLine($"{result.ErrorCode}: {result.ErrorMessage}");
            return RuntimeError;
        }

        Console.WriteLine(string.Join(Environment.NewLine, result.Value.OutputRefs));
        Console.WriteLine($"non_inter_rater_proof_refs_pass={result.Value.NonInterRaterProofRefsPassProduced}/{result.Value.NonInterRaterProofRefsTotal}");
        Console.WriteLine($"blocking_proof_refs_pass_after_producer={result.Value.BlockingProofRefsPassAfterProducer}/{result.Value.TotalProofRefs}");
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
        => Console.Error.WriteLine("Usage: dotnet run --project 43.SmartSkillsDomain.Tools/Quality03NonInterRaterProofProducers -- --evidence-pack <dir> --source-proof-contract-pack <dir> --output-dir <dir> [--run-id <id>] [--work-order-id <id>] [--approved-proof-bundle-root <dir-or-json>] [--approved-train-source-corpus-ref <ref>] [--approved-held-out-split-ref <ref>] [--approved-raw-private-corpus-ref <ref>]");
}
