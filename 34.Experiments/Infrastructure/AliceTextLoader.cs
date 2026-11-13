// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.RegularExpressions;

namespace Experiments;

// ─── AliceTextLoader: download + cache Alice in Wonderland, tokenize by word ──
//
// Scale tiers (word counts):
//   Scale 0 — first  2 000 words  (chapters 1–2)
//   Scale 1 — first  8 000 words  (~1/3 of book)
//   Scale 2 — first 15 000 words  (~1/2 of book)
//   Scale 3 — all   28 000 words  (full book)

public static class AliceTextLoader
{
    private static readonly int[] WordLimits = [2_000, 8_000, 15_000, int.MaxValue];
    private const string AliceUrl = "https://www.gutenberg.org/files/11/11-0.txt";
    private static readonly Regex NonAlpha = new(@"[^a-z]", RegexOptions.Compiled);
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    public static OperationResult<AliceDataset> Load(int scale, string projectRoot)
    {
        if (scale < 0 || scale > 3)
            return OperationResult<AliceDataset>.Fail($"Alice scale must be 0–3, got {scale}");

        string dataDir = Path.Combine(projectRoot, "data");
        Directory.CreateDirectory(dataDir);
        string path = Path.Combine(dataDir, "alice.txt");

        if (!File.Exists(path))
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                string text = client.GetStringAsync(AliceUrl).GetAwaiter().GetResult();
                File.WriteAllText(path, text);
            }
            catch (Exception ex)
            {
                return OperationResult<AliceDataset>.Fail(
                    $"Cannot download alice.txt: {ex.Message}. " +
                    $"Please place the file manually at: {path}");
            }
        }

        string raw   = File.ReadAllText(path);
        int    limit = WordLimits[scale];
        var    words = Tokenize(raw, limit);

        if (words.Count < 50)
            return OperationResult<AliceDataset>.Fail("alice.txt has fewer than 50 usable words");

        var vocab  = new Dictionary<string, int>();
        var tokens = new int[words.Count];
        for (int i = 0; i < words.Count; i++)
        {
            if (!vocab.TryGetValue(words[i], out int id))
                vocab[words[i]] = id = vocab.Count;
            tokens[i] = id;
        }

        return OperationResult<AliceDataset>.Ok(new AliceDataset(
            Words:  words.AsReadOnly(),
            Tokens: tokens,
            Vocab:  vocab));
    }

    private static List<string> Tokenize(string text, int maxWords)
    {
        return Whitespace.Split(text)
            .Select(w => NonAlpha.Replace(w.ToLowerInvariant(), ""))
            .Where(w => w.Length > 0)
            .Take(maxWords)
            .ToList();
    }

    public static readonly IReadOnlyDictionary<int, string> TierNames =
        new Dictionary<int, string>
        {
            [0] = "Alice-micro  (first 2 000 words)",
            [1] = "Alice-small  (first 8 000 words)",
            [2] = "Alice-medium (first 15 000 words)",
            [3] = "Alice-full   (all ~28 000 words)",
        };
}

public sealed record AliceDataset(
    IReadOnlyList<string>            Words,
    IReadOnlyList<int>               Tokens,
    IReadOnlyDictionary<string, int> Vocab);
