// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

public sealed class DevelopmentPromptCompilerTeachingIntegrationTests
{
    [Fact]
    public void SemanticTeachingPromptCompiler_ProducesProject23CanonicalPromptSide()
    {
        var compiled = new SemanticTeachingPromptCompiler().Compile("fix the null ref in the Angular service");

        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);
        Assert.Contains("IMPLEMENT", compiled.Value!.CanonicalText);
        Assert.Contains("TEST", compiled.Value.CanonicalText);
        Assert.Contains("semantic_domain:", compiled.Value.CanonicalText);
        Assert.Contains("semantic_stack:", compiled.Value.CanonicalText);
        Assert.DoesNotContain("fix the null ref", compiled.Value.CanonicalText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TrainingVectorBuilder_BuildFromCompiledPromptUsesDevelopmentPredicates()
    {
        var compiled = new SemanticTeachingPromptCompiler().Compile("fix the null ref in the Angular service");

        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);
        Assert.Equal(TrainingVectorBuilder.VectorWidth, compiled.Value!.TeachingVector.Length);
        Assert.True(compiled.Value.TeachingVector[5] > 0f); // IMPLEMENT
        Assert.True(compiled.Value.TeachingVector[6] > 0f); // TEST
    }

    [Fact]
    public void DefaultLogicalCapabilityTrainer_BuildTrainingTextUsesCompiledPrompt()
    {
        var compiled = new SemanticTeachingPromptCompiler().Compile("fix the null ref in the Angular service");
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);

        var trainingText = InvokeBuildTrainingText(
            compiled.Value!,
            BuildKnowledgeEdgeBranch("4.4"),
            BuildSubPhase("4.4", "Refactoring and Code Quality", ["IMPLEMENT"]));

        Assert.Contains("semantic_prompt:", trainingText.Text);
        Assert.Contains("IMPLEMENT", trainingText.Text);
        Assert.Contains("semantic_domain:", trainingText.Text);
        Assert.Contains("teaching_vector_60:", trainingText.Text);
        Assert.DoesNotContain("fix the null ref in the Angular service", trainingText.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(TrainingVectorBuilder.VectorWidth, trainingText.Vector.Length);
    }

    [Fact]
    public void DefaultLogicalCapabilityTrainer_BuildTokenizerUsesCanonicalPredicateSeed()
    {
        var compiled = new SemanticTeachingPromptCompiler().Compile("add authentication to the controller");
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);

        var tokenizer = InvokeBuildTokenizer(compiled.Value!, "semantic_prompt: SECURITY AUTHORIZATION IMPLEMENT");
        var decoded = tokenizer.Decode(tokenizer.Encode("SECURITY AUTHORIZATION IMPLEMENT"));

        Assert.Contains("SECURITY", decoded);
        Assert.Contains("AUTHORIZATION", decoded);
        Assert.Contains("IMPLEMENT", decoded);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_CompilesRuntimePromptBeforeLearnedUnitLookup()
    {
        var unit = new CapturingLogicalUnit();
        var flow = new GeneratedCapabilityFlow(
            "4",
            "4.1",
            "Backend Services",
            ["software-development"],
            ["IMPLEMENT"],
            Domain.BUG.ToString(),
            unit);
        var rawPrompt = "fix the null ref in the Angular service";

        var result = await flow.ExecuteAsync(new LogicalFlowRequest<string>(
            "generated-flow-compile-test",
            "generated-capability-flow-4-1",
            "generated-capability-4-1",
            rawPrompt,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEqual(rawPrompt, unit.CapturedInput);
        Assert.Contains("IMPLEMENT", unit.CapturedInput);
        Assert.Contains("semantic_domain:", unit.CapturedInput);
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_NullCompiledPromptCompilesOnDemandBeforeManifest()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhaseDefinition = BuildSubPhase("3.4", "Security and Authorization Model", ["SECURITY", "AUTHORIZATION", "IMPLEMENT"]);
        var subPhase = topology.AddOrUpdateSubPhase(subPhaseDefinition);
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);

        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"dpc-teaching-integration-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(
            checkpointRoot,
            admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority());

        try
        {
            var request = new LogicalCapabilityTrainingRequest(
                "dpc-on-demand-run",
                new LogicalOrchestratorRequest(
                    "dpc-on-demand-run",
                    AdaptiveLogicalOrchestrator.ScenarioId,
                    "add authentication to the controller",
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()),
                [Attest(BuildKnowledgeEdgeBranch("3.4"),
                    "add authentication to the controller", Domain.ARCH,
                    subPhaseDefinition)],
                topology,
                flows,
                units)
            {
                Domain = Domain.ARCH
            };

            var trained = await trainer.TrainAndRegisterAsync(request);

            Assert.True(trained.IsSuccess, trained.ErrorMessage);
            Assert.True(trained.Value!.CapabilityRegistered);
            var manifest = Directory
                .GetFiles(checkpointRoot, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName, SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .Select(json => System.Text.Json.JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(json))
                .Single(item => item is not null)!;
            Assert.Contains("semantic_prompt:", manifest.TrainingText);
            Assert.Contains("SECURITY", manifest.TrainingText);
            Assert.Contains("AUTHORIZATION", manifest.TrainingText);
            Assert.DoesNotContain("XIIGen documentation teaching", manifest.TrainingText);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    private static (string Text, float[] Vector) InvokeBuildTrainingText(
        SemanticTeachingPromptRepresentation compiledPrompt,
        LogicalFlowResult branch,
        LogicalSubPhase subPhase)
    {
        var value = LogicalCapabilityTrainingPreferenceProjector
            .ProjectCurrent(compiledPrompt.RawText, branch, subPhase);
        Assert.True(value.IsSuccess, value.ErrorMessage);
        return (value.Value!.Prompt, value.Value.TeachingVector);
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
            ["dpc-integration-test"]);
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

    private static LogicalFlowResult Attest(LogicalFlowResult branch,
        string userRequest, Domain domain, LogicalSubPhase subPhase)
        => branch with
        {
            TrainingContentProvenance = LogicalCapabilityTrainingAuthorityTestFactory
                .IssueSingle(branch,
                    new LogicalTrainingContentDeterministicInputs(userRequest,
                        domain.ToString(), subPhase.PhaseId, subPhase.SubPhaseId,
                        subPhase.Name, subPhase.Purpose, subPhase.PredicateIds,
                        subPhase.DomainIds))
        };

    private sealed class CapturingLogicalUnit : ILogicalUnit<string, string>
    {
        public string CapturedInput { get; private set; } = string.Empty;

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            CapturedInput = request.Input;
            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(new LogicalUnitResult<string>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                "captured",
                0.99,
                ["IMPLEMENT"],
                [request.DomainTag],
                ["capturing-unit"],
                [])));
        }
    }
}
