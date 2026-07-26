// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

// ─── Loop result ──────────────────────────────────────────────────────────────

public sealed record AgentLoopResult(
    bool   IsComplete,
    string FinalResponse,
    AgentState FinalState,
    string? FailureReason = null);

// ─── Stage 04 — Agent loop (ReAct pattern) ────────────────────────────────────

// Runs the Reason-Act-Observe loop until the model signals task_complete,
// an error occurs, or the maximum iteration count is reached.
//
// Signal convention (sent by the mock/real LLM in ResponseText):
//   "TASK_COMPLETE:<message>" — loop terminates successfully.
//   A tool call (ToolCallName != null) — execute tool, append result, continue.
//   Plain text — append to history, continue.
public sealed class AgentLoop
{
    private readonly ILlmClient   _llm;
    private readonly ToolRegistry _tools;
    private readonly int          _maxIterations;
    private readonly int          _compactionThreshold; // token count that triggers history compaction
    private readonly int          _recentTurnsToPreserve;
    private readonly Func<IReadOnlyList<ConversationMessage>, Task<string>>? _summaryGenerator;

    public AgentLoop(
        ILlmClient   llm,
        ToolRegistry tools,
        int          maxIterations          = 50,
        int          compactionThreshold    = 30_000,
        int          recentTurnsToPreserve  = 10,
        Func<IReadOnlyList<ConversationMessage>, Task<string>>? summaryGenerator = null)
    {
        _llm                   = llm;
        _tools                 = tools;
        _maxIterations         = maxIterations;
        _compactionThreshold   = compactionThreshold;
        _recentTurnsToPreserve = recentTurnsToPreserve;
        _summaryGenerator      = summaryGenerator;
    }

    public async Task<OperationResult<AgentLoopResult>> RunAsync(
        AgentState         initialState,
        CancellationToken  cancellationToken = default)
    {
        var state      = initialState;
        var toolDefs   = _tools.AllToolDefinitions();

        while (state.IterationCount < _maxIterations)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<AgentLoopResult>.Fail("Cancelled", "CANCELLED");

            // Compact history if it is growing too large.
            if (TokenCounter.EstimateHistory(state.MessageHistory) > _compactionThreshold)
            {
                state = _summaryGenerator is not null
                    ? await CompactWithSummaryAsync(state)
                    : CompactHistory(state);
            }

            var llmResult = await _llm.SendAsync(state.MessageHistory, toolDefs, cancellationToken);
            if (!llmResult.IsSuccess)
                return OperationResult<AgentLoopResult>.Fail(llmResult.ErrorMessage, llmResult.ErrorCode);

            var response = llmResult.Value!;

            // Task complete signal.
            if (response.ResponseText.StartsWith("TASK_COMPLETE:", StringComparison.Ordinal))
            {
                string finalMessage = response.ResponseText["TASK_COMPLETE:".Length..];
                state = state.WithMessage(new ConversationMessage("assistant", finalMessage));
                return OperationResult<AgentLoopResult>.Ok(
                    new AgentLoopResult(true, finalMessage, state));
            }

            // Tool call — execute and append the result.
            if (response.ToolCallName is not null)
            {
                state = state.WithMessage(
                    new ConversationMessage("assistant", $"[tool call: {response.ToolCallName}]"));

                var toolResult = await _tools.ExecuteAsync(
                    response.ToolCallName,
                    response.ToolCallArguments ?? []);

                string toolOutput = toolResult.IsSuccess
                    ? toolResult.Value!
                    : $"[tool error: {toolResult.ErrorMessage}]";

                state = state.WithMessage(
                    new ConversationMessage("tool", toolOutput));
                continue;
            }

            // Plain text response — append and continue.
            state = state.WithMessage(
                new ConversationMessage("assistant", response.ResponseText));
        }

        // Exceeded maximum iterations — return partial result.
        return OperationResult<AgentLoopResult>.Ok(
            new AgentLoopResult(
                false,
                "Maximum iterations reached without completing the task.",
                state,
                "MAX_ITERATIONS_EXCEEDED"));
    }

    // Use the summary generator to summarize older messages, keep the most recent N.
    private async Task<AgentState> CompactWithSummaryAsync(AgentState state)
    {
        var history   = state.MessageHistory;
        int keepCount = Math.Min(_recentTurnsToPreserve, history.Count);
        var recent    = history.Skip(history.Count - keepCount).ToList();
        var older     = history.Take(history.Count - keepCount).ToList();

        string summary = older.Count > 0
            ? await _summaryGenerator!(older)
            : "[no prior history]";

        return state with
        {
            MessageHistory = [new ConversationMessage("system", $"[Summary: {summary}]"), .. recent]
        };
    }

    // Keep only the last N messages to free up token budget (fallback when no summaryGenerator).
    private static AgentState CompactHistory(AgentState state)
    {
        const int keep = 10;
        var recent = state.MessageHistory.TakeLast(keep).ToList();
        var summary = new ConversationMessage("system",
            "[Earlier conversation history was compacted to fit the context window.]");
        return state with { MessageHistory = [summary, .. recent] };
    }
}
