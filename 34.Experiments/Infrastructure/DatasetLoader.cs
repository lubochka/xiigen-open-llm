// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Experiments;

// ─── DatasetLoader: read .cs files from the project by scale tier ─────────────
//
// Six scale tiers define how much source code is loaded:
//   Scale 0  Markov-micro    BpeTokenizer.cs (one file)              ~1 000 tokens
//   Scale 1  Markov-small    01.Tokenizer subproject                 ~9 100 tokens
//   Scale 2  Markov-medium   00–08 subprojects (9 folders)          ~30 000 tokens
//   Scale 3  Full            00–14 subprojects (all)                ~55 000 tokens
//   Scale 4  Probe-medium    same as Scale 2 (probe variant)        ~30 000 tokens
//   Scale 5  Full-alt        all subprojects (mirror of Scale 3)    ~55 000 tokens
//
// DatasetLoader has no side-effects — it only reads files.
// Each call to Load(scale, projectRoot) is idempotent.

public sealed class DatasetLoader
{
    // Folder/file patterns per scale tier (relative to projectRoot).
    private static readonly IReadOnlyDictionary<int, string[]> TierFolders =
        new Dictionary<int, string[]>
        {
            [0] = new[] { "01.Tokenizer/BpeTokenizer.cs" },
            [1] = new[] { "01.Tokenizer" },
            [2] = new[] { "00.TensorCore", "01.Tokenizer", "02.DatasetPipeline",
                          "03.TransformerModel", "04.TrainingLoop", "05.Embeddings",
                          "06.VectorSearch", "07.GraphEngine", "08.GraphRag" },
            [3] = new[] { "00.TensorCore", "01.Tokenizer", "02.DatasetPipeline",
                          "03.TransformerModel", "04.TrainingLoop", "05.Embeddings",
                          "06.VectorSearch", "07.GraphEngine", "08.GraphRag",
                          "09.Inference", "10.LoRA", "11.DPO", "12.SearchEngine",
                          "13.CodeOptimizer", "14.MachineLearning" },
            [4] = new[] { "00.TensorCore", "01.Tokenizer", "02.DatasetPipeline",
                          "03.TransformerModel", "04.TrainingLoop", "05.Embeddings",
                          "06.VectorSearch", "07.GraphEngine", "08.GraphRag" },
            [5] = new[] { "00.TensorCore", "01.Tokenizer", "02.DatasetPipeline",
                          "03.TransformerModel", "04.TrainingLoop", "05.Embeddings",
                          "06.VectorSearch", "07.GraphEngine", "08.GraphRag",
                          "09.Inference", "10.LoRA", "11.DPO", "12.SearchEngine",
                          "13.CodeOptimizer", "14.MachineLearning" },
        };

    // Human-readable tier labels (used in result files).
    public static readonly IReadOnlyDictionary<int, string> TierNames =
        new Dictionary<int, string>
        {
            [0] = "Markov-micro  (~1 000 tokens, 1 file)",
            [1] = "Markov-small  (~9 100 tokens, 1 subproject)",
            [2] = "Markov-medium (~30 000 tokens, 9 subprojects 00–08)",
            [3] = "Full          (~55 000 tokens, all subprojects 00–14)",
            [4] = "Probe-medium  (~30 000 tokens, probe mode)",
            [5] = "Full-alt      (~55 000 tokens, all subprojects)",
        };

    // Read all .cs files for the given scale tier.
    // projectRoot — root directory containing subfolders like 01.Tokenizer, 02.DatasetPipeline …
    // Returns DatasetContents with combined text and per-file entries.
    public OperationResult<DatasetContents> Load(int scale, string projectRoot)
    {
        if (scale < 0 || scale > 5)
            return OperationResult<DatasetContents>.Fail(
                $"Scale must be 0–5, got: {scale}");

        if (!Directory.Exists(projectRoot))
            return OperationResult<DatasetContents>.Fail(
                $"Project root not found: {projectRoot}");

        var entries  = new List<(string Path, string Text)>();
        var patterns = TierFolders[scale];

        foreach (string pattern in patterns)
        {
            string fullPath = Path.Combine(projectRoot, pattern);

            if (File.Exists(fullPath))
            {
                string text = File.ReadAllText(fullPath);
                if (!string.IsNullOrWhiteSpace(text))
                    entries.Add((fullPath, text));
            }
            else if (Directory.Exists(fullPath))
            {
                foreach (string csFile in Directory.EnumerateFiles(
                    fullPath, "*.cs", SearchOption.AllDirectories))
                {
                    if (IsGeneratedFile(csFile)) continue;
                    string text = File.ReadAllText(csFile);
                    if (!string.IsNullOrWhiteSpace(text))
                        entries.Add((csFile, text));
                }
            }
        }

        if (entries.Count == 0)
            return OperationResult<DatasetContents>.Fail(
                $"Scale {scale}: no .cs files found under {projectRoot}. " +
                $"Patterns checked: {string.Join(", ", patterns)}");

        string allText      = string.Join("\n\n", entries.Select(e => e.Text));
        int    totalChars   = allText.Length;
        int    approxTokens = totalChars / 4;   // ~4 chars per BPE token

        return OperationResult<DatasetContents>.Ok(new DatasetContents
        {
            Scale        = scale,
            TierName     = TierNames[scale],
            Files        = entries.Select(e => e.Path).ToList(),
            FileTexts    = entries.ToDictionary(e => e.Path, e => e.Text),
            CombinedText = allText,
            TotalChars   = totalChars,
            ApproxTokens = approxTokens,
        });
    }

    private static bool IsGeneratedFile(string path)
    {
        string normalised = path.Replace('\\', '/');
        return normalised.Contains("/bin/")   ||
               normalised.Contains("/obj/")   ||
               path.EndsWith(".g.cs")         ||
               path.EndsWith(".generated.cs");
    }
}

// ─── DatasetContents record ────────────────────────────────────────────────────

public sealed record DatasetContents
{
    public required int    Scale        { get; init; }
    public required string TierName     { get; init; }
    public required IReadOnlyList<string>              Files     { get; init; }
    public required IReadOnlyDictionary<string,string> FileTexts { get; init; }
    // All file texts joined for E01–E05 tokenisation.
    public required string CombinedText { get; init; }
    public required int    TotalChars   { get; init; }
    // Approximate BPE token count (TotalChars / 4).
    public required int    ApproxTokens { get; init; }
}
