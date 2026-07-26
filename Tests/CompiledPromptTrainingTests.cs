// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

public sealed class CompiledPromptTrainingTests
{
    [Fact]
    public void BuildTrainingText_UsesCompiledFormNotRawPrompt()
    {
        var branch = BuildKnowledgeEdgeBranch("4.1");
        var subPhase = BuildSubPhase("4.1", "Backend Services", ["IMPLEMENT"]);
        var value = InvokeBuildTrainingTextCore(
            null,
            "CALL_OP AWAIT_OP semantic_domain:BUG semantic_stack:typescript",
            "fix the rxjs pipe in the Angular interceptor service",
            branch,
            subPhase);

        Assert.Contains("CALL_OP", value.Text);
        Assert.Contains("semantic_stack:typescript", value.Text);
        Assert.False(value.Text.StartsWith("fix the rxjs pipe in the Angular interceptor service", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("LogicalTeachingTextCompressor", value.Text);
    }

    [Fact]
    public void BuildTrainingText_CompilesOnDemandWhenCompiledTextIsMissing()
    {
        var branch = BuildKnowledgeEdgeBranch("3.4");
        var subPhase = BuildSubPhase("3.4", "Security and Authorization Model", ["SECURITY", "AUTHORIZATION"]);
        var value = InvokeBuildTrainingTextCore(null, null, "fix the authorization check in the tenant service", branch, subPhase);

        Assert.Contains("semantic_domain:", value.Text);
        Assert.Contains("semantic_stack:", value.Text);
        Assert.Equal(TrainingVectorBuilder.VectorWidth, value.Vector.Length);
    }

    [Fact]
    public void BuildTokenizer_UsesFullPredicateVocabularyBudget()
    {
        var compiled = new SemanticTeachingPromptCompiler().Compile("fix rxjs pipe in angular service");
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);

        var tokenizer = InvokeBuildTokenizer(compiled.Value!, "CALL_OP AUTH_OP semantic_stack:typescript");

        Assert.Equal(512, tokenizer.VocabSize);
        Assert.NotEmpty(tokenizer.Encode("CALL_OP"));
        Assert.NotEmpty(tokenizer.Encode("AUTH_OP"));
        Assert.NotEmpty(tokenizer.Encode("semantic_stack:typescript"));
    }

    [Fact]
    public void CompiledText_DifferentStacksProduceDifferentTrainingText()
    {
        var branch = BuildKnowledgeEdgeBranch("4.1");
        var subPhase = BuildSubPhase("4.1", "Backend Services", ["IMPLEMENT"]);
        var typeScript = InvokeBuildTrainingTextCore(null, "IMPLEMENT semantic_domain:BUG semantic_stack:typescript", "fix angular service", branch, subPhase);
        var csharp = InvokeBuildTrainingTextCore(null, "IMPLEMENT semantic_domain:BUG semantic_stack:csharp", "fix csharp service", branch, subPhase);

        Assert.NotEqual(typeScript.Text, csharp.Text);
        Assert.Contains("semantic_stack:typescript", typeScript.Text);
        Assert.Contains("semantic_stack:csharp", csharp.Text);
    }

    [Fact]
    public void LogicalCapabilityTrainingRequest_CompiledTextFieldRoundTrips()
    {
        var request = new LogicalCapabilityTrainingRequest(
            "compiled-text-contract-run",
            new LogicalOrchestratorRequest("compiled-text-contract-run", AdaptiveLogicalOrchestrator.ScenarioId, "raw", new LogicalExecutionFlags(), new Dictionary<string, object>()),
            [],
            new InMemoryLogicalTopologyRegistry(),
            new InMemoryLogicalFlowRegistry(),
            new InMemoryLogicalUnitRegistry())
        {
            Domain = Domain.BUG,
            CompiledText = "IMPLEMENT TEST semantic_stack:csharp"
        };

        Assert.Equal("compiled-text-contract-run", request.RunId);
        Assert.Equal(Domain.BUG, request.Domain);
        Assert.Equal("IMPLEMENT TEST semantic_stack:csharp", request.CompiledText);
    }

    [Fact]
    public void SemanticTeachingPromptCompiler_CanonicalTextIsNotRawSurfaceText()
    {
        var compiled = new SemanticTeachingPromptCompiler().Compile("fix the null ref in the service");

        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);
        Assert.Contains("semantic_domain:", compiled.Value!.CanonicalText);
        Assert.Contains("semantic_stack:", compiled.Value.CanonicalText);
        Assert.NotEqual("fix the null ref in the service", compiled.Value.CanonicalText);
    }

    private static (string Text, float[] Vector) InvokeBuildTrainingTextCore(
        SemanticTeachingPromptRepresentation? compiledPrompt,
        string? compiledText,
        string userRequest,
        LogicalFlowResult branch,
        LogicalSubPhase subPhase)
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "BuildTrainingTextCore",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);
        var value = method.Invoke(null, [compiledPrompt, compiledText, userRequest, branch, subPhase]);
        Assert.NotNull(value);
        var text = (string)value.GetType().GetProperty("Text")!.GetValue(value)!;
        var vector = (float[])value.GetType().GetProperty("TeachingVector")!.GetValue(value)!;
        return (text, vector);
    }

    private static Tokenizer.BpeTokenizer InvokeBuildTokenizer(SemanticTeachingPromptRepresentation compiledPrompt, string trainingText)
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "BuildTokenizer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);
        return (Tokenizer.BpeTokenizer)method.Invoke(null, [compiledPrompt, trainingText])!;
    }

    private static LogicalSubPhase BuildSubPhase(string subPhaseId, string name, string[] predicates)
    {
        return new LogicalSubPhase(
            subPhaseId,
            subPhaseId.Split('.')[0],
            name,
            $"Owns {name}.",
            ["software-development"],
            predicates,
            ["compiled-training-test"]);
    }

    private static LogicalFlowResult BuildKnowledgeEdgeBranch(string subPhaseId)
    {
        var summary = new LogicalFlowSummary(
            $"summary-{subPhaseId}",
            1,
            0,
            0,
            "knowledge_edge",
            false,
            false,
            false,
            "GENERATE_CAPABILITY",
            0.70,
            $"missing_sub_phase={subPhaseId}",
            "Capability is missing for routed sub-phase.",
            0,
            0);

        return new LogicalFlowResult(
            $"missing-flow-{subPhaseId}",
            "missing-capability",
            "KNOWLEDGE_EDGE",
            "knowledge_edge: missing generated capability",
            summary,
            new Dictionary<string, string> { ["missing_sub_phase"] = subPhaseId },
            [],
            []);
    }
}
