namespace AgentAlgorithms;

// Reduces the size of the message history when it grows too large.
// Preserves the most recent N turns in full and summarizes older turns into one message.
public sealed class ContextCompactor
{
    // How many of the most recent message turns to always keep verbatim.
    public int RecentTurnsToPreserve { get; }

    public ContextCompactor(int recentTurnsToPreserve = 10)
        => RecentTurnsToPreserve = recentTurnsToPreserve;

    // Compact a message history.
    // summaryGenerator — function that summarizes older turns (real LLM call or test stub).
    public async Task<OperationResult<List<ConversationMessage>>> CompactAsync(
        List<ConversationMessage> messageHistory,
        Func<List<ConversationMessage>, Task<string>> summaryGenerator)
    {
        int olderCount = Math.Max(0, messageHistory.Count - RecentTurnsToPreserve);
        var olderTurns  = messageHistory.Take(olderCount).ToList();
        var recentTurns = messageHistory.Skip(olderCount).ToList();

        // Nothing old enough to summarize — return as-is.
        if (olderTurns.Count == 0)
            return OperationResult<List<ConversationMessage>>.Ok(messageHistory);

        string summary = await summaryGenerator(olderTurns);

        var summaryMessage = new ConversationMessage(
            "system",
            $"[Summary of earlier conversation: {summary}]");

        var compacted = new List<ConversationMessage> { summaryMessage };
        compacted.AddRange(recentTurns);

        return OperationResult<List<ConversationMessage>>.Ok(compacted);
    }
}
