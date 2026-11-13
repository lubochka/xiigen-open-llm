using GraphEngine.Models;

namespace GraphEngine;

/// <summary>
/// Power-iteration PageRank and personalised PageRank for graph nodes.
/// Damping factor d=0.85, convergence threshold 1e-6.
/// </summary>
public class PageRankCalculator
{
    private const double DampingFactor = 0.85;
    private const double Convergence  = 1e-6;
    private const int    MaxIter      = 100;

    public void ComputePageRank(KnowledgeGraph graph)
    {
        var ids = graph.Nodes.Keys.ToList();
        int n = ids.Count;
        if (n == 0) return;

        var rank = ids.ToDictionary(id => id, _ => 1.0 / n);
        var next  = new Dictionary<string, double>(n);

        for (int iter = 0; iter < MaxIter; iter++)
        {
            double dangling = 0;
            foreach (var id in ids)
                if (graph.OutEdges(id).Count == 0)
                    dangling += rank[id];

            double delta = 0;
            foreach (var id in ids)
            {
                double sum = 0;
                foreach (var edge in graph.InEdges(id))
                {
                    var outEdges = graph.OutEdges(edge.From);
                    double totalWeight = outEdges.Sum(e => e.Weight);
                    if (totalWeight > 0)
                        sum += rank[edge.From] * (edge.Weight / totalWeight);
                }

                double newRank = (1 - DampingFactor) / n
                               + DampingFactor * (sum + dangling / n);
                next[id] = newRank;
                delta    += Math.Abs(newRank - rank[id]);
            }

            foreach (var id in ids) rank[id] = next[id];
            if (delta < Convergence) break;
        }

        foreach (var id in ids)
            graph.Nodes[id].PageRankScore = rank[id];
    }

    /// <summary>
    /// Personalised PageRank — teleport probability distributes over <paramref name="seeds"/>.
    /// Returns scores for all nodes, sorted descending.
    /// </summary>
    public IReadOnlyList<(string id, double score)> PersonalisedPageRank(
        KnowledgeGraph graph, IEnumerable<string> seeds)
    {
        var ids  = graph.Nodes.Keys.ToList();
        int n    = ids.Count;
        if (n == 0) return [];

        var seedSet = seeds.ToHashSet();
        double seedWeight = seedSet.Count > 0 ? 1.0 / seedSet.Count : 0;

        var rank = ids.ToDictionary(id => id, _ => 1.0 / n);
        var next  = new Dictionary<string, double>(n);

        for (int iter = 0; iter < MaxIter; iter++)
        {
            double dangling = 0;
            foreach (var id in ids)
                if (graph.OutEdges(id).Count == 0)
                    dangling += rank[id];

            double delta = 0;
            foreach (var id in ids)
            {
                double teleport = seedSet.Contains(id) ? seedWeight : 0;
                double danglingShare = seedSet.Count > 0
                    ? (seedSet.Contains(id) ? dangling * seedWeight : 0)
                    : dangling / n;

                double sum = 0;
                foreach (var edge in graph.InEdges(id))
                {
                    var outEdges = graph.OutEdges(edge.From);
                    double totalWeight = outEdges.Sum(e => e.Weight);
                    if (totalWeight > 0)
                        sum += rank[edge.From] * (edge.Weight / totalWeight);
                }

                double newRank = (1 - DampingFactor) * teleport
                               + DampingFactor * (sum + danglingShare);
                next[id] = newRank;
                delta    += Math.Abs(newRank - rank[id]);
            }

            foreach (var id in ids) rank[id] = next[id];
            if (delta < Convergence) break;
        }

        return ids
            .Select(id => (id, rank[id]))
            .OrderByDescending(t => t.Item2)
            .ToList();
    }
}
