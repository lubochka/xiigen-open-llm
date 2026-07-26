// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

// Owns the single ordered leakage-exclusion vocabulary shared by structural and runtime authority guards.
public static class DynamicGraphEdgeAuthorityLeakageSignalVocabulary
{
    private static readonly string[] OrderedSignals =
    [
        "candidate_index",
        "source_path",
        "source_id",
        "file_order",
        "correlation_id",
        "exact_prompt"
    ];

    // Exactness is ordinal and positional: missing, reordered, duplicate, and additional signals all fail closed.
    public static bool IsExact(IReadOnlyList<string>? signals)
    {
        if (signals is null || signals.Count != OrderedSignals.Length)
            return false;

        for (var index = 0; index < OrderedSignals.Length; index++)
        {
            if (!StringComparer.Ordinal.Equals(signals[index], OrderedSignals[index]))
                return false;
        }

        return true;
    }

    // Callers receive an owned snapshot and cannot mutate the authoritative vocabulary.
    public static string[] CreateSnapshot()
        => OrderedSignals.ToArray();
}
