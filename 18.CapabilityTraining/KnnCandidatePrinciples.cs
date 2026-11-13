// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using AgentInfrastructure;

// This record is the immutable passport of one opt-in candidate-ranking principle.
public sealed record KnnCandidatePrincipleDescriptor(
    // This field is the stable identity of the principle inside one catalogue.
    string Id,
    // This field is the schema version the product is allowed to execute.
    string Version,
    // This field names the single domain profile the principle may serve.
    string Profile,
    // This field names the only allowed job of the principle: reordering already learned candidates.
    string Scope,
    // This field is the finite weight of the principle inside the shared ranking budget.
    double Weight);

// This record is everything a principle may read about the incoming request.
public sealed record KnnPrincipleQuery(
    // This field is the domain profile the serving unit was trained for.
    string Profile,
    // This field is the caller text exactly as the unit received it.
    string Text);

// This record is everything a principle may read about one already learned candidate.
public sealed record KnnPrincipleCandidate(
    // This field is the identity of the stored training example.
    string ExampleId,
    // This field is the stored input text of that example; the stored answer is deliberately absent.
    string Text);

// This record is the verdict of one principle about one candidate.
public sealed record KnnPrincipleScore(
    // This field says whether the principle could judge at all; a missing judgement is not a passed check.
    bool Applicable,
    // This field is the judgement in the closed range minus one to plus one.
    double Value,
    // This field is the human readable reason behind the judgement.
    string Reason);

// This interface is the only seam through which an external ranking rule enters the native KNN unit.
// The contract below is written out in full, because the name of an interface cannot by itself promise
// that an arbitrary external callback is pure; the product enforces only what is listed as enforced.
//
// Required of the implementer, and NOT enforced by the catalogue or by the ranker:
//   - Descriptor must return the same five passport values on every read; the catalogue copies them once
//     at admission and never reads the live property again, so a later rewrite is simply ignored.
//   - Evaluate must be a pure read: it may not change the query, the candidate, the stored memory,
//     any shared state, or its own fields, and it must return the same verdict for the same input.
//   - Evaluate may not write an answer; it only argues for or against a candidate the unit already learned.
//
// Enforced by the ranker, so an implementer cannot break the product with it:
//   - A refusal (Applicable false), a null verdict, or a non-finite verdict value contributes exactly zero
//     and the weights of the other principles are NOT renormalised to fill the gap.
//   - A verdict value outside minus one to plus one is clamped into that range before it is weighted.
//   - A thrown exception is an explicit refusal of the whole ranking, reported through the shared envelope
//     as PRINCIPLE_EVALUATION_FAILED; it is never swallowed and never escapes as an exception.
//   - The passport weight used for the sum is the copy captured at admission, never the live property.
public interface IKnnCandidatePrinciple
{
    // This property exposes the immutable passport used for admission and ordering.
    KnnCandidatePrincipleDescriptor Descriptor { get; }

    // This method judges one learned candidate against one request and never writes an answer.
    KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate);
}

// This record is the canonical admission entry: one callback frozen together with the passport it was admitted on.
public sealed record KnnAdmittedPrinciple
{
    // Only the admission gate may build an entry, so every entry in a catalogue is already validated.
    internal KnnAdmittedPrinciple(IKnnCandidatePrinciple principle, KnnCandidatePrincipleDescriptor descriptor)
    {
        // Keep the exact callback object that passed admission, so no later swap can reach the ranker.
        Principle = principle;
        // Keep the copy of the passport taken at admission, so a later rewrite of the live passport is ignored.
        Descriptor = descriptor;
    }

    // This property is the admitted callback itself, exposed for review but impossible to replace.
    // ВНИМАНИЕ ПОТРЕБИТЕЛЮ: через это свойство видно ЖИВОЙ паспорт правила (`Principle.Descriptor`),
    // который внешний автор правила может переписать уже после допуска или уронить исключением.
    // Для любого решения — порядок, вес, профиль, версия, область, аудит — берите СОСЕДНЕЕ свойство
    // `Descriptor` этой же записи: это замороженная на допуске копия, ровно её использует ранжировщик.
    // Обращение вида `catalog.Principles[0].Principle.Descriptor` воспроизводит уже исправленную ошибку
    // «живой переписываемый паспорт как основание решения» и в продуктовом коде запрещено.
    public IKnnCandidatePrinciple Principle { get; }

    // This property is the frozen passport the ranker actually uses for ordering, weighting and auditing.
    public KnnCandidatePrincipleDescriptor Descriptor { get; }
}

// This helper holds the shared text reading used by the transferred principles.
public static class KnnPrincipleText
{
    // Split text into lower-case words so that punctuation and markup never become evidence.
    public static IReadOnlyList<string> Words(string? text)
    {
        // Normalise first so that pre-composed and decomposed letters compare equal.
        var normalized = (text ?? string.Empty).Normalize(NormalizationForm.FormC);
        // Collect finished words here.
        var words = new List<string>();
        // Accumulate the letters of the word currently being read.
        var current = new StringBuilder();
        // Walk the text one symbol at a time so the word rules stay identical everywhere.
        foreach (var symbol in normalized)
        {
            // A letter or a combining mark continues the current word.
            if (char.IsLetter(symbol) || CharUnicodeInfo.GetUnicodeCategory(symbol) == UnicodeCategory.NonSpacingMark)
            {
                // Keep the symbol inside the word being built.
                current.Append(symbol);
                // Nothing else has to happen for a letter, so move straight to the next symbol.
                continue;
            }

            // Anything else closes the current word.
            if (current.Length > 0)
            {
                // Store the finished word in a case-insensitive form.
                words.Add(current.ToString().ToLowerInvariant());
                // Start the next word from an empty buffer.
                current.Clear();
            }
        }

        // Flush the last word when the text does not end with a separator.
        if (current.Length > 0)
            words.Add(current.ToString().ToLowerInvariant());

        // Return the ordered word list to the caller.
        return words;
    }

    // Return only the sentence that holds the cursor: everything after the last full stop, question or exclamation mark.
    public static string CursorSentence(string? prefix)
    {
        // Treat a missing prefix as empty text rather than failing.
        var text = prefix ?? string.Empty;
        // Remember the position of the last sentence terminator.
        var cut = -1;
        // Scan the whole text from the left so the last terminator is the one that survives.
        for (var index = 0; index < text.Length; index++)
        {
            // Record every terminator so the final one wins.
            if (text[index] is '.' or '?' or '!')
                cut = index;
        }

        // Without a terminator the whole text is the cursor sentence.
        return cut < 0 ? text : text[(cut + 1)..];
    }

    // Return the part of the text that follows the last occurrence of a declared layout marker.
    public static string SegmentAfterMarker(string? text, string? marker)
    {
        // Treat a missing text as empty so the caller never sees a null.
        var source = text ?? string.Empty;
        // Without a declared marker the whole text is the segment.
        if (string.IsNullOrEmpty(marker))
            return source;

        // Find the last marker so a repeated label still points at the final segment.
        var position = source.LastIndexOf(marker, StringComparison.Ordinal);
        // A missing marker means the layout is not present, so the whole text is used.
        if (position < 0)
            return source;

        // Cut everything up to and including the marker and drop the leading spaces.
        return source[(position + marker.Length)..].TrimStart();
    }
}

// This record is one candidate as the native fusion of the unit produced it.
public sealed record KnnNativeCandidate(
    // This field is the identity of the stored example.
    string ExampleId,
    // This field is the stored input text used by the principles.
    string Text,
    // This field is the fused native score of project 18 before any principle runs.
    double NativeScore);

// This record is one candidate after the opt-in principles adjusted its position.
public sealed record KnnRankedCandidate(
    // This field is the identity of the stored example.
    string ExampleId,
    // This field is the unchanged native score, kept so the adjustment stays auditable.
    double NativeScore,
    // This field is the score actually used for selection.
    double FinalScore);

// This catalogue is the admitted, immutable set of principles for exactly one profile.
public sealed class KnnCandidatePrincipleCatalog
{
    // This constant is the only principle schema version the product executes.
    public const string SupportedVersion = "v1";

    // This constant is the only job a principle is allowed to do.
    public const string SupportedScope = "candidate-ranking";

    // This constant is the total share of the score that all principles together may move.
    public const double PrincipleBudget = 0.20;

    // Keep the admitted entries in their canonical order so float summation is reproducible.
    // The array itself stays private and is never handed out, so no caller can replace an admitted entry.
    private readonly KnnAdmittedPrinciple[] _admitted;

    // Keep one pre-built protected view, so reading the catalogue never exposes the private array.
    private readonly ReadOnlyCollection<KnnAdmittedPrinciple> _exposed;

    // This private constructor is reachable only through Create, so its input is already admitted.
    private KnnCandidatePrincipleCatalog(string profile, KnnAdmittedPrinciple[] admitted)
    {
        // Remember the profile this catalogue is bound to.
        Profile = profile;
        // Store the already validated and already ordered admission entries.
        _admitted = admitted;
        // Wrap them once in a collection that refuses every write, including a write reached through a cast.
        _exposed = new ReadOnlyCollection<KnnAdmittedPrinciple>(_admitted);
    }

    // This property is the single profile the catalogue serves.
    public string Profile { get; }

    // This property is the number of admitted principles, zero meaning the default native behaviour.
    public int Count => _admitted.Length;

    // This property exposes the frozen admission entries in canonical order for review, ranking and tests.
    // It deliberately returns the protected view and never the backing array, because casting the old
    // IReadOnlyList back to its array let a caller replace an admitted callback after the gate had passed.
    public IReadOnlyList<KnnAdmittedPrinciple> Principles => _exposed;

    // Build an empty catalogue, which keeps the unit on its unchanged native path.
    public static OperationResult<KnnCandidatePrincipleCatalog> CreateEmpty(string profile)
        // Reuse the full admission path so an empty catalogue still validates the profile name.
        => Create(profile, []);

    // Admit a set of principles for one profile, rejecting every identity problem before any answer.
    public static OperationResult<KnnCandidatePrincipleCatalog> Create(
        string profile,
        IReadOnlyList<IKnnCandidatePrinciple> principles)
    {
        // A catalogue without a profile cannot be matched against a trained unit.
        if (string.IsNullOrWhiteSpace(profile))
            return OperationResult<KnnCandidatePrincipleCatalog>.Fail("Principle profile is required.", "PRINCIPLE_PROFILE_REQUIRED");
        // A missing list is a caller mistake, while an empty list is the valid default.
        if (principles is null)
            return OperationResult<KnnCandidatePrincipleCatalog>.Fail("Principle list is required.", "PRINCIPLES_REQUIRED");

        // Track identities so a duplicate is found before the catalogue exists.
        var seen = new HashSet<string>(StringComparer.Ordinal);
        // Collect the frozen admission entries here, so the live passports are never read again after this gate.
        var admitted = new List<KnnAdmittedPrinciple>(principles.Count);
        // Check every offered principle, because one bad passport invalidates the whole catalogue.
        foreach (var principle in principles)
        {
            // A null entry would crash the ranker later, so it is refused now.
            if (principle is null)
                return OperationResult<KnnCandidatePrincipleCatalog>.Fail("Principle entry cannot be null.", "INVALID_PRINCIPLE");

            // Здесь держим прочитанный паспорт; тип допускает null, потому что внешнее правило вправе вернуть его.
            KnnCandidatePrincipleDescriptor? descriptor;
            // Чтение свойства у чужого правила — это чужой код, и он может выбросить исключение.
            try
            {
                // Единственное чтение живого паспорта: ниже работает только проверенная и замороженная копия.
                descriptor = principle.Descriptor;
            }
            catch (Exception error)
            {
                // Правило 1 проекта: публичный метод возвращает конверт отказа, а не бросает наружу.
                return OperationResult<KnnCandidatePrincipleCatalog>.Fail(
                    // Сообщение называет причину человеку, не раскрывая внутренности допуска.
                    $"Principle descriptor could not be read: {error.Message}",
                    // Машинный код отличает упавшее чтение паспорта от просто неверного паспорта.
                    "PRINCIPLE_DESCRIPTOR_READ_FAILED");
            }

            // A principle without a passport cannot be admitted.
            if (descriptor is null || string.IsNullOrWhiteSpace(descriptor.Id))
                return OperationResult<KnnCandidatePrincipleCatalog>.Fail("Principle descriptor id is required.", "INVALID_PRINCIPLE_ID");
            // Two principles with the same identity would double-count the same evidence.
            if (!seen.Add(descriptor.Id))
                return OperationResult<KnnCandidatePrincipleCatalog>.Fail($"Duplicate principle id: {descriptor.Id}.", "DUPLICATE_PRINCIPLE_ID");
            // Only the measured schema version is executable in the product.
            if (!string.Equals(descriptor.Version, SupportedVersion, StringComparison.Ordinal))
                return OperationResult<KnnCandidatePrincipleCatalog>.Fail($"Unknown principle version: {descriptor.Id}.", "UNKNOWN_PRINCIPLE_VERSION");
            // Only candidate reordering is allowed; no other job may hide behind this seam.
            if (!string.Equals(descriptor.Scope, SupportedScope, StringComparison.Ordinal))
                return OperationResult<KnnCandidatePrincipleCatalog>.Fail($"Unknown principle scope: {descriptor.Id}.", "UNKNOWN_PRINCIPLE_SCOPE");
            // A principle of a foreign profile must never read the memory of this profile.
            if (!string.Equals(descriptor.Profile, profile, StringComparison.Ordinal))
                return OperationResult<KnnCandidatePrincipleCatalog>.Fail($"Foreign principle profile: {descriptor.Id}.", "FOREIGN_PRINCIPLE_PROFILE");
            // A non-finite or out-of-range weight would break the proven budget bound of plus or minus two tenths.
            if (!double.IsFinite(descriptor.Weight) || descriptor.Weight < 0.0 || descriptor.Weight > 1.0)
                return OperationResult<KnnCandidatePrincipleCatalog>.Fail($"Invalid principle weight: {descriptor.Id}.", "INVALID_PRINCIPLE_WEIGHT");

            // Copy the five significant passport fields that were just checked, so the admitted values are frozen here.
            var admittedDescriptor = new KnnCandidatePrincipleDescriptor(
                // Freeze the identity that ordering, de-duplication and auditing rely on.
                descriptor.Id,
                // Freeze the schema version that admission accepted.
                descriptor.Version,
                // Freeze the profile, so a later rewrite cannot point an admitted rule at a foreign memory.
                descriptor.Profile,
                // Freeze the job, so a later rewrite cannot widen the rule beyond candidate reordering.
                descriptor.Scope,
                // Freeze the weight, so a later rewrite cannot push an infinite or oversized share into the sum.
                descriptor.Weight);
            // Store the callback together with the passport it was admitted on; nothing else is trusted later.
            admitted.Add(new KnnAdmittedPrinciple(principle, admittedDescriptor));
        }

        // Order by the frozen identity so the same set always sums its contributions in the same float order.
        var ordered = admitted
            .OrderBy(entry => entry.Descriptor.Id, StringComparer.Ordinal)
            .ToArray();

        // Hand back the admitted immutable catalogue.
        return OperationResult<KnnCandidatePrincipleCatalog>.Ok(new KnnCandidatePrincipleCatalog(profile, ordered));
    }
}

// This ranker moves already learned candidates inside a fixed budget and never creates an answer.
public static class KnnCandidatePrincipleRanker
{
    // Reorder the native candidates with the admitted principles of the catalogue.
    public static OperationResult<IReadOnlyList<KnnRankedCandidate>> Rank(
        KnnCandidatePrincipleCatalog catalog,
        KnnPrincipleQuery query,
        IReadOnlyList<KnnNativeCandidate> candidates)
    {
        // A missing catalogue means the caller never passed the admission gate.
        if (catalog is null)
            return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Fail("Principle catalogue is required.", "PRINCIPLE_CATALOG_REQUIRED");
        // A missing query cannot be judged by any principle.
        if (query is null)
            return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Fail("Principle query is required.", "PRINCIPLE_QUERY_REQUIRED");
        // Without candidates there is nothing learned to reorder.
        if (candidates is null || candidates.Count == 0)
            return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Fail("At least one native candidate is required.", "NO_NEAREST_EXAMPLE");
        // The catalogue must belong to the same profile as the request being served.
        if (!string.Equals(catalog.Profile, query.Profile, StringComparison.Ordinal))
            return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Fail("Principle catalogue profile does not match the query profile.", "FOREIGN_PRINCIPLE_PROFILE");

        // Each principle receives an equal share of the shared budget, exactly as measured.
        var share = catalog.Count == 0 ? 0.0 : 1.0 / catalog.Count;
        // Collect the reordered candidates here.
        var ranked = new List<KnnRankedCandidate>(candidates.Count);
        // Judge every native candidate, because the order is only meaningful over the full set.
        foreach (var candidate in candidates)
        {
            // A candidate without an identity cannot be selected or audited.
            if (candidate is null || string.IsNullOrWhiteSpace(candidate.ExampleId))
                return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Fail("Native candidate id is required.", "INVALID_NATIVE_CANDIDATE");
            // A non-finite native score would make the final order meaningless.
            if (!double.IsFinite(candidate.NativeScore))
                return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Fail($"Native score is not finite: {candidate.ExampleId}.", "INVALID_NATIVE_SCORE");

            // Accumulate the weighted judgements of all admitted principles for this candidate.
            var added = 0.0;
            // Show every principle the same read-only view of the stored example.
            var view = new KnnPrincipleCandidate(candidate.ExampleId, candidate.Text);
            // Ask the admitted entries in their canonical order so the float sum is reproducible.
            foreach (var entry in catalog.Principles)
            {
                // Hold the verdict of this one principle about this one candidate.
                KnnPrincipleScore? score;
                // An external callback may still throw, so the failure is turned into an explicit refusal here.
                try
                {
                    // Ask the admitted callback itself, which is the exact object that passed the gate.
                    score = entry.Principle.Evaluate(query, view);
                }
                catch (Exception error)
                {
                    // A crashing rule refuses the whole ranking instead of silently contributing zero.
                    return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Fail(
                        $"Principle evaluation failed: {entry.Descriptor.Id}: {error.Message}",
                        "PRINCIPLE_EVALUATION_FAILED");
                }

                // A principle that refuses to judge contributes exactly zero and is not renormalised away.
                var usable = score is not null && score.Applicable && double.IsFinite(score.Value);
                // Clamp the judgement into the agreed range so one rule cannot dominate the budget.
                var value = usable ? Math.Clamp(score!.Value, -1.0, 1.0) : 0.0;
                // Weight the judgement with the passport weight frozen at admission, never with the live property.
                added += share * entry.Descriptor.Weight * value;
            }

            // An empty catalogue leaves the native score untouched and does not re-round it.
            var final = catalog.Count == 0
                ? candidate.NativeScore
                : candidate.NativeScore + (KnnCandidatePrincipleCatalog.PrincipleBudget * added);
            // Refuse a non-finite result outright, so no refusal of a rule can ever reach the confidence number.
            // Every input is already bounded, so this is the last guard rather than the first one.
            if (!double.IsFinite(final))
                return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Fail(
                    $"Final score is not finite: {candidate.ExampleId}.", "INVALID_FINAL_SCORE");
            // Keep both numbers so a later review can separate native evidence from the adjustment.
            ranked.Add(new KnnRankedCandidate(candidate.ExampleId, candidate.NativeScore, final));
        }

        // Highest final score wins, and an exact tie keeps the stable identity order of project 18.
        var ordered = ranked
            .OrderByDescending(candidate => candidate.FinalScore)
            .ThenBy(candidate => candidate.ExampleId, StringComparer.Ordinal)
            .ToArray();

        // Return the reordered list; the answer itself still comes from the stored example.
        return OperationResult<IReadOnlyList<KnnRankedCandidate>>.Ok(ordered);
    }
}

// This principle ranks candidates by the explicit time frame of the sentence holding the cursor.
public sealed class KnnTimeFrameCandidatePrinciple : IKnnCandidatePrinciple
{
    // Remember the layout marker that separates the instruction from the prefix inside the stored text.
    private readonly string _prefixMarker;

    // Words that mark a repeated action.
    private readonly string[] _habitual;

    // Words that mark a finished action.
    private readonly string[] _past;

    // Words that mark a planned action.
    private readonly string[] _future;

    // Words that cancel a time marker instead of confirming it.
    private readonly string[] _negations;

    // This constructor binds the rule to one profile and to the word tables of that one language.
    public KnnTimeFrameCandidatePrinciple(
        string profile,
        string prefixMarker,
        string[] habitual,
        string[] past,
        string[] future,
        string[] negations)
    {
        // Build the passport from the profile so the catalogue can refuse a foreign profile.
        Descriptor = new KnnCandidatePrincipleDescriptor(
            $"{profile.ToLowerInvariant()}.time-frame",
            KnnCandidatePrincipleCatalog.SupportedVersion,
            profile,
            KnnCandidatePrincipleCatalog.SupportedScope,
            1.0);
        // Store the declared layout marker; a string is already immutable, so nothing has to be copied.
        _prefixMarker = prefixMarker ?? string.Empty;
        // Copy the four word tables of this language, because the caller keeps its own arrays and may rewrite
        // a word in them after this rule has already been admitted; a copy keeps the admitted model unchanged.
        _habitual = Copy(habitual);
        // Copy the markers of a finished action for the same reason.
        _past = Copy(past);
        // Copy the markers of a planned action for the same reason.
        _future = Copy(future);
        // Copy the words that cancel a marker instead of confirming it, for the same reason.
        _negations = Copy(negations);
    }

    // Take a private snapshot of one caller-owned word table, treating a missing table as an empty one.
    private static string[] Copy(string[]? markers)
        // The collection expression allocates a new array, so a later write by the caller cannot reach this rule.
        => markers is null ? [] : [.. markers];

    // This property is the immutable passport of the principle.
    public KnnCandidatePrincipleDescriptor Descriptor { get; }

    // Judge one candidate by comparing its time frame with the time frame of the request.
    public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
    {
        // Read the time frame of the request from its own prefix only.
        var queryFrame = Frame(KnnPrincipleText.SegmentAfterMarker(query?.Text, _prefixMarker));
        // Read the time frame of the stored example from its own prefix only.
        var candidateFrame = Frame(KnnPrincipleText.SegmentAfterMarker(candidate?.Text, _prefixMarker));
        // When either side has no decidable time frame the principle stays out of the decision.
        if (queryFrame is null || candidateFrame is null)
            return new KnnPrincipleScore(false, 0.0, $"time frame undecided: query {queryFrame ?? "none"}, candidate {candidateFrame ?? "none"}");

        // Equal frames support the candidate, different frames argue against it.
        var same = string.Equals(queryFrame, candidateFrame, StringComparison.Ordinal);
        // Report the judgement together with both frames so a reviewer can retrace the decision.
        return new KnnPrincipleScore(true, same ? 1.0 : -1.0, $"query {queryFrame}, candidate {candidateFrame}");
    }

    // Decide the time frame of one prefix, refusing to guess when the evidence conflicts.
    private string? Frame(string prefix)
    {
        // Only the sentence that holds the cursor may vote.
        var sentence = KnnPrincipleText.CursorSentence(prefix);
        // Read that sentence as words.
        var words = KnnPrincipleText.Words(sentence);
        // A negation word cancels the time evidence of this sentence.
        var negated = words.Any(word => _negations.Contains(word, StringComparer.Ordinal));

        // Collect every frame that the sentence supports.
        var found = new HashSet<string>(StringComparer.Ordinal);
        // A repeated-action marker votes for the habitual frame.
        if (ContainsAny(words, _habitual)) found.Add("habitual");
        // A finished-action marker votes for the past frame.
        if (ContainsAny(words, _past)) found.Add("past");
        // A planned-action marker votes for the future frame.
        if (ContainsAny(words, _future)) found.Add("future");

        // A negated marker is not a frame, so the principle refuses to judge.
        if (negated && found.Count > 0)
            return null;
        // Conflicting markers inside one sentence are also a refusal, not a first-match choice.
        if (found.Count > 1)
            return null;
        // Exactly one marker gives the decided frame.
        if (found.Count == 1)
            return found.First();

        // No marker at all means there is nothing to judge.
        return null;
    }

    // Report whether any multi-word marker appears as a contiguous run of words.
    private static bool ContainsAny(IReadOnlyList<string> words, IReadOnlyList<string> markers)
    {
        // Try every declared marker; the first one that matches ends the search.
        foreach (var marker in markers)
        {
            // Read the marker with the same word rules as the text.
            var markerWords = KnnPrincipleText.Words(marker);
            // An empty marker can never match.
            if (markerWords.Count == 0)
                continue;
            // Slide the marker over every starting position that still leaves room for all its words.
            for (var start = 0; start + markerWords.Count <= words.Count; start++)
            {
                // Assume a hit and disprove it word by word.
                var hit = true;
                // Compare the marker word by word against the text starting at this position.
                for (var offset = 0; offset < markerWords.Count; offset++)
                {
                    // A single mismatch ends this starting position.
                    if (!string.Equals(words[start + offset], markerWords[offset], StringComparison.Ordinal))
                    {
                        // Record that this starting position failed.
                        hit = false;
                        // Stop comparing the remaining marker words, they can no longer help.
                        break;
                    }
                }

                // A complete run of marker words is a match.
                if (hit)
                    return true;
            }
        }

        // No marker matched anywhere in the sentence.
        return false;
    }
}

// This principle ranks orchestration candidates by the order of the stages already completed.
public sealed class KnnOrchestrationPredecessorStageCandidatePrinciple : IKnnCandidatePrinciple
{
    // This constructor binds the stage-order rule to the one orchestration profile it may serve.
    public KnnOrchestrationPredecessorStageCandidatePrinciple(string profile)
        // Build the passport for the orchestration profile that owns this rule.
        => Descriptor = new KnnCandidatePrincipleDescriptor(
            "or.predecessor-stage",
            KnnCandidatePrincipleCatalog.SupportedVersion,
            profile,
            KnnCandidatePrincipleCatalog.SupportedScope,
            1.0);

    // This property is the immutable passport of the principle.
    public KnnCandidatePrincipleDescriptor Descriptor { get; }

    // Judge one candidate by the stages visible in the request and in the stored example.
    public KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate)
    {
        // Read the stages the request says are already done.
        var queryCompleted = KnnOrchestrationFields.Completed(query?.Text);
        // Read the stages the stored example says were already done when it was learned.
        var candidateCompleted = KnnOrchestrationFields.Completed(candidate?.Text);

        // Derive the precondition of the candidate from its own visible contract, never from the expected answer.
        var prerequisiteMet = PrerequisiteSatisfied(candidateCompleted, queryCompleted);
        // A candidate whose precondition is not met in the request is argued against.
        if (prerequisiteMet is false)
            return new KnnPrincipleScore(true, -1.0, "candidate prerequisite is not completed in the request");

        // Both sides standing at the start of the plan agree on the order.
        if (queryCompleted.Count == 0 && candidateCompleted.Count == 0)
            return new KnnPrincipleScore(true, 1.0, "both sides are at the start of the plan");
        // Only one side at the start of the plan is a disagreement.
        if (queryCompleted.Count == 0 || candidateCompleted.Count == 0)
            return new KnnPrincipleScore(true, -1.0, "one side is at the start of the plan and the other already advanced");

        // Compare only the last two completed stages of each side.
        var left = queryCompleted.Skip(Math.Max(0, queryCompleted.Count - 2)).ToArray();
        // Take the same two-stage tail from the stored example so both sides are compared alike.
        var right = candidateCompleted.Skip(Math.Max(0, candidateCompleted.Count - 2)).ToArray();
        // An identical tail of completed stages supports the candidate.
        var same = left.SequenceEqual(right, StringComparer.Ordinal);
        // Report the judgement together with both stage tails so a reviewer can retrace the decision.
        return new KnnPrincipleScore(true, same ? 1.0 : -1.0,
            $"request stages [{string.Join(",", left)}] against [{string.Join(",", right)}]");
    }

    // Report whether the candidate precondition holds, or null when the data does not allow a judgement.
    private static bool? PrerequisiteSatisfied(IReadOnlyList<string> candidateCompleted, IReadOnlyList<string> queryCompleted)
    {
        // A candidate that completed nothing carries no precondition to check.
        if (candidateCompleted.Count == 0)
            return null;

        // A candidate that is a later step requires the request to have completed something too.
        return queryCompleted.Count > 0;
    }
}

// This helper reads the labelled fields of an orchestration row from its visible text only.
internal static class KnnOrchestrationFields
{
    // Return the value of one labelled field, or an empty string when the label is absent.
    public static string Field(string? text, string name)
    {
        // Read the row as semicolon separated parts, which is the declared layout of these rows.
        foreach (var part in (text ?? string.Empty).Split(';', StringSplitOptions.TrimEntries))
        {
            // Build the exact label prefix being looked for.
            var marker = name + "=";
            // The first part that starts with the label carries the value.
            if (part.StartsWith(marker, StringComparison.Ordinal))
                return part[marker.Length..].Trim();
        }

        // A missing label is an empty value, never a guess.
        return string.Empty;
    }

    // Return the ordered list of stages the row declares as completed.
    public static IReadOnlyList<string> Completed(string? text)
    {
        // Read the labelled field first.
        var raw = Field(text, "completed");
        // An absent or explicitly empty field means no stage is completed.
        if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, "пусто", StringComparison.Ordinal))
            return [];

        // Read the remaining value as ordered words.
        return KnnPrincipleText.Words(raw);
    }
}

// This factory exposes only the principle profiles whose gain was measured above five per cent.
public static class KnnDomainCandidatePrincipleProfiles
{
    // This constant is the stored-text label after which the Russian prefix begins.
    public const string RussianPrefixMarker = "Префикс:";

    // Build the Russian time-frame principle that moved the measured panel from five of eight to eight of eight.
    public static IKnnCandidatePrinciple RussianTimeFrame(string profile = "RU")
        // Use exactly the word tables of the measured experiment; other languages need their own tables.
        => new KnnTimeFrameCandidatePrinciple(
            profile,
            RussianPrefixMarker,
            ["каждый день", "ежедневно"],
            ["вчера", "прошлой ночью"],
            ["завтра"],
            ["не"]);

    // Build the orchestration stage-order principle that moved the measured panel from five of eight to eight of eight.
    public static IKnnCandidatePrinciple OrchestrationPredecessorStage(string profile = "OR")
        // The orchestration rule reads the labelled fields of the row and needs no word tables.
        => new KnnOrchestrationPredecessorStageCandidatePrinciple(profile);
}
