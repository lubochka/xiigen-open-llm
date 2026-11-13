// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace VectorSearch;

/// <summary>DNA rule 4: interface contract for all vector index implementations.</summary>
public interface IVectorIndex
{
    int Count { get; }
    void Add(float[] vector, string id, Dictionary<string, string>? metadata = null);
    List<(string id, float score)> Search(float[] query, int topK);
}
