// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// TextDatasetLoader loads text files for E08.
// Splits into sentences by periods/exclamation marks/question marks.
public static class TextDatasetLoader
{
    // Load the first maxSentences sentences from a text file.
    public static OperationResult<TextDatasetContents> Load(
        string textFilePath, int maxSentences)
    {
        if (!File.Exists(textFilePath))
            return OperationResult<TextDatasetContents>.Fail(
                $"File not found: {textFilePath}");

        string raw = File.ReadAllText(textFilePath, System.Text.Encoding.UTF8);

        var sentences = raw
            .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s =>
            {
                int words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                return words >= 5 && words <= 50;
            })
            .Take(maxSentences)
            .ToList()
            .AsReadOnly();

        if (sentences.Count == 0)
            return OperationResult<TextDatasetContents>.Fail(
                "No suitable sentences in the file");

        return OperationResult<TextDatasetContents>.Ok(new TextDatasetContents
        {
            FilePath     = textFilePath,
            Sentences    = sentences,
            CombinedText = string.Join(" ", sentences)
        });
    }
}

public sealed record TextDatasetContents
{
    public required string FilePath      { get; init; }
    public required IReadOnlyList<string> Sentences { get; init; }
    public required string CombinedText  { get; init; }
    public int SentenceCount => Sentences.Count;
}
