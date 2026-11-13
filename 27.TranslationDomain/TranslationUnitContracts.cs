// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text.RegularExpressions;

// This file defines all per-unit contracts for the structured translation training pipeline.
// Governed by: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md Direction 1, decisions A-LM-02, A-LM-04.
// Architecture:
//   TranslationMachine
//     └── TrainStructuredBatchAsync(TranslationLearningBatch)
//           └── routes by UnitId to ITranslationUnitTrainer
//   TranslationRoutingFlow (inference)
//     ├── SemanticTokenizerUnit
//     ├── StructureKnnRetrieverUnit (via TranslationKnnStore)
//     ├── ExecutorUnit<direction>
//     └── QualityVerifierUnit

// SentencePattern names the eight sentence structure patterns from E25 experiment
// (POS bi-gram K-NN classification). Determines word-order rewriting in ExecutorUnit.
public enum SentencePattern
{
    // SVO: subject-verb-object — neutral Russian ("Я добавляю проверку") and default English.
    SVO,
    // SOV: subject-object-verb — Russian topicalization ("Я проверку добавляю").
    SOV,
    // VSO: verb-subject-object — Hebrew action sentences.
    VSO,
    // IMPERATIVE_V: verb+object only, no explicit subject — the dominant form for technical prompts.
    IMPERATIVE_V,
    // IMPERATIVE_VS: verb+subject+object — Hebrew emphatic imperative.
    IMPERATIVE_VS,
    // SVO_INF: starts with infinitive verb, followed by object — Russian goal statements ("запустить поток").
    SVO_INF,
    // UNKNOWN: could not classify with available POS tags.
    UNKNOWN
}

// TranslationPos are the coarse POS (Part-Of-Speech) tags used by the semantic tokenizer.
// Derived from E25/E26 experiments — deliberately coarse to keep the vocabulary small.
public enum TranslationPos
{
    VERB,
    NOUN,
    NOUN_GEN,       // Russian genitive noun (обработки, данных)
    PREP,
    PRON,
    ADJ,
    ADV,
    TECH_TERM,      // language-independent keyword: null, true, false, 404
    CODE_IDENTIFIER // PascalCase or camelCase identifier: ValidateToken, IService
}

// SemanticConcept names the domain concept carried by a token.
// These are the building blocks of the structural signature used for KNN (K-Nearest Neighbour) retrieval.
public enum SemanticConcept
{
    ADD_OP,
    VALIDATE_CHECK,
    NULL_LITERAL,
    METHOD_REF,
    CODE_IDENTIFIER,
    LOG_OP,
    WRITE_OP,
    THREAD_REF,
    FLOW_REF,
    STREAM_REF,
    ERROR_EXCEPTION,
    PROCESSING_OF,
    DATA_ENTITY,
    LAUNCH_OP,
    RETURN_OP,
    HANDLE_OP,
    UNKNOWN
}

// SemanticToken is one tokenized word with its full structural metadata.
public sealed record SemanticToken(
    // Word is the original word from the source text.
    string Word,
    // Lemma is the normalised base form used for concept lookup and E26 disambiguation.
    string Lemma,
    // Pos is the coarse part-of-speech tag.
    TranslationPos Pos,
    // Role is the syntactic role in the sentence: IMPERATIVE, DIRECT_OBJ, CODE_NAME, etc.
    string Role,
    // Concept is the semantic domain concept — UNKNOWN when not recognisable.
    SemanticConcept Concept);

// SemanticTokenSequence is the full structural analysis of one source sentence.
// Signature is used as the KNN lookup key: "IMPERATIVE_V|ADD_OP|VALIDATE_CHECK|NULL_LITERAL|METHOD_REF".
public sealed record SemanticTokenSequence(
    // Pattern is the sentence structure (SVO, IMPERATIVE_V, etc.).
    SentencePattern Pattern,
    // Tokens is the ordered list of semantic tokens.
    IReadOnlyList<SemanticToken> Tokens,
    // PosBigrams is the ordered list of consecutive POS tag pairs — used for E25 KNN matching.
    IReadOnlyList<(TranslationPos A, TranslationPos B)> PosBigrams,
    // Signature is the pipe-delimited structural key: "{Pattern}|{concept1}|{concept2}|...".
    string Signature,
    // PrefixSignature is the pattern plus the first few non-empty concepts.
    string PrefixSignature,
    // StructureSignature is the pattern plus POS roles without specific concepts.
    string StructureSignature,
    // RuleTags are language-structure features used by KNN: tense, case, agreement,
    // exception class, and other grammar signals. They are structural addresses,
    // not translation decisions.
    IReadOnlyList<string> RuleTags);

public static class TranslationUnitIds
{
    public const string SemanticTokenizer = "semantic-tokenizer";
    public const string ExecutorRuEn = "executor-ru_en";
    public const string ExecutorEnRu = "executor-en_ru";
    public const string ExecutorHeEn = "executor-he_en";
    public const string ExecutorEnHe = "executor-en_he";
    public const string QualityVerifier = "quality-verifier";

    public static string ExecutorFor(TranslationDirection direction)
        => $"executor-{direction.ToString().ToLowerInvariant()}";
}

// TranslationLearningExample is the per-unit structured training example.
// It replaces the flat LearningTrainingItem for the translation structured training path.
// Each unit gets its own examples with typed input/output JSON specific to that unit's contract.
public sealed record TranslationLearningExample(
    // ExampleId is the stable identifier for this example row.
    string ExampleId,
    // UnitId is the routing key — matches ITranslationUnitTrainer.UnitId.
    string UnitId,
    // FlowId is the flow this example belongs to.
    string FlowId,
    // InputJson is the unit-specific input object serialised to JSON.
    string InputJson,
    // ChosenOutputJson is the preferred (correct) unit output, serialised to JSON.
    string ChosenOutputJson,
    // RejectedOutputJson is the non-preferred output with a [WRONG: weakness - gap - consequence] annotation.
    string RejectedOutputJson,
    // RejectedType is a short label for the rejection category (e.g. "wrong_pos_compound").
    string RejectedType,
    // SemanticSignature is the structural signature used for KNN indexing.
    string SemanticSignature,
    // SourceId names the corpus source that produced this example.
    string SourceId,
    // HeldOut = true means this row is used only for accuracy measurement, not for training.
    bool HeldOut,
    // PairId хранит ПРОИСХОЖДЕНИЕ урока — номер учебной пары, из которой он получен.
    // Происхождение вынесено сюда из рабочего ключа: ключ обязан зависеть только от смысла и
    // структуры текста, а номер пары смыслом текста не является. Поле необязательное и по
    // умолчанию пустое, поэтому все прежние вызовы по позициям продолжают работать без правок.
    string? PairId = null,
    // KeySchemaVersion называет форму значения SemanticSignature у ЭТОЙ строки: версия 1 —
    // ключ мог нести отрезок происхождения внутри себя, версия 2 — ключ канонический.
    //
    // Пустое значение означает «не названо», и это разрешённый случай: прежние писатели этого
    // поля не знают, и ломать их договор нельзя. Для такой строки форма ключа ВЫВОДИТСЯ из самих
    // данных, а не предполагается.
    //
    // Почему не «по умолчанию текущая версия». Прежде здесь стояла текущая версия как значение
    // по умолчанию, и это была ложь о данных: строка со старым ключом, несущим отрезок
    // происхождения, молча помечалась канонической. Объявленная величина обязана либо выводиться
    // из данных, либо сверяться с ними; названная версия, противоречащая ключу, даёт отказ с
    // названием обеих величин.
    int? KeySchemaVersion = null);

// TranslationLearningBatch carries one batch of structured per-unit examples.
// Passed to TranslationMachine.TrainStructuredBatchAsync.
public sealed record TranslationLearningBatch(
    // BatchId uniquely identifies this batch within the domain training history.
    string BatchId,
    // DomainTag is the string name of the domain ("TranslationDomain").
    string DomainTag,
    // Examples holds all per-unit structured training examples for this batch.
    IReadOnlyList<TranslationLearningExample> Examples,
    // SourceIds lists the corpus sources that contributed examples to this batch.
    IReadOnlyList<string> SourceIds);

// ITranslationUnitTrainer is implemented by each trainable unit in the translation flow.
// Design rule A-LM-02: Train* methods exist only on ILearningMachine and on the unit trainers
// it delegates to — never directly on ILogicalUnit or ILogicalFlow.
public interface ITranslationUnitTrainer
{
    // UnitId is the stable routing key — must match TranslationLearningExample.UnitId.
    string UnitId { get; }

    // TrainOnExample processes one DPO (Direct Preference Optimisation) training example
    // and updates the unit's internal weights or lookup tables.
    void TrainOnExample(TranslationLearningExample example);

    // MeasureAccuracy evaluates the held-out examples and returns accuracy in [0.0, 1.0].
    // Called after all train examples in a batch have been processed.
    double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut);
}

// TranslationKnnStore is the shared bridge between training and inference for ExecutorUnit.
// Training writes (signature → translation) pairs here; inference reads them.
// Shared state is the only mechanism that connects TrainBatchAsync with GetOrchestratorAsync.
public sealed class TranslationKnnStore
{
    // SourceAwareBroadSignatureSimilarity is the minimum source-token overlap for broad fallback buckets.
    private const double SourceAwareBroadSignatureSimilarity = 0.35;

    // SourceAwarePrefixSimilarity is the lower overlap accepted for more specific prefix buckets.
    private const double SourceAwarePrefixSimilarity = 0.25;

    // SourceTokenRegex extracts language tokens from source text for source-aware KNN guard comparisons.
    private static readonly Regex SourceTokenRegex = new(@"[\p{L}\p{N}_$]+", RegexOptions.Compiled);

    // LowInformationSourceWords are ignored when comparing two source sentences.
    private static readonly HashSet<string> LowInformationSourceWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
        "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
        "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those"
    };

    // _store maps structural signature to the list of (source, target) translation pairs.
    private readonly Dictionary<string, List<(string Source, string Target)>> _store
        = new(StringComparer.Ordinal);

    // AddPair stores one learned translation pair under its structural signature.
    public void AddPair(string signature, string sourceText, string targetText)
    {
        // Ensure the bucket for this signature exists.
        if (!_store.TryGetValue(signature, out var bucket))
        {
            bucket = new List<(string, string)>();
            _store[signature] = bucket;
        }
        // Append the new pair to the bucket — later pairs have higher DPO preference signal.
        bucket.Add((sourceText, targetText));
    }

    // FindNearest returns up to k translation pairs with signatures closest to the query.
    // Exact match is tried first; partial component overlap is the fallback.
    public IReadOnlyList<(string Source, string Target)> FindNearest(string signature, int k = 3)
    {
        // Exact match: return the bucket directly when the signature is known.
        if (_store.TryGetValue(signature, out var exact))
            return exact.Take(k).ToList();

        // Partial match: score each stored signature by component overlap with the query.
        var queryComponents = new HashSet<string>(signature.Split('|'), StringComparer.Ordinal);
        return _store
            .Select(kv => (
                Score: kv.Key.Split('|').Count(c => queryComponents.Contains(c)),
                Pairs: kv.Value))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(k)
            .SelectMany(x => x.Pairs)
            .Take(k)
            .ToList();
    }

    // FindNearestCascade searches by decreasing structural specificity:
    // full signature, then prefix signature, then exact POS-structure signature.
    // It deliberately avoids loose component overlap in the cascade, because a
    // broad "SVO" overlap can pull an unrelated trained sentence into runtime.
    // It decides how to search, not what translation means; stored targets come only from training data.
    // enableEnglishPolarityField is a trailing opt-in flag. It defaults to false, so every
    // existing caller keeps the original admission default; only a caller that explicitly
    // opts in gets the English source polarity check in front of the legacy rule.
    // candidatePredicate is a second trailing opt-in. Допуск вызывающей стороны стоит ДО
    // ограничения числа соседей: уровень сначала оставляет только пригодные записи и лишь затем
    // берёт из них первые k. Уровень, опустевший после такого отбора, проваливается на следующий,
    // более общий, а не заканчивает поиск. Если проверку не передали, поведение прежнее для всех
    // остальных потребителей этой памяти.
    public IReadOnlyList<(string Source, string Target)> FindNearestCascade(
        string fullSignature,
        string prefixSignature,
        string structureSignature,
        int k = 3,
        string? querySourceText = null,
        bool enableEnglishPolarityField = false,
        // candidatePredicate is a second trailing opt-in, added after the polarity flag so every
        // existing positional caller keeps working unchanged. It answers one question about one
        // already-ranked candidate: may this stored pair be handed to the caller at all? A caller
        // that passes nothing keeps exactly the previous behaviour.
        Func<string, string, bool>? candidatePredicate = null)
    {
        // The source guard now covers the most precise tier as well. A full structural
        // signature only says that two sentences have the same shape, not that they are about
        // the same thing, so the previous floor of zero for a detailed signature let this tier
        // hand back the first stored pair in the bucket whatever its words were: a question
        // about one subject was answered with another subject's learned translation, and a
        // subject with nothing learned about it received that same answer instead of a refusal.
        //
        // The floor is the one the broader tiers already use, so the most precise tier can no
        // longer be more permissive than the tiers below it. That also removes the need for the
        // shape-only check that used to pick between two different floors here, because there
        // is only one floor now.
        //
        // Only this floor changed. The prefix floor, the structure floor, the way source
        // overlap is measured, how many neighbours come back, the order of the tiers and the
        // opt-in English polarity field are all untouched.
        //
        // A caller that supplies no query source text now receives nothing from this tier,
        // because there is nothing to compare it against; a run without source text therefore
        // proves nothing about this guard. Every caller in this repository passes the query
        // text today.
        var fullThreshold = SourceAwareBroadSignatureSimilarity;
        // Полный допуск — закон источника, порог похожести источника и проверка вызывающей
        // стороны — стоит ДО ограничения числа соседей и ДО решения об остановке уровня. Прежде
        // проверка вызывающей стороны стояла после ограничения, и ограничение успевало выбросить
        // годную запись, стоявшую за непригодными: вызывающая сторона получала «ничего не
        // выучено» там, где годный ответ в памяти был.
        var full = FindExactWithSourceGuard(
            fullSignature, k, querySourceText, fullThreshold, enableEnglishPolarityField, candidatePredicate);
        // Решение остановиться на этом уровне принимается по результату уже полного допуска,
        // а не по обрезанному набору: пустой уровень означает «пригодных записей здесь нет».
        if (full.Count > 0) return full;

        // Укороченный адрес проходит тот же полный допуск до своего ограничения.
        var prefix = FindExactWithSourceGuard(
            prefixSignature, k, querySourceText, SourceAwarePrefixSimilarity, enableEnglishPolarityField, candidatePredicate);
        // Остановка на укороченном уровне — тоже по результату полного допуска.
        if (prefix.Count > 0) return prefix;

        // Частеречный адрес — последний, и допуск у него такой же полный.
        var structure = FindExactWithSourceGuard(
            structureSignature, k, querySourceText, SourceAwareBroadSignatureSimilarity, enableEnglishPolarityField, candidatePredicate);
        // Последняя остановка: более общих уровней за этим уже нет.
        if (structure.Count > 0) return structure;

        return [];
    }

    private IReadOnlyList<(string Source, string Target)> FindExactWithSourceGuard(
        string signature,
        int k,
        string? querySourceText,
        double minimumSourceSimilarity,
        // The same opt-in flag is carried down to the law without changing any threshold.
        bool enableEnglishPolarityField,
        // Проверка пригодности от вызывающей стороны приходит внутрь уровня, потому что только
        // здесь она успевает встать до ограничения числа соседей. Снаружи, после ограничения,
        // она уже не могла вернуть запись, которую ограничение выбросило.
        Func<string, string, bool>? candidatePredicate)
    {
        if (!_store.TryGetValue(signature, out var exact))
            return [];

        // Apply candidate-conditioned laws before the cap at every cascade level, including full signatures.
        // An unknown analysis remains eligible for the existing similarity and final-verifier checks.
        // Both arguments are SOURCE-language texts; the stored target is never classified.
        var admissible = exact.Where(pair => TranslationMemorySourceLaw.Compare(
                querySourceText, pair.Source, enableEnglishPolarityField)
            != TranslationMemorySourceLaw.SourceCompatibility.Contradiction);

        // Порог похожести источника — вторая часть допуска. Само значение порога не менялось:
        // оно по-прежнему приходит параметром от каскада, здесь меняется только порядок.
        if (minimumSourceSimilarity > 0.0)
        {
            // Без значимых слов запроса сравнивать не с чем, и уровень не отвечает: прежнее
            // поведение сохранено ровно в этой же точке.
            var queryTokens = SourceTokens(querySourceText);
            if (queryTokens.Count == 0)
                return [];

            // Отбор по доле общих значимых слов остаётся там же, где и был, — до ограничения.
            admissible = admissible.Where(pair =>
                SourceSimilarity(queryTokens, SourceTokens(pair.Source)) >= minimumSourceSimilarity);
        }

        // Проверка вызывающей стороны — последняя часть отбора и тоже ДО ограничения. Порядок
        // внутри отбора на состав результата не влияет: все условия соединены союзом «и». Делегат
        // при этом вызывается только на тех парах, которые сама память уже признала допустимыми.
        if (candidatePredicate is not null)
            admissible = admissible.Where(pair => candidatePredicate(pair.Source, pair.Target));

        // Ограничение числа соседей стоит ПОСЛЕДНИМ: первые k берутся из уже пригодных записей,
        // поэтому годная запись, стоявшая в ячейке за непригодными, теперь достижима.
        return admissible.Take(k).ToList();
    }

    private static HashSet<string> SourceTokens(string? sourceText)
        => SourceTokenRegex
            .Matches(sourceText ?? string.Empty)
            .Select(match => match.Value.Trim().ToLowerInvariant())
            .Where(token => token.Length >= 2)
            .Where(token => !LowInformationSourceWords.Contains(token))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static double SourceSimilarity(HashSet<string> queryTokens, HashSet<string> candidateTokens)
    {
        if (queryTokens.Count == 0 || candidateTokens.Count == 0)
            return 0.0;

        var overlap = queryTokens.Intersect(candidateTokens, StringComparer.OrdinalIgnoreCase).Count();
        return overlap / (double)queryTokens.Count;
    }

    // Count returns the total number of stored translation pairs across all signatures.
    public int Count => _store.Values.Sum(b => b.Count);

    // ExportCheckpoint builds a TranslationKnnStoreCheckpoint from all current in-memory pairs.
    // Entries are sorted deterministically: signature ascending, then source ascending.
    // The export does not write to disk — the caller is responsible for persistence.
    internal TranslationKnnStoreCheckpoint ExportCheckpoint()
    {
        // Flatten all buckets into a sorted list of (signature, source, target) triples.
        var entries = _store
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .SelectMany(kv => kv.Value
                .Select(pair => new TranslationKnnStoreEntryCheckpoint(kv.Key, pair.Source, pair.Target)))
            .ToList();
        // Wrap in the checkpoint record and return.
        return new TranslationKnnStoreCheckpoint(entries);
    }

    // ImportCheckpoint clears all current pairs and rebuilds the store from checkpoint entries.
    // After import the store is identical to the state it was in when ExportCheckpoint was called.
    internal void ImportCheckpoint(TranslationKnnStoreCheckpoint checkpoint)
    {
        // Remove all previously learned pairs before restoring from checkpoint.
        _store.Clear();
        // Каждая запись восстанавливается тем же методом, которым её положило обучение, но под
        // КАНОНИЧЕСКИМ ключом. Это перенос старой сохранённой памяти: ячейка, записанная прежней
        // формой ключа с отрезком происхождения, при загрузке встаёт по тому адресу, по которому
        // её ищет чтение. Содержимое ячеек — источник и перевод — не меняется, а две записи,
        // сошедшиеся в один ключ, просто лежат в одной ячейке рядом, как это и делает обучение.
        foreach (var entry in checkpoint.Entries)
            AddPair(TranslationSemanticKey.Canonicalize(entry.Signature), entry.Source, entry.Target);
    }
}
