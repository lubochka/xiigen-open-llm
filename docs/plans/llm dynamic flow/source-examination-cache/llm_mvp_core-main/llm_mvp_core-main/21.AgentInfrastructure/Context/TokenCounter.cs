namespace AgentInfrastructure;

// Estimates the token count of a string without calling the language model.
// Uses the approximation: 1 token ≈ 0.75 words (standard heuristic for English text).
public static class TokenCounter
{
    // Estimate how many tokens the given text would consume.
    public static int Estimate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        int wordCount = text.Split((char[])[], StringSplitOptions.RemoveEmptyEntries).Length;
        return (int)Math.Ceiling(wordCount / 0.75);
    }

    // Sum the estimated token counts for a list of conversation messages.
    public static int EstimateHistory(IReadOnlyList<ConversationMessage> history)
        => history.Sum(m => Estimate(m.Role) + Estimate(m.Content) + 4); // 4 overhead per message
}
