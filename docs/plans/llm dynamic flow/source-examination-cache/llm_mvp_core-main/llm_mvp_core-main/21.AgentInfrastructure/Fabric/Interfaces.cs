namespace AgentInfrastructure;

// Reads configuration JSON files and plain-text instruction files from a source (disk or memory).
public interface IConfigurationProvider
{
    Task<OperationResult<Dictionary<string, object>>> ReadJsonAsync(string path);
    Task<OperationResult<string>> ReadTextAsync(string path);
}

// Reads source-code files from disk (or any other backing store in tests).
public interface ICodeReader
{
    Task<OperationResult<string>> ReadFileAsync(string path);
    Task<OperationResult<IReadOnlyList<string>>> ListFilesAsync(string directory, string searchPattern);
}

// Writes source-code files (used by FileChangeApplier for atomic patches).
public interface ICodeWriter
{
    Task<OperationResult<bool>> WriteFileAsync(string path, string content);
}

// Sends a conversation to the language model and receives a response.
// The response may contain plain text or a structured tool-call instruction.
public interface ILlmClient
{
    Task<OperationResult<LlmResponse>> SendAsync(
        IReadOnlyList<ConversationMessage> messageHistory,
        IReadOnlyList<ToolDefinition>? availableTools,
        CancellationToken cancellationToken);
}

// Saves and restores full agent sessions (message history + task + todo list).
public interface ISessionStorage
{
    Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state);
    Task<OperationResult<AgentState>> LoadAsync(string sessionId);
    Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync();
}
