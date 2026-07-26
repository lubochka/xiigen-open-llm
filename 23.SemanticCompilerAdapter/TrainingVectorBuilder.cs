// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompilerAdapter;

using System.Text.RegularExpressions;
using SemanticCompiler29;

// Production home for the E51B teaching/runtime symmetry vector contract.
public static class TrainingVectorBuilder
{
    public const int VectorWidth = 60;
    public const int PredicateSlotCount = 22;
    public const int DomainSlotCount = 5;
    public const int RootSlotCount = 32;

    public static float[] Build(string prompt)
    {
        var rootVocab = BuildRootVocabulary([prompt]);
        return Build(prompt, rootVocab);
    }

    public static float[] Build(DevelopmentPromptCompilation compiled)
    {
        var vectorText = string.Join(" ",
            string.Join(" ", compiled.DevelopmentPredicateIds),
            compiled.NormalizedText,
            string.Join(" ", compiled.PipelinePredicates),
            $"semantic_domain:{compiled.Domain}");
        var roots = NormalizeRoots(vectorText).ToArray();
        var predicates = compiled.DevelopmentPredicateIds
            .Concat(compiled.PipelinePredicates)
            .Concat(roots.SelectMany(PredicateNamesForRoot))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var rootVocab = BuildRootVocabulary([
            compiled.NormalizedText,
            compiled.Text,
            string.Join(" ", compiled.DevelopmentPredicateIds),
            string.Join(" ", compiled.PipelinePredicates)
        ]);
        return BuildVector(roots, predicates, vectorText, rootVocab);
    }

    public static float[] Build(string prompt, IReadOnlyDictionary<string, int> rootVocab)
    {
        var roots = NormalizeRoots(prompt).ToArray();
        var predicates = roots.SelectMany(PredicateNamesForRoot).Distinct(StringComparer.Ordinal).ToArray();
        return BuildVector(roots, predicates, prompt, rootVocab);
    }

    public static Dictionary<string, int> BuildRootVocabulary(IEnumerable<string> prompts)
    {
        return prompts
            .SelectMany(NormalizeRoots)
            .Distinct(StringComparer.Ordinal)
            .Take(RootSlotCount)
            .Select((root, index) => (root, index))
            .ToDictionary(item => item.root, item => item.index, StringComparer.Ordinal);
    }

    public static string[] ExtractPredicates(string prompt)
    {
        return NormalizeRoots(prompt)
            .SelectMany(PredicateNamesForRoot)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public static string[] ExtractRoots(string prompt)
    {
        return NormalizeRoots(prompt).ToArray();
    }

    public static float[] BuildVector(
        string[] roots,
        string[] predicates,
        string prompt,
        IReadOnlyDictionary<string, int> rootVocab)
    {
        var vector = new float[VectorWidth];
        foreach (var predicate in predicates)
        {
            var slot = PredicateSlotFor(predicate);
            if (slot >= 0 && slot < PredicateSlotCount)
                vector[slot] += 1f;
        }

        var domainSlot = PredicateSlotCount + PredictDomainSlot(prompt);
        vector[domainSlot] = 1f;

        foreach (var root in roots)
        {
            if (rootVocab.TryGetValue(root, out var slot) && slot < RootSlotCount)
                vector[PredicateSlotCount + DomainSlotCount + slot] += 1f;
        }

        vector[VectorWidth - 1] = prompt.Contains(" and ", StringComparison.OrdinalIgnoreCase) ? 2f : 1f;
        return vector;
    }

    public static IEnumerable<string> NormalizeRoots(string text)
    {
        foreach (var raw in Regex.Split((text ?? string.Empty).ToLowerInvariant(), @"[^a-z]+"))
        {
            if (raw.Length < 2)
                continue;

            var root = Stem(raw) switch
            {
                "mailbox" => "email",
                "profile" => "provider",
                "github" => "provider",
                "google" => "provider",
                "people" => "user",
                "person" => "user",
                "repeated" => "duplicate",
                "repeat" => "duplicate",
                "twice" => "duplicate",
                "expires" => "expire",
                "expired" => "expire",
                "finish" => "complete",
                "finishes" => "complete",
                var value => value
            };

            if (root is "want" or "using" or "with" or "when" or "what" or "happens" or "make" or "after" or "before")
                continue;

            yield return root;
        }
    }

    public static IEnumerable<string> PredicateNamesForRoot(string root)
    {
        if (root is "provider" or "login" or "sign" or "token" or "session" or "cookie")
            yield return "AUTH_OP";
        if (root is "create" or "account" or "register" or "signup" or "join")
            yield return "CREATE_OP";
        if (root is "verify" or "verification" or "confirm" or "email")
            yield return "VALIDATE_OP";
        if (root is "send" or "mail")
            yield return "SEND_OP";
        if (root is "duplicate" or "same" or "callback" or "existing")
            yield return "GET_OP";
        if (root is "expire" or "invalid" or "stop")
            yield return "FAIL";
        if (root is "safe" or "secure")
            yield return "AUTH_OP";
    }

    public static int PredicateSlotFor(string predicate) => predicate switch
    {
        "SECURITY" => 0,
        "FAIL" => 1,
        "AUTHORIZATION" => 2,
        "API_CONTRACT" => 3,
        "ARCHITECTURE" => 4,
        "IMPLEMENT" => 5,
        "TEST" => 6,
        "QA" => 7,
        "UI" => 8,
        "DOCUMENT" => 9,
        "LEARNING" => 10,
        "GET_OP" => 12,
        "VALIDATE_OP" => 13,
        "CREATE_OP" => 14,
        "SEND_OP" => 19,
        "AUTH_OP" => 21,
        _ => -1
    };

    private static int PredictDomainSlot(string prompt)
    {
        var lower = (prompt ?? string.Empty).ToLowerInvariant();
        if (lower.Contains("semantic_domain:bug", StringComparison.Ordinal))
            return 1;
        if (lower.Contains("semantic_domain:plan", StringComparison.Ordinal))
            return 2;
        if (lower.Contains("semantic_domain:arch", StringComparison.Ordinal))
            return 3;
        if (lower.Contains("semantic_domain:refactor", StringComparison.Ordinal))
            return 4;
        if (lower.Contains("semantic_domain:code_q", StringComparison.Ordinal))
            return 0;
        if (ContainsAny(lower, "fix", "bug", "error", "crash", "null", "fails", "wrong"))
            return 1;
        if (ContainsAny(lower, "plan", "orchestrate", "workflow", "phase", "roadmap"))
            return 2;
        if (ContainsAny(lower, "architecture", "topology", "design", "registry", "adapter"))
            return 3;
        if (ContainsAny(lower, "refactor", "cleanup", "quality", "simplify"))
            return 4;
        return 0;
    }

    private static bool ContainsAny(string text, params string[] terms)
    {
        return terms.Any(term => text.Contains(term, StringComparison.Ordinal));
    }

    private static string Stem(string token)
    {
        if (token.Length > 4 && token.EndsWith("ing", StringComparison.Ordinal))
            return token[..^3];
        if (token.Length > 3 && token.EndsWith("ed", StringComparison.Ordinal))
            return token[..^2];
        if (token.Length > 3 && token.EndsWith("s", StringComparison.Ordinal))
            return token[..^1];
        return token;
    }
}
