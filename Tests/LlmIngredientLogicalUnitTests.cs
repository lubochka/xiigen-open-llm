// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using LoRA;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed class LlmIngredientLogicalUnitTests
{
    [Fact]
    public async Task TransformerEmbeddingKnnLogicalUnit_UsesTransformerEmbeddingAndHnswInsideExecuteAsync()
    {
        // Build a tiny local tokenizer and transformer so the test stays CPU-light.
        var tokenizer = BuildTokenizer();
        var model = new LlmModel(BuildTinyConfig());

        // Train a transformer-embedding KNN logical unit.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "unit-transformer-knn-domain-v1",
            "domain-splitter",
            "software-development",
            model,
            tokenizer,
            [
                new TransformerKnnTrainingExample<string>("pair-security", "jwt oauth token authorization tenant", "SECURITY", ["SECURITY"], ["security"]),
                new TransformerKnnTrainingExample<string>("pair-ui", "layout screen typography button design", "UI", ["UI"], ["ui"])
            ]);

        // Training must succeed through the shared result envelope.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // The manifest must prove projects 01, 03, 05, and 06 are on the unit path.
        Assert.Contains("01.Tokenizer:BpeTokenizer", trained.Value!.Manifest.IngredientIds);
        Assert.Contains("03.TransformerModel:LlmModel", trained.Value.Manifest.IngredientIds);
        Assert.Contains("05.Embeddings:EmbeddingModel", trained.Value.Manifest.IngredientIds);
        Assert.Contains("06.VectorSearch:HnswIndex", trained.Value.Manifest.IngredientIds);
        Assert.Contains("E34V2:BM25_KNN_FUSION", trained.Value.Manifest.IngredientIds);

        // Execute through ILogicalUnit.ExecuteAsync.
        var result = await trained.Value.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-transformer-knn",
            "unit-transformer-knn-domain-v1",
            "domain-splitter",
            "software-development",
            "jwt oauth token authorization tenant",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The output must come from the nearest learned example.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("SECURITY", result.Value!.Output);
        Assert.Contains("pair-security", result.Value.EvidenceIds);
        Assert.Contains("E34V2:BM25_KNN_FUSION", result.Value.EvidenceIds);
    }

    [Fact]
    public async Task TransformerEmbeddingKnnLogicalUnit_FusesBm25WithKnnForExactDeveloperTerms()
    {
        // Build a tiny local tokenizer and transformer.
        var tokenizer = BuildTokenizer();
        var model = new LlmModel(BuildTinyConfig());

        // Train with strongly different exact developer vocabulary.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "unit-transformer-fusion-v1",
            "domain-splitter",
            "software-development",
            model,
            tokenizer,
            [
                new TransformerKnnTrainingExample<string>("pair-figma", "figma figma export failure frame returned no data", "FIGMA", ["UI", "IMPLEMENT"], ["frontend"]),
                new TransformerKnnTrainingExample<string>("pair-auth", "jwt oauth token tenant authorization validation", "AUTH", ["SECURITY"], ["security"])
            ]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // Query with the exact lexical signal that E34V2 is meant to preserve.
        var result = await trained.Value!.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-transformer-fusion",
            "unit-transformer-fusion-v1",
            "domain-splitter",
            "software-development",
            "figma export frame returned no data",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // BM25+KNN fusion should select the exact developer-term example.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("FIGMA", result.Value!.Output);
        Assert.Contains("pair-figma", result.Value.EvidenceIds);
        Assert.Contains("E34V2:BM25_KNN_FUSION", result.Value.EvidenceIds);
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_ExecutesLoraAndDpoThroughLogicalUnit()
    {
        // Build a tiny local tokenizer and model pair.
        var tokenizer = BuildTokenizer();
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"logical-dpo-{Guid.NewGuid():N}");

        try
        {
            // Build a teaching request with one preference pair.
            var request = new LogicalDpoTrainingRequest(
                policy,
                reference,
                tokenizer,
                checkpointDir,
                [new LogicalDpoPreferenceExample("Classify the prompt.", "SECURITY", "UI")],
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] });

            // Execute the teaching unit.
            var result = await new LogicalDpoTrainingUnit().ExecuteAsync(new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                "run-logical-dpo",
                LogicalDpoTrainingUnit.Id,
                LogicalDpoTrainingUnit.Role,
                LogicalDpoTrainingUnit.DomainTag,
                request,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            // The teaching pass must succeed through ILogicalUnit.ExecuteAsync.
            Assert.True(result.IsSuccess, result.ErrorMessage);

            // LoRA and DPO must both be visibly connected to the unit result.
            Assert.True(result.Value!.Output.LoraAdapterCount > 0);
            Assert.Equal(1, result.Value.Output.DpoStepCount);
            Assert.Contains("10.LoRA:LoraApplicator", result.Value.Output.IngredientIds);
            Assert.Contains("11.DPO:DpoTrainer", result.Value.Output.IngredientIds);
            Assert.Single(result.Value.Output.MetricFilePaths);
            Assert.True(File.Exists(result.Value.Output.MetricFilePaths[0]));
        }
        finally
        {
            // Clean test artifacts without touching repository files.
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_BatchesMultiplePreferencesIntoOneOptimizerStep()
    {
        // Build a tiny local tokenizer and model pair.
        var tokenizer = BuildTokenizer();
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"logical-dpo-batch-{Guid.NewGuid():N}");

        try
        {
            // Build a teaching request with two preferences that should share one optimizer update.
            var request = new LogicalDpoTrainingRequest(
                policy,
                reference,
                tokenizer,
                checkpointDir,
                [
                    new LogicalDpoPreferenceExample("Classify jwt auth.", "SECURITY", "UI"),
                    new LogicalDpoPreferenceExample("Classify figma export failure.", "FIGMA", "AUTH")
                ],
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] });

            // Execute the teaching unit.
            var result = await new LogicalDpoTrainingUnit().ExecuteAsync(new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                "run-logical-dpo-batch",
                LogicalDpoTrainingUnit.Id,
                LogicalDpoTrainingUnit.Role,
                LogicalDpoTrainingUnit.DomainTag,
                request,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            // Both preference examples are consumed, but only one DPO optimizer step and metric file are emitted.
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(2, result.Value!.Output.PreferenceCount);
            Assert.Equal(1, result.Value.Output.DpoStepCount);
            Assert.Single(result.Value.Output.MetricFilePaths);
            Assert.True(File.Exists(result.Value.Output.MetricFilePaths[0]));
        }
        finally
        {
            // Clean test artifacts without touching repository files.
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    private static BpeTokenizer BuildTokenizer()
    {
        // Train a tiny byte-pair tokenizer with enough vocabulary headroom for merged tokens.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("jwt oauth token authorization tenant layout screen typography button design classify prompt security ui figma export failure frame returned no data auth", 300);
        return tokenizer;
    }

    private static ModelConfig BuildTinyConfig()
    {
        // Keep the model tiny so the test proves wiring without stressing the PC.
        return new ModelConfig
        {
            VocabSize = 512,
            ContextLen = 64,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };
    }
}
