// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using GraphRag.Models;

namespace GraphRag;

/// <summary>
/// Merges local and global results, de-duplicates by NodeId, builds context text.
/// DNA rule 1: weights are named constants, not magic numbers.
/// </summary>
public class HybridRetriever
{
    private const double LocalWeight  = 0.6;
    private const double GlobalWeight = 0.4;
    private const int    MaxTokens    = 3_000;

    private readonly LocalSearch  _local;
    private readonly GlobalSearch _global;

    public HybridRetriever(LocalSearch local, GlobalSearch global)
    {
        _local  = local;
        _global = global;
    }

    public RetrievalContext Retrieve(string query, int topK = 15)
    {
        var localResults  = _local.Search(query, topK);
        var globalResults = _global.Search(query, topK: topK);

        var merged = MergeAndRank(localResults, globalResults, topK);
        string contextText = BuildContext(merged);

        return new RetrievalContext
        {
            Query       = query,
            Results     = merged,
            ContextText = contextText,
            TokenCount  = contextText.Length / 4  // rough estimate
        };
    }

    private static List<SearchResult> MergeAndRank(
        List<SearchResult> local, List<SearchResult> global, int topK)
    {
        var seen   = new HashSet<string>();
        var merged = new Dictionary<string, SearchResult>();

        foreach (var r in local)
        {
            merged[r.NodeId] = r with
            {
                CombinedScore = r.CombinedScore * LocalWeight,
                Source        = "local"
            };
            seen.Add(r.NodeId);
        }

        foreach (var r in global)
        {
            if (merged.TryGetValue(r.NodeId, out var existing))
            {
                merged[r.NodeId] = existing with
                {
                    CombinedScore = existing.CombinedScore + r.GraphScore * GlobalWeight,
                    Source        = "hybrid"
                };
            }
            else
            {
                merged[r.NodeId] = r with
                {
                    CombinedScore = r.GraphScore * GlobalWeight,
                    Source        = "global"
                };
            }
        }

        return merged.Values
            .OrderByDescending(r => r.CombinedScore)
            .Take(topK)
            .ToList();
    }

    private static string BuildContext(List<SearchResult> results)
    {
        var sb = new System.Text.StringBuilder();
        int used = 0;

        foreach (var r in results)
        {
            string snippet = string.IsNullOrEmpty(r.RawText)
                ? $"[{r.Type}] {r.Name}"
                : $"[{r.Type}] {r.Name}\n{r.RawText}";

            if (used + snippet.Length > MaxTokens * 4) break;
            sb.AppendLine(snippet);
            sb.AppendLine();
            used += snippet.Length;
        }

        return sb.ToString().TrimEnd();
    }
}
