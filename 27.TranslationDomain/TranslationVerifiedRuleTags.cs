// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using AgentInfrastructure;

// TranslationVerifiedRuleTags normalises the grammar annotation that a verified training row
// carries for one pair, and refuses an annotation that cannot state one real grammar fact.
//
// Why this type exists. The annotation of the CORRECT side of a lesson used to be read from the
// "how it is wrong" side of the same pair. For a pair about tense that is the opposite of the
// truth: a sentence in the past tense was saved with the present-tense mark and a sentence in the
// present tense with the past-tense mark, so the stored lesson stated the opposite of what the
// correct answer actually is. The verified annotation now arrives beside the identifier and the
// validation fields of the row. It is never derived from the learner's answer and never from the
// rejected variant.
//
// What this type does not do. It does not read the meaning of a sentence, does not guess a tense
// the verified row did not state, does not translate, and does not teach the general parser any
// new tense rule. For text that was never trained the old parser imprecision stays visible.
public static class TranslationVerifiedRuleTags
{
    // The one accepted spelling of the past-tense fact, the form the rest of the code reads.
    private const string TensePastBody = "TENSE=PAST";

    // The one accepted spelling of the present-tense fact.
    private const string TensePresentBody = "TENSE=PRESENT";

    // The name of the tense family, used to recognise an entry that states a tense at all.
    private const string TenseFamily = "TENSE";

    // Значение прошедшего времени без имени семейства. Нужно отдельно, потому что пометки
    // сырого разбора пишут время так же: "RU:TENSE:PAST".
    private const string TensePastValue = "PAST";

    // Значение настоящего времени в том же виде.
    private const string TensePresentValue = "PRESENT";

    // The three separators a hand-written verified row may use between family name and value.
    private static readonly char[] FamilySeparators = ['_', ':', '='];

    // Canonicalize turns the verified tags of one pair into the canonical tag bodies that the
    // batch builder appends to the signature of the correct lesson.
    //
    // The rule in plain words: every entry is trimmed; an entry that is empty after trimming is a
    // refusal; an entry that states the tense family must state exactly PAST or PRESENT and is
    // rewritten to the one accepted spelling; any other tense value is a refusal; stating both
    // PAST and PRESENT for the same pair is a refusal; an entry of another family passes through
    // unchanged for the character normalisation the builder already owns; repeated identical
    // bodies collapse to one; the order of first appearance is kept.
    //
    // An absent or empty list is an allowed, honest state: the row carries no verified annotation
    // and its lesson then receives no rule tag at all. Absence is never a permission to read the
    // rejected side again.
    public static OperationResult<IReadOnlyList<string>> Canonicalize(IReadOnlyList<string>? verifiedRuleTags)
    {
        // A row without the field and a row with an empty list both mean "no verified annotation".
        if (verifiedRuleTags is null || verifiedRuleTags.Count == 0)
            return OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>());

        // Accepted bodies in the order they first appear, so the stored tag order is predictable.
        var bodies = new List<string>(verifiedRuleTags.Count);

        // The same bodies as a set, used both to drop repetitions and to detect a tense conflict.
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // Walk every entry separately, so a refusal can name the exact entry that caused it.
        foreach (var entry in verifiedRuleTags)
        {
            // Remove the surrounding spaces a hand-written manifest may carry.
            var trimmed = (entry ?? string.Empty).Trim();

            // An entry that carries no text at all cannot state a verified fact.
            if (trimmed.Length == 0)
                return OperationResult<IReadOnlyList<string>>.Fail(
                    "A verified rule tag is empty.",
                    "VERIFIED_RULE_TAG_EMPTY");

            // Decide whether this entry states a tense, and which accepted tense it means.
            var tenseBody = TryCanonicalizeTense(trimmed, out var statesTense);

            // A tense entry whose value this code cannot read is refused rather than guessed.
            if (statesTense && tenseBody is null)
                return OperationResult<IReadOnlyList<string>>.Fail(
                    $"Verified rule tag '{trimmed}' states an unsupported tense value.",
                    "VERIFIED_RULE_TAG_UNSUPPORTED_TENSE");

            // A tense entry becomes its one accepted spelling; another family keeps its own text.
            var body = tenseBody ?? trimmed;

            // The same fact written twice teaches nothing twice, so keep only its first copy.
            if (seen.Add(body))
                bodies.Add(body);
        }

        // One pair cannot be in the past tense and in the present tense at the same time.
        if (seen.Contains(TensePastBody) && seen.Contains(TensePresentBody))
            return OperationResult<IReadOnlyList<string>>.Fail(
                "A verified rule annotation states both the past and the present tense.",
                "VERIFIED_RULE_TAG_TENSE_CONFLICT");

        // Hand back the accepted bodies; the builder owns the character normalisation after this.
        return OperationResult<IReadOnlyList<string>>.Ok(bodies);
    }

    // ReconcileTrainedTags собирает набор учебных пометок, с которым ХРАНИТСЯ И ЧИТАЕТСЯ поздняя
    // поправка разбора.
    //
    // Что было неправильно. Поздняя поправка меняла у записи образец предложения и подпись, а
    // пометки оставляла от прежнего, ошибочного вида. То есть исправленный ответ носил пометки
    // своего прошлого: для предложения в настоящем времени там стояло прошедшее, а рядом в полном
    // ключе — проверенное настоящее. Ключ противоречил сам себе, и дальше по этим пометкам
    // собирались адреса поиска и писался слепок состояния.
    //
    // Что делает этот вычислитель. Он берёт пометки сырого разбора и проверенное время учебной
    // строки и возвращает ОДИН согласованный набор: время становится проверенным, а пометки,
    // которые имеют смысл только при прошедшем времени — пол глагола прошедшей формы и
    // согласование подлежащего с ней, — при другом времени убираются, потому что иначе они
    // противоречили бы собственному времени записи. Порядок первого появления сохраняется.
    //
    // Чего он НЕ делает. Он не читает смысл предложения, не исправляет базовую догадку сырого
    // разборщика о частях речи и времени (это отдельная работа) и не придумывает пометку
    // языкового семейства там, где сырой разбор не сказал о времени вообще: у такой записи
    // проверенное время остаётся только отдельной проверенной пометкой полного ключа.
    public static OperationResult<IReadOnlyList<string>> ReconcileTrainedTags(
        IReadOnlyList<string>? rawRuleTags,
        IReadOnlyList<string>? canonicalVerifiedBodies)
    {
        // Пометки сырого разбора проверяются той же меркой, что и проверенные: пустая запись
        // не может быть частью согласованного набора.
        var raw = ValidateTrainedAnnotation(rawRuleTags);
        if (!raw.IsSuccess)
            return raw;

        // Какое время назвала проверенная сторона. Ничего не назвала — реконсилировать нечего.
        var verifiedTense = TenseValueOf(canonicalVerifiedBodies);
        if (verifiedTense is null)
            return OperationResult<IReadOnlyList<string>>.Ok(raw.Value!);

        // Прошедшее время — единственное, при котором пометки прошедшей формы осмысленны.
        var keepsPastDependentTags = string.Equals(verifiedTense, TensePastValue, StringComparison.Ordinal);

        // Согласованный набор в порядке первого появления.
        var reconciled = new List<string>(raw.Value!.Count);

        // Тот же набор как множество, чтобы один и тот же факт не попал дважды.
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var tag in raw.Value!)
        {
            // Пометка о времени получает проверенное значение и остаётся на своём месте.
            if (TryReadTenseFamilyHead(tag, out var head))
            {
                var corrected = $"{head}:{TenseFamily}:{verifiedTense}";
                if (seen.Add(corrected))
                    reconciled.Add(corrected);
                continue;
            }

            // Пометка, осмысленная только при прошедшем времени, при другом времени не
            // сохраняется: она описывала бы вид записи, которого уже нет.
            if (!keepsPastDependentTags && DependsOnPastTense(tag))
                continue;

            // Все прочие пометки сырого разбора переносятся без изменений.
            if (seen.Add(tag))
                reconciled.Add(tag);
        }

        // Набор отдаётся готовым: именно он кладётся в урок, в ключи и в слепок состояния.
        return OperationResult<IReadOnlyList<string>>.Ok(reconciled);
    }

    // ValidateTrainedAnnotation проверяет форму набора учебных пометок ДО того, как он попадёт в
    // обученное состояние или будет восстановлен из слепка, и возвращает его защитную копию.
    //
    // Смысл проверки: набор обязан называть хотя бы непустые факты и не может утверждать сразу
    // прошедшее и настоящее время одной записи. Пустой набор — допустимое честное состояние.
    public static OperationResult<IReadOnlyList<string>> ValidateTrainedAnnotation(
        IReadOnlyList<string>? ruleTags)
    {
        // Отсутствующий набор — это пустой набор, а не отказ: у записи просто нет пометок.
        if (ruleTags is null || ruleTags.Count == 0)
            return OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>());

        // Копия делается сразу: наружу не отдаётся ссылка на чужую изменяемую коллекцию.
        var copied = new List<string>(ruleTags.Count);

        // Множество нужно и для поиска противоречия времени.
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var tag in ruleTags)
        {
            // Пустая пометка не называет никакого факта.
            if (string.IsNullOrWhiteSpace(tag))
                return OperationResult<IReadOnlyList<string>>.Fail(
                    "A trained rule annotation contains an empty tag.",
                    "TRAINED_RULE_TAG_EMPTY");

            // Повтор одного и того же факта ничего не добавляет и в набор не попадает.
            if (seen.Add(tag))
                copied.Add(tag);
        }

        // Два разных времени у одной записи — это та самая смесь, которую чинит этот перенос.
        var tenseValues = copied
            .Where(tag => TryReadTenseFamilyHead(tag, out _))
            .Select(tag => tag[(tag.LastIndexOf(':') + 1)..])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (tenseValues.Length > 1)
            return OperationResult<IReadOnlyList<string>>.Fail(
                $"A trained rule annotation states {tenseValues.Length} different tenses at once: " +
                $"{string.Join(", ", tenseValues)}.",
                "TRAINED_RULE_TAG_TENSE_CONFLICT");

        // Форма набора принята; отдаётся его копия.
        return OperationResult<IReadOnlyList<string>>.Ok(copied);
    }

    // TenseValueOf отвечает, какое время назвали канонические тела проверенной аннотации:
    // "PAST", "PRESENT" или ничего. Противоречие здесь уже невозможно: его отвергает
    // Canonicalize до выдачи тел.
    private static string? TenseValueOf(IReadOnlyList<string>? canonicalVerifiedBodies)
    {
        // Аннотации нет — времени она не называет.
        if (canonicalVerifiedBodies is null)
            return null;

        // Сравнение идёт с одним принятым написанием каждого из двух времён.
        foreach (var body in canonicalVerifiedBodies)
        {
            if (string.Equals(body, TensePastBody, StringComparison.Ordinal))
                return TensePastValue;
            if (string.Equals(body, TensePresentBody, StringComparison.Ordinal))
                return TensePresentValue;
        }

        // Ни одно тело о времени не говорит.
        return null;
    }

    // TryReadTenseFamilyHead отвечает, называет ли ОДНА пометка сырого разбора время, и если да —
    // отдаёт её начало до семейства времени (для "RU:TENSE:PAST" это "RU").
    //
    // Начало возвращается вместо того, чтобы вписывать "RU" самому: язык пометки задаёт разбор,
    // и придумывать его за него нельзя.
    private static bool TryReadTenseFamilyHead(string tag, out string head)
    {
        // Пустого начала не бывает: пометка без начала временем не считается.
        head = string.Empty;

        // Семейство времени стоит отдельным отрезком между началом и значением.
        var marker = $":{TenseFamily}:";
        var at = tag.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (at <= 0)
            return false;

        // После значения времени других отрезков быть не должно: иначе это другая пометка.
        if (tag.IndexOf(':', at + marker.Length) >= 0)
            return false;

        // Значение времени обязано быть непустым.
        if (at + marker.Length >= tag.Length)
            return false;

        // Начало пометки отдаётся вызывающему, чтобы он собрал её заново с нужным временем.
        head = tag[..at];
        return true;
    }

    // DependsOnPastTense отвечает, осмысленна ли пометка ТОЛЬКО при прошедшем времени.
    //
    // Таких семейств два: пол глагола прошедшей формы и согласование подлежащего с прошедшей
    // формой. Обе пометки описывают именно прошедшую форму глагола, поэтому при другом
    // проверенном времени они противоречили бы собственному времени записи.
    private static bool DependsOnPastTense(string tag)
        // Сравнение без учёта регистра: написание пометок задаёт разбор, а не этот вычислитель.
        => tag.Contains("VERB_GENDER", StringComparison.OrdinalIgnoreCase) ||
           tag.Contains($"_{TensePastValue}_", StringComparison.OrdinalIgnoreCase);

    // TryCanonicalizeTense answers two questions about one trimmed entry: does it state the tense
    // family at all, and if it does, which of the two accepted tense bodies it means.
    // It returns null both for an entry of another family (statesTense false) and for a tense
    // entry whose value is not PAST or PRESENT (statesTense true), so the caller can tell a
    // harmless other-family entry apart from a tense entry that must be refused.
    private static string? TryCanonicalizeTense(string trimmed, out bool statesTense)
    {
        // Compare without case, because a verified row may be written in either case.
        var upper = trimmed.ToUpperInvariant();

        // An entry states the tense family only when it begins with that family name.
        if (!upper.StartsWith(TenseFamily, StringComparison.Ordinal))
        {
            statesTense = false;
            return null;
        }

        // The bare family name states a tense without saying which one: that is a tense entry
        // with an unreadable value, not an entry of some other family.
        if (upper.Length == TenseFamily.Length)
        {
            statesTense = true;
            return null;
        }

        // A longer entry belongs to another family unless a separator follows the family name,
        // so a word such as TENSED is not mistaken for a statement about tense.
        if (Array.IndexOf(FamilySeparators, upper[TenseFamily.Length]) < 0)
        {
            statesTense = false;
            return null;
        }

        // From here the entry really is a statement about tense, right or wrong.
        statesTense = true;

        // Read the value that follows the separator.
        var value = upper[(TenseFamily.Length + 1)..];

        // Only the two tense values this family accepts become a canonical body.
        return value switch
        {
            "PAST" => TensePastBody,
            "PRESENT" => TensePresentBody,
            _ => null
        };
    }
}
