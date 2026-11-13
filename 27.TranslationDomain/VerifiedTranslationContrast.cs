// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// VerifiedTranslationContrast is the manifest entry a trusted calling process attaches to one
// candidate training pair after a person has compared that candidate with one already-trained
// pair and confirmed that exactly one language law differs between them.
// Why it exists: the training-history uniqueness filter measures only how many words two source
// sentences share. A sentence that differs by the single Russian word "не" ("not") shares five of
// six words with its positive twin, so the filter throws the opposite-meaning lesson away as a
// near duplicate and the translator never learns the negative form.
// What this record does NOT do: it never judges whether a translation is right. The product only
// checks the form of the human decision (identities, content hashes, direction, both sides really
// differ, a review receipt is present). The judgement itself stays with the person upstream, and
// one boolean from a manifest can never stand in for reading the sentence.
public sealed record VerifiedTranslationContrast(
    // Identifier of the candidate training pair this manifest entry is written for.
    string CandidateId,
    // Identifier of the already-trained pair the candidate is contrasted against.
    string ReferenceId,
    // Name of the single language law the person confirmed as changed; only polarity is accepted now.
    string ChangedLaw,
    // Content hash of the candidate pair, so one manifest cannot be reused for edited candidate bytes.
    string CandidateContentSha256,
    // Content hash of the reference pair, so the manifest cannot point at a rewritten history row.
    string ReferenceContentSha256,
    // Hash of the human review receipt; it records where the manual decision came from.
    string ReviewReceiptSha256,
    // The person's explicit statement that both sides were read and compared.
    bool Verified)
{
    // PolarityLaw is the only law name this first version accepts: a confirmed change of
    // affirmation into negation or back. A general, extensible language recogniser is deliberately
    // not created here, because every further law needs its own measured evidence first.
    public const string PolarityLaw = "polarity";

    // ReceiptHashLength is the exact length of a SHA-256 digest written as hexadecimal characters,
    // which is the shape a usable review receipt must have.
    private const int ReceiptHashLength = 64;

    // ComputeContentSha256 is the one serializer helper that both the manifest author and the
    // product must use, so the two sides can never disagree about what was hashed.
    // The hashed text is strictly the four fields that identify the lesson, written as a UTF-8 JSON
    // array in the fixed order identifier, direction, source side, chosen translation, with no
    // trimming and no normalisation: the bytes are hashed exactly as the pair carries them.
    public static string ComputeContentSha256(TranslationTrainReadyPair pair)
    {
        // A missing pair has no content to hash, and an empty result can never match a manifest value.
        if (pair is null)
            return string.Empty;

        // Serialise exactly the four designated fields in the designated order; a field that is
        // absent in the source data is written as a JSON null rather than silently skipped.
        var payload = JsonSerializer.Serialize(new string?[] { pair.Id, pair.Direction, pair.ASide, pair.Chosen });

        // Hash the UTF-8 bytes of that text, so the same four fields always produce the same digest.
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(payload));

        // Return the digest as lowercase hexadecimal, the form the manifest fields are written in.
        return Convert.ToHexString(digest).ToLowerInvariant();
    }

    // IsValidFor reports whether this manifest entry may excuse one named candidate from the
    // near-duplicate rule, measured against the one named pair already present in training history.
    // Every answer is a plain true or false: a missing or mismatched field returns false, so the
    // original uniqueness decision stands. It never throws and it never admits a pair unconditionally.
    public bool IsValidFor(TranslationTrainReadyPair candidate, TranslationTrainReadyPair reference)
    {
        // Both pairs must actually be present; a manifest alone is not a lesson.
        if (candidate is null || reference is null)
            return false;

        // The person must have stated plainly that both sides were read and compared.
        if (!Verified)
            return false;

        // Only the one law this version measured is accepted; any other word is refused.
        if (!string.Equals(ChangedLaw, PolarityLaw, StringComparison.Ordinal))
            return false;

        // The manifest must name the candidate it is written for, and that name must be this candidate.
        if (string.IsNullOrWhiteSpace(CandidateId) ||
            !string.Equals(CandidateId, candidate.Id, StringComparison.Ordinal))
            return false;

        // The manifest must name the old lesson it contrasts against, and that name must be this reference.
        if (string.IsNullOrWhiteSpace(ReferenceId) ||
            !string.Equals(ReferenceId, reference.Id, StringComparison.Ordinal))
            return false;

        // A pair cannot be a contrast of itself; the two lessons must be two different rows.
        if (string.Equals(CandidateId, ReferenceId, StringComparison.Ordinal))
            return false;

        // Both lessons must belong to the same translation direction, otherwise they are not comparable.
        if (string.IsNullOrWhiteSpace(candidate.Direction) ||
            !string.Equals(candidate.Direction, reference.Direction, StringComparison.Ordinal))
            return false;

        // The source sides must both exist and must really differ: an identical source sentence is an
        // exact correction of the same lesson, which this exception deliberately does not let through.
        if (!AreBothPresentAndDifferent(candidate.ASide, reference.ASide))
            return false;

        // The chosen translations must both exist and must really differ, otherwise no law changed and
        // the candidate teaches nothing the history does not already hold.
        if (!AreBothPresentAndDifferent(candidate.Chosen, reference.Chosen))
            return false;

        // The review receipt must be a full-length hexadecimal hash, so a blank or truncated
        // placeholder cannot pass as the record of a human decision.
        if (!IsHexHash(ReviewReceiptSha256))
            return false;

        // The candidate bytes must still be the bytes the person verified.
        if (!MatchesContentHash(CandidateContentSha256, candidate))
            return false;

        // The reference bytes must still be the bytes the person compared the candidate against.
        return MatchesContentHash(ReferenceContentSha256, reference);
    }

    // AreBothPresentAndDifferent checks that two sides of a comparison are both filled in and are not
    // the same text. Surrounding spaces and letter case are ignored for the difference test, so
    // re-typing the same sentence differently cannot be presented as a changed law.
    private static bool AreBothPresentAndDifferent(string? candidateSide, string? referenceSide)
    {
        // An empty side proves nothing and is refused before any comparison.
        if (string.IsNullOrWhiteSpace(candidateSide) || string.IsNullOrWhiteSpace(referenceSide))
            return false;

        // Compare the trimmed text case-insensitively; equal text means there is no contrast here.
        return !string.Equals(candidateSide.Trim(), referenceSide.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    // MatchesContentHash recomputes the content hash of a pair with the shared helper and compares it
    // with the value the manifest carries. Hexadecimal text is compared without regard to letter case,
    // because the two cases are the same digest, but nothing else about the value is adjusted.
    private static bool MatchesContentHash(string? declaredHash, TranslationTrainReadyPair pair)
    {
        // A manifest without a usable hash cannot prove which bytes were verified.
        if (!IsHexHash(declaredHash))
            return false;

        // Recompute the digest from the live pair and require an exact value match.
        return string.Equals(declaredHash, ComputeContentSha256(pair), StringComparison.OrdinalIgnoreCase);
    }

    // IsHexHash checks that a value is exactly a SHA-256 digest written as hexadecimal characters:
    // the right length and nothing but hexadecimal digits.
    private static bool IsHexHash(string? value)
    {
        // A blank value or a value of any other length is not a digest.
        if (string.IsNullOrEmpty(value) || value.Length != ReceiptHashLength)
            return false;

        // Every character must be a hexadecimal digit; spaces and other text are refused.
        foreach (var character in value)
            if (!Uri.IsHexDigit(character))
                return false;

        return true;
    }
}
