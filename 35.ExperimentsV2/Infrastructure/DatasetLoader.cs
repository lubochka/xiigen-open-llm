// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;

namespace ExperimentsV2;

public static class DatasetLoader
{
    private static readonly string[] Tier1 =
        ["01.Tokenizer"];

    private static readonly string[] Tier2 =
        ["01.Tokenizer", "02.DatasetPipeline",
         "05.Embeddings", "07.GraphEngine", "09.Inference", "10.LoRA"];

    public static OperationResult<DatasetContents> Load(int tier, string projectRoot)
    {
        if (!Directory.Exists(projectRoot))
            return OperationResult<DatasetContents>.Fail(
                $"Folder not found: {projectRoot}");

        var folders = ResolveFolders(tier, projectRoot);
        if (folders.Count == 0)
            return OperationResult<DatasetContents>.Fail(
                $"No folders for tier {tier} in {projectRoot}");

        var files = new List<(string Path, string Text)>();
        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder)) continue;
            foreach (var f in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                if (f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)) continue;
                if (f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)) continue;
                files.Add((f, File.ReadAllText(f, Encoding.UTF8)));
            }
        }

        if (files.Count == 0)
            return OperationResult<DatasetContents>.Fail(
                $"No .cs files for tier {tier} in {projectRoot}");

        string combined = string.Join("\n\n", files.Select(f => f.Text));

        return OperationResult<DatasetContents>.Ok(new DatasetContents
        {
            Tier         = tier,
            ProjectRoot  = projectRoot,
            Files        = files.AsReadOnly(),
            CombinedText = combined
        });
    }

    private static List<string> ResolveFolders(int tier, string root) => tier switch
    {
        1 => Tier1.Select(f => Path.Combine(root, f)).ToList(),
        2 => Tier2.Select(f => Path.Combine(root, f)).ToList(),
        _ => Directory.GetDirectories(root)
                 .Where(d =>
                 {
                     string name = Path.GetFileName(d);
                     return name.Length >= 2 && char.IsDigit(name[0]) && char.IsDigit(name[1]);
                 })
                 .OrderBy(d => d)
                 .ToList()
    };
}

public sealed record DatasetContents
{
    public required int    Tier         { get; init; }
    public required string ProjectRoot  { get; init; }
    public required IReadOnlyList<(string Path, string Text)> Files { get; init; }
    public required string CombinedText { get; init; }
    public int FileCount => Files.Count;
}
