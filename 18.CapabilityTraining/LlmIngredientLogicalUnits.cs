// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ProductHost")]

namespace AgentInfrastructure.Logical;

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using DPO;
using DPO.Models;
using Embeddings;
using LoRA;
using SemanticCompilerAdapter;
using TensorCore.Core;
using Tokenizer;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;
using VectorSearch;

// This record is one supervised example for a transformer-embedding KNN logical unit.
public sealed record TransformerKnnTrainingExample<TOutput>(
    string ExampleId,
    string Text,
    TOutput Output,
    string[] PredicateIds,
    string[] DomainTags);

// This record reports the concrete LLM ingredients connected to one logical unit.
public sealed record LlmIngredientManifest(
    string[] IngredientIds,
    int TrainingExampleCount,
    int VectorDimensions,
    int LoraAdapterCount,
    int DpoStepCount);

// This internal handle exposes only the exact restored inference objects to the trusted product host.
internal sealed class TransformerEmbeddingKnnRuntime
{
    internal TransformerEmbeddingKnnRuntime(LlmModel model, BpeTokenizer tokenizer)
    {
        Model = model;
        Tokenizer = tokenizer;
    }

    internal LlmModel Model { get; }
    internal BpeTokenizer Tokenizer { get; }
}

// This logical unit connects tokenizer, transformer embeddings, and HNSW retrieval to ExecuteAsync.
public sealed class TransformerEmbeddingKnnLogicalUnit<TOutput> : ILogicalUnit<string, TOutput>
{
    private readonly string _unitId;
    private readonly string _unitRole;
    private readonly string _domainTag;
    private readonly LlmModel _model;
    private readonly BpeTokenizer _tokenizer;
    private readonly EmbeddingModel _embeddingModel;
    private readonly HnswIndex _index;
    private readonly Dictionary<string, TransformerKnnTrainingExample<TOutput>> _examples;
    // Снимок словесной статистики этого корпуса: средняя длина документа, длины документов,
    // приведённые к корням слова и частота документов по слову. Считается один раз при
    // обучении и дальше только читается, поэтому оценка кандидата больше не пересчитывает
    // весь корпус. Поле только для чтения, как и сама память примеров.
    private readonly LexicalCorpusStatistics _lexicalStatistics;
    // This field holds the opt-in candidate-ranking catalogue; null keeps the unchanged native behaviour.
    private readonly KnnCandidatePrincipleCatalog? _candidatePrinciples;
    // Политика владения значением ответа. Один и тот же экземпляр закреплён во владельце
    // модели и применяется дважды: при приёме урока в память и при каждой выдаче ответа
    // наружу. Если бы её применял только один из двух путей, второй остался бы дырой:
    // копия на входе бесполезна, когда наружу по-прежнему отдают саму память.
    private readonly IKnnAnswerSnapshot<TOutput> _answerSnapshot;

    private TransformerEmbeddingKnnLogicalUnit(
        string unitId,
        string unitRole,
        string domainTag,
        LlmModel model,
        BpeTokenizer tokenizer,
        EmbeddingModel embeddingModel,
        HnswIndex index,
        Dictionary<string, TransformerKnnTrainingExample<TOutput>> examples,
        // Политика владения ответом приходит вместе с той памятью, к которой она применена.
        IKnnAnswerSnapshot<TOutput> answerSnapshot,
        LexicalCorpusStatistics lexicalStatistics,
        LlmIngredientManifest manifest,
        KnnCandidatePrincipleCatalog? candidatePrinciples)
    {
        _unitId = unitId;
        _unitRole = unitRole;
        _domainTag = domainTag;
        _model = model;
        _tokenizer = tokenizer;
        _embeddingModel = embeddingModel;
        _index = index;
        // Память берёт СВОЙ словарь уроков, а не тот объект словаря, который ей передали.
        // Прежде словарь принимался по ссылке, и владелец того словаря мог после создания
        // единицы добавить, удалить или подменить урок — ответы молча поехали бы вместе с
        // ним. Строки уроков внутри словаря уже приняты во владение выше, поэтому здесь
        // копируется только сам словарь, и сравнение опознаний остаётся побайтовым.
        _examples = new Dictionary<string, TransformerKnnTrainingExample<TOutput>>(examples, StringComparer.Ordinal);
        // Политика владения ответом живёт столько же, сколько сама память.
        _answerSnapshot = answerSnapshot;
        // Снимок приходит уже собранным и проверенным, вместе с той памятью, которой он принадлежит.
        _lexicalStatistics = lexicalStatistics;
        Manifest = manifest;
        // Keep the admitted catalogue, which was already validated before it reached this constructor.
        _candidatePrinciples = candidatePrinciples;
    }

    // The manifest identifies the transformer, embedding, index, and fixed retrieval ingredients.
    public LlmIngredientManifest Manifest { get; }

    // Это свойство сообщает, сколько правил ранжирования реально настроено в этом модуле.
    // Ноль означает прежнее родное поведение: и отсутствующий каталог, и пустой каталог.
    // Свойство нужно сохранению снимка, чтобы увидеть режим, который снимок пока не умеет записывать.
    internal int CandidatePrincipleCount => _candidatePrinciples?.Count ?? 0;

    // Раздельная стоимость последнего обычного вызова оценки кандидатов.
    //
    // Зачем она продукту. Словесная и структурная части оценки стоят по-разному и ведут себя
    // по-разному при росте числа кандидатов: словесная читает неизменяемый снимок корпуса и
    // потому от их числа не зависит, структурная разбирает текст каждого кандидата и потому
    // зависит. Пока обе цены складывались в одну, этого различия не было видно вовсе, и
    // улучшение одной половины могло скрыть ухудшение другой.
    //
    // Чем это НЕ является. Это измеритель, а не улучшение: структурная цена этим слайсом не
    // уменьшена, она только названа отдельным числом. И это не измерение времени: числа
    // счётные и воспроизводимые, а время зависит от машины.
    //
    // Пустая величина до первого вызова оценки: нулевые счётчики и ноль кандидатов.
    internal CandidateScoringCost LastCandidateScoringCost { get; private set; } = CandidateScoringCost.None;

    // Return a narrow immutable handle to the same model and tokenizer already used by this unit.
    internal TransformerEmbeddingKnnRuntime GetServingRuntime()
        => new(_model, _tokenizer);

    // Train the unit by embedding examples with the real transformer-backed embedding model.
    // Вызывающая сторона не передала политику владения ответом, поэтому она выбирается по
    // типу ответа: заведомо неизменяемый ответ (строка, число, перечисление) память берёт
    // себе без копии. Для любого другого типа политики по умолчанию НЕ СУЩЕСТВУЕТ, и
    // обучение отказывает здесь же — до вложений, до указателя близких векторов, до
    // снимка словесной статистики и до выдачи обученной единицы наружу. Режим общего
    // доступа остаётся доступным, но его называет сам вызывающий через перегрузку с явной
    // политикой; молча он не выбирается никогда, потому что молчаливый общий доступ
    // снаружи неотличим от настоящего владения.
    public static OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>> Train(
        string unitId,
        string unitRole,
        string domainTag,
        LlmModel model,
        BpeTokenizer tokenizer,
        IReadOnlyList<TransformerKnnTrainingExample<TOutput>> examples,
        // This optional catalogue is the opt-in candidate ranking; omitting it keeps the previous behaviour.
        KnnCandidatePrincipleCatalog? candidatePrinciples = null)
    {
        // Решение о политике принимается в одном месте на оба блока ближайших соседей:
        // разрешитель по умолчанию отвечает либо политикой для поддержанного
        // неизменяемого типа, либо именованным отказом для любого другого типа ответа.
        var defaultSnapshot = KnnAnswerSnapshot.ResolveDefault<TOutput>();

        // Отказ разрешителя возвращается немедленно и тем же конвертом с его собственным
        // кодом. Это самая ранняя возможная точка: ни один урок ещё не прочитан, вложений
        // не считалось, указателя близких векторов нет, ранее обученная память не затронута.
        if (!defaultSnapshot.IsSuccess)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail(defaultSnapshot.ErrorMessage, defaultSnapshot.ErrorCode);

        // Дальше обучение ничем не отличается от того, которому политику передали явно:
        // путь приёма и путь выдачи ответа ровно один, и он ниже.
        return Train(
            unitId,
            unitRole,
            domainTag,
            model,
            tokenizer,
            examples,
            defaultSnapshot.Value!,
            candidatePrinciples);
    }

    // Train the unit with an explicit answer-ownership policy.
    // Эта перегрузка нужна изменяемым типам ответа: политика обязана создавать новый
    // верхний объект И копировать вложенные коллекции, иначе получатель ответа сможет
    // молча переписать обученную память. Прежняя перегрузка без политики остаётся на
    // месте и для заведомо неизменяемых ответов работает как прежде, поэтому ни один
    // существующий вызывающий не меняется.
    public static OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>> Train(
        string unitId,
        string unitRole,
        string domainTag,
        LlmModel model,
        BpeTokenizer tokenizer,
        IReadOnlyList<TransformerKnnTrainingExample<TOutput>> examples,
        // Договор владения ответом переиспользуется из 21.AgentInfrastructure как есть.
        IKnnAnswerSnapshot<TOutput> answerSnapshot,
        // This optional catalogue is the opt-in candidate ranking; omitting it keeps the previous behaviour.
        KnnCandidatePrincipleCatalog? candidatePrinciples = null)
    {
        // A catalogue of another profile must never rank the memory of this unit.
        if (candidatePrinciples is not null && !string.Equals(candidatePrinciples.Profile, domainTag, StringComparison.Ordinal))
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Principle catalogue profile does not match the unit domain tag.", "FOREIGN_PRINCIPLE_PROFILE");

        // Reject missing ingredients through the shared envelope.
        if (model is null)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Transformer model is required.", "TRANSFORMER_MODEL_REQUIRED");
        if (tokenizer is null)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Tokenizer is required.", "TOKENIZER_REQUIRED");
        if (examples is null || examples.Count == 0)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("At least one training example is required.", "TRAINING_EXAMPLES_REQUIRED");
        // Без политики владения ответом обучать нечем: память не смогла бы ни принять
        // ответ себе, ни выдать его наружу, не поделившись ссылкой с чужим кодом.
        if (answerSnapshot is null)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Answer snapshot policy is required.", "ANSWER_SNAPSHOT_REQUIRED");

        // Build the transformer-backed embedding model.
        var embeddingModel = new EmbeddingModel(model, tokenizer, maxLen: Math.Min(64, model.Config.ContextLen));

        // Build a small HNSW index over the produced embeddings.
        var index = new HnswIndex(m: 8, efConstruction: 32, efSearch: 16);
        var exampleMap = new Dictionary<string, TransformerKnnTrainingExample<TOutput>>(StringComparer.Ordinal);
        foreach (var example in examples)
        {
            // A missing row is rejected through the shared envelope instead of failing later.
            if (example is null)
                return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Training examples require id and text.", "INVALID_TRAINING_EXAMPLE");
            // Reject incomplete examples before indexing.
            if (string.IsNullOrWhiteSpace(example.ExampleId) || string.IsNullOrWhiteSpace(example.Text))
                return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Training examples require id and text.", "INVALID_TRAINING_EXAMPLE");
            // Both metadata carriers are mandatory, so a missing array is refused here
            // rather than stored as a hole that only surfaces on a later answer.
            if (example.PredicateIds is null || example.DomainTags is null)
                return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Training examples require predicate ids and domain tags.", "INVALID_TRAINING_EXAMPLE");
            if (exampleMap.ContainsKey(example.ExampleId))
                return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Training example ids must be unique.", "INVALID_TRAINING_EXAMPLE");

            // Память берёт ответ себе ДО дорогой подготовки вложения и до записи в
            // указатель близких векторов. Политика, которая не умеет владеть этим типом
            // ответа, останавливает обучение внятным отказом, а не тихо принимает чужую
            // ссылку; ранее обученная память при этом не затронута.
            var ownedOutput = answerSnapshot.ForStorage(example.Output);

            // Отказ политики возвращается тем же конвертом с её собственным кодом.
            if (!ownedOutput.IsSuccess)
                return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail(ownedOutput.ErrorMessage, ownedOutput.ErrorCode);

            // Embed and index the example.
            var vector = embeddingModel.Embed(example.Text);
            index.Add(vector, example.ExampleId, new Dictionary<string, string>
            {
                ["unit_id"] = unitId,
                ["domain_tag"] = domainTag
            });
            // The memory keeps its own copies of both metadata arrays. The caller still
            // holds its own arrays and may change them afterwards; those changes now stay
            // outside this unit instead of silently rewriting what it answers.
            exampleMap[example.ExampleId] = example with
            {
                // В память кладётся значение, принятое во владение политикой, а не ссылка
                // вызывающей стороны: иначе изменение исходного объекта ответа после
                // обучения переписало бы урок, ничему эту память не обучая.
                Output = ownedOutput.Value!,
                PredicateIds = [.. example.PredicateIds],
                DomainTags = [.. example.DomainTags]
            };
        }

        // Словесная статистика корпуса собирается ОДИН раз, из той самой памяти, которая
        // только что проиндексирована, и в том же порядке перебора, в каком её читала
        // прежняя оценка. Разовая цена подготовки названа в самом снимке отдельными
        // величинами, а не спрятана в тёплый запрос.
        var lexicalStatistics = LexicalCorpusStatistics.Build(
            [.. exampleMap.Values.Select(stored => new LexicalCorpusDocument(
                stored.ExampleId,
                stored.Text,
                LexicalCorpusStatistics.DescribeOutput(stored.Output)))]);
        // Отказ сборки приходит общим конвертом: частично собранная единица наружу не выдаётся.
        if (!lexicalStatistics.IsSuccess)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail(lexicalStatistics.ErrorMessage, lexicalStatistics.ErrorCode);

        // Build the manifest with every concrete ingredient used by this path.
        var manifest = new LlmIngredientManifest(
            BuildIngredientIds(candidatePrinciples, answerSnapshot),
            examples.Count,
            model.Config.HiddenDim,
            LoraAdapterCount: 0,
            DpoStepCount: 0);

        // Return the executable retrieval unit.
        return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Ok(new TransformerEmbeddingKnnLogicalUnit<TOutput>(
            unitId,
            unitRole,
            domainTag,
            model,
            tokenizer,
            embeddingModel,
            index,
            exampleMap,
            // Та же политика переходит во владельца модели: приём и выдача обязаны судить
            // об одном и том же владении одинаково.
            answerSnapshot,
            lexicalStatistics.Value!,
            manifest,
            candidatePrinciples));
    }

    // Attach an opt-in ranking catalogue to an already trained unit without re-embedding anything.
    public OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>> WithCandidatePrinciples(
        KnnCandidatePrincipleCatalog candidatePrinciples)
    {
        // A missing catalogue is a caller mistake; an empty catalogue is built through CreateEmpty.
        if (candidatePrinciples is null)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Principle catalogue is required.", "PRINCIPLE_CATALOG_REQUIRED");
        // The catalogue must serve exactly the profile this unit was trained for.
        if (!string.Equals(candidatePrinciples.Profile, _domainTag, StringComparison.Ordinal))
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Principle catalogue profile does not match the unit domain tag.", "FOREIGN_PRINCIPLE_PROFILE");

        // Снимок статистики переиспользуется вместе с той же памятью, поэтому перед
        // переиспользованием доказывается, что он принадлежит именно ей: отпечаток
        // содержимого считается заново из фактических документов. Проверка не обращается
        // ни к модели, ни к указателю близких векторов.
        var statisticsLineage = _lexicalStatistics.RequireBelongsTo(DescribeCorpus());
        // Чужой или устаревший снимок отвергается общим конвертом, а не переиспользуется молча.
        if (!statisticsLineage.IsSuccess)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail(statisticsLineage.ErrorMessage, statisticsLineage.ErrorCode);

        // Reuse the very same model, tokenizer, embedding model, index and memory; only the ranking policy differs.
        return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Ok(new TransformerEmbeddingKnnLogicalUnit<TOutput>(
            _unitId,
            _unitRole,
            _domainTag,
            _model,
            _tokenizer,
            _embeddingModel,
            _index,
            _examples,
            // Политика владения ответом та же: меняется только порядок разбора кандидатов,
            // поэтому обещание владения не имеет права тихо измениться вместе с ним.
            _answerSnapshot,
            statisticsLineage.Value!,
            new LlmIngredientManifest(
                BuildIngredientIds(candidatePrinciples, _answerSnapshot),
                Manifest.TrainingExampleCount,
                Manifest.VectorDimensions,
                Manifest.LoraAdapterCount,
                Manifest.DpoStepCount),
            candidatePrinciples));
    }

    // List the concrete ingredients of this path, adding the ranking marker only when ranking is actually configured.
    // Состояние владения ответом тоже названо здесь: читатель описи обученной модели не
    // должен выяснять по коду, владеет ли эта память своими ответами.
    private static string[] BuildIngredientIds(
        KnnCandidatePrincipleCatalog? candidatePrinciples,
        IKnnAnswerSnapshot<TOutput> answerSnapshot)
    {
        // Start from the fixed ingredient list that every unit of this kind uses.
        var ingredients = new List<string>
        {
            "01.Tokenizer:BpeTokenizer",
            "03.TransformerModel:LlmModel",
            "05.Embeddings:EmbeddingModel",
            "06.VectorSearch:HnswIndex",
            "E15:SEMANTIC_ROOT_TOKENIZATION",
            "E25:PATTERN_CLASSIFIER",
            "E25:PATTERN_CLASSIFIER_FUSION",
            "E34V2:BM25_KNN_FUSION",
            // Состояние владения ответом видно в описи обученной модели:
            // «known-immutable» — память владеет ответом без копии,
            // «custom» — владеет по переданной типизированной политике,
            // «declared-shared» — владение НЕ заявлено и объявлено общим.
            $"knn-answer-snapshot:{KnnAnswerSnapshot.DescribeKind(answerSnapshot)}"
        };

        // An absent or empty catalogue leaves the ingredient list byte-identical to the previous behaviour.
        if (candidatePrinciples is { Count: > 0 })
            ingredients.Add("KDP01:CANDIDATE_PRINCIPLE_RANKING");

        // Hand back the ingredient list for the manifest.
        return [.. ingredients];
    }

    // Execute one inference pass through transformer embedding and HNSW nearest-neighbour selection.
    public Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(
        LogicalUnitRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("Cancelled.", "CANCELLED"));

        // Reject missing input.
        if (string.IsNullOrWhiteSpace(request.Input))
            return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("Unit input cannot be empty.", "EMPTY_UNIT_INPUT"));

        // Embed the input with the transformer-backed embedding model.
        var queryVector = _embeddingModel.Embed(request.Input);

        // Retrieve a candidate set, then apply the locked E34V2 BM25+KNN fusion decision.
        var candidates = _index.Search(queryVector, topK: Math.Max(1, _examples.Count))
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.id) && _examples.ContainsKey(candidate.id))
            .ToArray();
        if (candidates.Length == 0)
            return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("No nearest example was found.", "NO_NEAREST_EXAMPLE"));

        var scored = ScoreCandidates(request.Input, candidates);

        // Without an opt-in catalogue the native fusion order decides, exactly as before this transfer.
        var selectedId = scored[0].ExampleId;
        // Carry the fused score of that same winner, so the confidence is computed from one source.
        var selectedScore = scored[0].FusedScore;
        // This evidence list records the native fusion and, only when it ran, the opt-in ranking.
        var evidence = new List<string> { selectedId, "E34V2:BM25_KNN_FUSION" };

        // With an admitted non-empty catalogue the already found candidates are reordered inside a fixed budget.
        if (_candidatePrinciples is { Count: > 0 })
        {
            // Hand the ranker the same candidates and the same native scores the native fusion produced.
            var nativeCandidates = scored
                .Select(candidate => new KnnNativeCandidate(candidate.ExampleId, _examples[candidate.ExampleId].Text, candidate.FusedScore))
                .ToArray();
            // Run the admitted principles over those candidates; the principles never add a candidate.
            var ranked = KnnCandidatePrincipleRanker.Rank(
                _candidatePrinciples,
                new KnnPrincipleQuery(_domainTag, request.Input),
                nativeCandidates);
            // A refused ranking is reported through the shared envelope instead of silently falling back.
            if (!ranked.IsSuccess)
                return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail(ranked.ErrorMessage, ranked.ErrorCode));

            // Take the winner of the adjusted order and its adjusted score.
            selectedId = ranked.Value![0].ExampleId;
            selectedScore = ranked.Value![0].FinalScore;
            // Record that the opt-in ranking took part in this decision.
            evidence[0] = selectedId;
            evidence.Add("KDP01:CANDIDATE_PRINCIPLE_RANKING");
        }

        // Read the stored example that won the decision.
        var example = _examples[selectedId];

        // Наружу уходит значение, выданное политикой владения, а не сама запомненная
        // ссылка. Иначе получатель ответа смог бы изменить объект у себя и тем самым
        // переписать обученную память: второй такой же вопрос вернул бы уже испорченный
        // ответ, хотя никакого обучения между этими чтениями не было.
        var ownedAnswer = _answerSnapshot.ForRead(example.Output);

        // Отказ политики на выдаче возвращается тем же конвертом с её собственным кодом.
        if (!ownedAnswer.IsSuccess)
            return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail(ownedAnswer.ErrorMessage, ownedAnswer.ErrorCode));

        // Convert the selected score into the logical confidence range.
        var confidence = Math.Round(Math.Clamp(0.50 + (selectedScore * 0.49), 0.0, 0.99), 2);

        // Return the selected stored example output verbatim.
        var result = new LogicalUnitResult<TOutput>(
            _unitId,
            _unitRole,
            _domainTag,
            // Ответ отдаётся из политики владения, а не из памяти напрямую.
            ownedAnswer.Value!,
            confidence,
            // Each answer carries fresh copies of both metadata arrays. Whoever receives
            // this result may change them freely; the stored memory keeps its own values,
            // so the native order and the opt-in ranked order answer from one unchanged source.
            [.. example.PredicateIds],
            [.. example.DomainTags],
            [.. evidence],
            []);

        return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Ok(result));
    }

    private static readonly Lazy<SemanticCompiler29.PatternClassifier> PatternClassifier = new(
        () => new SemanticCompiler29.PatternClassifier(new InMemoryCompilerStorage(), new NoopCompilerEventLogger()));

    // Score HNSW candidates with vector, lexical BM25-like, and structural pattern evidence.
    private ScoredTransformerCandidate[] ScoreCandidates(string query, IReadOnlyList<(string id, float score)> candidates)
    {
        // Счётчики РАЗДЕЛЬНОЙ стоимости одного обычного вызова оценки. Они измеряются по
        // факту, а не выводятся из размера входа: поле, выведенное из входа, однажды соврёт
        // о работе, которой не было.
        var lexicalNormalizationCalls = 0;      // Приведений текста к корням в словесной части.
        long lexicalCharactersNormalized = 0;    // Знаков, прошедших словесное приведение.
        var lexicalCorpusNormalizationCalls = 0; // Приведений ДОКУМЕНТОВ КОРПУСА внутри оценки.
        var structuralClassificationCalls = 0;   // Обращений к разборщику образцов.
        long structuralCharactersClassified = 0; // Знаков, прошедших структурный разбор.

        // Сам запрос приводится к корням тем же единственным владельцем правила, что и
        // документы снимка: статистика и запрос обязаны разбираться одними правилами.
        var queryTokens = LexicalCorpusStatistics.Tokenize(query);
        // Словесное приведение запроса состоялось ровно один раз на весь вызов.
        lexicalNormalizationCalls++;
        lexicalCharactersNormalized += (query ?? string.Empty).Length;

        var queryPattern = ClassifyPattern(query);
        // Структурный разбор запроса состоялся ровно один раз на весь вызов.
        structuralClassificationCalls++;
        structuralCharactersClassified += (query ?? string.Empty).Length;

        var rawScores = new List<(string id, double vectorScore, double lexicalScore, double patternScore)>();
        foreach (var candidate in candidates)
        {
            var example = _examples[candidate.id];
            var vectorScore = Math.Clamp((candidate.score + 1.0) / 2.0, 0.0, 1.0);
            // Словесная оценка получает ОПОЗНАНИЕ документа, а не его текст: читать корпус
            // ей больше нечем, поэтому пересчитать его на каждого кандидата она не может.
            // Именно поэтому счётчик приведений корпуса внутри оценки остаётся нулём.
            var lexicalScore = ComputeBm25LikeScore(queryTokens, candidate.id);
            // Структурная часть разбирает текст КАЖДОГО кандидата, и эта цена растёт с их
            // числом. Слайс её измеряет отдельно, а не заявляет уменьшенной.
            var patternScore = string.Equals(queryPattern, ClassifyPattern(example.Text), StringComparison.Ordinal)
                ? 1.0
                : 0.0;
            structuralClassificationCalls++;
            structuralCharactersClassified += (example.Text ?? string.Empty).Length;
            rawScores.Add((candidate.id, vectorScore, lexicalScore, patternScore));
        }

        // Раздельная стоимость этого вызова публикуется одной неизменяемой величиной: по ней
        // видно, что словесная часть от числа кандидатов не зависит, а структурная зависит.
        LastCandidateScoringCost = new CandidateScoringCost(
            lexicalNormalizationCalls,       // Приведений текста к корням в словесной части.
            lexicalCharactersNormalized,     // Знаков словесного приведения.
            lexicalCorpusNormalizationCalls, // Приведений документов корпуса внутри оценки.
            structuralClassificationCalls,   // Обращений к разборщику образцов.
            structuralCharactersClassified,  // Знаков структурного разбора.
            candidates.Count);               // Сколько кандидатов оценено.

        var maxLexical = rawScores.Count == 0 ? 1.0 : Math.Max(1.0e-9, rawScores.Max(score => score.lexicalScore));
        return rawScores
            .Select(score =>
            {
                var normalizedLexical = Math.Clamp(score.lexicalScore / maxLexical, 0.0, 1.0);
                var fused = (score.vectorScore * 0.45) + (normalizedLexical * 0.35) + (score.patternScore * 0.20);
                return new ScoredTransformerCandidate(score.id, fused);
            })
            .OrderByDescending(score => score.FusedScore)
            .ThenBy(score => score.ExampleId, StringComparer.Ordinal)
            .ToArray();
    }

    // Compute a tiny BM25-style lexical score without adding a new project dependency.
    // Статистика корпуса читается из неизменяемого снимка версии корпуса, а не
    // пересчитывается. Прежде эта оценка приводила весь корпус к корням заново дважды:
    // ради средней длины документа и ради частоты документов на КАЖДОЕ слово запроса, и
    // всё это внутри оценки каждого кандидата. Формулы, округление и порядок слагаемых
    // остались прежними, поэтому оценка совпадает с прежней.
    private double ComputeBm25LikeScore(IReadOnlyDictionary<string, int> queryTokens, string documentId)
    {
        // Пустой запрос даёт нулевую словесную оценку, как и прежде.
        if (queryTokens.Count == 0)
            return 0.0;

        // Документ уже приведён к корням один раз при сборке снимка. Неизвестный документ
        // и документ без слов дают ноль, ровно как прежний пересчёт по его тексту.
        if (!_lexicalStatistics.TryGetDocument(documentId, out var document) || document.Tokens.Count == 0)
            return 0.0;

        // Длина документа взята из снимка и повторяет прежнюю нижнюю границу в единицу.
        var documentLength = document.Length;
        // Средняя длина документа тоже из снимка: прежде она считалась здесь по всему корпусу.
        var averageLength = _lexicalStatistics.AverageLength;
        var score = 0.0;
        const double k1 = 1.2;
        const double b = 0.75;
        foreach (var token in queryTokens.Keys)
        {
            // Слово запроса, которого в документе нет, в его оценку не входит.
            if (!document.Tokens.TryGetValue(token, out var frequency))
                continue;

            // Частота документов читается из снимка: прежде она пересчитывалась по всему
            // корпусу заново на каждое слово запроса.
            var documentFrequency = _lexicalStatistics.GetDocumentFrequency(token);
            var idf = Math.Log(1.0 + ((_lexicalStatistics.DocumentCount - documentFrequency + 0.5) / (documentFrequency + 0.5)));
            var numerator = frequency * (k1 + 1.0);
            var denominator = frequency + (k1 * (1.0 - b + (b * documentLength / averageLength)));
            score += idf * numerator / Math.Max(1.0e-9, denominator);
        }

        return score;
    }

    // Описание текущей памяти в том виде, в каком его видит проверка происхождения снимка:
    // порядок перебора тот же, что у сборки снимка, и у ответа берётся его текстовая форма.
    private LexicalCorpusDocument[] DescribeCorpus()
        => [.. _examples.Values.Select(stored => new LexicalCorpusDocument(
            stored.ExampleId,
            stored.Text,
            LexicalCorpusStatistics.DescribeOutput(stored.Output)))];

    private static string ClassifyPattern(string text)
    {
        var tokens = TrainingVectorBuilder.NormalizeRoots(text).ToArray();
        return tokens.Length == 0
            ? "UNKNOWN"
            : PatternClassifier.Value.Classify(tokens);
    }

    private sealed record ScoredTransformerCandidate(string ExampleId, double FusedScore);

    // Раздельная стоимость одного обычного вызова оценки кандидатов: словесная часть и
    // структурная часть считаются своими числами и в одну сумму не складываются.
    //
    // Почему разделение именно такое. Словесная часть приводит к корням ТОЛЬКО сам запрос, а
    // статистику документов читает из неизменяемого снимка версии корпуса; поэтому приведений
    // документов корпуса внутри оценки у неё ноль, и это число — главный признак того, что
    // прежний пересчёт корпуса на каждого кандидата больше не происходит. Структурная часть
    // разбирает образец запроса один раз и образец КАЖДОГО кандидата, поэтому её число растёт
    // вместе с числом кандидатов.
    internal sealed record CandidateScoringCost(
        int LexicalNormalizationCalls,        // Приведений текста к корням в словесной части.
        long LexicalCharactersNormalized,     // Знаков, прошедших словесное приведение.
        int LexicalCorpusNormalizationCalls,  // Приведений документов КОРПУСА внутри оценки.
        int StructuralClassificationCalls,    // Обращений к разборщику образцов.
        long StructuralCharactersClassified,  // Знаков, прошедших структурный разбор.
        int CandidatesScored)                 // Сколько кандидатов оценено этим вызовом.
    {
        // Пустая стоимость до первого вызова оценки: все счётчики нулевые.
        internal static CandidateScoringCost None { get; } = new(0, 0, 0, 0, 0, 0);
    }

    private sealed class InMemoryCompilerStorage : SemanticCompiler29.ICompilerStorage
    {
        private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);

        public void Save(string key, SemanticCompiler29.PipelineOutput output)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _items[key] = output;
        }

        public SemanticCompiler29.PipelineOutput? Load(string key)
            => string.IsNullOrWhiteSpace(key) ? null : _items.GetValueOrDefault(key);
    }

    private sealed class NoopCompilerEventLogger : SemanticCompiler29.ICompilerEventLogger
    {
        public void Log(SemanticCompiler29.StandardEvent evt)
        {
        }
    }
}

// This record is one preference example for DPO teaching.
public sealed record LogicalDpoPreferenceExample(
    string Prompt,
    string Chosen,
    string Rejected,
    float[]? TeachingVector = null)
{
    // Авторство сторон хранится рядом с raw строками: DPO batch получает
    // паспорт без догадок по домену или позиции вызова.
    public DpoPairSideAuthorKind? ChosenAuthorKind { get; init; }

    // Rejected-сторона обязательна отдельно, потому что запрет платной модели
    // в будущей двери тренера должен смотреть на обе стороны пары.
    public DpoPairSideAuthorKind? RejectedAuthorKind { get; init; }

    // Current rows carry the opaque deep-snapshot source attestation. Durable
    // replay rows instead carry the mutually exclusive server replay fields.
    public LogicalTrainingContentProvenance? TrainingContentProvenance
    {
        get;
        init;
    }

    public LogicalCapabilityTrainingReplayDescriptor? ReplayDescriptor
    {
        get;
        init;
    }

    public LogicalCapabilityTrainingReplayAttestation? ReplayAttestation
    {
        get;
        init;
    }
}

/// <summary>
/// One immutable, already-tokenized optimizer input. The teaching vector and
/// tensor batch are captured exactly once before legal-teacher admission; the
/// logical unit later passes the same internal <see cref="DpoVectorBatch"/>
/// instance to <see cref="DpoTrainer.StepVector"/>.
/// </summary>
public sealed class LogicalDpoPreparedOptimizerInput
{
    private readonly System.Collections.ObjectModel.ReadOnlyCollection<float>
        _teachingVector;

    internal LogicalDpoPreparedOptimizerInput(
        string prompt,
        string chosen,
        string rejected,
        DpoVectorBatch batch)
    {
        Prompt = prompt;
        Chosen = chosen;
        Rejected = rejected;
        Batch = batch;
        _teachingVector = Array.AsReadOnly(batch.TeachingVector);
    }

    public string Prompt { get; }
    public string Chosen { get; }
    public string Rejected { get; }
    public IReadOnlyList<float> TeachingVector => _teachingVector;
    internal DpoVectorBatch Batch { get; }
}

/// <summary>
/// Trusted prepared DPO execution. Raw request rows are never tokenized or
/// copied after admission.
/// </summary>
public sealed class LogicalDpoPreparedExecution
{
    private readonly System.Collections.ObjectModel.ReadOnlyCollection<
        LogicalDpoPreparedOptimizerInput> _optimizerInputs;

    internal LogicalDpoPreparedExecution(
        LogicalDpoPreparedOptimizerInput[] optimizerInputs,
        string materialVersion = DpoExactPairMaterial.LegacyVersion)
    {
        _optimizerInputs = Array.AsReadOnly(optimizerInputs);
        MaterialVersion = materialVersion;
    }

    public string MaterialVersion { get; }
    public IReadOnlyList<LogicalDpoPreparedOptimizerInput> OptimizerInputs =>
        _optimizerInputs;
}

// This immutable contract binds a continued DPO session to exact parent runtime semantics and artifacts.
public sealed class LogicalDpoParentProvenance
{
    private readonly string[] _parentLoraTargetModules;

    public LogicalDpoParentProvenance(
        ModelConfig parentConfig,
        LoraConfig parentLoraConfig,
        string parentTokenizerPath,
        string tokenizerSha256,
        string policyCheckpointSha256,
        string runtimeProvenanceSha256)
    {
        ParentConfig = parentConfig;
        ParentLoraRank = parentLoraConfig.Rank;
        ParentLoraAlpha = parentLoraConfig.Alpha;
        ParentLoraDropout = parentLoraConfig.Dropout;
        _parentLoraTargetModules = parentLoraConfig.TargetModules?.ToArray() ?? [];
        ParentTokenizerPath = parentTokenizerPath;
        TokenizerSha256 = tokenizerSha256;
        PolicyCheckpointSha256 = policyCheckpointSha256;
        RuntimeProvenanceSha256 = runtimeProvenanceSha256;
    }

    public ModelConfig ParentConfig { get; }
    public int ParentLoraRank { get; }
    public float ParentLoraAlpha { get; }
    public float ParentLoraDropout { get; }
    public IReadOnlyList<string> ParentLoraTargetModules => Array.AsReadOnly(_parentLoraTargetModules);
    public LoraConfig ParentLoraConfig => new()
    {
        Rank = ParentLoraRank,
        Alpha = ParentLoraAlpha,
        Dropout = ParentLoraDropout,
        TargetModules = _parentLoraTargetModules.ToArray()
    };
    public string ParentTokenizerPath { get; }
    public string TokenizerSha256 { get; }
    public string PolicyCheckpointSha256 { get; }
    public string RuntimeProvenanceSha256 { get; }
}

public sealed record LogicalDpoAdapterSeedAuthority(
    ulong RootInitializationSeed,
    string PolicyDropoutStreamDomain,
    string ReferenceDropoutStreamDomain);

// This record carries one DPO teaching request through ILogicalUnit.
public sealed record LogicalDpoTrainingRequest(
    LlmModel PolicyModel,
    LlmModel ReferenceModel,
    BpeTokenizer Tokenizer,
    string CheckpointDirectory,
    LogicalDpoPreferenceExample[] Preferences,
    LoraConfig? LoraConfig,
    string? ParentPolicyCheckpointPath = null,
    int MaxStepCount = 1,
    float ConvergenceThreshold = 0.002f,
    int ConvergenceWindowSteps = 25,
    LogicalDpoParentProvenance? ParentProvenance = null,
    LogicalCapabilityTrainingAdmissionCapabilities? AdmissionCapabilities = null,
    Func<DpoTrainerArtifactOwnershipProof, bool>?
        TryAcceptArtifactOwnership = null,
    LogicalDpoMutationAdmissionCapability? MutationAdmission = null,
    AdamWPortableState? OptimizerStateToRestore = null,
    Action<DpoTrainingPublicationBundle>? PersistPublicationBundle = null,
    Action<DpoTrainingPublicationBundle>? BeforePolicyFinalMove = null,
    Action? PublicationFailed = null)
{
    public LogicalDpoPreparedExecution? PreparedExecution { get; init; }
    public string MaterialVersion { get; init; } = DpoExactPairMaterial.LegacyVersion;
    public DpoContinuationProof? ContinuationProof { get; init; }
    public LogicalDpoAdapterSeedAuthority? AdapterSeedAuthority { get; init; }
    public bool SeededTopologyPreparedByRepositoryStart { get; init; }
    public LlmModelRandomStateSnapshot? RandomStateToRestore { get; init; }
    public DpoRandomStateColdStartAuthorization?
        RandomStateColdStartAuthorization { get; init; }
}

public sealed record LogicalDpoTrainingOptimizerStateIdentity(
    int FormatVersion,
    int Step,
    string Sha256,
    long ByteLength);

// This record reports one DPO teaching run.
public sealed record LogicalDpoTrainingResult(
    int PreferenceCount,
    int DpoStepCount,
    int LoraAdapterCount,
    // FIX-REPORT-READER-01: потеря и точность последнего шага НЕОБЯЗАТЕЛЬНЫ, и
    // отсутствие здесь означает «величина не измерена», а не «величина равна
    // нулю». Это разные факты, и прежний обязательный `float` их не различал:
    // при отсутствии квитанции шага отчёт получал ноль, который читается как
    // измеренный результат. Для потери такой ноль врёт в ЛЕСТНУЮ сторону —
    // нулевая потеря есть наилучший возможный исход обучения; для точности в
    // ОБРАТНУЮ — нулевая точность читается как полный провал. Обе величины при
    // отсутствии ложны, поэтому разведены вместе, одним решением.
    // Необязательное поле само служит признаком присутствия: отдельный флаг
    // рядом с обязательным числом выразил бы то же двумя полями вместо одного
    // и оставил бы число, которое всё равно надо чем-то заполнять. У соседнего
    // артефакта обучения отдельный признак присутствия нужен по другой причине:
    // там поле — путь к файлу, а у строки нет значения «не измерено»,
    // отличимого от пустой строки. У числа такое значение есть, и в этом же
    // коде оно уже применяется — полем немасштабированной потери квитанции
    // шага, которое при отсутствии не записывается вовсе.
    float? LastLoss,
    float? LastAccuracy,
    long TrainingTimeMs,
    string[] MetricFilePaths,
    string[] IngredientIds,
    AdamWPortableState? PostStepOptimizerState = null,
    LogicalDpoTrainingOptimizerStateIdentity?
        PostStepOptimizerStateIdentity = null,
    DpoTrainingOptimizerSettings? OptimizerSettings = null,
    LlmModelRandomStateSnapshot? PostStepRandomState = null);

internal interface ILogicalDpoPolicyLeaseObserver
{
    void OnWaitStarted(LlmModel policy);
    void OnWaitBlocked(LlmModel policy);
    Task OnLeaseAcquiredAsync(LlmModel policy, CancellationToken cancellationToken);
}

internal interface ILogicalDpoParentArtifactObserver
{
    void OnParentArtifactsVerified();
    void OnParentCallerIdentityRevalidated();
}

internal static class LogicalDpoExceptionBoundary
{
    internal static bool ShouldNormalize(Exception exception)
        => SnapshotExceptionGraphClassifier.IsNonFatal(exception);

    internal static bool TryGetContinuationRestoreFailureCode(
        Exception exception,
        out string? errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current.Message.StartsWith(
                    DpoTrainer.ContinuationRestoreRollbackFailedErrorCode,
                    StringComparison.Ordinal))
            {
                errorCode = DpoTrainer.ContinuationRestoreRollbackFailedErrorCode;
                return true;
            }

            if (current.Message.StartsWith(
                    DpoTrainer.ContinuationRestoreOptimizerRollbackFailedErrorCode,
                    StringComparison.Ordinal))
            {
                errorCode = DpoTrainer.ContinuationRestoreOptimizerRollbackFailedErrorCode;
                return true;
            }

            if (current.Message.StartsWith(
                    DpoTrainer.ContinuationRestoreRandomRollbackFailedErrorCode,
                    StringComparison.Ordinal))
            {
                errorCode = DpoTrainer.ContinuationRestoreRandomRollbackFailedErrorCode;
                return true;
            }

            if (current is AggregateException aggregate)
            {
                for (var index = aggregate.InnerExceptions.Count - 1; index >= 0; index--)
                    pending.Push(aggregate.InnerExceptions[index]);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        errorCode = null;
        return false;
    }
}

// This teaching unit connects LoRA and DPO training to the logical-unit interface.
public sealed class LogicalDpoTrainingUnit : ILogicalUnit<LogicalDpoTrainingRequest, LogicalDpoTrainingResult>
{
    public const int MinimumSupportedMaxStepCount = 1;
    // Reuse the established production teaching budget as the logical-unit safety ceiling.
    public const int MaximumSupportedMaxStepCount = 5;
    public const string MaxStepCountOutOfRangeErrorCode = "DPO_MAX_STEP_COUNT_OUT_OF_RANGE";
    public const int MaximumPreferenceCount =
        LogicalCapabilityTrainingAdmissionBatch.MaximumRowCount;
    public const int MaximumPreferenceTextUtf8Bytes = 1_048_576;
    public const long MaximumAggregatePreferenceTextUtf8Bytes = 4 * 1_048_576;
    public const int MaximumTeachingVectorLength = 65_536;
    public const long MaximumAggregateTeachingVectorLength = 262_144;
    public const string PreferenceBoundsInvalidErrorCode =
        "DPO_PREFERENCE_BOUNDS_INVALID";

    private static readonly ConditionalWeakTable<LlmModel, SemaphoreSlim> PolicyTrainingLeases = new();
    private readonly ILogicalDpoPolicyLeaseObserver? _leaseObserver;
    private readonly ILogicalDpoParentArtifactObserver? _parentArtifactObserver;
    private static readonly AsyncLocal<Func<bool>?>
        ForceReferenceValidationFailure = new();
    private static readonly AsyncLocal<Action<
        IReadOnlyList<LogicalDpoPreparedOptimizerInput>,
        IReadOnlyList<DpoVectorBatch>>?> OptimizerMutationSinkObserver = new();
    private static readonly AsyncLocal<Action<LogicalDpoPreferenceExample>?>
        BatchTokenizationObserver = new();

    internal static Func<bool>?
        ForceReferenceValidationFailureForTesting
    {
        get => ForceReferenceValidationFailure.Value;
        set => ForceReferenceValidationFailure.Value = value;
    }

    internal static Action<
        IReadOnlyList<LogicalDpoPreparedOptimizerInput>,
        IReadOnlyList<DpoVectorBatch>>?
        OptimizerMutationSinkObserverForTesting
    {
        get => OptimizerMutationSinkObserver.Value;
        set => OptimizerMutationSinkObserver.Value = value;
    }

    internal static Action<LogicalDpoPreferenceExample>?
        BatchTokenizationObserverForTesting
    {
        get => BatchTokenizationObserver.Value;
        set => BatchTokenizationObserver.Value = value;
    }

    public LogicalDpoTrainingUnit()
    {
    }

    internal LogicalDpoTrainingUnit(ILogicalDpoPolicyLeaseObserver leaseObserver)
    {
        _leaseObserver = leaseObserver ?? throw new ArgumentNullException(nameof(leaseObserver));
    }

    internal LogicalDpoTrainingUnit(ILogicalDpoParentArtifactObserver parentArtifactObserver)
    {
        _parentArtifactObserver = parentArtifactObserver
            ?? throw new ArgumentNullException(nameof(parentArtifactObserver));
    }

    // This id identifies the first DPO teaching logical unit.
    public const string Id = "unit-logical-dpo-training-v1";

    // This role names the model-teaching responsibility.
    public const string Role = "logical-dpo-training";

    // This domain tag scopes the unit to model teaching.
    public const string DomainTag = "logical-learning";

    public static OperationResult<int> ValidateMaxStepCount(int maxStepCount)
    {
        if (maxStepCount is < MinimumSupportedMaxStepCount or > MaximumSupportedMaxStepCount)
        {
            return OperationResult<int>.Fail(
                "DPO max step count is outside the supported logical unit range.",
                MaxStepCountOutOfRangeErrorCode);
        }

        return OperationResult<int>.Ok(maxStepCount);
    }

    // Сообщает, может ли эта настройка вообще сократить прогон по сходимости.
    // Монитор впервые отвечает "пора остановиться" только на третьем
    // наблюдении, а цикл обучения ниже спрашивает его раз в
    // `convergenceWindowSteps` шагов оптимизатора, поэтому самый ранний
    // возможный выход — шаг 3 * интервал. Когда бюджет шагов не больше трёх
    // интервалов наблюдения, этот выход либо совпадает с последним шагом, либо
    // не достигается вовсе, и цикл всё равно проходит весь бюджет.
    // Признак считается только из настройки и намеренно НЕ смотрит, создан ли
    // объект монитора: иначе он молча исчез бы вместе с монитором при нулевом
    // пороге, и мы своими руками сделали бы новое молчание.
    // Здесь это сильнее, чем у соседнего потребителя того же механизма: бюджет
    // шагов этой единицы жёстко ограничен `MaximumSupportedMaxStepCount` (5),
    // поэтому при интервале наблюдения от 2 и выше проверка сходимости мертва
    // при любом допустимом бюджете, а не только в краевой настройке.
    // Порог 0 выключает проверку намеренно и недостижимостью не считается — для
    // него есть свой отдельный признак ниже; интервал меньше 1 уже отклонён
    // проверкой настроек сходимости в ExecuteAsync.
    internal static bool ConvergenceDetectionIsUnreachable(
        int maxStepCount,
        int convergenceWindowSteps,
        float convergenceThreshold)
        => convergenceThreshold != 0f &&
           convergenceWindowSteps >= 1 &&
           maxStepCount <= 3L * convergenceWindowSteps;

    // Сообщает, выключена ли проверка сходимости нулевым порогом. Это отдельная
    // болезнь, а не частный случай недостижимости: сравнение "изменение меньше
    // нуля" ложно для любой пары наблюдений, включая равные потери, потому что
    // 0 < 0 не выполняется. Поэтому при нулевом пороге остановка невозможна по
    // арифметике, независимо от бюджета шагов и интервала наблюдения.
    internal static bool ConvergenceDetectionIsDisabledByZeroThreshold(
        float convergenceThreshold)
        => convergenceThreshold == 0f;

    // Выдаёт ровно один человекочитаемый след для настройки, которая выглядит
    // настроенной на раннюю остановку, но остановиться не может, и молчит для
    // работоспособной настройки. Отказа настройке не выдаём: заведомо короткие
    // и намеренно выключенные конфигурации должны продолжать работать как
    // раньше, но перестать делать это молча.
    // Порядок важен и задан отдельным требованием: при нулевом пороге говорит
    // только признак нуля, потому что сообщать про недостижимость там, где
    // сходимость вообще выключена, значит шуметь о двух разных вещах по одной
    // настройке.
    internal static void TraceConvergenceDetectionDiagnostics(
        int maxStepCount,
        int convergenceWindowSteps,
        float convergenceThreshold)
    {
        // Нулевой порог: говорит только этот признак, и дальше мы не идём.
        if (ConvergenceDetectionIsDisabledByZeroThreshold(convergenceThreshold))
        {
            // Один след с бюджетом шагов: видно, что вопрос о сходимости в этом
            // прогоне не задаётся ни разу, а причина — именно нулевой порог.
            System.Diagnostics.Trace.TraceWarning(
                "CapabilityTraining DPO convergence detection is disabled by a " +
                "zero threshold for this run: the comparison \"loss change below " +
                "zero\" is never satisfied, so no convergence monitor is created, " +
                "the convergence question is never asked, and the loop always " +
                "runs the full step budget of {0} steps. Zero is an accepted " +
                "setting: it either means \"do not check convergence\" on purpose " +
                "or it is a mistake.",
                maxStepCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return;
        }

        // Достижимая проверка не нуждается ни в каком сигнале.
        if (!ConvergenceDetectionIsUnreachable(
                maxStepCount,
                convergenceWindowSteps,
                convergenceThreshold))
        {
            return;
        }

        // Один след с обоими числами настройки и с жёстким потолком бюджета, по
        // которым видно, почему ранняя остановка в этом прогоне невозможна и
        // почему интервал от 2 и выше мертв здесь всегда.
        System.Diagnostics.Trace.TraceWarning(
            "CapabilityTraining DPO convergence detection is unreachable for this " +
            "run: step budget {0} does not exceed three observation intervals of " +
            "{1} steps (threshold {2}); the loop always runs the full step budget. " +
            "The step budget of this unit is capped at {3} steps, so every " +
            "observation interval of 2 or more steps leaves convergence detection " +
            "dead for every allowed budget.",
            maxStepCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            convergenceWindowSteps.ToString(System.Globalization.CultureInfo.InvariantCulture),
            convergenceThreshold.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
            MaximumSupportedMaxStepCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    // Execute one DPO teaching pass.
    public async Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecuteAsync(
        LogicalUnitRequest<LogicalDpoTrainingRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("Cancelled.", "CANCELLED");
        if (request?.Input is null)
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("DPO training request is required.", "DPO_TRAINING_REQUEST_REQUIRED");
        if (request.Input.PolicyModel is null || request.Input.ReferenceModel is null)
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("Policy and reference models are required.", "DPO_MODELS_REQUIRED");
        if (request.Input.AdmissionCapabilities is null)
        {
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                "A server-issued logical capability training admission bundle is required.",
                LogicalCapabilityTrainingAdmissionCapabilities.RequiredErrorCode);
        }

        var preparedExecution = request.Input.PreparedExecution;
        if (preparedExecution is null ||
            preparedExecution.OptimizerInputs.Count == 0)
        {
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                "An exact prepared DPO optimizer execution is required.",
                "DPO_PREPARED_EXECUTION_REQUIRED");
        }

        if (!float.IsFinite(request.Input.ConvergenceThreshold) ||
            request.Input.ConvergenceThreshold < 0f ||
            request.Input.ConvergenceWindowSteps < 1)
        {
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                "DPO convergence settings are invalid.",
                "DPO_CONVERGENCE_SETTINGS_INVALID");
        }

        var maxStepCountResult = ValidateMaxStepCount(request.Input.MaxStepCount);
        if (!maxStepCountResult.IsSuccess)
        {
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                maxStepCountResult.ErrorMessage,
                maxStepCountResult.ErrorCode);
        }

        var policyLease = PolicyTrainingLeases.GetValue(
            request.Input.PolicyModel,
            static _ => new SemaphoreSlim(1, 1));
        _leaseObserver?.OnWaitStarted(request.Input.PolicyModel);
        try
        {
            var waitTask = policyLease.WaitAsync(cancellationToken);
            if (!waitTask.IsCompleted)
                _leaseObserver?.OnWaitBlocked(request.Input.PolicyModel);
            await waitTask;
        }
        catch (OperationCanceledException)
        {
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("Cancelled.", "CANCELLED");
        }

        try
        {
            if (_leaseObserver is not null)
                await _leaseObserver.OnLeaseAcquiredAsync(request.Input.PolicyModel, cancellationToken);
            return await ExecuteWithExclusivePolicyLeaseAsync(
                request,
                preparedExecution,
                maxStepCountResult.Value,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("Cancelled.", "CANCELLED");
        }
        finally
        {
            policyLease.Release();
        }
    }

    private async Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecuteWithExclusivePolicyLeaseAsync(
        LogicalUnitRequest<LogicalDpoTrainingRequest> request,
        LogicalDpoPreparedExecution preparedExecution,
        int validatedMaxStepCount,
        CancellationToken cancellationToken)
    {
        PreparedContinuationTransaction? preparation = null;
        try
        {
            var result = await ExecuteCore();
            var failures = preparation?.Rollback();
            return failures is { Count: > 0 }
                ? PreparationRollbackFailure($"{result.ErrorCode}: {result.ErrorMessage}", failures)
                : result;
        }
        catch (Exception exception)
        {
            // Cleanup applies to every armed exception graph, not only graphs
            // eligible for normalization. Wrapped cancellation/fatal causes must
            // still propagate with their identity/semantics intact.
            var failures = preparation?.Rollback();
            if (failures is { Count: > 0 })
            {
                // A direct cancellation may use the failure envelope, but must
                // not hide a fatal or otherwise non-normalizable inner graph.
                if ((exception is OperationCanceledException
                        ? exception.InnerException is null || LogicalDpoExceptionBoundary.ShouldNormalize(exception.InnerException)
                        : LogicalDpoExceptionBoundary.ShouldNormalize(exception)) &&
                    failures.All(LogicalDpoExceptionBoundary.ShouldNormalize))
                    return PreparationRollbackFailure(exception.ToString(), failures);
                throw new AggregateException("DPO_PREPARATION_ROLLBACK_FAILED",
                    new[] { exception }.Concat(failures));
            }
            throw;
        }

        Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecuteCore()
        {
        // Return cancellation through the shared envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("Cancelled.", "CANCELLED"));

        // Validate request shape.
        if (request.Input is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("DPO training request is required.", "DPO_TRAINING_REQUEST_REQUIRED"));
        if (request.Input.PolicyModel is null || request.Input.ReferenceModel is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("Policy and reference models are required.", "DPO_MODELS_REQUIRED"));
        if (request.Input.Tokenizer is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("Tokenizer is required.", "TOKENIZER_REQUIRED"));

        // Capture and validate exact material before LoRA preparation permission,
        // RNG restore, forward or files. Later callbacks cannot rewrite targets.
        DpoVectorBatch[] executionBatches;
        try
        {
            if (preparedExecution.MaterialVersion != request.Input.MaterialVersion ||
                request.Input.MaterialVersion is not (DpoExactPairMaterial.LegacyVersion or DpoExactPairMaterial.Version))
                throw new DpoMutationAdmissionException("DPO material profile does not match prepared execution.", DpoExactPairMaterial.InvalidErrorCode);
            var tokenizerSha256 = request.Input.MaterialVersion == DpoExactPairMaterial.Version
                ? request.Input.Tokenizer.ComputeStateSha256() : null;
            executionBatches = preparedExecution.OptimizerInputs.Select(input =>
                request.Input.MaterialVersion == DpoExactPairMaterial.Version
                    ? new DpoVectorBatch(DpoExactPairMaterial.CaptureAndValidate(input.Batch.Batch,
                        Math.Min(request.Input.PolicyModel.Config.VocabSize, request.Input.ReferenceModel.Config.VocabSize),
                        Math.Min(request.Input.PolicyModel.Config.ContextLen, request.Input.ReferenceModel.Config.ContextLen),
                        tokenizerSha256, request.Input.MaterialVersion), input.Batch.TeachingVector.ToArray(), input.Batch.Weight)
                    : input.Batch).ToArray();
            if (request.Input.MaterialVersion == DpoExactPairMaterial.Version)
            {
                if (request.Input.OptimizerStateToRestore is not null)
                {
                    if (request.Input.ContinuationProof is null) DpoExecutionProfile.Mismatch();
                    request.Input.ContinuationProof!.RequireMatches(request.Input.OptimizerStateToRestore, request.Input.RandomStateToRestore);
                }
                else if (request.Input.ContinuationProof is not null) DpoExecutionProfile.Mismatch();
                DpoProjectLoraRecipient.RequirePreparedPolicy(request.Input.PolicyModel, request.Input.LoraConfig ?? new LoraConfig());
            }
        }
        catch (DpoMutationAdmissionException exception)
        {
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(exception.Message, exception.ErrorCode));
        }

        var seedAuthority = request.Input.AdapterSeedAuthority;
        LoraConfig? seededCanonicalLoraConfig = null;
        var seededExpectedPolicyAdapterCount = 0;
        var seededExpectedReferenceAdapterCount = 0;
        if (seedAuthority is not null)
        {
            if (!request.Input.SeededTopologyPreparedByRepositoryStart &&
                (!string.IsNullOrWhiteSpace(request.Input.ParentPolicyCheckpointPath) ||
                 request.Input.ParentProvenance is not null))
            {
                return Task.FromResult(OperationResult<
                    LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    "A direct-fresh seeded DPO request cannot also carry a parent.",
                    "DPO_SEEDED_CONTINUATION_UNSUPPORTED"));
            }
            if (string.IsNullOrWhiteSpace(request.Input.CheckpointDirectory))
            {
                return Task.FromResult(OperationResult<
                    LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    "DPO checkpoint directory is required.",
                    "DPO_CHECKPOINT_DIRECTORY_REQUIRED"));
            }

            try
            {
                seededCanonicalLoraConfig = LoraApplicator.ValidateAndCanonicalize(
                    request.Input.LoraConfig ?? new LoraConfig());
                if (seededCanonicalLoraConfig.TargetModules.Length == 0)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "DPO model topology could not be prepared.",
                        "DPO_LORA_APPLICATION_FAILED"));
                }

                _ = LoraAdapterRandomSeedDerivation.Derive(
                    seedAuthority.RootInitializationSeed,
                    blockIndex: 0,
                    canonicalProjection: "q_proj",
                    dropoutStreamDomain:
                        seedAuthority.PolicyDropoutStreamDomain);
                _ = LoraAdapterRandomSeedDerivation.Derive(
                    seedAuthority.RootInitializationSeed,
                    blockIndex: 0,
                    canonicalProjection: "q_proj",
                    dropoutStreamDomain:
                        seedAuthority.ReferenceDropoutStreamDomain);
                seededExpectedPolicyAdapterCount = checked(
                    request.Input.PolicyModel.Blocks.Count *
                    seededCanonicalLoraConfig.TargetModules.Length);
                seededExpectedReferenceAdapterCount = checked(
                    request.Input.ReferenceModel.Blocks.Count *
                    seededCanonicalLoraConfig.TargetModules.Length);
                if (seededExpectedPolicyAdapterCount == 0 ||
                    seededExpectedReferenceAdapterCount == 0)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "DPO model topology could not be prepared.",
                        "DPO_LORA_APPLICATION_FAILED"));
                }
            }
            catch (Exception exception)
                when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
            {
                return Task.FromResult(OperationResult<
                    LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    "DPO model topology could not be prepared.",
                    "DPO_LORA_APPLICATION_FAILED"));
            }
        }

        var effectiveLoraConfig = seededCanonicalLoraConfig
            ?? request.Input.LoraConfig
            ?? new LoraConfig();
        var randomStateRequired = seedAuthority is not null &&
            effectiveLoraConfig.Dropout > 0f &&
            effectiveLoraConfig.TargetModules.Length > 0;
        LlmModelRandomStateIdentity? expectedRandomStateIdentity = null;
        if (randomStateRequired && seedAuthority is not null)
        {
            var authority = new LoraPairRandomSeedAuthority(
                seedAuthority.RootInitializationSeed,
                seedAuthority.PolicyDropoutStreamDomain,
                seedAuthority.ReferenceDropoutStreamDomain);
            expectedRandomStateIdentity = new(
                LoraRandomStateIdentity.ComputeTopologySha256(
                    request.Input.PolicyModel.Config,
                    effectiveLoraConfig,
                    authority),
                LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
                    effectiveLoraConfig,
                    authority));
        }
        var hasParent = !string.IsNullOrWhiteSpace(
                request.Input.ParentPolicyCheckpointPath) ||
            request.Input.ParentProvenance is not null ||
            request.Input.SeededTopologyPreparedByRepositoryStart;
        if (randomStateRequired)
        {
            if (request.Input.RandomStateToRestore is null)
            {
                // The exact server-owned initial route can already have its
                // seeded topology installed by START. Its explicit cold grant
                // does not authorize a warm optimizer or parent-provenance reset.
                var initialExactStart = request.Input.MaterialVersion == DpoExactPairMaterial.Version &&
                    request.Input.OptimizerStateToRestore is null && request.Input.ContinuationProof is null &&
                    string.IsNullOrWhiteSpace(request.Input.ParentPolicyCheckpointPath) && request.Input.ParentProvenance is null &&
                    request.Input.RandomStateColdStartAuthorization is { IsWellFormed: true } cold &&
                    cold.RootSeed == seedAuthority!.RootInitializationSeed;
                if ((hasParent || request.Input.SeededTopologyPreparedByRepositoryStart) && !initialExactStart)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "A dropout-enabled continuation requires its persisted RNG state.",
                        "DPO_RANDOM_STATE_REQUIRED_FOR_CONTINUATION"));
                }
                var coldStart = request.Input.RandomStateColdStartAuthorization;
                if (coldStart is null || !coldStart.IsWellFormed ||
                    coldStart.RootSeed != seedAuthority!.RootInitializationSeed)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "A deterministic RNG cold-start authorization is required.",
                        "DPO_RANDOM_STATE_COLD_START_AUTHORIZATION_REQUIRED"));
                }
            }
            else if (request.Input.RandomStateColdStartAuthorization is not null)
            {
                return Task.FromResult(OperationResult<
                    LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    "RNG cold-start authorization is forbidden for continuation.",
                    "DPO_RANDOM_STATE_COLD_START_AUTHORIZATION_INVALID"));
            }
        }
        else if (request.Input.RandomStateToRestore is not null ||
                 request.Input.RandomStateColdStartAuthorization is not null)
        {
            return Task.FromResult(OperationResult<
                LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                "RNG state was supplied for a topology that does not require it.",
                "DPO_RANDOM_STATE_UNEXPECTED"));
        }
        if (seedAuthority is null &&
            effectiveLoraConfig.Dropout > 0f &&
            effectiveLoraConfig.TargetModules.Length > 0)
        {
            return Task.FromResult(OperationResult<
                LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                "Dropout-enabled DPO requires deterministic adapter authority.",
                    "DPO_RANDOM_STATE_NONDETERMINISTIC_ADAPTER"));
        }

        if (request.Input.RandomStateToRestore is not null)
        {
            try
            {
                var randomState = request.Input.RandomStateToRestore;
                if (expectedRandomStateIdentity is null ||
                    randomState.FormatVersion !=
                        LlmModelRandomStateSnapshot.CurrentFormatVersion ||
                    !StringComparer.Ordinal.Equals(
                        randomState.Algorithm,
                        LlmModelRandomStateSnapshot.CurrentAlgorithm) ||
                    randomState.RandomStateStep <= 0 ||
                    !StringComparer.Ordinal.Equals(
                        randomState.TopologySha256,
                        expectedRandomStateIdentity.TopologySha256) ||
                    !StringComparer.Ordinal.Equals(
                        randomState.LoraConfigurationSha256,
                        expectedRandomStateIdentity.LoraConfigurationSha256) ||
                    request.Input.OptimizerStateToRestore is null ||
                    randomState.RandomStateStep !=
                        request.Input.OptimizerStateToRestore.Step)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "The supplied DPO random state does not match the admitted optimizer step, topology, or LoRA configuration.",
                        "DPO_RANDOM_STATE_IDENTITY_MISMATCH"));
                }
            }
            catch (Exception exception)
                when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
            {
                return Task.FromResult(OperationResult<
                    LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    "The supplied DPO random state identity is malformed.",
                    "DPO_RANDOM_STATE_IDENTITY_MISMATCH"));
            }
        }

        var parentProvenanceValidation = OpenAndValidateParentArtifacts(request.Input);
        if (!parentProvenanceValidation.IsSuccess)
        {
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                parentProvenanceValidation.ErrorMessage,
                parentProvenanceValidation.ErrorCode));
        }
        using var parentArtifacts = parentProvenanceValidation.Value;

        if (parentArtifacts is not null)
        {
            try
            {
                _parentArtifactObserver?.OnParentArtifactsVerified();
                if (!parentArtifacts.PolicyCheckpoint.StillMatches()
                    || !parentArtifacts.Tokenizer.StillMatches()
                    || !parentArtifacts.MatchesCurrentCallerIdentity(
                        request.Input.LoraConfig ?? new LoraConfig(),
                        request.Input.Tokenizer))
                {
                    return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                        "DPO_PARENT_PROVENANCE_INVALID"));
                }

                // This internal seam is intentionally after caller-identity
                // revalidation. Continued execution below must remain isolated
                // from any subsequent caller-owned object mutation.
                _parentArtifactObserver?.OnParentCallerIdentityRevalidated();
            }
            catch (Exception exception)
                when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
            {
                return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                    "DPO_PARENT_PROVENANCE_INVALID"));
            }
        }

        // A continued session executes only from the retained, validated identity.
        // First-generation training has no parent provenance and keeps its existing
        // caller-owned configuration behavior.
        var loraConfig = seededCanonicalLoraConfig
                         ?? parentArtifacts?.CanonicalLoraConfig
                         ?? request.Input.LoraConfig
                         ?? new LoraConfig();
        IReadOnlyList<LoraLinear> adapters;
        OperationResult<FrozenReferenceSnapshot> referenceCapture;
        DpoTrainer trainer;
        {
            var preparationPermit = AcquireMutationPermit(request.Input);
            if (!preparationPermit.IsSuccess || preparationPermit.Value is null)
            {
                return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    preparationPermit.ErrorMessage,
                    preparationPermit.ErrorCode));
            }

            using var permit = preparationPermit.Value;
            if (seedAuthority is not null)
            {
                if (File.Exists(request.Input.CheckpointDirectory))
                {
                    return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "DPO trainer could not be initialized.", "DPO_TRAINER_INITIALIZATION_FAILED"));
                }
                var compatibility = ValidateSeededPolicyReferenceCompatibility(
                    request.Input.PolicyModel,
                    request.Input.ReferenceModel,
                    request.Input.SeededTopologyPreparedByRepositoryStart);
                if (!compatibility.IsSuccess)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        compatibility.ErrorMessage,
                        compatibility.ErrorCode));
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "Cancelled.",
                        "CANCELLED"));
                }

                FrozenReferenceSnapshot? lockedReferenceSnapshot = null;
                try
                {
                    IReadOnlyList<LoraLinear> pairPolicyAdapters;
                    IReadOnlyList<LoraLinear> pairReferenceAdapters;
                    if (request.Input.SeededTopologyPreparedByRepositoryStart)
                    {
                        var resolver = new Func<int, string, LoraAdapterRandomSeeds>(
                            (blockIndex, projection) =>
                                LoraAdapterRandomSeedDerivation.Derive(
                                    seedAuthority.RootInitializationSeed,
                                    blockIndex,
                                    projection,
                                    seedAuthority.PolicyDropoutStreamDomain));
                        pairPolicyAdapters = LoraApplicator.ValidateSeededApplicationProvenance(
                            request.Input.PolicyModel,
                            loraConfig,
                            resolver);
                        pairReferenceAdapters = LoraApplicator.ValidateSeededApplicationProvenance(
                            request.Input.ReferenceModel,
                            loraConfig,
                            (blockIndex, projection) =>
                                LoraAdapterRandomSeedDerivation.Derive(
                                    seedAuthority.RootInitializationSeed,
                                    blockIndex,
                                    projection,
                                    seedAuthority.ReferenceDropoutStreamDomain));
                        if (!FullStatesMatch(request.Input.PolicyModel, request.Input.ReferenceModel))
                            throw new SeededDpoPreparationException(
                                "The prepared DPO reference does not match the full step-zero policy.",
                                "DPO_REFERENCE_PROVENANCE_MISMATCH");
                        // Capture the actual live pair, not the incoming continuation cursor.
                        // This transaction owns only preparation, never an applied step.
                        preparation = new PreparedContinuationTransaction(
                            request.Input, loraConfig, seedAuthority,
                            pairPolicyAdapters, pairReferenceAdapters);
                        var preparedCapture = CaptureFrozenReferenceState(
                            request.Input.PolicyModel,
                            request.Input.ReferenceModel,
                            copyPolicyState: false);
                        if (!preparedCapture.IsSuccess || preparedCapture.Value is null)
                            throw new SeededDpoPreparationException(
                                preparedCapture.ErrorMessage,
                                preparedCapture.ErrorCode);
                        lockedReferenceSnapshot = preparedCapture.Value;
                    }
                    else
                    {
                        var pairApplication = LoraApplicator.ApplyFreshSeededPair(
                            request.Input.PolicyModel,
                            request.Input.ReferenceModel,
                            loraConfig,
                            seedAuthority.RootInitializationSeed,
                            seedAuthority.PolicyDropoutStreamDomain,
                            seedAuthority.ReferenceDropoutStreamDomain,
                            beforeMutationUnderPairLock: () =>
                            {
                                var lockedCompatibility =
                                    ValidateSeededPolicyReferenceCompatibility(
                                        request.Input.PolicyModel,
                                        request.Input.ReferenceModel);
                                if (!lockedCompatibility.IsSuccess)
                                {
                                    throw new SeededDpoPreparationException(
                                        lockedCompatibility.ErrorMessage,
                                        lockedCompatibility.ErrorCode);
                                }
                            },
                            afterCommitUnderPairLock: _ =>
                            {
                                var lockedCapture = CaptureFrozenReferenceState(
                                    request.Input.PolicyModel,
                                    request.Input.ReferenceModel,
                                    copyPolicyState: true);
                                if (!lockedCapture.IsSuccess)
                                {
                                    throw new SeededDpoPreparationException(
                                        lockedCapture.ErrorMessage,
                                        lockedCapture.ErrorCode);
                                }
                                if (lockedCapture.Value is null)
                                {
                                    throw new SeededDpoPreparationException(
                                        "The frozen DPO reference could not be verified against the full step-zero policy.",
                                        "DPO_REFERENCE_PROVENANCE_MISMATCH");
                                }

                                lockedReferenceSnapshot = lockedCapture.Value;
                            });
                        pairPolicyAdapters = pairApplication.FirstModelAdapters;
                        pairReferenceAdapters = pairApplication.SecondModelAdapters;
                    }
                    if (pairPolicyAdapters.Count !=
                            seededExpectedPolicyAdapterCount ||
                        pairReferenceAdapters.Count !=
                            seededExpectedReferenceAdapterCount ||
                        pairPolicyAdapters.Count == 0 ||
                        pairReferenceAdapters.Count == 0)
                    {
                        return Task.FromResult(OperationResult<
                            LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                            "DPO model topology could not be prepared.",
                            "DPO_LORA_APPLICATION_FAILED"));
                    }
                    if (lockedReferenceSnapshot is null)
                    {
                        throw new SeededDpoPreparationException(
                            "The frozen DPO reference could not be verified against the full step-zero policy.",
                            "DPO_REFERENCE_PROVENANCE_MISMATCH");
                    }

                    adapters = pairPolicyAdapters;
                    referenceCapture = OperationResult<FrozenReferenceSnapshot>.Ok(
                        lockedReferenceSnapshot);
                }
                catch (SeededDpoPreparationException exception)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        exception.Message,
                        exception.ErrorCode));
                }
                catch (Exception exception)
                    when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
                {
                    return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "DPO model topology could not be prepared.",
                        "DPO_LORA_APPLICATION_FAILED"));
                }
            }
            else
            {
                try
                {
                    // The policy and reference must expose the same full
                    // base-plus-adapter topology before parent loading or capture.
                    adapters = LoraApplicator.Apply(
                        request.Input.PolicyModel,
                        loraConfig);
                    _ = LoraApplicator.Apply(
                        request.Input.ReferenceModel,
                        loraConfig);
                }
                catch (Exception exception)
                    when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
                {
                    return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "DPO model topology could not be prepared.",
                        "DPO_LORA_APPLICATION_FAILED"));
                }

                var hasParentCheckpoint = !string.IsNullOrWhiteSpace(
                    request.Input.ParentPolicyCheckpointPath);
                if (hasParentCheckpoint)
                {
                    try
                    {
                        parentArtifacts!.PolicyCheckpoint.Reset();
                        request.Input.PolicyModel.LoadCheckpoint(parentArtifacts.PolicyCheckpoint.Stream);
                        parentArtifacts.PolicyCheckpoint.Reset();
                        request.Input.ReferenceModel.LoadCheckpoint(parentArtifacts.PolicyCheckpoint.Stream);
                    }
                    catch (Exception exception)
                        when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
                    {
                        return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                            "Parent policy checkpoint could not be loaded into the DPO models.",
                            "PARENT_POLICY_CHECKPOINT_LOAD_FAILED"));
                    }
                }

                referenceCapture = CaptureFrozenReferenceState(
                    request.Input.PolicyModel,
                    request.Input.ReferenceModel,
                    copyPolicyState: !hasParentCheckpoint);
                if (!referenceCapture.IsSuccess)
                {
                    return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        referenceCapture.ErrorMessage,
                        referenceCapture.ErrorCode));
                }

            }
        }

        LlmModelRandomStateSnapshot? postStepRandomState = null;
        var trainerResult = CreateDpoTrainer(
            request.Input,
            loraConfig,
            seedAuthority,
            randomStateRequired,
            bundle => postStepRandomState = bundle.RandomState,
            adapters, executionBatches[0].Batch);
        if (!trainerResult.IsSuccess || trainerResult.Value is null)
        {
            return Task.FromResult(OperationResult<
                LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                trainerResult.ErrorMessage,
                trainerResult.ErrorCode));
        }
        trainer = trainerResult.Value;
        preparation?.AttachTrainer(trainer);

        try
        {
            if (!trainer.TryRestoreContinuationState(
                    request.Input.RandomStateToRestore,
                    request.Input.OptimizerStateToRestore))
            {
                return Task.FromResult(OperationResult<
                    LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    "The supplied DPO optimizer state is incompatible with the policy model.",
                    "DPO_OPTIMIZER_STATE_RESTORE_FAILED"));
            }
        }
        catch (Exception exception)
            when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
        {
            if (LogicalDpoExceptionBoundary.TryGetContinuationRestoreFailureCode(
                    exception,
                    out var rollbackFailureCode))
            {
                return Task.FromResult(OperationResult<
                    LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    "DPO continuation state rollback failed after restore failure.",
                    rollbackFailureCode!));
            }

            return Task.FromResult(OperationResult<
                LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                "The supplied DPO random state is incompatible with the policy model.",
                "DPO_RANDOM_STATE_RESTORE_FAILED"));
        }

        // Execute one batched DPO optimizer update. Each preference stays short, while
        // gradients are averaged and applied once for the whole same-domain row.
        var started = DateTimeOffset.UtcNow;
        var preparedInputs = preparedExecution.OptimizerInputs;
        var batches = executionBatches;
        DPO.Models.DpoMetrics? lastMetrics = null;
        var windowSteps = Math.Max(1, request.Input.ConvergenceWindowSteps);
        // Нулевой порог выключает проверку сходимости: сравнение "изменение
        // меньше нуля" ложно для любой пары наблюдений, поэтому монитор при
        // нуле не способен ответить "да" ни разу. Не создаём его вовсе — ровно
        // так же, как сосед по этому же механизму в
        // DesignConversionDpoPolicyCheckpointBridge.ExecuteDpoStepLoop, чтобы
        // два потребителя одного монитора говорили о нуле одно и то же.
        // Правка наблюдаемо нейтральна: сообщаемое число шагов берётся из
        // локальной `stepsTaken` ниже, собственный учёт монитора
        // (`ConvergenceMonitor.StepsTaken`) в этом файле не читается нигде,
        // поэтому снятие заведомо мёртвых вызовов не меняет ни одного
        // сообщаемого числа, ни одной контрольной точки, ни одного файла
        // метрик.
        var monitor = request.Input.ConvergenceThreshold == 0f
            ? null
            : new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(
                request.Input.ConvergenceThreshold,
                windowSteps);
        // Молча выключенная или недостижимая проверка сходимости — худший
        // исход: прогон выглядит настроенным на раннюю остановку, а на самом
        // деле всегда доходит до конца бюджета шагов. Поэтому ровно один
        // человекочитаемый след выдаётся перед первым шагом. Оба признака
        // считаются из настройки и не зависят от того, создан ли объект
        // монитора выше.
        TraceConvergenceDetectionDiagnostics(
            validatedMaxStepCount,
            windowSteps,
            request.Input.ConvergenceThreshold);
        var stepsTaken = 0;
        // FIX-MEASURER-03: признак того, что о пропуске проверки сходимости из-за
        // отсутствующей немасштабированной потери уже сказано. Один след за
        // прогон, а не по одному на каждый момент наблюдения: иначе честное
        // сообщение превратилось бы в шум.
        var unscaledLossUnavailableReported = false;
        for (var step = 1; step <= validatedMaxStepCount; step++)
        {
            var stepPermit = AcquireMutationPermit(request.Input);
            if (!stepPermit.IsSuccess || stepPermit.Value is null)
            {
                return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                    stepPermit.ErrorMessage,
                    stepPermit.ErrorCode));
            }

            using (stepPermit.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                        "Cancelled.",
                        "CANCELLED"));
                }

                try
                {
                    OptimizerMutationSinkObserver.Value?.Invoke(
                        preparedInputs,
                        batches);
                    cancellationToken.ThrowIfCancellationRequested();
                    preparation?.Disarm();
                    lastMetrics = trainer.StepVector(batches, step);
                }
                catch (DpoMutationAdmissionException exception)
                {
                    return Task.FromResult(OperationResult<
                        LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                            "A live DPO mutation admission is required.",
                            exception.ErrorCode));
                }
                stepsTaken = step;
                // Порядок проверок сохранён: сначала наступил ли момент
                // наблюдения, затем существует ли монитор, и только затем сам
                // вопрос о сходимости. При нулевом пороге монитора нет, и
                // вопрос не задаётся; при любом другом пороге монитор есть, и
                // условие совпадает с прежним посимвольно по смыслу.
                if (step % windowSteps == 0 && monitor is not null)
                {
                    // FIX-MEASURER-03: сторож сравнивает с НЕПОДВИЖНЫМ порогом
                    // только немасштабированную потерю. Обычное поле `Loss` в
                    // векторном пути домножается на плотность обучающего вектора
                    // на ветви точных пар, а награды, запас и доля верных — нет.
                    // Поэтому прежде порог зависел от плотности вектора, то есть
                    // от свойства данных, а не от хода обучения. Новое поле
                    // копится построчно до индивидуального масштабирования и на
                    // обеих ветвях одинаково, поэтому порог перестаёт зависеть от
                    // плотности. Прежнее поле не читается здесь больше вовсе и
                    // ничем не изменено.
                    var comparableLoss = lastMetrics.UnscaledLoss;
                    if (comparableLoss is null)
                    {
                        // Пустое поле означает «не считалось», а НЕ ноль: такую
                        // квитанцию мог записать прежний выпуск кода. Остановиться
                        // по несравнимому числу хуже, чем пройти объявленный
                        // бюджет шагов, поэтому решение названо вслух один раз за
                        // прогон и обучение продолжается. Молчаливого нуля здесь
                        // нет и быть не должно.
                        if (!unscaledLossUnavailableReported)
                        {
                            unscaledLossUnavailableReported = true;
                            System.Diagnostics.Trace.TraceWarning(
                                "CapabilityTraining DPO convergence check is skipped for this run: " +
                                "the step receipt carries no unscaled loss, so there is no value " +
                                "comparable to the fixed threshold. An absent value is not zero, " +
                                "and stopping on an incomparable number would be worse than " +
                                "running the declared budget, so the loop runs the full step " +
                                "budget of {0} steps instead.",
                                validatedMaxStepCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        }
                    }
                    else if (monitor.ShouldStop(comparableLoss.Value))
                    {
                        break;
                    }
                }
            }
        }

        var postStepOptimizerState = trainer.ExportOptimizerState();
        if (postStepOptimizerState.Step <= 0)
        {
            return Task.FromResult(OperationResult<
                LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                "The DPO trainer did not expose a post-step optimizer state.",
                "DPO_OPTIMIZER_STATE_EXPORT_FAILED"));
        }
        var postStepOptimizerStateIdentity =
            CreateOptimizerStateIdentity(postStepOptimizerState);

        if (ForceReferenceValidationFailure.Value?.Invoke() == true ||
            !FullStateMatchesSnapshot(
                request.Input.ReferenceModel,
                referenceCapture.Value!))
        {
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
                "The frozen DPO reference changed during optimization.",
                "DPO_REFERENCE_MUTATED"));
        }

        // Collect generated metric files.
        var metricFiles = Directory.Exists(request.Input.CheckpointDirectory)
            ? Directory.GetFiles(request.Input.CheckpointDirectory, "dpo_step_*.json").OrderBy(path => path, StringComparer.Ordinal).ToArray()
            : [];

        // Build the teaching result.
        var trainingTimeMs = (long)(DateTimeOffset.UtcNow - started).TotalMilliseconds;
        var output = new LogicalDpoTrainingResult(
            preparedInputs.Count,
            DpoStepCount: stepsTaken,
            adapters.Count,
            // FIX-REPORT-READER-01: приведения отсутствия к нулю здесь больше
            // нет. Если квитанции последнего шага нет, в отчёт уходит
            // отсутствие, а не ноль: поле необязательно именно для этого.
            // Прежде оба числа при отсутствии становились нулём, и отчёт
            // утверждал измеренный факт, которого никто не измерял.
            // Достижимость этого состояния измерена и названа прямо: счёт шагов
            // проверяется диапазоном от одного до пяти до входа в цикл, цикл
            // всегда делает хотя бы один шаг, а тело шага либо присваивает
            // квитанцию, либо возвращает отказ и сюда не доходит. Значит сегодня
            // отсутствие по боевому пути НЕДОСТИЖИМО, и это не отменяет правку:
            // устраняется не наблюдаемое сейчас число, а ложь, которую
            // унаследовал бы первый будущий читатель — например тот, кто
            // разрешит нулевой бюджет шагов или сложит отчёт из частично
            // прочитанных квитанций. Договор обязан быть честным до появления
            // такого читателя, а не после.
            lastMetrics?.Loss,
            lastMetrics?.Accuracy,
            trainingTimeMs,
            metricFiles,
            [
                "01.Tokenizer:BpeTokenizer",
                "03.TransformerModel:LlmModel",
                "10.LoRA:LoraApplicator",
                "11.DPO:DpoTrainer",
                "11.DPO:DpoTrainer.StepVector",
                "23.SemanticCompilerAdapter:TrainingVectorBuilder"
            ],
            postStepOptimizerState,
            postStepOptimizerStateIdentity,
            trainer.OptimizerSettings,
            postStepRandomState);

        // Wrap the teaching result in the logical-unit envelope.
        var result = new LogicalUnitResult<LogicalDpoTrainingResult>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            output,
            0.90,
            ["LEARNING", "DPO"],
            [DomainTag],
            metricFiles,
            []);

        return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Ok(result));
        }
    }

    private static OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>> PreparationRollbackFailure(
        string primaryCause, IReadOnlyList<Exception> failures)
    {
        if (!failures.All(LogicalDpoExceptionBoundary.ShouldNormalize))
            throw new AggregateException("DPO_PREPARATION_ROLLBACK_FAILED",
                new[] { new InvalidOperationException(primaryCause) }.Concat(failures));
        return OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(
            $"Primary preparation failure: {primaryCause}; rollback failures: " +
            string.Join(" | ", failures.Select(static failure => failure.ToString())),
            "DPO_PREPARATION_ROLLBACK_FAILED");
    }

    // Private and lease-scoped: no directory creation, topology reconstruction or
    // ownership cleanup is permitted here. Saved gradient buffers need their own
    // content copies because callers can retain them after reference.Grad is cleared.
    private sealed class PreparedContinuationTransaction
    {
        private readonly PreparedModelSnapshot _policy;
        private readonly PreparedModelSnapshot _reference;
        private DpoTrainer? _trainer;
        private AdamWPortableState? _optimizer;
        private bool _armed = true;

        internal PreparedContinuationTransaction(
            LogicalDpoTrainingRequest input, LoraConfig config,
            LogicalDpoAdapterSeedAuthority authority,
            IReadOnlyList<LoraLinear> policyAdapters,
            IReadOnlyList<LoraLinear> referenceAdapters)
        {
            var identityAuthority = new LoraPairRandomSeedAuthority(
                authority.RootInitializationSeed, authority.PolicyDropoutStreamDomain,
                authority.ReferenceDropoutStreamDomain);
            var identity = new LlmModelRandomStateIdentity(
                LoraRandomStateIdentity.ComputeTopologySha256(input.PolicyModel.Config, config, identityAuthority),
                LoraRandomStateIdentity.ComputeLoraConfigurationSha256(config, identityAuthority));
            _policy = Capture(input.PolicyModel, policyAdapters, authority.PolicyDropoutStreamDomain);
            _reference = Capture(input.ReferenceModel, referenceAdapters, authority.ReferenceDropoutStreamDomain);

            PreparedModelSnapshot Capture(LlmModel model, IReadOnlyList<LoraLinear> adapters, string role)
                => new(model, identity, adapters, () => LoraApplicator.ValidateSeededApplicationProvenance(
                    model, config, (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                        authority.RootInitializationSeed, block, projection, role)));
        }

        internal void AttachTrainer(DpoTrainer trainer)
        {
            var optimizer = trainer.ExportOptimizerState();
            _trainer = trainer;
            _optimizer = optimizer;
        }

        internal void Disarm() => _armed = false;

        internal IReadOnlyList<Exception> Rollback()
        {
            var failures = new List<Exception>();
            if (!_armed)
                return failures;
            _armed = false;
            // Each owner is attempted even if an earlier owner cannot be restored.
            Attempt(() =>
            {
                if (_trainer is not null && !_trainer.TryRestorePreparationOptimizerState(_optimizer))
                    throw new InvalidOperationException("DPO_PREPARATION_OPTIMIZER_ROLLBACK_FAILED");
            });
            Attempt(_policy.RestoreValuesAndTopology);
            Attempt(_reference.RestoreValuesAndTopology);
            Attempt(_policy.RestoreRandomState);
            Attempt(_reference.RestoreRandomState);
            _trainer = null;
            return failures;

            void Attempt(Action restore)
            {
                try { restore(); }
                // Collected, not swallowed: the boundary preserves the primary
                // cause and refuses normalization of cancellation/fatal graphs.
                catch (Exception exception)
                { failures.Add(exception); }
            }
        }
    }

    private sealed class PreparedModelSnapshot
    {
        private readonly LlmModel _model;
        private readonly LlmModelRandomStateIdentity _identity;
        private readonly LlmModelRandomStateSnapshot _random;
        private readonly LoraLinear[] _adapters;
        private readonly Func<IReadOnlyList<LoraLinear>> _validateAdapters;
        private readonly PreparedParameterSnapshot[] _parameters;

        internal PreparedModelSnapshot(LlmModel model, LlmModelRandomStateIdentity identity,
            IReadOnlyList<LoraLinear> adapters, Func<IReadOnlyList<LoraLinear>> validateAdapters)
        {
            _model = model;
            _identity = identity;
            _adapters = adapters.ToArray();
            _validateAdapters = validateAdapters;
            _parameters = model.GetCheckpointParameterEntries().Select(static entry =>
                new PreparedParameterSnapshot(entry)).ToArray();
            _random = model.CaptureRandomState(identity.TopologySha256, identity.LoraConfigurationSha256, 0);
        }

        internal void RestoreValuesAndTopology()
        {
            // Restore the saved objects even if topology verification will report
            // an invariant violation; never rebind parameters/adapters to hide it.
            foreach (var parameter in _parameters)
                parameter.Restore();
            var current = _model.GetCheckpointParameterEntries();
            if (current.Count != _parameters.Length || current.Where((entry, index) =>
                    !_parameters[index].Matches(entry)).Any() ||
                !_validateAdapters().SequenceEqual(_adapters, ReferenceEqualityComparer.Instance))
                throw new InvalidOperationException("DPO_PREPARATION_TOPOLOGY_ROLLBACK_FAILED");
        }

        internal void RestoreRandomState() => _model.RestoreRandomState(_random, _identity);
    }

    private sealed class PreparedParameterSnapshot
    {
        private readonly LlmCheckpointParameterEntry _entry;
        private readonly Tensor _value;
        private readonly TensorStorage _storage;
        private readonly float[] _data;
        private readonly float[] _contents;
        private readonly int[] _shape;
        private readonly bool _requiresGrad;
        private readonly Tensor? _gradient;
        private readonly TensorStorage? _gradientStorage;
        private readonly float[]? _gradientData;
        private readonly float[]? _gradientContents;

        internal PreparedParameterSnapshot(LlmCheckpointParameterEntry entry)
        {
            _entry = entry;
            _value = entry.Parameter.Value;
            _storage = _value.Storage;
            _data = _value.Data;
            _contents = _data.ToArray();
            _shape = _value.Shape.Dimensions.ToArray();
            _requiresGrad = _value.RequiresGrad;
            _gradient = _value.Grad;
            _gradientStorage = _gradient?.Storage;
            _gradientData = _gradient?.Data;
            _gradientContents = _gradientData?.ToArray();
        }

        internal void Restore()
        {
            Array.Copy(_contents, _data, _contents.Length);
            if (_gradientData is not null)
                Array.Copy(_gradientContents!, _gradientData, _gradientData.Length);
            _value.RequiresGrad = _requiresGrad;
            _value.Grad = _gradient;
        }

        internal bool Matches(LlmCheckpointParameterEntry entry)
            => entry.Key == _entry.Key && ReferenceEquals(entry.Parameter, _entry.Parameter) &&
                ReferenceEquals(entry.Parameter.Value, _value) && ReferenceEquals(_value.Storage, _storage) &&
                ReferenceEquals(_value.Data, _data) && _value.Shape.Dimensions.SequenceEqual(_shape) &&
                ReferenceEquals(_gradient?.Storage, _gradientStorage) &&
                ReferenceEquals(_gradient?.Data, _gradientData);
    }

    private static OperationResult<IDisposable> AcquireMutationPermit(
        LogicalDpoTrainingRequest input)
    {
        try { ValidateExactPreparation(input); }
        catch (DpoMutationAdmissionException exception)
        { return OperationResult<IDisposable>.Fail(exception.Message, exception.ErrorCode); }
        if (input.AdmissionCapabilities is null)
        {
            return OperationResult<IDisposable>.Fail(
                "A server-issued logical capability training admission bundle is required.",
                LogicalCapabilityTrainingAdmissionCapabilities.RequiredErrorCode);
        }

        var permit = input.AdmissionCapabilities.AcquireMutationPermit();
        if (!permit.IsSuccess)
            return permit;
        if (permit.Value is null)
        {
            return OperationResult<IDisposable>.Fail(
                "The DPO mutation permit was not issued.",
                "DPO_MUTATION_PERMIT_REQUIRED");
        }

        try { ValidateExactPreparation(input); }
        catch (DpoMutationAdmissionException exception)
        {
            permit.Value.Dispose();
            return OperationResult<IDisposable>.Fail(exception.Message, exception.ErrorCode);
        }
        return permit;
    }

    private static void ValidateExactPreparation(LogicalDpoTrainingRequest input)
    {
        if (input.MaterialVersion != DpoExactPairMaterial.Version) return;
        DpoProjectLoraRecipient.RequirePreparedPolicy(input.PolicyModel, input.LoraConfig ?? new LoraConfig());
        if (input.OptimizerStateToRestore is not null)
        {
            if (input.ContinuationProof is null) DpoExecutionProfile.Mismatch();
            input.ContinuationProof!.RequireMatches(input.OptimizerStateToRestore, input.RandomStateToRestore);
        }
    }

    public static OperationResult<LogicalDpoPreparedExecution>
        PrepareExecution(
            LogicalDpoPreferenceExample[]? preferences,
            BpeTokenizer? tokenizer,
            string materialVersion = DpoExactPairMaterial.LegacyVersion)
    {
        if (preferences is null || tokenizer is null ||
            preferences.Length is 0 or > MaximumPreferenceCount ||
            materialVersion is not (DpoExactPairMaterial.LegacyVersion or DpoExactPairMaterial.Version))
        {
            return InvalidPreparedExecution();
        }

        try
        {
            var prepared = new LogicalDpoPreparedOptimizerInput[
                preferences.Length];
            long aggregateTextBytes = 0;
            long aggregateVectorLength = 0;
            for (var index = 0; index < preferences.Length; index++)
            {
                var preference = preferences[index];
                if (preference is null ||
                    !TryAccumulateBoundedUtf8Bytes(
                        preference.Prompt,
                        ref aggregateTextBytes) ||
                    !TryAccumulateBoundedUtf8Bytes(
                        preference.Chosen,
                        ref aggregateTextBytes) ||
                    !TryAccumulateBoundedUtf8Bytes(
                        preference.Rejected,
                        ref aggregateTextBytes))
                {
                    return InvalidPreparedExecution();
                }

                if (!TryGetDefinedPairAuthors(
                        preference,
                        out _,
                        out _))
                {
                    return InvalidPreparedExecution();
                }

                var sourceVector = preference.TeachingVector;
                if (sourceVector is null)
                    sourceVector = TrainingVectorBuilder.Build(preference.Prompt);
                if (sourceVector is null ||
                    sourceVector.Length is 0 or > MaximumTeachingVectorLength ||
                    aggregateVectorLength + sourceVector.Length >
                        MaximumAggregateTeachingVectorLength)
                {
                    return InvalidPreparedExecution();
                }

                var vector = new float[sourceVector.Length];
                for (var vectorIndex = 0;
                     vectorIndex < sourceVector.Length;
                     vectorIndex++)
                {
                    var value = sourceVector[vectorIndex];
                    if (!float.IsFinite(value))
                        return InvalidPreparedExecution();
                    vector[vectorIndex] = value;
                }
                aggregateVectorLength += vector.Length;

                var ownedPreference = preference with
                {
                    TeachingVector = vector
                };
                prepared[index] = new LogicalDpoPreparedOptimizerInput(
                    ownedPreference.Prompt,
                    ownedPreference.Chosen,
                    ownedPreference.Rejected,
                    new DpoVectorBatch(
                        BuildBatch(tokenizer, ownedPreference, materialVersion),
                        vector,
                        1f));
            }

            return OperationResult<LogicalDpoPreparedExecution>.Ok(
                new LogicalDpoPreparedExecution(prepared, materialVersion));
        }
        catch (DpoMutationAdmissionException exception)
        {
            return OperationResult<LogicalDpoPreparedExecution>.Fail(exception.Message, exception.ErrorCode);
        }
        catch (Exception exception)
            when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
        {
            return InvalidPreparedExecution();
        }
    }

    private static bool TryAccumulateBoundedUtf8Bytes(
        string? text,
        ref long aggregateTextBytes)
    {
        if (text is null)
            return false;

        var byteCount = Encoding.UTF8.GetByteCount(text);
        if (byteCount > MaximumPreferenceTextUtf8Bytes ||
            aggregateTextBytes + byteCount >
                MaximumAggregatePreferenceTextUtf8Bytes)
        {
            return false;
        }

        aggregateTextBytes += byteCount;
        return true;
    }

    private static bool TryGetDefinedPairAuthors(
        LogicalDpoPreferenceExample preference,
        out DpoPairSideAuthorKind chosenAuthor,
        out DpoPairSideAuthorKind rejectedAuthor)
    {
        chosenAuthor = default;
        rejectedAuthor = default;
        if (!preference.ChosenAuthorKind.HasValue ||
            !preference.RejectedAuthorKind.HasValue)
        {
            return false;
        }

        chosenAuthor = preference.ChosenAuthorKind.Value;
        rejectedAuthor = preference.RejectedAuthorKind.Value;
        return Enum.IsDefined(typeof(DpoPairSideAuthorKind), chosenAuthor) &&
               Enum.IsDefined(typeof(DpoPairSideAuthorKind), rejectedAuthor);
    }

    private static OperationResult<LogicalDpoPreparedExecution>
        InvalidPreparedExecution()
        => OperationResult<LogicalDpoPreparedExecution>.Fail(
            "DPO preference rows, UTF-8 text, or teaching vectors are invalid or exceed the supported bounds.",
            PreferenceBoundsInvalidErrorCode);

    private static OperationResult<ValidatedParentArtifacts?> OpenAndValidateParentArtifacts(
        LogicalDpoTrainingRequest input)
    {
        var hasParentCheckpoint = !string.IsNullOrWhiteSpace(input.ParentPolicyCheckpointPath);
        if (!hasParentCheckpoint)
        {
            return input.ParentProvenance is null
                ? OperationResult<ValidatedParentArtifacts?>.Ok(null)
                : OperationResult<ValidatedParentArtifacts?>.Fail(
                    "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                    "DPO_PARENT_PROVENANCE_INVALID");
        }

        var provenance = input.ParentProvenance;
        if (provenance is null)
        {
            return OperationResult<ValidatedParentArtifacts?>.Fail(
                "Parent DPO provenance is required for continued training.",
                "DPO_PARENT_PROVENANCE_REQUIRED");
        }

        var provenanceLora = RuntimeProvenance.CanonicalizeLora(provenance.ParentLoraConfig);
        var requestLora = RuntimeProvenance.CanonicalizeLora(input.LoraConfig ?? new LoraConfig());
        if (!RuntimeProvenance.IsValidModelConfig(provenance.ParentConfig)
            || provenance.ParentConfig != input.PolicyModel.Config
            || provenance.ParentConfig != input.ReferenceModel.Config
            || input.Tokenizer.VocabSize != provenance.ParentConfig.VocabSize
            || string.IsNullOrWhiteSpace(provenance.ParentTokenizerPath)
            || !RuntimeProvenance.IsSha256Hex(provenance.TokenizerSha256)
            || !RuntimeProvenance.IsSha256Hex(provenance.PolicyCheckpointSha256)
            || !RuntimeProvenance.IsSha256Hex(provenance.RuntimeProvenanceSha256)
            || !provenanceLora.IsSuccess
            || !requestLora.IsSuccess
            || !RuntimeProvenance.LoraEquals(provenanceLora.Value!, requestLora.Value!))
        {
            return OperationResult<ValidatedParentArtifacts?>.Fail(
                "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                "DPO_PARENT_PROVENANCE_INVALID");
        }

        HeldRuntimeArtifact? checkpointArtifact = null;
        HeldRuntimeArtifact? tokenizerArtifact = null;
        try
        {
            checkpointArtifact = HeldRuntimeArtifact.Open(input.ParentPolicyCheckpointPath!);
            tokenizerArtifact = HeldRuntimeArtifact.Open(provenance.ParentTokenizerPath);
            tokenizerArtifact.Reset();
            var durableTokenizer = BpeTokenizer.Load(tokenizerArtifact.Stream);
            var liveTokenizerSha256 = input.Tokenizer.ComputeStateSha256();
            var durableTokenizerSha256 = durableTokenizer.ComputeStateSha256();
            var runtimeProvenance = RuntimeProvenance.ComputeSha256(
                provenance.ParentConfig,
                provenanceLora.Value!,
                tokenizerArtifact.Sha256,
                checkpointArtifact.Sha256);
            if (!runtimeProvenance.IsSuccess
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    provenance.PolicyCheckpointSha256,
                    checkpointArtifact.Sha256)
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    provenance.TokenizerSha256,
                    tokenizerArtifact.Sha256)
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    provenance.TokenizerSha256,
                    liveTokenizerSha256)
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    provenance.TokenizerSha256,
                    durableTokenizerSha256)
                || !RuntimeProvenance.FixedTimeSha256Equals(
                    provenance.RuntimeProvenanceSha256,
                    runtimeProvenance.Value!))
            {
                checkpointArtifact.Dispose();
                tokenizerArtifact.Dispose();
                return OperationResult<ValidatedParentArtifacts?>.Fail(
                    "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                    "DPO_PARENT_PROVENANCE_INVALID");
            }

            return OperationResult<ValidatedParentArtifacts?>.Ok(
                new ValidatedParentArtifacts(
                    checkpointArtifact,
                    tokenizerArtifact,
                    provenanceLora.Value!,
                    durableTokenizer,
                    provenance.TokenizerSha256));
        }
        catch (Exception exception)
            when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
        {
            checkpointArtifact?.Dispose();
            tokenizerArtifact?.Dispose();
            return OperationResult<ValidatedParentArtifacts?>.Fail(
                "Parent DPO provenance is invalid or does not match the supplied artifacts.",
                "DPO_PARENT_PROVENANCE_INVALID");
        }
    }

    private sealed class ValidatedParentArtifacts : IDisposable
    {
        internal ValidatedParentArtifacts(
            HeldRuntimeArtifact policyCheckpoint,
            HeldRuntimeArtifact tokenizer,
            LoraConfig canonicalLoraConfig,
            BpeTokenizer durableTokenizer,
            string tokenizerSha256)
        {
            PolicyCheckpoint = policyCheckpoint;
            Tokenizer = tokenizer;
            CanonicalLoraConfig = new LoraConfig
            {
                Rank = canonicalLoraConfig.Rank,
                Alpha = canonicalLoraConfig.Alpha,
                Dropout = canonicalLoraConfig.Dropout,
                TargetModules = canonicalLoraConfig.TargetModules.ToArray()
            };
            DurableTokenizer = durableTokenizer;
            TokenizerSha256 = tokenizerSha256;
        }

        internal HeldRuntimeArtifact PolicyCheckpoint { get; }
        internal HeldRuntimeArtifact Tokenizer { get; }
        internal LoraConfig CanonicalLoraConfig { get; }
        internal BpeTokenizer DurableTokenizer { get; }
        private string TokenizerSha256 { get; }

        internal bool MatchesCurrentCallerIdentity(
            LoraConfig currentLoraConfig,
            BpeTokenizer currentTokenizer)
        {
            var canonicalCurrentLora = RuntimeProvenance.CanonicalizeLora(currentLoraConfig);
            return canonicalCurrentLora.IsSuccess
                   && RuntimeProvenance.LoraEquals(
                       CanonicalLoraConfig,
                       canonicalCurrentLora.Value!)
                   && RuntimeProvenance.FixedTimeSha256Equals(
                       TokenizerSha256,
                       currentTokenizer.ComputeStateSha256());
        }

        public void Dispose()
        {
            Tokenizer.Dispose();
            PolicyCheckpoint.Dispose();
        }
    }

    private static OperationResult<DpoTrainer> CreateDpoTrainer(
        LogicalDpoTrainingRequest input,
        LoraConfig loraConfig,
        LogicalDpoAdapterSeedAuthority? seedAuthority,
        bool randomStateRequired,
        Action<DpoTrainingPublicationBundle>? publicationBundleObserver,
        IReadOnlyList<LoraLinear> adapters,
        DpoBatch actualBatch)
    {
        try
        {
            Func<int, LlmModelRandomStateSnapshot>? captureRandomState = null;
            Action<LlmModelRandomStateSnapshot>? restoreRandomState = null;
            if (randomStateRequired && seedAuthority is not null)
            {
                var authority = new LoraPairRandomSeedAuthority(
                    seedAuthority.RootInitializationSeed,
                    seedAuthority.PolicyDropoutStreamDomain,
                    seedAuthority.ReferenceDropoutStreamDomain);
                var topologyHash = LoraRandomStateIdentity.ComputeTopologySha256(
                    input.PolicyModel.Config,
                    loraConfig,
                    authority);
                var loraConfigurationHash =
                    LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
                        loraConfig,
                        authority);
                captureRandomState = step => input.PolicyModel.CaptureRandomState(
                    topologyHash,
                    loraConfigurationHash,
                    step);
                var expectedIdentity = new LlmModelRandomStateIdentity(
                    topologyHash,
                    loraConfigurationHash);
                restoreRandomState = state => input.PolicyModel.RestoreRandomState(
                    state,
                    expectedIdentity);
            }
            var publicationObserver = input.TryAcceptArtifactOwnership is null &&
                                      input.PersistPublicationBundle is null &&
                                      input.BeforePolicyFinalMove is null &&
                                      input.PublicationFailed is null
                ? null
                : new DpoTrainerPublicationObserver
                {
                    TryAcceptArtifactOwnership = input.TryAcceptArtifactOwnership,
                    // The repository-owned executor owns the in-flight policy and
                    // metrics artifacts until the whole step commits. Enabling
                    // custody tracking here lets DpoTrainer remove only the
                    // artifacts created by this publication when state
                    // readback or a later final move fails.
                    ArtifactOwnershipProven = input.TryAcceptArtifactOwnership is null
                        ? _ => { }
                        : null,
                    BeforePolicyFinalMove = bundle =>
                    {
                        input.PersistPublicationBundle?.Invoke(bundle);
                        input.BeforePolicyFinalMove?.Invoke(bundle);
                        publicationBundleObserver?.Invoke(bundle);
                    },
                    PublicationFailed = input.PublicationFailed
                };
            if (input.MaterialVersion == DpoExactPairMaterial.Version)
            {
                var recipient = DpoProjectLoraRecipient.Capture(input.PolicyModel, input.ReferenceModel, adapters, loraConfig);
                var profile = DpoExecutionProfile.Capture(actualBatch, recipient.Bindings);
                return OperationResult<DpoTrainer>.Ok(DpoTrainer.CreateForProjectLora(input.PolicyModel, input.ReferenceModel,
                    input.CheckpointDirectory, input.AdmissionCapabilities!.DpoTrainerMutationAdmissionCapability,
                    recipient, profile, input.ContinuationProof, publicationObserver, captureRandomState, restoreRandomState));
            }
            if (input.SeededTopologyPreparedByRepositoryStart)
                return OperationResult<DpoTrainer>.Ok(DpoTrainer.CreateForPreparedContinuation(
                    input.PolicyModel, input.ReferenceModel, input.CheckpointDirectory,
                    input.AdmissionCapabilities!.DpoTrainerMutationAdmissionCapability,
                    DpoTrainer.DefaultLearningRate, publicationObserver, captureRandomState, restoreRandomState));
            return OperationResult<DpoTrainer>.Ok(new DpoTrainer(
                input.PolicyModel,
                input.ReferenceModel,
                input.CheckpointDirectory,
                input.AdmissionCapabilities!.DpoTrainerMutationAdmissionCapability,
                learningRate: DpoTrainer.DefaultLearningRate,
                publicationObserver: publicationObserver,
                captureRandomState: captureRandomState,
                restoreRandomState: restoreRandomState));
        }
        catch (DpoMutationAdmissionException exception)
        {
            return OperationResult<DpoTrainer>.Fail(
                "A live DPO mutation admission is required.",
                exception.ErrorCode);
        }
        catch (Exception exception)
            when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
        {
            return OperationResult<DpoTrainer>.Fail(
                "DPO trainer could not be initialized.",
                "DPO_TRAINER_INITIALIZATION_FAILED");
        }
    }

    private static LogicalDpoTrainingOptimizerStateIdentity
        CreateOptimizerStateIdentity(AdamWPortableState state)
    {
        var bytes = state.ToBytes();
        return new LogicalDpoTrainingOptimizerStateIdentity(
            state.FormatVersion,
            state.Step,
            Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
            bytes.LongLength);
    }

    private static OperationResult<bool> ValidateSeededPolicyReferenceCompatibility(
        LlmModel policy,
        LlmModel reference,
        bool topologyPreparedByRepositoryStart = false)
    {
        if (ReferenceEquals(policy, reference))
        {
            return OperationResult<bool>.Fail(
                "Policy and reference must be distinct model instances.",
                "DPO_REFERENCE_STORAGE_SHARED");
        }

        if (policy.Config != reference.Config)
        {
            return OperationResult<bool>.Fail(
                "Policy and reference model configurations must match exactly.",
                "DPO_REFERENCE_CONFIG_MISMATCH");
        }

        try
        {
            var policyEntries = policy.GetCheckpointParameterEntries();
            var referenceEntries = reference.GetCheckpointParameterEntries();
            if (policyEntries.Count != policy.Parameters().Count() ||
                referenceEntries.Count != reference.Parameters().Count() ||
                // Fresh attachment requires adapter-free models. A repository
                // continuation already contains adapters; its exact seeded
                // topology is verified below before any update. In both routes
                // compare the complete pair and each key/shape/storage owner.
                (!topologyPreparedByRepositoryStart &&
                    (policyEntries.Count != policy.BaseParameters.Count ||
                     referenceEntries.Count != reference.BaseParameters.Count)) ||
                policy.BaseParameters.Count != reference.BaseParameters.Count ||
                policyEntries.Count != referenceEntries.Count)
            {
                return OperationResult<bool>.Fail(
                    "Policy and reference full parameter counts do not match.",
                    "DPO_REFERENCE_PROVENANCE_MISMATCH");
            }

            // Independence is across the complete models, not only matching keys.
            var policyStorage = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var entry in policyEntries)
            {
                policyStorage.Add(entry.Parameter);
                policyStorage.Add(entry.Parameter.Value);
                policyStorage.Add(entry.Parameter.Value.Storage);
                policyStorage.Add(entry.Parameter.Value.Data);
            }

            var uniqueKeys = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < policyEntries.Count; index++)
            {
                var sourceEntry = policyEntries[index];
                var destinationEntry = referenceEntries[index];
                if (!uniqueKeys.Add(sourceEntry.Key) ||
                    !string.Equals(
                        sourceEntry.Key,
                        destinationEntry.Key,
                        StringComparison.Ordinal))
                {
                    return OperationResult<bool>.Fail(
                        "Policy and reference parameter keys or ordering do not match.",
                        "DPO_REFERENCE_PROVENANCE_MISMATCH");
                }

                var source = sourceEntry.Parameter;
                var destination = destinationEntry.Parameter;
                if (policyStorage.Contains(destination) ||
                    policyStorage.Contains(destination.Value) ||
                    policyStorage.Contains(destination.Value.Storage) ||
                    policyStorage.Contains(destination.Value.Data))
                {
                    return OperationResult<bool>.Fail(
                        "Policy and reference parameter storage must be independent.",
                        "DPO_REFERENCE_STORAGE_SHARED");
                }

                if (!source.Value.Shape.Dimensions.SequenceEqual(
                        destination.Value.Shape.Dimensions) ||
                    source.Value.Data.Length != destination.Value.Data.Length)
                {
                    return OperationResult<bool>.Fail(
                        "Policy and reference full parameter shapes do not match.",
                        "DPO_REFERENCE_PROVENANCE_MISMATCH");
                }
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (LogicalDpoExceptionBoundary.ShouldNormalize(exception))
        {
            return OperationResult<bool>.Fail(
                "Policy and reference full parameter counts do not match.",
                "DPO_REFERENCE_PROVENANCE_MISMATCH");
        }
    }

    private static OperationResult<FrozenReferenceSnapshot> CaptureFrozenReferenceState(
        LlmModel policy,
        LlmModel reference,
        bool copyPolicyState)
    {
        if (ReferenceEquals(policy, reference))
        {
            return OperationResult<FrozenReferenceSnapshot>.Fail(
                "Policy and reference must be distinct model instances.",
                "DPO_REFERENCE_STORAGE_SHARED");
        }

        if (policy.Config != reference.Config)
        {
            return OperationResult<FrozenReferenceSnapshot>.Fail(
                "Policy and reference model configurations must match exactly.",
                "DPO_REFERENCE_CONFIG_MISMATCH");
        }

        var policyEntries = policy.GetCheckpointParameterEntries();
        var referenceEntries = reference.GetCheckpointParameterEntries();
        if (policyEntries.Count != policy.Parameters().Count()
            || referenceEntries.Count != reference.Parameters().Count()
            || policyEntries.Count != referenceEntries.Count)
        {
            return OperationResult<FrozenReferenceSnapshot>.Fail(
                "Policy and reference full parameter counts do not match.",
                "DPO_REFERENCE_PROVENANCE_MISMATCH");
        }

        // Prepared continuations must already share the same numeric baseline.
        // Reject before freezing or clearing any gradient on either model.
        if (!copyPolicyState && !FullStatesMatch(policy, reference))
        {
            return OperationResult<FrozenReferenceSnapshot>.Fail(
                "The prepared DPO reference does not match the full step-zero policy.",
                "DPO_REFERENCE_PROVENANCE_MISMATCH");
        }

        var uniqueKeys = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < policyEntries.Count; index++)
        {
            var sourceEntry = policyEntries[index];
            var destinationEntry = referenceEntries[index];
            if (!uniqueKeys.Add(sourceEntry.Key)
                || !string.Equals(sourceEntry.Key, destinationEntry.Key, StringComparison.Ordinal))
            {
                return OperationResult<FrozenReferenceSnapshot>.Fail(
                    "Policy and reference parameter keys or ordering do not match.",
                    "DPO_REFERENCE_PROVENANCE_MISMATCH");
            }

            var source = sourceEntry.Parameter;
            var destination = destinationEntry.Parameter;
            if (ReferenceEquals(source, destination)
                || ReferenceEquals(source.Value, destination.Value)
                || ReferenceEquals(source.Value.Storage, destination.Value.Storage)
                || ReferenceEquals(source.Value.Data, destination.Value.Data))
            {
                return OperationResult<FrozenReferenceSnapshot>.Fail(
                    "Policy and reference parameter storage must be independent.",
                    "DPO_REFERENCE_STORAGE_SHARED");
            }

            if (!source.Value.Shape.Dimensions.SequenceEqual(destination.Value.Shape.Dimensions)
                || source.Value.Data.Length != destination.Value.Data.Length)
            {
                return OperationResult<FrozenReferenceSnapshot>.Fail(
                    "Policy and reference full parameter shapes do not match.",
                    "DPO_REFERENCE_PROVENANCE_MISMATCH");
            }

            if (copyPolicyState)
                Array.Copy(source.Value.Data, destination.Value.Data, source.Value.Data.Length);
            destination.Value.RequiresGrad = false;
            destination.Value.Grad = null;
        }

        if (!FullStatesMatch(policy, reference))
        {
            return OperationResult<FrozenReferenceSnapshot>.Fail(
                "The frozen DPO reference could not be verified against the full step-zero policy.",
                "DPO_REFERENCE_PROVENANCE_MISMATCH");
        }

        return OperationResult<FrozenReferenceSnapshot>.Ok(SnapshotFullState(reference));
    }

    private static bool FullStatesMatch(
        LlmModel expected,
        LlmModel actual)
    {
        var expectedEntries = expected.GetCheckpointParameterEntries();
        var actualEntries = actual.GetCheckpointParameterEntries();
        if (expectedEntries.Count != actualEntries.Count)
            return false;

        for (var parameterIndex = 0; parameterIndex < expectedEntries.Count; parameterIndex++)
        {
            if (!string.Equals(
                    expectedEntries[parameterIndex].Key,
                    actualEntries[parameterIndex].Key,
                    StringComparison.Ordinal))
            {
                return false;
            }

            var expectedData = expectedEntries[parameterIndex].Parameter.Value.Data;
            var actualData = actualEntries[parameterIndex].Parameter.Value.Data;
            if (expectedData.Length != actualData.Length)
                return false;

            for (var valueIndex = 0; valueIndex < expectedData.Length; valueIndex++)
            {
                if (BitConverter.SingleToInt32Bits(expectedData[valueIndex])
                    != BitConverter.SingleToInt32Bits(actualData[valueIndex]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static FrozenReferenceSnapshot SnapshotFullState(LlmModel model)
    {
        var entries = model.GetCheckpointParameterEntries();
        return new FrozenReferenceSnapshot(
            entries.Select(entry => entry.Key).ToArray(),
            entries.Select(entry => entry.Parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
                .ToArray());
    }

    private static bool FullStateMatchesSnapshot(
        LlmModel model,
        FrozenReferenceSnapshot snapshot)
    {
        var entries = model.GetCheckpointParameterEntries();
        if (entries.Count != snapshot.Keys.Length
            || entries.Count != snapshot.ParameterBits.Length)
        {
            return false;
        }

        for (var parameterIndex = 0; parameterIndex < entries.Count; parameterIndex++)
        {
            if (!string.Equals(
                    entries[parameterIndex].Key,
                    snapshot.Keys[parameterIndex],
                    StringComparison.Ordinal))
            {
                return false;
            }

            var data = entries[parameterIndex].Parameter.Value.Data;
            var expectedBits = snapshot.ParameterBits[parameterIndex];
            if (data.Length != expectedBits.Length)
                return false;

            for (var valueIndex = 0; valueIndex < data.Length; valueIndex++)
            {
                if (BitConverter.SingleToInt32Bits(data[valueIndex]) != expectedBits[valueIndex])
                    return false;
            }
        }

        return true;
    }

    private sealed record FrozenReferenceSnapshot(
        string[] Keys,
        int[][] ParameterBits);

    private sealed class SeededDpoPreparationException : Exception
    {
        internal SeededDpoPreparationException(
            string message,
            string errorCode)
            : base(message)
            => ErrorCode = errorCode;

        internal string ErrorCode { get; }
    }

    // Convert one preference example into the tensor shape expected by DpoTrainer.
    private static DpoBatch BuildBatch(BpeTokenizer tokenizer, LogicalDpoPreferenceExample preference,
        string materialVersion)
    {
        if (!TryGetDefinedPairAuthors(
                preference,
                out var chosenAuthor,
                out var rejectedAuthor))
        {
            throw new InvalidOperationException(
                "DPO pair side authors must be defined before tokenization.");
        }

        // Паспорт считаем по raw Prompt/Chosen/Rejected до Compact: юридический
        // fingerprint должен привязываться к исходным байтам, а не к укороченной
        // форме, созданной только для стабильной токенизации.
        var admitted = DpoTrainerPairCustoms.CreateAdmitted(
            preference.Prompt,
            preference.Chosen,
            preference.Rejected,
            chosenAuthor,
            rejectedAuthor,
            nameof(LogicalDpoTrainingUnit));

        // Тестовый seam стоит прямо перед Encode: negative author path должен
        // остановиться на паспорте пары, до любого tokenizer-derived состояния.
        BatchTokenizationObserver.Value?.Invoke(preference);

        if (materialVersion == DpoExactPairMaterial.Version)
            return DpoExactPairMaterial.CreateBatch(tokenizer, preference.Prompt,
                preference.Chosen, preference.Rejected, admitted.Admission);

        // Concatenate prompt and answer so the preference is conditioned on the same A side.
        var chosenIds = tokenizer.Encode($"{Compact(preference.Prompt)} {Compact(preference.Chosen)}");
        var rejectedIds = tokenizer.Encode($"{Compact(preference.Prompt)} {Compact(preference.Rejected)}");

        // Ensure both sequences are non-empty and equal length.
        if (chosenIds.Count == 0)
            chosenIds.Add(0);
        if (rejectedIds.Count == 0)
            rejectedIds.Add(0);

        var length = Math.Max(chosenIds.Count, rejectedIds.Count);
        return new DpoBatch
        {
            ChosenIds = ToTensor(chosenIds, length),
            ChosenMask = ToMask(chosenIds.Count, length),
            RejectedIds = ToTensor(rejectedIds, length),
            RejectedMask = ToMask(rejectedIds.Count, length),
            Admission = admitted.Admission
        };
    }

    // Convert token ids into [1, T] tensor shape.
    private static Tensor ToTensor(IReadOnlyList<int> ids, int length)
    {
        var data = new float[length];
        for (var index = 0; index < length; index++)
            data[index] = index < ids.Count ? ids[index] : 0;

        return new Tensor(data, new TensorShape(1, length));
    }

    // Convert sequence length into an attention mask.
    private static Tensor ToMask(int count, int length)
    {
        var data = new float[length];
        for (var index = 0; index < length; index++)
            data[index] = index < count ? 1f : 0f;

        return new Tensor(data, new TensorShape(1, length));
    }

    // Keep merged batch rows bounded so broad orchestration prompts do not create huge token sequences.
    private static string Compact(string text)
    {
        var compact = string.Join(" ", (text ?? string.Empty)
            .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return compact.Length <= 480 ? compact : compact[..480];
    }
}
