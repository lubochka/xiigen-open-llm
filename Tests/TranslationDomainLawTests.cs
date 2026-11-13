// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using TranslationDomain;

public sealed class TranslationDomainLawTests
{
    [Theory]
    [InlineData("full")]
    [InlineData("prefix")]
    [InlineData("structure")]
    public void Cascade_FiltersKnownOppositesBeforeCapAtEachLevel(string level)
    {
        // Query keys come from the real tokenizer; inserting entries here forces a cascade branch property.
        // Natural training-key reachability is measured separately by the public saved-memory runner.
        const string query = "the file is not null";
        var tokens = new TranslationSemanticTokenizer().Tokenize(query, "en");
        var signature = level switch
        {
            "full" => tokens.Signature,
            "prefix" => tokens.PrefixSignature,
            _ => tokens.StructureSignature
        };
        var store = new TranslationKnnStore();
        for (var i = 0; i < 5; i++)
            store.AddPair(signature, "the file is null", "файл имеет нулевое значение");
        store.AddPair(signature, "a file is not null", "файл не имеет нулевого значения");

        // Force each cascade level by omitting earlier keys, while preserving actual tokenizer addresses.
        var actual = store.FindNearestCascade(
            level == "full" ? tokens.Signature : string.Empty,
            level == "structure" ? string.Empty : tokens.PrefixSignature,
            tokens.StructureSignature, 1, query);

        var selected = Assert.Single(actual);
        Assert.Equal("a file is not null", selected.Source);
    }

    [Theory]
    [InlineData("the box is not only empty but also closed")]
    [InlineData("\"the box is not empty\"")]
    [InlineData("the box is not")]
    [InlineData("it is not empty")]
    [InlineData("the box was not empty")]
    public void Cascade_UnknownScopeReferentOrTenseDoesNotBecomeHardVeto(string query)
    {
        var tokens = new TranslationSemanticTokenizer().Tokenize(query, "en");
        var store = new TranslationKnnStore();
        store.AddPair(tokens.Signature, "the box is empty", "коробка пустая");

        var actual = store.FindNearestCascade(tokens.Signature, tokens.PrefixSignature,
            tokens.StructureSignature, 1, query);

        Assert.NotEmpty(actual);
    }

    [Fact]
    public async Task OrdinaryConsumer_EmptyLawfulPoolIsExplicitLowConfidenceAfterRestore()
    {
        // Import an actual exported checkpoint shape without adding synthetic tokenizer corrections.
        var machine = new TranslationMachine();
        var checkpoint = machine.ExportCheckpoint().Value!;
        var source = "the box is empty";
        var tokens = new TranslationSemanticTokenizer().Tokenize(source, "en");
        checkpoint = checkpoint with
        {
            KnnStore = new TranslationKnnStoreCheckpoint(
                [new TranslationKnnStoreEntryCheckpoint(tokens.Signature, source, "коробка пустая")])
        };
        Assert.True(machine.ImportCheckpoint(checkpoint).IsSuccess);
        var orchestrator = (await machine.GetOrchestratorAsync()).Value!;

        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "polarity-empty-pool", TranslationRoutingFlow.ScenarioId, "the box is not empty",
            new LogicalExecutionFlags(), new Dictionary<string, object>
            {
                ["source_language"] = "en", ["target_language"] = "ru", ["topic_domain"] = "general"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("TRANSLATION_LOW_CONFIDENCE", result.Value!.OverallVerdict);
        Assert.Contains("BOOTSTRAP", result.Value.FinalHumanResult);
        Assert.False(result.Value.FlowResult!.Summary.Passed);
        Assert.NotEmpty(result.Value.FlowResult.Warnings);
    }
}
