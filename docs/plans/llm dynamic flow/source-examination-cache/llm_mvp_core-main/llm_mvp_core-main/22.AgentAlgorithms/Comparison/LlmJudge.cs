using System.Text.Json;

namespace AgentAlgorithms;

// Submits two candidate contexts (A = original, B = enhanced) to an LLM and returns
// a structured quality verdict on relevance, coverage, and clarity.
public sealed class LlmJudge
{
    private readonly ILlmGateway _llm;

    public LlmJudge(ILlmGateway llm) => _llm = llm;

    public async Task<OperationResult<JudgmentResult>> JudgeAsync(
        string            taskDescription,
        string            originalContext,
        string            enhancedContext,
        CancellationToken cancellationToken = default)
    {
        string prompt = $$"""
            You are evaluating two different contexts prepared for the same coding task.

            TASK: {{taskDescription}}

            CONTEXT A (original):
            {{originalContext}}

            CONTEXT B (enhanced, uses AST-packed representation):
            {{enhancedContext}}

            Compare these two contexts on three criteria:
            1. Relevance — does the context contain the files/methods needed for the task?
            2. Coverage — how much of the relevant codebase is included?
            3. Clarity — is the structural information clear and actionable?

            Answer in JSON:
            {
              "winner": "A or B or tie",
              "relevance_winner": "A or B or tie",
              "coverage_winner": "A or B or tie",
              "clarity_winner": "A or B or tie",
              "reasoning": "one paragraph explanation"
            }
            """;

        var response = await _llm.SendAsync(
            [new ConversationMessage("user", prompt)],
            cancellationToken);

        if (!response.IsSuccess)
            return OperationResult<JudgmentResult>.Fail(response.ErrorMessage, response.ErrorCode);

        return ParseJudgment(response.Value!);
    }

    private static OperationResult<JudgmentResult> ParseJudgment(string responseText)
    {
        try
        {
            int start = responseText.IndexOf('{');
            int end   = responseText.LastIndexOf('}');
            if (start < 0 || end < start)
                return OperationResult<JudgmentResult>.Fail("No JSON object in LLM response.");

            string json = responseText[start..(end + 1)];
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return OperationResult<JudgmentResult>.Ok(new JudgmentResult
            {
                Winner          = GetString(root, "winner"),
                RelevanceWinner = GetString(root, "relevance_winner"),
                CoverageWinner  = GetString(root, "coverage_winner"),
                ClarityWinner   = GetString(root, "clarity_winner"),
                Reasoning       = GetString(root, "reasoning"),
            });
        }
        catch (Exception ex)
        {
            return OperationResult<JudgmentResult>.Fail(
                $"Failed to parse LLM judgment: {ex.Message}");
        }
    }

    private static string GetString(JsonElement el, string key)
        => el.TryGetProperty(key, out var v) ? v.GetString() ?? "" : "";
}
