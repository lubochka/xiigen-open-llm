namespace SemanticCompiler29;

// FamilyCollapser: collapse word families to root semantic op form.
// E16: +8.4% — "validation", "validates", "validated" → "validate" → VALIDATE_OP.
// Runs after SemanticNormalizer: operates on already-stemmed, stop-word-free tokens.
public sealed class FamilyCollapser : BaseService, IFamilyCollapser29
{
    // Standard constructor: inject storage and logger without constructing them here.
    public FamilyCollapser(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Synonym → canonical root op mapping (prefix-match enabled).
    private static readonly Dictionary<string, string> SynonymMap =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // GET_OP family: reading, querying, finding data.
        ["get"]="GET_OP", ["fetch"]="GET_OP", ["find"]="GET_OP", ["load"]="GET_OP",
        ["query"]="GET_OP", ["read"]="GET_OP", ["select"]="GET_OP", ["lookup"]="GET_OP",
        ["search"]="GET_OP", ["retriev"]="GET_OP", ["explain"]="GET_OP",
        ["list"]="GET_OP", ["show"]="GET_OP",
        // VALIDATE_OP family: checking, verifying, asserting.
        ["valid"]="VALIDATE_OP", ["check"]="VALIDATE_OP", ["verif"]="VALIDATE_OP",
        ["assert"]="VALIDATE_OP", ["confirm"]="VALIDATE_OP", ["ensur"]="VALIDATE_OP",
        ["test"]="VALIDATE_OP",
        // CREATE_OP family: creating, building, generating.
        ["creat"]="CREATE_OP", ["make"]="CREATE_OP", ["build"]="CREATE_OP",
        ["construct"]="CREATE_OP", ["init"]="CREATE_OP", ["generat"]="CREATE_OP",
        ["add"]="CREATE_OP", ["implement"]="CREATE_OP",
        // UPDATE_OP family: modifying, saving, persisting.
        ["updat"]="UPDATE_OP", ["set"]="UPDATE_OP", ["write"]="UPDATE_OP",
        ["save"]="UPDATE_OP", ["store"]="UPDATE_OP", ["modifi"]="UPDATE_OP",
        // DELETE_OP family: removing, clearing.
        ["delet"]="DELETE_OP", ["remov"]="DELETE_OP", ["clear"]="DELETE_OP",
        ["clean"]="DELETE_OP", ["reset"]="DELETE_OP",
        // PROCESS_OP family: executing, handling, running.
        ["process"]="PROCESS_OP", ["handl"]="PROCESS_OP", ["execut"]="PROCESS_OP",
        ["run"]="PROCESS_OP", ["perform"]="PROCESS_OP", ["fix"]="PROCESS_OP",
        ["refactor"]="PROCESS_OP", ["extract"]="PROCESS_OP",
        // CONVERT_OP family: transforming, parsing, formatting.
        ["convert"]="CONVERT_OP", ["pars"]="CONVERT_OP", ["transform"]="CONVERT_OP",
        ["format"]="CONVERT_OP", ["serial"]="CONVERT_OP",
        // SEND_OP family: emitting, publishing, notifying.
        ["send"]="SEND_OP", ["publish"]="SEND_OP", ["emit"]="SEND_OP",
        ["dispatch"]="SEND_OP", ["notifi"]="SEND_OP",
        // LOG_OP family: logging, tracing, recording.
        ["log"]="LOG_OP", ["trace"]="LOG_OP", ["debug"]="LOG_OP",
        ["record"]="LOG_OP", ["track"]="LOG_OP",
        // AUTH_OP family: authentication, authorization.
        ["auth"]="AUTH_OP", ["login"]="AUTH_OP", ["token"]="AUTH_OP",
        ["permiss"]="AUTH_OP", ["access"]="AUTH_OP",
    };

    // Collapse a token array: known stems → canonical op, unknown → unchanged.
    public string[] Collapse(string[] tokens)
        => tokens.Select(CollapseOne).ToArray();

    // Collapse a single token via direct match then longest-prefix match.
    private static string CollapseOne(string token)
    {
        if (SynonymMap.TryGetValue(token, out string? op)) return op;
        string? bestKey = null;
        foreach (var key in SynonymMap.Keys)
            if (token.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                if (bestKey is null || key.Length > bestKey.Length)
                    bestKey = key;
        return bestKey is not null ? SynonymMap[bestKey] : token;
    }
}
