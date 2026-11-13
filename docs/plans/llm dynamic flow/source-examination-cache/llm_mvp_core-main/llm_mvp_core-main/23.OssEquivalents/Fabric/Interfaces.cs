namespace OssEquivalents;

// Calls the language model and validates the response against a declared JSON Schema.
// Retries automatically if the response does not match the schema.
public interface ILlmClientWithSchema
{
    Task<OperationResult<Dictionary<string, object>>> SendWithSchemaAsync(
        IReadOnlyList<ConversationMessage> messageHistory,
        Dictionary<string, object>        expectedSchema,
        int                               maximumRetryCount = 3,
        CancellationToken                 cancellationToken = default);
}

// Executes a named tool with given arguments and returns the result.
public interface IToolExecutor
{
    Task<OperationResult<Dictionary<string, object>>> ExecuteAsync(
        string                     toolName,
        Dictionary<string, object> arguments,
        CancellationToken          cancellationToken = default);
}
