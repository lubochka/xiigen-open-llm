namespace AgentInfrastructure;

// ─── Todo item ────────────────────────────────────────────────────────────────

public sealed record TodoItem(string Description, bool IsCompleted = false);

// ─── Agent state ──────────────────────────────────────────────────────────────

// Captures everything the agent needs to continue from where it stopped.
// Stored as a plain record so it serializes cleanly to SQLite.
public sealed record AgentState
{
    public string SessionId       { get; init; } = Guid.NewGuid().ToString();
    public string CurrentTask     { get; init; } = string.Empty;
    public int    IterationCount  { get; init; }

    public IReadOnlyList<ConversationMessage> MessageHistory { get; init; }
        = [];

    public IReadOnlyList<TodoItem> TodoList { get; init; }
        = [];

    // Append one message and return a new state (records are immutable).
    public AgentState WithMessage(ConversationMessage message)
        => this with
        {
            MessageHistory = [.. MessageHistory, message],
            IterationCount = IterationCount + 1
        };

    // Mark a todo item completed by index.
    public AgentState WithTodoCompleted(int index)
    {
        var updated = TodoList.Select((t, i) => i == index ? t with { IsCompleted = true } : t).ToList();
        return this with { TodoList = updated };
    }
}
