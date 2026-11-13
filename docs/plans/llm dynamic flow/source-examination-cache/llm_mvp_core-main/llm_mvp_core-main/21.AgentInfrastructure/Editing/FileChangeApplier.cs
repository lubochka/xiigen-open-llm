namespace AgentInfrastructure;

// Stage 06 — Applying changes to files.
// Applies a search-and-replace patch to a source file atomically.
// Fails if the search text appears zero times (nothing to replace)
// or more than once (ambiguous — the agent must be more specific).
public sealed class FileChangeApplier
{
    private readonly ICodeReader _reader;
    private readonly ICodeWriter _writer;

    public FileChangeApplier(ICodeReader reader, ICodeWriter writer)
        => (_reader, _writer) = (reader, writer);

    // Apply a single patch: find searchText in the file and replace it with replacementText.
    public async Task<OperationResult<bool>> ApplyPatchAsync(
        string filePath,
        string searchText,
        string replacementText)
    {
        if (string.IsNullOrEmpty(searchText))
            return OperationResult<bool>.Fail("Search text cannot be empty.", "EMPTY_SEARCH");

        var readResult = await _reader.ReadFileAsync(filePath);
        if (!readResult.IsSuccess)
            return OperationResult<bool>.Fail(readResult.ErrorMessage, readResult.ErrorCode);

        string content = readResult.Value!;
        int occurrences = CountOccurrences(content, searchText);

        if (occurrences == 0)
            return OperationResult<bool>.Fail(
                $"Search text not found in '{filePath}'.", "PATCH_NOT_FOUND");

        if (occurrences > 1)
            return OperationResult<bool>.Fail(
                $"Search text appears {occurrences} times in '{filePath}'. Make the patch more specific.",
                "PATCH_AMBIGUOUS");

        string patched = content.Replace(searchText, replacementText, StringComparison.Ordinal);
        return await _writer.WriteFileAsync(filePath, patched);
    }

    // Two-pass fuzzy patch: Pass 1 = exact Ordinal; Pass 2 = normalize whitespace.
    public async Task<OperationResult<bool>> ApplyPatchFuzzyAsync(
        string filePath,
        string searchText,
        string replacementText)
    {
        if (string.IsNullOrEmpty(searchText))
            return OperationResult<bool>.Fail("Search text cannot be empty.", "EMPTY_SEARCH");

        var readResult = await _reader.ReadFileAsync(filePath);
        if (!readResult.IsSuccess)
            return OperationResult<bool>.Fail(readResult.ErrorMessage, readResult.ErrorCode);

        string content = readResult.Value!;

        // Pass 1: exact Ordinal
        var exactResult = ApplyPatchToContent(content, searchText, replacementText, fuzzy: false);
        if (exactResult.IsSuccess || exactResult.ErrorCode != "PATCH_NOT_FOUND")
        {
            if (!exactResult.IsSuccess)
                return OperationResult<bool>.Fail(exactResult.ErrorMessage, exactResult.ErrorCode);
            return await _writer.WriteFileAsync(filePath, exactResult.Value!);
        }

        // Pass 2: fuzzy (normalize whitespace)
        var fuzzyResult = ApplyPatchToContent(content, searchText, replacementText, fuzzy: true);
        if (!fuzzyResult.IsSuccess)
            return OperationResult<bool>.Fail(fuzzyResult.ErrorMessage, fuzzyResult.ErrorCode);
        return await _writer.WriteFileAsync(filePath, fuzzyResult.Value!);
    }

    // Apply patches sequentially in memory; write once only if all succeed.
    // Returns MultiPatchResult with AppliedCount and FirstFailedIndex (-1 if all ok).
    public async Task<OperationResult<MultiPatchResult>> ApplyManyPatchesAsync(
        string filePath,
        IReadOnlyList<(string Search, string Replacement)> patches,
        bool useFuzzy = true)
    {
        if (patches.Count == 0)
            return OperationResult<MultiPatchResult>.Ok(new MultiPatchResult(0, null, -1));

        var readResult = await _reader.ReadFileAsync(filePath);
        if (!readResult.IsSuccess)
            return OperationResult<MultiPatchResult>.Fail(readResult.ErrorMessage, readResult.ErrorCode);

        string current = readResult.Value!;

        for (int i = 0; i < patches.Count; i++)
        {
            var (search, replacement) = patches[i];
            var patchResult = ApplyPatchToContent(current, search, replacement, fuzzy: useFuzzy);
            if (!patchResult.IsSuccess)
                return OperationResult<MultiPatchResult>.Ok(
                    new MultiPatchResult(i, patchResult.ErrorCode, i));
            current = patchResult.Value!;
        }

        await _writer.WriteFileAsync(filePath, current);
        return OperationResult<MultiPatchResult>.Ok(new MultiPatchResult(patches.Count, null, -1));
    }

    // Normalize line endings and trailing whitespace per line.
    // CRLF → LF, each line TrimEnd'd.
    internal static string NormalizeWhitespace(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');
        return string.Join("\n", lines.Select(l => l.TrimEnd()));
    }

    // Apply a single in-memory patch. When fuzzy=true: Pass 1 exact, Pass 2 normalized.
    // Returns the patched string on success, or a failure OperationResult.
    private static OperationResult<string> ApplyPatchToContent(
        string content, string searchText, string replacementText, bool fuzzy)
    {
        if (string.IsNullOrEmpty(searchText))
            return OperationResult<string>.Fail("Search text cannot be empty.", "EMPTY_SEARCH");

        int occurrences = CountOccurrences(content, searchText);

        if (occurrences == 1)
            return OperationResult<string>.Ok(
                content.Replace(searchText, replacementText, StringComparison.Ordinal));

        if (occurrences > 1)
            return OperationResult<string>.Fail(
                $"Search text appears {occurrences} times. Make the patch more specific.",
                "PATCH_AMBIGUOUS");

        // occurrences == 0
        if (!fuzzy)
            return OperationResult<string>.Fail("Search text not found.", "PATCH_NOT_FOUND");

        // Fuzzy pass: normalize both content and search text
        string normContent = NormalizeWhitespace(content);
        string normSearch  = NormalizeWhitespace(searchText);

        int fuzzyCount = CountOccurrences(normContent, normSearch);
        if (fuzzyCount == 0)
            return OperationResult<string>.Fail("Search text not found.", "PATCH_NOT_FOUND");
        if (fuzzyCount > 1)
            return OperationResult<string>.Fail(
                "Search text appears multiple times after normalization. Make the patch more specific.",
                "PATCH_AMBIGUOUS");

        // Exactly 1 fuzzy match — map back to original positions
        int normStart = normContent.IndexOf(normSearch, StringComparison.Ordinal);
        int normEnd   = normStart + normSearch.Length;

        int[] map      = BuildNormToOrigMap(content);
        int origStart  = map[normStart];
        int origEnd    = map[normEnd - 1] + 1;

        string patched = content[..origStart] + replacementText + content[origEnd..];
        return OperationResult<string>.Ok(patched);
    }

    // Build map: normalizedIndex → originalIndex.
    // Trailing spaces and \r chars are skipped in normalized, so their positions are absent.
    private static int[] BuildNormToOrigMap(string original)
    {
        var normIndices = new List<int>(original.Length);
        var pending     = new List<int>(); // buffered space/tab original indices (may be trailing)

        for (int oi = 0; oi < original.Length; )
        {
            char c = original[oi];

            if (c == '\r' && oi + 1 < original.Length && original[oi + 1] == '\n')
            {
                // \r\n: trailing spaces dropped, \r dropped, \n emitted
                pending.Clear();
                oi++; // skip \r
                normIndices.Add(oi); // \n
                oi++;
            }
            else if (c == '\n')
            {
                // trailing spaces dropped, \n emitted
                pending.Clear();
                normIndices.Add(oi);
                oi++;
            }
            else if (c is ' ' or '\t')
            {
                pending.Add(oi);
                oi++;
            }
            else
            {
                // non-space, non-newline: buffered spaces are mid-line → keep them
                normIndices.AddRange(pending);
                pending.Clear();
                normIndices.Add(oi);
                oi++;
            }
        }
        // trailing spaces/tabs at EOF are dropped (TrimEnd)

        var map = new int[normIndices.Count + 1];
        for (int j = 0; j < normIndices.Count; j++)
            map[j] = normIndices[j];
        map[normIndices.Count] = original.Length; // sentinel
        return map;
    }

    // Count non-overlapping occurrences of needle in haystack.
    private static int CountOccurrences(string haystack, string needle)
    {
        int count = 0;
        int index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }
}

public sealed record MultiPatchResult(
    int     AppliedCount,
    string? FirstFailedErrorCode,
    int     FirstFailedIndex);
