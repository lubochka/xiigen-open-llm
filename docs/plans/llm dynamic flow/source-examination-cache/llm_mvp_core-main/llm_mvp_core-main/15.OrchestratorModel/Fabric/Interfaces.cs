namespace OrchestratorModel;

using AgentInfrastructure;

// Fabric interface for template-based text beautification.
// ResultMerger accepts this interface — never SemanticCompiler29.TemplateBeautifier directly.
// Implemented by TemplateBeautifierAdapter in Adapters/ (Fabric First, Rule 2).
public interface ITemplateBeautifier
{
    // Beautify a merged result string. Returns TEMPLATE_NOT_FOUND when no mapping exists —
    // callers fall back to the raw text in that case.
    OperationResult<string> Beautify(string text);
}
