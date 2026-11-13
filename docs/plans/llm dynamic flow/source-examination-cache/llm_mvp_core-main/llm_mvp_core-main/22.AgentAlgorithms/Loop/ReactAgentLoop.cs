namespace AgentAlgorithms;

// ─── Loop result ──────────────────────────────────────────────────────────────

public sealed record ReactLoopResult(
    bool   IsComplete,
    string FinalResponse,
    int    TotalIterations,
    IReadOnlyList<ConversationMessage> FinalHistory);

// ─── ReAct agent loop ─────────────────────────────────────────────────────────

// Implements the Reasoning and Acting pattern:
//   1. Send history + tools to language model
//   2. If response is text → append to history, continue
//   3. If response is TASK_COMPLETE → return
//   4. If token count exceeds compaction threshold → compact history
//   5. If iterations exceed maximum → return partial result
//
// Tool execution is delegated to the toolExecutor Func<name,args,result>.
public sealed class ReactAgentLoop
{
    private readonly ILlmGateway _llm;
    private readonly ContextCompactor _compactor;
    private readonly Func<string, Dictionary<string, object>, Task<string>>? _toolExecutor;
    private readonly int _maxIterations;
    private readonly int _compactionThresholdChars; // character count that triggers compaction

    public ReactAgentLoop(
        ILlmGateway   llm,
        ContextCompactor compactor,
        Func<string, Dictionary<string, object>, Task<string>>? toolExecutor = null,
        int maxIterations           = 50,
        int compactionThresholdChars = 40_000)
    {
        _llm                      = llm;
        _compactor                = compactor;
        _toolExecutor             = toolExecutor;
        _maxIterations            = maxIterations;
        _compactionThresholdChars = compactionThresholdChars;
    }

    public async Task<OperationResult<ReactLoopResult>> RunAsync(
        string task,
        CancellationToken cancellationToken = default)
    {
        var history = new List<ConversationMessage>
        {
            new("system", "You are a helpful coding agent."),
            new("user",   task)
        };

        int iteration = 0;

        while (iteration < _maxIterations)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<ReactLoopResult>.Fail("Cancelled", "CANCELLED");

            // Compact history if the total character count exceeds the threshold.
            int totalChars = history.Sum(m => m.Content.Length);
            if (totalChars > _compactionThresholdChars)
            {
                var compact = await _compactor.CompactAsync(history, msgs =>
                    Task.FromResult($"[summarized {msgs.Count} messages]"));
                if (compact.IsSuccess)
                    history = compact.Value!;
            }

            var llmResult = await _llm.SendAsync(history, cancellationToken);
            if (!llmResult.IsSuccess)
                return OperationResult<ReactLoopResult>.Fail(llmResult.ErrorMessage, llmResult.ErrorCode);

            string response = llmResult.Value!;
            iteration++;

            // Task complete signal.
            if (response.StartsWith("TASK_COMPLETE:", StringComparison.Ordinal))
            {
                string final = response["TASK_COMPLETE:".Length..];
                history.Add(new ConversationMessage("assistant", final));
                return OperationResult<ReactLoopResult>.Ok(
                    new ReactLoopResult(true, final, iteration, history));
            }

            // Tool call format: "TOOL:<name>:{...json args...}"
            if (response.StartsWith("TOOL:", StringComparison.Ordinal) && _toolExecutor is not null)
            {
                int secondColon = response.IndexOf(':', 5);
                string toolName = secondColon > 0 ? response[5..secondColon] : response[5..];
                string argsJson = secondColon > 0 ? response[(secondColon + 1)..] : "{}";

                Dictionary<string, object> args;
                try
                {
                    args = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, object>>(argsJson) ?? [];
                }
                catch { args = []; }

                string toolOutput = await _toolExecutor(toolName, args);
                history.Add(new ConversationMessage("assistant", $"[tool: {toolName}]"));
                history.Add(new ConversationMessage("tool", toolOutput));
                continue;
            }

            // Plain text response.
            history.Add(new ConversationMessage("assistant", response));
        }

        return OperationResult<ReactLoopResult>.Ok(
            new ReactLoopResult(
                false,
                "Maximum iterations reached without completing the task.",
                iteration,
                history));
    }
}
