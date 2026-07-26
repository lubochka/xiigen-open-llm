// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

public sealed class ExperimentInfrastructureConsolidationTests
{
    [Fact]
    public void ExperimentsV2_InfrastructureProductionCopies_AreRemoved()
    {
        var root = FindRepositoryRoot();
        var infrastructure = Path.Combine(root, "35.ExperimentsV2", "Infrastructure");
        var removedFiles = new[]
        {
            "DomainClassifier.cs",
            "DomainClassifierCompatibility.cs",
            "FamilyCollapser.cs",
            "LogicExtractor.cs",
            "SemanticNormalizer.cs",
            "SentencePatternClassifier.cs",
            "StopSignalDetector.cs",
            "PromptSplitter.cs",
            "AttributedRouter.cs",
            "AttributedRoutingResult.cs",
            "SkillTriggerLibrary.cs"
        };

        foreach (var fileName in removedFiles)
            Assert.False(File.Exists(Path.Combine(infrastructure, fileName)), $"{fileName} must not live in 35.ExperimentsV2/Infrastructure.");
    }

    [Fact]
    public void ExperimentsV2_CanonicalSemanticTypes_AreAvailableFromProductionAssemblies()
    {
        Assert.Equal("SemanticCompiler", typeof(SemanticCompiler.FamilyCollapser).Assembly.GetName().Name);
        Assert.Equal("SemanticCompiler", typeof(SemanticCompiler.LogicExtractor).Assembly.GetName().Name);
        Assert.Equal("SemanticCompiler", typeof(SemanticCompiler.SemanticNormalizer).Assembly.GetName().Name);
        Assert.Equal("SemanticCompiler", typeof(SemanticCompiler.StopSignalDetector).Assembly.GetName().Name);
        Assert.Equal("SemanticCompiler", typeof(SemanticCompiler.PatternClassifier).Assembly.GetName().Name);
        Assert.Equal("SemanticCompilerAdapter", typeof(SemanticCompilerAdapter.DomainClassifierUnit).Assembly.GetName().Name);
        Assert.Equal("AgentAlgorithms", typeof(AgentAlgorithms.Skills.SkillTriggerLibrary).Assembly.GetName().Name);
        Assert.True(AgentAlgorithms.Skills.SkillTriggerLibrary.All.Count >= 40);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
