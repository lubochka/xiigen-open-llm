// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;

namespace VectorSearch;

/// <summary>
/// Hierarchical Navigable Small World (HNSW) approximate nearest-neighbour index.
///
/// Structure: each inserted vector gets a random max-level.
/// Upper levels are sparse (fast coarse navigation).
/// Level 0 contains all vectors (precise local search).
///
/// Insert: find entry point at top level, greedily descend to target level,
///         then connect the new node to its ef-construction nearest neighbours.
/// Search: greedy descent from top to level 1, then beam-search at level 0.
///
/// DNA rule 4: implements IVectorIndex.
/// </summary>
public class HnswIndex : IVectorIndex
{
    private readonly int _m;              // max neighbours per node per level
    private readonly int _efConstruction; // search width during build
    private int _efSearch;                // search width during query

    private readonly List<float[]> _vectors = [];
    private readonly List<string> _ids = [];
    private readonly List<Dictionary<string, string>> _meta = [];
    private readonly List<List<List<int>>> _graph = [];  // [node][level][neighbours]
    private int _entryPoint = -1;
    private int _maxLevel = -1;

    private static readonly Random _rng = new();

    public int Count => _vectors.Count;

    public HnswIndex(int m = 16, int efConstruction = 200, int efSearch = 50)
    {
        _m = m;
        _efConstruction = efConstruction;
        _efSearch = efSearch;
    }

    public void SetEfSearch(int ef) => _efSearch = ef;

    public void Add(float[] vector, string id, Dictionary<string, string>? metadata = null)
    {
        int newId = _vectors.Count;
        _vectors.Add(vector);
        _ids.Add(id);
        _meta.Add(metadata ?? []);

        int level = RandomLevel();
        var nodeLevels = new List<List<int>>(level + 1);
        for (int l = 0; l <= level; l++) nodeLevels.Add([]);
        _graph.Add(nodeLevels);

        if (_entryPoint == -1)
        {
            _entryPoint = newId;
            _maxLevel = level;
            return;
        }

        int currEntry = _entryPoint;

        // Greedy descent from top to level+1
        for (int l = _maxLevel; l > level; l--)
            currEntry = GreedySearch(vector, currEntry, l);

        // From level down to 0: find neighbours and bidirectionally connect
        for (int l = Math.Min(level, _maxLevel); l >= 0; l--)
        {
            int maxM = l == 0 ? _m * 2 : _m;
            var candidates = SearchLayer(vector, [currEntry], _efConstruction, l);
            var neighbours = SelectNeighbours(vector, candidates.Select(c => c.id).ToList(), maxM);

            foreach (int nb in neighbours)
            {
                _graph[newId][l].Add(nb);
                _graph[nb][l].Add(newId);
                if (_graph[nb][l].Count > maxM)
                    _graph[nb][l] = SelectNeighbours(_vectors[nb], _graph[nb][l], maxM);
            }

            if (candidates.Count > 0) currEntry = candidates[0].id;
        }

        if (level > _maxLevel)
        {
            _maxLevel = level;
            _entryPoint = newId;
        }
    }

    public List<(string id, float score)> Search(float[] query, int topK)
    {
        if (_entryPoint == -1) return [];

        int currEntry = _entryPoint;
        for (int l = _maxLevel; l > 0; l--)
            currEntry = GreedySearch(query, currEntry, l);

        var candidates = SearchLayer(query, [currEntry], Math.Max(_efSearch, topK), 0);
        return candidates.Take(topK)
            .Select(c => (_ids[c.id], c.score))
            .ToList();
    }

    // ── Persistence (DNA rule 7: atomic write) ───────────────────────────────

    public void Save(string path)
    {
        var data = new SerializedIndex
        {
            Vectors = _vectors.Select(v => v.ToList()).ToList(),
            Ids = _ids,
            Meta = _meta,
            Graph = _graph.Select(n => n.Select(l => l.ToList()).ToList()).ToList(),
            EntryPoint = _entryPoint,
            MaxLevel = _maxLevel,
            M = _m,
            EfConstruction = _efConstruction,
            EfSearch = _efSearch
        };
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(data));
        File.Move(tmp, path, overwrite: true);
    }

    public static HnswIndex Load(string path)
    {
        var data = JsonSerializer.Deserialize<SerializedIndex>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Failed to deserialize HNSW index");
        var idx = new HnswIndex(data.M, data.EfConstruction, data.EfSearch)
        {
            _entryPoint = data.EntryPoint,
            _maxLevel = data.MaxLevel
        };
        idx._vectors.AddRange(data.Vectors.Select(v => v.ToArray()));
        idx._ids.AddRange(data.Ids);
        idx._meta.AddRange(data.Meta);
        foreach (var node in data.Graph)
        {
            var levels = node.Select(l => l.ToList()).ToList();
            idx._graph.Add(levels);
        }
        return idx;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private int RandomLevel()
    {
        double ml = 1.0 / Math.Log(_m);
        int l = 0;
        while (_rng.NextDouble() < Math.Exp(-1.0 / ml) && l < 32) l++;
        return l;
    }

    private int GreedySearch(float[] query, int entry, int level)
    {
        int best = entry;
        float bestDist = Distances.CosineSimilarity(query, _vectors[entry]);
        bool changed = true;
        while (changed)
        {
            changed = false;
            if (level >= _graph[best].Count) break;
            foreach (int nb in _graph[best][level])
            {
                float d = Distances.CosineSimilarity(query, _vectors[nb]);
                if (d > bestDist) { bestDist = d; best = nb; changed = true; }
            }
        }
        return best;
    }

    // Descending by score; id breaks ties to ensure uniqueness in the set.
    private static readonly IComparer<(float score, int id)> ScoreDesc =
        Comparer<(float score, int id)>.Create((a, b) =>
        {
            int c = b.score.CompareTo(a.score);
            return c != 0 ? c : a.id.CompareTo(b.id);
        });

    private List<(int id, float score)> SearchLayer(float[] query, List<int> entries, int ef, int level)
    {
        var visited    = new HashSet<int>(entries);
        var candidates = new SortedSet<(float score, int id)>(ScoreDesc);
        var results    = new SortedSet<(float score, int id)>(ScoreDesc);

        foreach (int e in entries)
        {
            float d = Distances.CosineSimilarity(query, _vectors[e]);
            candidates.Add((d, e));
            results.Add((d, e));
        }

        while (candidates.Count > 0)
        {
            var best = candidates.Min;
            candidates.Remove(best);

            float worstResult = results.Count > 0 ? results.Max.score : float.MinValue;
            if (best.score < worstResult && results.Count >= ef) break;

            if (level < _graph[best.id].Count)
                foreach (int nb in _graph[best.id][level])
                {
                    if (!visited.Add(nb)) continue;
                    float d = Distances.CosineSimilarity(query, _vectors[nb]);
                    results.Add((d, nb));
                    candidates.Add((d, nb));
                    if (results.Count > ef) results.Remove(results.Max);
                }
        }

        return results.Take(ef).Select(t => (t.id, t.score)).ToList();
    }

    private List<int> SelectNeighbours(float[] query, List<int> candidates, int maxM)
    {
        return candidates
            .OrderByDescending(c => Distances.CosineSimilarity(query, _vectors[c]))
            .Take(maxM)
            .ToList();
    }

    private class SerializedIndex
    {
        public List<List<float>> Vectors { get; set; } = [];
        public List<string> Ids { get; set; } = [];
        public List<Dictionary<string, string>> Meta { get; set; } = [];
        public List<List<List<int>>> Graph { get; set; } = [];
        public int EntryPoint { get; set; }
        public int MaxLevel { get; set; }
        public int M { get; set; }
        public int EfConstruction { get; set; }
        public int EfSearch { get; set; }
    }
}
