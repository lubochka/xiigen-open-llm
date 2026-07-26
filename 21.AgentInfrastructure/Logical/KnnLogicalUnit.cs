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
    double ConfidenceFloor,
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
    private readonly double _confidenceFloor;

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
        int vectorDimensions,
        int k,
        double confidenceFloor,
        LogicalKnnModelManifest manifest)
    {
        _unitId = unitId;
        _unitRole = unitRole;
        _domainTag = domainTag;
        _tokenizer = tokenizer;
        _index = index;
        _examples = examples;
        _textSelector = textSelector;
        _vectorDimensions = vectorDimensions;
        _k = k;
        _confidenceFloor = confidenceFloor;
        Manifest = manifest;
    }

    // Train one learned logical unit from labelled examples.
    public static OperationResult<KnnLogicalUnit<TInput, TOutput>> Train(
        string unitId,
        string unitRole,
        string domainTag,
        IReadOnlyList<LogicalKnnTrainingExample<TInput, TOutput>> examples,
        Func<TInput, string> textSelector,
        int vectorDimensions = 64,
        int k = 3,
        double confidenceFloor = 0.70)
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

        // Normalize model parameters.
        var dimensions = Math.Max(MinimumVectorDimensions, vectorDimensions);
        var safeK = Math.Max(1, Math.Min(k, examples.Count));
        var safeConfidenceFloor = Math.Clamp(confidenceFloor, 0.0, 1.0);

        try
        {
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
            foreach (var example in examples)
            {
                // Reject examples without stable identity.
                if (string.IsNullOrWhiteSpace(example.ExampleId))
                    return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Fail("Training example id is required.", "TRAINING_EXAMPLE_ID_REQUIRED");

                // Store the example and add its vector.
                var trimmedId = example.ExampleId.Trim();
                exampleMap[trimmedId] = example with { ExampleId = trimmedId };
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
                safeConfidenceFloor,
                stopwatch.Elapsed.TotalMilliseconds,
                Math.Max(0, allocationAfter - allocationBefore),
                "01.Tokenizer:BpeTokenizer",
                "06.VectorSearch:HnswIndex",
                [
                    "01.Tokenizer:BpeTokenizer",
                    "06.VectorSearch:HnswIndex",
                    "logical-predicate-vector-tier",
                    "knn-nearest-neighbour-selection"
                ]);

            // Return the trained logical unit.
            return OperationResult<KnnLogicalUnit<TInput, TOutput>>.Ok(new KnnLogicalUnit<TInput, TOutput>(
                unitId.Trim(),
                unitRole.Trim(),
                domainTag.Trim(),
                tokenizer,
                index,
                exampleMap,
                textSelector,
                dimensions,
                safeK,
                safeConfidenceFloor,
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
            // Build query predicates from metadata when present.
            var queryPredicates = request.Metadata.TryGetValue("predicate_ids", out var predicateObject) && predicateObject is string[] predicateArray
                ? NormalizePredicates(predicateArray)
                : new HashSet<string>(StringComparer.Ordinal);

            // Search nearest training examples.
            var query = Vectorize(_tokenizer, _textSelector(request.Input), queryPredicates, _vectorDimensions);
            var hits = _index.Search(query, _k);
            if (hits.Count == 0)
                return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("No learned neighbours were found.", "NO_KNN_NEIGHBOURS"));

            // Use the nearest neighbour output as the learned prediction.
            var best = hits[0];
            var confidence = Math.Round(Math.Clamp(best.score, 0.0f, 1.0f), 4);
            var bestExample = _examples[best.id];

            // Low-confidence results remain visible in warnings instead of being invented as a clean pass.
            var warnings = request.Flags.UseConfidenceFloor && confidence < _confidenceFloor
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
                bestExample.Output,
                confidence,
                bestExample.PredicateIds,
                bestExample.DomainTags.Length == 0 ? [_domainTag] : bestExample.DomainTags,
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
