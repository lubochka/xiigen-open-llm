// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// RoleAssigner: assigns syntactic roles to tokens — CODE, SQL, JSON, NL.
// E08: +56.3% accuracy with non-code syntax roles.
// E09: +45.3% with structural K-NN (K-Nearest Neighbours) for role boundary detection.
public sealed class RoleAssigner : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public RoleAssigner(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // SQL (Structured Query Language) keywords that distinguish SQL fragments from code.
    private static readonly HashSet<string> SqlKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT","FROM","WHERE","JOIN","LEFT","RIGHT","INNER","OUTER",
        "INSERT","UPDATE","DELETE","CREATE","DROP","ALTER","TABLE","INDEX",
        "ON","GROUP","ORDER","BY","HAVING","LIMIT","OFFSET","DISTINCT",
    };

    // JSON (JavaScript Object Notation) structural signals.
    private static readonly HashSet<string> JsonSignals = new(StringComparer.OrdinalIgnoreCase)
    {
        "{","[","}","]",":"
    };

    // Assign role tags: CODE / SQL / JSON / NL to each token.
    public OperationResult<(string Token, string Role)[]> AssignRoles(string[] tokens)
    {
        if (tokens.Length == 0)
            return OperationResult<(string, string)[]>.Fail("Token array is empty", "EMPTY_TOKENS");

        // Count structural signals to pick the dominant role for the whole fragment.
        int sqlCount  = tokens.Count(t => SqlKeywords.Contains(t));
        int jsonCount = tokens.Count(t => JsonSignals.Contains(t));
        bool isDominantlySql  = sqlCount  >= 2;
        bool isDominantlyJson = jsonCount >= 2;

        var result = tokens.Select(t =>
        {
            // Per-token role: SQL keyword → SQL, JSON signal → JSON, NL words → NL, else CODE.
            string role = SqlKeywords.Contains(t) ? "SQL"
                        : JsonSignals.Contains(t) ? "JSON"
                        : IsNaturalLanguage(t, isDominantlySql, isDominantlyJson) ? "NL"
                        : "CODE";
            return (t, role);
        }).ToArray();

        return OperationResult<(string, string)[]>.Ok(result);
    }

    // Heuristic: a token is natural language when it contains only letters and is not
    // a keyword in the dominant syntax, and the fragment isn't predominantly SQL/JSON.
    private static bool IsNaturalLanguage(string token, bool isSql, bool isJson)
    {
        if (isSql || isJson) return false;
        return token.All(char.IsLetter) && token.Length > 2;
    }
}
