// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using System.Diagnostics;
using Tokenizer;
using VectorSearch;

// This record is one supervised example used to teach a KNN-backed logical unit.
public sealed record LogicalKnnTrainingExample<TInput, TOutput>(
    string ExampleId,
    TInput Input,
    TOutput Output,
    string[] PredicateIds,
    string[] DomainTags,
    string[] EvidenceIds);

// This record describes the local learned model created for one logical unit.
public sealed record LogicalKnnModelManifest(
    string UnitId,
    string UnitRole,
    string DomainTag,
    int TrainingExampleCount,
    int VectorDimensions,
    int K,
    double LessonRequestConfidenceThreshold,
    double TrainingTimeMs,
    long ManagedAllocationBytes,
    string TokenizerKind,
    string VectorIndexKind,
    string[] IngredientIds);

// This logical unit is the first learned-unit bridge: BPE tokenisation + predicate features + HNSW KNN inference.
public sealed class KnnLogicalUnit<TInput, TOutput> : ILogicalUnit<TInput, TOutput>
{
    // The minimum vector width keeps token and predicate tiers separate.
    private const int MinimumVectorDimensions = 16;

    // The stable unit identity returned in LogicalUnitResult.
    private readonly string _unitId;

    // The role slot this learned unit fills.
    private readonly string _unitRole;

    // The domain scope this learned unit serves.
    private readonly string _domainTag;

    // The trained BPE tokenizer from project 01.
    private readonly BpeTokenizer _tokenizer;

    // The HNSW index from project 06.
    private readonly HnswIndex _index;

    // The source examples keyed by example id.
    private readonly Dictionary<string, LogicalKnnTrainingExample<TInput, TOutput>> _examples;

    // The text selector converts runtime input into the same learning text used during training.
    private readonly Func<TInput, string> _textSelector;

    // The vector dimensions used by the learned model.
    private readonly int _vectorDimensions;

    // The number of nearest neighbours to inspect.
    private readonly int _k;

    // The confidence floor used when the caller enables confidence gating.
    private readonly double _lessonRequestConfidenceThreshold;

    // Политика владения значением ответа. Один и тот же экземпляр закреплён во
    // владельце модели и применяется и при приёме урока в память, и при каждой
    // выдаче ответа наружу, иначе один из двух путей остался бы дырой.
    private readonly IKnnAnswerSnapshot<TOutput> _answerSnapshot;

    // The model manifest exposes the training and ingredient metadata.
    public LogicalKnnModelManifest Manifest { get; }

    private KnnLogicalUnit(
        string unitId,
        string unitRole,
        string domainTag,
        BpeTokenizer tokenizer,
        HnswIndex index,
        Dictionary<string, LogicalKnnTrainingExample<TInput, TOutput>> examples,
        Func<TInput, string> textSelector,
        IKnnAnswerSnapshot<TOutput> answerSnapshot,
        int vectorDimensions,
        int k,
        double lessonRequestConfidenceThreshold,
        LogicalKnnModelManifest manifest)
    {
        _unitId = unitId;
        _unitRole = unitRole;
        _domainTag = domainTag;
        _tokenizer = tokenizer;
        _index = index;
        _examples = examples;
        _textSelector = textSelector;
        // Политика владения ответом живёт столько же, сколько сама память.
        _answerSnapshot = answerSnapshot;
        _vectorDimensions = vectorDimensions;
        _k = k;
        _lessonRequestConfidenceThreshold = lessonRequestConfidenceThreshold;
        Manifest = manifest;
    }

    // Train one learned logical unit from labelled examples.
    // Вызывающая сторона не передала политику владения ответом, поэтому она
    // выбирается по типу ответа: заведомо неизменяемый ответ (строка, число,
    // перечисление) память берёт себе без копии. Для любого другого типа
    // политики по умолчанию НЕ СУЩЕСТВУЕТ, и обучение отказывает здесь же —
    // до проверки набора уроков, до словаря кусочков слов, до указателя
    // соседей и до выдачи обученного блока наружу. Режим общего доступа
    // остаётся доступным, но его называет сам вызывающий через перегрузку с
    // явной политикой; молча он больше не выбирается, потому что молчаливый
    // общий доступ снаружи неотличим от настоящего владения.
    public static OperationResult<KnnLogicalUnit<TInput, TOutput>> Train(
        string unitId,
        string unitRole,
        string domainTag,
        IReadOnlyList<LogicalKnnTrainingExample<TInput, TOutput>> examples,
        Func<TInput, string> textSelector,
        int vectorDimensions = 64,
        int k = 3,
        double lessonRequestConfidenceThreshold = 0.70)
    {
        // Решение о политике принимается в одном месте: разрешитель по умолчанию
        // отвечает либо политикой для поддержанного неизменяемого типа, либо
        // именованным отказом для любого другого типа ответа.
        var defaultSnapshot = KnnAnswerSnapshot.ResolveDefault<TOutput>();

        // Отказ разрешителя возвращается немедленно и тем же конвертом с его
        // собственным кодом. Это самая ранняя возможная точка: ни одна строка
        // набора ещё не прочитана, словарь и указатель соседей не создавались,
        // обученный блок наружу не выдан, ранее обученная память не затронута.
        if (!defaultSnapshot.IsSuccess)
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail(defaultSnapshot.ErrorMessage, defaultSnapshot.ErrorCode);

        // Дальше обучение ничем не отличается от того, которому политику
        // передали явно: путь приёма и выдачи ответа ровно один.
        return Train(
            unitId,
            unitRole,
            domainTag,
            examples,
            textSelector,
            defaultSnapshot.Value!,
            vectorDimensions,
            k,
            lessonRequestConfidenceThreshold);
    }

    // Train one learned logical unit with an explicit answer-ownership policy.
    // Эта перегрузка нужна изменяемым типам ответа: политика обязана создавать
    // новый верхний объект и копировать вложенные коллекции, иначе получатель
    // ответа сможет молча переписать обученную память.
    public static OperationResult<KnnLogicalUnit<TInput, TOutput>> Train(
        string unitId,
        string unitRole,
        string domainTag,
        IReadOnlyList<LogicalKnnTrainingExample<TInput, TOutput>> examples,
        Func<TInput, string> textSelector,
        IKnnAnswerSnapshot<TOutput> answerSnapshot,
        int vectorDimensions = 64,
        int k = 3,
        double lessonRequestConfidenceThreshold = 0.70)
    {
        // Validate identity fields first.
        if (string.IsNullOrWhiteSpace(unitId))
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("Unit id is required.", "UNIT_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(unitRole))
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("Unit role is required.", "UNIT_ROLE_REQUIRED");
        if (string.IsNullOrWhiteSpace(domainTag))
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("Domain tag is required.", "DOMAIN_TAG_REQUIRED");
        if (examples is null || examples.Count == 0)
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("At least one training example is required.", "TRAINING_EXAMPLES_REQUIRED");
        if (textSelector is null)
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("Text selector is required.", "TEXT_SELECTOR_REQUIRED");
        // Без политики владения ответом обучать нечем: память не смогла бы ни
        // принять ответ себе, ни выдать его наружу, не поделившись ссылкой.
        if (answerSnapshot is null)
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("Answer snapshot policy is required.", "ANSWER_SNAPSHOT_REQUIRED");

        // Normalize model parameters.
        var dimensions = Math.Max(MinimumVectorDimensions, vectorDimensions);
        var safeK = Math.Max(1, Math.Min(k, examples.Count));
        var safeLessonRequestConfidenceThreshold = Math.Clamp(lessonRequestConfidenceThreshold, 0.0, 1.0);

        try
        {
            // One lesson must own exactly one name, so every normalized id seen so far is
            // collected here while the rows are still only being checked.
            var normalizedIds = new HashSet<string>(StringComparer.Ordinal);

            // Собственные значения ответов складываются здесь в том же порядке,
            // в каком пришли строки: ниже они попадут в память вместо исходных
            // ссылок вызывающей стороны. Порядок важен, поэтому список, а не набор.
            var ownedOutputs = new List<TOutput>(examples.Count);

            // Check every row before the expensive preparation starts: an incomplete row
            // must not cost a tokenizer run and an index build before it is refused.
            foreach (var example in examples)
            {
                // Reject examples without stable identity.
                if (string.IsNullOrWhiteSpace(example.ExampleId))
                    return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("Training example id is required.", "TRAINING_EXAMPLE_ID_REQUIRED");

                // All three metadata carriers are mandatory, so a missing array is refused
                // here rather than stored as a hole that only surfaces on a later answer.
                if (example.PredicateIds is null || example.DomainTags is null || example.EvidenceIds is null)
                    return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("Training example predicate ids, domain tags and evidence ids are required.", "TRAINING_EXAMPLE_METADATA_REQUIRED");

                // Normalize the id exactly the way the storage loop below normalizes it:
                // trim the edges, then compare the remaining characters byte for byte.
                var normalizedId = example.ExampleId.Trim();

                // A repeated normalized id means two different lessons claim one memory key.
                // The example dictionary would keep only the last of them while the vector
                // index would keep both vectors, so an exact hit on the first lesson's vector
                // would answer with the second lesson's output. The whole batch is refused
                // here, before the tokenizer and the index exist, which leaves any memory
                // trained earlier untouched.
                if (!normalizedIds.Add(normalizedId))
                    return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail(
                        $"Training example id '{normalizedId}' is used by more than one row after trimming.",
                        "TRAINING_EXAMPLE_ID_DUPLICATE");

                // Память берёт ответ себе ещё здесь, до токенизатора и указателя
                // соседей. Политика, которая не умеет владеть этим типом ответа,
                // останавливает обучение внятным отказом, а не тихо принимает
                // чужую ссылку; ранее обученная память при этом не затронута.
                var ownedOutput = answerSnapshot.ForStorage(example.Output);

                // Отказ политики возвращается тем же конвертом с её собственным кодом.
                if (!ownedOutput.IsSuccess)
                    return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail(ownedOutput.ErrorMessage, ownedOutput.ErrorCode);

                // Принятое во владение значение запоминается на своём месте в порядке строк.
                ownedOutputs.Add(ownedOutput.Value!);
            }

            // Measure training work for evidence.
            var allocationBefore = GC.GetAllocatedBytesForCurrentThread();
            var stopwatch = Stopwatch.StartNew();

            // Train BPE over the same text corpus the unit will classify.
            var tokenizer = new BpeTokenizer();
            var trainingText = string.Join(Environment.NewLine, examples.Select(example => textSelector(example.Input)));
            tokenizer.Train(trainingText, vocabSize: 320);

            // Build the HNSW vector index.
            var index = new HnswIndex(m: 8, efConstruction: 64, efSearch: 32);
            var exampleMap = new Dictionary<string, LogicalKnnTrainingExample<TInput, TOutput>>(StringComparer.Ordinal);
            // Обход идёт по номеру строки, потому что рядом с самой строкой нужно
            // взять принятое во владение значение ответа из списка выше.
            for (var exampleIndex = 0; exampleIndex < examples.Count; exampleIndex++)
            {
                // Текущая строка набора и её принятый во владение ответ.
                var example = examples[exampleIndex];
                // Store the example and add its vector.
                var trimmedId = example.ExampleId.Trim();
                // The memory keeps its own copies of all three metadata arrays. A record copy
                // alone shares the nested arrays with the caller, so changing them after
                // training would silently rewrite what this unit answers. The caller still
                // holds its own arrays and may change them freely; those changes now stay
                // outside this unit.
                exampleMap[trimmedId] = example with
                {
                    ExampleId = trimmedId,
                    // В память кладётся значение, принятое во владение политикой, а не
                    // ссылка вызывающей стороны: иначе изменение исходного объекта
                    // после обучения переписало бы ответ без всякого обучения.
                    Output = ownedOutputs[exampleIndex],
                    PredicateIds = [.. example.PredicateIds],
                    DomainTags = [.. example.DomainTags],
                    EvidenceIds = [.. example.EvidenceIds]
                };
                index.Add(Vectorize(tokenizer, textSelector(example.Input), NormalizePredicates(example.PredicateIds), dimensions), trimmedId);
            }

            // Stop training measurement.
            stopwatch.Stop();
            var allocationAfter = GC.GetAllocatedBytesForCurrentThread();

            // Build the ingredient manifest.
            var manifest = new LogicalKnnModelManifest(
                unitId.Trim(),
                unitRole.Trim(),
                domainTag.Trim(),
                exampleMap.Count,
                dimensions,
                safeK,
                safeLessonRequestConfidenceThreshold,
                stopwatch.Elapsed.TotalMilliseconds,
                Math.Max(0, allocationAfter - allocationBefore),
                "01.Tokenizer:BpeTokenizer",
                "06.VectorSearch:HnswIndex",
                [
                    "01.Tokenizer:BpeTokenizer",
                    "06.VectorSearch:HnswIndex",
                    "logical-predicate-vector-tier",
                    "knn-nearest-neighbour-selection",
                    // Состояние владения ответом видно в описи обученной модели:
                    // «known-immutable» — память владеет ответом без копии,
                    // «custom» — владеет по переданной типизированной политике,
                    // «declared-shared» — владение НЕ заявлено и объявлено общим.
                    $"knn-answer-snapshot:{KnnAnswerSnapshot.DescribeKind(answerSnapshot)}"
                ]);

            // Four carriers now describe the same batch, so they are compared before the
            // unit is handed out: a disagreement means one lesson became reachable under
            // another lesson's name, and such a unit must not be published as trained.
            var memoryConsistency = VerifyTrainedMemoryConsistency(
                examples.Count,
                exampleMap.Count,
                index.Count,
                manifest.TrainingExampleCount);

            // A mismatch is reported through the same envelope instead of a silent success.
            if (!memoryConsistency.IsSuccess)
                return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail(memoryConsistency.ErrorMessage, memoryConsistency.ErrorCode);

            // Return the trained logical unit.
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Ok(new KnnLogicalUnit<TInput, TOutput>(
                unitId.Trim(),
                unitRole.Trim(),
                domainTag.Trim(),
                tokenizer,
                index,
                exampleMap,
                textSelector,
                // Та же политика переходит во владельца модели: приём и выдача
                // обязаны судить об одном и том же владении одинаково.
                answerSnapshot,
                dimensions,
                safeK,
                safeLessonRequestConfidenceThreshold,
                manifest));
        }
        catch (Exception ex)
        {
            // Keep training failures inside the shared operation envelope.
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail(ex.Message, "KNN_LOGICAL_UNIT_TRAINING_FAILED");
        }
    }

    // Execute one learned KNN inference pass.
    public Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(
        LogicalUnitRequest<TInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("Cancelled.", "CANCELLED"));

        // Reject missing input.
        if (request.Input is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("Input is required.", "INPUT_REQUIRED"));

        try
        {
            // Признаки запроса начинаются пустым набором: отсутствующий ключ и
            // значение-пустышка означают законное отсутствие признаков, как и прежде.
            var queryPredicates = new HashSet<string>(StringComparer.Ordinal);

            // Ключ есть и значение не пустышка: дальше носитель обязан быть пригодным.
            if (request.Metadata.TryGetValue("predicate_ids", out var predicateObject) && predicateObject is not null)
            {
                // Прежде принимался ровно один носитель — массив строк, а любой другой
                // молча становился пустым набором, и потерю признаков никто не замечал.
                // Теперь принимается любая последовательность строк: массив, список,
                // закрытая от записи коллекция.
                if (predicateObject is not IEnumerable<string> predicateSequence)
                    return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail(
                        $"Metadata key 'predicate_ids' carries '{predicateObject.GetType().FullName}', which is not a sequence of strings. Pass string[], List<string>, ReadOnlyCollection<string> or another IEnumerable<string>.",
                        "PREDICATE_IDS_CARRIER_UNSUPPORTED"));

                // Последовательность перечисляется РОВНО ОДИН раз в принадлежащий этому
                // вызову массив: одноразовый источник остаётся пригодным, а поздняя
                // правка коллекции вызывающей стороной этот запрос уже не меняет.
                var ownedPredicates = predicateSequence.ToArray();

                // Элемент-пустышка раньше либо ронял чтение безымянным сбоем вывода,
                // либо исчезал вместе со всеми признаками и давал успешный ответ.
                // Ни одно из двух не говорит вызывающей стороне, что именно непригодно.
                for (var predicateIndex = 0; predicateIndex < ownedPredicates.Length; predicateIndex++)
                {
                    // Отказ называет и носитель, и точное место непригодного элемента.
                    if (ownedPredicates[predicateIndex] is null)
                        return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail(
                            $"Metadata key 'predicate_ids' carried by '{predicateObject.GetType().FullName}' has a null element at index {predicateIndex}.",
                            "PREDICATE_IDS_NULL_ELEMENT"));
                }

                // Проверенная собственная копия идёт в ТУ ЖЕ нормализацию, что и у
                // писателя урока: обрезка, верхний регистр, отбрасывание пустых строк.
                queryPredicates = NormalizePredicates(ownedPredicates);
            }

            // Search nearest training examples.
            var query = Vectorize(_tokenizer, _textSelector(request.Input), queryPredicates, _vectorDimensions);
            var hits = _index.Search(query, _k);
            if (hits.Count == 0)
                return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("No learned neighbours were found.", "NO_KNN_NEIGHBOURS"));

            // Use the nearest neighbour output as the learned prediction.
            var best = hits[0];
            var confidence = Math.Round(Math.Clamp(best.score, 0.0f, 1.0f), 4);
            var bestExample = _examples[best.id];

            // Наружу уходит значение, выданное политикой владения, а не сама
            // запомненная ссылка. Иначе получатель ответа смог бы изменить
            // объект у себя и тем самым переписать обученную память, и второй
            // такой же вопрос вернул бы уже испорченный ответ.
            var ownedAnswer = _answerSnapshot.ForRead(bestExample.Output);

            // Отказ политики на выдаче возвращается тем же конвертом с её кодом.
            if (!ownedAnswer.IsSuccess)
                return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail(ownedAnswer.ErrorMessage, ownedAnswer.ErrorCode));

            // Low-confidence results remain visible in warnings instead of being invented as a clean pass.
            var warnings = request.Flags.UseLowConfidenceLessonSignal && confidence < _lessonRequestConfidenceThreshold
                ? new[] { "knowledge_edge: learned-unit confidence below floor" }
                : [];

            // Include neighbour evidence ids for traceability.
            var evidenceIds = hits
                .Select(hit => hit.id)
                .Concat(bestExample.EvidenceIds)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            // Return the learned result through the normal logical-unit envelope.
            var result = new LogicalUnitResult<TOutput>(
                _unitId,
                _unitRole,
                _domainTag,
                // Ответ отдаётся из политики владения, а не из памяти напрямую.
                ownedAnswer.Value!,
                confidence,
                // Each answer carries fresh copies of both metadata arrays. Whoever receives
                // this result may change them freely; the stored memory keeps its own values,
                // so the next answer for the same neighbour is unchanged without re-training.
                [.. bestExample.PredicateIds],
                bestExample.DomainTags.Length == 0 ? [_domainTag] : [.. bestExample.DomainTags],
                // The neighbour evidence ids are already rebuilt into a new array above.
                evidenceIds,
                warnings);

            return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Ok(result));
        }
        catch (Exception ex)
        {
            // Keep inference failures inside the shared operation envelope.
            return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail(ex.Message, "KNN_LOGICAL_UNIT_INFERENCE_FAILED"));
        }
    }

    // Compare the four carriers that must describe one and the same trained batch.
    // Accepted rows, stored examples, vector index entries and the manifest count are
    // filled by different code paths, so agreement between them is what proves that no
    // lesson was silently replaced by another lesson under a shared name. The method is
    // visible to the test assembly so a deliberately wrong count can be fed to it.
    internal static OperationResult<int> VerifyTrainedMemoryConsistency(
        int acceptedRowCount,
        int uniqueNormalizedIdCount,
        int vectorIndexEntryCount,
        int manifestTrainingExampleCount)
    {
        // All four carriers agree, so the agreed lesson count is the result of the check.
        if (acceptedRowCount == uniqueNormalizedIdCount
            && uniqueNormalizedIdCount == vectorIndexEntryCount
            && vectorIndexEntryCount == manifestTrainingExampleCount)
            return OperationResult<int>.Ok(acceptedRowCount);

        // Name every carrier and its own count, so the disagreement is readable from the
        // message alone and the caller does not have to reproduce the batch to see it.
        return OperationResult<int>.Fail(
            $"Trained memory counts disagree: accepted rows {acceptedRowCount}, unique normalized ids {uniqueNormalizedIdCount}, vector index entries {vectorIndexEntryCount}, manifest training example count {manifestTrainingExampleCount}.",
            "TRAINED_MEMORY_COUNT_MISMATCH");
    }

    // Convert text and predicates into a two-tier vector: BPE token tier + predicate tier.
    private static float[] Vectorize(BpeTokenizer tokenizer, string text, HashSet<string> predicates, int dimensions)
    {
        // Split the vector into token and predicate halves.
        var vector = new float[dimensions];
        var tokenTierWidth = Math.Max(1, dimensions / 2);
        var predicateTierOffset = tokenTierWidth;
        var predicateTierWidth = dimensions - predicateTierOffset;

        // Add BPE token counts to the token tier.
        foreach (var token in tokenizer.Encode(text ?? string.Empty))
        {
            var slot = Math.Abs(token) % tokenTierWidth;
            vector[slot] += 1.0f;
        }

        // Add predicate counts to the predicate tier.
        if (predicateTierWidth > 0)
        {
            foreach (var predicate in predicates)
            {
                var slot = predicateTierOffset + (StableHash(predicate) % predicateTierWidth);
                vector[slot] += 2.0f;
            }
        }

        // Normalize the final vector so cosine similarity is meaningful.
        var norm = MathF.Sqrt(vector.Sum(value => value * value));
        if (norm <= 1e-6f)
            return vector;

        for (var i = 0; i < vector.Length; i++)
            vector[i] /= norm;

        return vector;
    }

    // Normalize predicate ids to stable uppercase tokens.
    private static HashSet<string> NormalizePredicates(string[]? predicates)
    {
        return (predicates ?? [])
            .Select(predicate => predicate.Trim())
            .Where(predicate => !string.IsNullOrWhiteSpace(predicate))
            .Select(predicate => predicate.ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);
    }

    // Stable non-cryptographic hash for predicate slots.
    private static int StableHash(string value)
    {
        unchecked
        {
            var hash = 23;
            foreach (var ch in value)
                hash = (hash * 31) + ch;
            return Math.Abs(hash);
        }
    }
}
