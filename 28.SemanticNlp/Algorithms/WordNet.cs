// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticNlp;

// ─── Subproject 30 — WordNet (synonym sets and hierarchy) ───────────────────
// WordNet groups words by sense rather than by spelling. A synset is a set
// of synonyms that share one meaning; synsets link to broader synsets
// (hypernyms) and narrower ones (hyponyms), forming a directed-acyclic
// hierarchy. The annotator picks the first synset for each input word and
// reports it as the word's "semantic family".

// ─── SynsetDatabase ─────────────────────────────────────────────────────────
// Holds synsets in memory, indexed by id and reverse-indexed by word.
// Implements ISemanticLexicon for FindSynsetsForWordAsync. The
// FrameNet/synset interface methods that do not apply here return empty
// lists per the contract.
public sealed class SynsetDatabase : ISemanticLexicon
{
    // Map: synsetId → synset record.
    private readonly Dictionary<string, Synset> _synsetById;

    // Map: lower-cased word → list of synset ids that contain the word.
    private readonly Dictionary<string, List<string>> _wordToSynsetIds;

    public SynsetDatabase(IEnumerable<Synset> synsets)
    {
        // Build the by-id index in one pass; require unique ids (fail fast).
        _synsetById = new Dictionary<string, Synset>();
        foreach (var synset in synsets)
        {
            if (_synsetById.ContainsKey(synset.SynsetId))
                throw new ArgumentException($"Duplicate synset id '{synset.SynsetId}'", nameof(synsets));
            _synsetById[synset.SynsetId] = synset;
        }

        // Build the reverse word index using case-insensitive keys.
        _wordToSynsetIds = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var synset in _synsetById.Values)
        {
            foreach (var word in synset.Words)
            {
                string key = word.ToLowerInvariant();
                if (!_wordToSynsetIds.TryGetValue(key, out var ids))
                {
                    ids = [];
                    _wordToSynsetIds[key] = ids;
                }
                if (!ids.Contains(synset.SynsetId))
                    ids.Add(synset.SynsetId);
            }
        }
    }

    // Default synset hierarchy used by tests — small, deterministic, sufficient
    // to demonstrate hypernym chains and Wu-Palmer similarity.
    public static SynsetDatabase CreateDefault() => new(
    [
        // Top of the noun hierarchy.
        new Synset { SynsetId = "entity.n.01",  Words = ["entity"],  Definition = "anything that exists",
                     HypernymIds = []                                   },
        new Synset { SynsetId = "object.n.01",  Words = ["object", "thing"], Definition = "physical thing",
                     HypernymIds = ["entity.n.01"]                       },
        new Synset { SynsetId = "animal.n.01",  Words = ["animal", "beast"], Definition = "living organism",
                     HypernymIds = ["object.n.01"]                       },
        new Synset { SynsetId = "mammal.n.01",  Words = ["mammal"],  Definition = "warm-blooded vertebrate",
                     HypernymIds = ["animal.n.01"]                       },
        new Synset { SynsetId = "canine.n.01",  Words = ["canine"],  Definition = "carnivorous mammal of the dog family",
                     HypernymIds = ["mammal.n.01"]                       },
        new Synset { SynsetId = "dog.n.01",     Words = ["dog", "domestic dog"], Definition = "the domestic dog",
                     HypernymIds = ["canine.n.01"]                       },
        new Synset { SynsetId = "wolf.n.01",    Words = ["wolf"],    Definition = "wild canine",
                     HypernymIds = ["canine.n.01"]                       },
        new Synset { SynsetId = "cat.n.01",     Words = ["cat", "feline"], Definition = "the domestic cat",
                     HypernymIds = ["mammal.n.01"]                       },

        // The Attack synset family (verbs and the related noun).
        new Synset { SynsetId = "attack.n.01",  Words = ["attack", "assault"], Definition = "an offensive action against an enemy",
                     HypernymIds = ["entity.n.01"]                       },
        new Synset { SynsetId = "attack.v.01",  Words = ["attack", "assault", "assail"], Definition = "launch an attack against",
                     HypernymIds = ["entity.n.01"]                       },
        new Synset { SynsetId = "purchase.v.01", Words = ["purchase", "buy", "acquire"], Definition = "obtain by paying",
                     HypernymIds = ["entity.n.01"]                       },
    ]);

    // Find synsets that contain the given word; empty list when unknown.
    public Task<OperationResult<IReadOnlyList<string>>> FindSynsetsForWordAsync(string word)
    {
        if (string.IsNullOrEmpty(word))
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
                (IReadOnlyList<string>)Array.Empty<string>()));

        if (!_wordToSynsetIds.TryGetValue(word, out var ids))
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
                (IReadOnlyList<string>)Array.Empty<string>()));

        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)ids.ToList()));
    }

    // WordNet has no frames — return empty list per ISemanticLexicon contract.
    public Task<OperationResult<IReadOnlyList<string>>> FindFramesForWordAsync(string word)
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)Array.Empty<string>()));

    // Members of a synset = the synonym words listed in the synset record.
    public Task<OperationResult<IReadOnlyList<string>>> GetFrameMembersAsync(string synsetId)
    {
        if (!_synsetById.TryGetValue(synsetId, out var synset))
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Fail(
                $"Synset '{synsetId}' not found", "SYNSET_NOT_FOUND"));

        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)synset.Words.ToList()));
    }

    // Direct lookup by id; used by the navigator.
    public OperationResult<Synset> GetSynset(string synsetId)
        => _synsetById.TryGetValue(synsetId, out var synset)
            ? OperationResult<Synset>.Ok(synset)
            : OperationResult<Synset>.Fail($"Synset '{synsetId}' not found", "SYNSET_NOT_FOUND");
}

// ─── SynsetNavigator ────────────────────────────────────────────────────────
// Walks the synset hierarchy and computes Wu-Palmer similarity between two
// words. The navigator does no I/O — it only follows pointers in the
// SynsetDatabase that was injected through the constructor.
public sealed class SynsetNavigator
{
    private readonly SynsetDatabase _database;

    public SynsetNavigator(SynsetDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    // Build the chain of hypernyms from a synset up to (and including) the root.
    // The first element is the synset itself, the last element is the most general ancestor.
    public OperationResult<IReadOnlyList<string>> GetHypernymChain(string synsetId, int maxDepth = 0)
    {
        var chain        = new List<string>();
        string currentId = synsetId;
        int depth        = 0;

        // Cycle protection — the data is supposed to be acyclic but we guard anyway.
        var seen = new HashSet<string>();

        while (!string.IsNullOrEmpty(currentId) && (maxDepth == 0 || depth < maxDepth))
        {
            if (!seen.Add(currentId)) break;

            var lookup = _database.GetSynset(currentId);
            if (!lookup.IsSuccess) break;

            chain.Add(currentId);

            // Move to the first declared parent; if none we have hit the root.
            currentId = lookup.Value!.HypernymIds.Count > 0
                ? lookup.Value.HypernymIds[0]
                : string.Empty;
            depth++;
        }

        return OperationResult<IReadOnlyList<string>>.Ok(chain);
    }

    // Wu-Palmer similarity = 2 * depth(LCA) / (depth(s1) + depth(s2))
    // where LCA is the lowest common ancestor of the two synsets in the
    // hypernym tree, and depth(x) is the distance from x to the root.
    public async Task<OperationResult<double>> ComputeSimilarityAsync(string word1, string word2)
    {
        var ids1Result = await _database.FindSynsetsForWordAsync(word1);
        var ids2Result = await _database.FindSynsetsForWordAsync(word2);

        if (!ids1Result.IsSuccess || ids1Result.Value!.Count == 0)
            return OperationResult<double>.Fail($"Word '{word1}' has no synsets", "WORD_NOT_FOUND");
        if (!ids2Result.IsSuccess || ids2Result.Value!.Count == 0)
            return OperationResult<double>.Fail($"Word '{word2}' has no synsets", "WORD_NOT_FOUND");

        // For pedagogical simplicity we compare the first sense of each word.
        // Production code would search every synset pair and pick the maximum.
        string id1 = ids1Result.Value![0];
        string id2 = ids2Result.Value![0];

        // Identical synsets — perfect similarity.
        if (id1 == id2) return OperationResult<double>.Ok(1.0);

        var chain1 = GetHypernymChain(id1).Value!;
        var chain2 = GetHypernymChain(id2).Value!;

        // Find the lowest common ancestor (the first id in chain2 that also appears in chain1).
        var chain1Set = chain1.ToHashSet();
        string lcaId  = chain2.FirstOrDefault(id => chain1Set.Contains(id)) ?? string.Empty;

        if (string.IsNullOrEmpty(lcaId))
            return OperationResult<double>.Ok(0.0);

        // Depth(synset) = number of nodes from synset down to root, inclusive of the root.
        // Depth(LCA) measured from the root, so we want the root distance not the offset.
        var lcaChainLength = GetHypernymChain(lcaId).Value!.Count;

        // Depth-from-root values for s1 and s2 are the lengths of their hypernym chains.
        int depth1   = chain1.Count;
        int depth2   = chain2.Count;
        int lcaDepth = lcaChainLength;

        // Guard against division by zero when both chains are length zero.
        if (depth1 + depth2 == 0)
            return OperationResult<double>.Ok(0.0);

        double similarity = 2.0 * lcaDepth / (depth1 + depth2);
        return OperationResult<double>.Ok(Math.Clamp(similarity, 0.0, 1.0));
    }
}

// ─── WordNetAnnotator ───────────────────────────────────────────────────────
// For each token in the sentence, look up the first matching synset and
// emit it as the token's semantic family. Tokens with no synset get a
// null family, mirroring the WordNet approach of not labelling unknowns.
public sealed class WordNetAnnotator
{
    private readonly SynsetDatabase _database;

    public WordNetAnnotator(SynsetDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return OperationResult<SemanticAnnotation>.Fail(
                "Sentence is empty", "EMPTY_SENTENCE");

        var tokens = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        var annotated = new List<SemanticToken>(tokens.Count);

        for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            string surface = tokens[tokenIndex];
            string clean   = surface.Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();

            // Look up synsets for the cleaned form.
            var lookup = await _database.FindSynsetsForWordAsync(clean);
            string? synsetId = (lookup.IsSuccess && lookup.Value!.Count > 0)
                ? lookup.Value![0]
                : null;

            annotated.Add(new SemanticToken
            {
                OriginalText     = surface,
                Position         = tokenIndex,
                SemanticFamily   = synsetId is null ? null : $"synset:{synsetId}",
                RoleWithinFamily = synsetId is null ? null : "Member",
            });
        }

        return OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
        {
            OriginalSentence = sentence,
            Tokens           = annotated,
            AlgorithmName    = "WordNet",
        });
    }
}

// ─── WordNetService ─────────────────────────────────────────────────────────
public sealed class WordNetService : BaseService, ISemanticAnnotator
{
    private readonly WordNetAnnotator _annotator;

    public string AlgorithmName => "WordNet";

    public WordNetService(
        WordNetAnnotator     annotator,
        ISemanticStorage     storage,
        ISemanticEventLogger eventLogger)
        : base(storage, eventLogger)
    {
        _annotator = annotator ?? throw new ArgumentNullException(nameof(annotator));
    }

    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(
        string operationId, string sentence)
    {
        if (Idempotency.TryGetResult(operationId, out var cached)
            && cached.TryGetValue("annotation", out var cachedObj)
            && cachedObj is SemanticAnnotation cachedAnnotation)
        {
            return OperationResult<SemanticAnnotation>.Ok(cachedAnnotation);
        }

        var result = await _annotator.AnnotateAsync(sentence);
        if (!result.IsSuccess) return result;

        await Storage.SaveAnnotationAsync(operationId, result.Value!);

        EventLogger.Log(new StandardEvent(
            EventType:   "semantic.annotation.completed",
            OperationId: operationId,
            OccurredAt:  DateTimeOffset.UtcNow,
            Payload:     new Dictionary<string, object>
            {
                ["algorithm"] = AlgorithmName,
                ["sentence"]  = sentence,
            }));

        Idempotency.Record(operationId, new Dictionary<string, object>
        {
            ["annotation"] = result.Value!,
        });

        return result;
    }
}
