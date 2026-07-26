// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

// TranslationCheckpointCache is the in-memory cache layer for loaded checkpoints.
// Design: cache-first → disk fallback (mirrors DefaultLogicalCapabilityTrainer pattern).
// Cache key includes file stamp so a checkpoint rewritten on disk is not served stale.
// This class is PUBLIC so TatoebaPilotLoopRunOptions can accept it and tests can create it directly.
public sealed class TranslationCheckpointCache
{
    // CacheKey identifies one cached checkpoint by path, schema version, and file stamp.
    // The file stamp (LastWriteUtcTicks + FileLength) detects in-process file replacement.
    private readonly record struct CacheKey(
        string AbsolutePath,
        int SchemaVersion,
        long LastWriteUtcTicks,
        long FileLength);

    // _cache maps file-stamp key to a loaded TranslationMachineLoadResult.
    // Caching the full result (not just Machine) ensures the runner can restore cursor on cache hit.
    private readonly Dictionary<CacheKey, TranslationMachineLoadResult> _cache = new();

    // TryGet returns a cached load result only when the file stamp still matches disk.
    // Returns false when the file is missing, replaced, or has never been cached.
    public bool TryGet(string absolutePath, out TranslationMachineLoadResult? result)
    {
        // Default out value to null before any path.
        result = null;
        // File must exist for the cache entry to be valid.
        if (!File.Exists(absolutePath)) return false;
        // Read current file metadata to build the validation key.
        var info = new FileInfo(absolutePath);
        var key = new CacheKey(absolutePath, 1, info.LastWriteTimeUtc.Ticks, info.Length);
        // Return the cached result only when key matches exactly.
        return _cache.TryGetValue(key, out result);
    }

    // Store saves a load result keyed by the current file stamp.
    // Called immediately after a successful LoadCheckpointAsync or SaveCheckpointAsync.
    public void Store(string absolutePath, TranslationMachineLoadResult loadResult)
    {
        // Skip storing when the file no longer exists (race condition guard).
        if (!File.Exists(absolutePath)) return;
        // Read current file metadata to build the cache key.
        var info = new FileInfo(absolutePath);
        // Write or overwrite the entry for this path.
        _cache[new CacheKey(absolutePath, 1, info.LastWriteTimeUtc.Ticks, info.Length)] = loadResult;
    }

    // Invalidate removes all cache entries for this path.
    // Called before writing a new checkpoint so the next TryGet falls through to disk.
    public void Invalidate(string absolutePath)
    {
        // Collect keys for this path first to avoid modifying the dictionary during iteration.
        var keysToRemove = _cache.Keys
            .Where(k => k.AbsolutePath == absolutePath)
            .ToList();
        // Remove all collected entries.
        foreach (var key in keysToRemove)
            _cache.Remove(key);
    }
}
