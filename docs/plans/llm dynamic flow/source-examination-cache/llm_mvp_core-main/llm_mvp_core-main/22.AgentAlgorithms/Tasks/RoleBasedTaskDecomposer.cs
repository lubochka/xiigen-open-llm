namespace AgentAlgorithms;

// Task decomposer that uses English role tagging to find true task boundaries.
// E08 +56.3%: roles detect VERB+NOUN / CONJ / VERB+NOUN patterns that naive conjunction-
// splitting misses.
// E21 +142.9%: AND splits only when BOTH sides have VERB+NOUN AND verbs are different —
// prevents "Find null AND log them" (right has no NOUN) from being split.
// E19 +18.2%: word-family boundary weight (VERB→VERB = strong, NOUN→NOUN = weak).
//
// EnglishRoleTagger logic is copied here — not referenced from 35.ExperimentsV2.
// Experiments and production must not share source files.
public sealed class RoleBasedTaskDecomposer : ITaskDecomposer
{
    private enum Role { Unknown, Verb, Noun, Adj, Adv, Prep, Det, Conj, Pron, Aux }

    private enum WordFamily { Verb, Noun, Other }

    private static readonly HashSet<string> _verbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "add","find","update","delete","create","remove","write","read","run","test",
        "log","fix","get","set","check","validate","process","parse","load","save",
        "send","receive","build","deploy","refactor","rename","move","search","verify",
        "extract","generate","render","call","return","throw","catch","handle",
    };

    private static readonly HashSet<string> _nouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "method","class","file","function","service","module","test","code","error",
        "bug","null","data","list","result","value","type","field","property","variable",
        "parameter","argument","interface","object","instance","model","view","controller",
        "database","query","table","record","response","request","endpoint",
    };

    private static readonly HashSet<string> _conjunctions = new(StringComparer.OrdinalIgnoreCase)
        { "and","or","but","nor" };

    private static readonly HashSet<string> _hardSplits = new(StringComparer.OrdinalIgnoreCase)
        { "then","also","additionally" };

    private static readonly HashSet<string> _pronouns = new(StringComparer.OrdinalIgnoreCase)
        { "them","it","they","this","that","these","those","its","their","our","my" };

    private static readonly HashSet<string> _articles = new(StringComparer.OrdinalIgnoreCase)
        { "a","an","the" };

    private static readonly HashSet<string> _preps = new(StringComparer.OrdinalIgnoreCase)
        { "in","on","at","by","for","with","about","from","to","of","into","through" };

    public OperationResult<IReadOnlyList<SubTask>> Decompose(string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<IReadOnlyList<SubTask>>.Fail("Task description is empty.", "EMPTY_TASK");

        var words = taskDescription.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var roles = words.Select(TagWord).ToArray();

        var splitIndices = new List<int>();
        for (int i = 0; i < words.Length; i++)
        {
            // Hard-split words always mark a boundary
            if (_hardSplits.Contains(Normalize(words[i])))
            {
                splitIndices.Add(i);
                continue;
            }

            // CONJ: E21 — split only when BOTH sides have VERB+NOUN with different verbs
            if (roles[i] == Role.Conj && i > 0 && i < words.Length - 1)
            {
                if (IsTaskBoundary(roles[..i], words[..i], roles[(i + 1)..], words[(i + 1)..]))
                    splitIndices.Add(i);
            }
        }

        if (splitIndices.Count == 0)
            return OperationResult<IReadOnlyList<SubTask>>.Ok(
                [new SubTask(taskDescription.Trim(), InferRole(taskDescription))]);

        var parts = new List<string>();
        int start = 0;
        foreach (int idx in splitIndices)
        {
            string part = string.Join(" ", words[start..idx]).Trim();
            if (!string.IsNullOrWhiteSpace(part)) parts.Add(part);
            start = idx + 1;
        }
        string last = string.Join(" ", words[start..]).Trim();
        if (!string.IsNullOrWhiteSpace(last)) parts.Add(last);

        var subTasks = parts
            .Select(p => new SubTask(p, InferRole(p)))
            .ToList();

        return OperationResult<IReadOnlyList<SubTask>>.Ok(subTasks);
    }

    private static Role TagWord(string word)
    {
        string lower = Normalize(word);
        if (_conjunctions.Contains(lower)) return Role.Conj;
        if (_pronouns.Contains(lower))     return Role.Pron;
        if (_articles.Contains(lower))     return Role.Det;
        if (_preps.Contains(lower))        return Role.Prep;
        if (_verbs.Contains(lower))        return Role.Verb;
        if (_nouns.Contains(lower))        return Role.Noun;
        if (lower.EndsWith("ing"))         return Role.Verb;
        if (lower.EndsWith("ed"))          return Role.Verb;
        if (lower.EndsWith("ly"))          return Role.Adv;
        if (lower.EndsWith("tion") || lower.EndsWith("ment") || lower.EndsWith("ness"))
            return Role.Noun;
        if (lower.EndsWith("ful") || lower.EndsWith("ous") || lower.EndsWith("ive"))
            return Role.Adj;
        return Role.Noun;
    }

    // E21: true boundary requires VERB+NOUN on both sides AND different verbs.
    private static bool IsTaskBoundary(Role[] leftRoles, string[] leftWords,
                                       Role[] rightRoles, string[] rightWords)
    {
        bool leftComplete  = leftRoles.Any(r => r == Role.Verb) && leftRoles.Any(r => r == Role.Noun);
        bool rightComplete = rightRoles.Any(r => r == Role.Verb) && rightRoles.Any(r => r == Role.Noun);

        if (!leftComplete || !rightComplete) return false;

        string? leftVerb  = leftWords.Zip(leftRoles).FirstOrDefault(p => p.Second == Role.Verb).First;
        string? rightVerb = rightWords.Zip(rightRoles).FirstOrDefault(p => p.Second == Role.Verb).First;
        return leftVerb != null && rightVerb != null
            && !string.Equals(Normalize(leftVerb), Normalize(rightVerb), StringComparison.OrdinalIgnoreCase);
    }

    // E19: VERB→VERB transition = strong boundary (1.0), NOUN→NOUN = weak (0.3).
    // Accepts raw words so callers outside the class can use it (Role enum is private).
    public static double ComputeBoundaryWeight(string leftContext, string rightContext)
    {
        var leftRoles  = leftContext.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(TagWord).ToArray();
        var rightRoles = rightContext.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(TagWord).ToArray();
        bool leftEndsVerb   = GetWordFamily(leftRoles.LastOrDefault())  == WordFamily.Verb;
        bool rightStartVerb = GetWordFamily(rightRoles.FirstOrDefault()) == WordFamily.Verb;
        return leftEndsVerb && rightStartVerb ? 1.0 : 0.3;
    }

    private static WordFamily GetWordFamily(Role r) => r switch
    {
        Role.Verb => WordFamily.Verb,
        Role.Noun => WordFamily.Noun,
        _         => WordFamily.Other,
    };

    private static string Normalize(string w) => w.ToLowerInvariant().Trim('.', ',', ';', '!', '?');

    private static string InferRole(string part)
    {
        var lower = part.ToLowerInvariant();
        if (lower.Contains("read") || lower.Contains("search") || lower.Contains("find"))
            return "reader";
        if (lower.Contains("refactor") || lower.Contains("rename") || lower.Contains("move"))
            return "refactorer";
        if (lower.Contains("test") || lower.Contains("verify"))
            return "tester";
        return "coder";
    }
}
