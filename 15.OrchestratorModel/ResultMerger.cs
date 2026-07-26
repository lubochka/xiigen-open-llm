// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel;

using AgentInfrastructure;

// Merges multi-agent results into one coherent output.
// FC-12: accepts ITemplateBeautifier — never SemanticCompiler29.TemplateBeautifier directly.
// E23: SUCCESS=RETURN mapping is embedded in the real Beautifier inside the adapter.
public sealed class ResultMerger : BaseService, IResultMerger
{
    // Fabric interface — not a concrete type (Fabric First, Rule 2).
    private readonly ITemplateBeautifier _beautifier;

    // FC-12: ISessionStorage first parameter → base(storage).
    public ResultMerger(ISessionStorage storage, ITemplateBeautifier beautifier)
        : base(storage)
        => _beautifier = beautifier;

    // Merge all results. Single result is returned unchanged (no beautification overhead).
    // Multi-result: formatted with step headers then passed through the beautifier.
    // On TEMPLATE_NOT_FOUND the raw merged text is returned as the fallback.
    public OperationResult<string> Merge(
        IReadOnlyList<(AgentAlgorithms.SubTask Task, string Result)> results)
    {
        if (!results.Any())
            return OperationResult<string>.Fail("No results to merge.", "EMPTY_RESULTS");

        // Single result — return without modification.
        if (results.Count == 1)
            return OperationResult<string>.Ok(results[0].Result);

        // Join results with step headers for readability.
        var merged = string.Join("\n\n", results.Select((r, i) =>
            $"## Step {i + 1}: {r.Task.RequiredRole}\n{r.Result}"));

        // Try template beautification; fall back to raw merged on no-match.
        var beautifyResult = _beautifier.Beautify(merged);
        return beautifyResult.IsSuccess
            ? beautifyResult
            : OperationResult<string>.Ok(merged);
    }
}
