namespace OrchestratorModel;

using AgentInfrastructure;

// Null-object implementation of ISessionStorage for classes with no real I/O.
// Used in tests instead of a full stub. internal — not part of public API.
internal sealed class NullSessionStorage : ISessionStorage
{
    // Singleton — no state to share, safe to reuse.
    internal static readonly NullSessionStorage Instance = new();

    // Private constructor enforces singleton usage.
    private NullSessionStorage() { }

    // No-op save — null storage discards all writes.
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    // Always returns Fail — null storage never holds anything.
    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

    // Always returns an empty session list.
    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)Array.Empty<string>()));
}
