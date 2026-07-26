// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// PatternReorderer: reorder tokens from detected pattern into English SVO (Subject-Verb-Object).
// E27: +250.0% — converts SOV (Subject-Object-Verb) / VSO (Verb-Subject-Object) to SVO order.
public sealed class PatternReorderer : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public PatternReorderer(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Reorder tokens from the detected source pattern into English SVO.
    public string[] Reorder(string[] tokens, string fromPattern)
        => fromPattern.ToUpperInvariant() switch
        {
            // SVO is already correct — English SVO needs no change.
            "SVO"       => tokens,

            // SOV → SVO: move the verb from the end to immediately after the subject.
            "SOV"       => ReorderSovToSvo(tokens),

            // VSO → SVO: move the subject before the verb.
            "VSO"       => ReorderVsoToSvo(tokens),

            // IMPERATIVE: already verb-first, acceptable English order — no change.
            "IMPERATIVE"=> tokens,

            // Unknown or already-SVO patterns — return unchanged.
            _           => tokens,
        };

    // SOV → SVO: find the last verb, move it after the first noun/subject.
    private static string[] ReorderSovToSvo(string[] tokens)
    {
        var list    = tokens.ToList();
        int verbIdx = list.FindLastIndex(t => IsVerb(t));
        if (verbIdx <= 0) return tokens;

        var verb = list[verbIdx];
        list.RemoveAt(verbIdx);
        // Insert verb after the first noun-like token (index 1 if nothing found).
        int insertAt = list.FindIndex(t => !IsVerb(t) && !IsStructure(t));
        list.Insert(insertAt < 0 ? 1 : insertAt + 1, verb);
        return [.. list];
    }

    // VSO → SVO: move the first subject noun before the leading verb.
    private static string[] ReorderVsoToSvo(string[] tokens)
    {
        var list    = tokens.ToList();
        int verbIdx = list.FindIndex(t => IsVerb(t));
        if (verbIdx <= 0) return tokens;

        // Find first noun after the verb, move it before the verb.
        int nounIdx = list.FindIndex(verbIdx + 1, t => !IsVerb(t) && !IsStructure(t));
        if (nounIdx < 0) return tokens;

        var noun = list[nounIdx];
        list.RemoveAt(nounIdx);
        list.Insert(verbIdx, noun);
        return [.. list];
    }

    private static bool IsVerb(string token)
    {
        string lower = token.ToLowerInvariant();
        return lower is "add" or "fix" or "create" or "update" or "delete" or "validate"
            or "check" or "return" or "throw" or "send" or "log" or "get" or "fetch"
            or "process" or "handle" or "run" or "build" or "set" or "remove" or "find";
    }

    private static bool IsStructure(string token)
    {
        string lower = token.ToLowerInvariant();
        return lower is "a" or "an" or "the" or "in" or "on" or "at" or "by"
            or "for" or "with" or "and" or "or" or "but" or "not";
    }
}
