// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// ─── Pipeline result ──────────────────────────────────────────────────────────

public sealed record PipelineResult(
    bool   Succeeded,
    string Summary,
    IReadOnlyList<string> ChangedFiles);

// ─── Four-stage agent pipeline ────────────────────────────────────────────────

// Models the four stages shared by all AI coding agents:
//   Stage 1 — index the codebase (build or refresh the symbol graph)
//   Stage 2 — assemble context (select the most relevant code for this task)
//   Stage 3 — reason with the language model (get a plan)
//   Stage 4 — apply changes (write the model's edits to files)
public sealed class AgentPipeline
{
    private readonly ISymbolGraphProvider    _symbols;
    private readonly ISemanticSearchProvider _search;
    private readonly ILlmGateway            _llm;

    public AgentPipeline(
        ISymbolGraphProvider    symbols,
        ISemanticSearchProvider search,
        ILlmGateway             llm)
    {
        _symbols = symbols;
        _search  = search;
        _llm     = llm;
    }

    public async Task<OperationResult<PipelineResult>> RunAsync(
        string task,
        CancellationToken cancellationToken = default)
    {
        // Stage 1 — ensure the codebase index is available.
        var symbolsResult = await _symbols.GetAllSymbolsAsync();
        if (!symbolsResult.IsSuccess)
            return OperationResult<PipelineResult>.Fail(
                $"Stage 1 failed: {symbolsResult.ErrorMessage}", "STAGE1_FAILED");

        // Stage 2 — semantic search for the most relevant context.
        var searchResult = await _search.SearchAsync(task, topK: 10);
        if (!searchResult.IsSuccess)
            return OperationResult<PipelineResult>.Fail(
                $"Stage 2 failed: {searchResult.ErrorMessage}", "STAGE2_FAILED");

        // Build prompt from top-ranked context chunks.
        var contextSummary = string.Join(", ", searchResult.Value!.Select(r => r.ChunkId));
        var history = new List<ConversationMessage>
        {
            new("system", "You are a coding agent."),
            new("user",   $"Task: {task}\nRelevant context: {contextSummary}")
        };

        // Stage 3 — call the language model for a plan.
        var llmResult = await _llm.SendAsync(history, cancellationToken);
        if (!llmResult.IsSuccess)
            return OperationResult<PipelineResult>.Fail(
                $"Stage 3 failed: {llmResult.ErrorMessage}", "STAGE3_FAILED");

        // Stage 4 — change application.
        //
        // This pipeline returns the language model's plan as the result.
        // The caller is responsible for parsing that plan and calling
        // FileChangeApplier (21.AgentInfrastructure/Editing/FileChangeApplier.cs)
        // to apply each search-replace block to the relevant files.
        //
        // This separation is intentional: the pipeline provides the reasoning;
        // the infrastructure layer owns file writes and atomic rollback.
        // ChangedFiles is empty here because no files are written at this layer.
        return OperationResult<PipelineResult>.Ok(
            new PipelineResult(true, llmResult.Value!, []));
    }
}
