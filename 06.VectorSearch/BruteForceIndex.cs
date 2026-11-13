// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace VectorSearch;

/// <summary>
/// Exact nearest-neighbour search by scanning every vector.
/// O(n) per query — correct but slow for large collections.
/// Use for: small collections (&lt;10,000 vectors) or verifying HNSW correctness.
/// DNA rule 4: implements IVectorIndex interface.
/// </summary>
public class BruteForceIndex : IVectorIndex
{
    private readonly List<IndexEntry> _entries = [];

    // The dimension this index has accepted. -1 means no vector has been
    // accepted yet, so the next accepted one decides it. Same derived field as
    // in HnswIndex, the other implementation of IVectorIndex: nothing is stored
    // and no public shape changes, because this unit has no persistence at all.
    private int _dimensions = -1;

    public int Count => _entries.Count;

    // FIX-R29 sibling transfer: both repairs the neighbour index received on
    // 29-30 October are applied here, to the second implementation of the same
    // contract. Until now this unit checked no shape at all and stored the
    // caller's own array and dictionary, so one contract had two different
    // promises depending on which implementation the caller happened to hold.
    //
    // The shape is checked first, before the single line that changes state. A
    // rejected call must leave the index exactly as it was, so nothing below
    // this block may run first.
    public void Add(float[] vector, string id, Dictionary<string, string>? metadata = null)
    {
        // A missing array is a caller-contract violation, not a business outcome.
        if (vector is null)
            throw new ArgumentNullException(nameof(vector), "VECTOR_REQUIRED");
        // An empty vector carries no position, and as the very first one it would
        // fix the accepted dimension at zero and lock the whole index out.
        if (vector.Length == 0)
            throw new ArgumentException("VECTOR_EMPTY: a vector must have at least one component.", nameof(vector));
        // Once a dimension has been accepted, every later vector must match it.
        if (_dimensions >= 0 && vector.Length != _dimensions)
            throw new ArgumentException(
                $"VECTOR_DIMENSION_MISMATCH: expected {_dimensions}, actual {vector.Length}", nameof(vector));

        // From here the call is admitted; the first admitted vector fixes the dimension.
        _dimensions = vector.Length;

        // The entry is built from the index's own array and its own dictionary,
        // never from the caller's. Until these copies existed the caller could
        // change its own cells after the call and the stored row changed with
        // them: the same query answered differently with no Add, no re-training
        // and no reload in between, and no event marked the moment. The caller's
        // own key comparer travels with the dictionary copy, so lookup semantics
        // stay exactly what the caller chose and a case-colliding pair cannot
        // fail this copy. Nothing reads the caller's array after this line —
        // Search reads the stored entry — so there is no second place to thread
        // the copy through, unlike the neighbour index with its graph traversal.
        _entries.Add(new IndexEntry(
            id,
            [.. vector],
            metadata is null ? [] : new Dictionary<string, string>(metadata, metadata.Comparer)));
    }

    public List<(string id, float score)> Search(float[] query, int topK)
    {
        // An index that has accepted nothing answers an empty list for a query of
        // any length. This keeps the previous behaviour and matches the neighbour
        // index, where the empty-index return also stands before the shape check:
        // with no accepted dimension there is nothing to hold the query to.
        if (_dimensions < 0) return [];

        // A missing query is a caller-contract violation, not a business outcome.
        if (query is null)
            throw new ArgumentNullException(nameof(query), "QUERY_REQUIRED");
        // The shared cosine walks the length of its FIRST argument, which is the
        // query. So a shorter query used to score silently on a prefix of every
        // stored row, and a longer one used to run off the end of a stored row
        // mid-comparison. Both are refused here, before the first comparison.
        if (query.Length != _dimensions)
            throw new ArgumentException(
                $"QUERY_DIMENSION_MISMATCH: expected {_dimensions}, actual {query.Length}", nameof(query));

        return _entries
            .Select(e => (e.Id, Distances.CosineSimilarity(query, e.Vector)))
            .OrderByDescending(t => t.Item2)
            .Take(topK)
            .ToList();
    }

    // No stored-shape check on a load path here, unlike the neighbour index, and
    // that is deliberate: this unit has no persistence at all. Both members below
    // refuse by design, so no saved file exists whose rows could disagree on
    // length. If persistence is ever implemented, the same load-time checks the
    // neighbour index carries must come with it.
    public void Save(string path) =>
        throw new NotImplementedException("BruteForceIndex is in-memory only");

    public static BruteForceIndex Load(string path) =>
        throw new NotImplementedException("BruteForceIndex is in-memory only");
}
