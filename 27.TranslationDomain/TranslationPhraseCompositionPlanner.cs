// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

// This file holds the phrase-composition planner used by ExecutorUnit when it assembles a
// translation out of learned phrases.
//
// The problem it solves. The assembler used to walk the source words from left to right and at
// every position take the longest learned phrase that happened to start there. That single choice
// was never reconsidered, so one long phrase could swallow a word that a different, shorter split
// would have translated as well. On the lesson set "close user", "user session", "close" the exam
// "close user session" lost the word "session": the long phrase "close user" was taken first, the
// tail "session" had no learned phrase of its own, the coverage stayed at two of three source
// words, and the whole phrase branch then refused to answer.
//
// What this planner does instead. It treats the positions between source words as points and every
// learned phrase that matches at a position as a jump from that point to the point after the
// phrase. One more jump of a single word is always available and covers nothing: that is how a
// word with no learned phrase is skipped. Among all the ways to walk from the first point to the
// last one it returns the single best one.
//
// Why it is cheap. The best walk is built from the end backwards, and only ONE best walk is kept
// per point. A direct search that enumerated every combination of phrases would hold every
// finished variant in memory at once; the earlier prototype of this repair paid about three
// quarters more allocated memory for exactly that. Keeping one best walk per point needs at most
// one entry per source word, so the full optimal answer is reached without storing the variants.
//
// What it deliberately does NOT claim. Covering more source words is not the same as understanding
// which word plays which role. Two splits that cover equally many words are equally good to this
// planner, and the tie is then broken by a fixed written-down order so that the same learned memory
// always produces the same answer. The planner never looks at an expected or correct translation:
// it only reads the learned targets.
//
// Governed by: plan 109 section "FIX-P54-PHRASE-COVERAGE", experiment 54.

// TranslationPhraseEdge is one learned phrase offered to the planner: the normalized address it is
// stored under, that address already cut into words, and the learned translation of it.
public sealed record TranslationPhraseEdge(
    // SourceKey is the normalized source address of the phrase, used for the fixed tie-break order.
    string SourceKey,
    // SourceTokens is the same address cut into the individual normalized source words.
    IReadOnlyList<string> SourceTokens,
    // Target is the learned translation that this phrase contributes to the answer.
    string Target);

// TranslationPhrasePlan is the chosen composition: how many source words it covers, out of how many
// phrases it is built, which learned addresses were used, and the translated pieces in order.
public sealed record TranslationPhrasePlan(
    // CoveredSourceTokens counts the source words that a learned phrase actually translated.
    int CoveredSourceTokens,
    // PhraseSpanCount counts how many separate learned phrases the answer is built from.
    int PhraseSpanCount,
    // ChosenSourceKeys lists the learned addresses in the order they appear in the answer.
    IReadOnlyList<string> ChosenSourceKeys,
    // Parts lists the learned translations in the order they must be joined.
    IReadOnlyList<string> Parts);

// TranslationPhraseCompositionPlanner picks the best split of a source sentence into learned
// phrases. It is a pure function over its arguments: it holds no state, trains nothing, and reads
// no expected answer, so the product path and the contract checks can share this one implementation.
public static class TranslationPhraseCompositionPlanner
{
    // AlgorithmIdentity names the selection rule of the SINGLE-ANSWER search below (the Plan
    // method), so a report or a saved bundle can say which rule produced an answer instead of
    // leaving the reader to guess. Its semantics are "most covered words, then fewest phrases,
    // then the written-down order of addresses", and the admission rule of the caller plays no
    // part in it. This value is deliberately unchanged: the records and the expectations that
    // already carry it describe exactly that rule, and renaming it would silently relabel them.
    public const string AlgorithmIdentity = "phrase-coverage-canonical-v1";

    // AdmissibleAlgorithmIdentity names the selection rule of the PlanAdmissible method, which is
    // what the product calls. Its semantics are different and must be distinguishable in evidence:
    // "best among the splits the caller's admission rule accepts, and honestly no plan when it
    // accepts none". One shared value for both methods would make the records of two different
    // selection rules indistinguishable, and a later reader of a receipt could not say which rule
    // produced it — that is why there are two values and not one.
    public const string AdmissibleAlgorithmIdentity = "phrase-coverage-admissible-v2";

    // MaxPlannedSourceTokens is the length of source sentence this planner is admitted for. A
    // longer sentence keeps the earlier left-to-right assembler, because the cost of the full
    // choice was only measured and agreed for sentences up to this length.
    public const int MaxPlannedSourceTokens = 8;

    // Plan returns the best split of the given source words into the given learned phrases.
    // Best means, in this exact order: the most covered source words; then the fewest separate
    // phrases; then the alphabetically smallest sequence of learned addresses, which is only a
    // fixed tie-break and not a claim that this split is the semantically right one.
    public static TranslationPhrasePlan Plan(
        IReadOnlyList<string> normalizedSourceTokens,
        IReadOnlyList<TranslationPhraseEdge> learnedPhrases)
    {
        // An answer that covers nothing is the starting point and also the answer for no input.
        var emptyPlan = new TranslationPhrasePlan(0, 0, [], []);
        // With no source words there is nothing to cover, so the empty answer is already the best.
        if (normalizedSourceTokens is null || normalizedSourceTokens.Count == 0)
            return emptyPlan;

        // Count the source words this call has to walk across.
        var tokenCount = normalizedSourceTokens.Count;

        // Keep only usable phrases and put them into one fixed order by their learned address, so
        // that the answer never depends on the order the learned memory happens to enumerate in.
        var usablePhrases = (learnedPhrases ?? [])
            .Where(phrase => phrase is not null)
            .Where(phrase => phrase.SourceTokens is { Count: > 0 })
            .Where(phrase => !string.IsNullOrWhiteSpace(phrase.Target))
            .OrderBy(phrase => phrase.SourceKey, StringComparer.Ordinal)
            .ToArray();

        // bestFromPosition[p] is the best walk from the point before source word p to the very end.
        var bestFromPosition = new TranslationPhrasePlan[tokenCount + 1];
        // Past the last source word there is nothing left to translate, so the walk ends empty.
        bestFromPosition[tokenCount] = emptyPlan;

        // Build the best walk for every point from the last source word backwards to the first one,
        // because the best walk from a point is one jump plus the already known best walk after it.
        for (var position = tokenCount - 1; position >= 0; position--)
        {
            // Start from the jump that always exists: leave this one source word untranslated.
            var bestHere = bestFromPosition[position + 1];

            // Then try every learned phrase that starts exactly at this point.
            foreach (var phrase in usablePhrases)
            {
                // A phrase that does not match the source words at this point cannot be used here.
                if (!MatchesAt(normalizedSourceTokens, position, phrase.SourceTokens))
                    continue;

                // Read the already computed best walk for the point right after this phrase.
                var tail = bestFromPosition[position + phrase.SourceTokens.Count];
                // Build the candidate walk: this phrase first, then that best remainder.
                var candidate = new TranslationPhrasePlan(
                    // This phrase covers all of its own source words, the remainder covers the rest.
                    phrase.SourceTokens.Count + tail.CoveredSourceTokens,
                    // This phrase is one more separate phrase on top of the remainder's phrases.
                    1 + tail.PhraseSpanCount,
                    // The addresses of the answer are this phrase's address followed by the rest.
                    Prepend(phrase.SourceKey, tail.ChosenSourceKeys),
                    // The translated pieces are this phrase's translation followed by the rest.
                    Prepend(phrase.Target, tail.Parts));

                // Keep the candidate only when it beats the best walk found for this point so far.
                if (IsBetter(candidate, bestHere))
                    bestHere = candidate;
            }

            // Record the winner for this point so earlier points can build on it.
            bestFromPosition[position] = bestHere;
        }

        // The best walk from the very first point is the answer for the whole sentence.
        return bestFromPosition[0];
    }

    // PlanAdmissible returns the best split among those that the caller's own admission rule
    // accepts, and honestly reports no plan when the rule accepts none of them.
    //
    // Why this method stands next to Plan instead of replacing it. Plan answers one question: which
    // split covers the most source words, and among those, which uses the fewest phrases. The
    // caller then applies its own admission rule to that single answer and may refuse it. Those are
    // not the same question. The admission rule has a second route that needs TWO OR MORE phrases
    // at half coverage, while Plan's second criterion actively drives the phrase count DOWN. So
    // Plan could hand over a wider split that the rule refuses, while a narrower split the rule
    // would have accepted existed and had already been thrown away — and then nobody answered.
    //
    // What this method does instead. It keeps one best walk per point AND per number of phrases
    // used, so every whole-plan candidate the admission rule could possibly accept survives the
    // search instead of being dropped inside it. The caller's rule is then applied to those
    // FINISHED candidates, and the best accepted one is returned by exactly the order Plan uses:
    // most covered words, then fewest phrases, then the fixed written-down order of addresses.
    //
    // Why the rule is applied to finished candidates only. Admission is a property of the whole
    // answer: it compares covered words and phrase count against the length of the whole sentence.
    // A half-built walk has no such length yet, so filtering during the walk would refuse plans
    // that become admissible once they are finished.
    //
    // What it deliberately does NOT do. It does not know the admission numbers and never compares
    // against one: the rule arrives as the predicate, and its single owner stays in the unit that
    // applies it. It does not loosen that rule, and it never looks at an expected translation.
    //
    // THE ASSUMPTION THE COMPLETENESS OF THIS SEARCH RESTS ON. "Completeness" here means that no
    // admissible plan is lost inside the search. That holds because only ONE walk is kept per
    // phrase count — and it rests on TWO separate things that must not be confused: an invariant of
    // THIS SEARCH, and three conditions on the CALLER'S predicate.
    //
    // THE INVARIANT OF THE SEARCH (not the caller's duty, and not a fourth condition on him):
    // inside the row of one phrase count, the walk that survives is the one with the MOST coverage.
    // That is produced by exactly ONE thing: coverage being the FIRST comparison key of IsBetter
    // below. The caller's monotonicity rescues completeness only in company with this invariant.
    // If the first key ever became something else — say "fewest phrases" — the survivor of a row
    // would no longer be its best-covering plan, a refusal on the survivor would no longer imply a
    // refusal on every worse-covering plan of that row, and a losable plan would be lost silently.
    // So CHANGING THE FIRST COMPARISON KEY REQUIRES REDOING THIS PROOF, exactly as changing the
    // rule to a non-monotone one does.
    //
    // THE CONDITIONS ON THE CALLER'S PREDICATE — all three must hold:
    //
    //   (1) the predicate reads only the coverage, the phrase count, and the number of answer
    //       pieces — and it may use that number only as a non-empty/empty flag, never as a value
    //       it compares against something. It must never look at the actual addresses or the
    //       translated pieces of a plan. (Reading the piece count as a value would need its own
    //       monotonicity argument, exactly like coverage.) That parenthesis is a fence for the
    //       future, not today's reason to be safe: TODAY the piece count equals the phrase count
    //       BY CONSTRUCTION — every phrase contributes exactly one piece, and the empty plan has
    //       zero of both — so it is constant inside the row of one phrase count, and even reading
    //       it as a value could not break completeness. Look for the danger where it actually is:
    //       in coverage, and in the inputs of condition (3).
    //   (2) at a FIXED phrase count the predicate is monotone in coverage: if the best-covering
    //       plan of that phrase count is refused, every plan of that same phrase count with less
    //       coverage is refused too.
    //   (3) the predicate receives THE SAME INPUTS in both places it is consulted — here as the
    //       constraint of the search, and in the caller as the admission of the finished answer.
    //       Today that holds because of the precondition of the admitted area: exactly ZERO
    //       accepted single-word translations, which makes a plan's covered words and its
    //       phrase-covered words the same number. IF THAT PRECONDITION CHANGES, the consistency
    //       and the completeness must be proven again: the caller's total coverage would exceed
    //       the phrase coverage a candidate here can know, and one predicate would then be
    //       computed on two different numbers in the two places.
    //
    // The admission rule in force satisfies all three: each of its two routes is a "coverage is not
    // below a threshold" test at a fixed phrase count, so dropping coverage can never turn a
    // refusal into an acceptance; it reads the piece count only as a non-empty flag; and the area's
    // precondition keeps its two coverage inputs equal.
    //
    // IF THE RULE CHANGES, THIS PROOF DOES NOT CARRY OVER. A predicate that is not monotone in
    // coverage (for example "coverage exactly one half", or an upper bound on coverage), or one
    // that looks at which addresses were chosen, can accept a plan this search already discarded
    // as worse-covering inside its phrase-count row — and the discarded plan is then lost. Such a
    // rule needs its own completeness argument, and this search needs to keep more than one walk
    // per row before it may be used with it. Do not widen the predicate without redoing that
    // proof: the loss would be silent, because the search would still return an admissible plan,
    // just not the best one — or none at all while one existed.
    public static TranslationPhrasePlan PlanAdmissible(
        IReadOnlyList<string> normalizedSourceTokens,
        IReadOnlyList<TranslationPhraseEdge> learnedPhrases,
        Func<TranslationPhrasePlan, bool> isAdmissible)
    {
        // An answer that covers nothing is both the starting point and the "no plan" answer.
        var emptyPlan = new TranslationPhrasePlan(0, 0, [], []);
        // With no source words there is nothing to cover, so there is nothing to admit either.
        if (normalizedSourceTokens is null || normalizedSourceTokens.Count == 0)
            return emptyPlan;
        // Without a rule to apply this method has no question to answer, so it claims no plan.
        if (isAdmissible is null)
            return emptyPlan;

        // Count the source words this call has to walk across.
        var tokenCount = normalizedSourceTokens.Count;

        // Keep only usable phrases and put them into the same fixed order by learned address that
        // the single-answer search uses, so neither method depends on enumeration order.
        var usablePhrases = (learnedPhrases ?? [])
            .Where(phrase => phrase is not null)
            .Where(phrase => phrase.SourceTokens is { Count: > 0 })
            .Where(phrase => !string.IsNullOrWhiteSpace(phrase.Target))
            .OrderBy(phrase => phrase.SourceKey, StringComparer.Ordinal)
            .ToArray();

        // best[p, s] is the best walk from the point before source word p to the very end that uses
        // exactly s phrases. A missing cell means that phrase count cannot be reached from there.
        // One row per phrase count is exactly what keeps an admissible alternative alive: the
        // single-answer search collapses them all into one winner and loses the rest.
        var best = new TranslationPhrasePlan?[tokenCount + 1, tokenCount + 1];
        // Past the last source word the only walk is the empty one, and it uses no phrase at all.
        best[tokenCount, 0] = emptyPlan;

        // Build every point from the last source word backwards, because the best walk from a point
        // is one jump plus an already known best walk after it.
        for (var position = tokenCount - 1; position >= 0; position--)
            // A phrase covers at least one word, so no walk can use more phrases than there are
            // words, and this row count is the full range the search has to keep.
            for (var spans = 0; spans <= tokenCount; spans++)
            {
                // Start from the jump that always exists: leave this one source word untranslated.
                // Skipping adds no phrase, so it is read from the same phrase-count row.
                var bestHere = best[position + 1, spans];

                // A walk that uses no phrase at all can only be that skip walk, so there is
                // nothing more to try in the zero row.
                if (spans > 0)
                    foreach (var phrase in usablePhrases)
                    {
                        // A phrase that does not match the source words here cannot be used here.
                        if (!MatchesAt(normalizedSourceTokens, position, phrase.SourceTokens))
                            continue;

                        // This phrase is one of the s phrases, so the remainder uses exactly s-1.
                        var tail = best[position + phrase.SourceTokens.Count, spans - 1];
                        // A remainder that cannot be walked with that many phrases offers nothing.
                        if (tail is null)
                            continue;

                        // Build the candidate walk: this phrase first, then that best remainder.
                        var candidate = new TranslationPhrasePlan(
                            // This phrase covers all of its own words, the remainder covers the rest.
                            phrase.SourceTokens.Count + tail.CoveredSourceTokens,
                            // The phrase count of this cell is fixed by the row it is built for.
                            spans,
                            // The addresses are this phrase's address followed by the remainder's.
                            Prepend(phrase.SourceKey, tail.ChosenSourceKeys),
                            // The translated pieces are this phrase's translation then the rest.
                            Prepend(phrase.Target, tail.Parts));

                        // Keep the candidate only when it beats the best walk of this cell so far.
                        // Inside one cell the phrase count is equal, so this compares coverage and
                        // then the written-down order of addresses.
                        if (bestHere is null || IsBetter(candidate, bestHere))
                            bestHere = candidate;
                    }

                // Record the winner of this cell so earlier points can build on it.
                best[position, spans] = bestHere;
            }

        // The finished whole-sentence candidates are the walks from the very first point: at most
        // one per phrase count, which is every answer the admission rule could accept.
        TranslationPhrasePlan? chosen = null;
        // Walk the candidates in order of phrase count; the order of this loop does not decide the
        // winner, because the comparison below does.
        for (var spans = 0; spans <= tokenCount; spans++)
        {
            // A phrase count that cannot be reached at all offers no candidate here.
            var candidate = best[0, spans];
            if (candidate is null)
                continue;

            // The caller's own rule decides whether this whole plan may be used at all. This is the
            // only place the rule is consulted, and its numbers live entirely on the caller's side.
            if (!isAdmissible(candidate))
                continue;

            // Among the accepted candidates the winner is picked by exactly the order the
            // single-answer search uses, so accepted answers keep the agreed preference.
            if (chosen is null || IsBetter(candidate, chosen))
                chosen = candidate;
        }

        // No accepted candidate means there honestly is no usable plan. The empty answer is what
        // the caller's own rule refuses by its "no phrases, no parts" condition, so the product
        // continues into the fallback path it already has instead of being handed a refused plan.
        return chosen ?? emptyPlan;
    }

    // IsBetter answers whether the candidate walk should replace the current best walk.
    private static bool IsBetter(TranslationPhrasePlan candidate, TranslationPhrasePlan incumbent)
    {
        // First rule: the walk that leaves fewer source words untranslated wins.
        //
        // READ THIS BEFORE CHANGING THE ORDER OF THESE RULES. Coverage being the FIRST key here is
        // not only a preference: it is the invariant the completeness of PlanAdmissible rests on.
        // That search keeps one walk per phrase count, and its proof needs the survivor of a row to
        // be the row's BEST-COVERING plan — which is true only while coverage is compared first. If
        // another key moved to first place, a refused survivor would no longer imply that every
        // worse-covering plan of its row is refused too, and an admissible plan could be dropped
        // inside the search without any sign of it. Changing this key therefore requires redoing
        // the completeness argument written above PlanAdmissible, not just rerunning the tests.
        if (candidate.CoveredSourceTokens != incumbent.CoveredSourceTokens)
            return candidate.CoveredSourceTokens > incumbent.CoveredSourceTokens;

        // Second rule: at equal coverage the walk built from fewer separate phrases wins, because a
        // longer learned phrase carries more of the learned word order than two short ones.
        if (candidate.PhraseSpanCount != incumbent.PhraseSpanCount)
            return candidate.PhraseSpanCount < incumbent.PhraseSpanCount;

        // Third rule: a written-down order over the learned addresses, so that two walks which are
        // equally good by the two rules above still always produce the same single answer.
        return CompareSourceKeys(candidate.ChosenSourceKeys, incumbent.ChosenSourceKeys) < 0;
    }

    // CompareSourceKeys orders two address sequences position by position, and puts the shorter
    // sequence first when one is the beginning of the other.
    private static int CompareSourceKeys(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        // Compare only as far as both sequences actually reach.
        var shared = Math.Min(left.Count, right.Count);
        // Walk the shared positions and return as soon as one address differs from the other.
        for (var index = 0; index < shared; index++)
        {
            // Compare this pair of addresses by their exact characters, never by culture rules.
            var comparison = string.CompareOrdinal(left[index], right[index]);
            // A difference at this position decides the whole comparison.
            if (comparison != 0)
                return comparison;
        }

        // Every shared position was equal, so the shorter sequence is the smaller one.
        return left.Count.CompareTo(right.Count);
    }

    // MatchesAt answers whether the phrase words sit in the source words starting exactly at start.
    private static bool MatchesAt(
        IReadOnlyList<string> sourceTokens,
        int start,
        IReadOnlyList<string> phraseTokens)
    {
        // A phrase that would run past the end of the sentence cannot match here.
        if (start + phraseTokens.Count > sourceTokens.Count)
            return false;

        // Compare the phrase word by word against the source words at the same offset.
        for (var offset = 0; offset < phraseTokens.Count; offset++)
            // Letter case is ignored, exactly as the earlier assembler compared these addresses.
            if (!string.Equals(
                    sourceTokens[start + offset],
                    phraseTokens[offset],
                    StringComparison.OrdinalIgnoreCase))
                // One disagreeing word is enough to reject this position.
                return false;

        // Every word of the phrase lined up with the source, so the phrase matches here.
        return true;
    }

    // Prepend returns a new sequence that starts with head and continues with the whole tail.
    private static IReadOnlyList<string> Prepend(string head, IReadOnlyList<string> tail)
    {
        // Allocate exactly the needed room: the new first element plus everything already there.
        var combined = new List<string>(tail.Count + 1) { head };
        // Copy the already chosen remainder after the new first element.
        combined.AddRange(tail);
        // Hand back the finished sequence.
        return combined;
    }
}
