namespace OrchestratorModel;

using AgentInfrastructure;

// Adapts SemanticCompiler29.TemplateBeautifier behind the ITemplateBeautifier fabric interface.
// This is the only file in 15.OrchestratorModel that names SemanticCompiler29.TemplateBeautifier
// as a concrete type (Fabric First, Rule 2).
public sealed class TemplateBeautifierAdapter : ITemplateBeautifier
{
    // The wrapped SC29 beautifier — held by interface so tests can sub-class.
    private readonly SemanticCompiler29.TemplateBeautifier _inner;

    public TemplateBeautifierAdapter(SemanticCompiler29.TemplateBeautifier inner)
        => _inner = inner;

    // Convert the merged result string into a predicate array for TemplateBeautifier.
    // TemplateBeautifier joins predicates with " → " internally, so splitting on " → "
    // reconstructs the original sequence when the text came from a pipeline output.
    // For natural-language merged results the split produces tokens that rarely match
    // any template, causing TEMPLATE_NOT_FOUND — callers fall back to the raw text.
    public OperationResult<string> Beautify(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return OperationResult<string>.Fail("Text cannot be empty.", "EMPTY_TEXT");

        var predicates = text.Split(" → ", StringSplitOptions.RemoveEmptyEntries);
        var result     = _inner.Beautify(predicates);

        // Map SemanticCompiler29.OperationResult to AgentInfrastructure.OperationResult.
        return result.IsSuccess
            ? OperationResult<string>.Ok(result.Value!)
            : OperationResult<string>.Fail(result.ErrorMessage, result.ErrorCode);
    }
}
