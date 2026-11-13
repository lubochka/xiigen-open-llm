// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text.Json;
using System.Text.Json.Serialization;

// This file defines the three trainable units of the translation pipeline:
//   SemanticTokenizerUnit  — corrects pattern classification via DPO feedback
//   ExecutorUnit           — assembles translations via KNN + E26 word-choice + E27 reorder
//   QualityVerifierUnit    — verifies translations against learned concept requirements
//
// Each unit implements ITranslationUnitTrainer.
// The bridge between training and inference is TranslationKnnStore (shared by reference).
// Governed by: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md Direction 1, decisions A-LM-02, A-LM-04.

// ── JSON transfer objects (shared by all three units) ──────────────────────────

// TokenizerInput is the JSON-serialised input for SemanticTokenizerUnit training examples.
public sealed record TokenizerInput(string RawText, string SourceLang);

// TokenizerOutput is the JSON-serialised chosen output for SemanticTokenizerUnit training examples.
//
// RuleTags и AnnotationVersion добавлены последними полями намеренно: учебная строка прежней
// формы читается как версия 0 без набора пометок, то есть остаётся ровно тем, чем была.
public sealed record TokenizerOutput(
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SentencePattern Pattern,
    string Signature,
    // RuleTags — согласованный набор учебных пометок ЭТОЙ записи. Он описывает тот же
    // исправленный ответ, что и образец предложения с подписью, а не прежний вид записи.
    IReadOnlyList<string>? RuleTags = null,
    // AnnotationVersion называет форму записи аннотации: 0 — прежняя запись, у которой набора
    // пометок нет и согласованность ключей по ней восстановить нечем; 1 — запись с собственным
    // согласованным набором. Версия 0 не получает статус исправленной сама по себе.
    int AnnotationVersion = 0);

// TranslationTokenizerAnnotationVersions называет формы записи учебной аннотации словами,
// чтобы номер версии не стоял в коде голым числом.
public static class TranslationTokenizerAnnotationVersions
{
    // Legacy — прежняя запись: только образец предложения и подпись.
    public const int Legacy = 0;

    // ConsistentTagSet — запись с собственным согласованным набором пометок.
    public const int ConsistentTagSet = 1;
}

// ExecutorInput is the JSON-serialised input for ExecutorUnit training examples.
public sealed record ExecutorInput(string SourceText, string SourceLang, string TopicDomain);

// ExecutorWordMapping is one learned word-to-word mapping with its associated concept.
public sealed record ExecutorWordMapping(
    string SourceWord,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SemanticConcept Concept,
    string TargetWord);

public sealed record ExecutorPhraseMapping(
    string SourcePhrase,
    string TargetPhrase);

// ExecutorOutput is the JSON-serialised chosen output for ExecutorUnit training examples.
public sealed record ExecutorOutput(
    string Text,
    double Confidence,
    IReadOnlyList<ExecutorWordMapping>? WordMappings,
    IReadOnlyList<ExecutorPhraseMapping>? PhraseMappings = null);

// VerifierInput is the JSON-serialised input for QualityVerifierUnit training examples.
public sealed record VerifierInput(string SourceText, string CandidateText);

// VerifierOutput is the JSON-serialised chosen output for QualityVerifierUnit training examples.
public sealed record VerifierOutput(
    double Score,
    IReadOnlyList<string>? Issues,
    bool Pass,
    string? CorrectionHint);

public sealed record VerificationResult(
    double Score,
    IReadOnlyList<string> Issues,
    bool Pass,
    string? CorrectionHint);

// ── SemanticTokenizerUnit ──────────────────────────────────────────────────────

// SemanticTokenizerUnit wraps TranslationSemanticTokenizer and applies DPO-trained corrections.
// Training: reads chosen pattern from example, stores it as a correction for the example's signature.
// Inference: runs rule-based tokenizer then overlays trained pattern corrections.
// UnitId: "semantic-tokenizer"
public sealed class SemanticTokenizerUnit : ITranslationUnitTrainer
{
    // UnitId is the routing key used in TranslationLearningExample.UnitId.
    public string UnitId => "semantic-tokenizer";

    // _tokenizer is the rule-based semantic tokenizer used as the Bootstrap fallback.
    private readonly TranslationSemanticTokenizer _tokenizer = new();

    // _patternCorrections stores trained overrides: input_signature → corrected_pattern.
    // Populated when TrainOnExample receives an example that disagrees with the rule-based result.
    private readonly Dictionary<string, SentencePattern> _patternCorrections
        = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TokenizerOutput> _signatureCorrections
        = new(StringComparer.OrdinalIgnoreCase);

    // Tokenize runs the base tokenizer and applies any trained pattern correction for this input.
    public SemanticTokenSequence Tokenize(string rawText, string sourceLang)
    {
        // Run the base rule-based tokenizer to get the initial sequence.
        var seq = _tokenizer.Tokenize(rawText, sourceLang);

        if (_signatureCorrections.TryGetValue(Normalize(rawText), out var exactCorrection))
        {
            // Запись версии 1 несёт СВОЙ согласованный набор учебных пометок. Прежде поправка
            // меняла образец предложения и подпись, а пометки оставляла от прежнего, ошибочного
            // вида записи: исправленный ответ носил пометки своего прошлого, префиксный и
            // структурный ключи собирались вообще без пометок, и адрес чтения расходился с
            // адресом, под который обучение положило перевод.
            if (exactCorrection.AnnotationVersion >= TranslationTokenizerAnnotationVersions.ConsistentTagSet)
            {
                // Защитная копия: наружу не отдаётся ссылка на хранимый набор, иначе читатель
                // мог бы незаметно изменить обученное состояние.
                var correctedTags = (exactCorrection.RuleTags ?? []).ToArray();

                // Все ключи собираются ОДНИМ общим вычислителем из исправленного образца, лексем
                // и ровно этого набора пометок. Другой формулы у этой ветви больше нет.
                var correctedKeys = TranslationSemanticTokenizer.ComposeSignatures(
                    exactCorrection.Pattern, seq.Tokens, correctedTags);

                return seq with
                {
                    Pattern = exactCorrection.Pattern,
                    // Полный ключ отдаётся СОХРАНЁННЫЙ: под ним урок лежит в памяти, и он несёт
                    // вдобавок отдельные проверенные пометки учебной строки. Его согласованность с
                    // набором обеспечил конструктор партий, собравший его тем же вычислителем.
                    Signature = exactCorrection.Signature,
                    PrefixSignature = correctedKeys.PrefixSignature,
                    StructureSignature = correctedKeys.StructureSignature,
                    RuleTags = correctedTags
                };
            }

            // Запись прежней формы: набора пометок у неё нет, согласовывать нечем, поэтому её
            // поведение остаётся в точности прежним. Версия 0 не получает статус исправленной
            // сама по себе.
            return seq with
            {
                Pattern = exactCorrection.Pattern,
                Signature = exactCorrection.Signature,
                PrefixSignature = BuildLegacyPrefixSignature(exactCorrection.Pattern, seq.Tokens),
                StructureSignature = BuildLegacyStructureSignature(exactCorrection.Pattern, seq.Tokens)
            };
        }

        // Поиск поправки идёт по КАНОНИЧЕСКОМУ ключу — ровно по тому, под которым обучение её и
        // положило. Разбор правилами отрезка происхождения не содержит, поэтому канонизация здесь
        // ничего не меняет в значении; она стоит явно, чтобы ключ записи и ключ чтения считались
        // одним и тем же способом в одном месте и больше не могли разойтись.
        if (_patternCorrections.TryGetValue(TranslationSemanticKey.Canonicalize(seq.Signature), out var corrected)
            && corrected != seq.Pattern)
        {
            // Поправка ОБРАЗЦА предложения о пометках не говорит ничего, поэтому согласованный
            // набор этой ветви — пометки самого разбора. Прежде здесь подпись пересобиралась
            // подменой начала строки, а префиксный и структурный ключи — другой формулой без
            // пометок; получалась та же смесь нового образца со старыми ключами.
            var correctedSignatures = TranslationSemanticTokenizer.ComposeSignatures(
                corrected, seq.Tokens, seq.RuleTags);

            return seq with
            {
                Pattern = corrected,
                Signature = correctedSignatures.Signature,
                PrefixSignature = correctedSignatures.PrefixSignature,
                StructureSignature = correctedSignatures.StructureSignature
            };
        }

        return seq;
    }

    // TrainOnExample reads the chosen (correct) tokenizer output and stores any pattern correction.
    public void TrainOnExample(TranslationLearningExample example)
    {
        // Held-out examples are for evaluation only — do not train on them.
        if (example.HeldOut) return;

        // Deserialise the chosen output to extract the correct pattern.
        var chosen = JsonSerializer.Deserialize<TokenizerOutput>(example.ChosenOutputJson,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
        if (chosen is null) return;

        // Поправка кладётся под КАНОНИЧЕСКИЙ ключ урока. Прежде здесь стоял ключ учебной строки
        // как есть, и в режиме точной пары он несёт отрезок происхождения; читатель ниже ищет по
        // разбору текста, где происхождения нет, поэтому такая поправка не находилась никогда.
        _patternCorrections[TranslationSemanticKey.Canonicalize(example.SemanticSignature)] = chosen.Pattern;

        var input = JsonSerializer.Deserialize<TokenizerInput>(example.InputJson,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
        if (input is null) return;

        // Форма набора учебных пометок проверяется ДО записи в обученное состояние. Набор,
        // который называет сразу два времени или несёт пустую пометку, описывает не один ответ,
        // а смесь; такую запись принимать нельзя, и прежнее принятое состояние остаётся целым.
        var annotation = TranslationVerifiedRuleTags.ValidateTrainedAnnotation(chosen.RuleTags);
        if (!annotation.IsSuccess) return;

        // Набор копируется: учебная строка снаружи больше не может изменить обученное состояние.
        // Сохранённая подпись урока канонизируется: именно её точный путь чтения возвращает
        // наружу как подпись времени исполнения, и происхождению там не место.
        _signatureCorrections[Normalize(input.RawText)] = chosen with
        {
            Signature = TranslationSemanticKey.Canonicalize(chosen.Signature),
            RuleTags = annotation.Value!.Count == 0 && chosen.RuleTags is null
                ? null
                : annotation.Value!.ToArray()
        };
    }

    private static string Normalize(string text)
    {
        return string.Join(" ", (text ?? "").Trim().Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries));
    }

    // BuildLegacyPrefixSignature — ПРЕЖНЯЯ форма префиксного ключа поздней поправки: образец
    // предложения и первые понятия, без учебных пометок.
    //
    // Она сохранена ровно для записи версии 0, у которой набора пометок нет: по такой записи
    // согласовать ключи нечем, и выдумывать ей пометки запрещено. У записи версии 1 ключи
    // собирает общий вычислитель разбора, и эта форма там не применяется.
    private static string BuildLegacyPrefixSignature(SentencePattern pattern, IReadOnlyList<SemanticToken> tokens)
    {
        var prefix = string.Join("|",
            tokens.Where(t => t.Concept != SemanticConcept.UNKNOWN)
                  .Take(2)
                  .Select(t => t.Concept.ToString()));
        return string.IsNullOrEmpty(prefix) ? pattern.ToString() : $"{pattern}|{prefix}";
    }

    // BuildLegacyStructureSignature — прежняя форма структурного ключа поздней поправки: образец
    // предложения и части речи, без учебных пометок. Применяется только к записи версии 0.
    private static string BuildLegacyStructureSignature(SentencePattern pattern, IReadOnlyList<SemanticToken> tokens)
        => $"{pattern}|{string.Join("|", tokens.Select(t => t.Pos.ToString()))}";

    // MeasureAccuracy evaluates held-out tokenizer examples and returns the fraction
    // where the tokenizer produces the correct sentence pattern.
    public double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
    {
        // Filter to held-out examples for this unit only.
        var relevant = heldOut.Where(e => e.HeldOut && e.UnitId == UnitId).ToList();
        if (relevant.Count == 0) return 0.0;

        var correct = 0;
        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        foreach (var ex in relevant)
        {
            // Deserialise the input and expected output.
            var input    = JsonSerializer.Deserialize<TokenizerInput>(ex.InputJson, opts);
            var expected = JsonSerializer.Deserialize<TokenizerOutput>(ex.ChosenOutputJson, opts);
            if (input is null || expected is null) continue;

            // Run the tokenizer and compare the resulting pattern to the expected pattern.
            var result = Tokenize(input.RawText, input.SourceLang);
            if (result.Pattern == expected.Pattern) correct++;
        }

        return (double)correct / relevant.Count;
    }

    // ExportCheckpoint builds a SemanticTokenizerUnitCheckpoint from all current trained corrections.
    // Pattern corrections are sorted by signature; signature corrections are sorted by normalized key.
    internal SemanticTokenizerUnitCheckpoint ExportCheckpoint()
    {
        // Serialize _patternCorrections as a sorted list of (signature, pattern) entries.
        var patternCorrections = _patternCorrections
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => new SemanticTokenizerPatternCorrectionCheckpoint(kv.Key, kv.Value))
            .ToList();
        // Serialize _signatureCorrections as a sorted list of (normalizedRawText, pattern, signature) entries.
        // Набор учебных пометок и номер формы записи сохраняются вместе с ключом: без них свежая
        // загрузка не могла бы восстановить ни правильные пометки, ни согласие ключей, и
        // восстановленная единица возвращала бы догадку сырого разбора.
        var signatureCorrections = _signatureCorrections
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new SemanticTokenizerSignatureCorrectionCheckpoint(
                kv.Key,
                kv.Value.Pattern,
                kv.Value.Signature,
                // Копия набора: слепок не должен ссылаться на живое обученное состояние.
                kv.Value.RuleTags?.ToArray(),
                kv.Value.AnnotationVersion))
            .ToList();
        // Return the combined checkpoint record.
        return new SemanticTokenizerUnitCheckpoint(patternCorrections, signatureCorrections);
    }

    // ImportCheckpoint clears all trained corrections and restores them from the checkpoint.
    // Does not re-run the tokenizer — restores keys and values exactly as serialized.
    internal void ImportCheckpoint(SemanticTokenizerUnitCheckpoint checkpoint)
    {
        // Clear both correction dictionaries before restoring to avoid stale entries.
        _patternCorrections.Clear();
        _signatureCorrections.Clear();
        // Поправки образцов восстанавливаются под КАНОНИЧЕСКИМ ключом. Это и есть перенос
        // старой сохранённой памяти: ключ, записанный прежней формой с отрезком происхождения,
        // при загрузке приводится к той форме, по которой читатель ищет. Сам файл контрольной
        // точки при этом не меняется — он только читается, и прежняя запись остаётся
        // воспроизводимой.
        foreach (var entry in checkpoint.PatternCorrections)
            _patternCorrections[TranslationSemanticKey.Canonicalize(entry.Signature)] = entry.Pattern;
        // Точные поправки восстанавливаются по прежнему адресу — нормализованному учебному
        // тексту, — а сохранённая подпись приводится к канонической форме по той же причине.
        // Набор учебных пометок и номер формы записи восстанавливаются вместе с подписью, иначе
        // восстановленная единица отдавала бы исправленный образец со старыми пометками.
        foreach (var entry in checkpoint.SignatureCorrections)
            _signatureCorrections[entry.NormalizedRawText] = new TokenizerOutput(
                entry.Pattern,
                TranslationSemanticKey.Canonicalize(entry.TokenizerSignature),
                // Копия набора: живое состояние не должно ссылаться на прочитанный слепок.
                entry.RuleTags?.ToArray(),
                entry.AnnotationVersion);
    }
}

// ── ExecutorUnit ───────────────────────────────────────────────────────────────

// ExecutorUnit assembles the final English translation using:
//   1. KNN (K-Nearest Neighbour) retrieval from TranslationKnnStore (trained pairs)
//   2. E26 contextual word-choice dictionary (concept-aware word mapping)
//   3. E27 minimal word-order correction (TECH_TERM + VALIDATE_CHECK → "null check" not "check null")
//
// The shared TranslationKnnStore is the bridge between training (AddPair) and inference (FindNearest).
// UnitId: "executor-{direction}" e.g. "executor-ru_en"
public sealed class ExecutorUnit : ITranslationUnitTrainer
{
    // Почему порог 0.60. Это минимальная доля общих значимых слов между запросом и сохранённым
    // источником для ОБЫЧНОГО направления английский→русский — то есть для той ветки, где в
    // разборе нет ни одного известного смыслового понятия. В этой ветке у поиска нет никакой
    // другой опоры, кроме совпадения слов: структурный адрес вида «подлежащее-сказуемое-
    // дополнение» говорит только о форме фразы, а не о том, что две фразы об одном предмете.
    // Ниже 0.60 выученный ответ про один предмет начинал отвечать на вопрос про другой.
    //
    // Порог привязан к НАПРАВЛЕНИЮ и к отсутствию смысловых понятий, а не к происхождению ключа
    // и не ко всем ветвям сразу. Там, где смысловые понятия в разборе есть, опора есть, и уже
    // разрешённый пол 0.0 сохраняется без изменений. Общим запретом этот порог не становится.
    private const double EnRuOrdinaryKnnMinimumSourceSimilarity = 0.60;
    private const int EnRuOrdinaryKnnMinimumSharedSourceTokens = 2;

    // _direction is the translation direction this executor handles (RU_EN, HE_EN, etc.).
    private readonly TranslationDirection _direction;

    // UnitId is the direction-specific routing key.
    public string UnitId => $"executor-{_direction.ToString().ToLowerInvariant()}";

    // _knnStore is shared with the inference path — training writes, inference reads.
    private readonly TranslationKnnStore _knnStore;

    // _wordMap is the E26 contextual dictionary: (lemma, concept) → target word.
    // Populated during training; used at inference time to translate individual tokens.
    private readonly Dictionary<(string Lemma, SemanticConcept Concept), string> _wordMap = new();
    private readonly Dictionary<string, string> _surfaceWordMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _phraseMap = new(StringComparer.OrdinalIgnoreCase);
    // _exactTopicSourceMap is the exact translation memory addressed by the pair (topic, source).
    // The address used to be the source text alone, so the finance lesson and the river lesson
    // about the word "bank" shared one cell and the second lesson erased the first one. With the
    // typed pair address both known meanings of one word live side by side.
    private readonly Dictionary<TranslationExactSourceKey, string> _exactTopicSourceMap = new();

    // Наблюдение реально найденных кандидатов перед последней перепроверкой в цикле выбора.
    // Смысл поля изменился вместе с порядком допуска: прежде здесь были записи, которые порог
    // похожести источника ещё НЕ видел, теперь — записи, уже прошедшие полный допуск внутри
    // каскада, включая этот порог. Копия только для чтения не позволяет проверке изменить память
    // или результат каскада.
    internal Action<IReadOnlyList<(string Source, string Target)>, double>? BeforeKnnSourceFloor { get; set; }

    // ExecutorUnit requires a shared KNN store so training and inference use the same data.
    public ExecutorUnit(TranslationDirection direction, TranslationKnnStore knnStore)
    {
        // Store the direction for UnitId generation and inference routing.
        _direction = direction;
        // Share the KNN store — this is the sole bridge between TrainBatchAsync and inference.
        _knnStore = knnStore;
    }

    // Execute assembles a translation for the given semantic token sequence.
    // Path: KNN lookup → exact match → return; fallback → E26 word map + E27 reorder.
    public TranslationCandidate Execute(SemanticTokenSequence tokens, string topicDomain)
    {
        // The request topic is no longer ignored: it is one half of the address of the cell.
        // Both halves are produced by the single shared builder that training also calls, so a
        // lesson can never be stored under an address this reader cannot reproduce.
        var exactKey = TranslationExactSourceKey.Create(
            string.Join(" ", tokens.Tokens.Select(token => token.Word)), topicDomain);
        // Read the cell of this exact topic only; another topic's lesson stays in its own cell.
        if (_exactTopicSourceMap.TryGetValue(exactKey, out var exactTranslation))
            return new TranslationCandidate(exactTranslation, 0.93, UnitId + "-exact-source");

        if (TryComposeFromPhraseMappings(tokens, out var phraseTranslation))
            return new TranslationCandidate(phraseTranslation, 0.86, UnitId + "-phrase-map");

        if (TryComposeFromWordMappings(tokens, out var wordMappedTranslation))
            return new TranslationCandidate(
                wordMappedTranslation,
                0.50,
                UnitId + (tokens.RuleTags.Count > 0 ? "-rule-wordmap" : "-wordmap"));

        // Единственная опора для поиска без порога — настоящее известное смысловое понятие в
        // разборе. Прежде рядом стояло второе условие: «в рабочем ключе есть отрезок
        // происхождения». Оно и было дырой: чужой номер учебной пары в ключе сам по себе уводил
        // обычную строку в ветку с полом ноль, то есть происхождение фактически отменяло порог
        // уверенности направления. Происхождение теперь живёт отдельным полем и на выбор ветки
        // не влияет никак.
        var hasSemanticConcept = tokens.Tokens.Any(token => token.Concept != SemanticConcept.UNKNOWN);
        if (hasSemanticConcept)
        {
            if (TrySelectKnnCandidate(tokens, minimumSourceSimilarity: 0.0, out var semanticKnnCandidate))
                return semanticKnnCandidate;
        }
        else if (_direction == TranslationDirection.EN_RU &&
            TrySelectKnnCandidate(tokens, EnRuOrdinaryKnnMinimumSourceSimilarity, out var ordinaryKnnCandidate))
        {
            return ordinaryKnnCandidate with
            {
                Confidence = 0.84,
                AdapterId = UnitId + "-ordinary-source-knn"
            };
        }

        var parts = new List<string>(tokens.Tokens.Count);
        var usedLearnedMapping = false;
        foreach (var token in tokens.Tokens)
        {
            // CODE_IDENTIFIERs are never translated — kept verbatim.
            if (token.Pos == TranslationPos.CODE_IDENTIFIER) { parts.Add(token.Word); continue; }
            // Check the E26 contextual word map first (concept-aware).
            if (TryGetAcceptedWordMapping(token, out var mapped))
            {
                parts.Add(mapped);
                usedLearnedMapping = true;
                continue;
            }

            // Fallback: use the original word (Bootstrap behaviour for unknown tokens).
            parts.Add(token.Word);
        }

        // Step 4: apply minimal E27 correction — TECH_TERM before VALIDATE_CHECK → swap.
        ApplyE27Reorder(parts, tokens.Tokens);

        if (usedLearnedMapping)
        {
            var mappedTranslation = NormalizeComposedTranslation(string.Join(" ", parts));
            if (IsTargetCompatibleForDirection(tokens, mappedTranslation))
                return new TranslationCandidate(
                    mappedTranslation,
                    0.50,
                    UnitId + "-wordmap");
        }

        return new TranslationCandidate(
            string.Join(" ", parts),
            0.49,
            UnitId + "-fallback");
    }

    private bool TrySelectKnnCandidate(
        SemanticTokenSequence tokens,
        double minimumSourceSimilarity,
        out TranslationCandidate candidate)
    {
        candidate = new TranslationCandidate(string.Empty, 0.0, UnitId);
        var sourceText = string.Join(" ", tokens.Tokens.Select(token => token.Word));
        var nearest = _knnStore.FindNearestCascade(
            TranslationSemanticKey.Canonicalize(tokens.Signature),
            TranslationSemanticKey.Canonicalize(tokens.PrefixSignature),
            TranslationSemanticKey.Canonicalize(tokens.StructureSignature),
            k: 5,
            querySourceText: sourceText,
            // Opt in to the English source polarity field for EN_RU only: a contracted
            // negation such as "can't" must not admit the positive memory entry and return
            // the opposite meaning. Every other direction keeps the legacy admission default.
            enableEnglishPolarityField: _direction == TranslationDirection.EN_RU,
            // Tell the lookup which stored pairs this direction can actually use, so that a tier
            // made up only of unusable targets no longer ends the search. Before this, such a tier
            // was returned as the answer, every candidate in it was then dropped by the checks in
            // the loop below, and this method reported "nothing learned" although the correct
            // translation was sitting one tier down: training writes every learned pair under the
            // full, the prefix and the structure address alike, so a usable answer is often there.
            //
            // Сюда передаётся ПОЛНЫЙ допуск этой единицы, а не только половина про цель. Нижний
            // порог похожести источника прежде оставался после каскада, и от этого возвращалась
            // та же болезнь на другом условии: уровень приходил непустым, порог отвергал на нём
            // все записи, а спуститься на более общий уровень было уже нельзя. Значение порога
            // НЕ менялось — меняется только место, где его спрашивают.
            candidatePredicate: (candidateSource, target) =>
                // Первая половина допуска: цель непустая и пригодна для этого направления.
                !string.IsNullOrWhiteSpace(target)
                && IsTargetCompatibleForDirection(tokens, target)
                // Вторая половина: при нулевом поле порог не спрашивается вовсе, поэтому законная
                // смысловая ветвь сохраняет свой пол ноль в точности без изменений.
                && (minimumSourceSimilarity <= 0.0
                    || SourceTokenSimilarity(sourceText, candidateSource) >= minimumSourceSimilarity));

        BeforeKnnSourceFloor?.Invoke(Array.AsReadOnly(nearest.ToArray()), minimumSourceSimilarity);

        foreach (var neighbor in nearest)
        {
            // Те же три условия остаются здесь последней перепроверкой уже допущенных записей.
            // Первой и единственной точкой применения они быть перестали: решение «годится ли
            // запись» теперь принимается внутри каскада, до ограничения числа соседей.
            if (string.IsNullOrWhiteSpace(neighbor.Target))
                continue;

            if (!IsTargetCompatibleForDirection(tokens, neighbor.Target))
                continue;

            if (minimumSourceSimilarity > 0.0 &&
                SourceTokenSimilarity(sourceText, neighbor.Source) < minimumSourceSimilarity)
                continue;

            candidate = new TranslationCandidate(neighbor.Target, 0.88, UnitId + "-knn");
            return true;
        }

        return false;
    }

    private bool TryComposeFromWordMappings(SemanticTokenSequence tokens, out string translation)
    {
        translation = string.Empty;
        var parts = new List<string>(tokens.Tokens.Count);
        var mappedTokens = 0;

        foreach (var token in tokens.Tokens)
        {
            if (token.Pos == TranslationPos.CODE_IDENTIFIER) { parts.Add(token.Word); continue; }
            if (TryGetAcceptedWordMapping(token, out var mapped))
            {
                parts.Add(mapped);
                mappedTokens++;
                continue;
            }

            // Do not copy unknown source words into the target candidate here:
            // mixed-language output is not a learned translation.
        }

        if (parts.Count == tokens.Tokens.Count)
            ApplyE27Reorder(parts, tokens.Tokens);

        var translatableTokenCount = tokens.Tokens.Count(token => token.Pos != TranslationPos.CODE_IDENTIFIER);
        var mappedRatio = translatableTokenCount == 0
            ? 0.0
            : mappedTokens / (double)translatableTokenCount;
        if (mappedTokens < 2 || mappedRatio < 0.50)
            return false;

        translation = NormalizeComposedTranslation(string.Join(" ", parts));
        if (!IsTargetCompatibleForDirection(tokens, translation))
            return false;

        return !string.IsNullOrWhiteSpace(translation);
    }

    // Общий держатель настроек разбора уроков этой единицы. Прежде настройки
    // создавались заново на КАЖДЫЙ урок, и вместе с ними заново строился
    // внутренний справочник разбора по типам — именно эта повторная постройка и
    // была главной стоимостью, а не сам объект настроек. Один общий экземпляр
    // убирает повторную постройку; значения настроек остаются теми же.
    private static class TrainingJsonPolicy
    {
        // Единственный экземпляр, только для чтения: после создания он не меняется,
        // и ни одна точка этого файла не присваивает его свойствам новое значение.
        internal static readonly JsonSerializerOptions Options = new()
        {
            // Тот же единственный преобразователь перечислений, что стоял здесь прежде:
            // поведение разбора не меняется ни на символ.
            Converters = { new JsonStringEnumConverter() }
        };

        // Явный статический конструктор задаёт ленивую первую подготовку: справочник
        // разбора строится при первом обращении к этому типу, а не при загрузке сборки.
        static TrainingJsonPolicy() { }
    }

    // TrainOnExample reads the chosen translation and stores it in the KNN store and E26 word map.
    public void TrainOnExample(TranslationLearningExample example)
    {
        if (example.HeldOut) return;

        // Настройки берутся у общего держателя вместо создания новых на каждый урок.
        var opts = TrainingJsonPolicy.Options;
        var input  = JsonSerializer.Deserialize<ExecutorInput>(example.InputJson, opts);
        var chosen = JsonSerializer.Deserialize<ExecutorOutput>(example.ChosenOutputJson, opts);
        if (input is null || chosen is null) return;

        // The lesson is written under the pair address (topic, source) built by the same shared
        // builder the reader uses. The topic comes from the lesson input and is never guessed.
        var exactKey = TranslationExactSourceKey.Create(input.SourceText, input.TopicDomain);
        // A lesson with an empty address half or an empty translation is not a usable cell.
        if (exactKey.IsAddressable && !string.IsNullOrWhiteSpace(chosen.Text))
            _exactTopicSourceMap[exactKey] = chosen.Text;

        // Выученный перевод кладётся в память ближайших соседей под КАНОНИЧЕСКИМ ключом урока.
        // Прежде здесь стоял ключ учебной строки как есть: в режиме точной пары он несёт отрезок
        // происхождения, и такой урок образовывал свою отдельную ячейку, до которой чтение по
        // разбору текста не доходило.
        _knnStore.AddPair(
            TranslationSemanticKey.Canonicalize(example.SemanticSignature),
            input.SourceText,
            chosen.Text);

        var sourceTokens = new TranslationSemanticTokenizer().Tokenize(input.SourceText, input.SourceLang);
        _knnStore.AddPair(sourceTokens.PrefixSignature, input.SourceText, chosen.Text);
        _knnStore.AddPair(sourceTokens.StructureSignature, input.SourceText, chosen.Text);

        // Store every individual word mapping from the chosen output into the E26 word map.
        // For each mapping, also store the opposite-gender past-tense form so that masculine
        // and feminine Russian verb forms (забыл / забыла) both map to the same English target.
        if (chosen.WordMappings is not null)
        {
            foreach (var m in chosen.WordMappings)
            {
                AddWordMapping(m.SourceWord, m.Concept, m.TargetWord);

                // Store the gender-alternate past-tense form so cross-gender sentences
                // ("она ответила" trained → "он ответил" inferred) work without extra pairs.
                var wordAlt = DeriveGenderAlternateForm(NormalizeMapKey(m.SourceWord));
                if (wordAlt is not null)
                    AddWordMapping(wordAlt, m.Concept, m.TargetWord);

                var matchingTokens = sourceTokens.Tokens.Where(token =>
                    token.Concept == m.Concept &&
                    string.Equals(NormalizeMapKey(token.Word), NormalizeMapKey(m.SourceWord), StringComparison.OrdinalIgnoreCase));
                foreach (var token in matchingTokens)
                {
                    AddWordMapping(token.Lemma, m.Concept, m.TargetWord);
                    // Also store the gender alternate of the lemma form.
                    var lemmaAlt = DeriveGenderAlternateForm(NormalizeMapKey(token.Lemma));
                    if (lemmaAlt is not null)
                        AddWordMapping(lemmaAlt, m.Concept, m.TargetWord);
                }
            }
        }

        if (chosen.PhraseMappings is not null)
            foreach (var phrase in chosen.PhraseMappings)
                AddPhraseMapping(phrase.SourcePhrase, phrase.TargetPhrase);
    }

    // MeasureAccuracy evaluates held-out executor examples and returns the fraction
    // where the produced translation exactly matches the chosen translation (case-insensitive).
    public double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
    {
        var relevant = heldOut.Where(e => e.HeldOut && e.UnitId == UnitId).ToList();
        if (relevant.Count == 0) return 0.0;

        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var tokenizer = new TranslationSemanticTokenizer();
        var correct = 0;
        foreach (var ex in relevant)
        {
            var input    = JsonSerializer.Deserialize<ExecutorInput>(ex.InputJson, opts);
            var expected = JsonSerializer.Deserialize<ExecutorOutput>(ex.ChosenOutputJson, opts);
            if (input is null || expected is null) continue;

            // Re-tokenize and run Execute, then compare text output.
            var seq    = tokenizer.Tokenize(input.SourceText, input.SourceLang);
            var result = Execute(seq, input.TopicDomain);
            if (string.Equals(result.Text, expected.Text, StringComparison.OrdinalIgnoreCase))
                correct++;
        }

        return (double)correct / relevant.Count;
    }

    // ExportCheckpoint builds an ExecutorUnitCheckpoint from all current trained maps.
    // All four dictionaries are exported in deterministic sorted order.
    internal ExecutorUnitCheckpoint ExportCheckpoint()
    {
        // Export _wordMap: key is a (Lemma, Concept) tuple; sorted by lemma then concept name.
        var wordMap = _wordMap
            .OrderBy(kv => kv.Key.Lemma, StringComparer.Ordinal)
            .ThenBy(kv => kv.Key.Concept.ToString(), StringComparer.Ordinal)
            .Select(kv => new ExecutorWordMapEntryCheckpoint(kv.Key.Lemma, kv.Key.Concept, kv.Value))
            .ToList();
        // Export _surfaceWordMap: key is normalized surface form; sorted alphabetically.
        var surfaceWordMap = _surfaceWordMap
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new ExecutorStringMapEntryCheckpoint(kv.Key, kv.Value))
            .ToList();
        // Export _phraseMap: key is normalized source phrase; sorted alphabetically.
        var phraseMap = _phraseMap
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new ExecutorStringMapEntryCheckpoint(kv.Key, kv.Value))
            .ToList();
        // Export _exactTopicSourceMap: the address is the pair (topic, source); sorted by topic
        // and then by source so the written file is byte-stable for the same trained content.
        var topicSourceMap = _exactTopicSourceMap
            .OrderBy(kv => kv.Key.TopicDomain, StringComparer.Ordinal)
            .ThenBy(kv => kv.Key.Source, StringComparer.Ordinal)
            .Select(kv => new ExecutorTopicSourceMapEntryCheckpoint(
                kv.Key.TopicDomain, kv.Key.Source, kv.Value))
            .ToList();
        // Wrap the maps in the unit checkpoint record tagged with this executor's direction.
        // The legacy source-only list is written EMPTY on purpose: the file must carry exactly
        // one authoritative exact memory, never two competing sets the importer has to choose from.
        return new ExecutorUnitCheckpoint(
            _direction,
            wordMap,
            surfaceWordMap,
            phraseMap,
            ExactSourceMap: [],
            ExactSourceKeyVersion: TranslationExactSourceKey.ContextualTopicVersion,
            TopicSourceMap: topicSourceMap);
    }

    // ImportCheckpoint clears all four trained maps and restores them from the checkpoint.
    // The direction must match this executor's configured direction; caller validates before calling.
    internal void ImportCheckpoint(ExecutorUnitCheckpoint checkpoint)
    {
        // Clear all four trained maps before restoring to avoid stale entries.
        _wordMap.Clear();
        _surfaceWordMap.Clear();
        _phraseMap.Clear();
        _exactTopicSourceMap.Clear();
        // Restore the contextual word map: rebuild (Lemma, Concept) tuple keys.
        foreach (var entry in checkpoint.WordMap)
            if (ShouldAcceptStandaloneWordMapping(entry.Lemma, entry.Target))
                _wordMap[(entry.Lemma, entry.Concept)] = entry.Target;
        // Restore the surface-form word map.
        foreach (var entry in checkpoint.SurfaceWordMap)
            if (ShouldAcceptStandaloneWordMapping(entry.Key, entry.Value))
                _surfaceWordMap[entry.Key] = entry.Value;
        // Restore the phrase map.
        foreach (var entry in checkpoint.PhraseMap)
            _phraseMap[entry.Key] = entry.Value;
        // Restore the exact memory from its pair address. The caller validated this content
        // beforehand, so here each stored pair is rebuilt into the typed address and nothing is
        // re-normalized into a different cell. A legacy saved file carries no contextual entries
        // at all, and the validator already refused a legacy file whose source-only memory was
        // non-empty — its topic is unrecoverable, so it is never silently relabelled "general".
        foreach (var entry in checkpoint.TopicSourceMap ?? [])
            if (TranslationExactSourceKey.TryCreateFromStoredPair(
                    entry.TopicDomain, entry.Source, out var exactKey))
                _exactTopicSourceMap[exactKey] = entry.Target;
    }

    private bool ShouldAcceptStandaloneWordMapping(string sourceKey, string target)
    {
        var normalizedSource = NormalizeMapKey(sourceKey);
        var normalizedTarget = NormalizeMapKey(target);
        if (string.IsNullOrWhiteSpace(normalizedSource) || string.IsNullOrWhiteSpace(target))
            return false;

        if (_direction != TranslationDirection.EN_RU)
            return true;

        if (EnRuAllowedLowInformationPairs.Contains((normalizedSource, normalizedTarget)))
            return true;

        if (EnRuStandaloneFunctionSourceWords.Contains(normalizedSource))
            return false;

        if (ContainsLatinLetter(target))
            return false;

        if (!ContainsCyrillic(target) && !IsSafeNumericOrPunctuationTarget(target))
            return false;

        var isSingleTargetToken = normalizedTarget.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 1;
        if (isSingleTargetToken && EnRuLowInformationTargets.Contains(normalizedTarget))
            return false;

        return true;
    }

    private static readonly HashSet<string> EnRuLowInformationTargets = new(StringComparer.OrdinalIgnoreCase)
    {
        "не",
        "от",
        "на",
        "в",
        "к",
        "и",
        "он",
        "она",
        "они",
        "это",
        "том",
        "пожалуйста",
        "люди",
        "дверь",
        "пойду",
        "спросить"
    };

    private static readonly HashSet<(string Source, string Target)> EnRuAllowedLowInformationPairs = new()
    {
        ("cant", "не"),
        ("not", "не"),
        ("no", "не"),
        ("cannot", "не"),
        ("tom", "том"),
        ("door", "дверь")
    };

    private static readonly HashSet<string> EnRuStandaloneFunctionSourceWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a",
        "about",
        "after",
        "all",
        "am",
        "an",
        "and",
        "any",
        "are",
        "around",
        "as",
        "at",
        "be",
        "because",
        "been",
        "before",
        "being",
        "between",
        "both",
        "but",
        "by",
        "can",
        "cannot",
        "cant",
        "could",
        "did",
        "do",
        "does",
        "doing",
        "done",
        "dont",
        "during",
        "each",
        "either",
        "every",
        "few",
        "for",
        "from",
        "had",
        "has",
        "have",
        "having",
        "he",
        "her",
        "here",
        "hers",
        "him",
        "his",
        "how",
        "i",
        "if",
        "in",
        "into",
        "is",
        "it",
        "its",
        "least",
        "less",
        "many",
        "may",
        "me",
        "might",
        "mine",
        "more",
        "most",
        "much",
        "must",
        "my",
        "neither",
        "no",
        "not",
        "of",
        "on",
        "only",
        "onto",
        "or",
        "ought",
        "our",
        "ours",
        "over",
        "own",
        "same",
        "shall",
        "she",
        "should",
        "some",
        "such",
        "than",
        "that",
        "the",
        "their",
        "theirs",
        "them",
        "then",
        "there",
        "these",
        "they",
        "this",
        "those",
        "through",
        "to",
        "under",
        "us",
        "was",
        "we",
        "were",
        "what",
        "when",
        "where",
        "which",
        "while",
        "who",
        "whole",
        "whom",
        "whose",
        "why",
        "will",
        "with",
        "without",
        "would",
        "you",
        "your",
        "yours"
    };

    private static bool IsSafeNumericOrPunctuationTarget(string target)
        => !string.IsNullOrWhiteSpace(target) &&
           target.Any(char.IsDigit) &&
           target.All(ch => char.IsDigit(ch) || char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch));

    // ApplyE27Reorder swaps TECH_TERM (null) and VALIDATE_CHECK (check) when adjacent
    // to produce the correct English compound order "null check" instead of "check null".
    private static void ApplyE27Reorder(List<string> parts, IReadOnlyList<SemanticToken> tokens)
    {
        // Scan for adjacent VALIDATE_CHECK + TECH_TERM and swap them.
        for (var i = 0; i + 1 < tokens.Count && i + 1 < parts.Count; i++)
        {
            var a = tokens[i].Concept;
            var b = tokens[i + 1].Concept;
            // VALIDATE_CHECK followed by NULL_LITERAL → swap to "null check" (English ADJ+NOUN order).
            if (a == SemanticConcept.VALIDATE_CHECK && b == SemanticConcept.NULL_LITERAL)
                (parts[i], parts[i + 1]) = (parts[i + 1], parts[i]);
        }
    }

    private void AddWordMapping(string sourceWord, SemanticConcept concept, string targetWord)
    {
        var key = NormalizeMapKey(sourceWord);
        if (!string.IsNullOrWhiteSpace(key) &&
            !string.IsNullOrWhiteSpace(targetWord) &&
            ShouldAcceptStandaloneWordMapping(key, targetWord))
        {
            _wordMap[(key, concept)] = targetWord;
            _surfaceWordMap[key] = targetWord;
        }
    }

    private void AddPhraseMapping(string sourcePhrase, string targetPhrase)
    {
        var key = NormalizeMapKey(sourcePhrase);
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(targetPhrase))
            _phraseMap[key] = targetPhrase.Trim();
    }

    // DeriveGenderAlternateForm returns the opposite-gender Russian past-tense verb form
    // when derivable from the regular -л / -ла alternation pattern.
    //
    // Regular pattern:
    //   Masculine past ends in -л  (забыл, ответил, написал).
    //   Feminine  past ends in -ла (забыла, ответила, написала).
    //
    // When training on one form, storing the alternate ensures that "она ответила → she answered"
    // automatically teaches that "он ответил" also maps to "answered". This is the core
    // subject-gender agreement transfer signal for RU→EN verb forms.
    // Only applied to non-ASCII strings (Cyrillic -л / -ла; never touches English words).
    private static string? DeriveGenderAlternateForm(string normalizedWord)
    {
        // Too short to carry a meaningful Cyrillic ending.
        if (string.IsNullOrWhiteSpace(normalizedWord) || normalizedWord.Length < 3)
            return null;

        // Skip plain ASCII/Latin words — Cyrillic 'л' (U+043B) is above 'z' (U+007A).
        if (!normalizedWord.Any(ch => ch > 'z'))
            return null;

        // Feminine past -ла → masculine -л: remove the trailing Cyrillic 'а' (забыла → забыл).
        if (normalizedWord.EndsWith("ла", StringComparison.Ordinal) && normalizedWord.Length > 3)
            return normalizedWord[..^1];

        // Masculine past -л → feminine -ла: append Cyrillic 'а' (забыл → забыла).
        // Guard: must not already end in -ла (handled above).
        if (normalizedWord.EndsWith("л", StringComparison.Ordinal) &&
            !normalizedWord.EndsWith("ла", StringComparison.Ordinal) &&
            normalizedWord.Length > 2)
            return normalizedWord + "а";

        return null;
    }

    private bool TryComposeFromPhraseMappings(SemanticTokenSequence tokens, out string translation)
    {
        translation = string.Empty;
        if (_phraseMap.Count == 0 || tokens.Tokens.Count == 0)
            return false;

        var sourceTokens = tokens.Tokens
            .Select(token => NormalizeMapKey(token.Word))
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();
        if (sourceTokens.Length == 0)
            return false;

        // The learned phrases are read once into a shape both assemblers below can use: the address
        // the phrase is stored under, that address cut into words, and the learned translation.
        var phraseEntries = _phraseMap
            .Select(kv => (
                SourceKey: kv.Key,
                SourceTokens: kv.Key.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                Target: kv.Value))
            .Where(entry => entry.SourceTokens.Length > 0 && !string.IsNullOrWhiteSpace(entry.Target))
            .OrderByDescending(entry => entry.SourceTokens.Length)
            .ToArray();

        // The translated pieces of the answer, and the three counters the thresholds below read.
        var parts = new List<string>();
        var matchedTokens = 0;
        var matchedPhraseSpans = 0;
        var matchedPhraseTokens = 0;

        if (IsPhraseCoveragePlannerScope(tokens, sourceTokens))
        {
            // Inside the admitted area the split is chosen as a whole instead of word by word, so a
            // long learned phrase can no longer swallow a source word that a different split would
            // have translated. This is the defect of experiment 54: on the lessons "close user",
            // "user session" and "close" the exam "close user session" took "close user" first,
            // lost "session", stayed at two covered words of three and the branch then refused.
            // Планировщику передаётся ДЕЙСТВУЮЩЕЕ правило допуска этой ветви как ограничение
            // поиска, а не только слова и выученные фразы. Прежде он выбирал разрез по своему
            // правилу («больше покрытых слов, при равенстве меньше фраз»), а допуск спрашивался
            // уже после — и отвергал выбранное, хотя годный разрез существовал и был отброшен
            // внутри поиска. Второе правило выбора прямо противоположно второму пути допуска:
            // поиск сокращал число фраз ровно тогда, когда допуску нужно иметь их две.
            //
            // Числа порога сюда не копируются: предикат ниже зовёт тот же единственный метод
            // правила допуска, который применяется и к готовому ответу этой ветви.
            var plan = TranslationPhraseCompositionPlanner.PlanAdmissible(
                sourceTokens,
                phraseEntries
                    .Select(entry => new TranslationPhraseEdge(
                        entry.SourceKey, entry.SourceTokens, entry.Target))
                    .ToArray(),
                // Внутри этой области ни у одного слова нет принятого однословного перевода,
                // поэтому покрытые слова и покрытые фразами слова — одно и то же число, и оно
                // передаётся в правило дважды ровно по этой причине.
                candidate => IsPhraseCompositionAdmissible(
                    candidate.CoveredSourceTokens,
                    candidate.CoveredSourceTokens,
                    candidate.PhraseSpanCount,
                    sourceTokens.Length,
                    candidate.Parts.Count));
            // The chosen translated pieces become the answer in the order the planner returned them.
            parts.AddRange(plan.Parts);
            // Inside this area no source word has an accepted single-word translation, so every
            // covered word was covered by a learned phrase and both counters read the same number.
            matchedTokens = plan.CoveredSourceTokens;
            matchedPhraseTokens = plan.CoveredSourceTokens;
            // How many separate learned phrases the chosen answer is built from.
            matchedPhraseSpans = plan.PhraseSpanCount;
        }
        else
        {
            // Outside the admitted area the earlier assembler stays exactly as it was: it walks the
            // source words from left to right, at each position takes the longest learned phrase
            // that starts there, and otherwise falls back to a learned single-word translation.
            var i = 0;
            while (i < sourceTokens.Length)
            {
                var phrase = phraseEntries.FirstOrDefault(entry => MatchesAt(sourceTokens, i, entry.SourceTokens));
                if (phrase.SourceTokens is { Length: > 0 })
                {
                    parts.Add(phrase.Target);
                    matchedTokens += phrase.SourceTokens.Length;
                    matchedPhraseSpans++;
                    matchedPhraseTokens += phrase.SourceTokens.Length;
                    i += phrase.SourceTokens.Length;
                    continue;
                }

                var token = tokens.Tokens[Math.Min(i, tokens.Tokens.Count - 1)];
                if (TryGetAcceptedWordMapping(token, out var mapped))
                {
                    parts.Add(mapped);
                    matchedTokens++;
                }

                i++;
            }
        }

        // Правило допуска этой ветви спрашивается ровно одним методом — тем же, который получает
        // как ограничение поиска планировщик выше. Прежде формула стояла здесь единственным
        // выражением; теперь у неё один владелец, и два места не могут разойтись в числах.
        if (!IsPhraseCompositionAdmissible(
                matchedTokens, matchedPhraseTokens, matchedPhraseSpans, sourceTokens.Length, parts.Count))
            return false;

        translation = NormalizeComposedTranslation(string.Join(" ", parts));
        if (!IsPhraseMapTargetCompatibleForDirection(tokens, translation))
            return false;

        return !string.IsNullOrWhiteSpace(translation);
    }

    // IsPhraseCompositionAdmissible — ЕДИНСТВЕННЫЙ владелец правила допуска фразовой сборки.
    //
    // Правило не изменилось ни на символ: ответ годится, если покрыто не меньше 85 процентов слов
    // источника, ЛИБО ответ собран не меньше чем из двух фраз и фразами покрыто не меньше 50
    // процентов; ответ без фраз или без частей не годится никогда. Прежде это выражение стояло
    // единственным условием внутри сборки, и больше его спросить было негде.
    //
    // Почему оно стало отдельным методом. Планировщик теперь получает это правило как ограничение
    // поиска, иначе он отбрасывает годный разрез раньше, чем правило его увидит. Если бы правило
    // пришлось повторить в месте вызова, числа порога оказались бы в двух местах и со временем
    // разошлись бы. Теперь оба места — предикат поиска и проверка готового ответа — зовут этот
    // метод, и порог живёт в нём одном.
    //
    // Почему покрытых слов ДВА числа. Вне области планировщика прежний сборщик умеет добавить
    // однословный перевод: такое слово увеличивает общее покрытие, но не покрытие фразами. Внутри
    // области планировщика однословных переводов нет по определению самой области, и оба числа
    // равны. Правило читает именно те два числа, что читало раньше.
    private static bool IsPhraseCompositionAdmissible(
        int matchedTokens,
        int matchedPhraseTokens,
        int matchedPhraseSpans,
        int sourceTokenCount,
        int partsCount)
    {
        // Без слов источника доли не определены, и годного ответа быть не может.
        if (sourceTokenCount <= 0)
            return false;

        // Ответ, в котором нет ни одной выученной фразы, не является фразовой сборкой.
        if (matchedPhraseSpans == 0)
            return false;

        // Пустой набор частей означает, что сказать нечего.
        if (partsCount == 0)
            return false;

        // Доля покрытых слов источника — то же выражение, что стояло в прежнем условии.
        var coverage = matchedTokens / (double)sourceTokenCount;
        // Второй путь допуска: не меньше двух фраз и фразами покрыто не меньше половины слов.
        var repeatedPhraseCoverage = matchedPhraseSpans >= 2 &&
            matchedPhraseTokens / (double)sourceTokenCount >= 0.50;

        // Годится по любому из двух путей: повторная фраза при половине либо покрытие от 85 процентов.
        return repeatedPhraseCoverage || coverage >= 0.85;
    }

    // IsPhraseCoveragePlannerScope answers whether this request belongs to the narrow area where
    // the whole split is chosen by the planner instead of word by word from left to right.
    //
    // Three conditions, all required, and all of them are limits of what was actually agreed:
    //   * the direction is English into Russian, the only direction the full choice was prepared and
    //     measured for;
    //   * the sentence is short enough that the agreed cost of the full choice still applies;
    //   * not one source word has an accepted learned single-word translation. The planner chooses
    //     between learned phrases only; a request that also has single-word translations keeps the
    //     earlier assembler, whose own single-word fallback is a separate mechanism with its own
    //     known position defect, and that defect is not touched here.
    //
    // Outside this area nothing changes: the earlier assembler, the admission thresholds, the
    // target checks and the priority of the exact translation memory all stay as they were.
    private bool IsPhraseCoveragePlannerScope(
        SemanticTokenSequence tokens,
        IReadOnlyList<string> normalizedSourceTokens)
    {
        // Only English into Russian was prepared for the full choice; every other direction keeps
        // the earlier assembler untouched.
        if (_direction != TranslationDirection.EN_RU)
            return false;

        // A sentence longer than the agreed length keeps the earlier assembler, because the cost of
        // the full choice was only measured up to that length.
        if (normalizedSourceTokens.Count > TranslationPhraseCompositionPlanner.MaxPlannedSourceTokens)
            return false;

        // One source word with an accepted learned single-word translation sends the whole request
        // back to the earlier assembler, which is the only one that can use such a translation.
        foreach (var token in tokens.Tokens)
            if (TryGetAcceptedWordMapping(token, out _))
                return false;

        // All three conditions hold, so this request is inside the admitted area of the planner.
        return true;
    }

    private bool TryGetAcceptedWordMapping(SemanticToken token, out string mapped)
    {
        var lemmaKey = NormalizeMapKey(token.Lemma);
        var wordKey = NormalizeMapKey(token.Word);

        if (TryGetAcceptedContextWordMapping(lemmaKey, token.Concept, out mapped) ||
            TryGetAcceptedContextWordMapping(wordKey, token.Concept, out mapped) ||
            TryGetAcceptedSurfaceWordMapping(lemmaKey, out mapped) ||
            TryGetAcceptedSurfaceWordMapping(wordKey, out mapped))
            return true;

        mapped = string.Empty;
        return false;
    }

    private bool TryGetAcceptedContextWordMapping(
        string sourceKey,
        SemanticConcept concept,
        out string mapped)
    {
        mapped = string.Empty;
        if (string.IsNullOrWhiteSpace(sourceKey) ||
            !_wordMap.TryGetValue((sourceKey, concept), out var candidate) ||
            !ShouldAcceptStandaloneWordMapping(sourceKey, candidate))
            return false;

        mapped = candidate;
        return true;
    }

    private bool TryGetAcceptedSurfaceWordMapping(string sourceKey, out string mapped)
    {
        mapped = string.Empty;
        if (string.IsNullOrWhiteSpace(sourceKey) ||
            !_surfaceWordMap.TryGetValue(sourceKey, out var candidate) ||
            !ShouldAcceptStandaloneWordMapping(sourceKey, candidate))
            return false;

        mapped = candidate;
        return true;
    }

    private static bool MatchesAt(IReadOnlyList<string> sourceTokens, int start, IReadOnlyList<string> phraseTokens)
    {
        if (start + phraseTokens.Count > sourceTokens.Count)
            return false;

        for (var j = 0; j < phraseTokens.Count; j++)
            if (!string.Equals(sourceTokens[start + j], phraseTokens[j], StringComparison.OrdinalIgnoreCase))
                return false;

        return true;
    }

    private static string NormalizeComposedTranslation(string value)
        => string.Join(" ", (value ?? string.Empty)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .Replace(" !", "!", StringComparison.Ordinal)
            .Replace(" .", ".", StringComparison.Ordinal)
            .Replace(" ,", ",", StringComparison.Ordinal);

    private static bool ContainsCyrillic(string value)
        => (value ?? string.Empty).Any(ch => ch is >= '\u0400' and <= '\u04FF');

    private bool IsTargetCompatibleForDirection(SemanticTokenSequence sourceTokens, string translation)
    {
        if (string.IsNullOrWhiteSpace(translation))
            return false;

        return _direction switch
        {
            TranslationDirection.RU_EN =>
                !ContainsCyrillic(translation) &&
                !LooksLikeCopiedSource(sourceTokens, translation),
            TranslationDirection.EN_RU =>
                ContainsCyrillic(translation) &&
                !ContainsLatinLetter(translation) &&
                !LooksLikeCopiedSource(sourceTokens, translation),
            _ => !ContainsCyrillic(translation)
        };
    }

    private bool IsPhraseMapTargetCompatibleForDirection(SemanticTokenSequence sourceTokens, string translation)
    {
        if (string.IsNullOrWhiteSpace(translation))
            return false;

        return _direction switch
        {
            TranslationDirection.RU_EN =>
                !ContainsCyrillic(translation) &&
                !LooksLikeCopiedSource(sourceTokens, translation),
            TranslationDirection.EN_RU =>
                ContainsCyrillic(translation) &&
                !LooksLikeCopiedSource(sourceTokens, translation),
            _ => !ContainsCyrillic(translation)
        };
    }

    private static bool ContainsLatinLetter(string value)
        => (value ?? string.Empty).Any(ch => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z');

    private static double SourceTokenSimilarity(string querySource, string candidateSource)
    {
        var queryTokens = SourceContentTokens(querySource);
        var candidateTokens = SourceContentTokens(candidateSource);
        if (queryTokens.Count == 0 || candidateTokens.Count == 0)
            return 0.0;

        var overlap = queryTokens.Intersect(candidateTokens, StringComparer.OrdinalIgnoreCase).Count();
        if (overlap < EnRuOrdinaryKnnMinimumSharedSourceTokens)
            return 0.0;

        return overlap / (double)queryTokens.Count;
    }

    private static HashSet<string> SourceContentTokens(string sourceText)
        => NormalizeMapKey(sourceText)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length >= 2)
            .Where(token => !EnRuOrdinaryKnnLowInformationSourceWords.Contains(token))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> EnRuOrdinaryKnnLowInformationSourceWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
        "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
        "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those",
        "source", "sentence", "fresh", "legacy", "known", "novel", "starter"
    };

    private static bool LooksLikeCopiedSource(SemanticTokenSequence sourceTokens, string translation)
    {
        var sourceWords = sourceTokens.Tokens
            .Where(token => token.Pos != TranslationPos.CODE_IDENTIFIER)
            .Select(token => NormalizeMapKey(token.Word))
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var targetWords = NormalizeMapKey(translation)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (sourceWords.Count == 0 || targetWords.Count == 0)
            return false;

        var overlap = sourceWords.Intersect(targetWords, StringComparer.OrdinalIgnoreCase).Count();
        return overlap / (double)Math.Max(sourceWords.Count, targetWords.Count) >= 0.80;
    }

    // NormalizeMapKey is the product normalizer of a dictionary address: trimmed, lower-cased
    // and stripped of punctuation. It is visible inside this assembly because the shared
    // exact-memory address builder must normalize the source half with exactly this rule and
    // never with a second copy of it that could drift away from the one used here.
    internal static string NormalizeMapKey(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (LooksLikeUtf8Mojibake(trimmed))
            return string.Join(" ", trimmed
                .ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Trim('\'', '"', '`', '.', ',', '!', '?', ';', ':', '(', ')', '[', ']'))
                .Where(token => !string.IsNullOrWhiteSpace(token)));

        return string.Join(" ", new string(trimmed
                .ToLowerInvariant()
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$' || char.IsWhiteSpace(ch))
                .ToArray())
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool LooksLikeUtf8Mojibake(string value)
        => (value ?? string.Empty).Contains("Ð", StringComparison.Ordinal) ||
           (value ?? string.Empty).Contains("Ñ", StringComparison.Ordinal) ||
           (value ?? string.Empty).Contains("Ã", StringComparison.Ordinal);
}

// ── QualityVerifierUnit ────────────────────────────────────────────────────────

// QualityVerifierUnit checks whether a candidate translation satisfies learned concept requirements.
// Training: extracts concept-to-required-word rules from FAIL examples (chosen output).
// Inference: checks candidate text for required words given the source's semantic concepts.
// UnitId: "quality-verifier"
public sealed class QualityVerifierUnit : ITranslationUnitTrainer
{
    // UnitId is the routing key.
    public string UnitId => "quality-verifier";

    // _conceptRules maps (source_concept, context_concept) → required word in the target.
    // A rule "THREAD_REF in LAUNCH_OP context → 'thread'" means any candidate that lacks
    // the word "thread" when the source contains THREAD_REF + LAUNCH_OP is marked as failing.
    private readonly Dictionary<(SemanticConcept Src, SemanticConcept Ctx), string> _conceptRules = new();
    private readonly Dictionary<string, List<string>> _requiredTermsBySignature
        = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> _requiredTermsBySourceMarker
        = new(StringComparer.OrdinalIgnoreCase);

    // Verify checks the candidate translation against the source's semantic token sequence.
    public VerificationResult Verify(string candidateText, SemanticTokenSequence sourceTokens)
    {
        var issues = new List<string>();

        // Требование ищется по КАНОНИЧЕСКОМУ ключу разбора — ровно по тому, под которым обучение
        // его и записало. Прежде ключ записи нёс отрезок происхождения, а ключ чтения — нет,
        // поэтому требование не находилось никогда и плохой перевод проходил проверку.
        var runtimeKey = TranslationSemanticKey.Canonicalize(sourceTokens.Signature);
        if (_requiredTermsBySignature.TryGetValue(runtimeKey, out var requiredTerms))
            foreach (var required in requiredTerms)
                // The learned term must be present as whole words. The previous bare substring
                // test let the candidate "already" stand in for the required word "ready", so a
                // wrong translation was reported as passing verification.
                if (!ContainsRequiredTermAsWholeWords(candidateText, required))
                    // В претензии называется тот же канонический ключ, по которому шёл поиск.
                    issues.Add($"expected '{required}' for signature {runtimeKey}");

        var sourceMarker = ExtractSourceMarker(sourceTokens);
        if (!string.IsNullOrWhiteSpace(sourceMarker) &&
            _requiredTermsBySourceMarker.TryGetValue(sourceMarker, out var markerRequiredTerms))
        {
            var hasAnyAcceptableMarker = markerRequiredTerms.Any(required =>
                candidateText.Contains(required, StringComparison.OrdinalIgnoreCase));
            if (!hasAnyAcceptableMarker)
                issues.Add($"expected one of '{string.Join("|", markerRequiredTerms.Distinct(StringComparer.OrdinalIgnoreCase))}' for source marker {sourceMarker}");
        }

        // Check each learned concept rule against the candidate text.
        foreach (var ((src, ctx), required) in _conceptRules)
        {
            // If the source contains a token with this concept...
            if (!sourceTokens.Tokens.Any(t => t.Concept == src)) continue;

            // ...and the candidate text does NOT contain the required target word...
            if (!candidateText.Contains(required, StringComparison.OrdinalIgnoreCase))
                // Record the issue with a structured format parseable by future training.
                issues.Add($"expected '{required}' for concept {src} in context {ctx}");
        }

        // Score = 1.0 minus 0.15 per issue; minimum 0.0.
        var score = Math.Max(0.0, 1.0 - issues.Count * 0.15);
        return new VerificationResult(
            Score: score,
            Issues: issues,
            Pass: issues.Count == 0,
            CorrectionHint: issues.Count > 0
                ? $"Review: {string.Join("; ", issues)}"
                : null);
    }

    // TrainOnExample reads a FAIL verification example and extracts concept-to-word rules.
    public void TrainOnExample(TranslationLearningExample example)
    {
        if (example.HeldOut) return;

        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var input  = JsonSerializer.Deserialize<VerifierInput>(example.InputJson, opts);
        var chosen = JsonSerializer.Deserialize<VerifierOutput>(example.ChosenOutputJson, opts);
        if (input is null || chosen is null) return;

        // Ключ записи требований — КАНОНИЧЕСКИЙ ключ урока, вычисленный один раз на строку.
        // Это та же величина, по которой выше идёт чтение, поэтому адрес записи и адрес поиска
        // больше не могут разойтись.
        var lessonKey = TranslationSemanticKey.Canonicalize(example.SemanticSignature);

        // FAIL examples carry explicit issues that name the concept and required word.
        if (!chosen.Pass && chosen.Issues is not null)
            foreach (var issue in chosen.Issues)
                if (TryParseIssue(issue, out var src, out var ctx, out var req))
                {
                    if (!_requiredTermsBySignature.TryGetValue(lessonKey, out var requiredTerms))
                    {
                        requiredTerms = [];
                        _requiredTermsBySignature[lessonKey] = requiredTerms;
                    }

                    requiredTerms.Add(req);

                    if (ctx != SemanticConcept.UNKNOWN && src != SemanticConcept.CODE_IDENTIFIER)
                        _conceptRules[(src, ctx)] = req;
                }
                else if (TryParseRequiredTerm(issue, out req))
                {
                    var qualityMarkerIssue = issue.Contains("translated quality marker", StringComparison.OrdinalIgnoreCase);
                    // Наличие известного смыслового понятия проверяется по каноническому ключу:
                    // отрезок происхождения понятием не является и в этом решении не участвует.
                    if (!qualityMarkerIssue && SignatureHasSpecificConcept(lessonKey))
                    {
                        if (!_requiredTermsBySignature.TryGetValue(lessonKey, out var requiredTerms))
                        {
                            requiredTerms = [];
                            _requiredTermsBySignature[lessonKey] = requiredTerms;
                        }

                        requiredTerms.Add(req);
                    }

                    var marker = ExtractSourceMarker(input.SourceText);
                    if (!string.IsNullOrWhiteSpace(marker))
                    {
                        if (!_requiredTermsBySourceMarker.TryGetValue(marker, out var markerRequiredTerms))
                        {
                            markerRequiredTerms = [];
                            _requiredTermsBySourceMarker[marker] = markerRequiredTerms;
                        }

                        markerRequiredTerms.Add(req);
                    }
                }
    }

    // MeasureAccuracy evaluates held-out verifier examples and returns the fraction
    // where the produced Pass/Fail verdict matches the expected verdict.
    public double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
    {
        var relevant = heldOut.Where(e => e.HeldOut && e.UnitId == UnitId).ToList();
        if (relevant.Count == 0) return 0.0;

        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var tokenizer = new TranslationSemanticTokenizer();
        var correct = 0;
        foreach (var ex in relevant)
        {
            var input    = JsonSerializer.Deserialize<VerifierInput>(ex.InputJson, opts);
            var expected = JsonSerializer.Deserialize<VerifierOutput>(ex.ChosenOutputJson, opts);
            if (input is null || expected is null) continue;

            // Re-tokenize the source and run Verify against the candidate.
            var seq    = tokenizer.Tokenize(input.SourceText, "ru");
            var result = Verify(input.CandidateText, seq);
            if (result.Pass == expected.Pass) correct++;
        }

        return (double)correct / relevant.Count;
    }

    // ExportCheckpoint builds a QualityVerifierUnitCheckpoint from all current trained rules.
    // All three dictionaries are exported in deterministic sorted order.
    internal QualityVerifierUnitCheckpoint ExportCheckpoint()
    {
        // Export _conceptRules: key is a (Src, Ctx) concept tuple; sorted by concept name strings.
        var conceptRules = _conceptRules
            .OrderBy(kv => kv.Key.Src.ToString(), StringComparer.Ordinal)
            .ThenBy(kv => kv.Key.Ctx.ToString(), StringComparer.Ordinal)
            .Select(kv => new QualityVerifierConceptRuleCheckpoint(kv.Key.Src, kv.Key.Ctx, kv.Value))
            .ToList();
        // Export _requiredTermsBySignature: one entry per structural signature with its term list.
        var requiredBySignature = _requiredTermsBySignature
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => new QualityVerifierStringListCheckpoint(kv.Key, kv.Value.ToList()))
            .ToList();
        // Export _requiredTermsBySourceMarker: one entry per source marker with its term list.
        var requiredByMarker = _requiredTermsBySourceMarker
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new QualityVerifierStringListCheckpoint(kv.Key, kv.Value.ToList()))
            .ToList();
        // Wrap all three rule sets in the unit checkpoint record.
        return new QualityVerifierUnitCheckpoint(conceptRules, requiredBySignature, requiredByMarker);
    }

    // ImportCheckpoint clears all three rule dictionaries and restores them from the checkpoint.
    // Lists are restored exactly; no deduplication is applied because training pairs are authoritative.
    internal void ImportCheckpoint(QualityVerifierUnitCheckpoint checkpoint)
    {
        // Clear all three rule dictionaries before restoring to avoid stale rules.
        _conceptRules.Clear();
        _requiredTermsBySignature.Clear();
        _requiredTermsBySourceMarker.Clear();
        // Restore concept-pair rules by rebuilding (Src, Ctx) tuple keys.
        foreach (var rule in checkpoint.ConceptRules)
            _conceptRules[(rule.SourceConcept, rule.ContextConcept)] = rule.Required;
        // Требования по ключу восстанавливаются под КАНОНИЧЕСКИМ ключом: это перенос старой
        // сохранённой памяти проверяющего к той форме ключа, по которой идёт поиск. Списки
        // требований двух строк, сошедшихся в один ключ, объединяются, а не затирают друг друга:
        // требование — это добавочное условие, и потерять одно из них нельзя.
        foreach (var entry in checkpoint.RequiredTermsBySignature)
        {
            // Ключ прежней формы приводится к канонической форме один раз на запись.
            var restoredKey = TranslationSemanticKey.Canonicalize(entry.Key);
            // Если под этим ключом список уже есть, новые требования дописываются в его конец.
            if (_requiredTermsBySignature.TryGetValue(restoredKey, out var existingTerms))
                existingTerms.AddRange(entry.Values);
            else
                _requiredTermsBySignature[restoredKey] = entry.Values.ToList();
        }
        // Restore per-source-marker required-term lists.
        foreach (var entry in checkpoint.RequiredTermsBySourceMarker)
            _requiredTermsBySourceMarker[entry.Key] = entry.Values.ToList();
    }

    // ContainsRequiredTermAsWholeWords answers whether the candidate translation carries the
    // learned required term as a run of whole words instead of as a bare substring.
    // A substring test cannot tell "ready" from "already", "unready", or "readyState", so it
    // reported a wrong candidate as satisfying the learned requirement. It also accepted an
    // empty required term, because an empty substring is contained in every string. Both sides
    // are cut into maximal runs of letters and digits, which also makes the underscore a word
    // boundary, and the required run has to appear as adjacent whole words.
    //
    // Scope of this repair is exactly one factor: only this signature branch changed. The
    // source-marker branch and the concept-rule branch above still use their own substring
    // test; they are separate owners with their own planned transfers.
    private static bool ContainsRequiredTermAsWholeWords(string candidateText, string required)
    {
        // Cut the learned requirement into the whole words it is actually made of.
        var requiredWords = SplitIntoWholeWords(required);
        // An empty requirement carries no evidence, so it can never be reported as satisfied.
        if (requiredWords.Count == 0) return false;

        // Cut the candidate translation into whole words by the same rule.
        var candidateWords = SplitIntoWholeWords(candidateText);
        // A candidate with fewer words than the requirement cannot contain the whole run.
        if (candidateWords.Count < requiredWords.Count) return false;

        // Slide the required run across the candidate words looking for an adjacent match.
        for (var start = 0; start + requiredWords.Count <= candidateWords.Count; start++)
        {
            // Treat this offset as matching until one word position disagrees.
            var matches = true;
            // Compare every required word against the candidate word at the same offset.
            for (var offset = 0; offset < requiredWords.Count; offset++)
                // Letter case is ignored so an upper-case "READY" still satisfies "ready".
                if (!string.Equals(
                        candidateWords[start + offset],
                        requiredWords[offset],
                        StringComparison.OrdinalIgnoreCase))
                {
                    // This offset is not a match: stop comparing it and try the next one.
                    matches = false;
                    break;
                }

            // An offset where every required word lined up proves the requirement is present.
            if (matches) return true;
        }

        // No offset carried the whole required run, so the requirement is not satisfied.
        return false;
    }

    // SplitIntoWholeWords cuts a string into maximal runs of letters and digits, so every other
    // character — space, punctuation, or underscore — ends the word that was being read.
    private static List<string> SplitIntoWholeWords(string value)
    {
        // Collect the finished words of this string in their original order.
        var words = new List<string>();
        // Accumulate the letters and digits of the word currently being read.
        var current = new System.Text.StringBuilder();
        // Walk the string one character at a time so word boundaries are found exactly.
        foreach (var ch in value ?? string.Empty)
        {
            // A letter or digit belongs to the word being accumulated.
            if (char.IsLetterOrDigit(ch)) { current.Append(ch); continue; }

            // Any other character closes the current word when one was being accumulated.
            if (current.Length > 0)
            {
                words.Add(current.ToString());
                current.Clear();
            }
        }

        // A string that ends inside a word still has that final word to add.
        if (current.Length > 0) words.Add(current.ToString());
        // Return the whole-word sequence so adjacency can be compared position by position.
        return words;
    }

    // TryParseIssue parses "expected '{word}' for concept {src} in context {ctx}".
    private static bool TryParseIssue(
        string issue,
        out SemanticConcept src,
        out SemanticConcept ctx,
        out string required)
    {
        src = SemanticConcept.UNKNOWN;
        ctx = SemanticConcept.UNKNOWN;
        required = string.Empty;

        // Split on single-quote to extract the required word.
        var parts = issue.Split('\'');
        if (parts.Length < 3) return false;
        required = parts[1];

        // Scan for concept names in the remainder of the issue string.
        var rest = parts[2];
        foreach (var name in Enum.GetNames<SemanticConcept>())
        {
            if (!rest.Contains(name, StringComparison.OrdinalIgnoreCase)) continue;
            if (src == SemanticConcept.UNKNOWN)
                src = Enum.Parse<SemanticConcept>(name);
            else
            {
                ctx = Enum.Parse<SemanticConcept>(name);
                break;
            }
        }

        return src != SemanticConcept.UNKNOWN && !string.IsNullOrEmpty(required);
    }

    private static bool TryParseRequiredTerm(string issue, out string required)
    {
        required = string.Empty;
        var parts = issue.Split('\'');
        if (parts.Length < 3) return false;

        required = parts[1];
        return !string.IsNullOrWhiteSpace(required);
    }

    private static bool SignatureHasSpecificConcept(string signature)
        => (signature ?? string.Empty).Split('|')
            .Skip(1)
            .Any(component => Enum.GetNames<SemanticConcept>()
                .Any(name => string.Equals(component, name, StringComparison.Ordinal)));

    private static string ExtractSourceMarker(string sourceText)
        => ExtractSourceMarker(new TranslationSemanticTokenizer().Tokenize(sourceText, "ru"));

    private static string ExtractSourceMarker(SemanticTokenSequence sourceTokens)
    {
        var token = sourceTokens.Tokens.FirstOrDefault(t => t.Pos == TranslationPos.ADV)
            ?? sourceTokens.Tokens.FirstOrDefault(t =>
                t.Pos is not TranslationPos.PREP
                    and not TranslationPos.PRON
                    and not TranslationPos.CODE_IDENTIFIER
                    and not TranslationPos.VERB &&
                t.Concept == SemanticConcept.UNKNOWN &&
                (sourceTokens.RuleTags.Count == 0 || LooksLikeMannerMarker(t.Word) || LooksLikeMannerMarker(t.Lemma)));

        return NormalizeMarker(token?.Lemma ?? token?.Word ?? string.Empty);
    }

    private static string NormalizeMarker(string value)
        => string.Concat((value ?? string.Empty)
            .Trim()
            .Trim('\'', '"', '`', '.', ',', '!', '?', ';', ':', '«', '»', '“', '”', '(', ')', '[', ']')
            .ToLowerInvariant()
            .Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$'));

    private static bool LooksLikeMannerMarker(string value)
    {
        var marker = NormalizeMarker(value);
        return marker.Length > 2 &&
            (marker.EndsWith("Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("Ð½Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÐºÐ¾", StringComparison.Ordinal) ||
             marker.EndsWith("Ð»Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÃÂ¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÃÂ½ÃÂ¾", StringComparison.Ordinal) ||
             marker.EndsWith("ly", StringComparison.OrdinalIgnoreCase));
    }
}

// TranslationCandidate is the output of ExecutorUnit: a translated text with confidence.
public sealed record TranslationCandidate(
    // Text is the assembled translation.
    string Text,
    // Confidence is the executor's estimated quality score [0.0–1.0].
    double Confidence,
    // AdapterId identifies which path produced this candidate.
    string AdapterId);
