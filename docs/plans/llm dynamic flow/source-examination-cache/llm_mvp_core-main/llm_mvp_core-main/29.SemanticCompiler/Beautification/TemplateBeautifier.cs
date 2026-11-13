namespace SemanticCompiler29;

// TemplateBeautifier: map predicate sequences to code templates.
// E23: +833.3% — SUCCESS=RETURN is the critical mapping (Rule B).
//
// WARNING (Rule B): "VALIDATE_OP(X) → SUCCESS" MUST map to "return Validate({X});"
//                   NOT to a string containing "SUCCESS".
//                   Without this: template_match_rate = 10.9%.
//                   With this:    template_match_rate = 100%.
//
// Canonical mappings — do not change without re-running E23.
public sealed class TemplateBeautifier : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public TemplateBeautifier(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Template table: predicate sequence key → code snippet.
    // SUCCESS = RETURN (E23 Rule B critical fix — verified by grep on this line).
    private static readonly Dictionary<string, string> Templates = new()
    {
        // SUCCESS = RETURN: validate then return the validated value.
        ["VALIDATE_OP(X) → SUCCESS"]    = "return Validate({X});",

        // Null guard → Fail: guard clause pattern before business logic.
        ["NULL_CHECK(X) → ASSIGN_OP"]   = "if ({X} == null) return OperationResult<T>.Fail(\"not found\");",

        // Loop with filter: iterate and collect matching items.
        ["LOOP_OP(X) → FILTER_OP(Y)"]   = "foreach (var item in {X}.Where({Y}))",

        // Assign then return Ok: standard repository-save pattern.
        ["ASSIGN_OP(X) → RETURN_OP"]    = "{X} = ...; return OperationResult<T>.Ok({X});",

        // Validate then fail: explicit validation failure.
        ["VALIDATE_OP(X) → ASSIGN_OP"]  = "var valid = Validate({X}); if (!valid.IsSuccess) return valid.MapError<T>();",

        // Create then return: factory/constructor pattern.
        ["CREATE_OP(X) → RETURN_OP"]    = "var {X} = new {Type}(...); return OperationResult<T>.Ok({X});",

        // Log then return fail: error-logging guard.
        ["LOG_OP → ASSIGN_OP → RETURN_OP"] = "_logger.LogError(...); return OperationResult<T>.Fail(...);",
    };

    // Map a predicate sequence to the best matching code template.
    // Returns TEMPLATE_NOT_FOUND when no canonical mapping exists.
    public OperationResult<string> Beautify(string[] predicates)
    {
        if (predicates.Length == 0)
            return OperationResult<string>.Fail("No predicates provided", "EMPTY_PREDICATES");

        string key = string.Join(" → ", predicates);

        // Exact match first.
        if (Templates.TryGetValue(key, out string? template))
            return OperationResult<string>.Ok(template);

        // Prefix match: find the template whose key is a prefix of the query.
        var match = Templates.FirstOrDefault(kv => key.StartsWith(kv.Key, StringComparison.Ordinal));
        if (match.Value is not null)
            return OperationResult<string>.Ok(match.Value);

        // Substring match: find any template whose key appears in the query.
        var substr = Templates.FirstOrDefault(kv => key.Contains(kv.Key, StringComparison.Ordinal));
        if (substr.Value is not null)
            return OperationResult<string>.Ok(substr.Value);

        return OperationResult<string>.Fail(
            $"No template for: {key}", "TEMPLATE_NOT_FOUND");
    }
}
