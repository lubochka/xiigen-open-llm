// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using DPO;
using DPO.Models;
using Embeddings;
using LoRA;
using SemanticCompilerAdapter;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
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

// This logical unit connects tokenizer, transformer embeddings, and HNSW retrieval to ExecuteAsync.
public sealed class TransformerEmbeddingKnnLogicalUnit<TOutput> : ILogicalUnit<string, TOutput>
{
    private readonly string _unitId;
    private readonly string _unitRole;
    private readonly string _domainTag;
    private readonly EmbeddingModel _embeddingModel;
    private readonly HnswIndex _index;
    private readonly Dictionary<string, TransformerKnnTrainingExample<TOutput>> _examples;

    private TransformerEmbeddingKnnLogicalUnit(
        string unitId,
        string unitRole,
        string domainTag,
        EmbeddingModel embeddingModel,
        HnswIndex index,
        Dictionary<string, TransformerKnnTrainingExample<TOutput>> examples,
        LlmIngredientManifest manifest)
    {
        _unitId = unitId;
        _unitRole = unitRole;
        _domainTag = domainTag;
        _embeddingModel = embeddingModel;
        _index = index;
        _examples = examples;
        Manifest = manifest;
    }

    // The manifest lets tests and inspectors prove this is not a hardcoded deterministic unit.
    public LlmIngredientManifest Manifest { get; }

    // Train the unit by embedding examples with the real transformer-backed embedding model.
    public static OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>> Train(
        string unitId,
        string unitRole,
        string domainTag,
        LlmModel model,
        BpeTokenizer tokenizer,
        IReadOnlyList<TransformerKnnTrainingExample<TOutput>> examples)
    {
        // Reject missing ingredients through the shared envelope.
        if (model is null)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Transformer model is required.", "TRANSFORMER_MODEL_REQUIRED");
        if (tokenizer is null)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Tokenizer is required.", "TOKENIZER_REQUIRED");
        if (examples is null || examples.Count == 0)
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("At least one training example is required.", "TRAINING_EXAMPLES_REQUIRED");

        // Build the transformer-backed embedding model.
        var embeddingModel = new EmbeddingModel(model, tokenizer, maxLen: Math.Min(64, model.Config.ContextLen));

        // Build a small HNSW index over the produced embeddings.
        var index = new HnswIndex(m: 8, efConstruction: 32, efSearch: 16);
        var exampleMap = new Dictionary<string, TransformerKnnTrainingExample<TOutput>>(StringComparer.Ordinal);
        foreach (var example in examples)
        {
            // Reject incomplete examples before indexing.
            if (string.IsNullOrWhiteSpace(example.ExampleId) || string.IsNullOrWhiteSpace(example.Text))
                return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Fail("Training examples require id and text.", "INVALID_TRAINING_EXAMPLE");

            // Embed and index the example.
            var vector = embeddingModel.Embed(example.Text);
            index.Add(vector, example.ExampleId, new Dictionary<string, string>
            {
                ["unit_id"] = unitId,
                ["domain_tag"] = domainTag
            });
            exampleMap[example.ExampleId] = example;
        }

        // Build the manifest with every concrete ingredient used by this path.
        var manifest = new LlmIngredientManifest(
            [
                "01.Tokenizer:BpeTokenizer",
                "03.TransformerModel:LlmModel",
                "05.Embeddings:EmbeddingModel",
                "06.VectorSearch:HnswIndex",
                "E15:SEMANTIC_ROOT_TOKENIZATION",
                "E25:PATTERN_CLASSIFIER",
                "E25:PATTERN_CLASSIFIER_FUSION",
                "E34V2:BM25_KNN_FUSION"
            ],
            examples.Count,
            model.Config.HiddenDim,
            LoraAdapterCount: 0,
            DpoStepCount: 0);

        // Return the executable learned unit.
        return OperationResult<TransformerEmbeddingKnnLogicalUnit<TOutput>>.Ok(new TransformerEmbeddingKnnLogicalUnit<TOutput>(
            unitId,
            unitRole,
            domainTag,
            embeddingModel,
            index,
            exampleMap,
            manifest));
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
        var selected = scored[0];
        var example = _examples[selected.ExampleId];

        // Convert the fused score into the logical confidence range.
        var confidence = Math.Round(Math.Clamp(0.50 + (selected.FusedScore * 0.49), 0.0, 0.99), 2);

        // Return the selected learned output.
        var result = new LogicalUnitResult<TOutput>(
            _unitId,
            _unitRole,
            _domainTag,
            example.Output,
            confidence,
            example.PredicateIds,
            example.DomainTags,
            [example.ExampleId, "E34V2:BM25_KNN_FUSION"],
            []);

        return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Ok(result));
    }

    private static readonly Lazy<SemanticCompiler29.PatternClassifier> PatternClassifier = new(
        () => new SemanticCompiler29.PatternClassifier(new InMemoryCompilerStorage(), new NoopCompilerEventLogger()));

    // Score HNSW candidates with vector, lexical BM25-like, and structural pattern evidence.
    private ScoredTransformerCandidate[] ScoreCandidates(string query, IReadOnlyList<(string id, float score)> candidates)
    {
        var queryTokens = Tokenize(query);
        var queryPattern = ClassifyPattern(query);
        var rawScores = new List<(string id, double vectorScore, double lexicalScore, double patternScore)>();
        foreach (var candidate in candidates)
        {
            var example = _examples[candidate.id];
            var vectorScore = Math.Clamp((candidate.score + 1.0) / 2.0, 0.0, 1.0);
            var lexicalScore = ComputeBm25LikeScore(queryTokens, example.Text);
            var patternScore = string.Equals(queryPattern, ClassifyPattern(example.Text), StringComparison.Ordinal)
                ? 1.0
                : 0.0;
            rawScores.Add((candidate.id, vectorScore, lexicalScore, patternScore));
        }

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
    private double ComputeBm25LikeScore(IReadOnlyDictionary<string, int> queryTokens, string document)
    {
        if (queryTokens.Count == 0 || string.IsNullOrWhiteSpace(document))
            return 0.0;

        var documentTokens = Tokenize(document);
        if (documentTokens.Count == 0)
            return 0.0;

        var documentLength = Math.Max(1, documentTokens.Values.Sum());
        var averageLength = Math.Max(1.0, _examples.Values.Select(example => Tokenize(example.Text).Values.Sum()).DefaultIfEmpty(1).Average());
        var score = 0.0;
        const double k1 = 1.2;
        const double b = 0.75;
        foreach (var token in queryTokens.Keys)
        {
            if (!documentTokens.TryGetValue(token, out var frequency))
                continue;

            var documentFrequency = _examples.Values.Count(example => Tokenize(example.Text).ContainsKey(token));
            var idf = Math.Log(1.0 + ((_examples.Count - documentFrequency + 0.5) / (documentFrequency + 0.5)));
            var numerator = frequency * (k1 + 1.0);
            var denominator = frequency + (k1 * (1.0 - b + (b * documentLength / averageLength)));
            score += idf * numerator / Math.Max(1.0e-9, denominator);
        }

        return score;
    }

    // Tokenize text through the same project-23 root normalizer used by teaching vectors.
    private static Dictionary<string, int> Tokenize(string text)
    {
        var tokens = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var token in TrainingVectorBuilder.NormalizeRoots(text).Where(token => token.Length > 1))
        {
            tokens[token] = tokens.TryGetValue(token, out var count) ? count + 1 : 1;
        }

        return tokens;
    }

    private static string ClassifyPattern(string text)
    {
        var tokens = TrainingVectorBuilder.NormalizeRoots(text).ToArray();
        return tokens.Length == 0
            ? "UNKNOWN"
            : PatternClassifier.Value.Classify(tokens);
    }

    private sealed record ScoredTransformerCandidate(string ExampleId, double FusedScore);

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
public sealed record LogicalDpoPreferenceExample(string Prompt, string Chosen, string Rejected, float[]? TeachingVector = null);

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
    int ConvergenceWindowSteps = 25);

// This record reports one DPO teaching run.
public sealed record LogicalDpoTrainingResult(
    int PreferenceCount,
    int DpoStepCount,
    int LoraAdapterCount,
    float LastLoss,
    float LastAccuracy,
    long TrainingTimeMs,
    string[] MetricFilePaths,
    string[] IngredientIds);

// This teaching unit connects LoRA and DPO training to the logical-unit interface.
public sealed class LogicalDpoTrainingUnit : ILogicalUnit<LogicalDpoTrainingRequest, LogicalDpoTrainingResult>
{
    // This id identifies the first DPO teaching logical unit.
    public const string Id = "unit-logical-dpo-training-v1";

    // This role names the model-teaching responsibility.
    public const string Role = "logical-dpo-training";

    // This domain tag scopes the unit to model teaching.
    public const string DomainTag = "logical-learning";

    // Execute one DPO teaching pass.
    public Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecuteAsync(
        LogicalUnitRequest<LogicalDpoTrainingRequest> request,
        CancellationToken cancellationToken = default)
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
        if (request.Input.Preferences is null || request.Input.Preferences.Length == 0)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("At least one preference example is required.", "DPO_PREFERENCES_REQUIRED"));

        // Apply LoRA adapters before DPO so the teaching path includes the adapter tier.
        var adapters = LoraApplicator.Apply(request.Input.PolicyModel, request.Input.LoraConfig ?? new LoraConfig());
        if (!string.IsNullOrWhiteSpace(request.Input.ParentPolicyCheckpointPath))
        {
            try
            {
                request.Input.PolicyModel.LoadCheckpoint(request.Input.ParentPolicyCheckpointPath);
            }
            catch (Exception ex)
            {
                return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail(ex.Message, "PARENT_POLICY_CHECKPOINT_LOAD_FAILED"));
            }
        }

        // Execute one batched DPO optimizer update. Each preference stays short, while
        // gradients are averaged and applied once for the whole same-domain row.
        var started = DateTimeOffset.UtcNow;
        var trainer = new DpoTrainer(request.Input.PolicyModel, request.Input.ReferenceModel, request.Input.CheckpointDirectory);
        var batches = request.Input.Preferences
            .Select(preference => new DpoVectorBatch(
                BuildBatch(request.Input.Tokenizer, preference),
                preference.TeachingVector ?? TrainingVectorBuilder.Build(preference.Prompt),
                1f))
            .ToArray();
        DPO.Models.DpoMetrics? lastMetrics = null;
        var maxStepCount = Math.Max(1, request.Input.MaxStepCount);
        var windowSteps = Math.Max(1, request.Input.ConvergenceWindowSteps);
        var monitor = new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(request.Input.ConvergenceThreshold, windowSteps);
        var stepsTaken = 0;
        for (var step = 1; step <= maxStepCount; step++)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Fail("Cancelled.", "CANCELLED"));

            lastMetrics = trainer.StepVector(batches, step);
            stepsTaken = step;
            if (step % windowSteps == 0 && monitor.ShouldStop(lastMetrics.Loss))
                break;
        }

        // Collect generated metric files.
        var metricFiles = Directory.Exists(request.Input.CheckpointDirectory)
            ? Directory.GetFiles(request.Input.CheckpointDirectory, "dpo_step_*.json").OrderBy(path => path, StringComparer.Ordinal).ToArray()
            : [];

        // Build the teaching result.
        var trainingTimeMs = (long)(DateTimeOffset.UtcNow - started).TotalMilliseconds;
        var output = new LogicalDpoTrainingResult(
            request.Input.Preferences.Length,
            DpoStepCount: stepsTaken,
            adapters.Count,
            lastMetrics?.Loss ?? 0f,
            lastMetrics?.Accuracy ?? 0f,
            trainingTimeMs,
            metricFiles,
            [
                "01.Tokenizer:BpeTokenizer",
                "03.TransformerModel:LlmModel",
                "10.LoRA:LoraApplicator",
                "11.DPO:DpoTrainer",
                "11.DPO:DpoTrainer.StepVector",
                "23.SemanticCompilerAdapter:TrainingVectorBuilder"
            ]);

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

    // Convert one preference example into the tensor shape expected by DpoTrainer.
    private static DpoBatch BuildBatch(BpeTokenizer tokenizer, LogicalDpoPreferenceExample preference)
    {
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
            RejectedMask = ToMask(rejectedIds.Count, length)
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
