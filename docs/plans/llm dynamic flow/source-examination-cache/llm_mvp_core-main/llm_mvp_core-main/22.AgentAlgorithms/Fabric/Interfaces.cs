namespace AgentAlgorithms;

// Content type for token budget estimation (E11: AST-tokens 5.2× more efficient for code).
public enum ContentType { CSharpCode, Documentation, NaturalLanguage, Json, SkillDocument }

// Estimates token cost by content type.
// CSharpCode: one AST node per ~20 characters (E11 5.2× density). Others: length/4 or length/5.
public interface ITokenBudgetCalculator
{
    int Estimate(string content, ContentType type);
}

// Structural index over a codebase — supports SyntaxKind:RoleId queries (E09 +45.3%).
public interface ICodeIndex
{
    void IndexFile(string fileId, string csharpSource);
    IReadOnlyList<(string FileId, double Score)> Search(CodeQuery query, int topK);
}

// Query for ICodeIndex: SyntaxKind name, optional role byte (E09), absence flag.
public record CodeQuery(string SyntaxKind, byte? RoleId = null, bool RequireAbsence = false);

// Decomposes a task description into independent sub-tasks (E08 RoleBasedTaskDecomposer).
public interface ITaskDecomposer
{
    OperationResult<IReadOnlyList<SubTask>> Decompose(string taskDescription);
}

// Provides access to a pre-built symbol graph for a codebase.
public interface ISymbolGraphProvider
{
    Task<OperationResult<List<Dictionary<string, object>>>> GetAllSymbolsAsync();
    Task<OperationResult<List<Dictionary<string, object>>>> GetReferencedByAsync(string symbolId);
}

// Runs semantic (similarity) search over a codebase index.
public interface ISemanticSearchProvider
{
    Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(string query, int topK);
}

// Sends tasks to sub-agents and receives their results.
public interface IAgentMessageBus
{
    Task<OperationResult<string>> AssignTaskAsync(string subAgentName, string task);
}

// Skill filter — selects relevant skills for a task from the full skill catalogue.
// E38v2: KNN on task predicates retrieves ~6 correct skills at 95% precision (86% load reduction).
// Returns all candidates when confidence < confidenceThreshold (fallback rate ≈ 22%).
public interface ISkillFilter
{
    Task<OperationResult<IReadOnlyList<TextChunk>>> FilterAsync(
        IReadOnlyList<TextChunk> skillCandidates,
        string                   taskDescription,
        int                      topK                = 5,
        double                   confidenceThreshold = 0.40);
}

// Calls the language model and returns a plain response.
public interface ILlmGateway
{
    Task<OperationResult<string>> SendAsync(
        IReadOnlyList<ConversationMessage> history,
        CancellationToken cancellationToken = default);
}
