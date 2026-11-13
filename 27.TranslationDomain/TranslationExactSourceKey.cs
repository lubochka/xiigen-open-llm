// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

// This file defines the address of the executor's exact translation memory.
//
// Problem it solves: the exact memory used to be addressed by source text only, so the
// lesson "bank in the finance topic is банк" and the lesson "bank in the river topic is
// берег" landed in one and the same cell and the second lesson erased the first one.
// The address below is the pair (topic domain, source text), so both known meanings of
// one word survive next to each other and a request picks its own cell.
//
// Deliberate boundaries of this file:
//   * there is no dictionary of reference translations here — only the address;
//   * there is no trained topic recogniser here — the topic already arrives in the request;
//   * user-supplied parts are never glued through a separator character, because an
//     unchecked separator inside the source text or the topic name would produce
//     colliding addresses; the typed pair below cannot collide that way.
//
// Governed by: docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 105-late-learning-transfer-plan-2026-10-30.md section 3.2 (FIX-P51-TOPIC).

// TranslationExactSourceKey is the typed address of one cell of the exact translation memory.
// Both halves are stored already normalized, so two equal addresses are always equal strings.
internal readonly record struct TranslationExactSourceKey
{
    // GeneralTopicDomain is the topic assigned to a request that names no topic at all.
    internal const string GeneralTopicDomain = "general";

    // LegacySourceOnlyVersion marks saved content whose exact memory was addressed by source only.
    internal const int LegacySourceOnlyVersion = 1;

    // ContextualTopicVersion marks saved content whose exact memory is addressed by topic plus source.
    internal const int ContextualTopicVersion = 2;

    // The constructor is private so that an address can only be produced by the shared
    // builder below; that keeps write-time and read-time addresses provably identical.
    private TranslationExactSourceKey(string topicDomain, string source)
    {
        // Store the already-normalized topic half of the address.
        TopicDomain = topicDomain;
        // Store the already-normalized source half of the address.
        Source = source;
    }

    // TopicDomain is the normalized topic half of the address, for example "finance" or "general".
    internal string TopicDomain { get; }

    // Source is the normalized source-text half of the address, produced by the product normalizer.
    internal string Source { get; }

    // IsAddressable reports whether this address may be written into or read out of the memory.
    // An address with an empty half is not a usable cell and is silently skipped at write time.
    internal bool IsAddressable
        => !string.IsNullOrWhiteSpace(TopicDomain) && !string.IsNullOrWhiteSpace(Source);

    // NormalizeTopicDomain turns a request topic into the canonical topic half of the address.
    // Trimming plus invariant lower-casing is the whole rule; an empty topic means "general".
    internal static string NormalizeTopicDomain(string? topicDomain)
    {
        // Treat a missing or blank topic as the general topic, exactly as the P51 experiment did.
        if (string.IsNullOrWhiteSpace(topicDomain))
            return GeneralTopicDomain;

        // Remove surrounding spaces and lower-case by invariant culture so "Finance" == "finance".
        return topicDomain.Trim().ToLowerInvariant();
    }

    // Create is the single shared address builder. Training and inference both call this one
    // method, so a lesson can never be written under an address the reader cannot reproduce.
    internal static TranslationExactSourceKey Create(string? sourceText, string? topicDomain)
        // The source half reuses the existing product normalizer; the topic half uses the rule above.
        => new(NormalizeTopicDomain(topicDomain), ExecutorUnit.NormalizeMapKey(sourceText ?? string.Empty));

    // TryCreateFromStoredPair rebuilds an address from a saved entry and proves the saved
    // halves were already canonical. A saved entry whose text is not canonical is rejected by
    // the importer instead of being quietly re-normalized into a different address.
    internal static bool TryCreateFromStoredPair(
        string? storedTopicDomain,
        string? storedSource,
        out TranslationExactSourceKey key)
    {
        // Start from a cleared address so a rejected entry never leaks a half-built value.
        key = default;

        // A saved entry with a missing or blank half carries no usable address.
        if (string.IsNullOrWhiteSpace(storedTopicDomain) || string.IsNullOrWhiteSpace(storedSource))
            return false;

        // Rebuild the address through the shared builder used by training and inference.
        var rebuilt = Create(storedSource, storedTopicDomain);

        // Compare both halves literally: the saved text must already equal the canonical text.
        if (!string.Equals(rebuilt.TopicDomain, storedTopicDomain, StringComparison.Ordinal) ||
            !string.Equals(rebuilt.Source, storedSource, StringComparison.Ordinal))
            return false;

        // Hand back the proven canonical address.
        key = rebuilt;
        return true;
    }
}
