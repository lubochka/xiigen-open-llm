// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using LoRA;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

internal static class ProvenanceTestFactory
{
    private static readonly ModelConfig ModelConfig = new()
    {
        VocabSize = 256,
        ContextLen = 32,
        HiddenDim = 8,
        NumLayers = 1,
        NumHeads = 2,
        NumKvHeads = 1,
        FfnHiddenDim = 16,
    };

    public static string SourceId(string origin, string content)
        => TrainingSourceProvenance.CreateSourceId(origin, content);

    public static TrainingExample WithSource(
        TrainingExample example,
        string? discriminator = null)
        => example with
        {
            SourceId = SourceId(
                discriminator ?? example.Prompt,
                string.Concat(example.Prompt, "\n", example.Response)),
        };

    public static TrainedAdapterArtifact Artifact(
        string adapterPath,
        IReadOnlyList<TrainingExample> examples)
    {
        var sourceIds = examples.Select((example, index) =>
            TrainingSourceProvenance.IsCanonicalSourceId(example.SourceId)
                ? example.SourceId!
                : SourceId($"fixture/{index}/{example.Prompt}", example.Response));
        var manifest = TrainingSourceProvenance.BuildManifest(sourceIds).Value!;
        return new TrainedAdapterArtifact(
            Path.GetFullPath(adapterPath),
            new ModelAdapterProvenance(
                manifest,
                HashFileOrZeros(adapterPath),
                new string('1', 64),
                new string('2', 64),
                TrainingSourceProvenance.Identify(ModelConfig),
                TrainingSourceProvenance.Identify(IncrementalTrainer.DefaultConfig)));
    }

    public static ModelAdapter Adapter(
        string domain,
        string adapterPath,
        IReadOnlyList<TrainingExample> examples)
    {
        var artifact = Artifact(adapterPath, examples);
        return new ModelAdapter(
            domain,
            artifact.AdapterPath,
            examples.Count,
            DateTimeOffset.UtcNow,
            artifact.Provenance);
    }

    private static string HashFileOrZeros(string path)
    {
        if (!File.Exists(path))
            return new string('0', 64);
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        return TrainingSourceProvenance.Sha256(stream);
    }
}
