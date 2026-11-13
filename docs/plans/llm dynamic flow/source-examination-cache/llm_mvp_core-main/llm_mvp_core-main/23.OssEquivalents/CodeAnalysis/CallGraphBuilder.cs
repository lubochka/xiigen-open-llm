namespace OssEquivalents;

// Builds a directed call graph from the symbol list produced by RoslynCodeAnalyzer.
// Each method's "calledBy" list is populated by scanning other methods' "calls" lists.
public sealed class CallGraphBuilder
{
    // Enrich the symbol list with reverse-call edges (calledBy).
    // The input list is mutated in place for efficiency.
    public OperationResult<List<Dictionary<string, object>>> Build(
        List<Dictionary<string, object>> symbols)
    {
        if (symbols is null)
            return OperationResult<List<Dictionary<string, object>>>.Fail(
                "Symbol list cannot be null.", "NULL_INPUT");

        // Build a name → symbol lookup.
        var byName = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
        foreach (var sym in symbols)
        {
            if (sym.TryGetValue("name", out var nameObj) && nameObj is string name)
                byName[name] = sym;
        }

        // For every method that has a "calls" list, add the caller to each callee's "calledBy".
        foreach (var sym in symbols)
        {
            if (!sym.TryGetValue("name", out var callerNameObj)) continue;
            string callerName = callerNameObj.ToString()!;

            if (!sym.TryGetValue("calls", out var callsObj)) continue;
            if (callsObj is not List<string> calls) continue;

            foreach (var callee in calls)
            {
                if (!byName.TryGetValue(callee, out var calleeSymbol)) continue;
                if (calleeSymbol["calledBy"] is List<string> calledBy)
                    calledBy.Add(callerName);
            }
        }

        return OperationResult<List<Dictionary<string, object>>>.Ok(symbols);
    }
}
